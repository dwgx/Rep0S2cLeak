using Photon.Pun;
using UnityEngine;

public class PlayerLocalCamera : MonoBehaviour, IPunObservable
{
	private PhotonView photonView;

	private Vector3 clientPosition;

	private Quaternion clientRotation;

	private bool teleported;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
	}

	private void Update()
	{
		if (SemiFunc.IsMultiplayer() && !photonView.IsMine)
		{
			if (Vector3.Distance(base.transform.position, clientPosition) > 10f || teleported)
			{
				base.transform.position = clientPosition;
				base.transform.rotation = clientRotation;
			}
			base.transform.position = Vector3.Lerp(base.transform.position, clientPosition, 20f * Time.deltaTime);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, clientRotation, 20f * Time.deltaTime);
			teleported = false;
		}
		else
		{
			base.transform.position = CameraNoise.Instance.transform.position;
			base.transform.rotation = CameraNoise.Instance.transform.rotation;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!SemiFunc.MasterAndOwnerOnlyRPC(info, photonView))
		{
			return;
		}
		if (stream.IsWriting)
		{
			stream.SendNext(CameraAim.Instance.transform.position);
			stream.SendNext(CameraAim.Instance.transform.rotation);
			stream.SendNext(teleported);
			return;
		}
		Vector3 vector = (Vector3)stream.ReceiveNext();
		if (!float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z))
		{
			vector = Vector3.ClampMagnitude(vector, 1000f);
			clientPosition = vector;
		}
		clientRotation = (Quaternion)stream.ReceiveNext();
		teleported = (bool)stream.ReceiveNext();
	}

	public void Teleported()
	{
		teleported = true;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(1f, 0f, 0.79f, 0.5f);
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawSphere(Vector3.zero, 0.1f);
		Gizmos.DrawCube(new Vector3(0f, 0f, 0.15f), new Vector3(0.1f, 0.1f, 0.3f));
	}
}
