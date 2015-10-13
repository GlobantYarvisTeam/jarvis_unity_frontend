using UnityEngine;
using System.Collections;

public class GizmoDrawer : MonoBehaviour {

	public void OnDrawGizmos()
	{
		TextSize[] textsWithLimitedSize = FindObjectsOfType<TextSize> ();
		for (int i = 0; i < textsWithLimitedSize.Length; i++) {
			Gizmos.DrawWireCube (textsWithLimitedSize[i].gameObject.transform.position, new Vector3(0, 0.5f, textsWithLimitedSize[i].wantedWidth));
		}
	}
}
