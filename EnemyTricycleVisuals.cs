using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTricycleVisuals : MonoBehaviour
{
	[Serializable]
	public class TricycleWheel
	{
		public string name;

		public Transform transformShake;

		public Transform transformRotation;

		[HideInInspector]
		public Quaternion originalRotation;

		[HideInInspector]
		public float currentRotation;

		private SpringQuaternion shakeSpring;

		private Quaternion shakeTarget = Quaternion.identity;

		private float shakeIntensity;

		private float shakeFrequency;

		private float shakeTimer;

		private float noiseSeedX;

		private float noiseSeedY;

		private float noiseSeedZ;

		public void Initialize(float springSpeed, float springDamping)
		{
			originalRotation = transformRotation.localRotation;
			shakeSpring = new SpringQuaternion();
			shakeSpring.speed = springSpeed;
			shakeSpring.damping = springDamping;
			shakeSpring.maxAngle = 15f;
			shakeSpring.clamp = false;
			noiseSeedX = UnityEngine.Random.value * 10f;
			noiseSeedY = UnityEngine.Random.value * 10f;
			noiseSeedZ = UnityEngine.Random.value * 10f;
		}

		public void Update(float rotationSpeed, float deltaTime)
		{
			shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, deltaTime * 10f);
			shakeFrequency = Mathf.Lerp(shakeFrequency, 0f, deltaTime * 5f);
			if (shakeIntensity > 0.01f)
			{
				float y = Time.time * Mathf.Max(0.01f, shakeFrequency);
				float x = Mathf.PerlinNoise(noiseSeedX, y) * 2f - 1f;
				float y2 = Mathf.PerlinNoise(noiseSeedY, y) * 2f - 1f;
				float z = Mathf.PerlinNoise(noiseSeedZ, y) * 2f - 1f;
				Vector3 euler = new Vector3(x, y2, z) * (shakeIntensity * 5f);
				shakeTarget = Quaternion.Euler(euler);
			}
			else
			{
				shakeTarget = Quaternion.identity;
			}
			transformShake.localRotation = SemiFunc.SpringQuaternionGet(shakeSpring, shakeTarget);
			currentRotation += rotationSpeed * deltaTime;
			Quaternion quaternion = Quaternion.Euler(currentRotation, 0f, 0f);
			transformRotation.localRotation = originalRotation * quaternion;
		}

		public void ApplyShakeImpulse(float intensity, float frequency = 10f)
		{
			shakeIntensity = Mathf.Max(shakeIntensity, intensity);
			shakeFrequency = Mathf.Max(shakeFrequency, frequency);
			if (shakeTimer <= 0f)
			{
				shakeTimer = 0.01f;
			}
		}
	}

	[Serializable]
	public class TricycleHandlebars
	{
		public Transform transform;

		public Transform transformShake;

		public Transform handlebar1GrabPoint;

		public Transform handlebar2GrabPoint;

		public Transform handlebarPointTransform;

		[HideInInspector]
		public Vector3 previousPosition;

		[HideInInspector]
		public Vector3 handlebarPreviousPosition;

		[HideInInspector]
		public Vector3 handlebarVelocity;

		[HideInInspector]
		public float velocity;

		[HideInInspector]
		public Quaternion originalRotation;

		private SpringQuaternion steeringSpring;

		private SpringQuaternion shakeSpring;

		private Quaternion shakeTarget = Quaternion.identity;

		private float shakeIntensity;

		private float shakeFrequency;

		private float shakeTimer;

		private float noiseSeedX;

		private float noiseSeedY;

		private float noiseSeedZ;

		private float currentSteeringAngle;

		private float targetSteeringAngle;

		public void Initialize()
		{
			previousPosition = transform.position;
			originalRotation = transform.localRotation;
			handlebarPreviousPosition = handlebarPointTransform.position;
			steeringSpring = new SpringQuaternion();
			steeringSpring.speed = 10f;
			steeringSpring.damping = 0.6f;
			steeringSpring.maxAngle = 80f;
			steeringSpring.clamp = true;
			shakeSpring = new SpringQuaternion();
			shakeSpring.speed = 20f;
			shakeSpring.damping = 0.5f;
			shakeSpring.maxAngle = 30f;
			shakeSpring.clamp = false;
			noiseSeedX = UnityEngine.Random.value * 10f;
			noiseSeedY = UnityEngine.Random.value * 10f;
			noiseSeedZ = UnityEngine.Random.value * 10f;
		}

		public void Update(float deltaTime, Transform tricycleTransform, Vector3 tricycleVelocity)
		{
			velocity = Vector3.Distance(transform.position, previousPosition) / deltaTime;
			previousPosition = transform.position;
			handlebarVelocity = (handlebarPointTransform.position - handlebarPreviousPosition) / deltaTime;
			handlebarPreviousPosition = handlebarPointTransform.position;
			if (handlebarVelocity.magnitude > 0.1f)
			{
				float value = Vector3.SignedAngle(tricycleTransform.forward, handlebarVelocity.normalized, Vector3.up);
				targetSteeringAngle = Mathf.Clamp(value, -45f, 45f);
			}
			else
			{
				targetSteeringAngle = 0f;
			}
			shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, deltaTime * 10f);
			shakeFrequency = Mathf.Lerp(shakeFrequency, 0f, deltaTime * 5f);
			if (shakeIntensity > 0.01f)
			{
				float y = Time.time * Mathf.Max(0.01f, shakeFrequency);
				float x = Mathf.PerlinNoise(noiseSeedX, y) * 2f - 1f;
				float y2 = Mathf.PerlinNoise(noiseSeedY, y) * 2f - 1f;
				float z = Mathf.PerlinNoise(noiseSeedZ, y) * 2f - 1f;
				Vector3 euler = new Vector3(x, y2, z) * (shakeIntensity * 3f);
				shakeTarget = Quaternion.Euler(euler);
			}
			else
			{
				shakeTarget = Quaternion.identity;
			}
			transformShake.localRotation = SemiFunc.SpringQuaternionGet(shakeSpring, shakeTarget);
			currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, targetSteeringAngle, deltaTime * 8f);
			Quaternion targetRotation = Quaternion.Euler(0f, currentSteeringAngle, 0f);
			Quaternion quaternion = SemiFunc.SpringQuaternionGet(steeringSpring, targetRotation);
			transform.localRotation = originalRotation * quaternion;
		}

		public void ApplyShakeImpulse(float intensity, float frequency = 10f)
		{
			shakeIntensity = Mathf.Max(shakeIntensity, intensity);
			shakeFrequency = Mathf.Max(shakeFrequency, frequency);
			if (shakeTimer <= 0f)
			{
				shakeTimer = 0.01f;
			}
		}
	}

	[Header("Components")]
	public List<TricycleWheel> wheels = new List<TricycleWheel>();

	public TricycleHandlebars handlebars;

	public Transform bodyTransform;

	public Transform bellTransform;

	public Transform pedalCrankTransform;

	public Transform pedal1Transform;

	public Transform pedal2Transform;

	[Header("Limb Targets")]
	public Transform hand1Transform;

	public Transform hand2Transform;

	public Transform foot1Transform;

	public Transform foot2Transform;

	public BotSystemSpringPoseAnimator botSystemSpringPoseAnimator;

	[Header("Wheel Settings")]
	public float wheelShakeSpringSpeed = 20f;

	public float wheelShakeSpringDamping = 0.5f;

	[Header("Body Shake")]
	private SpringQuaternion bodyShakeSpring;

	private Quaternion bodyShakeTarget = Quaternion.identity;

	private float bodyShakeIntensity;

	private float bodyShakeFrequency;

	private float bodyShakeTimer;

	private float bodyNoiseSeedX;

	private float bodyNoiseSeedY;

	private float bodyNoiseSeedZ;

	[Header("Bell")]
	private SpringFloat bellSpring = new SpringFloat();

	private float bellTarget;

	private float wheelRotationSpeed;

	private float wheelRotationSpeedTarget;

	private SpringFloat wheelRotationSpeedSpring = new SpringFloat();

	private Vector3 _prevTrikePosForHB;

	private float pedal1OriginalXRotation;

	private float pedal2OriginalXRotation;

	private float limbStretchingOverrideTimer;

	private float handsLetGoTimer;

	private float feetLetGoTimer;

	private void Start()
	{
		InitializeWheels();
		InitializeHandlebars();
		InitializeBodyShake();
		InitializeBell();
		InitializePedals();
		_prevTrikePosForHB = base.transform.position;
	}

	private void Update()
	{
		UpdateWheelRotationSpeed();
		UpdateWheels();
		UpdateHandlebars();
		UpdateBodyShake();
		UpdateBell();
		UpdatePedals();
		if (limbStretchingOverrideTimer <= 0f)
		{
			UpdateLimbStretching();
		}
		if (limbStretchingOverrideTimer > 0f)
		{
			limbStretchingOverrideTimer -= Time.deltaTime;
		}
		if (handsLetGoTimer > 0f)
		{
			handsLetGoTimer -= Time.deltaTime;
		}
		if (feetLetGoTimer > 0f)
		{
			feetLetGoTimer -= Time.deltaTime;
		}
	}

	private void InitializeWheels()
	{
		foreach (TricycleWheel wheel in wheels)
		{
			wheel.Initialize(wheelShakeSpringSpeed, wheelShakeSpringDamping);
		}
	}

	private void InitializeHandlebars()
	{
		handlebars.Initialize();
	}

	private void InitializeBodyShake()
	{
		bodyShakeSpring = new SpringQuaternion();
		bodyShakeSpring.speed = 15f;
		bodyShakeSpring.damping = 0.6f;
		bodyShakeSpring.maxAngle = 20f;
		bodyShakeSpring.clamp = false;
		bodyNoiseSeedX = UnityEngine.Random.value * 10f;
		bodyNoiseSeedY = UnityEngine.Random.value * 10f;
		bodyNoiseSeedZ = UnityEngine.Random.value * 10f;
	}

	private void InitializeBell()
	{
		bellSpring.speed = 55f;
		bellSpring.damping = 0.1f;
	}

	private void InitializePedals()
	{
		pedal1OriginalXRotation = pedal1Transform.localEulerAngles.x;
		pedal2OriginalXRotation = pedal2Transform.localEulerAngles.x;
	}

	private void UpdateWheelRotationSpeed()
	{
		wheelRotationSpeed = SemiFunc.SpringFloatGet(wheelRotationSpeedSpring, wheelRotationSpeedTarget);
		wheelRotationSpeedTarget = Mathf.Lerp(wheelRotationSpeedTarget, 0f, Time.deltaTime * 5f);
	}

	private void UpdateWheels()
	{
		foreach (TricycleWheel wheel in wheels)
		{
			wheel.Update(wheelRotationSpeed, Time.deltaTime);
		}
	}

	private void UpdateHandlebars()
	{
		Vector3 tricycleVelocity = (base.transform.position - _prevTrikePosForHB) / Mathf.Max(1E-06f, Time.deltaTime);
		_prevTrikePosForHB = base.transform.position;
		handlebars.Update(Time.deltaTime, base.transform, tricycleVelocity);
	}

	private void UpdateBodyShake()
	{
		bodyShakeIntensity = Mathf.Lerp(bodyShakeIntensity, 0f, Time.deltaTime * 8f);
		bodyShakeFrequency = Mathf.Lerp(bodyShakeFrequency, 0f, Time.deltaTime * 5f);
		if (bodyShakeIntensity > 0.01f)
		{
			float y = Time.time * Mathf.Max(0.01f, bodyShakeFrequency);
			float x = Mathf.PerlinNoise(bodyNoiseSeedX, y) * 2f - 1f;
			float y2 = Mathf.PerlinNoise(bodyNoiseSeedY, y) * 2f - 1f;
			float z = Mathf.PerlinNoise(bodyNoiseSeedZ, y) * 2f - 1f;
			Vector3 euler = new Vector3(x, y2, z) * bodyShakeIntensity;
			bodyShakeTarget = Quaternion.Euler(euler);
		}
		else
		{
			bodyShakeTarget = Quaternion.identity;
		}
		bodyTransform.localRotation = SemiFunc.SpringQuaternionGet(bodyShakeSpring, bodyShakeTarget);
	}

	private void UpdateBell()
	{
		float z = SemiFunc.SpringFloatGet(bellSpring, bellTarget);
		bellTransform.localRotation = Quaternion.Euler(0f, 0f, z);
	}

	private void UpdatePedals()
	{
		pedalCrankTransform.localRotation = wheels[0].transformRotation.localRotation;
		float num = ((wheels.Count > 0) ? wheels[0].currentRotation : 0f);
		pedal1Transform.localRotation = Quaternion.Euler(0f - num, 0f, 0f) * Quaternion.Euler(pedal1OriginalXRotation, 0f, 0f);
		num = ((wheels.Count > 0) ? wheels[0].currentRotation : 0f);
		pedal2Transform.localRotation = Quaternion.Euler(0f - num, 0f, 0f) * Quaternion.Euler(pedal2OriginalXRotation, 0f, 0f);
	}

	private void UpdateLimbStretching()
	{
		if (handsLetGoTimer <= 0f)
		{
			botSystemSpringPoseAnimator.StretchLimbToPoint("leftarm", hand1Transform.position);
			botSystemSpringPoseAnimator.StretchLimbToPoint("rightarm", hand2Transform.position);
		}
		if (feetLetGoTimer <= 0f)
		{
			botSystemSpringPoseAnimator.StretchLimbToPoint("rightleg", foot1Transform.position);
			botSystemSpringPoseAnimator.StretchLimbToPoint("leftleg", foot2Transform.position);
		}
	}

	public void ImpulseWheelRotation(float speed)
	{
		wheelRotationSpeedTarget = Mathf.Max(wheelRotationSpeedTarget, speed);
	}

	public void ImpulseWheelShake(float intensity, float frequency = 10f)
	{
		foreach (TricycleWheel wheel in wheels)
		{
			wheel.ApplyShakeImpulse(intensity, frequency);
		}
	}

	public void ImpulseHandlebarShake(float intensity, float frequency = 10f)
	{
		handlebars.ApplyShakeImpulse(intensity, frequency);
	}

	public void ImpulseBodyShake(float intensity, float frequency = 10f)
	{
		bodyShakeIntensity = Mathf.Max(bodyShakeIntensity, intensity);
		bodyShakeFrequency = Mathf.Max(bodyShakeFrequency, frequency);
		if (bodyShakeTimer <= 0f)
		{
			bodyShakeTimer = 0.01f;
		}
	}

	public void ImpulseWheelShakeSingle(int wheelIndex, float intensity, float frequency = 10f)
	{
		if (wheelIndex >= 0 && wheelIndex < wheels.Count)
		{
			wheels[wheelIndex].ApplyShakeImpulse(intensity, frequency);
		}
	}

	public void ImpulseBellRing()
	{
		bellSpring.springVelocity += 2000f;
	}

	public void ImpulseImpact(float intensity = 10f)
	{
		bodyShakeIntensity = intensity;
		bodyShakeFrequency = 25f;
		if (bodyShakeTimer <= 0f)
		{
			bodyShakeTimer = 0.01f;
		}
		foreach (TricycleWheel wheel in wheels)
		{
			wheel.ApplyShakeImpulse(intensity * 0.8f, 30f);
		}
		handlebars.ApplyShakeImpulse(intensity * 1.2f, 28f);
	}

	public void ImpulseAttackImpact()
	{
		ImpulseImpact(8f);
	}

	public void DeactivateLimbStretching(float duration)
	{
		limbStretchingOverrideTimer = duration;
	}

	public void HandsLetGo(float duration)
	{
		handsLetGoTimer = duration;
	}

	public void FeetLetGo(float duration)
	{
		feetLetGoTimer = duration;
	}
}
