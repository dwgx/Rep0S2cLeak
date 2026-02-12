using System;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class IceSawValuable : Trap
{
	public enum States
	{
		Idle,
		Active
	}

	public Sound soundBladeLoop;

	public Sound soundBladeStart;

	public Sound soundBladeEnd;

	[Space]
	public Transform meshTransform;

	public UnityEvent sawTimer;

	public Transform blade;

	public AnimationCurve bladeCurve;

	public float bladeSpeed;

	private float bladeMaxSpeed = 1500f;

	private float bladeLerp;

	private float secondsToStart = 2f;

	private float secondsToStop = 2f;

	public HurtCollider hurtCollider;

	public Collider triggerCollider;

	private float overLapBoxCheckTimer;

	public ParticleSystem sparkParticles;

	internal States currentState;

	private bool stateStart;

	[Space]
	private Quaternion initialTankRotation;

	private Animator animator;

	private PhysGrabObject physgrabobject;

	private Rigidbody rb;

	private bool loopPlaying;

	protected override void Start()
	{
		base.Start();
		physgrabobject = GetComponent<PhysGrabObject>();
		photonView = GetComponent<PhotonView>();
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		initialTankRotation = meshTransform.transform.localRotation;
	}

	private void FixedUpdate()
	{
		switch (currentState)
		{
		case States.Active:
			StateActive();
			break;
		case States.Idle:
			StateIdle();
			break;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (trapStart)
		{
			TrapActivate();
		}
		soundBladeLoop.PlayLoop(loopPlaying, 5f, 5f);
		blade.Rotate(-Vector3.right * bladeSpeed * Time.deltaTime);
		bladeSpeed = Mathf.Lerp(0f, bladeMaxSpeed, bladeCurve.Evaluate(bladeLerp));
		float num = 0.3f * bladeCurve.Evaluate(bladeLerp);
		float num2 = 60f * bladeCurve.Evaluate(bladeLerp);
		float num3 = num * Mathf.Sin(Time.time * num2);
		float z = num * Mathf.Sin(Time.time * num2 + MathF.PI / 2f);
		meshTransform.transform.localRotation = initialTankRotation * Quaternion.Euler(num3, 0f, z);
		meshTransform.transform.localPosition = new Vector3(meshTransform.transform.localPosition.x, meshTransform.transform.localPosition.y - num3 * 0.005f * Time.deltaTime, meshTransform.transform.localPosition.z);
		if (currentState == States.Active)
		{
			enemyInvestigate = true;
			enemyInvestigateRange = 15f;
			if (bladeLerp < 1f)
			{
				bladeLerp += Time.deltaTime / secondsToStart;
			}
			else
			{
				bladeLerp = 1f;
			}
		}
		else if (bladeLerp > 0f)
		{
			bladeLerp -= Time.deltaTime / secondsToStop;
		}
		else
		{
			bladeLerp = 0f;
		}
	}

	private void StateActive()
	{
		if (stateStart)
		{
			hurtCollider.gameObject.SetActive(value: true);
			soundBladeStart.Play(physgrabobject.centerPoint);
			loopPlaying = true;
			stateStart = false;
		}
		overLapBoxCheckTimer += Time.deltaTime;
		if (overLapBoxCheckTimer >= 0.1f)
		{
			Vector3 vector = triggerCollider.bounds.size * 0.5f;
			vector.x *= Mathf.Abs(base.transform.lossyScale.x);
			vector.y *= Mathf.Abs(base.transform.lossyScale.y);
			vector.z *= Mathf.Abs(base.transform.lossyScale.z);
			if (Physics.OverlapBox(triggerCollider.bounds.center, vector / 2f, triggerCollider.transform.rotation, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide).Length != 0)
			{
				Sparks();
			}
			overLapBoxCheckTimer = 0f;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			rb.AddTorque(base.transform.up * 1f * Time.fixedDeltaTime * 30f, ForceMode.Force);
			if (UnityEngine.Random.Range(0, 100) < 7)
			{
				rb.AddForce(UnityEngine.Random.insideUnitSphere * 0.5f, ForceMode.Impulse);
				rb.AddTorque(UnityEngine.Random.insideUnitSphere * 0.1f, ForceMode.Impulse);
			}
			if (!physgrabobject.grabbed && !trapActive)
			{
				SetState(States.Idle);
			}
		}
	}

	private void StateIdle()
	{
		if (stateStart)
		{
			hurtCollider.gameObject.SetActive(value: false);
			loopPlaying = false;
			soundBladeEnd.Play(physgrabobject.centerPoint);
			stateStart = false;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && (physgrabobject.grabbed || trapActive))
		{
			SetState(States.Active);
		}
	}

	[PunRPC]
	public void SetStateRPC(States state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = state;
			stateStart = true;
		}
	}

	private void SetState(States state)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				SetStateRPC(state);
				return;
			}
			photonView.RPC("SetStateRPC", RpcTarget.All, state);
		}
	}

	public void TrapActivate()
	{
		if (!trapTriggered)
		{
			GrabRelease();
			sawTimer.Invoke();
			trapActive = true;
			trapTriggered = true;
		}
	}

	public void TrapStop()
	{
		trapActive = false;
	}

	public void Sparks()
	{
		sparkParticles.Play();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			rb.AddForce(base.transform.right * 2f, ForceMode.Impulse);
		}
	}

	public void ImpactDamage()
	{
		physGrabObject.lightBreakImpulse = true;
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
