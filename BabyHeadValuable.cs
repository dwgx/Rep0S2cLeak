using System.Collections.Generic;
using UnityEngine;

public class BabyHeadValuable : Trap
{
	[Header("Transforms")]
	public Transform head;

	public Transform eyelidPivot;

	public Transform leftEyePivot;

	public Transform rightEyePivot;

	public Transform leftEye;

	public Transform rightEye;

	[Header("Awake and asleep eyelid rotations")]
	public Quaternion awakeEyelidRotation;

	public Quaternion asleepEyelidRotation;

	[Header("Head tilt variables")]
	public float maxTiltAngle = 90f;

	public float headAwakeAngle = 45f;

	[Header("Eyelid animation variables")]
	public float eyeLerpSpeed = 2f;

	[Header("Sounds")]
	public Sound awakeSound;

	public Sound crySound;

	public Sound asleepSound;

	[Header("Transition times")]
	public float asleepTransitionTime = 0.5f;

	public float awakeTransitionTime = 0.5f;

	[Header("Crying shake variables")]
	public float joltIntervalMin = 0.2f;

	public float joltIntervalMax = 1f;

	[Space]
	public float joltForceMin = 1f;

	public float joltForceMax = 5f;

	[Space]
	public float torqueMultiplier = 0.5f;

	[Space]
	public float maxVelocity = 2f;

	[Header("Cry distance variables")]
	public float cryDistance = 20f;

	protected Rigidbody rb;

	private bool asleepTimerOn;

	private bool awakeTimerOn;

	private bool isAwake;

	private bool animating;

	private bool playersNearby;

	private float transitionTimer;

	private float cryJoltTimer;

	private float eyeSnapTimer;

	private float eyeSnapTime;

	private float headTiltAngle;

	private List<PlayerAvatar> players;

	protected override void Start()
	{
		base.Start();
		rb = GetComponent<Rigidbody>();
		headTiltAngle = Vector3.Angle(head.up, Vector3.up);
	}

	protected override void Update()
	{
		base.Update();
		crySound.PlayLoop(isAwake, 1f, 1f);
		AdjustEyelidPivotBasedOnhHeadTilt();
		CheckForNearbyPlayers();
		if (isAwake)
		{
			enemyInvestigate = true;
			EyesAnimation();
		}
		AsleepAwakeHandle();
	}

	private void CheckForNearbyPlayers()
	{
		if (!SemiFunc.FPSImpulse5())
		{
			return;
		}
		players = SemiFunc.PlayerGetList();
		foreach (PlayerAvatar player in players)
		{
			if (Vector3.Distance(player.transform.position, base.transform.position) < cryDistance)
			{
				playersNearby = true;
				break;
			}
			playersNearby = false;
		}
	}

	private void FixedUpdate()
	{
		if (isAwake && playersNearby)
		{
			BabyCry();
		}
	}

	private void EyesAnimation()
	{
		eyeSnapTimer -= Time.deltaTime;
		if (!(eyeSnapTimer >= 0f))
		{
			float x = Random.Range(-25f, 25f);
			float y = Random.Range(-50f, 50f);
			Quaternion quaternion = Quaternion.Euler(x, y, 0f);
			leftEyePivot.localRotation = Quaternion.Lerp(leftEyePivot.localRotation, quaternion, 1f);
			rightEyePivot.localRotation = Quaternion.Lerp(rightEyePivot.localRotation, quaternion, 1f);
			eyeSnapTimer = Random.Range(joltIntervalMin, joltIntervalMax);
		}
	}

	private void AsleepTransitionAnimation(Quaternion newPos)
	{
		eyelidPivot.localRotation = Quaternion.Lerp(eyelidPivot.localRotation, newPos, eyeLerpSpeed * Time.deltaTime);
		leftEyePivot.localRotation = Quaternion.Lerp(leftEye.localRotation, Quaternion.Euler(0f, 0f, 0f), eyeLerpSpeed * Time.deltaTime);
		rightEyePivot.localRotation = Quaternion.Lerp(rightEye.localRotation, Quaternion.Euler(0f, 0f, 0f), eyeLerpSpeed * Time.deltaTime);
		if (Quaternion.Angle(eyelidPivot.localRotation, newPos) < 1f)
		{
			animating = false;
			eyelidPivot.localRotation = newPos;
		}
	}

	private void AdjustEyelidPivotBasedOnhHeadTilt()
	{
		if (!isAwake)
		{
			float t = Mathf.Clamp01(Mathf.InverseLerp(0f, maxTiltAngle, headTiltAngle));
			if (!animating)
			{
				eyelidPivot.localRotation = Quaternion.Lerp(awakeEyelidRotation, asleepEyelidRotation, t);
			}
			else
			{
				AsleepTransitionAnimation(Quaternion.Lerp(awakeEyelidRotation, asleepEyelidRotation, t));
			}
		}
	}

	private void AsleepAwakeHandle()
	{
		headTiltAngle = Vector3.Angle(head.up, Vector3.up);
		if (IsInAwakePosition())
		{
			if (asleepTimerOn)
			{
				asleepTimerOn = false;
				transitionTimer = 0f;
			}
			if (awakeTimerOn && !isAwake)
			{
				transitionTimer += Time.fixedDeltaTime;
				if (transitionTimer >= awakeTransitionTime)
				{
					BabyAwake();
					awakeTimerOn = false;
					transitionTimer = 0f;
				}
			}
			else if (!isAwake)
			{
				awakeTimerOn = true;
			}
			return;
		}
		if (awakeTimerOn)
		{
			awakeTimerOn = false;
			transitionTimer = 0f;
		}
		if (asleepTimerOn && isAwake)
		{
			transitionTimer += Time.fixedDeltaTime;
			if (transitionTimer >= asleepTransitionTime)
			{
				BabySleep();
				asleepTimerOn = false;
				transitionTimer = 0f;
			}
		}
		else if (isAwake)
		{
			asleepTimerOn = true;
		}
	}

	private void BabyAwake()
	{
		awakeSound.Play(physGrabObject.centerPoint);
		isAwake = true;
		cryJoltTimer = Random.Range(joltIntervalMin, joltIntervalMax);
		eyeSnapTimer = Random.Range(joltIntervalMin, joltIntervalMax);
		eyelidPivot.localRotation = Quaternion.Lerp(eyelidPivot.localRotation, awakeEyelidRotation, 1f);
	}

	private void BabySleep()
	{
		asleepSound.Play(physGrabObject.centerPoint);
		isAwake = false;
		animating = true;
	}

	private bool IsInAwakePosition()
	{
		return headTiltAngle < headAwakeAngle;
	}

	private void BabyCry()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		cryJoltTimer -= Time.deltaTime;
		if (cryJoltTimer <= 0f)
		{
			if (rb.velocity.magnitude < maxVelocity)
			{
				rb.AddForce(Vector3.up * Random.Range(joltForceMin, joltForceMax), ForceMode.Impulse);
				Vector3 torque = Random.insideUnitSphere.normalized * torqueMultiplier;
				rb.AddTorque(torque, ForceMode.Impulse);
			}
			cryJoltTimer = Random.Range(joltIntervalMin, joltIntervalMax);
		}
	}
}
