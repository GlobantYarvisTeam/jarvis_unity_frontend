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

	public Button itemPrefab;
	public GameObject contentPanel;
	public string[] items;
	public GameObject loadingScreen;

	public DisplayController displayController;
	public GameObject threeScreensDisplayContainer;
	public GameObject oneScreenDisplayContainer;
	public ToggleGroup toggleGroup;

	public void Awake()
	{
		instance = this;
	}

	public void Start()
	{
		Preloader.instance.onOperationCompleteCallback = OnScreenListReady;
		Preloader.instance.FetchScreenList ();

		string displayType = PlayerPrefs.GetString (DISPLAY_TYPE_KEY);
		
		if (displayType != string.Empty) {
			Toggle[] toggles = toggleGroup.GetComponentsInChildren<Toggle> ();
			Toggle toggle = toggles.Where (d => d.name == displayType).Select (d => d).FirstOrDefault (); 
			if (toggle != null) {
				toggle.isOn = true;
			}
		} else {
			toggleGroup.GetComponentInChildren<Toggle>().isOn = true;
		}
	}

	public void InitializeScreenList(IEnumerable<DreamforceScreen> screenList)
	{
		//Debug.Log ("InitializeScreenList");
		for (int i = contentPanel.transform.childCount - 1; i > -1 ; i--) {
			GameObject.Destroy(contentPanel.transform.GetChild(i));
		}
		foreach(var item in screenList)
		{
			CreateItem(item.id, item.name);
		}

		loadingScreen.SetActive (false);
	}

	private void CreateItem(int id, string name)
	{
		Button folderItemGameObject = Instantiate (itemPrefab) as Button;
		ScreenListItem item = 
			folderItemGameObject.GetComponent<ScreenListItem> ();

		item.id = id;
		item.ItemName = name;
		item.onClickCallback = 
			new ScreenListItem.OnClickCallbackDelegate(OnItemSelected);

		folderItemGameObject.transform.SetParent(contentPanel.transform);
		folderItemGameObject.gameObject.transform.localScale = Vector3.one;
	}

	public void OnItemSelected(ScreenListItem item)
	{
		//TODO FETCH DISPLAYS DATA
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
			}
			else
			{
				displayContainer = oneScreenDisplayContainer;
			}

			PlayerPrefs.SetString(DISPLAY_TYPE_KEY, toggle.name);

			displayContainer.SetActive(true);

			displayController.gameObject.SetActive(true);
			displayController.Initialize(displayContainer);
			gameObject.SetActive(false);
			Cursor.visible = false;
		}
	}
	
	public void OnScreenListReady()
	{
		if (Preloader.instance.GetScreenList != null && 
		    Preloader.instance.GetScreenList.Count() != 
		    contentPanel.transform.childCount) 
		{
			InitializeScreenList (Preloader.instance.GetScreenList);
		} else {
			Debug.LogWarning("The screen list is not available");
		}
	}

	public void RefreshScreenList()
	{
		Preloader.instance.FetchScreenList ();
	}
}