using Photon.Pun;
using UnityEngine;

public class PlayerVisionTarget : MonoBehaviourPunCallbacks, IPunObservable
{
	private PhotonView PhotonView;

	private PlayerAvatar PlayerAvatar;

	private PlayerController PlayerController;

	public Transform VisionTransform;

	private Camera MainCamera;

	[Space]
	public float StandPosition;

	public float CrouchPosition;

	public float CrawlPosition;

	[Space]
	public float HeadStandPosition;

	public float HeadCrouchPosition;

	public float HeadCrawlPosition;

	private float TargetPosition;

	private Quaternion TargetRotation;

	internal float CurrentPosition;

	internal Quaternion CurrentRotation;

	public float LerpSpeed;

	[Space]
	public bool DebugMeshActive = true;

	public Mesh DebugMesh;

	private void Awake()
	{
		PhotonView = GetComponent<PhotonView>();
	}

	private void Start()
	{
		PlayerAvatar = GetComponent<PlayerAvatar>();
		CurrentPosition = StandPosition;
		PlayerController = PlayerController.instance;
		MainCamera = Camera.main;
	}

	private void Update()
	{
		if (PlayerAvatar.isLocal)
		{
			if (PlayerController.Crouching)
			{
				if (PlayerController.Crawling)
				{
					TargetPosition = CrawlPosition;
				}
				else
				{
					TargetPosition = CrouchPosition;
				}
			}
			else
			{
				TargetPosition = StandPosition;
			}
			TargetRotation = MainCamera.transform.rotation;
		}
		CurrentPosition = Mathf.Lerp(CurrentPosition, TargetPosition, Time.deltaTime * LerpSpeed);
		CurrentRotation = Quaternion.Slerp(CurrentRotation, TargetRotation, Time.deltaTime * 20f);
		VisionTransform.localPosition = new Vector3(0f, CurrentPosition, 0f);
		VisionTransform.rotation = CurrentRotation;
	}

	private void OnDrawGizmos()
	{
		if (DebugMeshActive)
		{
			Gizmos.color = new Color(0f, 1f, 0.13f, 0.75f);
			Gizmos.matrix = VisionTransform.localToWorldMatrix;
			Gizmos.DrawMesh(DebugMesh, 0, Vector3.zero, Quaternion.identity, Vector3.one * 0.15f);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterAndOwnerOnlyRPC(info, base.photonView))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(TargetPosition);
				stream.SendNext(TargetRotation);
			}
			else
			{
				TargetPosition = (float)stream.ReceiveNext();
				TargetRotation = (Quaternion)stream.ReceiveNext();
			}
		}
	}
}
