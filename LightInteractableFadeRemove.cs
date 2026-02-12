using UnityEngine;

public class LightInteractableFadeRemove : MonoBehaviour
{
	public AnimationCurve fadeCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	public float fadeDuration = 2f;

	private Light lightComponent;

	private float currentTime;

	private bool isFading;

	private void Start()
	{
		lightComponent = GetComponent<Light>();
	}

	private void Update()
	{
		if (isFading)
		{
			currentTime += Time.deltaTime;
			float time = currentTime / fadeDuration;
			lightComponent.intensity = fadeCurve.Evaluate(time) * lightComponent.intensity;
			if (currentTime >= fadeDuration)
			{
				Object.Destroy(lightComponent);
				Object.Destroy(this);
			}
		}
	}

	public void StartFading()
	{
		isFading = true;
	}
}
