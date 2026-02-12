using System;
using Photon.Pun;
using UnityEngine;

public class JackhammerValuable : MonoBehaviour
{
	public enum States
	{
		Idle,
		Active
	}

	private static readonly int animActive = Animator.StringToHash("active");

	private Animator animator;

	private PhysGrabObject physGrabObject;

	public Sound soundStart;

	public Sound soundEnd;

	public Sound soundLoop;

	public Sound soundImpactLoop;

	public GameObject mesh;

	public CollisionFree colliderCollision;

	public ParticleSystem dirtParticles;

	public ParticleSystem dustParticles;

	private Quaternion initialRotation;

	private float impactEffectsTimer;

	private float investigateTimer;

	private float investigateInterval = 2f;

	private PhotonView photonView;

	internal States currentState;

	private bool stateStart;

	private bool mainLoopPlaying;

	private bool impactLoopPlaying;

	private void Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
		photonView = GetComponent<PhotonView>();
		animator = GetComponent<Animator>();
		animator.enabled = false;
		dirtParticles.Stop();
		initialRotation = mesh.transform.localRotation;
	}

	private void Update()
	{
		soundLoop.PlayLoop(mainLoopPlaying, 5f, 5f);
		soundImpactLoop.PlayLoop(impactLoopPlaying, 5f, 5f);
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

	private void StateActive()
	{
		if (stateStart)
		{
			stateStart = false;
			soundStart.Play(physGrabObject.centerPoint);
		}
		mainLoopPlaying = true;
		animator.SetBool(animActive, value: true);
		animator.enabled = true;
		if (investigateTimer <= 0f)
		{
			EnemyDirector.instance.SetInvestigate(base.transform.position, 15f);
			investigateTimer = investigateInterval;
		}
		else
		{
			investigateTimer -= Time.deltaTime;
		}
		if (colliderCollision.colliding)
		{
			impactLoopPlaying = true;
			if (impactEffectsTimer >= 0.075f)
			{
				dirtParticles.Play();
				dustParticles.Play();
				GameDirector.instance.CameraImpact.ShakeDistance(0.75f, 1f, 6f, base.transform.position, 0.01f);
				GameDirector.instance.CameraShake.ShakeDistance(1f, 1f, 6f, base.transform.position, 0.25f);
				impactEffectsTimer = 0f;
			}
			impactEffectsTimer += Time.deltaTime;
		}
		else
		{
			impactLoopPlaying = false;
		}
		float num = 50f;
		float x = 2f * Mathf.Sin(Time.time * num);
		float z = 2f * Mathf.Sin(Time.time * num + MathF.PI / 2f);
		mesh.transform.localRotation = initialRotation * Quaternion.Euler(x, 0f, z);
		if (SemiFunc.IsMasterClientOrSingleplayer() && !physGrabObject.grabbed)
		{
			SetState(States.Idle);
		}
	}

	private void StateIdle()
	{
		if (stateStart)
		{
			dirtParticles.Stop();
			mesh.transform.localRotation = initialRotation;
			stateStart = false;
			if (!LevelGenerator.Instance.Generated)
			{
				soundEnd.Play(physGrabObject.centerPoint);
			}
			impactLoopPlaying = false;
		}
		mainLoopPlaying = false;
		animator.SetBool(animActive, value: false);
		if (SemiFunc.IsMasterClientOrSingleplayer() && physGrabObject.grabbed)
		{
			SetState(States.Active);
		}
	}

	public void StopAnimator()
	{
		animator.enabled = false;
	}

	public void OnHurtColliderHitEnemy()
	{
		physGrabObject.lightBreakImpulse = true;
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
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState != state)
		{
			if (!SemiFunc.IsMultiplayer())
			{
				SetStateRPC(state);
				return;
			}
			photonView.RPC("SetStateRPC", RpcTarget.All, state);
		}
	}
}
