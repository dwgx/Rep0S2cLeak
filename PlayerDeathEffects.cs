using UnityEngine;

public class PlayerDeathEffects : MonoBehaviour
{
	private bool triggered;

	public PlayerAvatarVisuals playerAvatarVisuals;

	[Space]
	public Transform followTransform;

	public Transform enableTransform;

	[Space]
	public Light deathLight;

	private float deathLightIntensityDefault;

	public ParticleSystem smokeParticles;

	public ParticleSystem fireParticles;

	public ParticleSystem bitWeakParticles;

	public ParticleSystem bitStrongParticles;

	public HurtCollider hurtCollider;

	private float hurtColliderTime = 0.5f;

	private float hurtColliderTimer;

	[Space]
	public Sound deathSound;

	private void Start()
	{
		deathLightIntensityDefault = deathLight.intensity;
	}

	private void Update()
	{
		base.transform.position = followTransform.position;
		base.transform.rotation = followTransform.rotation;
		if (!triggered)
		{
			return;
		}
		deathLight.intensity = Mathf.Lerp(deathLight.intensity, 0f, Time.deltaTime * 1f);
		if (smokeParticles.isStopped && bitWeakParticles.isStopped && bitStrongParticles.isStopped && deathLight.intensity < 0.01f)
		{
			base.gameObject.SetActive(value: false);
		}
		if (hurtColliderTimer > 0f)
		{
			hurtColliderTimer -= Time.deltaTime;
			if (hurtColliderTimer <= 0f)
			{
				hurtCollider.gameObject.SetActive(value: false);
			}
		}
	}

	public void Trigger()
	{
		ParticleSystem.MainModule main = bitWeakParticles.main;
		main.startColor = playerAvatarVisuals.color;
		ParticleSystem.MainModule main2 = bitStrongParticles.main;
		main2.startColor = playerAvatarVisuals.color;
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		triggered = true;
		enableTransform.gameObject.SetActive(value: true);
		hurtCollider.gameObject.SetActive(value: true);
		hurtColliderTimer = hurtColliderTime;
		deathSound.Play(base.transform.position);
		smokeParticles.gameObject.SetActive(value: true);
		smokeParticles.Play();
		fireParticles.gameObject.SetActive(value: true);
		fireParticles.Play();
		bitWeakParticles.gameObject.SetActive(value: true);
		bitWeakParticles.Play();
		bitStrongParticles.gameObject.SetActive(value: true);
		bitStrongParticles.Play();
	}

	public void Reset()
	{
		base.gameObject.SetActive(value: true);
		triggered = false;
		deathLight.intensity = deathLightIntensityDefault;
		enableTransform.gameObject.SetActive(value: false);
	}
}
