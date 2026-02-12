using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class PhysGrabObjectGrabArea : MonoBehaviour
{
	[Serializable]
	public class GrabArea
	{
		public Transform grabAreaTransform;

		[Space(20f)]
		public UnityEvent grabAreaEventOnStart = new UnityEvent();

		public UnityEvent grabAreaEventOnRelease = new UnityEvent();

		public UnityEvent grabAreaEventOnHolding = new UnityEvent();

		[HideInInspector]
		public bool grabAreaActive;

		[HideInInspector]
		public List<PhysGrabber> listOfGrabbers = new List<PhysGrabber>();

		[HideInInspector]
		public List<Collider> grabAreaColliders = new List<Collider>();
	}

	private PhysGrabObject physGrabObject;

	private StaticGrabObject staticGrabObject;

	private PhotonView photonView;

	[HideInInspector]
	public List<PhysGrabber> listOfAllGrabbers = new List<PhysGrabber>();

	public List<GrabArea> grabAreas = new List<GrabArea>();

	private void Start()
	{
		physGrabObject = GetComponentInParent<PhysGrabObject>();
		staticGrabObject = GetComponentInParent<StaticGrabObject>();
		photonView = GetComponentInParent<PhotonView>();
		foreach (GrabArea grabArea in grabAreas)
		{
			if ((bool)grabArea.grabAreaTransform)
			{
				if (grabArea.grabAreaTransform.childCount == 0)
				{
					Collider component = grabArea.grabAreaTransform.GetComponent<Collider>();
					if (component != null)
					{
						grabArea.grabAreaColliders.Add(component);
					}
					else
					{
						Debug.LogWarning("Grab area '" + grabArea.grabAreaTransform.name + "' is missing a Collider component.");
					}
				}
				else
				{
					Collider[] componentsInChildren = grabArea.grabAreaTransform.GetComponentsInChildren<Collider>();
					if (componentsInChildren.Length != 0)
					{
						grabArea.grabAreaColliders.AddRange(componentsInChildren);
					}
					else
					{
						Debug.LogWarning("Grab area '" + grabArea.grabAreaTransform.name + "' has children but no colliders.");
					}
				}
			}
			else
			{
				Debug.LogWarning("Grab area in '" + base.gameObject.name + "' has a missing Transform. Please assign it.");
			}
		}
	}

	public PlayerAvatar GetLatestGrabber()
	{
		if (listOfAllGrabbers.Count > 0)
		{
			return listOfAllGrabbers[listOfAllGrabbers.Count - 1].playerAvatar;
		}
		return null;
	}

	private void Update()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		foreach (GrabArea grabArea in grabAreas)
		{
			for (int num = grabArea.listOfGrabbers.Count - 1; num >= 0; num--)
			{
				PhysGrabber physGrabber = grabArea.listOfGrabbers[num];
				if (!physGrabber || !physGrabber.grabbed || ((bool)physGrabber.grabbedPhysGrabObject && physGrabber.grabbedPhysGrabObject != physGrabObject) || ((bool)physGrabber.grabbedStaticGrabObject && physGrabber.grabbedStaticGrabObject != staticGrabObject))
				{
					UpdateList(add: false, physGrabber);
					listOfAllGrabbers.Remove(physGrabber);
					grabArea.listOfGrabbers.RemoveAt(num);
				}
			}
		}
		foreach (PhysGrabber item in (physGrabObject ? physGrabObject.playerGrabbing : staticGrabObject.playerGrabbing).ToList())
		{
			if (item.initialPressTimer <= 0f)
			{
				continue;
			}
			Vector3 position = item.physGrabPoint.position;
			foreach (GrabArea grabArea2 in grabAreas)
			{
				if (grabArea2.grabAreaColliders.Count == 0)
				{
					continue;
				}
				bool flag = false;
				foreach (Collider grabAreaCollider in grabArea2.grabAreaColliders)
				{
					if (grabAreaCollider.ClosestPoint(position) == position)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				if (!grabArea2.listOfGrabbers.Contains(item))
				{
					grabArea2.listOfGrabbers.Add(item);
					if (!listOfAllGrabbers.Contains(item))
					{
						listOfAllGrabbers.Add(item);
						UpdateList(add: true, item);
					}
					grabArea2.grabAreaEventOnStart?.Invoke();
				}
				else
				{
					grabArea2.grabAreaEventOnHolding?.Invoke();
				}
				grabArea2.grabAreaActive = true;
				break;
			}
		}
		foreach (GrabArea grabArea3 in grabAreas)
		{
			if (grabArea3.listOfGrabbers.Count == 0 && grabArea3.grabAreaActive)
			{
				grabArea3.grabAreaEventOnRelease?.Invoke();
				grabArea3.grabAreaActive = false;
			}
		}
	}

	[PunRPC]
	public void AddToGrabbersList(int grabberId)
	{
		PhysGrabber physGrabber = FindGrabberById(grabberId);
		if (physGrabber != null && !listOfAllGrabbers.Contains(physGrabber))
		{
			listOfAllGrabbers.Add(physGrabber);
		}
	}

	[PunRPC]
	public void RemoveFromGrabbersList(int grabberId)
	{
		PhysGrabber physGrabber = FindGrabberById(grabberId);
		if (physGrabber != null)
		{
			listOfAllGrabbers.Remove(physGrabber);
		}
	}

	private PhysGrabber FindGrabberById(int id)
	{
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			PhysGrabber componentInChildren = item.GetComponentInChildren<PhysGrabber>();
			if (componentInChildren != null && componentInChildren.photonView.ViewID == id)
			{
				return componentInChildren;
			}
		}
		return null;
	}

	private void UpdateList(bool add, PhysGrabber grabber)
	{
		if (SemiFunc.IsMultiplayer() && !(grabber == null))
		{
			int viewID = grabber.photonView.ViewID;
			if (add)
			{
				photonView.RPC("AddToGrabbersList", RpcTarget.Others, viewID);
			}
			else
			{
				photonView.RPC("RemoveFromGrabbersList", RpcTarget.Others, viewID);
			}
		}
	}
}
