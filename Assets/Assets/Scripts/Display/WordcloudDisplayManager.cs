using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WordcloudDisplayManager : IDisplayManager {

	public float modifier = 2f;
	public int maxTries = 3;
	public Vector2 offset;
	public int minSizeIndexForRotation = 4;

	public GameObject wordCloudParent;
	public GameObject textPrefab;

	public int[] wordFontSize;
	public int[] wordFontSizeRange;
	public Font firstWordFont;
	public Font[] randomFonts;
	public string[] words;

	private List<Text> _wordsToCheck;
	private bool _checked = false;

	private List<Rect> _placedWords;
	private bool _fadeIn = false;
	private bool _fadeOut = false;
	private bool _cyclingOut = false;
	private int _currentFadeWordIndex = 0;
	private float _lastFadedWordTime = 0;

	public override void InitializeDisplay (int displayId)
	{
		words = Preloader.instance.GetWordcloud (Preloader.instance.GetRunningDisplay());
		_displayId = displayId;
		_wordsToCheck = new List<Text> ();
		_currentFadeWordIndex = 0;
		_lastFadedWordTime = 0;
		_fadeIn = false;
		_checked = false;
		
		GenerateCloud ();
	}


	public void GenerateCloud()
	{
		Text[] children = wordCloudParent.transform.GetComponentsInChildren<Text> ();
		foreach (Text child in children) {
			Destroy(child.gameObject);
		}


		for (int i = 0; i < words.Length; i++) {
			string word = words[i];
			GameObject wordContainer = GameObject.Instantiate(textPrefab);
			Text wordContainerText = wordContainer.GetComponent<Text>();

			wordContainer.transform.SetParent(wordCloudParent.transform);
			wordContainer.transform.localScale = Vector3.one;
			wordContainerText.text = word;

			if(i > 0)
			{
				int fontIndex = Random.Range(0, randomFonts.Length);
				wordContainerText.font = randomFonts[fontIndex];
			}
			else
			{
				wordContainerText.font = firstWordFont;
			}


			int wordSizeIndex = wordFontSize.Length - 1;
			for(int j = 0; j < wordFontSizeRange.Length; j++)
			{
				if(i <= wordFontSizeRange[j])
				{
					wordSizeIndex = Mathf.Clamp(j, 0, wordFontSize.Length - 1);
					break;
				}
			}

			wordContainerText.fontSize = wordFontSize[wordSizeIndex];

			_wordsToCheck.Add(wordContainerText);
		}
	}

	public void LateUpdate()
	{
		if (!_checked && CheckWidths ()) {
			RectTransform parentRectTransform = wordCloudParent.GetComponent<RectTransform> ();
			_checked = true;
			bool firstWord = true;

			_placedWords = new List<Rect> ();
			Text[] wordsToCheck = _wordsToCheck.ToArray();
			foreach (Text word in wordsToCheck) {
				RectTransform wordRectTransform = word.gameObject.GetComponent<RectTransform> ();

				bool placed = false;

				int tries = 0;

				Vector2 randomOffset;

				float wordWidth = wordRectTransform.rect.width;
				float wordHeight = wordRectTransform.rect.height;
				bool rotated = false;

				if (!firstWord) {
					rotated = word.fontSize < wordFontSize [minSizeIndexForRotation] && Random.Range (0f, 1.0f) > 0.6f;

					float randomX = Random.Range (-280, 200);
					float randomY = Random.Range (-100, 80);
					randomOffset = new Vector2 (randomX, randomY);

					if (rotated) {
						word.gameObject.transform.Rotate (Vector3.forward, -90);
						wordWidth = wordRectTransform.rect.height;
						wordHeight = wordRectTransform.rect.width * -1;
					}
				} else {
					randomOffset = new Vector2 (-(wordRectTransform.rect.width / 2), -20);
					firstWord = false;
				}

				do {
					//we first try horizontal
					Vector2 coordinates = (getTileCoordinates (tries) * modifier + randomOffset);

					if (coordinates.x + wordWidth > parentRectTransform.rect.width / 2) {
						coordinates.x = (parentRectTransform.rect.width / 2) - (coordinates.x + wordWidth);
					}

					if (rotated) {
						if ((coordinates.y + wordHeight) < parentRectTransform.rect.height / -2) {
							coordinates.y = 0 - coordinates.y + wordHeight;
						}
					} else {
						if (coordinates.y + wordHeight > parentRectTransform.rect.height / 2) {
							coordinates.y = (parentRectTransform.rect.height / 2) - (coordinates.y + wordHeight);
						}
					}



					Rect newWordRect = new Rect (coordinates.x, coordinates.y, wordWidth, wordHeight);
					bool intersects = false;


					foreach (Rect placedWord in _placedWords) {

						if (placedWord.Overlaps (newWordRect, true)) {
							intersects = true;
							break;
						} else {
							intersects = false;
						}
					}
				
					if (!intersects) {
						_placedWords.Add (newWordRect);
						placed = true;
						word.gameObject.transform.localPosition = coordinates;
					}
				
					tries++;
				} while(tries < maxTries && !placed);
			
				if (!placed) {
					_wordsToCheck.Remove(word);
					GameObject.Destroy (word.gameObject);
				}
			}
			_fadeIn = true;
		} else {
			if(_fadeIn)
			{
				if(Time.time - _lastFadedWordTime > 0.1f)
				{
					_wordsToCheck[_currentFadeWordIndex].GetComponent<Animator>().SetTrigger("FadeIn");
					_lastFadedWordTime = Time.time;
					_currentFadeWordIndex++;

					if(_currentFadeWordIndex == _wordsToCheck.Count)
					{
						_currentFadeWordIndex--;
						_fadeIn = false;
					}
				}
			}
			else if(_fadeOut)
			{
				if(Time.time - _lastFadedWordTime > 0.01f)
				{
					_wordsToCheck[_currentFadeWordIndex].GetComponent<Animator>().SetTrigger("FadeOut");
					_lastFadedWordTime = Time.time;
					_currentFadeWordIndex--;
					
					if(_currentFadeWordIndex < 0)
					{
						_currentFadeWordIndex++;
						_fadeOut = false;
						_cyclingOut = true;
					}
				}
			}
			else if(_cyclingOut)
			{
				_displayOutFinished = _wordsToCheck[0].GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle");
				_cyclingOut = !_displayOutFinished;
			}
		}
	}

	public bool CheckWidths()
	{
		foreach (Text word in _wordsToCheck) {
			if(word.rectTransform.rect.width == 0 || word.rectTransform.rect.height == 0)
			{
				return false;
			}
		}

		return true;
	}

	public override void DisplayOut ()
	{
		base.DisplayOut ();
		_displayOutFinished = false;
		_cyclingOut = true;
		_fadeOut = true;

	}

	public override void FinalizeDisplay ()
	{
	}

	public Vector2 getTileCoordinates(int tileNum)
	{
		int intRoot = Mathf.FloorToInt(Mathf.Sqrt(tileNum));
		
		int x = Mathf.RoundToInt((Mathf.RoundToInt(intRoot/2) * Mathf.Pow(-1,intRoot+1)) + (Mathf.Pow(-1,intRoot+1) * (((intRoot*(intRoot+1))-tileNum) - Mathf.Abs((intRoot*(intRoot+1))-tileNum))/2));
		
		int y = Mathf.RoundToInt((Mathf.RoundToInt(intRoot/2) * Mathf.Pow(-1,intRoot)) + (Mathf.Pow(-1,intRoot+1) * (((intRoot*(intRoot+1))-tileNum) + Mathf.Abs((intRoot*(intRoot+1))-tileNum))/2));

		return new Vector2(x,y);
	}
}
