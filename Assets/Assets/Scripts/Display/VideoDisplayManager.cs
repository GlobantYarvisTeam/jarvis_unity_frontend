using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;

public class VideoDisplayManager : IDisplayManager {
	public const string NAME_PHOTO_A = "tPhotoA";
	public const string NAME_PHOTO_B = "tPhotoB";
	
	public RawImage videoContainer;
	private MovieTexture movie;
	public Texture transparent;
	public float mixRatioModifier = 0.3f;
	
	private bool _checkForOutEnd = false;
	private bool _initialized = false;
	private float _loadStartTime = 0f;
	private int _loadTries = 0;
	
	public override void InitializeDisplay (int displayId)
	{
		cycleTime = 30f;
		_initialized = false;
		_displayId = displayId;
		
		_checkForOutEnd = false;
		videoContainer.gameObject.SetActive (false);
		movie = null;
		_loadStartTime = Time.time;
		string videoPath = Preloader.instance.GetVideoPath (Preloader.instance.GetRunningDisplay());
		
		_loadTries = 0;
		//Debug.Log ("VIDEO PATH: " + videoPath);
		if (videoPath != "") {
			StartCoroutine ("LoadMovie", videoPath);
		} else {
			cycleTime = 0f;
		}
	}
	
	private void OnVideoLoaded()
	{	
		mixRatioModifier = Mathf.Abs (mixRatioModifier);
		videoContainer.gameObject.SetActive (true);
		videoContainer.material.SetFloat ("mixRatio", 0f);
		movie.Stop ();
		
		float movieDuration = Preloader.instance.GetDisplayDuration (Preloader.instance.GetRunningDisplay());
		
		cycleTime = movieDuration + (Time.time - _loadStartTime);
		
		movie.loop = false;
		videoContainer.material.SetTexture (NAME_PHOTO_A, movie);
		videoContainer.material.SetTexture (NAME_PHOTO_B, transparent);
		
		_initialized = true;
	}
	
	public void Update()
	{
		if (!_initialized) {
			if(movie != null && movie.isReadyToPlay)
			{
				if(movie.isPlaying)
				{
					//Debug.Log("OnVideoLoaded");
					OnVideoLoaded();
					int newWidth = 0;
					float localHeight = this.gameObject.GetComponent<RectTransform>().rect.height;
					if(movie.height > 0)
					{
						newWidth = Mathf.CeilToInt(movie.width * ((float)localHeight / (float)movie.height));
					}
					
					videoContainer.rectTransform.sizeDelta = 
						new Vector2(newWidth , localHeight);
				}
				else
				{
					//Debug.Log("MoviePlay");
					movie.Play();
				}
			}
			return;
		}
		
		CalculateMixRatio ();
		_displayOutFinished = _checkForOutEnd && (videoContainer.material.GetFloat ("mixRatio") == 1 || videoContainer.material.GetFloat ("mixRatio") == 0);
		if(!_checkForOutEnd && !movie.isPlaying && 
		   (videoContainer.material.GetFloat ("mixRatio") == 1 || videoContainer.material.GetFloat ("mixRatio") == 0))
		{
			movie.Play ();
		}
	}
	
	private void CalculateMixRatio()
	{
		float mixRatio = videoContainer.material.GetFloat ("mixRatio");
		mixRatio = Mathf.Clamp(mixRatio + (mixRatioModifier * Time.deltaTime), 0f, 1f);
		videoContainer.material.SetFloat ("mixRatio", mixRatio);
	}
	
	public override void DisplayOut ()
	{
		base.DisplayOut ();
		_displayOutFinished = false;
		mixRatioModifier *= -1;
		_checkForOutEnd = true;
	}
	
	public override void FinalizeDisplay ()
	{
		Destroy (movie);
		System.GC.Collect();
	}
	
	public IEnumerator LoadMovie(string filePath) {
		//Debug.Log ("Loading video: " + filePath);
		WWW diskMovieDir = new WWW ("file:///" + filePath); //"http://techslides.com/demos/sample-videos/small.ogv" TEST VIDEO
		
		while (!diskMovieDir.isDone) {
			//Debug.Log(diskMovieDir.progress);
			yield return diskMovieDir;
		}
		
		//Debug.Log ("MOVIE LOADED: " + diskMovieDir.movie.duration);
		
		if (diskMovieDir.error != null) {
			//Debug.LogError ("ERROR: " + diskMovieDir.error);
			_loadTries++;
			if(_loadTries < 3)
			{
				StartCoroutine ("LoadMovie", filePath);
			}
			else
			{
				cycleTime = 0;
			}
			
			return false;
		} else {
			movie = diskMovieDir.movie;
		}
	}
}