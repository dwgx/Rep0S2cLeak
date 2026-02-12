using Photon.Pun;
using UnityEngine;

public class PlayerAvatarCollision : MonoBehaviourPunCallbacks, IPunObservable
{
	public PlayerAvatar PlayerAvatar;

	private PlayerController PlayerController;

	public Transform CollisionTransform;

	public CapsuleCollider Collider;

	private Vector3 Scale;

	internal Vector3 deathHeadPosition;

	private void Start()
	{
		PlayerController = PlayerController.instance;
	}

	private void Update()
	{
		if (PlayerAvatar.isLocal)
		{
			Scale = PlayerController.PlayerCollision.transform.localScale;
			Collider.enabled = false;
		}
		CollisionTransform.localScale = Scale;
		deathHeadPosition = CollisionTransform.position + Vector3.up * (Collider.height * CollisionTransform.localScale.y - 0.18f);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterAndOwnerOnlyRPC(info, PlayerAvatar.photonView))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(Scale);
			}
			else
			{
				Scale = (Vector3)stream.ReceiveNext();
			}
		}
	}

	public void SetCrouch()
	{
		Scale = PlayerCollision.instance.CrouchCollision.localScale;
		CollisionTransform.localScale = Scale;
	}
}
