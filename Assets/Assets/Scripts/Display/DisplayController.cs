using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class DisplayController: MonoBehaviour {

	public float cycleTime = 10f;

	public ScreenList screenList;

	public static int instagramTimeFilter = 72;
	public static int twitterTimeFilter = 72;

	private GameObject _displayContainer;

	private IDisplayManager _currentDisplayManager;
	private float _lastCycleTime = 0f;
	private bool _cyclingDisplay = false;
	private bool _readyToCycle = false;

	private bool _initialized = false;

	public void Initialize(GameObject displayContainer)
	{
		_displayContainer = displayContainer;

		Preloader.instance.ResetDisplayIndex ();

		_currentDisplayManager = GetCurrentDisplayManager ();

		//if none of the tags are supported we have to prevent initialization and show a message or something
		if (_currentDisplayManager == null) {
			//TODO SHOW UNSUPPORTED MESSAGE
		} else {
			_currentDisplayManager.gameObject.SetActive (true);
			_currentDisplayManager.InitializeDisplay (Preloader.instance.GetRunningDisplayId());

			_cyclingDisplay = false;
			_lastCycleTime = Time.time;

			_initialized = true;
		}
	}

	public void FinalizeController()
	{
		if (_initialized) {
			_currentDisplayManager.FinalizeDisplay ();
			_currentDisplayManager.gameObject.SetActive (false);
		}

		_displayContainer.SetActive (false);
		screenList.gameObject.SetActive (true);
		_initialized = false;
		Cursor.visible = true;
		Preloader.instance.CancelUpdate ();
		gameObject.SetActive (false);
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Backspace)) {
			FinalizeController();
		}

		// TODO Should be somewhere else ? Also useless?
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit();
		}

		//If the controller was not initialized we wont do any of what follows
		if (!_initialized)
			return;


		_readyToCycle = (_currentDisplayManager.timeDriven && Time.time - _lastCycleTime > _currentDisplayManager.cycleTime) ||
			(!_currentDisplayManager.timeDriven && _currentDisplayManager.readyToCycle) || _currentDisplayManager.forceCycle;

		if (_readyToCycle)
		{
			if(!_cyclingDisplay && !_currentDisplayManager.forceCycle)
			{
				//Start the out animation for every avaialable display animator
				foreach(Animator displayAnimator in _currentDisplayManager.animators)
				{
					if(displayAnimator.gameObject.activeSelf)
					{
						displayAnimator.SetTrigger("DisplayOut");
					}
				}
				
				_currentDisplayManager.DisplayOut();
				_cyclingDisplay = true;
			}
			else
			{
				bool stillCycling = false;

				stillCycling = !_currentDisplayManager.DisplayOutFinished;

				if(!stillCycling && !_currentDisplayManager.forceCycle)
				{
					//We check in every display animator if the out animation finished
					foreach(Animator displayAnimator in _currentDisplayManager.animators)
					{
						if(displayAnimator.gameObject.activeSelf)
						{
							if(!displayAnimator.GetCurrentAnimatorStateInfo(0).IsName("DisplayOut"))
							{
								stillCycling = true;
							}
							else if(displayAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
							{
								stillCycling = true;
							}
						}
					}
				}

				//For now we just call the same display in animation
				if(!stillCycling)
				{
					// Finalize and de activate the current display
					_currentDisplayManager.FinalizeDisplay();
					_currentDisplayManager.gameObject.SetActive (false);

					// Add 1 to the current index and initialize the next display
					Preloader.instance.SetNextDisplayIndex();

					_currentDisplayManager = GetCurrentDisplayManager();
					if(_currentDisplayManager == null)
					{
						_initialized = false;
						return;
					}
					_currentDisplayManager.gameObject.SetActive (true);

					//Here we should send the right display id to retreive the right data
					_currentDisplayManager.InitializeDisplay(Preloader.instance.GetRunningDisplayId());

					_cyclingDisplay = false;
					_lastCycleTime = Time.time;
				}
			}
		}
	}

	private IDisplayManager GetCurrentDisplayManager()
	{
		int tries = 0;
		do
		{
			IDisplayManager displayManager = GetDisplayManager(Preloader.instance.GetRunningDisplayType());

			Preloader.instance.UpdateDisplayList();
			
			if(Preloader.instance.IsLastDisplay())
			{
				Preloader.instance.UpdateRunningDisplayListData();
			}

			if(displayManager != null)
			{
				return displayManager;
			}
			else
			{
				Preloader.instance.SetNextDisplayIndex();
				tries++;
			}
		}while(tries < Preloader.instance.GetDisplayListLength());

		return null;
	}

	private IDisplayManager GetDisplayManager(string tag)
	{
		switch (tag) {
			case DisplayType.TWITTER:
				if(!Preloader.instance.HasTexts(Preloader.instance.GetRunningDisplay (), twitterTimeFilter))
				{
					return null;
				}
				
				return _displayContainer.GetComponentsInChildren<TwitterDisplayManager>(true).FirstOrDefault();

			case DisplayType.WORDCLOUD:
				return _displayContainer.GetComponentsInChildren<WordcloudDisplayManager>(true).FirstOrDefault();

			case DisplayType.INSTAGRAM:
				if(!Preloader.instance.HasTexts (Preloader.instance.GetRunningDisplay (), instagramTimeFilter))
				{
					return null;
				}

				return _displayContainer.GetComponentsInChildren<InstagramDisplayManager>(true).FirstOrDefault();

			case DisplayType.PHOTOGRAPHY:
				if(!Preloader.instance.HasPhotos(Preloader.instance.GetRunningDisplay()))
				{
					return null;
				}
				return _displayContainer.GetComponentsInChildren<PhotographyDisplayManager>(true).FirstOrDefault();

			case DisplayType.VIDEO:
				if(Preloader.instance.GetVideoPath(Preloader.instance.GetRunningDisplay()) == "")
				{
					return null;
				}
				return _displayContainer.GetComponentsInChildren<VideoDisplayManager>(true).FirstOrDefault();

			case DisplayType.COUNTDOWN:
				if(IsCountdownTimeNegative(DateTime.Parse (Preloader.instance.GetString (Preloader.instance.GetRunningDisplay(), "time"))))
				{
					return null;
				}
				return _displayContainer.GetComponentsInChildren<CountdownDisplayManager>(true).FirstOrDefault();

			case DisplayType.INFLUENCERS:
				return _displayContainer.GetComponentsInChildren<InfluencersDisplayManager>(true).FirstOrDefault();

			case DisplayType.DEMOGRAPHIC:
				return _displayContainer.GetComponentsInChildren<DemographicDisplayManager>(true).FirstOrDefault();

			default:
				//TODO What to do here?
				return null;
		}
	}
	private bool IsCountdownTimeNegative(DateTime conferenceTime)
	{
		// The -1 is a hack to be revised since for some reason time is coming with an extra hour from the backend
		return ((conferenceTime - DateTime.UtcNow).TotalHours < 0);
	}
}

public static class DisplayType
{
	public const string COUNTDOWN = "countdown";
	public const string DEMOGRAPHIC = "demographic";
	public const string VIDEO = "video";
	public const string PHOTOGRAPHY = "photography";
	public const string INFLUENCERS = "influencer";
	public const string INSTAGRAM = "instagram";
	public const string TWITTER = "twitter";
	public const string WORDCLOUD = "wordcloudterm";
}