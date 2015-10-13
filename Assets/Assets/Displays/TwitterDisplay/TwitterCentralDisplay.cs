using UnityEngine;
using System.Collections;

public class TwitterCentralDisplay : MonoBehaviour {

	private TwitterDisplayManager displayManager;

	void Start()
	{
		displayManager = gameObject.GetComponentInParent<TwitterDisplayManager> ();
	}

	public void SetNextTweet()
	{
		displayManager.SetNextTweet ();
	}
}
