using System;
using UnityEngine;

public class AnimalCrateTrap : Trap
{
	private PhysGrabObject physgrabobject;

	[Space]
	[Header("Animal Crate Components")]
	public GameObject Crate;

	public Transform Center;

	private Material material;

	[Space]
	[Header("Sounds")]
	public Sound tinyBump;

	public Sound smallBump;

	public Sound mediumBump;

	public Sound bigBump;

	public Sound berzerk;

	[Space]
	private Quaternion initialAnimalCrateRotation;

	private Rigidbody rb;

	private ParticleScriptExplosion particleScriptExplosion;

	public ParticleSystem particleBitsTiny;

	public ParticleSystem particleBitsSmall;

	public ParticleSystem particleBitsMedium;

	public ParticleSystem particleBitsBig;

	private Vector3 randomTorque;

	private float timeToPound = 1f;

	[Space]
	[Header("Pound Animation")]
	public AnimationCurve poundCurve;

	private bool poundActive;

	public float poundSpeed;

	public float poundIntensity;

	private float poundIntensityMuilplier;

	private float poundLerp;

	private float shakeAmplitudeMultiplier;

	private float shakeFrequencyMultiplier;

	protected override void Start()
	{
		base.Start();
		initialAnimalCrateRotation = Crate.transform.localRotation;
		rb = GetComponent<Rigidbody>();
		physgrabobject = GetComponent<PhysGrabObject>();
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		material = Crate.GetComponent<Renderer>().material;
	}

	protected override void Update()
	{
		base.Update();
		berzerk.PlayLoop(poundActive, 1f, 1f);
		if (physgrabobject.grabbed)
		{
			TrapActivate();
		}
	}

	private void FixedUpdate()
	{
		if (poundActive)
		{
			if (shakeFrequencyMultiplier > 5f)
			{
				material.EnableKeyword("_EMISSION");
			}
			GameDirector.instance.CameraImpact.ShakeDistance(0.75f * poundIntensityMuilplier, 1f, 6f, base.transform.position, 0.01f * poundIntensityMuilplier);
			float num = shakeAmplitudeMultiplier * (1f - poundLerp);
			float num2 = shakeFrequencyMultiplier * (1f - poundLerp);
			float x = num * Mathf.Sin(Time.time * num2);
			float z = num * Mathf.Sin(Time.time * num2 + MathF.PI / 2f);
			Crate.transform.localRotation = initialAnimalCrateRotation * Quaternion.Euler(x, 0f, z);
			poundLerp += poundSpeed / poundIntensityMuilplier * Time.deltaTime;
			if (poundLerp >= 1f)
			{
				poundLerp = 0f;
				material.DisableKeyword("_EMISSION");
				poundActive = false;
			}
			Crate.transform.localScale = new Vector3(1f + poundCurve.Evaluate(poundLerp) * (poundIntensity * poundIntensityMuilplier), 1f + poundCurve.Evaluate(poundLerp) * (poundIntensity * poundIntensityMuilplier), 1f + poundCurve.Evaluate(poundLerp) * (poundIntensity * poundIntensityMuilplier));
		}
		if (trapActive && Vector3.Dot(base.transform.up, Vector3.up) < 0.5f)
		{
			if (timeToPound > 0f)
			{
				timeToPound -= Time.deltaTime;
				return;
			}
			poundActive = true;
			BigBump();
			timeToPound = 1f;
			physgrabobject.lightBreakImpulse = true;
		}
	}

	public void TinyBump()
	{
		particleBitsTiny.Play();
		tinyBump.Play(physgrabobject.centerPoint);
		shakeAmplitudeMultiplier = 0.2f;
		shakeFrequencyMultiplier = 5f;
		poundIntensityMuilplier = 1.5f;
		enemyInvestigate = true;
		poundActive = true;
		if (isLocal)
		{
			float num = UnityEngine.Random.Range(0.05f, 0.2f);
			rb.AddForce(Vector3.up * num, ForceMode.Impulse);
			Vector3 normalized = UnityEngine.Random.insideUnitSphere.normalized;
			rb.AddTorque(normalized * 2f, ForceMode.Impulse);
		}
	}

	public void SmallBump()
	{
		particleBitsSmall.Play();
		smallBump.Play(physgrabobject.centerPoint);
		shakeAmplitudeMultiplier = 0.5f;
		shakeFrequencyMultiplier = 10f;
		poundIntensityMuilplier = 2f;
		enemyInvestigate = true;
		poundActive = true;
		if (isLocal)
		{
			float num = UnityEngine.Random.Range(0.1f, 0.5f);
			rb.AddForce(Vector3.up * num, ForceMode.Impulse);
			Vector3 normalized = UnityEngine.Random.insideUnitSphere.normalized;
			rb.AddTorque(normalized * 3f, ForceMode.Impulse);
		}
	}

	public void MediumBump()
	{
		particleBitsMedium.Play();
		mediumBump.Play(physgrabobject.centerPoint);
		shakeAmplitudeMultiplier = 1f;
		shakeFrequencyMultiplier = 20f;
		poundIntensityMuilplier = 3f;
		enemyInvestigate = true;
		poundActive = true;
		if (isLocal)
		{
			float num = UnityEngine.Random.Range(0.3f, 1f);
			rb.AddForce(Vector3.up * num, ForceMode.Impulse);
			Vector3 normalized = UnityEngine.Random.insideUnitSphere.normalized;
			rb.AddTorque(normalized * 5f, ForceMode.Impulse);
		}
	}

	public void BigBump()
	{
		particleBitsBig.Play();
		bigBump.Play(physgrabobject.centerPoint);
		shakeAmplitudeMultiplier = 2f;
		shakeFrequencyMultiplier = 20f;
		poundIntensityMuilplier = 4f;
		enemyInvestigate = true;
		poundActive = true;
		if (isLocal)
		{
			float num = UnityEngine.Random.Range(0.8f, 2f);
			rb.AddForce(Vector3.up * num, ForceMode.Impulse);
			Vector3 normalized = UnityEngine.Random.insideUnitSphere.normalized;
			rb.AddTorque(normalized * 6f, ForceMode.Impulse);
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

	public void TrapStop()
	{
		particleScriptExplosion.Spawn(Center.position, 1f, 50, 300);
	}
}
