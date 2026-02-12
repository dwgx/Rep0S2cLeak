using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTickAnim : MonoBehaviour
{
	[Serializable]
	public class springLimb
	{
		public SpringQuaternion spring;

		public string name;

		public Transform transform;

		public Transform target;

		[HideInInspector]
		public Quaternion originalQuaternion;

		public float sineEval;
	}

	[Serializable]
	public class springBloatPart
	{
		public SpringVector3 spring;

		public string name;

		public Transform transform;

		public Transform target;

		[HideInInspector]
		public Vector3 originalScale;

		public float sineEval;
	}

	public Enemy enemy;

	public EnemyTick controller;

	internal Materials.MaterialTrigger material = new Materials.MaterialTrigger();

	public GameObject mouth;

	private float mouthLerp;

	private float mouthLerpSpeed = 1f;

	private float mouthLerpIntensity = 0.5f;

	public GameObject lipsClosed;

	public GameObject lipsOpen;

	public bool lipsState;

	public GameObject flatHead;

	public GameObject bloatedHead;

	private bool flatHeadSwitch;

	private Vector3 bloatPartsOrginalScale;

	[Space]
	public ParticleSystem particleImpact;

	public ParticleSystem particleBits;

	public ParticleSystem particleDirectionalBits;

	public List<ParticleSystem> failedHealParticles;

	[Space]
	[Header("Sounds")]
	public Sound skitteringLoop;

	public Sound walkLoop;

	public Sound runLoop;

	public Sound suckLoop;

	public Sound hurtSound;

	public Sound deathSound;

	public Sound bigSuckSound;

	public Sound jumpSound;

	public Sound noticeSound;

	private bool walkLoopPlaying;

	private bool runLoopPlaying;

	private bool jumpImpulse;

	public List<springLimb> springLimbs = new List<springLimb>();

	public List<springBloatPart> springBloatParts = new List<springBloatPart>();

	private SpringQuaternion jumpRight = new SpringQuaternion();

	public Transform jumpTransformRight;

	public Transform jumpTransformTargetRight;

	private SpringQuaternion springJumpLeft = new SpringQuaternion();

	public Transform jumpTransformLeft;

	public Transform jumpTransformTargetLeft;

	private SpringQuaternion springBody = new SpringQuaternion();

	public Transform bodyTransform;

	public Transform bodyTransformTarget;

	public SpringVector3 springHead = new SpringVector3();

	public Transform headTransform;

	public Transform headTransformTarget;

	private float overrideWalkAnimationTimer;

	private bool isWalking;

	private bool wasGrounded;

	private float materialImpactTimer;

	private float bodyAnimationEvalX;

	private float bodyAnimationEvalY;

	private float bodyAnimationEvalZ;

	public AnimationCurve BodyAnimationCurve;

	private float maxScaleBloatParts = 1.3f;

	private Vector3 lastPosition;

	private float janneksVelocity;

	public int maxHealth = 100;

	private int lastSyncedHealth;

	public bool hasBeenFull;

	private EnemyTick.State prevState;

	private void Awake()
	{
		bloatPartsOrginalScale = springBloatParts[0].transform.localScale;
	}

	private void Update()
	{
		if (controller.currentState == EnemyTick.State.Despawn)
		{
			headTransformTarget.localScale = Vector3.zero;
			if (headTransform.localScale.y <= 0.1f)
			{
				foreach (ParticleSystem failedHealParticle in failedHealParticles)
				{
					failedHealParticle.Play();
				}
				enemy.EnemyParent.Despawn();
			}
		}
		if (controller.syncedHealth != lastSyncedHealth)
		{
			OnHealthChanged(controller.syncedHealth);
			if (controller.syncedHealth > lastSyncedHealth)
			{
				bigSuckSound.Play(controller.transform.position);
			}
			lastSyncedHealth = controller.syncedHealth;
		}
		VisualUpdateSprings();
		OverrideWalkAnimationTick();
		WalkAnimationLogic();
		if (flatHeadSwitch && headTransform.localScale.y <= 0.5f)
		{
			flatHead.SetActive(value: true);
			bloatedHead.SetActive(value: false);
			headTransformTarget.localScale = Vector3.one;
			headTransform.localScale = Vector3.one;
			flatHeadSwitch = false;
		}
		OverrideWalkAnimation();
		if (controller.currentState == EnemyTick.State.Bite)
		{
			SuckAnimationLogic();
			if (!lipsState)
			{
				lipsState = true;
				lipsClosed.SetActive(value: false);
				lipsOpen.SetActive(value: true);
			}
			mouthLerp += Time.deltaTime * mouthLerpSpeed;
			if (mouthLerp > 0.5f)
			{
				mouthLerp = 0f;
			}
			mouth.transform.localScale = new Vector3(1f + BodyAnimationCurve.Evaluate(mouthLerp) * mouthLerpIntensity, 1f + BodyAnimationCurve.Evaluate(mouthLerp) * mouthLerpIntensity, 1f + BodyAnimationCurve.Evaluate(mouthLerp) * mouthLerpIntensity);
		}
		else
		{
			mouth.transform.localScale = Vector3.one;
			if (lipsState)
			{
				lipsState = false;
				lipsClosed.SetActive(value: true);
				lipsOpen.SetActive(value: false);
			}
		}
		if (controller.enemy.Grounded.grounded && !wasGrounded)
		{
			Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Light, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
			wasGrounded = true;
		}
		else if (!controller.enemy.Grounded.grounded && wasGrounded)
		{
			wasGrounded = false;
		}
		if (controller.currentState == EnemyTick.State.Spawn && prevState != EnemyTick.State.Spawn)
		{
			springHead.lastPosition = Vector3.zero;
			springHead.springVelocity = Vector3.zero;
			headTransformTarget.localScale = Vector3.one;
			foreach (ParticleSystem failedHealParticle2 in failedHealParticles)
			{
				failedHealParticle2.Play();
			}
			OnHealthChanged(controller.syncedHealth);
		}
		float pitchMultiplier = Mathf.Clamp(janneksVelocity * 12f, 0.75f, 2f);
		if ((double)janneksVelocity > 0.02 && controller.enemy.Grounded.grounded)
		{
			if (hasBeenFull)
			{
				walkLoopPlaying = false;
				runLoopPlaying = true;
			}
			else
			{
				runLoopPlaying = false;
				walkLoopPlaying = true;
			}
		}
		else
		{
			walkLoopPlaying = false;
			runLoopPlaying = false;
		}
		if (SemiFunc.Arachnophobia())
		{
			skitteringLoop.PlayLoop(playing: false, 5f, 5f);
			walkLoop.PlayLoop(playing: false, 5f, 5f);
		}
		else
		{
			skitteringLoop.PlayLoop((double)janneksVelocity > 0.02, 5f, 5f, pitchMultiplier);
			walkLoop.PlayLoop(walkLoopPlaying, 5f, 5f);
		}
		runLoop.PlayLoop(runLoopPlaying, 5f, 5f);
		if (walkLoopPlaying || runLoopPlaying)
		{
			float num = Mathf.Lerp(1f, 0.2f, Mathf.Clamp01(janneksVelocity * 4f));
			if (materialImpactTimer >= num)
			{
				Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint, Vector3.down, Materials.SoundType.Light, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
				materialImpactTimer = 0f;
			}
			materialImpactTimer += Time.deltaTime;
		}
		if (controller.currentState == EnemyTick.State.Bite)
		{
			suckLoop.PlayLoop(playing: true, 5f, 5f);
		}
		else
		{
			suckLoop.PlayLoop(playing: false, 5f, 5f);
		}
		if (controller.currentState == EnemyTick.State.Notice && prevState != EnemyTick.State.Notice)
		{
			noticeSound.Play(base.transform.position);
		}
		if (controller.currentState == EnemyTick.State.Tumble && prevState != EnemyTick.State.Tumble)
		{
			jumpTransformTargetRight.localRotation = Quaternion.Euler(0f, 0f, 45f);
			jumpTransformTargetLeft.localRotation = Quaternion.Euler(0f, 0f, -45f);
			jumpSound.Play(base.transform.position);
		}
		if (enemy.Jump.jumping)
		{
			if (jumpImpulse)
			{
				jumpTransformTargetRight.localRotation = Quaternion.Euler(0f, 0f, 50f);
				jumpTransformTargetLeft.localRotation = Quaternion.Euler(0f, 0f, -50f);
				jumpSound.Play(controller.transform.position);
				jumpImpulse = false;
			}
		}
		else
		{
			if (jumpTransformRight.localRotation.z >= Quaternion.Euler(0f, 0f, 45f).z)
			{
				jumpTransformTargetRight.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
			if (jumpTransformLeft.localRotation.z <= Quaternion.Euler(0f, 0f, -45f).z)
			{
				jumpTransformTargetLeft.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
			jumpImpulse = true;
		}
		prevState = controller.currentState;
	}

	private void VisualUpdateSprings()
	{
		int num = 30;
		float damping = 0.35f;
		int num2 = 20;
		float damping2 = 0.15f;
		foreach (springLimb springLimb in springLimbs)
		{
			springLimb.spring.speed = num;
			springLimb.spring.damping = damping;
			springLimb.spring.maxAngle = 5f;
			springLimb.spring.clamp = false;
			springLimb.transform.rotation = SemiFunc.SpringQuaternionGet(springLimb.spring, springLimb.target.rotation);
		}
		foreach (springBloatPart springBloatPart in springBloatParts)
		{
			springBloatPart.spring.speed = num2;
			springBloatPart.spring.damping = damping2;
			springBloatPart.spring.maxDistance = 0.5f;
			springBloatPart.spring.clamp = false;
			springBloatPart.transform.localScale = SemiFunc.SpringVector3Get(springBloatPart.spring, springBloatPart.target.localScale);
		}
		bodyTransform.rotation = SemiFunc.SpringQuaternionGet(springBody, bodyTransformTarget.rotation);
		springBody.speed = 20f;
		springBody.damping = 0.35f;
		headTransform.localScale = SemiFunc.SpringVector3Get(springHead, headTransformTarget.localScale);
		springHead.speed = 20f;
		springHead.damping = 0.15f;
		jumpTransformRight.localRotation = SemiFunc.SpringQuaternionGet(jumpRight, jumpTransformTargetRight.localRotation);
		jumpRight.speed = 20f;
		jumpRight.damping = 0.35f;
		jumpTransformLeft.localRotation = SemiFunc.SpringQuaternionGet(springJumpLeft, jumpTransformTargetLeft.localRotation);
		springJumpLeft.speed = 20f;
		springJumpLeft.damping = 0.35f;
	}

	private void WalkAnimationLogic()
	{
		if (!isWalking)
		{
			return;
		}
		if (SemiFunc.FPSImpulse15())
		{
			janneksVelocity = Vector3.Distance(lastPosition, enemy.Rigidbody.transform.position);
			lastPosition = enemy.Rigidbody.transform.position;
		}
		float num = janneksVelocity * 6f;
		int num2 = 0;
		foreach (springLimb springLimb in springLimbs)
		{
			float num3 = BodyAnimationCurve.Evaluate(springLimb.sineEval);
			if (num2 % 2 == 0)
			{
				if (springLimb.sineEval <= 0f)
				{
					springLimb.sineEval = 1f;
				}
				springLimb.sineEval -= Time.deltaTime * 5f * num;
			}
			else
			{
				if (springLimb.sineEval >= 1f)
				{
					springLimb.sineEval = 0f;
				}
				springLimb.sineEval += Time.deltaTime * 5f * num;
			}
			springLimb.target.localRotation = Quaternion.Euler(num3 * 20f, num3 * 20f, num3 * 20f);
			num2++;
		}
		if (bodyAnimationEvalX >= 1f)
		{
			bodyAnimationEvalX = 0f;
		}
		float num4 = BodyAnimationCurve.Evaluate(bodyAnimationEvalX);
		bodyAnimationEvalX += Time.deltaTime * 5f * num;
		if (bodyAnimationEvalY >= 1f)
		{
			bodyAnimationEvalY = 0f;
		}
		float num5 = BodyAnimationCurve.Evaluate(bodyAnimationEvalY);
		bodyAnimationEvalY += Time.deltaTime * 10f * num;
		if (bodyAnimationEvalZ >= 1f)
		{
			bodyAnimationEvalZ = 0f;
		}
		float num6 = BodyAnimationCurve.Evaluate(bodyAnimationEvalZ);
		bodyAnimationEvalZ += Time.deltaTime * 2.5f * num;
		bodyTransformTarget.localRotation = Quaternion.Euler(num4 * 8f, num5 * 8f, num6 * 8f);
	}

	private void SuckAnimationLogic()
	{
		if (bodyAnimationEvalX >= 1f)
		{
			bodyAnimationEvalX = 0f;
		}
		float num = BodyAnimationCurve.Evaluate(bodyAnimationEvalX);
		bodyAnimationEvalX += Time.deltaTime * 5f;
		if (bodyAnimationEvalY >= 1f)
		{
			bodyAnimationEvalY = 0f;
		}
		float num2 = BodyAnimationCurve.Evaluate(bodyAnimationEvalY);
		bodyAnimationEvalY += Time.deltaTime * 10f;
		if (bodyAnimationEvalZ >= 1f)
		{
			bodyAnimationEvalZ = 0f;
		}
		float num3 = BodyAnimationCurve.Evaluate(bodyAnimationEvalZ);
		bodyAnimationEvalZ += Time.deltaTime * 2.5f;
		bodyTransformTarget.localRotation = Quaternion.Euler(num * 8f, num2 * 8f, num3 * 8f);
	}

	private float HealthToColor(int _syncedHealth)
	{
		float min = 30f;
		float num = 100f;
		float num2 = (float)maxHealth * 0.3f;
		if ((float)_syncedHealth <= num2)
		{
			return num;
		}
		return Mathf.Clamp(num2 + (float)maxHealth - Mathf.Clamp(_syncedHealth, num2, maxHealth), min, num);
	}

	public void OnHealthChanged(int _syncedHealth)
	{
		if (!hasBeenFull)
		{
			float currentValue = HealthToColor(_syncedHealth);
			float minValue = (float)maxHealth * 0.3f;
			foreach (Material instancedMaterial in controller.enemy.Health.instancedMaterials)
			{
				instancedMaterial.SetColor("_EmissionColor", SemiFunc.ColorDifficultyGet(minValue, maxHealth, currentValue));
			}
			if ((float)_syncedHealth >= (float)maxHealth * 0.1f)
			{
				if (flatHead.activeSelf)
				{
					flatHead.SetActive(value: false);
					bloatedHead.SetActive(value: true);
					springHead.lastPosition = new Vector3(1f, 0.5f, 1f);
					springHead.springVelocity = Vector3.zero;
					headTransformTarget.localScale = new Vector3(1f, 0.5f, 1f);
					headTransform.localScale = new Vector3(1f, 0.5f, 1f);
					if (headTransform.localScale.y == 0.5f)
					{
						headTransformTarget.localScale = Vector3.one;
					}
				}
			}
			else if ((float)_syncedHealth < (float)maxHealth * 0.1f && bloatedHead.activeSelf)
			{
				headTransformTarget.localScale = new Vector3(1f, 0.5f, 1f);
				flatHeadSwitch = true;
			}
			int num = Mathf.RoundToInt((float)maxHealth * 0.3f);
			if (_syncedHealth >= num)
			{
				int num2 = Mathf.CeilToInt((float)Mathf.Max(10, maxHealth - num) / 10f);
				int num3 = Mathf.Clamp((_syncedHealth - num) / 10, 0, num2);
				float t = ((num2 > 0) ? ((float)num3 / (float)num2) : 0f);
				float num4 = Mathf.Lerp(1f, maxScaleBloatParts, t);
				Vector3 localScale = new Vector3(num4, num4, num4);
				foreach (springBloatPart springBloatPart in springBloatParts)
				{
					springBloatPart.target.localScale = localScale;
				}
			}
			else if (!flatHead.activeSelf)
			{
				foreach (springBloatPart springBloatPart2 in springBloatParts)
				{
					springBloatPart2.target.localScale = bloatPartsOrginalScale;
				}
			}
		}
		if (_syncedHealth >= maxHealth && !hasBeenFull)
		{
			hasBeenFull = true;
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				enemy.Health.spawnValuable = true;
			}
		}
	}

	private void OverrideWalkAnimation()
	{
		overrideWalkAnimationTimer = 1f;
	}

	private void OverrideWalkAnimationTick()
	{
		if (overrideWalkAnimationTimer <= 0f)
		{
			bodyTransformTarget.localRotation = Quaternion.identity;
			isWalking = false;
		}
		if (overrideWalkAnimationTimer > 0f)
		{
			overrideWalkAnimationTimer -= Time.deltaTime;
			isWalking = true;
		}
	}
}
