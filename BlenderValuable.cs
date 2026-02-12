using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class BlenderValuable : Trap
{
	public enum States
	{
		Idle,
		Active,
		Pop,
		IdleEmpty,
		ActiveEmpty
	}

	private ParticleScriptExplosion particleScriptExplosion;

	private float startLerpDuration = 1f;

	private float moneyLossTimer = 1f;

	public Sound soundBladeLoop;

	public Sound soundBladeStart;

	public Sound soundBladeEnd;

	public Sound soundBladeStuckLoop;

	public Sound soundBladeEmptyLoop;

	public Sound capPop;

	public Sound blast;

	public ParticleSystem billParticles;

	public ParticleSystem billParticlesRandom;

	public ParticleSystem dustParticles;

	public ParticleSystem capParticle;

	[Space]
	public Transform meshTransform;

	public UnityEvent blenderTimer;

	private bool continueBlendAfterRelease;

	public Transform blade;

	public Transform money;

	public Transform moneyBot;

	public Transform billSoup;

	public Transform cap;

	public Transform explosionCenter;

	public GameObject capCollider;

	public AnimationCurve bladeCurve;

	public AnimationCurve billSoupCurve;

	public float bladeSpeed;

	private float bladeMaxSpeed = 1500f;

	private float bladeLerp;

	private float secondsToStart = 2f;

	private float secondsToStop = 0.5f;

	private bool dustParticlesPlaying;

	private Vector3 billSoupZeroScale = Vector3.zero;

	private Vector3 initialBillSoupScale = new Vector3(0.8f, 0.1f, 0.8f);

	private Vector3 targetBillSoupScale = new Vector3(1f, 1f, 1f);

	private float billSoupLerpDuration = 5f;

	private float billSoupLerpTime;

	private Vector3 initialMoneyScale = new Vector3(0.85f, 0.85f, 0.85f);

	private Vector3 targetMoneyScale = new Vector3(0.7f, 0.2f, 0.7f);

	private float moneyLerpDuration = 10f;

	private float moneyLerpTime;

	public HurtCollider hurtCollider;

	internal States currentState;

	private bool stateStart;

	private float stateTimer;

	[Space]
	private Quaternion initialTankRotation;

	private Animator animator;

	private PhysGrabObject physgrabobject;

	private Rigidbody rb;

	private bool fullLoopPlaying;

	private bool stuckLoopPlaying;

	private bool emptyLoopPlaying;

	protected override void Start()
	{
		base.Start();
		physgrabobject = GetComponent<PhysGrabObject>();
		photonView = GetComponent<PhotonView>();
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		initialTankRotation = meshTransform.transform.localRotation;
	}

	private void FixedUpdate()
	{
		switch (currentState)
		{
		case States.Active:
			StateActive();
			InvokeImpactDamage();
			break;
		case States.Idle:
			StateIdle();
			break;
		case States.Pop:
			StatePop();
			break;
		case States.ActiveEmpty:
			StateActive();
			break;
		case States.IdleEmpty:
			StateIdle();
			break;
		}
	}

	protected override void Update()
	{
		base.Update();
		soundBladeLoop.PlayLoop(fullLoopPlaying, 5f, 5f);
		soundBladeStuckLoop.PlayLoop(stuckLoopPlaying, 5f, 5f);
		soundBladeEmptyLoop.PlayLoop(emptyLoopPlaying, 5f, 5f);
		blade.Rotate(Vector3.up * bladeSpeed * Time.deltaTime);
		bladeSpeed = Mathf.Lerp(0f, bladeMaxSpeed, bladeCurve.Evaluate(bladeLerp));
		money.Rotate(Vector3.up * (bladeSpeed / 1.5f) * Time.deltaTime);
		moneyBot.Rotate(Vector3.up * (bladeSpeed / 1.5f) * Time.deltaTime);
		billSoup.Rotate(Vector3.up * (bladeSpeed / 5f) * Time.deltaTime);
		float num = 0.3f * bladeCurve.Evaluate(bladeLerp);
		float num2 = 60f * bladeCurve.Evaluate(bladeLerp);
		float num3 = num * Mathf.Sin(Time.time * num2);
		float z = num * Mathf.Sin(Time.time * num2 + MathF.PI / 2f);
		meshTransform.transform.localRotation = initialTankRotation * Quaternion.Euler(num3, 0f, z);
		meshTransform.transform.localPosition = new Vector3(meshTransform.transform.localPosition.x, meshTransform.transform.localPosition.y - num3 * 0.005f * Time.deltaTime, meshTransform.transform.localPosition.z);
		money.localPosition = new Vector3(money.localPosition.x, money.localPosition.y - num3 * 0.005f * Time.deltaTime, money.localPosition.z);
		billSoup.localPosition = new Vector3(billSoup.localPosition.x, billSoup.localPosition.y - num3 * 0.005f * Time.deltaTime, billSoup.localPosition.z);
		if (currentState == States.Active || currentState == States.ActiveEmpty || currentState == States.Pop)
		{
			if (bladeLerp < 1f)
			{
				bladeLerp += Time.deltaTime / secondsToStart;
				if (bladeLerp > 0.5f && !hurtCollider.gameObject.activeSelf)
				{
					continueBlendAfterRelease = true;
					hurtCollider.gameObject.SetActive(value: true);
				}
			}
			else
			{
				bladeLerp = 1f;
			}
			if (currentState == States.ActiveEmpty || currentState == States.Pop)
			{
				return;
			}
			billSoup.gameObject.SetActive(value: true);
			if (billSoupLerpTime < startLerpDuration && bladeLerp >= 1f)
			{
				billSoupLerpTime += Time.deltaTime;
				float t = billSoupLerpTime / startLerpDuration;
				billSoup.localScale = Vector3.Lerp(billSoupZeroScale, initialBillSoupScale, t);
			}
			else if (billSoupLerpTime < startLerpDuration + billSoupLerpDuration && bladeLerp >= 1f)
			{
				billSoupLerpTime += Time.deltaTime;
				float t2 = (billSoupLerpTime - startLerpDuration) / billSoupLerpDuration;
				billSoup.localScale = Vector3.Lerp(initialBillSoupScale, targetBillSoupScale, t2);
			}
			if (moneyLerpTime < moneyLerpDuration && bladeLerp >= 1f)
			{
				moneyLerpTime += Time.deltaTime;
				float t3 = moneyLerpTime / moneyLerpDuration;
				Vector3 localScale = Vector3.Lerp(initialMoneyScale, targetMoneyScale, t3);
				money.localScale = localScale;
				moneyBot.localScale = localScale;
			}
			if (moneyLerpTime > moneyLerpDuration * 0.75f)
			{
				if (!dustParticlesPlaying)
				{
					dustParticles.Play();
					dustParticlesPlaying = true;
					fullLoopPlaying = false;
					emptyLoopPlaying = false;
					stuckLoopPlaying = true;
				}
			}
			else if (bladeLerp >= 1f && !billParticlesRandom.isPlaying)
			{
				billParticlesRandom.Play();
			}
			if (moneyLerpTime > moneyLerpDuration)
			{
				money.gameObject.SetActive(value: false);
				if (SemiFunc.IsMasterClientOrSingleplayer())
				{
					SetState(States.Pop);
				}
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
			soundBladeStart.Play(physgrabobject.centerPoint);
			if (currentState == States.Active)
			{
				fullLoopPlaying = true;
			}
			else
			{
				emptyLoopPlaying = true;
			}
			stateStart = false;
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (!physgrabobject.grabbed && currentState == States.Active)
		{
			if (continueBlendAfterRelease)
			{
				continueBlendAfterRelease = false;
			}
			SetState(States.Idle);
		}
		if (!physgrabobject.grabbed && currentState == States.ActiveEmpty)
		{
			if (!continueBlendAfterRelease)
			{
				SetState(States.IdleEmpty);
			}
			else
			{
				blenderTimer.Invoke();
			}
		}
	}

	private void StateIdle()
	{
		if (stateStart)
		{
			hurtCollider.gameObject.SetActive(value: false);
			fullLoopPlaying = false;
			stuckLoopPlaying = false;
			emptyLoopPlaying = false;
			soundBladeEnd.Play(physgrabobject.centerPoint);
			stateStart = false;
			if (dustParticlesPlaying)
			{
				dustParticles.Stop();
				dustParticlesPlaying = false;
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (physgrabobject.grabbed && currentState == States.Idle)
			{
				SetState(States.Active);
			}
			if (physgrabobject.grabbed && currentState == States.IdleEmpty)
			{
				SetState(States.ActiveEmpty);
			}
		}
	}

	private void StatePop()
	{
		if (stateStart)
		{
			particleScriptExplosion.Spawn(explosionCenter.position, 0.5f, 0, 0, 0f, onlyParticleEffect: true);
			billSoup.gameObject.SetActive(value: false);
			billParticles.Play();
			capParticle.Play();
			capCollider.SetActive(value: false);
			cap.gameObject.SetActive(value: false);
			capPop.Play(physgrabobject.centerPoint);
			blast.Play(physgrabobject.centerPoint);
			if (dustParticlesPlaying)
			{
				dustParticles.Stop();
				dustParticlesPlaying = false;
			}
			stateStart = false;
			stateTimer = 0.5f;
		}
		else
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f && SemiFunc.IsMasterClientOrSingleplayer())
			{
				SetState(States.ActiveEmpty);
			}
		}
	}

	public void InvokeImpactDamage()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			moneyLossTimer -= Time.deltaTime;
			if (moneyLossTimer <= 0f && bladeLerp >= 1f)
			{
				physgrabobject.lightBreakImpulse = true;
				moneyLossTimer = 0.33f;
			}
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

	public void TrapActivate()
	{
		if (!trapTriggered)
		{
			trapActive = true;
			trapTriggered = true;
		}
	}

	public void TimerEnd()
	{
		continueBlendAfterRelease = false;
	}

	public void TrapStop()
	{
		trapActive = false;
	}
}
