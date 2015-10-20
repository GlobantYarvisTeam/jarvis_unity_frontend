using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class InstagramDisplayManager : IDisplayManager {

	public string leftScreenTitle;
	public string leftScreenMessage;
	public string rightScreenTitle;
	public string[] photoTags;
	public Texture2D[] photoTextures;
	public PostEntry[] posts;

	//Scene Objects
	public RawImage[] photoContainers;
	public Text[] photoTagTexts;
	public Text leftScreenTitleText;
	public Text leftScreenMessageText;
	public Text rightScreenTitleText;
	public Text centralScreenTitle;

	private List<int> _indexes;

	public override void InitializeDisplay (int displayId)
	{
		_displayId = displayId;

		posts = Preloader.instance.GetPosts (
			Preloader.instance.GetRunningDisplay (), "image", 24);

		string hashtag = "#" + Preloader.instance.GetMostUsedHashtag (
			Preloader.instance.GetRunningDisplay());

		if(leftScreenMessageText != null) {
			leftScreenMessageText.text = hashtag;
		}

		centralScreenTitle.text = hashtag;

		if (posts.Length > 0) {
			UpdateInstagramIndexes ();

			int photoIndex = 0;
		
			for (int i = 0; i < 5; i++) {
				photoContainers [i].texture = 
					posts [_indexes [photoIndex]].texture;

				photoTagTexts [i].text = "@" + 
					posts [_indexes [photoIndex]].userName;

				photoIndex = ++photoIndex < _indexes.Count ? photoIndex : 0;
			}

			if(leftScreenTitleText != null) {
				leftScreenTitleText.text = leftScreenTitle;
			}

			if(rightScreenTitleText != null) {
				rightScreenTitleText.text = rightScreenTitle;
			}
		}
	}

	private void UpdateInstagramIndexes()
	{
		Random.seed = Mathf.RoundToInt(Time.time);
		_indexes = new List<int>();
		for (int i = 0; i < 5; i++) {
			int randomIndex = 0;
			int tries = 0;
			randomIndex = Random.Range(0, posts.Length);
			while(randomIndex >= posts.Length || 
			      (_indexes.Contains(randomIndex) && tries < posts.Length))
			{
				randomIndex = ++randomIndex >= posts.Length ? 0 : randomIndex;
				tries++;
			}
			
			_indexes.Add(randomIndex);
		}
	}

	public override void FinalizeDisplay ()
	{
		foreach (PostEntry post in posts) {
			Destroy(post.texture);
		}
		
		System.GC.Collect();
	}
}
