using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhotographyDisplayManager : IDisplayManager {

	public const string NAME_PHOTO_A = "tPhotoA";
	public const string NAME_PHOTO_B = "tPhotoB";

	public Texture[] photos;
	public RawImage photoContainer;
	public Texture transitionTexture;

	public float switchTime = 3f;

	public float mixRatioModifier = 0.5f;

    public RawImage auxPhoto;

	private string _lastUsedPhoto;
	private int _currentPictureIndex = 0;
	private float _lastSwitch;
	private bool _switchedPhoto;
	private bool _timeSwitchingEnabled;
	private bool _initialized = false;
	
	public override void InitializeDisplay (int displayId)
	{
		_initialized = false;
		_displayId = displayId;

		photos = Preloader.instance.GetPhotos (Preloader.instance.GetRunningDisplay());
		_currentPictureIndex = 0;
		cycleTime = photos.Length * switchTime;

		_timeSwitchingEnabled = true;
		_switchedPhoto = false;

        
        photoContainer.material.SetFloat("mixRatio", 1f);
        mixRatioModifier = Mathf.Abs(mixRatioModifier);
        CalculateMixRatio();


        if (!auxPhoto.gameObject.activeSelf)
        {
            SetTransitionPicture(NAME_PHOTO_A);
            SetNextPicture(NAME_PHOTO_B);
        }
        else
        {
            photoContainer.material.SetTexture(NAME_PHOTO_A, photos[_currentPictureIndex]);
            SetNextPicture(NAME_PHOTO_B);
        }

		_lastSwitch = Time.time - switchTime;
		_initialized = true;
	}

	public void Update()
	{
		if (!_initialized)
			return;

		if (_timeSwitchingEnabled) {
			float mixRatio = photoContainer.material.GetFloat ("mixRatio");

			if ((mixRatio == 1f || mixRatio == 0f)) {
				if (!_switchedPhoto) {
                    auxPhoto.gameObject.SetActive(false);
                    if (_lastUsedPhoto == NAME_PHOTO_A) {
						SetNextPicture (NAME_PHOTO_B);
					} else {
						SetNextPicture (NAME_PHOTO_A);
					}
				}
				
				if (Time.time - _lastSwitch > switchTime) {
					_lastSwitch = Time.time;
					mixRatioModifier *= -1;
					CalculateMixRatio();
					_switchedPhoto = false;
				}
			}
			else
			{
				CalculateMixRatio();
			}
		} else {
			CalculateMixRatio();
			_displayOutFinished = photoContainer.material.GetFloat ("mixRatio") == 1 || photoContainer.material.GetFloat ("mixRatio") == 0;
		}
	}

	private void CalculateMixRatio()
	{
		float mixRatio = photoContainer.material.GetFloat ("mixRatio");
		mixRatio = Mathf.Clamp(mixRatio + (mixRatioModifier * Time.deltaTime), 0f, 1f);
		photoContainer.material.SetFloat ("mixRatio", mixRatio);
	}

	public override void DisplayOut ()
	{
		base.DisplayOut ();
		_displayOutFinished = false;
        auxPhoto.texture = photos[_currentPictureIndex];
        _currentPictureIndex = --_currentPictureIndex < 0 ? photos.Length - 1 : _currentPictureIndex;
        _timeSwitchingEnabled = false;
		_switchedPhoto = false;
		if (_lastUsedPhoto == NAME_PHOTO_A) {
			SetTransitionPicture (NAME_PHOTO_A);
		} else {
			SetTransitionPicture (NAME_PHOTO_B);
		}
		mixRatioModifier *= -1;
	}

	public override void FinalizeDisplay ()
	{
		foreach (Texture2D texture in photos) {
            if (texture != auxPhoto.texture)
            {
                Destroy(texture);
            }
		}
		
		System.GC.Collect();
		_initialized = false;
	}

	private void SetNextPicture(string container)
	{
        photoContainer.material.SetTexture (container, photos[_currentPictureIndex]);
		_lastUsedPhoto = container;
		_currentPictureIndex = ++_currentPictureIndex == photos.Length ? 0 : _currentPictureIndex;
		_switchedPhoto = true;
	}

	private void SetTransitionPicture(string container)
	{
		photoContainer.material.SetTexture (container, transitionTexture);
	}

    public void SetAuxPhoto()
    {
        auxPhoto.gameObject.SetActive(true);
    }
}