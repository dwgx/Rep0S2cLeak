using Photon.Pun;
using UnityEngine;

public class ItemGunTranq : MonoBehaviour
{
	private PhotonView photonView;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
	}

	public void MakeHitPlayersHigh()
	{
		if (SemiFunc.IsMultiplayer())
		{
			PlayerAvatar instance = PlayerAvatar.instance;
			instance.OverridePupilSize(3f, 4, 1f, 1f, 5f, 0.5f, 1.8f);
			photonView.RPC("SlowDownVoiceRPC", RpcTarget.Others, instance.photonView.ViewID);
		}
	}

	[PunRPC]
	public void SlowDownVoiceRPC(int _photonID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(_photonID);
		if (SemiFunc.OwnerOnlyRPC(_info, playerAvatar.photonView))
		{
			playerAvatar.voiceChat.OverridePitch(0.65f, 1f, 1f, 3f, 0.1f, 20f);
		}
	}
}
