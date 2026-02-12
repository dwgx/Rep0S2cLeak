using UnityEngine;

public class ParticleScriptExplosion : MonoBehaviour
{
	public ExplosionPreset explosionPreset;

	private GameObject explosionPrefab;

	private void Start()
	{
		explosionPrefab = Resources.Load<GameObject>("Effects/Part Prefab Explosion");
	}

	public void PlayExplosionSoundSmall(Vector3 _position)
	{
		explosionPreset.explosionSoundSmall.Play(_position);
		explosionPreset.explosionSoundSmallGlobal.Play(_position);
	}

	public void PlayExplosionSoundMedium(Vector3 _position)
	{
		explosionPreset.explosionSoundMedium.Play(_position);
		explosionPreset.explosionSoundMediumGlobal.Play(_position);
	}

	public void PlayExplosionSoundBig(Vector3 _position)
	{
		explosionPreset.explosionSoundBig.Play(_position);
		explosionPreset.explosionSoundBigGlobal.Play(_position);
	}

	public ParticlePrefabExplosion Spawn(Vector3 position, float size, int damage, int enemyDamage, float forceMulti = 1f, bool onlyParticleEffect = false, bool disableSound = false, float shakeMultiplier = 1f)
	{
		if (size < 0.25f)
		{
			if (!disableSound)
			{
				explosionPreset.explosionSoundSmall.Play(position);
				explosionPreset.explosionSoundSmallGlobal.Play(position);
			}
			if (shakeMultiplier != 0f)
			{
				GameDirector.instance.CameraImpact.ShakeDistance(3f * shakeMultiplier, 3f, 6f, base.transform.position, 0.2f);
				GameDirector.instance.CameraShake.ShakeDistance(3f * shakeMultiplier, 3f, 6f, base.transform.position, 0.5f);
			}
		}
		else if (size < 0.5f)
		{
			if (!disableSound)
			{
				explosionPreset.explosionSoundMedium.Play(position);
				explosionPreset.explosionSoundMediumGlobal.Play(position);
			}
			if (shakeMultiplier != 0f)
			{
				GameDirector.instance.CameraImpact.ShakeDistance(5f * shakeMultiplier, 4f, 8f, base.transform.position, 0.2f);
				GameDirector.instance.CameraShake.ShakeDistance(5f * shakeMultiplier, 4f, 8f, base.transform.position, 0.5f);
			}
		}
		else
		{
			if (!disableSound)
			{
				explosionPreset.explosionSoundBig.Play(position);
				explosionPreset.explosionSoundBigGlobal.Play(position);
			}
			if (shakeMultiplier != 0f)
			{
				GameDirector.instance.CameraImpact.ShakeDistance(10f * shakeMultiplier, 6f, 12f, base.transform.position, 0.2f);
				GameDirector.instance.CameraShake.ShakeDistance(5f * shakeMultiplier, 6f, 12f, base.transform.position, 0.5f);
			}
		}
		ParticlePrefabExplosion component = Object.Instantiate(explosionPrefab, position, Quaternion.identity).GetComponent<ParticlePrefabExplosion>();
		component.forceMultiplier = explosionPreset.explosionForceMultiplier * forceMulti;
		component.explosionSize = size;
		component.explosionDamage = damage;
		component.explosionDamageEnemy = enemyDamage;
		component.lightColorOverTime = explosionPreset.lightColor;
		ParticleSystem.ColorOverLifetimeModule colorOverLifetime = component.particleFire.colorOverLifetime;
		colorOverLifetime.color = explosionPreset.explosionColors;
		colorOverLifetime = component.particleSmoke.colorOverLifetime;
		colorOverLifetime.color = explosionPreset.smokeColors;
		component.particleFire.Play();
		component.particleSmoke.Play();
		component.light.enabled = true;
		return component;
	}
}
