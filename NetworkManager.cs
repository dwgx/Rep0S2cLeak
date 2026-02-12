using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks, IPunObservable
{
	public static NetworkManager instance;

	public float gameTime;

	private float syncInterval = 0.5f;

	private float lastSyncTime;

	public GameObject playerAvatarPrefab;

	private List<Player> instantiatedPlayerAvatarsList = new List<Player>();

	private int instantiatedPlayerAvatars;

	private bool LoadingDone;

	internal bool leavePhotonRoom;

	private float loadingScreenTimer;

	private void Start()
	{
		instance = this;
		if (PhotonNetwork.IsMasterClient)
		{
			lastSyncTime = 0f;
		}
		if (GameManager.instance.gameMode != 1)
		{
			return;
		}
		PhotonNetwork.Instantiate(playerAvatarPrefab.name, Vector3.zero, Quaternion.identity, 0);
		PhotonNetwork.SerializationRate = 25;
		PhotonNetwork.SendRate = 25;
		bool flag = true;
		PhotonVoiceView[] array = Object.FindObjectsByType<PhotonVoiceView>(FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			PhotonNetwork.Instantiate("Voice", Vector3.zero, Quaternion.identity, 0);
		}
		base.photonView.RPC("PlayerSpawnedRPC", RpcTarget.All);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		PhotonNetwork.NetworkingClient.EventReceived += OnEventReceivedCustom;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceivedCustom;
	}

	[PunRPC]
	public void PlayerSpawnedRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!instantiatedPlayerAvatarsList.Contains(_info.Sender))
		{
			instantiatedPlayerAvatarsList.Add(_info.Sender);
			instantiatedPlayerAvatars++;
		}
	}

	[PunRPC]
	public void AllPlayerSpawnedRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			LevelGenerator.Instance.AllPlayersReady = true;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(lastSyncTime);
				stream.SendNext(instantiatedPlayerAvatars);
			}
			else
			{
				gameTime = (float)stream.ReceiveNext();
				instantiatedPlayerAvatars = (int)stream.ReceiveNext();
			}
		}
	}

	private void Update()
	{
		if (GameManager.instance.gameMode == 1)
		{
			if (PhotonNetwork.IsMasterClient)
			{
				if (!LoadingDone && instantiatedPlayerAvatars >= PhotonNetwork.CurrentRoom.PlayerCount)
				{
					base.photonView.RPC("AllPlayerSpawnedRPC", RpcTarget.AllBuffered);
					LoadingDone = true;
				}
				gameTime += Time.deltaTime;
				if (Time.time - lastSyncTime > syncInterval)
				{
					lastSyncTime = gameTime;
				}
			}
			else
			{
				gameTime += Time.deltaTime;
			}
		}
		if (GameDirector.instance.currentState == GameDirector.gameState.Load || GameDirector.instance.currentState == GameDirector.gameState.EndWait)
		{
			loadingScreenTimer += Time.deltaTime;
			if (loadingScreenTimer >= 25f)
			{
				LoadingUI.instance.stuckActive = true;
				if (SemiFunc.InputDown(InputKey.Menu))
				{
					TriggerLeavePhotonRoomForced();
					MenuManager.instance.PagePopUpScheduled("Disconnected", Color.red, "Cause: Stuck in loading", "Ok Dang", richText: true);
				}
			}
		}
		else
		{
			if (LoadingUI.instance.stuckActive)
			{
				LoadingUI.instance.stuckActive = false;
			}
			loadingScreenTimer = 0f;
		}
	}

	public void LeavePhotonRoom()
	{
		Debug.Log("Leave Photon");
		PhotonNetwork.Disconnect();
		SteamManager.instance.LeaveLobby();
		GameManager.instance.SetGameMode(0);
		leavePhotonRoom = false;
		if (SemiFunc.RunIsTutorial())
		{
			TutorialDirector.instance.EndTutorial();
		}
		StartCoroutine(RunManager.instance.LeaveToMainMenu());
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		Debug.Log("Player entered room: " + newPlayer.NickName);
		if ((bool)MenuPageLobby.instance)
		{
			MenuPageLobby.instance.JoiningPlayer(newPlayer.NickName);
		}
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		Debug.Log("Player left room: " + otherPlayer.NickName);
	}

	public override void OnMasterClientSwitched(Player _newMasterClient)
	{
		Debug.Log("Master client left...");
		MenuManager.instance.PagePopUpScheduled("Disconnected", Color.red, "Cause: Host disconnected", "Ok Dang", richText: true);
		TriggerLeavePhotonRoomForced();
	}

	public void TriggerLeavePhotonRoomForced()
	{
		GameDirector.instance.currentState = GameDirector.gameState.Main;
		GameDirector.instance.OutroStart();
		leavePhotonRoom = true;
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log($"Disconnected from server for reason {cause}");
		switch (cause)
		{
		default:
			MenuManager.instance.PagePopUpScheduled("Disconnected", Color.red, "<b>Cause:\n</b>" + cause, "Ok Dang", richText: true);
			break;
		case DisconnectCause.DisconnectByDisconnectMessage:
			break;
		case DisconnectCause.DisconnectByServerLogic:
		case DisconnectCause.DisconnectByClientLogic:
			return;
		}
		GameDirector.instance.OutroStart();
		leavePhotonRoom = true;
	}

	private void OnEventReceivedCustom(EventData photonEvent)
	{
		if (photonEvent.Code == 199)
		{
			Debug.Log("You were kicked by the server.");
			MenuManager.instance.PagePopUpScheduled("Kicked", Color.red, "You were kicked by the host.", "Ok Dang", richText: true);
			GameDirector.instance.OutroStart();
			leavePhotonRoom = true;
		}
	}

	public void DestroyAll()
	{
		if (SemiFunc.IsMultiplayer())
		{
			Debug.Log("Destroyed all network objects.");
			PhotonNetwork.DestroyAll();
		}
	}

	public void KickPlayer(PlayerAvatar _playerAvatar)
	{
		if (_playerAvatar.photonView.OwnerActorNr == _playerAvatar.photonView.CreatorActorNr)
		{
			object[] eventContent = new object[1] { _playerAvatar.photonView.OwnerActorNr };
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			};
			PhotonNetwork.RaiseEvent(123, eventContent, raiseEventOptions, SendOptions.SendReliable);
		}
	}

	public void BanPlayer(PlayerAvatar _playerAvatar)
	{
		if (_playerAvatar.photonView.OwnerActorNr == _playerAvatar.photonView.CreatorActorNr)
		{
			object[] eventContent = new object[1] { _playerAvatar.photonView.OwnerActorNr };
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions
			{
				Receivers = ReceiverGroup.All
			};
			PhotonNetwork.RaiseEvent(124, eventContent, raiseEventOptions, SendOptions.SendReliable);
		}
	}

	private void OnApplicationQuit()
	{
	}
}
