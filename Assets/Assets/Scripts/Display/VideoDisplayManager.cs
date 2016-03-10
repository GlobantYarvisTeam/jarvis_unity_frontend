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
    private MovieTexture movieB;
    private MovieTexture currentMovie;
	public Texture transparent;
    public float mixRatioModifier = 0.3f;
    public RawImage auxPhoto;
	private bool _checkForOutEnd = false;
	private bool _initialized = false;
	private float _loadStartTime = 0f;
	private int _loadTries = 0;
    private float _originalMixRatioModifier;
    private bool _nextIsA = true;

    void Awake()
    {
        _originalMixRatioModifier = mixRatioModifier;
    }

	public override void InitializeDisplay (int displayId)
	{
        movie = null;
        movieB = null;
        currentMovie = null;
        _nextIsA = true;
        forceCycle = false;
		cycleTime = 30f;
		_initialized = false;
		_displayId = displayId;
        mixRatioModifier = _originalMixRatioModifier;

		_checkForOutEnd = false;
        videoContainer.material.SetFloat("mixRatio", 0f);
        videoContainer.gameObject.SetActive (false);
		_loadStartTime = Time.time;
		string videoPath = Preloader.instance.GetVideoPath (Preloader.instance.GetRunningDisplay());
		
		_loadTries = 0;
		//Debug.Log ("VIDEO PATH: " + videoPath);
		if (videoPath != "") {
            //SetVideo(videoPath);
			StartCoroutine (LoadMovie(videoPath));
		} else {
			cycleTime = 0f;
		}
	}
	
	private void OnVideoLoaded()
	{	
		//mixRatioModifier = Mathf.Abs (mixRatioModifier);
		videoContainer.gameObject.SetActive (true);

        if (!_nextIsA)
        {
            videoContainer.material.SetFloat("mixRatio", 0f);
        }
        else
        {
            videoContainer.material.SetFloat("mixRatio", 1f);
        }
        //currentMovie.Stop ();
		
		float movieDuration = Preloader.instance.GetDisplayDuration (Preloader.instance.GetRunningDisplay());

        cycleTime = movieDuration + (Time.time - _loadStartTime);

        currentMovie.loop = false;

        if (movieB == null)
        {
            videoContainer.material.SetTexture(NAME_PHOTO_A, movie);
            videoContainer.material.SetTexture(NAME_PHOTO_B, transparent);
        }
        else
        {
            if (videoContainer.material.GetTexture(NAME_PHOTO_A) != movie)
            {
                videoContainer.material.SetTexture(NAME_PHOTO_A, movie);
            }

            if (videoContainer.material.GetTexture(NAME_PHOTO_B) != movieB)
            {
                videoContainer.material.SetTexture(NAME_PHOTO_B, movieB);
            }
        }
		
		_initialized = true;
	}
	
	public void Update()
	{
		if (!_initialized)
        {
			if(currentMovie != null && currentMovie.isReadyToPlay)
			{
				if(currentMovie.isPlaying)
				{
					//Debug.Log("OnVideoLoaded");
					OnVideoLoaded();
					int newWidth = 0;
					float localHeight = this.gameObject.GetComponent<RectTransform>().rect.height;
					if(currentMovie.height > 0)
					{
						newWidth = Mathf.CeilToInt(currentMovie.width * ((float)localHeight / (float)currentMovie.height));
					}
					
					videoContainer.rectTransform.sizeDelta = 
						new Vector2(newWidth , localHeight);
				}
				else
				{
                    //Debug.Log("MoviePlay");
                    currentMovie.Play();
				}
			}
			return;
		}
		
		CalculateMixRatio ();
		_displayOutFinished = _checkForOutEnd && (videoContainer.material.GetFloat ("mixRatio") == 1 || videoContainer.material.GetFloat ("mixRatio") == 0);
		if(!_checkForOutEnd && 
		   (videoContainer.material.GetFloat ("mixRatio") == 1 || videoContainer.material.GetFloat ("mixRatio") == 0))
		{
            if (_nextIsA)
            {
                if (movie != null)
                {
                    DestroyMaterialTexture(NAME_PHOTO_A);

                    DestroyImmediate(movie, true);
                    movie = null;
                }
            }
            else
            {
                if (movieB != null)
                {
                    DestroyMaterialTexture(NAME_PHOTO_B);

                    DestroyImmediate(movieB, true);
                    movieB = null;
                }
            }
            if (currentMovie != null)
            {
                currentMovie.Play();
            }
            Destroy(auxPhoto.texture);
            auxPhoto.gameObject.SetActive(false);
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

        if (_nextIsA)
        {
            videoContainer.material.SetTexture(NAME_PHOTO_A, transparent);
            videoContainer.material.SetTexture(NAME_PHOTO_B, movieB);
        }
        else
        {
            videoContainer.material.SetTexture(NAME_PHOTO_A, movie);
            videoContainer.material.SetTexture(NAME_PHOTO_B, transparent);
        }

        _displayOutFinished = false;
		mixRatioModifier *= -1;
		_checkForOutEnd = true;
	}
	
	public override void FinalizeDisplay ()
	{
        DestroyMaterialTexture(NAME_PHOTO_A);
        DestroyMaterialTexture(NAME_PHOTO_B);
        Destroy(movie);
        Destroy(movieB);
        Destroy(currentMovie);
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
				forceCycle = true;
			}

            yield break;
		} else {
            currentMovie = diskMovieDir.movie;
            Destroy(diskMovieDir.movie);

            if (_nextIsA)
            {
                videoContainer.material.SetFloat("mixRatio", 0f);

                if (movie != null)
                {
                    Destroy(movie);
                }

                movie = currentMovie;
            }
            else
            {
                videoContainer.material.SetFloat("mixRatio", 1f);

                if (movieB != null)
                {
                    Destroy(movieB);
                }

                movieB = currentMovie;
            }

            System.GC.Collect();
            _nextIsA = !_nextIsA;
            _initialized = false;
        }
	}

    //private void SetVideo(string filePath)
    //{
    //    currentMovie = Preloader.instance.GetVideo(filePath);

    //    if (currentMovie != null)
    //    {
    //        if (_nextIsA)
    //        {
    //            videoContainer.material.SetFloat("mixRatio", 0f);

    //            if (movie != null)
    //            {
    //                Destroy(movie);
    //            }

    //            movie = currentMovie;
    //        }
    //        else
    //        {
    //            videoContainer.material.SetFloat("mixRatio", 1f);

    //            if (movieB != null)
    //            {
    //                Destroy(movieB);
    //            }

    //            movieB = currentMovie;
    //        }

    //        _nextIsA = !_nextIsA;
    //        _initialized = false;
    //    }
    //    else
    //    {
    //        cycleTime = 0;
    //        forceCycle = true;
    //    }
    //}

    public void AddNextVideo(int displayId)
    {
        _displayId = displayId;
        mixRatioModifier *= -1;

        _loadStartTime = Time.time;
        string videoPath = Preloader.instance.GetVideoPath(Preloader.instance.GetRunningDisplay());

        _loadTries = 0;
        //Debug.Log ("VIDEO PATH: " + videoPath);
        if (videoPath != "")
        {
            //SetVideo(videoPath);
            StartCoroutine("LoadMovie", videoPath);
        }
        else
        {
            cycleTime = 0f;
        }
    }

    public void SetAuxPhoto(Texture2D texture)
    {
        auxPhoto.gameObject.SetActive(true);
        auxPhoto.texture = texture;
    }

    private void DestroyMaterialTexture(string textureName)
    {
        if (videoContainer.material.GetTexture(textureName).name != transparent.name)
        {
            DestroyImmediate(videoContainer.material.GetTexture(textureName), true);
        }
    }
}