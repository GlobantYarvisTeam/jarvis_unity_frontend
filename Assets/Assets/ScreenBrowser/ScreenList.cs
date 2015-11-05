using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ScreenList : MonoBehaviour {

	public static ScreenList instance;
	public const string DISPLAY_TYPE_KEY = "DisplayType";

	public float updateListTimer = 10f;

	public Button itemPrefab;
	public GameObject contentPanel;
	public string[] items;
	public GameObject loadingScreen;

	public DisplayController displayController;
	public GameObject threeScreensDisplayContainer;
	public GameObject threeScreensDotBackground;
	public GameObject oneScreenDisplayContainer;
	public GameObject oneScreenDotBackground;
	public ToggleGroup toggleGroup;

	public GameObject errorPanel;

	public void Awake()
	{
		instance = this;
	}

	public void OnEnable()
	{
		InvokeRepeating("UpdateScreenList", 0f, updateListTimer);
	}

	public void OnDisable()
	{
		CancelInvoke();
	}

	public void Start()
	{
		string displayType = PlayerPrefs.GetString (DISPLAY_TYPE_KEY);
		
		if (displayType != string.Empty) {
			Toggle[] toggles = toggleGroup.GetComponentsInChildren<Toggle> ();
			Toggle toggle = toggles.Where (d => d.name == displayType)
				.Select (d => d).FirstOrDefault (); 
			if (toggle != null) {
				toggle.isOn = true;
			}
		} else {
			toggleGroup.GetComponentInChildren<Toggle>().isOn = true;
		}
	}

	public void UpdateScreenList()
	{
		Preloader.instance.onOperationCompleteCallback = OnScreenListReady;
		Preloader.instance.FetchScreenList (contentPanel.transform.childCount 
		                                    == 0);
	}

	public void InitializeScreenList(IEnumerable<DreamforceScreen> screenList)
	{
		//Debug.Log ("InitializeScreenList");
		for (int i = contentPanel.transform.childCount - 1; i > -1 ; i--) {
			GameObject.Destroy(contentPanel.transform.GetChild(i).gameObject);
		}
		foreach(var item in screenList)
		{
			CreateItem(item.id, item.name);
		}

		loadingScreen.SetActive (false);
	}

	private void CreateItem(int id, string name)
	{
		Button button = Instantiate (itemPrefab) as Button;
		ScreenListItem item = 
			button.GetComponent<ScreenListItem> ();

		item.id = id;
		item.ItemName = name;
		item.onClickCallback = 
			new ScreenListItem.OnClickCallbackDelegate(OnItemSelected);

		button.transform.SetParent(contentPanel.transform);
		button.gameObject.transform.localScale = Vector3.one;
	}

	public void OnItemSelected(ScreenListItem item)
	{
		errorPanel.SetActive(false);
		CancelInvoke();
		Preloader.instance.CancelScreenListUpdate();
		Preloader.instance.onOperationCompleteCallback = OnDisplayListReady;
		Preloader.instance.FetchDisplayList (item.id, true);
	}

	private void OnDisplayListReady()
	{
		Preloader.instance.onOperationCompleteCallback = null;
		Preloader.instance.UpdateDisplayList ();
		if(Preloader.instance.GetDisplayListLength() > 0)
		{
			loadingScreen.SetActive(false);

			Toggle toggle = toggleGroup.ActiveToggles().FirstOrDefault();
			GameObject displayContainer;

			if(toggle.name == "ThreeScreensToggle")
			{
				displayContainer = threeScreensDisplayContainer;
				threeScreensDotBackground.SetActive(true);
				oneScreenDotBackground.SetActive(false);

			}
			else
			{
				displayContainer = oneScreenDisplayContainer;
				threeScreensDotBackground.SetActive(false);
				oneScreenDotBackground.SetActive(true);
			}

			PlayerPrefs.SetString(DISPLAY_TYPE_KEY, toggle.name);

			displayContainer.SetActive(true);

			displayController.gameObject.SetActive(true);
			displayController.Initialize(displayContainer);
			gameObject.SetActive(false);
			Cursor.visible = false;
		}
		else
		{
			errorPanel.SetActive(false);
			errorPanel.SetActive(true);
			errorPanel.GetComponentInChildren<Text>().text = "The selected " +
				"screen has no displays.";

			errorPanel.GetComponent<Animator>().SetTrigger("ShowError");

			InvokeRepeating("UpdateScreenList", updateListTimer, updateListTimer);
		}
	}
	
	public void OnScreenListReady()
	{
		if (Preloader.instance.GetScreenList != null) 
		{
			ScreenListItem[] screenListItems = 
				contentPanel.GetComponentsInChildren<ScreenListItem>();

			// If the new list has different amount of items means something
			// was deleted or added  and we need to refresh the list.
			if (Preloader.instance.GetScreenList.Count() 
			    != screenListItems.Length)
			{
				InitializeScreenList (Preloader.instance.GetScreenList);
			}
			else
			{
				// Otherwise, there is the same amount and we need to check
				// if they are all the same. If they are not, we update the list
				bool found;
				foreach(var item in Preloader.instance.GetScreenList)
				{
					//Debug.Log("Searching for: " + item.name);
					found = false;
					foreach(ScreenListItem screenListItem in screenListItems)
					{
						if(item.id == screenListItem.id)
						{
							found = true;
							break;
						}
					}

					if(!found)
					{
						//Debug.Log("NOT FOUND");
						InitializeScreenList (Preloader.instance.GetScreenList);
						break;
					}
				}
			}
		} else {
			Debug.LogWarning("The screen list is not available");
		}
	}
}