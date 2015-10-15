using UnityEngine;
using System.Collections;

public class ClampRotation : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
	}
}
