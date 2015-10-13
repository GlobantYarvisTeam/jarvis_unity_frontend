using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenTypeSelector : MonoBehaviour {

	public CanvasScaler canvas;

	public void Update()
	{
		if ((float)Screen.width < (float)Screen.height * 1.8) {
			canvas.referenceResolution = new Vector2 (860, 650);
		} else {
			canvas.referenceResolution = new Vector2 (800, 600);
		}
	}
}