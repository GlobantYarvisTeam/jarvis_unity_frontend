using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;
using UnityEngine.UI;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class Preloader : MonoBehaviour
{
	public static Preloader instance;

	public static string DOWNLOAD_PATH;

	public delegate void OnOperationComplete();
	public OnOperationComplete onOperationCompleteCallback;

	public GameObject loadingScreen;
	public Text statusText;
	public Text progressText;

	//
	private string yarvis_backend_url = "http://dmk-cnx-hive.herokuapp.com/api/display-wall/screensfull/";
	private IEnumerable<DreamforceScreen> _screenList;
	private JToken _tempScreenDisplaysList;
	private IEnumerable<JToken> _currentScreenDisplaysList;
	private int _runningDisplayIndex = 0;
	private Stack<String> _assetUrlStack = new Stack<string>();
	private int _currentScreenId = 0;
	private int _tempScreenId = 0;
	private bool _fetchingDisplayList = false;
	private bool _showLoadingScreen = false;

	public void Awake()
	{
		_fetchingDisplayList = false;
		DOWNLOAD_PATH = Application.persistentDataPath + "/downloaded_assets";
		instance = this;
		_screenList = null;
		ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
	}

	/**
     * Fetch Screen list asynchronously
     **/
	public void FetchScreenList ()
	{
		//Debug.Log ("FetchScreenList");
		loadingScreen.SetActive (true);
		statusText.text = "Fetching Screen List";
		progressText.text = "0%";
		StartCoroutine ("DownloadScreenList");
	}

	IEnumerator DownloadScreenList()
	{
		WWW www = new WWW(yarvis_backend_url + "?format=json");

		while (!www.isDone) {
			progressText.text = (Mathf.CeilToInt(www.progress * 100)).ToString() + "%";
			yield return null;
		}

		if (www.error != null) {
			statusText.text = "The service is not available. Retrying...";
			Debug.LogError (www.error);
			yield return new WaitForSeconds(2f);
			StartCoroutine ("DownloadScreenList");
		} else {
			FetchScreenListCompletedCallback(www.text);
		}
	}

	public void FetchScreenListCompletedCallback (string result)
	{
		//Debug.Log ("FetchScreenListCompletedCallback");

		try
		{
			var screenLists = JObject.Parse(result)["results"];

			_screenList = 
				from screen in screenLists
				select new DreamforceScreen(){id = (int)screen["id"], name = (string)screen["name"]};

			/*
			Debug.Log("ids");
			foreach (var item in _screenList) {
				Debug.Log("Name: " + item.name + "- ID: " + item.id);
			}
			*/

			if (onOperationCompleteCallback != null) {
				onOperationCompleteCallback ();
			}
		}
		catch (Exception e)
		{
			Debug.LogError(e.Message);
			//Debug.Log("Retrying DownloadScreenList");
			statusText.text = "Network error. Retrying...";
			StartCoroutine("DownloadScreenList");
		}
	}
	
	public IEnumerable<DreamforceScreen> GetScreenList  { get{ return _screenList; }}

	public void CancelUpdate ()
	{
		CancelInvoke ();
		_fetchingDisplayList = false;
	}

	public void UpdateRunningDisplayListData()
	{
		Debug.Log ("UPDATING RUNNING DISPLAY DATA");
		FetchDisplayList (_currentScreenId, false);
	}

	/**
     * Fetch displays in a screen asynchronously
     **/
	public void FetchDisplayList ( int id, bool showLoadingScreen )
	{
		if (_fetchingDisplayList)
			return;

		_showLoadingScreen = showLoadingScreen;

		_fetchingDisplayList = true;
		if (showLoadingScreen) {
			loadingScreen.SetActive (true);
			statusText.text = "Fetching Displays";
			progressText.text = "0%";
		}

		StartCoroutine ("DownloadDisplayList", id);
	}

	IEnumerator DownloadDisplayList(int id)
	{
		//Debug.Log ("FetchDisplayList: " + id.ToString());

		WWW www = new WWW(yarvis_backend_url + id.ToString() + "/?format=json");
		
		while (!www.isDone) {
			progressText.text = (Mathf.CeilToInt(www.progress * 100)).ToString() + "%";
			yield return null;
		}
		
		if (www.error != null) {
			//Debug.LogError (www.error);
			yield return new WaitForSeconds(2f);
			StartCoroutine("DownloadDisplayList", id);
		} else {
			FetchDisplayListCompletedCallback(www.text, id);
		}
	}
	
	public void FetchDisplayListCompletedCallback (string result, int id)
	{
		//Debug.Log ("FetchDisplayListCompletedCallback");
		try
		{
			_tempScreenDisplaysList = JObject.Parse(result)["displays"];
			Debug.Log(_tempScreenDisplaysList.ToString());

			StartCoroutine("SetAssetUrlStack");
			_tempScreenId = id;
		}
		catch (Exception e)
		{
			Debug.LogError(e.Message);
			//Debug.Log("Retrying DownloadScreenList");
			statusText.text = "Network error. Retrying...";
			StartCoroutine("DownloadDisplayList", id);
		}
	}

	public void SetAssetUrlStackCompleteCallback()
	{
		DownloadAssets ();
	}

	public void DownloadCompleteCallback()
	{
		loadingScreen.SetActive (false);
		_fetchingDisplayList = false;

		if (onOperationCompleteCallback != null) {
			onOperationCompleteCallback ();
		}
	}

	public void UpdateDisplayList()
	{
		if (!_fetchingDisplayList) {
			_currentScreenId = _tempScreenId;
			_currentScreenDisplaysList = _tempScreenDisplaysList;
		}
	}

	public int GetDisplayListLength()
	{
		return _currentScreenDisplaysList.Count ();
	}
	
	public string GetRunningDisplayType() 
	{
		return _currentScreenDisplaysList.ElementAt (_runningDisplayIndex).SelectToken ("$.displayType").ToString();
	}

	public int GetRunningDisplayId() 
	{
		return int.Parse(_currentScreenDisplaysList.ElementAt (_runningDisplayIndex).SelectToken ("$.object_id").ToString());
	}

	public void ResetDisplayIndex ()
	{
		_runningDisplayIndex = 0;
	}

	public JToken GetRunningDisplay ()
	{
		return _currentScreenDisplaysList.ElementAt (_runningDisplayIndex);
	}
	
	public void SetNextDisplayIndex ()
	{
		_runningDisplayIndex = ++_runningDisplayIndex < _currentScreenDisplaysList.Count () ? _runningDisplayIndex : 0;
	}

	public bool IsLastDisplay()
	{
		return _runningDisplayIndex == _currentScreenDisplaysList.Count() - 1;
	}
	
	/**
     * Download media assets
     **/
	IEnumerator SetAssetUrlStack ()
	{
		var data_token = _tempScreenDisplaysList
			.SelectMany (d => d.SelectToken ("data", false).Children ());

		_assetUrlStack = new Stack<string>(
			data_token
				.Select (di => (string)di.SelectToken ("image", false) )
				.Where(di => String.IsNullOrEmpty(di) == false)
			.Union (
				data_token	
					.Select (v => (string)v.SelectToken ("video", false) )
					.Where(v => String.IsNullOrEmpty(v) == false)
				)
			.Union (
				data_token
					.Select (u => (string)u.SelectToken ("user.image", false) )
					.Where(u => String.IsNullOrEmpty(u) == false)
				)
			.Union (
				_tempScreenDisplaysList
					.Select(i => (string)i.SelectToken("image", false) )
					.Where(i => String.IsNullOrEmpty(i) == false)
			)
			.Select (s => Regex.Replace(s, @"^//", "https://") )
			.Distinct()
			.ToList<string>()
		);

		yield return null;

		SetAssetUrlStackCompleteCallback ();
	}

	public void DownloadAssets ()
	{
		if (_assetUrlStack == null || _assetUrlStack.Count == 0) {
			DownloadCompleteCallback();
			return;
		}

		if (_showLoadingScreen) {
			loadingScreen.SetActive (true);
		}
			
		statusText.text = "Downloading Assets. Remaining: " + _assetUrlStack.Count.ToString ();

		string url = _assetUrlStack.Pop();

		if (url.Length < 5 && _assetUrlStack.Count > 0)
		{
			DownloadAssets ();
			return;
		}

		Uri uri = new Uri (url);

		if (!File.Exists(Path.Combine(DOWNLOAD_PATH, Path.GetFileName(uri.LocalPath))))
		{
			StartCoroutine("DownloadAsset", uri);
		}
		else
		{
			if (_assetUrlStack.Count > 0)
			{
				DownloadAssets ();
			}
			else
			{
				DownloadCompleteCallback();
			}
		}
	}

	IEnumerator DownloadAsset(Uri uri)
	{
		WWW www = new WWW (uri.AbsoluteUri);

		while (!www.isDone) {
			progressText.text = (Mathf.CeilToInt(www.progress * 100)).ToString() + "%";
			yield return null;
		}
		
		if (www.error != null) {
			Debug.LogError (www.error);
			//Debug.Log("Retrying download: " + uri.AbsoluteUri);
			StartCoroutine("DownloadAsset", uri);
			return false;
		} else {
			try
			{
				if(!Directory.Exists(DOWNLOAD_PATH))
				{
					Directory.CreateDirectory(DOWNLOAD_PATH);
				}

				File.WriteAllBytes(Path.Combine(DOWNLOAD_PATH, Path.GetFileName(uri.LocalPath)),  www.bytes);
			}
			catch(Exception e)
			{
				Debug.LogError(e.Message);
				//Debug.Log("Retrying download: " + uri.AbsoluteUri);
				StartCoroutine("DownloadAsset", uri);
				return false;
			}

			DownloadAssetCompletedCallback();
		}
	}
	
	public void DownloadAssetCompletedCallback ()
	{
		if (_assetUrlStack.Count > 0) {
			DownloadAssets ();
		} else {
			DownloadCompleteCallback();
		}
	}	

	private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		//Return true if the server certificate is ok
		if (sslPolicyErrors == SslPolicyErrors.None)
			return true;
		
		bool acceptCertificate = true;
		string msg = "The server could not be validated for the following reason(s):\r\n";
		
		//The server did not present a certificate
		if ((sslPolicyErrors &
		     SslPolicyErrors.RemoteCertificateNotAvailable) == SslPolicyErrors.RemoteCertificateNotAvailable)
		{
			msg = msg + "\r\n    -The server did not present a certificate.\r\n";
			acceptCertificate = false;
		}
		else
		{
			//The certificate does not match the server name
			if ((sslPolicyErrors &
			     SslPolicyErrors.RemoteCertificateNameMismatch) == SslPolicyErrors.RemoteCertificateNameMismatch)
			{
				msg = msg + "\r\n    -The certificate name does not match the authenticated name.\r\n";
				acceptCertificate = false;
			}
			
			//There is some other problem with the certificate
			if ((sslPolicyErrors &
			     SslPolicyErrors.RemoteCertificateChainErrors) == SslPolicyErrors.RemoteCertificateChainErrors)
			{
				foreach (X509ChainStatus item in chain.ChainStatus)
				{
					if (item.Status != X509ChainStatusFlags.RevocationStatusUnknown &&
					    item.Status != X509ChainStatusFlags.OfflineRevocation)
						break;
					
					if (item.Status != X509ChainStatusFlags.NoError)
					{
						msg = msg + "\r\n    -" + item.StatusInformation;
						acceptCertificate = false;
					}
				}
			}
		}
		
		//If Validation failed, present message box
		if (acceptCertificate == false)
		{
			msg = msg + "\r\nDo you wish to override the security check?";
			acceptCertificate = true;
		}
		
		return acceptCertificate;
	}


	// METHODS TO GET THE NEEDED DATA

	public float GetDisplayDuration(JToken displayData)
	{
		float duration = 0;
		float.TryParse (displayData.SelectToken ("$.config.duration").ToString (), out duration);
		return duration / 1000;
	}

	public string GetString(JToken displayData, string path)
	{

		return displayData.SelectToken ("$." + path).ToString();
	}


	public bool HasTexts(JToken displayData, int timeFilter)
	{
		JToken data = displayData.SelectToken ("$.data");
		JToken item;
		
		for(int i = 0; i < data.Children().Count(); i++) {
			item = data.Children().ElementAt(i);
			DateTime creationTime = DateTime.Parse(item.SelectToken("$.created_at").ToString());
			if(item.SelectToken("$.text") != null && Mathf.Abs((float)(DateTime.UtcNow - creationTime).TotalHours) < timeFilter)
			{
				return true;
			}
		}

		return false;
	}

	public Texture2D GetImage(JToken displayData, string path)
	{
		string fileName = Path.GetFileName(displayData.SelectToken("$." + path).ToString());
		return LoadImage(Path.Combine(DOWNLOAD_PATH, fileName));
	}

	public PostEntry[] GetPosts(JToken displayData, string imagePath, int timeFilter)
	{
		List<PostEntry> posts = new List<PostEntry> ();
		JToken data = displayData.SelectToken ("$.data");
		JToken item;

		PostEntry postEntry;

		for (int i = 0; i < data.Children().Count(); i++) {
			item = data.Children().ElementAt(i);

			DateTime creationTime = DateTime.Parse(item.SelectToken("$.created_at").ToString());
			if(Mathf.Abs((float)(DateTime.UtcNow - creationTime).TotalHours) < timeFilter)
			{
				postEntry = new PostEntry();

				//picture
				string fileName = Path.GetFileName(item.SelectToken("$." + imagePath).ToString());
				postEntry.texture = LoadImage(Path.Combine(DOWNLOAD_PATH, fileName));

				//Text
				postEntry.text = item.SelectToken("$.text").ToString();

				//UserName
				postEntry.userName = item.SelectToken("$.user.username").ToString();

				//Hashtag
				postEntry.hashtag = item.SelectToken("$.hashtags").Children().ElementAt(0).SelectToken("$.name").ToString();

				posts.Add(postEntry);
			}
		}

		return posts.ToArray ();
	}

	public Texture2D[] GetUserImages(JToken displayData, string path)
	{
		List<Texture2D> userImages = new List<Texture2D> ();

		JToken data = displayData.SelectToken ("$.data");
		JToken item;

		for(int i = 0; i < data.Children().Count(); i++) {
			item = data.Children().ElementAt(i);
			string fileName = Path.GetFileName(item.SelectToken("$." + path).ToString());
			userImages.Add(LoadImage(Path.Combine(DOWNLOAD_PATH, fileName)));
		}

		return userImages.ToArray ();
	}

	public string[] GetTexts(JToken displayData)
	{
		List<string> tweets = new List<string> ();

		JToken data = displayData.SelectToken ("$.data");
		JToken item;

		for(int i = 0; i < data.Children().Count(); i++) {
			item = data.Children().ElementAt(i);
			tweets.Add(item.SelectToken("$.text").ToString());
		}

		return tweets.ToArray ();
	}

	public string[] GetUserNames(JToken displayData)
	{
		List<string> userNames = new List<string> ();
		
		JToken data = displayData.SelectToken ("$.data");
		JToken item;
		
		for(int i = 0; i < data.Children().Count(); i++) {
			item = data.Children().ElementAt(i);
			userNames.Add(item.SelectToken("$.user.username").ToString());
		}

		return userNames.ToArray ();
	}

	public string[] GetHashtags(JToken displayData)
	{
		List<string> hashtags = new List<string> ();
		
		JToken data = displayData.SelectToken ("$.data");
		JToken item;
		
		for(int i = 0; i < data.Children().Count(); i++) {
			item = data.Children().ElementAt(i);
			hashtags.Add(item.SelectToken("$.hashtags").Children().ElementAt(0).SelectToken("$.name").ToString());
		}
		
		return hashtags.ToArray ();
	}

	public string GetMostUsedHashtag(JToken displayData)
	{
		Dictionary<string, int> hashtags = new Dictionary<string, int>();
		
		JToken data = displayData.SelectToken ("$.data");
		JToken item;
		
		for(int i = 0; i < data.Children().Count(); i++) {
			item = data.Children().ElementAt(i);

			foreach(JToken hashtagItem in item.SelectToken("$.hashtags").Children())
			{
				string hashtag = hashtagItem.SelectToken("$.name").ToString();
				if(hashtags.ContainsKey(hashtag))
				{
					hashtags[hashtag]++;
				}
				else
				{
					hashtags.Add(hashtag, 1);
				}
			}
		}
		
		var orderedHashtags = hashtags.OrderByDescending(i => i.Value);
		return orderedHashtags.Count() > 0 ? orderedHashtags.ElementAt (0).Key : "";//orderedWordCloud.ToDictionary (i => i.Key).Keys.ToArray();
	}

	public string GetVideoPath(JToken displayData)
	{
		JToken data = displayData.SelectToken ("$.data");
		if (data.Children ().Count () > 0) {
			JToken item = data.Children ().ElementAt (0);

			string fileName = Path.GetFileName (item.SelectToken ("$.video").ToString ());
			return DOWNLOAD_PATH + "/" + fileName;
		} else {
			return "";
		}
	}

	public Texture2D[] GetPhotos(JToken displayData)
	{
		List<Texture2D> photos = new List<Texture2D> ();
		
		JToken data = displayData.SelectToken ("$.data");
		JToken item;
		
		for(int i = 0; i < data.Children().Count(); i++) {
			item = data.Children().ElementAt(i);
			string fileName = Path.GetFileName(item.SelectToken("$.image").ToString());
			photos.Add(LoadImage(Path.Combine(DOWNLOAD_PATH, fileName)));
		}
		
		return photos.ToArray ();
	}

	public bool HasPhotos(JToken displayData)
	{
		JToken data = displayData.SelectToken ("$.data");
		JToken item;
		
		for(int i = 0; i < data.Children().Count(); i++) {
			item = data.Children().ElementAt(i);
			string fileName = Path.GetFileName(item.SelectToken("$.image").ToString());
			if(fileName != null && fileName != "")
			{
				return true;
			}
		}
		
		return false;
	}

	public int GetDemographicAgeCount(JToken displayData, string ageRangeId)
	{
		JToken data = displayData.SelectToken ("$.data");
		JToken item;
		
		for(int i = 0; i < data.Children().Count(); i++) {
			item = data.Children().ElementAt(i);
			if(ageRangeId == item.SelectToken("$.label").ToString())
			{
				return int.Parse(item.SelectToken("$.count").ToString());
			}
		}

		return 0;
	}

	public string[] GetWordcloud(JToken displayData)
	{
		Dictionary<string, int> wordcloud = new Dictionary<string, int>();

		JToken data = displayData.SelectToken ("$.data");
		JToken item;

		for(int i = 0; i < data.Children().Count(); i++) {
			item = data.Children().ElementAt(i);
			wordcloud.Add(item.SelectToken("$.term").ToString(), int.Parse(item.SelectToken("$.weight").ToString()));
		}

		var orderedWordCloud = wordcloud.OrderByDescending(i => i.Value);
		return orderedWordCloud.ToDictionary (i => i.Key).Keys.ToArray();
	}

	public Texture2D[] GetInfluencersImages(JToken displayData)
	{
		string[] imagePaths = GetInfluencersImagePaths (displayData);

		List<Texture2D> photos = new List<Texture2D> ();
		
		for(int i = 0; i < imagePaths.Length; i++) {
			string fileName = Path.GetFileName(imagePaths[i]);
			photos.Add(LoadImage(Path.Combine(DOWNLOAD_PATH, fileName)));
		}
		
		return photos.ToArray ();

	}

	public string[] GetInfluencersImagePaths(JToken displayData)
	{
		Dictionary<string, int> influencers = new Dictionary<string, int>();
		
		JToken data = displayData.SelectToken ("$.data");
		JToken item;
		
		for(int i = 0; i < data.Children().Count(); i++) {
			item = data.Children().ElementAt(i);
			influencers.Add(item.SelectToken("$.image").ToString(), int.Parse(item.SelectToken("$.score").ToString()));
		}
		
		var orderedWordCloud = influencers.OrderByDescending(i => i.Value);
		return orderedWordCloud.ToDictionary (i => i.Key).Keys.ToArray();
	}

	//Image loading
	public static Texture2D LoadImage(string filePath) 
	{
		Texture2D tex = null;
		byte[] fileData;
		
		if (File.Exists(filePath))     {
			fileData = File.ReadAllBytes(filePath);
			tex = new Texture2D(2, 2);
			tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
		}
		return tex;
	}
}

public struct PostEntry
{
	public Texture2D texture;
	public string hashtag;
	public string userName;
	public string text;
}

public struct DreamforceScreen
{
	public int id;
	public string name;
}