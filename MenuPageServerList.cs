using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class MenuPageServerList : MonoBehaviourPunCallbacks
{
	private class ServerListRoom
	{
		public string displayName;

		public string roomName;

		public int playerCount;

		public int maxPlayers;
	}

	public GameObject serverElementPrefab;

	public Transform serverElementParent;

	public MenuLoadingGraphics loadingGraphics;

	private bool receivedList;

	private List<ServerListRoom> roomList = new List<ServerListRoom>();

	private List<ServerListRoom> roomListSearched = new List<ServerListRoom>();

	private int pageRooms = 8;

	private int pageCurrent;

	private int pagePrevious = -1;

	private int pageMax;

	private MenuPage menuPage;

	[Space]
	public MenuButtonArrow buttonNext;

	public MenuButtonArrow buttonPrevious;

	[Space]
	public RectTransform searchOffset;

	public CanvasGroup searchTextCanvas;

	public TextMeshProUGUI searchText;

	public AnimationCurve searchOffsetCurve;

	private float searchOffsetLerp;

	internal string searchString;

	private bool searchActive;

	private bool searchInProgress;

	private void Start()
	{
		menuPage = GetComponent<MenuPage>();
		buttonNext.HideSetInstant();
		buttonPrevious.HideSetInstant();
		StartCoroutine(GetServerList());
	}

	private IEnumerator GetServerList()
	{
		PhotonNetwork.Disconnect();
		while (PhotonNetwork.NetworkingClient.State != ClientState.Disconnected && PhotonNetwork.NetworkingClient.State != ClientState.PeerCreated)
		{
			yield return null;
		}
		SteamManager.instance.SendSteamAuthTicket();
		DataDirector.instance.PhotonSetRegion();
		DataDirector.instance.PhotonSetVersion();
		DataDirector.instance.PhotonSetAppId();
		PhotonNetwork.ConnectUsingSettings();
		while (!receivedList)
		{
			yield return null;
		}
		loadingGraphics.SetDone();
		float _pitch = 1f;
		float _positionY = 0f;
		int _rooms = 0;
		foreach (ServerListRoom room in roomList)
		{
			CreateServerElement(ref _positionY, room, _rooms, MenuElementServer.IntroType.Vertical);
			float pitch = MenuManager.instance.soundPageIntro.Pitch;
			MenuManager.instance.soundPageIntro.Pitch = 1f + _pitch;
			MenuManager.instance.soundPageIntro.Play(Vector3.zero, 0.75f);
			MenuManager.instance.soundPageIntro.Pitch = pitch;
			_pitch += 0.1f;
			_rooms++;
			if (_rooms >= pageRooms)
			{
				break;
			}
			yield return new WaitForSecondsRealtime(0.1f);
		}
	}

	public override void OnConnectedToMaster()
	{
		PhotonNetwork.JoinLobby(DataDirector.instance.customLobby);
	}

	public override void OnRoomListUpdate(List<RoomInfo> _roomList)
	{
		roomList.Clear();
		foreach (RoomInfo _room in _roomList)
		{
			if (!_room.RemovedFromList && _room.IsOpen && _room.PlayerCount < _room.MaxPlayers)
			{
				ServerListRoom serverListRoom = new ServerListRoom();
				serverListRoom.displayName = (string)_room.CustomProperties["server_name"];
				serverListRoom.roomName = _room.Name;
				serverListRoom.playerCount = _room.PlayerCount;
				serverListRoom.maxPlayers = _room.MaxPlayers;
				roomList.Add(serverListRoom);
			}
		}
		roomList.Shuffle();
		SetPageLogic();
		receivedList = true;
		PhotonNetwork.Disconnect();
	}

	private void Update()
	{
		if (pageMax == 0 || searchInProgress)
		{
			buttonNext.Hide();
			buttonPrevious.Hide();
		}
		else
		{
			if (pageCurrent >= pageMax)
			{
				buttonNext.Hide();
			}
			if (pageCurrent <= 0)
			{
				buttonPrevious.Hide();
			}
		}
		if (searchOffsetLerp < 1f)
		{
			searchOffsetLerp += Time.deltaTime * 5f;
			if (searchActive)
			{
				searchOffset.anchoredPosition = new Vector3(0f, Mathf.LerpUnclamped(0f, 8f, searchOffsetCurve.Evaluate(searchOffsetLerp)), 0f);
				searchTextCanvas.alpha = Mathf.Lerp(searchTextCanvas.alpha, 1f, searchOffsetLerp);
			}
			else
			{
				searchOffset.anchoredPosition = new Vector3(0f, Mathf.LerpUnclamped(8f, 0f, searchOffsetCurve.Evaluate(searchOffsetLerp)), 0f);
				searchTextCanvas.alpha = Mathf.Lerp(searchTextCanvas.alpha, 0f, searchOffsetLerp);
			}
		}
		if (SemiFunc.InputDown(InputKey.Back) && MenuManager.instance.currentMenuPageIndex == MenuPageIndex.ServerList)
		{
			ExitPage();
		}
	}

	public void ExitPage()
	{
		MenuManager.instance.PageCloseAll();
		MenuManager.instance.PageOpen(MenuPageIndex.PublicGameChoice);
	}

	private void CreateServerElement(ref float _positionY, ServerListRoom _room, int _index, MenuElementServer.IntroType _introType)
	{
		GameObject obj = Object.Instantiate(serverElementPrefab, serverElementParent);
		obj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, _positionY, 0f);
		MenuElementServer component = obj.GetComponent<MenuElementServer>();
		component.textName.text = _room.displayName;
		component.textPlayers.text = _room.playerCount + "/" + _room.maxPlayers;
		component.roomName = _room.roomName;
		component.introType = _introType;
		component.menuButtonPopUp.bodyText = "Are you sure you want to join\n''" + _room.displayName + "''";
		_positionY -= 28f;
	}

	private IEnumerator UpdatePage()
	{
		foreach (Transform item in serverElementParent)
		{
			Object.Destroy(item.gameObject);
		}
		MenuElementServer.IntroType _introType = MenuElementServer.IntroType.Right;
		if (pagePrevious > pageCurrent)
		{
			_introType = MenuElementServer.IntroType.Left;
		}
		List<ServerListRoom> _list = roomList;
		if (searchActive)
		{
			_list = roomListSearched;
		}
		float _pitch = 1f;
		float _positionY = 0f;
		int _page = pageCurrent * pageRooms;
		for (int i = 0; i < pageRooms; i++)
		{
			if (_page >= _list.Count)
			{
				break;
			}
			float pitch = MenuManager.instance.soundPageIntro.Pitch;
			MenuManager.instance.soundPageIntro.Pitch = 1f + _pitch;
			MenuManager.instance.soundPageIntro.Play(Vector3.zero, 0.5f);
			MenuManager.instance.soundPageIntro.Pitch = pitch;
			CreateServerElement(ref _positionY, _list[_page], _page, _introType);
			yield return new WaitForSecondsRealtime(0.05f);
			_pitch += 0.1f;
			_page++;
		}
		pagePrevious = pageCurrent;
	}

	private IEnumerator SearchLogic()
	{
		searchInProgress = true;
		foreach (Transform item2 in serverElementParent)
		{
			Object.Destroy(item2.gameObject);
		}
		loadingGraphics.Reset();
		roomListSearched.Clear();
		if (searchActive)
		{
			List<string> _searchTerms = new List<string>();
			string[] array = searchString.Split(' ');
			foreach (string item in array)
			{
				_searchTerms.Add(item);
			}
			searchText.text = "''";
			foreach (string item3 in _searchTerms)
			{
				if (_searchTerms.IndexOf(item3) == 0)
				{
					searchText.text += item3;
					continue;
				}
				TextMeshProUGUI textMeshProUGUI = searchText;
				textMeshProUGUI.text = textMeshProUGUI.text + " + " + item3;
			}
			searchText.text += "''";
			int _logicCurrent = 0;
			int _logicMax = 5;
			foreach (ServerListRoom _room in roomList)
			{
				bool _add = true;
				foreach (string _term in _searchTerms)
				{
					_logicCurrent++;
					if (_logicCurrent >= _logicMax)
					{
						_logicCurrent = 0;
						yield return null;
					}
					if (!_room.displayName.ToLower().Contains(_term.ToLower()))
					{
						_add = false;
						break;
					}
				}
				if (_add)
				{
					roomListSearched.Add(_room);
				}
			}
			roomListSearched.Shuffle();
		}
		else
		{
			yield return new WaitForSeconds(1f);
		}
		SetPageLogic();
		loadingGraphics.SetDone();
		float _pitch = 1f;
		float _positionY = 0f;
		List<ServerListRoom> list = roomList;
		if (searchActive)
		{
			list = roomListSearched;
		}
		int _rooms = 0;
		foreach (ServerListRoom item4 in list)
		{
			CreateServerElement(ref _positionY, item4, _rooms, MenuElementServer.IntroType.Vertical);
			float pitch = MenuManager.instance.soundPageIntro.Pitch;
			MenuManager.instance.soundPageIntro.Pitch = 1f + _pitch;
			MenuManager.instance.soundPageIntro.Play(Vector3.zero, 0.75f);
			MenuManager.instance.soundPageIntro.Pitch = pitch;
			_pitch += 0.1f;
			_rooms++;
			if (_rooms >= pageRooms)
			{
				break;
			}
			yield return new WaitForSecondsRealtime(0.1f);
		}
		searchInProgress = false;
	}

	public void SetSearch(string _searchString)
	{
		searchString = _searchString;
		bool flag = searchActive;
		if (!string.IsNullOrEmpty(searchString))
		{
			searchActive = true;
		}
		else
		{
			searchActive = false;
		}
		if (searchActive != flag)
		{
			searchOffsetLerp = 0f;
		}
		StartCoroutine(SearchLogic());
	}

	private void SetPageLogic()
	{
		pageCurrent = 0;
		pageMax = 0;
		List<ServerListRoom> list = roomList;
		if (searchActive)
		{
			list = roomListSearched;
		}
		if (list.Count > pageRooms)
		{
			pageMax = Mathf.CeilToInt(list.Count / pageRooms);
		}
		if (pageMax > 0 && pageMax * pageRooms == list.Count)
		{
			pageMax--;
		}
	}

	private void OnDestroy()
	{
		PhotonNetwork.Disconnect();
	}

	public void ButtonCreateNew()
	{
		if (!searchInProgress)
		{
			MenuManager.instance.PageOpenOnTop(MenuPageIndex.ServerListCreateNew).GetComponent<MenuPageServerListCreateNew>().menuPageParent = menuPage;
		}
	}

	public void ButtonSearch()
	{
		if (!searchInProgress)
		{
			MenuPageServerListSearch component = MenuManager.instance.PageOpenOnTop(MenuPageIndex.ServerListSearch).GetComponent<MenuPageServerListSearch>();
			component.menuPageParent = menuPage;
			component.menuPageServerList = this;
		}
	}

	public void ButtonNextPage()
	{
		if (!searchInProgress && pageCurrent < pageMax)
		{
			MenuManager.instance.soundPageIntro.Play(Vector3.zero, 0.75f);
			pageCurrent++;
			StopAllCoroutines();
			StartCoroutine(UpdatePage());
		}
	}

	public void ButtonPreviousPage()
	{
		if (!searchInProgress && pageCurrent > 0)
		{
			MenuManager.instance.soundPageOutro.Play(Vector3.zero, 0.75f);
			pageCurrent--;
			StopAllCoroutines();
			StartCoroutine(UpdatePage());
		}
	}
}
