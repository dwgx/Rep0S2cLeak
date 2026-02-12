using System;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class BottleTrap : Trap
{
	public UnityEvent bottleTimer;

	private PhysGrabObject physgrabobject;

	[Space]
	[Header("Bottle Components")]
	public GameObject Bottle;

	public GameObject Cork;

	[Space]
	[Header("Sounds")]
	public Sound Pop;

	public Sound FlyLoop;

	[Space]
	private Quaternion initialBottleRotation;

	private Rigidbody rb;

	private bool LoopPlaying;

	private Vector3 randomTorque;

	private int timeToTwist;

	public ParticleSystem bottleParticleSystem;

	public ParticleSystem corkParticleSystem;

	protected override void Start()
	{
		base.Start();
		initialBottleRotation = Bottle.transform.localRotation;
		rb = GetComponent<Rigidbody>();
		physgrabobject = GetComponent<PhysGrabObject>();
	}

	protected override void Update()
	{
		base.Update();
		FlyLoop.PlayLoop(LoopPlaying, 0.8f, 0.8f);
		if (trapStart)
		{
			TrapActivate();
		}
		if (trapActive)
		{
			enemyInvestigate = true;
			enemyInvestigateRange = 15f;
			LoopPlaying = true;
			float num = 40f;
			float num2 = 1f * Mathf.Sin(Time.time * num);
			float z = 1f * Mathf.Sin(Time.time * num + MathF.PI / 2f);
			Bottle.transform.localRotation = initialBottleRotation * Quaternion.Euler(num2, 0f, z);
			Bottle.transform.localPosition = new Vector3(Bottle.transform.localPosition.x, Bottle.transform.localPosition.y - num2 * 0.005f * Time.deltaTime, Bottle.transform.localPosition.z);
		}
	}

	private void FixedUpdate()
	{
		if (!trapActive || !isLocal)
		{
			return;
		}
		rb.AddForce(-base.transform.up * 0.45f * 40f * Time.fixedDeltaTime * 50f, ForceMode.Force);
		rb.AddForce(Vector3.up * 0.15f * 10f * Time.fixedDeltaTime * 50f, ForceMode.Force);
		rb.AddTorque(randomTorque * 30f * Time.fixedDeltaTime * 50f, ForceMode.Force);
		if (timeToTwist > 200)
		{
			randomTorque = UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(0f, 0.5f);
			timeToTwist = 0;
			if (rb.velocity.magnitude < 0.5f && !physgrabobject.grabbed)
			{
				rb.AddForce(base.transform.up * 5f, ForceMode.Impulse);
				rb.AddTorque(randomTorque * 20f, ForceMode.Impulse);
			}
		}
	}

	public void TrapStop()
	{
		trapActive = false;
		LoopPlaying = false;
		DeparentAndDestroy(bottleParticleSystem);
	}

	private void DeparentAndDestroy(ParticleSystem particleSystem)
	{
		if (particleSystem != null && particleSystem.isPlaying)
		{
			particleSystem.gameObject.transform.parent = null;
			ParticleSystem.MainModule main = particleSystem.main;
			main.stopAction = ParticleSystemStopAction.Destroy;
			particleSystem.Stop(withChildren: false);
			particleSystem = null;
		}
	}

	public void IncrementTimeToTwist()
	{
		timeToTwist++;
	}

	public void TrapActivate()
	{
		if (!trapTriggered)
		{
			Pop.Play(physgrabobject.centerPoint);
			bottleTimer.Invoke();
			Cork.SetActive(value: false);
			corkParticleSystem.Play(withChildren: false);
			bottleParticleSystem.Play(withChildren: false);
			GrabRelease();
			trapActive = true;
			trapTriggered = true;
		}
	}

	private void OnDestroy()
	{
		if ((bool)bottleParticleSystem)
		{
			bottleParticleSystem.transform.parent = null;
			bottleParticleSystem.Stop(withChildren: true);
			ParticleSystem.MainModule main = bottleParticleSystem.main;
			main.stopAction = ParticleSystemStopAction.Destroy;
		}
	}

	public void GrabRelease()
	{
		bool flag = false;
		foreach (PhysGrabber item in physGrabObject.playerGrabbing.ToList())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				item.ReleaseObject(photonView.ViewID);
			}
			else
			{
				item.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, 0.1f, photonView.ViewID);
			}
			flag = true;
		}
		if (flag)
		{
			if (GameManager.instance.gameMode == 0)
			{
				GrabReleaseRPC();
			}
			else
			{
				photonView.RPC("GrabReleaseRPC", RpcTarget.All);
			}
		}
	}

	[PunRPC]
	private void GrabReleaseRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
			physGrabObject.grabDisableTimer = 1f;
		}
	}
}
