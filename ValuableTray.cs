using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ValuableTray : Trap
{
	public List<Transform> trayThings = new List<Transform>();

	public List<ParticleSystem> trayParticles = new List<ParticleSystem>();

	public List<Collider> trayColliders = new List<Collider>();

	private float dropThreshold = 1.5f;

	private float dropTimer;

	private int dropped = -1;

	private int previousDropped;

	protected override void Start()
	{
		base.Start();
		dropTimer = dropThreshold;
		previousDropped = dropped;
	}

	protected override void Update()
	{
		if (dropped >= trayThings.Count)
		{
			return;
		}
		if (dropped != previousDropped && dropped < trayThings.Count)
		{
			trayThings[dropped].gameObject.SetActive(value: false);
			trayParticles[dropped].Play();
			physGrabObject.impactDetector.BreakMedium(physGrabObject.centerPoint, _forceBreak: true);
			previousDropped = dropped;
			if (dropped == 0)
			{
				trayColliders[0].gameObject.SetActive(value: false);
			}
			else if (dropped == 1)
			{
				trayColliders[1].gameObject.SetActive(value: false);
			}
			else if (dropped == 3)
			{
				trayColliders[2].gameObject.SetActive(value: false);
			}
			else if (dropped == 5)
			{
				trayColliders[3].gameObject.SetActive(value: false);
			}
			else if (dropped == 7)
			{
				trayColliders[4].gameObject.SetActive(value: false);
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			float num = Vector3.Dot(base.transform.up, Vector3.up);
			dropThreshold = GetDropThreshold(num);
			dropTimer += Time.deltaTime;
			if (num < 0.85f && dropTimer >= dropThreshold)
			{
				DropThing();
			}
		}
	}

	private float GetDropThreshold(float tilt)
	{
		if (tilt > 0.85f)
		{
			return dropThreshold;
		}
		if (tilt > 0.8f)
		{
			return 1f;
		}
		if (tilt > 0.5f)
		{
			return 0.5f;
		}
		if (tilt > 0.3f)
		{
			return 0.4f;
		}
		return 0.1f;
	}

	private void DropThing()
	{
		if (dropped < trayThings.Count)
		{
			dropTimer = 0f;
			UpdateDropAmount(dropped + 1);
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				physGrabObject.OverrideMass(4f - (float)dropped * 0.25f);
				physGrabObject.massOriginal = 4f - (float)dropped * 0.25f;
			}
		}
	}

	[PunRPC]
	public void UpdateDropAmountRPC(int _index, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			dropped = _index;
		}
	}

	private void UpdateDropAmount(int _index)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && dropped < trayThings.Count && dropped != _index)
		{
			if (!SemiFunc.IsMultiplayer())
			{
				UpdateDropAmountRPC(_index);
				return;
			}
			photonView.RPC("UpdateDropAmountRPC", RpcTarget.All, _index);
		}
	}
}
