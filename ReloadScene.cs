using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadScene : MonoBehaviour, IPunObservable
{
	private PhotonView photonview;

	public int PlayersReady;

	private List<Player> PlayersReadyList = new List<Player>();

	private bool Restarting;

	private float minTime = 0.1f;

	[PunRPC]
	private void PlayerReady(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!PlayersReadyList.Contains(_info.Sender))
		{
			PlayersReadyList.Add(_info.Sender);
			PlayersReady++;
		}
	}

	private void Awake()
	{
		photonview = GetComponent<PhotonView>();
	}

	private void Start()
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonview.RPC("PlayerReady", RpcTarget.All);
		}
	}

	private void Update()
	{
		if (minTime > 0f)
		{
			minTime -= Time.deltaTime;
		}
		else if (!Restarting)
		{
			if (!SemiFunc.IsMultiplayer())
			{
				SceneManager.LoadSceneAsync("Main");
				Restarting = true;
			}
			else if (PhotonNetwork.IsMasterClient && PlayersReady >= PhotonNetwork.CurrentRoom.PlayerCount && (PhotonNetwork.LevelLoadingProgress == 0f || PhotonNetwork.LevelLoadingProgress == 1f))
			{
				PhotonNetwork.AutomaticallySyncScene = true;
				PhotonNetwork.LoadLevel("Main");
				Restarting = true;
			}
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(PlayersReady);
			}
			else
			{
				PlayersReady = (int)stream.ReceiveNext();
			}
		}
	}
}
