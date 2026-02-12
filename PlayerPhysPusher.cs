using Photon.Pun;
using UnityEngine;

public class PlayerPhysPusher : MonoBehaviour
{
	private PhotonView PhotonView;

	private Rigidbody Rigidbody;

	public PlayerAvatar Player;

	[Space]
	public Transform ColliderTarget;

	public Transform Collider;

	internal bool Reset;

	private Vector3 PreviousVelocity;

	private void Awake()
	{
		PhotonView = GetComponent<PhotonView>();
		Rigidbody = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		if (GameManager.instance.gameMode == 0 || !PhotonNetwork.IsMasterClient || PhotonView.IsMine)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void FixedUpdate()
	{
		if (Player.isDisabled || Player.isTumbling || Player.rbVelocity.magnitude < 0.1f)
		{
			Collider.gameObject.SetActive(value: false);
		}
		else
		{
			Collider.gameObject.SetActive(value: true);
		}
		float num = Vector3.Distance(base.transform.position, ColliderTarget.position);
		if ((Reset && num > 0.5f) || num > 1f || Player.rbVelocity.magnitude < 0.1f || Vector3.Dot(Player.rbVelocity, PreviousVelocity) < 0.25f)
		{
			Rigidbody.MovePosition(ColliderTarget.position);
			Reset = false;
		}
		Rigidbody.MoveRotation(ColliderTarget.rotation);
		Vector3 vector = base.transform.InverseTransformDirection(Rigidbody.velocity);
		Rigidbody.AddRelativeForce(Player.rbVelocity - vector, ForceMode.Impulse);
		PreviousVelocity = Player.rbVelocity;
	}

	private void Update()
	{
		Collider.localScale = ColliderTarget.localScale;
	}
}
