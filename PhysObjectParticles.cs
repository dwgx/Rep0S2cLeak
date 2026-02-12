using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysObjectParticles : MonoBehaviour
{
	public Gradient gradient;

	public ParticleSystem particleSystemBits;

	public ParticleSystem particleSystemBitsSmall;

	public ParticleSystem particleSystemSmoke;

	private ParticleSystem.Particle[] particles;

	private bool particlesSpawned;

	private PhysGrabObject physGrabObject;

	internal float multiplier = 1f;

	public List<Transform> colliderTransforms = new List<Transform>();

	private void Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
	}

	public void DestroyParticles()
	{
		if (!SemiFunc.RunIsTutorial())
		{
			StartCoroutine(DestroyParticlesAfterTime(4f));
		}
		foreach (Transform colliderTransform2 in colliderTransforms)
		{
			Vector3 localScale = colliderTransform2.localScale;
			Vector3 eulerAngles = colliderTransform2.rotation.eulerAngles;
			Transform colliderTransform = colliderTransform2;
			int value = (int)(localScale.x * 100f * (localScale.y * 100f) * (localScale.z * 100f) / 1000f);
			value = (int)((float)Mathf.Clamp(value, 10, 150) * multiplier);
			float num = localScale.x * 100f * (localScale.y * 100f) * (localScale.z * 100f) / 30000f;
			if ((bool)colliderTransform2.GetComponent<SphereCollider>())
			{
				num *= 0.55f;
				localScale *= 0.4f;
			}
			num = Mathf.Clamp(num, 0.3f, 2f);
			SpawnParticles(value, num * multiplier, localScale * multiplier, eulerAngles, colliderTransform);
		}
	}

	private IEnumerator DestroyParticlesAfterTime(float time)
	{
		yield return new WaitForSeconds(time);
		Object.Destroy(base.gameObject);
	}

	public void ImpactSmoke(int amount, Vector3 position, float size)
	{
		size = Mathf.Clamp(size, 0.7f, 1.5f);
		Vector3 localPosition = base.transform.InverseTransformPoint(position);
		particleSystemSmoke.transform.localPosition = localPosition;
		ParticleSystem.MainModule main = particleSystemSmoke.main;
		ParticleSystem.ShapeModule shape = particleSystemSmoke.shape;
		float startSizeMultiplier = main.startSizeMultiplier;
		shape.scale = new Vector3(0.2f, 0.2f, 0.2f);
		float max = Mathf.Clamp(size / 4f, 0f, 2f);
		main.startSpeed = new ParticleSystem.MinMaxCurve(0f, max);
		main.startSizeXMultiplier *= size;
		main.startSizeYMultiplier *= size;
		main.startSizeZMultiplier *= size;
		particleSystemSmoke.Emit(amount);
		main.startSizeXMultiplier = startSizeMultiplier;
		main.startSizeYMultiplier = startSizeMultiplier;
		main.startSizeZMultiplier = startSizeMultiplier;
	}

	private void SpawnParticles(int bitCount, float size, Vector3 colliderScale, Vector3 colliderRotation, Transform colliderTransform)
	{
		_ = Vector3.left * 5f;
		particleSystemBits.transform.position = colliderTransform.position;
		particleSystemBitsSmall.transform.position = colliderTransform.position;
		particleSystemSmoke.transform.position = colliderTransform.position;
		ParticleSystem.ShapeModule shape = particleSystemBits.shape;
		ParticleSystem.MainModule main = particleSystemBits.main;
		main.startSizeXMultiplier *= size;
		main.startSizeYMultiplier *= size;
		main.startSizeZMultiplier *= size;
		shape.scale = colliderScale;
		shape.rotation = colliderRotation;
		main = particleSystemBitsSmall.main;
		main.startSizeXMultiplier *= size;
		main.startSizeYMultiplier *= size;
		main.startSizeZMultiplier *= size;
		shape = particleSystemBitsSmall.shape;
		shape.scale = colliderScale;
		shape.rotation = colliderRotation;
		main = particleSystemSmoke.main;
		shape = particleSystemSmoke.shape;
		float startSizeMultiplier = main.startSizeMultiplier;
		main.startSizeXMultiplier *= Mathf.Clamp(size, 0.5f, 1.5f);
		main.startSizeYMultiplier *= Mathf.Clamp(size, 0.5f, 1.5f);
		main.startSizeZMultiplier *= Mathf.Clamp(size, 0.5f, 1.5f);
		float max = Mathf.Clamp(size, 0.5f, 2f);
		main.startSpeed = new ParticleSystem.MinMaxCurve(0f, max);
		shape.scale = colliderScale;
		shape.rotation = colliderRotation;
		particleSystemBits.Emit(bitCount);
		particleSystemBitsSmall.Emit(bitCount / 3);
		particleSystemSmoke.Emit(bitCount);
		particlesSpawned = true;
		main = particleSystemBits.main;
		main.startSizeXMultiplier /= size;
		main.startSizeYMultiplier /= size;
		main.startSizeZMultiplier /= size;
		main = particleSystemBitsSmall.main;
		main.startSizeXMultiplier /= size;
		main.startSizeYMultiplier /= size;
		main.startSizeZMultiplier /= size;
		main = particleSystemSmoke.main;
		main.startSizeXMultiplier = startSizeMultiplier;
		main.startSizeYMultiplier = startSizeMultiplier;
		main.startSizeZMultiplier = startSizeMultiplier;
	}

	private void LateUpdate()
	{
		if (particlesSpawned)
		{
			int maxParticles = particleSystemBits.main.maxParticles;
			if (particles == null || particles.Length < maxParticles)
			{
				particles = new ParticleSystem.Particle[maxParticles];
			}
			int num = particleSystemBits.GetParticles(particles);
			for (int i = 0; i < num; i++)
			{
				float time = (float)i / (float)num;
				Color color = gradient.Evaluate(time);
				particles[i].startColor = color;
			}
			particleSystemBits.SetParticles(particles, num);
			maxParticles = particleSystemBitsSmall.main.maxParticles;
			if (particles == null || particles.Length < maxParticles)
			{
				particles = new ParticleSystem.Particle[maxParticles];
			}
			num = particleSystemBitsSmall.GetParticles(particles);
			for (int j = 0; j < num; j++)
			{
				float time2 = (float)j / (float)num;
				Color color2 = gradient.Evaluate(time2);
				particles[j].startColor = color2;
			}
			particleSystemBitsSmall.SetParticles(particles, num);
			particlesSpawned = false;
		}
	}
}
