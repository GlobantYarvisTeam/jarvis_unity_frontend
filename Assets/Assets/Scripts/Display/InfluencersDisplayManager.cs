using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfluencersDisplayManager : IDisplayManager {

	public RawImage[] influencersContainers;
	public Text[] influencersUserNames;
	public SocialInfluencer[] influencers;

	public override void InitializeDisplay (int displayId)
	{
		_displayId = displayId;

		influencers = Preloader.instance.GetInfluencers (Preloader.instance.GetRunningDisplay());

		for (int i = 0; i < influencersContainers.Length; i++) {
			if(i < influencers.Length)
			{
				influencersContainers[i].texture = influencers[i].texture;
				influencersUserNames[i].text = "@" + influencers[i].userName;
			}
		}
	}

	public override void FinalizeDisplay ()
	{
		foreach (SocialInfluencer influencer in influencers) {
			Destroy(influencer.texture);
		}
		
		System.GC.Collect();
	}
}
