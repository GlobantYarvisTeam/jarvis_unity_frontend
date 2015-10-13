using UnityEngine;
using System.Collections;
using System.Timers;

public class salesforcePolkadotScript : MonoBehaviour {
	private float deltaT = 0.01f;

	private Texture2D generateRandomTexture(int frequency) {
		var texture = new Texture2D (frequency, frequency, TextureFormat.ARGB32, false);

		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;

		frequency = Mathf.FloorToInt (frequency);

		// set the pixel values
		for (var i = 0; i < frequency; i++)
		{
			for (var j = 0; j < frequency; j++)
			{
				var randomValue =  Random.Range(0.0f, 1.0f);
				if (randomValue < 0.10f) randomValue = 0.0f;

				texture.SetPixel(i, j, new Color(randomValue,randomValue, randomValue, randomValue));
			}
		}
		
		// Apply all SetPixel calls
		texture.Apply ();

		return texture;
	}

	public void Awake() {
		// Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
		var frequency = (int)GetComponent<Renderer> ().material.GetFloat ("_frequency");
		var noiseA = generateRandomTexture (frequency);
		var noiseB = generateRandomTexture (frequency);

		// connect texture to material of GameObject this script is attached to
		GetComponent<Renderer>().material.SetTexture("_noiseA", noiseA);
		GetComponent<Renderer>().material.SetTexture("_noiseB", noiseB);
	}

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update () {
		deltaT += Time.deltaTime;
		GetComponent<Renderer> ().material.SetFloat("_mixture", Mathf.Sin(deltaT));
	}
}
