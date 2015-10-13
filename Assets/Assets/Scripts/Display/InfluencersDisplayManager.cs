using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InfluencersDisplayManager : IDisplayManager {

	public RawImage[] influencersContainers;
	public Texture[] influencersTextures;

	public override void InitializeDisplay (int displayId)
	{
		_displayId = displayId;

		influencersTextures = Preloader.instance.GetInfluencersImages (Preloader.instance.GetRunningDisplay());

		for (int i = 0; i < influencersContainers.Length; i++) {
			if(i < influencersTextures.Length)
			{
				influencersContainers[i].texture = influencersTextures[i];
			}
		}
	}

	public override void FinalizeDisplay ()
	{
		foreach (Texture2D texture in influencersTextures) {
			Destroy(texture);
		}
		
		System.GC.Collect();
	}
}
