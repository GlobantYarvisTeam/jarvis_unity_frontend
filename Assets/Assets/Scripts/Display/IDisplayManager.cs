using UnityEngine;
using System.Collections;

public abstract class IDisplayManager : MonoBehaviour {

	public bool timeDriven = true;
	public float cycleTime = 10f;
	public bool readyToCycle = false;
	public bool forceCycle = false;

	protected int _displayId;
	protected bool _displayOutFinished = false;
	protected GameObject[] _lines;

	public Animator[] animators;
	public abstract void InitializeDisplay (int displayId);
	public abstract void FinalizeDisplay();

	public virtual void DisplayOut()
	{
		_lines = GameObject.FindGameObjectsWithTag ("lines");
		if (_lines != null) {
			foreach(GameObject line in _lines)
			{
				line.GetComponent<Animator>().SetTrigger("FadeOut");
			}
		}

		// Do or not whatever is needed.
		_displayOutFinished = true;
	}

	public bool DisplayOutFinished
	{
		get{return _displayOutFinished;}	
	}
}