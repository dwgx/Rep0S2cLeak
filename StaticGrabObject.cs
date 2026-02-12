using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class StaticGrabObject : MonoBehaviour
{
	internal PhotonView photonView;

	private bool isMaster;

	public Transform colliderTransform;

	[HideInInspector]
	public Vector3 velocity;

	[HideInInspector]
	public bool grabbed;

	public List<PhysGrabber> playerGrabbing = new List<PhysGrabber>();

	[HideInInspector]
	public bool dead;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		if (GameManager.instance.gameMode == 1 && PhotonNetwork.IsMasterClient)
		{
			isMaster = true;
			photonView.TransferOwnership(PhotonNetwork.MasterClient);
		}
	}

	private void Update()
	{
		if (grabbed)
		{
			for (int i = 0; i < playerGrabbing.Count; i++)
			{
				if (!playerGrabbing[i])
				{
					playerGrabbing.RemoveAt(i);
				}
			}
		}
		if (GameManager.instance.gameMode != 0 && !isMaster)
		{
			return;
		}
		velocity = Vector3.zero;
		foreach (PhysGrabber item in playerGrabbing)
		{
			Vector3 vector = (item.physGrabPointPullerPosition - item.physGrabPoint.position) * 5f;
			velocity += vector * Time.deltaTime;
		}
		if (dead && playerGrabbing.Count == 0)
		{
			DestroyPhysGrabObject();
		}
	}

	private void OnDisable()
	{
		playerGrabbing.Clear();
		grabbed = false;
	}

	public void GrabLink(int playerPhotonID, Vector3 point)
	{
		photonView.RPC("GrabLinkRPC", RpcTarget.All, playerPhotonID, point);
	}

	[PunRPC]
	private void GrabLinkRPC(int playerPhotonID, Vector3 point)
	{
		PhysGrabber component = PhotonView.Find(playerPhotonID).GetComponent<PhysGrabber>();
		component.physGrabPoint.position = point;
		component.localGrabPosition = colliderTransform.InverseTransformPoint(point);
		component.grabbedObjectTransform = colliderTransform;
		component.grabbed = true;
		if (component.photonView.IsMine)
		{
			Vector3 localPosition = component.physGrabPoint.localPosition;
			photonView.RPC("GrabPointSyncRPC", RpcTarget.MasterClient, playerPhotonID, localPosition);
		}
	}

	[PunRPC]
	private void GrabPointSyncRPC(int playerPhotonID, Vector3 localPointInBox)
	{
		PhotonView.Find(playerPhotonID).GetComponent<PhysGrabber>().physGrabPoint.localPosition = localPointInBox;
	}

	public void GrabStarted(PhysGrabber player)
	{
		if (grabbed)
		{
			return;
		}
		grabbed = true;
		if (GameManager.instance.gameMode == 0)
		{
			if (!playerGrabbing.Contains(player))
			{
				playerGrabbing.Add(player);
			}
		}
		else
		{
			photonView.RPC("GrabStartedRPC", RpcTarget.MasterClient, player.photonView.ViewID);
		}
	}

	[PunRPC]
	private void GrabStartedRPC(int playerPhotonID)
	{
		PhysGrabber component = PhotonView.Find(playerPhotonID).GetComponent<PhysGrabber>();
		if (!playerGrabbing.Contains(component))
		{
			playerGrabbing.Add(component);
		}
	}

	public void GrabEnded(PhysGrabber player)
	{
		if (!grabbed)
		{
			return;
		}
		grabbed = false;
		if (GameManager.instance.gameMode == 0)
		{
			if (playerGrabbing.Contains(player))
			{
				playerGrabbing.Remove(player);
			}
		}
		else
		{
			photonView.RPC("GrabEndedRPC", RpcTarget.MasterClient, player.photonView.ViewID);
		}
	}

	[PunRPC]
	private void GrabEndedRPC(int playerPhotonID)
	{
		PhysGrabber component = PhotonView.Find(playerPhotonID).GetComponent<PhysGrabber>();
		component.grabbed = false;
		if (playerGrabbing.Contains(component))
		{
			playerGrabbing.Remove(component);
		}
	}

	private void DestroyPhysGrabObject()
	{
		if (GameManager.instance.gameMode == 0)
		{
			DestroyPhysObjectFailsafe();
			Object.Destroy(base.gameObject);
		}
		else
		{
			photonView.RPC("DestroyPhysGrabObjectRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void DestroyPhysGrabObjectRPC()
	{
		DestroyPhysObjectFailsafe();
		Object.Destroy(base.gameObject);
	}

	private void DestroyPhysObjectFailsafe()
	{
		foreach (Transform item in base.transform)
		{
			if (item.CompareTag("Phys Grab Controller"))
			{
				item.SetParent(null);
			}
		}
	}
}
