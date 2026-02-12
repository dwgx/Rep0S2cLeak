using UnityEngine;

public class EnemyHunterAlwaysActive : MonoBehaviour
{
	private bool shootEffectActive;

	public PropLight shootLight;

	private float shootLightIntensity;

	private bool hitEffectActive;

	public PropLight hitLight;

	private float hitLightIntensity;

	private void Start()
	{
		shootLightIntensity = shootLight.lightComponent.intensity;
		hitLightIntensity = hitLight.lightComponent.intensity;
	}

	private void Update()
	{
		if (shootEffectActive)
		{
			shootLight.lightComponent.intensity -= Time.deltaTime * 20f;
			shootLight.originalIntensity = shootLightIntensity;
			if (shootLight.lightComponent.intensity <= 0f)
			{
				shootLight.lightComponent.enabled = false;
				shootEffectActive = false;
			}
		}
		if (hitEffectActive)
		{
			hitLight.lightComponent.intensity -= Time.deltaTime * 20f;
			hitLight.originalIntensity = hitLightIntensity;
			if (hitLight.lightComponent.intensity <= 0f)
			{
				hitLight.lightComponent.enabled = false;
				hitEffectActive = false;
			}
		}
	}

	public void Trigger()
	{
		shootEffectActive = true;
		hitEffectActive = true;
		shootLight.lightComponent.enabled = true;
		shootLight.lightComponent.intensity = shootLightIntensity;
		shootLight.originalIntensity = shootLightIntensity;
		hitLight.lightComponent.enabled = true;
		hitLight.lightComponent.intensity = hitLightIntensity;
		hitLight.originalIntensity = hitLightIntensity;
	}
}
