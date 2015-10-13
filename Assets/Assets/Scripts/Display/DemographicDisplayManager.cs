using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DemographicDisplayManager : IDisplayManager {

	public Image[] boxes;
	public int[] boxesPercentage;
	public float maxHeight;
	public float boxAnimationSpeed = 2f;
	public Text femalePercentage;
	public Text malePercentage;

	private bool _animating;
	private float _currentTargetHeight;

	public override void InitializeDisplay (int displayId)
	{
		_displayId = displayId;
		UpdatePercentages ();
		for (int i = 0; i < boxes.Length; i++) {
			Rect rect = boxes[i].rectTransform.rect;
			rect.height = 0;
			boxes[i].rectTransform.sizeDelta = new Vector2(boxes[i].rectTransform.sizeDelta.x, 0);
		}
		_currentTargetHeight = maxHeight;
		_animating = true;
	}

	public void Update()
	{
		if (_animating) {
			_animating = false;
			for (int i = 0; i < boxes.Length; i++) {
				float targetHeight = (boxesPercentage [i] * _currentTargetHeight) / 100;
				float newHeight = iTween.FloatUpdate (boxes [i].rectTransform.rect.height, targetHeight, boxAnimationSpeed);

				boxes [i].rectTransform.sizeDelta = new Vector2 (boxes [i].rectTransform.sizeDelta.x, newHeight);

				if (Mathf.Abs( newHeight - targetHeight) > 0.2f) {
					_animating = true;
				}
			}
		}

		_displayOutFinished = !_animating && _currentTargetHeight == 0f;
	}

	private void UpdatePercentages()
	{
		//Boxes percentages
		List<int> counts = new List<int> ();

		counts.Add(Preloader.instance.GetDemographicAgeCount (Preloader.instance.GetRunningDisplay (), "18-20"));
		counts.Add(Preloader.instance.GetDemographicAgeCount (Preloader.instance.GetRunningDisplay (), "21-24"));
		counts.Add(Preloader.instance.GetDemographicAgeCount (Preloader.instance.GetRunningDisplay (), "25-34"));
		counts.Add(Preloader.instance.GetDemographicAgeCount (Preloader.instance.GetRunningDisplay (), "35-44"));
		counts.Add(Preloader.instance.GetDemographicAgeCount (Preloader.instance.GetRunningDisplay (), "45-54"));
		counts.Add(Preloader.instance.GetDemographicAgeCount (Preloader.instance.GetRunningDisplay (), "55-64"));
		counts.Add(Preloader.instance.GetDemographicAgeCount (Preloader.instance.GetRunningDisplay (), "65+"));

		int totalCount = 0;
		foreach (int count in counts) {
			totalCount += count;
		}

		for (int i = 0; i < counts.Count; i++) {
			counts[i] = (counts[i] * 100) / totalCount;
		}

		boxesPercentage = counts.ToArray ();

		//Gender percentage
		int femaleCount = Preloader.instance.GetDemographicAgeCount (Preloader.instance.GetRunningDisplay (), "female");
		int maleCount = Preloader.instance.GetDemographicAgeCount (Preloader.instance.GetRunningDisplay (), "male");

		femalePercentage.text = Mathf.RoundToInt((femaleCount * 100f) / (float)(femaleCount + maleCount)).ToString();
		malePercentage.text = Mathf.RoundToInt((maleCount * 100f) / (float)(femaleCount + maleCount)).ToString();
	}

	public override void DisplayOut ()
	{
		base.DisplayOut ();
		_displayOutFinished = false;
		_currentTargetHeight = 0f;
		_animating = true;
	}

	public override void FinalizeDisplay ()
	{
		//TODO Finalize this display data
	}
}
