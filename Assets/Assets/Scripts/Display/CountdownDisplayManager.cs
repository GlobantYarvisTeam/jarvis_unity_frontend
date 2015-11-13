using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class CountdownDisplayManager : IDisplayManager {

	public Text conferenceTitleText;
	public Text speakerNameText;
	public RawImage userImageContainer;
	public string conferenceTitle;
	public string speakerName;
	public Sprite userImage;
	public DateTime conferenceTime;
	public Text textHours;
	public Text textMins;
	public Text textSecs;
	public Text textHoursTitle;
	public Text textMinsTitle;
	public Text textSecsTitle;
	public string daysTitle;
	public string dayTitle;
	public string hoursTitle;
	public string minutesTitle;
	public string secondsTitle;

	public override void InitializeDisplay (int displayId)
	{
		_displayId = displayId;
		conferenceTitleText.text = Preloader.instance.GetString (Preloader.instance.GetRunningDisplay(), "title");
		speakerNameText.text = Preloader.instance.GetString (Preloader.instance.GetRunningDisplay(), "subtitle");
		userImageContainer.texture = Preloader.instance.GetImage (Preloader.instance.GetRunningDisplay(), "image");


		conferenceTime = DateTime.Parse (Preloader.instance.GetString (Preloader.instance.GetRunningDisplay(), "time"));
		//Debug.Log (Preloader.instance.GetString (Preloader.instance.GetRunningDisplay(), "time"));
		//Debug.Log (conferenceTime);
		//Debug.Log (DateTime.UtcNow);
	}

	public override void FinalizeDisplay ()
	{
		Destroy (userImageContainer.texture);
		System.GC.Collect ();
	}

	// Update is called once per frame
	void Update () {
		TimeSpan timeUntilConference = conferenceTime - DateTime.UtcNow;

		int days = timeUntilConference.Days;
		Debug.Log (days);
		string hours = "0";
		string minutes = "0";
		string seconds = "0";

		if (timeUntilConference.Days > 0) {
			if(timeUntilConference.Days > 1)
			{
				textHoursTitle.text = daysTitle;
			}
			else
			{
				textHoursTitle.text = dayTitle;
			}

			textMinsTitle.text = hoursTitle;
			textSecsTitle.text = minutesTitle;

			hours = timeUntilConference.Days.ToString ();
			minutes = timeUntilConference.Hours.ToString ();
			seconds = timeUntilConference.Minutes.ToString ();
		} else {
			textHoursTitle.text = hoursTitle;
			textMinsTitle.text = minutesTitle;
			textSecsTitle.text = secondsTitle;
			// The -1 is a hack to be revised since for some reason time is coming with an extra hour from the backend
			hours = (Mathf.FloorToInt ((float)timeUntilConference.TotalHours)).ToString ();
			minutes = timeUntilConference.Minutes.ToString ();
			seconds = timeUntilConference.Seconds.ToString ();
			if(int.Parse(hours) < 0)
			{
				hours = minutes = seconds = "00";
			}
		}

		textHours.text = hours.Length == 1 ? "0" + hours : hours;
		textMins.text = minutes.Length == 1 ? "0" + minutes : minutes;
		textSecs.text = seconds.Length == 1 ? "0" + seconds : seconds;
	}
}