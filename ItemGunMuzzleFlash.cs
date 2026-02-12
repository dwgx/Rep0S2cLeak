using System.Collections;
using UnityEngine;

public class ItemGunMuzzleFlash : MonoBehaviour
{
	private ParticleSystem smoke;

	private ParticleSystem impact;

	private ParticleSystem sparks;

	private Light shootLight;

	public void ActivateAllEffects()
	{
		base.gameObject.SetActive(value: true);
		smoke = base.transform.Find("Particle Smoke").GetComponent<ParticleSystem>();
		impact = base.transform.Find("Particle Impact").GetComponent<ParticleSystem>();
		sparks = base.transform.Find("Particle Sparks").GetComponent<ParticleSystem>();
		shootLight = GetComponentInChildren<Light>();
		smoke.gameObject.SetActive(value: true);
		impact.gameObject.SetActive(value: true);
		sparks.gameObject.SetActive(value: true);
		shootLight.enabled = true;
		smoke.Play();
		impact.Play();
		sparks.Play();
		StartCoroutine(MuzzleFlashDestroy());
	}

	private IEnumerator MuzzleFlashDestroy()
	{
		yield return new WaitForSeconds(0.1f);
		while (smoke.isPlaying || impact.isPlaying || sparks.isPlaying || shootLight.enabled)
		{
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		if ((bool)shootLight)
		{
			shootLight.intensity = Mathf.Lerp(shootLight.intensity, 0f, Time.deltaTime * 10f);
			if (shootLight.intensity < 0.01f)
			{
				shootLight.enabled = false;
			}
		}
	}
}
