using UnityEngine;
using System.Collections;

public class ToggleMask : MonoBehaviour {

	public GameObject mask;

	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.LeftControl) && Input.GetKeyDown (KeyCode.M)) {
			mask.SetActive(!mask.activeSelf);
		}
	}
}
