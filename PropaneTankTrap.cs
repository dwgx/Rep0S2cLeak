using System;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class PropaneTankTrap : Trap
{
	public UnityEvent tankTimer;

	private PhysGrabObject physgrabobject;

	private ParticleScriptExplosion particleScriptExplosion;

	[Space]
	public GameObject Tank;

	public Transform Center;

	[Space]
	[Header("Sounds")]
	public Sound Pop;

	public Sound FlyLoop;

	public Sound flyStart;

	public Sound flyEnd;

	[Space]
	private Quaternion initialTankRotation;

	private Rigidbody rb;

	private bool LoopPlaying;

	private Vector3 randomTorque;

	private int timeToTwist;

	public HurtCollider hurtCollider;

	public ParticleSystem smokeParticleSystem;

	public ParticleSystem fireParticleSystem;

	public GameObject fireLight;

	protected override void Start()
	{
		base.Start();
		initialTankRotation = Tank.transform.localRotation;
		rb = GetComponent<Rigidbody>();
		physgrabobject = GetComponent<PhysGrabObject>();
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		hurtCollider.gameObject.SetActive(value: false);
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
			hurtCollider.gameObject.SetActive(value: true);
			float num = 40f;
			float num2 = 1f * Mathf.Sin(Time.time * num);
			float z = 1f * Mathf.Sin(Time.time * num + MathF.PI / 2f);
			Tank.transform.localRotation = initialTankRotation * Quaternion.Euler(num2, 0f, z);
			Tank.transform.localPosition = new Vector3(Tank.transform.localPosition.x, Tank.transform.localPosition.y - num2 * 0.005f * Time.deltaTime, Tank.transform.localPosition.z);
		}
	}

	private void FixedUpdate()
	{
		if (!trapActive || !isLocal)
		{
			return;
		}
		rb.AddForce(-base.transform.forward * 0.3f * 40f * Time.fixedDeltaTime * 50f, ForceMode.Force);
		rb.AddForce(Vector3.up * 0.1f * 10f * Time.fixedDeltaTime * 50f, ForceMode.Force);
		rb.AddTorque(-base.transform.right * 0.1f * 10f * Time.fixedDeltaTime * 50f, ForceMode.Force);
		if (timeToTwist > 200)
		{
			randomTorque = UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(0f, 0.5f);
			timeToTwist = 0;
			if (rb.velocity.magnitude < 0.5f && !physgrabobject.grabbed)
			{
				rb.AddForce(base.transform.forward * 5f, ForceMode.Impulse);
				rb.AddTorque(randomTorque * 20f, ForceMode.Impulse);
			}
		}
	}

	public void TrapStop()
	{
		trapActive = false;
		flyEnd.Play(physgrabobject.centerPoint);
		LoopPlaying = false;
		hurtCollider.gameObject.SetActive(value: false);
		DeparentAndDestroy(smokeParticleSystem);
		DeparentAndDestroy(fireParticleSystem);
		fireLight.SetActive(value: false);
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
			tankTimer.Invoke();
			fireParticleSystem.Play(withChildren: false);
			fireLight.SetActive(value: true);
			flyStart.Play(physgrabobject.centerPoint);
			GrabRelease();
			trapActive = true;
			trapTriggered = true;
		}
	}

	public void Explode()
	{
		particleScriptExplosion.Spawn(Center.position, 0.8f, 50, 100);
		DeparentAndDestroy(smokeParticleSystem);
		DeparentAndDestroy(fireParticleSystem);
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
