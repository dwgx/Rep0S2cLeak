using UnityEngine;

public class LightFlickerer : MonoBehaviour
{
	private Light lightComponent;

	private float intensityTarget;

	private float timer;

	private float intensity;

	private void Start()
	{
		lightComponent = GetComponent<Light>();
		intensity = lightComponent.intensity;
	}

	private void Update()
	{
		if (timer <= 0f)
		{
			timer = Random.Range(0.05f, 0.2f);
			intensityTarget = Random.Range(0.75f, 1f);
		}
		else
		{
			timer -= Time.deltaTime;
		}
		lightComponent.intensity = Mathf.Lerp(lightComponent.intensity, intensity * intensityTarget, Time.deltaTime * 30f);
	}
}
