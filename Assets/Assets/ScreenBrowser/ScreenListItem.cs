using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScreenListItem : MonoBehaviour {

	public int id;
	public delegate void OnClickCallbackDelegate(ScreenListItem item);
	public OnClickCallbackDelegate onClickCallback;
	public RectTransform content;

	private Text _buttonText;
	//private Button _button;

	public string ItemName {
		get {
			return _buttonText.text;
		}
		set {
			_buttonText.text = value;
		}
	}

	void Awake()
	{
		_buttonText = gameObject.GetComponentInChildren<Text> ();
		//_button = gameObject.GetComponent<Button> ();
	}

	public void OnClick()
	{
		onClickCallback (this);
	}
}