using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TwitterDisplayManager : IDisplayManager {

	public float switchTime = 5f;

	public string[] tweets;
	public Texture2D[] userImages;
	public string[] userNames;
	public string[] hashtags;
	public string hashtag;

	public Text tweetText;
	public RawImage userImage;
	public Text userName;
	public Text hashTag;
	public Text title;
	public PostEntry[] posts;

	public Animator switchTweetAnimator;

	private int _currentTweetIndex = 0;
	private float _lastSwitch = 0f;
	private bool _initialized = false;
	private List<int> _indexes;

	public override void InitializeDisplay (int displayId)
	{
		_initialized = false;
		_currentTweetIndex = 0;

		posts = Preloader.instance.GetPosts (Preloader.instance.GetRunningDisplay (), "user.image", 24);
		//userImages = Preloader.instance.GetUserImages (Preloader.instance.GetRunningDisplay(), "user.image");
		//tweets = Preloader.instance.GetTexts (Preloader.instance.GetRunningDisplay ());
		//userNames = Preloader.instance.GetUserNames (Preloader.instance.GetRunningDisplay());
		//hashtags = Preloader.instance.GetHashtags (Preloader.instance.GetRunningDisplay ());
		hashtag = "#" + Preloader.instance.GetMostUsedHashtag (Preloader.instance.GetRunningDisplay());

		if (posts.Length > 0) {
			UpdateTweetIndexes ();
			_displayId = displayId;
			SetTweet (_indexes [_currentTweetIndex]);
			_lastSwitch = Time.time;
			_initialized = true;
		}
	}

	private void UpdateTweetIndexes()
	{
		Random.seed = Mathf.RoundToInt(Time.time);
		int tweetsAmount = Mathf.CeilToInt (cycleTime / switchTime);
		_indexes = new List<int>();
		for (int i = 0; i < tweetsAmount; i++) {
			int randomIndex = 0;
			int tries = 0;
			randomIndex = Random.Range(0, posts.Length);
			
			while(randomIndex >= posts.Length || (_indexes.Contains(randomIndex) && tries < posts.Length))
			{
				randomIndex = ++randomIndex >= posts.Length ? 0 : randomIndex;
				tries++;
			}

			_indexes.Add(randomIndex);
		}
	}

	public void Update()
	{
		if (!_initialized) {
			return;
		}

		if (Time.time - _lastSwitch > switchTime) {
			_lastSwitch = Time.time;
			if(switchTweetAnimator != null)
			{
				switchTweetAnimator.SetTrigger("SwitchTweet");
			}
		}
	}

	public override void FinalizeDisplay ()
	{
		foreach (PostEntry post in posts) {
			Destroy(post.texture);
		}

		System.GC.Collect();
		_initialized = false;
	}

	public void SetNextTweet()
	{
		_currentTweetIndex = ++_currentTweetIndex == _indexes.Count ? 0 : _currentTweetIndex;
		SetTweet (_indexes[_currentTweetIndex]);
	}

	private void SetTweet(int index)
	{
		tweetText.text = posts [index].text;
		userImage.texture = posts [index].texture;
		userName.text = "@" + posts [index].userName;
		hashTag.text = hashtag; //"#" + hashtags[index];
		title.text = hashtag; //"#" + hashtags[index];
	}
}
