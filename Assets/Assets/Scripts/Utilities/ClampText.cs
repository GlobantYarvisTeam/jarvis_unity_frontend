using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ClampText : MonoBehaviour {

	public float maxWidth;

	private bool _clamp;
	private bool _clamped;
	private Text _uiText;

	public void Awake()
	{
		_uiText = gameObject.GetComponent<Text>();
	}

	public string text
	{
		set
		{
			_clamp = false;
			_uiText.text = value;
			Invoke("StartClamp", 1f);
		}
	}

	private void StartClamp()
	{
		_clamp = true;
		_clamped = false;
	}

	public void LateUpdate()
	{
		if(_clamp)
		{
			if(_uiText.rectTransform.rect.width > maxWidth)
			{
				if(_uiText.text.Length - 1 > 0)
				{
					_clamped = true;
					_uiText.text = _uiText.text.Substring(0, _uiText.text.Length - 1);
				}
			} else {
				if(_clamped)
				{
					_uiText.text += "...";
				}

				_clamp = false;
			}
		}
	}
}
