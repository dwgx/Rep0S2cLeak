using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ValuableEgg : Trap
{
	public enum EggState
	{
		State1,
		State2,
		State3,
		State4,
		State5,
		Explode
	}

	[Header("Times")]
	public float explosionTime = 3f;

	public float crackRestTime = 0.5f;

	public Vector2 joltIntervalRange = new Vector2(0.2f, 1f);

	[Header("Physics")]
	public float torqueMultiplier = 0.5f;

	[Header("Camera Shake")]
	public float cameraShakeTime = 0.2f;

	public float cameraShakeStrength = 3f;

	public Vector2 cameraShakeBounds = new Vector2(1.5f, 5f);

	[Header("Arm & Leg Animation")]
	public Vector2 armRange = new Vector2(-45f, 45f);

	public Vector2 legRange = new Vector2(-25f, 20f);

	[Header("Transforms")]
	public Transform objectTransform;

	public Transform armL;

	public Transform armR;

	public Transform legL;

	public Transform legR;

	[Header("Materials")]
	public Material state2Material;

	public Material state3Material;

	public Material state4Material;

	public Material state5Material;

	[Header("Mesh renderer")]
	public MeshRenderer meshRenderer;

	[Header("Particles")]
	public ParticleSystem breakParticle;

	[Space]
	public Light pointLight;

	[Header("Sounds")]
	public Sound crackSound;

	public Sound screamSound;

	public Sound explosionSound;

	private EggState currentState;

	private ParticleScriptExplosion particleScriptExplosion;

	private Rigidbody rb;

	private Vector3 originalSize;

	private float explosionTimer;

	private float joltTimer;

	private bool effectsOn;

	protected override void Start()
	{
		base.Start();
		originalSize = objectTransform.localScale;
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		rb = GetComponent<Rigidbody>();
	}

	protected override void Update()
	{
		screamSound.PlayLoop(currentState == EggState.State5, 1f, 1f);
		if (currentState == EggState.Explode)
		{
			Explode();
		}
		if (currentState == EggState.State5)
		{
			PlayEffects();
			AnimateArmsAndLegs();
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState == EggState.State5)
		{
			explosionTimer += Time.deltaTime;
			if (explosionTimer >= explosionTime)
			{
				SetNextState();
			}
			enemyInvestigate = true;
		}
	}

	protected void FixedUpdate()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		List<PhysGrabber> playerGrabbing = physGrabObject.playerGrabbing;
		bool flag = false;
		foreach (PhysGrabber item in playerGrabbing)
		{
			if (item.isRotating)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			Quaternion turnX = Quaternion.Euler(0f, -180f, 0f);
			Quaternion turnY = Quaternion.Euler(0f, 0f, 0f);
			Quaternion identity = Quaternion.identity;
			physGrabObject.TurnXYZ(turnX, turnY, identity);
			physGrabObject.OverrideTorqueStrength(2f + physGrabObject.massOriginal);
		}
		if (currentState == EggState.State5)
		{
			ScreamShake();
		}
	}

	[PunRPC]
	public void SetStateRPC(EggState _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _state;
		}
	}

	private void SetState(EggState _state)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				SetStateRPC(_state);
				return;
			}
			photonView.RPC("SetStateRPC", RpcTarget.All, _state);
		}
	}

	private void SetNextState()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			switch (currentState)
			{
			case EggState.State1:
				SetState(EggState.State2);
				break;
			case EggState.State2:
				SetState(EggState.State3);
				break;
			case EggState.State3:
				SetState(EggState.State4);
				break;
			case EggState.State4:
				SetState(EggState.State5);
				break;
			case EggState.State5:
				SetState(EggState.Explode);
				break;
			}
		}
	}

	public void Explode()
	{
		particleScriptExplosion.Spawn(meshRenderer.transform.position, 1.5f, 100, 300);
		physGrabObject.dead = true;
		Object.Destroy(base.gameObject);
	}

	public void Crack()
	{
		if (currentState == EggState.State5)
		{
			return;
		}
		crackSound.Play(physGrabObject.centerPoint);
		foreach (PhysGrabber item in new List<PhysGrabber>(physGrabObject.playerGrabbing))
		{
			item.OverrideGrabRelease(photonView.ViewID);
		}
		ParticleSystem.MainModule main = breakParticle.main;
		switch (currentState)
		{
		case EggState.State1:
			objectTransform.localScale = originalSize * 0.9f;
			meshRenderer.material = state2Material;
			break;
		case EggState.State2:
			objectTransform.localScale = originalSize * 0.7f;
			meshRenderer.material = state3Material;
			main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.6f, 0.6f, 0f));
			break;
		case EggState.State3:
			objectTransform.localScale = originalSize * 0.5f;
			meshRenderer.material = state4Material;
			main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.7f, 0f));
			break;
		case EggState.State4:
			objectTransform.localScale = originalSize * 0.25f;
			meshRenderer.material = state5Material;
			main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.3f, 0f));
			EnableLimbs();
			break;
		}
		breakParticle.Play();
		CameraShake();
		SetNextState();
		SetMass();
	}

	private void SetMass()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			switch (currentState)
			{
			case EggState.State1:
				physGrabObject.OverrideMass(2f);
				physGrabObject.massOriginal = 2f;
				break;
			case EggState.State2:
				physGrabObject.OverrideMass(1.75f);
				physGrabObject.massOriginal = 1.75f;
				break;
			case EggState.State3:
				physGrabObject.OverrideMass(1.25f);
				physGrabObject.massOriginal = 1.25f;
				break;
			case EggState.State4:
				physGrabObject.OverrideMass(1f);
				physGrabObject.massOriginal = 1f;
				break;
			case EggState.State5:
				physGrabObject.OverrideMass(0.5f);
				physGrabObject.massOriginal = 0.5f;
				break;
			}
		}
	}

	private void EnableLimbs()
	{
		armL.gameObject.SetActive(value: true);
		armR.gameObject.SetActive(value: true);
		legL.gameObject.SetActive(value: true);
		legR.gameObject.SetActive(value: true);
	}

	private void ScreamShake()
	{
		joltTimer -= Time.deltaTime;
		if (joltTimer <= 0f)
		{
			Vector3 torque = Random.insideUnitSphere.normalized * torqueMultiplier;
			rb.AddTorque(torque, ForceMode.Impulse);
			joltTimer = Random.Range(joltIntervalRange.x, joltIntervalRange.y);
		}
	}

	private void CameraShake()
	{
		GameDirector.instance.CameraShake.ShakeDistance(cameraShakeStrength, 3f, 8f, base.transform.position, cameraShakeTime);
		GameDirector.instance.CameraImpact.ShakeDistance(cameraShakeStrength, cameraShakeBounds.x, cameraShakeBounds.y, base.transform.position, cameraShakeTime);
	}

	private void PlayEffects()
	{
		if (!effectsOn)
		{
			pointLight.enabled = true;
			effectsOn = true;
		}
	}

	private void AnimateArmsAndLegs()
	{
		float x = Mathf.Lerp(armRange.x, armRange.y, Mathf.PingPong(Time.time * 8f, 1f));
		float x2 = Mathf.Lerp(armRange.x, armRange.y, Mathf.PingPong(Time.time * 8f + 0.5f, 1f));
		armL.localRotation = Quaternion.Euler(x, 0f, 0f);
		armR.localRotation = Quaternion.Euler(x2, 0f, 0f);
		float x3 = Mathf.Lerp(legRange.x, legRange.y, Mathf.PingPong(Time.time * 8f, 1f));
		float x4 = Mathf.Lerp(legRange.x, legRange.y, Mathf.PingPong(Time.time * 8f + 0.5f, 1f));
		legL.localRotation = Quaternion.Euler(x3, 0f, 0f);
		legR.localRotation = Quaternion.Euler(x4, 0f, 0f);
	}
}
