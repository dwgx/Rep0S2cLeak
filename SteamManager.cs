using System;
using System.Text;
using Discord.Sdk;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
	[Serializable]
	public class Developer
	{
		public string name;

		public string steamID;
	}

	public static SteamManager instance;

	internal Lobby currentLobby;

	internal Lobby noLobby;

	internal bool joinLobby;

	private bool privateLobby;

	public GameObject networkConnectPrefab;

	internal AuthTicket steamAuthTicket;

	internal ulong lobbyIdToAutoJoin;

	[Space]
	internal bool developerMode;

	internal SemiFunc.User developerUser = SemiFunc.User.None;

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			try
			{
				SteamClient.Init(3241660u);
			}
			catch (Exception ex)
			{
				Debug.LogError("Steamworks failed to initialize. Error: " + ex.Message);
			}
			string text = SteamClient.SteamId.ToString();
			Debug.Log("STEAM ID: " + text);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnEnable()
	{
		SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
		SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
		SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
		SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeft;
		SteamMatchmaking.OnLobbyMemberDataChanged += OnLobbyMemberDataChanged;
		SteamFriends.OnGameOverlayActivated += OnGameOverlayActivated;
	}

	private void OnDisable()
	{
		SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
		SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
		SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
		SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeft;
		SteamMatchmaking.OnLobbyMemberDataChanged -= OnLobbyMemberDataChanged;
		SteamFriends.OnGameOverlayActivated -= OnGameOverlayActivated;
	}

	private void Start()
	{
		GetSteamAuthTicket(out steamAuthTicket);
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs.Length < 2)
		{
			return;
		}
		for (int i = 0; i < commandLineArgs.Length - 1; i++)
		{
			if (commandLineArgs[i].ToLower() == "+connect_lobby")
			{
				if (ulong.TryParse(commandLineArgs[i + 1], out var result) && result != 0)
				{
					lobbyIdToAutoJoin = result;
				}
				break;
			}
		}
	}

	private void OnLobbyMemberJoined(Lobby _lobby, Friend _friend)
	{
		Debug.Log("Steam: Lobby member joined: " + _friend.Name);
		if (privateLobby && (bool)MenuPageLobby.instance)
		{
			MenuPageLobby.instance.JoiningPlayer(_friend.Name);
		}
		SetPlayedWith();
		UpdateSteamFriendGrouping(_lobby);
		DiscordManager.instance?.activityParty?.SetCurrentSize(_lobby.MemberCount);
		DiscordManager.instance?.RefreshDiscordRichPresence();
	}

	private void OnLobbyMemberLeft(Lobby _lobby, Friend _friend)
	{
		Debug.Log("Steam: Lobby member left: " + _friend.Name);
		UpdateSteamFriendGrouping(_lobby);
		DiscordManager.instance?.activityParty?.SetCurrentSize(_lobby.MemberCount);
		DiscordManager.instance?.RefreshDiscordRichPresence();
	}

	private void OnLobbyMemberDataChanged(Lobby _lobby, Friend _friend)
	{
		Debug.Log(" ");
		Debug.Log("Steam: Lobby member data changed for: " + _friend.Name);
		Debug.Log("I am " + SteamClient.Name);
		Debug.Log("Current Owner: " + _lobby.Owner.Name);
		if (PhotonNetwork.IsMasterClient && RunManager.instance.masterSwitched && (ulong)SteamClient.SteamId == (ulong)_lobby.Owner.Id)
		{
			Debug.Log("I am the new owner and i am locking the lobby.");
			LockLobby();
		}
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			CancelSteamAuthTicket();
			SteamClient.Shutdown();
		}
	}

	internal void OnGameLobbyJoinRequested(Lobby _lobby, SteamId _steamID)
	{
		if (SemiFunc.IsSplashScreen())
		{
			lobbyIdToAutoJoin = _lobby.Id;
		}
		else
		{
			JoinSteamLobby(_lobby, _steamID);
		}
	}

	internal async void JoinSteamLobby(Lobby _lobby, SteamId _steamID)
	{
		if ((ulong)_lobby.Id == (ulong)currentLobby.Id)
		{
			Debug.Log("Steam: Already in this lobby.");
			return;
		}
		PhotonNetwork.Disconnect();
		LeaveLobby();
		GameManager.instance.SetGameMode(0);
		GameManager.instance.SetConnectRandom(_connectRandom: false);
		if (SemiFunc.RunIsTutorial())
		{
			TutorialDirector.instance.EndTutorial();
		}
		GameDirector.instance.OutroStart();
		Debug.Log("Steam: Game lobby join requested: " + _lobby.Id.ToString());
		await SteamMatchmaking.JoinLobbyAsync(_lobby.Id);
		StatsManager.instance.saveFileCurrent = "";
		if (!SemiFunc.IsMainMenu())
		{
			RunManager.instance.lobbyJoin = true;
			RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.LobbyMenu);
		}
		else
		{
			joinLobby = true;
			MenuManager.instance?.PageCloseAll();
			MenuManager.instance?.PageOpen(MenuPageIndex.Main);
		}
	}

	private void OnLobbyEntered(Lobby _lobby)
	{
		currentLobby.Leave();
		currentLobby = _lobby;
		Debug.Log("Steam: Lobby entered with ID: " + _lobby.Id.ToString());
		Debug.Log("Steam: Region: " + _lobby.GetData("Region"));
		SetPlayedWith();
		UpdateSteamFriendGrouping(_lobby);
		if ((bool)DiscordManager.instance)
		{
			DiscordManager.instance.activityParty = new ActivityParty();
			DiscordManager.instance.activityParty.SetId(_lobby.Owner.Id.ToString());
			DiscordManager.instance.activityParty.SetCurrentSize(_lobby.MemberCount);
			DiscordManager.instance.activityParty.SetMaxSize(_lobby.MaxMembers);
			DiscordManager.instance.activityParty.SetPrivacy(ActivityPartyPrivacy.Private);
			DiscordManager.instance.RefreshDiscordRichPresence();
		}
	}

	private void OnLobbyCreated(Result _result, Lobby _lobby)
	{
		if (_result == Result.OK)
		{
			Debug.Log("Steam: Lobby created with ID: " + _lobby.Id.ToString());
			return;
		}
		Debug.LogError("Steam: Failed to create lobby. Error: " + _result);
		NetworkManager.instance.LeavePhotonRoom();
	}

	public async void HostLobby(bool _open)
	{
		Debug.Log("Steam: Hosting lobby...");
		Lobby? lobby = await SteamMatchmaking.CreateLobbyAsync(6);
		if (!lobby.HasValue)
		{
			Debug.LogError("Lobby created but not correctly instantiated.");
		}
		else if (_open)
		{
			lobby.Value.SetPublic();
			lobby.Value.SetJoinable(b: false);
			privateLobby = false;
		}
		else
		{
			lobby.Value.SetPrivate();
			lobby.Value.SetFriendsOnly();
			lobby.Value.SetJoinable(b: false);
			privateLobby = true;
		}
	}

	public void LeaveLobby()
	{
		if (currentLobby.IsOwnedBy(SteamClient.SteamId))
		{
			Debug.Log("Steam: Leaving lobby... and ruining it for others.");
			currentLobby.SetData("BuildName", "");
		}
		else
		{
			Debug.Log("Steam: Leaving lobby...");
		}
		CancelSteamAuthTicket();
		currentLobby.Leave();
		currentLobby = noLobby;
		UpdateSteamFriendGrouping(noLobby);
		if ((bool)DiscordManager.instance)
		{
			DiscordManager.instance.activityParty = null;
		}
	}

	public void UnlockLobby(bool _open)
	{
		Debug.Log("Steam: Unlocking lobby...");
		if (_open)
		{
			currentLobby.SetPublic();
			privateLobby = false;
		}
		else
		{
			currentLobby.SetPrivate();
			currentLobby.SetFriendsOnly();
			privateLobby = true;
		}
		currentLobby.SetJoinable(b: true);
	}

	public void LockLobby()
	{
		Debug.Log("Steam: Locking lobby...");
		currentLobby.SetPrivate();
		currentLobby.SetFriendsOnly();
		currentLobby.SetJoinable(b: false);
		privateLobby = true;
	}

	public void JoinLobby(SteamId _lobbyID)
	{
		Debug.Log("Steam: Joining lobby...");
		SteamMatchmaking.JoinLobbyAsync(_lobbyID);
	}

	public void SetLobbyData(string _roomName)
	{
		Debug.Log("Steam: Setting lobby data...");
		currentLobby.SetData("Region", PhotonNetwork.CloudRegion);
		currentLobby.SetData("BuildName", BuildManager.instance.version.title);
		currentLobby.SetData("RoomName", _roomName);
		if (privateLobby && !string.IsNullOrEmpty(DataDirector.instance.networkPassword))
		{
			currentLobby.SetData("HasPassword", "1");
		}
		else
		{
			currentLobby.SetData("HasPassword", "0");
		}
	}

	public void SendSteamAuthTicket()
	{
		Debug.Log("Sending Steam Auth Ticket...");
		string value = GetSteamAuthTicket(out steamAuthTicket);
		PhotonNetwork.AuthValues = new AuthenticationValues();
		PhotonNetwork.AuthValues.UserId = SteamClient.SteamId.ToString();
		PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Steam;
		PhotonNetwork.AuthValues.AddAuthParameter("ticket", value);
	}

	private string GetSteamAuthTicket(out AuthTicket ticket)
	{
		Debug.Log("Getting Steam Auth Ticket...");
		ticket = SteamUser.GetAuthSessionTicket(SteamClient.SteamId);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < ticket.Data.Length; i++)
		{
			stringBuilder.AppendFormat("{0:x2}", ticket.Data[i]);
		}
		return stringBuilder.ToString();
	}

	public void CancelSteamAuthTicket()
	{
		Debug.Log("Cancelling Steam Auth Ticket...");
		if (steamAuthTicket != null)
		{
			steamAuthTicket.Cancel();
		}
	}

	public void OpenSteamOverlayToJoin()
	{
		SteamFriends.OpenOverlay("friends");
	}

	public void OpenSteamOverlayToInvite()
	{
		if (!SteamUtils.IsOverlayEnabled)
		{
			SteamFriends.OpenOverlay("friends");
		}
		else
		{
			SteamFriends.OpenGameInviteOverlay(currentLobby.Id.Value);
		}
	}

	private void OnGameOverlayActivated(bool obj)
	{
		InputManager.instance.ResetInput();
	}

	public void OpenProfile(PlayerAvatar _playerAvatar)
	{
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (item == _playerAvatar)
			{
				SteamId id = default(SteamId);
				if (GameManager.Multiplayer() && GameManager.instance.localTest)
				{
					id.Value = ulong.Parse(item.steamID.Substring(0, item.steamID.Length - 1));
				}
				else
				{
					id.Value = ulong.Parse(item.steamID);
				}
				SteamFriends.OpenUserOverlay(id, "steamid");
			}
		}
	}

	public void SetPlayedWith()
	{
		foreach (Friend member in currentLobby.Members)
		{
			string text = member.Name;
			SteamId id = member.Id;
			SemiLogger.LogAxel("i played with: " + text + " - " + id.ToString());
			SteamFriends.SetPlayedWith(member.Id);
		}
	}

	public void UpdateSteamFriendGrouping(Lobby _lobby)
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.RichPresence) == 2 && (ulong)_lobby.Id != (ulong)noLobby.Id)
		{
			SteamFriends.SetRichPresence("steam_player_group", _lobby.Id.ToString());
			SteamFriends.SetRichPresence("steam_player_group_size", _lobby.MemberCount.ToString());
		}
		else
		{
			SteamFriends.SetRichPresence("steam_player_group", null);
			SteamFriends.SetRichPresence("steam_player_group_size", null);
		}
	}

	public void StartAutoJoiningLobby()
	{
		Debug.Log("Auto-Connecting to lobby: " + lobbyIdToAutoJoin);
		JoinSteamLobby(new Lobby(lobbyIdToAutoJoin), SteamClient.SteamId);
		lobbyIdToAutoJoin = 0uL;
	}
}
