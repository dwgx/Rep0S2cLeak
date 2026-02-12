using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkConnect : MonoBehaviourPunCallbacks
{
	private bool joinedRoom;

	private string RoomName;

	private bool ConnectedToMasterServer;

	public GameObject punVoiceClient;

	private bool passwordCheck;

	private MenuPage menuPagePassword;

	private float passwordTimer = 2f;

	private void Start()
	{
		PhotonNetwork.NickName = SteamClient.Name;
		PhotonNetwork.AutomaticallySyncScene = false;
		DataDirector.instance.PhotonSetRegion();
		DataDirector.instance.PhotonSetVersion();
		DataDirector.instance.PhotonSetAppId();
		Object.Instantiate(punVoiceClient, Vector3.zero, Quaternion.identity);
		PhotonNetwork.Disconnect();
		StartCoroutine(CreateLobby());
	}

	private IEnumerator CreateLobby()
	{
		while (PhotonNetwork.NetworkingClient.State != ClientState.Disconnected && PhotonNetwork.NetworkingClient.State != ClientState.PeerCreated)
		{
			yield return null;
		}
		if (GameManager.instance.connectRandom)
		{
			Debug.Log("Connect random.");
			SteamManager.instance.SendSteamAuthTicket();
			PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = DataDirector.instance.networkRegion;
			PhotonNetwork.ConnectUsingSettings();
			yield break;
		}
		if (!GameManager.instance.localTest)
		{
			bool flag = true;
			if (SteamManager.instance.currentLobby.Id.IsValid)
			{
				flag = SteamManager.instance.currentLobby.GetData("HasPassword") == "1";
			}
			if (flag)
			{
				while (passwordTimer > 0f)
				{
					passwordTimer -= Time.deltaTime;
					yield return null;
				}
				AudioManager.instance.SetSoundSnapshot(AudioManager.SoundSnapshot.CutsceneOnly, 0.1f);
				menuPagePassword = MenuManager.instance.PageOpen(MenuPageIndex.Password);
				menuPagePassword.transform.SetParent(menuPagePassword.transform.parent.parent.parent);
				Transform _prevCursorParent = MenuCursor.instance.transform.parent;
				MenuCursor.instance.transform.SetParent(menuPagePassword.transform.parent, worldPositionStays: false);
				while ((bool)menuPagePassword)
				{
					MenuManager.instance.CutsceneSoundOverride();
					yield return null;
				}
				MenuCursor.instance.transform.SetParent(_prevCursorParent, worldPositionStays: false);
				AudioManager.instance.SetSoundSnapshot(AudioManager.SoundSnapshot.Off, 0.1f);
			}
		}
		if (!GameManager.instance.localTest)
		{
			if (SteamManager.instance.currentLobby.Id.IsValid)
			{
				RoomName = SteamManager.instance.currentLobby.GetData("RoomName");
				PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = SteamManager.instance.currentLobby.GetData("Region");
				string data = SteamManager.instance.currentLobby.GetData("BuildName");
				if (data != BuildManager.instance.version.title)
				{
					if (data != "")
					{
						Debug.Log("Build name mismatch. Leaving lobby. Build name is ''" + data + "''");
						string bodyText = "Game lobby is using version\n<color=#FDFF00><b>" + data + "</b>";
						MenuManager.instance.PagePopUpScheduled("Wrong Game Version", Color.red, bodyText, "Ok Dang", richText: true);
					}
					else
					{
						Debug.Log("Lobby closed. Leaving lobby.");
						MenuManager.instance.PagePopUpScheduled("Lobby Closed", Color.red, "The lobby has closed.", "Ok Dang", richText: true);
					}
					PhotonNetwork.Disconnect();
					SteamManager.instance.LeaveLobby();
					GameManager.instance.SetGameMode(0);
					RunManager.instance.levelCurrent = RunManager.instance.levelMainMenu;
					SceneManager.LoadSceneAsync("Reload");
					yield break;
				}
				Debug.Log("Already in lobby on Network Connect. Connecting to region: " + PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion);
			}
			else
			{
				Debug.Log("Created lobby on Network Connect.");
				PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = DataDirector.instance.networkRegion;
				SteamManager.instance.HostLobby(_open: false);
				while (!SteamManager.instance.currentLobby.Id.IsValid)
				{
					yield return null;
				}
				RoomName = SteamManager.instance.currentLobby.Id.ToString();
			}
			SteamManager.instance.SendSteamAuthTicket();
		}
		else
		{
			Debug.Log("Local test mode.");
			RunManager.instance.ResetProgress();
			PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "eu";
			RoomName = SteamClient.Name;
		}
		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to Master Server");
		if (GameManager.instance.connectRandom)
		{
			if (!string.IsNullOrEmpty(DataDirector.instance.networkServerName))
			{
				Debug.Log("I am creating a custom open lobby named: " + DataDirector.instance.networkServerName);
				RoomOptions roomOptions = new RoomOptions();
				roomOptions.CustomRoomPropertiesForLobby = new string[1] { "server_name" };
				roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable { 
				{
					"server_name",
					DataDirector.instance.networkServerName
				} };
				roomOptions.MaxPlayers = 6;
				roomOptions.IsVisible = true;
				PhotonNetwork.CreateRoom(null, roomOptions, DataDirector.instance.customLobby);
			}
			else if (!string.IsNullOrEmpty(DataDirector.instance.networkJoinServerName))
			{
				Debug.Log("Joining specific open server: " + DataDirector.instance.networkJoinServerName);
				PhotonNetwork.JoinRoom(DataDirector.instance.networkJoinServerName);
			}
			else
			{
				Debug.Log("I am joining or creating an open lobby.");
				PhotonNetwork.JoinRandomOrCreateRoom(null, 6, MatchmakingMode.FillRoom, TypedLobby.Default, null, null, new RoomOptions
				{
					MaxPlayers = 6,
					IsVisible = true
				});
			}
		}
		else if (!GameManager.instance.localTest && SteamManager.instance.currentLobby.Id.IsValid && SteamManager.instance.currentLobby.IsOwnedBy(SteamClient.SteamId))
		{
			Debug.Log("I am the owner.");
			SteamManager.instance.SetLobbyData(RoomName);
			TryJoiningRoom();
		}
		else
		{
			Debug.Log("I am not the owner.");
			TryJoiningRoom();
		}
	}

	private void TryJoiningRoom()
	{
		Debug.Log("Trying to join room: " + RoomName);
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		hashtable.Add("PASSWORD", DataDirector.instance.networkPassword);
		PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
		RoomOptions roomOptions = new RoomOptions
		{
			MaxPlayers = 6,
			IsVisible = false
		};
		ExitGames.Client.Photon.Hashtable hashtable2 = new ExitGames.Client.Photon.Hashtable();
		hashtable2.Add("PASSWORD", DataDirector.instance.networkPassword);
		roomOptions.CustomRoomProperties = hashtable2;
		PhotonNetwork.JoinOrCreateRoom(RoomName, roomOptions, DataDirector.instance.privateLobby);
	}

	public override void OnCreatedRoom()
	{
		Debug.Log("Created room successfully " + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CloudRegion);
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CloudRegion);
		joinedRoom = true;
		PhotonNetwork.AutomaticallySyncScene = true;
		RunManager.instance.waitToChangeScene = false;
		if (!PhotonNetwork.IsMasterClient)
		{
			StatsManager.instance.saveFileCurrent = "";
		}
		if (GameManager.instance.connectRandom && PhotonNetwork.IsMasterClient)
		{
			Debug.Log("Created Open Room.");
			SemiFunc.SaveFileCreate();
			SceneManager.LoadSceneAsync("Main");
		}
		if (GameManager.instance.localTest && PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.LoadLevel("Reload");
		}
	}

	public override void OnCreateRoomFailed(short returnCode, string cause)
	{
		Debug.LogError("Failed to create room: " + cause);
		MenuManager.instance.PagePopUpScheduled("Disconnected", Color.red, "<b>Cause:\n</b>" + cause, "Ok Dang", richText: true);
		PhotonNetwork.Disconnect();
		SteamManager.instance.LeaveLobby();
		GameManager.instance.SetGameMode(0);
		StartCoroutine(RunManager.instance.LeaveToMainMenu());
	}

	public override void OnJoinRoomFailed(short returnCode, string cause)
	{
		Debug.LogError("Failed to join room: " + cause);
		if (cause == "UserId found in excluded list")
		{
			cause = "You are banned from this lobby.";
		}
		MenuManager.instance.PagePopUpScheduled("Disconnected", Color.red, "<b>Cause:\n</b>" + cause, "Ok Dang", richText: true);
		PhotonNetwork.Disconnect();
		SteamManager.instance.LeaveLobby();
		GameManager.instance.SetGameMode(0);
		StartCoroutine(RunManager.instance.LeaveToMainMenu());
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log($"Disconnected from server for reason {cause}");
		if (cause != DisconnectCause.DisconnectByClientLogic && cause != DisconnectCause.DisconnectByServerLogic)
		{
			MenuManager.instance.PagePopUpScheduled("Disconnected", Color.red, "<b>Cause:\n</b>" + cause, "Ok Dang", richText: true);
			PhotonNetwork.Disconnect();
			SteamManager.instance.LeaveLobby();
			GameManager.instance.SetGameMode(0);
			StartCoroutine(RunManager.instance.LeaveToMainMenu());
		}
	}

	private void OnDestroy()
	{
		if (joinedRoom)
		{
			Debug.Log("Game Mode: Multiplayer");
			GameManager.instance.SetGameMode(1);
		}
		Debug.Log("NetworkConnect destroyed.");
		RunManager.instance.waitToChangeScene = false;
		DataDirector.instance.networkServerName = "";
		DataDirector.instance.networkJoinServerName = "";
	}
}
