using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class FanTrap : Trap
{
	public enum States
	{
		Idle,
		Active
	}

	public UnityEvent fanTimer;

	public Transform fanButton;

	public HurtCollider hurtCollider;

	public MeshRenderer buttonMesh;

	private float initialPlayerHitForce;

	private float initialPhysHitForce;

	private float initialEnemyHitForce;

	[Header("Fan Blade")]
	public Transform fanBlades;

	public AnimationCurve fanBladeSpeedCurve;

	private PhysGrabObject physgrabobject;

	private float fanBladeSpeed;

	private float fanBladeMaxSpeed = 1500f;

	private float fanBladeLerp;

	private float secondsToStart = 2f;

	private float secondsToStop = 4f;

	internal States currentState;

	private bool stateStart;

	[Header("Sounds")]
	public Sound sfxButtonOn;

	public Sound sfxButtonOff;

	public Sound sfxFanLoop;

	[Header("Particles")]
	public ParticleSystem windParticles;

	public ParticleSystem windSmallParticles;

	protected override void Start()
	{
		base.Start();
		hurtCollider.gameObject.SetActive(value: false);
		physgrabobject = GetComponent<PhysGrabObject>();
		photonView = GetComponent<PhotonView>();
		initialPlayerHitForce = hurtCollider.playerHitForce;
		initialPhysHitForce = hurtCollider.physHitForce;
		initialEnemyHitForce = hurtCollider.enemyHitForce;
	}

	private void FixedUpdate()
	{
	}

	protected override void Update()
	{
		base.Update();
		switch (currentState)
		{
		case States.Active:
			StateActive();
			break;
		case States.Idle:
			StateIdle();
			break;
		}
		hurtCollider.gameObject.SetActive(currentState == States.Active);
		sfxFanLoop.PlayLoop(currentState == States.Active, 0.1f, 0.025f);
		sfxFanLoop.LoopPitch = Mathf.Lerp(0.1f, 1f, fanBladeSpeedCurve.Evaluate(fanBladeLerp));
		hurtCollider.playerHitForce = Mathf.Lerp(0f, initialPlayerHitForce, fanBladeSpeedCurve.Evaluate(fanBladeLerp));
		hurtCollider.physHitForce = Mathf.Lerp(0f, initialPhysHitForce, fanBladeSpeedCurve.Evaluate(fanBladeLerp));
		hurtCollider.enemyHitForce = Mathf.Lerp(0f, initialEnemyHitForce, fanBladeSpeedCurve.Evaluate(fanBladeLerp));
		fanBlades.Rotate(-Vector3.forward * fanBladeSpeed * Time.deltaTime);
		fanBladeSpeed = Mathf.Lerp(0f, fanBladeMaxSpeed, fanBladeSpeedCurve.Evaluate(fanBladeLerp));
	}

	private void StateActive()
	{
		if (stateStart)
		{
			sfxButtonOn.Play(physgrabobject.centerPoint);
			windParticles.Play();
			windSmallParticles.Play();
			buttonMesh.material.EnableKeyword("_EMISSION");
			fanTimer.Invoke();
			stateStart = false;
		}
		enemyInvestigate = true;
		if (SemiFunc.IsMasterClientOrSingleplayer() && !physgrabobject.grabbed)
		{
			SetState(States.Idle);
		}
		fanButton.localEulerAngles = new Vector3(21f, 0f, 0f);
		if (fanBladeLerp < 1f)
		{
			fanBladeLerp += Time.deltaTime / secondsToStart;
		}
	}

	private void StateIdle()
	{
		if (stateStart)
		{
			sfxButtonOff.Play(physgrabobject.centerPoint);
			windParticles.Stop();
			windSmallParticles.Stop();
			buttonMesh.material.DisableKeyword("_EMISSION");
			stateStart = false;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && physgrabobject.grabbed)
		{
			SetState(States.Active);
		}
		fanButton.localEulerAngles = new Vector3(0f, 0f, 0f);
		if (fanBladeLerp > 0f)
		{
			fanBladeLerp -= Time.deltaTime / secondsToStop;
		}
	}

	[PunRPC]
	public void SetStateRPC(States state)
	{
		currentState = state;
		stateStart = true;
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
}
