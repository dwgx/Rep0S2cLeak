using System.Collections.Generic;
using UnityEngine;

public class EnemySlowMouthParticlePukeCollision : MonoBehaviour
{
	public Light pukeLight;

	private ParticleSystem pukeParticles;

	public GameObject hurtCollider;

	private float hurtColliderTimer;

	private Transform parentTransform;

	private Vector3 startPosition;

	public ParticleSystem pukeBubbleParticles;

	public ParticleSystem pukeSplashParticles;

	public ParticleSystem pukeSmokeParticles;

	private void Start()
	{
		pukeParticles = GetComponent<ParticleSystem>();
		parentTransform = base.transform.parent;
		startPosition = base.transform.localPosition;
	}

	private void Update()
	{
		if (pukeParticles.isPlaying)
		{
			if (!pukeLight.enabled)
			{
				pukeLight.enabled = true;
				pukeLight.intensity = 0f;
			}
			pukeLight.intensity = Mathf.Lerp(pukeLight.intensity, 0.6f, Time.deltaTime * 5f);
			pukeLight.intensity += Mathf.Sin(Time.time * 40f) * 0.07f;
		}
		else if (pukeLight.enabled)
		{
			pukeLight.intensity = Mathf.Lerp(pukeLight.intensity, 0f, Time.deltaTime * 1f);
			pukeLight.intensity += Mathf.Sin(Time.time * 30f) * 0.01f;
			if (pukeLight.intensity < 0.01f)
			{
				pukeLight.enabled = false;
			}
		}
		if (hurtColliderTimer > 0f)
		{
			hurtColliderTimer -= Time.deltaTime;
			if (hurtColliderTimer <= 0f)
			{
				hurtCollider.SetActive(value: false);
			}
		}
		if (!SemiFunc.FPSImpulse15())
		{
			return;
		}
		base.transform.localPosition = startPosition;
		float num = Vector3.Distance(parentTransform.position, base.transform.position);
		if (Physics.Raycast(parentTransform.position, num * parentTransform.forward, out var hitInfo, num, SemiFunc.LayerMaskGetVisionObstruct()))
		{
			Vector3 point = hitInfo.point;
			if (Vector3.Distance(parentTransform.position, point) < num)
			{
				Vector3 vector = parentTransform.InverseTransformPoint(point);
				base.transform.localPosition = new Vector3(0f, 0f, vector.z);
			}
		}
	}

	private void ActivateHurtCollider(Vector3 _direction, Vector3 _position)
	{
		pukeSmokeParticles.transform.position = _position;
		pukeSmokeParticles.transform.rotation = Quaternion.LookRotation(_direction);
		pukeSmokeParticles.Emit(1);
		hurtCollider.SetActive(value: true);
		pukeBubbleParticles.transform.position = _position;
		pukeBubbleParticles.transform.rotation = Quaternion.LookRotation(_direction);
		pukeBubbleParticles.Emit(2);
		pukeSplashParticles.transform.position = _position;
		pukeSplashParticles.Emit(3);
		hurtCollider.transform.rotation = Quaternion.LookRotation(_direction);
		hurtCollider.transform.position = _position;
		hurtColliderTimer = 0.2f;
		_direction.y += 180f;
		pukeSplashParticles.transform.rotation = Quaternion.LookRotation(_direction);
	}

	private void OnParticleCollision(GameObject other)
	{
		List<ParticleCollisionEvent> list = new List<ParticleCollisionEvent>();
		int collisionEvents = pukeParticles.GetCollisionEvents(other, list);
		for (int i = 0; i < collisionEvents; i++)
		{
			ParticleCollisionEvent particleCollisionEvent = list[i];
			Vector3 intersection = particleCollisionEvent.intersection;
			Vector3 velocity = particleCollisionEvent.velocity;
			Vector3 normalized = velocity.normalized;
			if (velocity.magnitude > 3f)
			{
				ActivateHurtCollider(normalized, intersection);
			}
		}
	}
}
