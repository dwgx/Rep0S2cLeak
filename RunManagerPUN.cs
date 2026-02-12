using System.Collections;
using Photon.Pun;
using Steamworks;
using UnityEngine;

public class RunManagerPUN : MonoBehaviour
{
	internal PhotonView photonView;

	private RunManager runManager;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		runManager = RunManager.instance;
		runManager.runManagerPUN = this;
		runManager.levelIsShop = runManager.levelCurrent == runManager.levelShop;
		runManager.restarting = false;
		runManager.restartingDone = false;
		if (PhotonNetwork.IsMasterClient && GameManager.instance.connectRandom)
		{
			if (!SteamManager.instance.currentLobby.Id.IsValid)
			{
				StartCoroutine(HostSteamLobby());
			}
			else if (SemiFunc.RunIsLobbyMenu())
			{
				SendJoinSteamLobby();
			}
		}
	}

	private void SendJoinSteamLobby()
	{
		SteamManager.instance.UnlockLobby(_open: true);
		photonView.RPC("JoinSteamLobbyRPC", RpcTarget.OthersBuffered, SteamManager.instance.currentLobby.Id.ToString());
	}

	private IEnumerator HostSteamLobby()
	{
		SteamManager.instance.HostLobby(_open: true);
		while (!SteamManager.instance.currentLobby.Id.IsValid)
		{
			yield return null;
		}
		Debug.Log("Created open lobby with ID: " + SteamManager.instance.currentLobby.Id.ToString());
		SteamManager.instance.SetLobbyData(PhotonNetwork.CurrentRoom.Name);
		SendJoinSteamLobby();
	}

	[PunRPC]
	private void JoinSteamLobbyRPC(string _steamID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			Debug.Log("I got the lobby id: " + _steamID);
			SteamId lobbyID = new SteamId
			{
				Value = ulong.Parse(_steamID)
			};
			SteamManager.instance.JoinLobby(lobbyID);
		}
	}

	[PunRPC]
	private void UpdateLevelRPC(string _levelName, int _levelsCompleted, bool _gameOver, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			runManager.UpdateLevel(_levelName, _levelsCompleted, _gameOver);
		}
	}
}
