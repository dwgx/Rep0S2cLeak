using UnityEngine;

public class LightFlicker : MonoBehaviour
{
	private Light lightComp;

	public AnimationCurve intensityCurve;

	public float intensityAmountMin;

	public float intensityAmountMax;

	public float intensitySpeedMin;

	public float intensitySpeedMax;

	private float intensityLerp = 1f;

	private float intensityNew;

	private float intensityOld;

	private float intensitySpeed;

	private float intensityInit;

	private void Start()
	{
		lightComp = GetComponent<Light>();
		intensityInit = lightComp.intensity;
	}

	private void Update()
	{
		float num = Mathf.LerpUnclamped(intensityOld, intensityNew, intensityCurve.Evaluate(intensityLerp));
		intensityLerp += intensitySpeed * Time.deltaTime;
		if (intensityLerp >= 1f)
		{
			intensityOld = intensityNew;
			intensityNew = Random.Range(intensityAmountMin, intensityAmountMax);
			intensitySpeed = Random.Range(intensitySpeedMin, intensitySpeedMax);
			intensityLerp = 0f;
		}
		lightComp.intensity = intensityInit + num;
	}
}
