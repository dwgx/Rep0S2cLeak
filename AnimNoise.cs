using UnityEngine;

public class AnimNoise : MonoBehaviour
{
	public AnimationCurve noiseCurve;

	public AnimationCurve noiseOverrideCurve;

	public float noiseStrengthDefault = 1f;

	public float noiseSpeedDefault = 1f;

	private float noiseStrength = 1f;

	private float noiseSpeed = 1f;

	[HideInInspector]
	public float MasterAmount = 1f;

	[Header("Override Multipliers")]
	public float noiseOverrideMultStrength = 1f;

	public float noiseOverrideMultSpeed = 1f;

	private float noiseOverrideLerp;

	private float noiseOverrideTimer;

	private float noiseOverrideStrength;

	private float noiseOverrideSpeed;

	private float noiseOverrideIntroSpeed;

	private float noiseOverrideOutroSpeed;

	[Header("Rotation X")]
	public float noiseRotXAmountMin;

	public float noiseRotXAmountMax;

	public float noiseRotXSpeedMin;

	public float noiseRotXSpeedMax;

	private float noiseRotXLerp = 1f;

	private float noiseRotXNew;

	private float noiseRotXOld;

	private float noiseRotXSpeed;

	[Header("Rotation Y")]
	public float noiseRotYAmountMin;

	public float noiseRotYAmountMax;

	public float noiseRotYSpeedMin;

	public float noiseRotYSpeedMax;

	private float noiseRotYLerp = 1f;

	private float noiseRotYNew;

	private float noiseRotYOld;

	private float noiseRotYSpeed;

	[Header("Rotation Z")]
	public float noiseRotZAmountMin;

	public float noiseRotZAmountMax;

	public float noiseRotZSpeedMin;

	public float noiseRotZSpeedMax;

	private float noiseRotZLerp = 1f;

	private float noiseRotZNew;

	private float noiseRotZOld;

	private float noiseRotZSpeed;

	[Header("Position X")]
	public float noisePosXAmountMin;

	public float noisePosXAmountMax;

	public float noisePosXSpeedMin;

	public float noisePosXSpeedMax;

	private float noisePosXLerp = 1f;

	private float noisePosXNew;

	private float noisePosXOld;

	private float noisePosXSpeed;

	[Header("Position Y")]
	public float noisePosYAmountMin;

	public float noisePosYAmountMax;

	public float noisePosYSpeedMin;

	public float noisePosYSpeedMax;

	private float noisePosYLerp = 1f;

	private float noisePosYNew;

	private float noisePosYOld;

	private float noisePosYSpeed;

	[Header("Position Z")]
	public float noisePosZAmountMin;

	public float noisePosZAmountMax;

	public float noisePosZSpeedMin;

	public float noisePosZSpeedMax;

	private float noisePosZLerp = 1f;

	private float noisePosZNew;

	private float noisePosZOld;

	private float noisePosZSpeed;

	public void NoiseOverride(float time, float speed, float strength, float introSpeed, float outroSpeed)
	{
		noiseOverrideTimer = time;
		noiseOverrideSpeed = Mathf.Max(speed, speed * noiseOverrideMultSpeed);
		noiseOverrideStrength = Mathf.Max(strength, strength * noiseOverrideMultStrength);
		noiseOverrideIntroSpeed = introSpeed;
		noiseOverrideOutroSpeed = outroSpeed;
	}

	private void Update()
	{
		float num = Mathf.LerpUnclamped(noiseRotXOld, noiseRotXNew, noiseCurve.Evaluate(noiseRotXLerp));
		noiseRotXLerp += noiseRotXSpeed * noiseSpeed * Time.deltaTime;
		if (noiseRotXLerp >= 1f)
		{
			noiseRotXOld = noiseRotXNew;
			noiseRotXNew = Random.Range(noiseRotXAmountMin, noiseRotXAmountMax) * noiseStrength;
			noiseRotXSpeed = Random.Range(noiseRotXSpeedMin, noiseRotXSpeedMax);
			noiseRotXLerp = 0f;
		}
		float num2 = Mathf.LerpUnclamped(noiseRotYOld, noiseRotYNew, noiseCurve.Evaluate(noiseRotYLerp));
		noiseRotYLerp += noiseRotYSpeed * noiseSpeed * Time.deltaTime;
		if (noiseRotYLerp >= 1f)
		{
			noiseRotYOld = noiseRotYNew;
			noiseRotYNew = Random.Range(noiseRotYAmountMin, noiseRotYAmountMax) * noiseStrength;
			noiseRotYSpeed = Random.Range(noiseRotYSpeedMin, noiseRotYSpeedMax);
			noiseRotYLerp = 0f;
		}
		float num3 = Mathf.LerpUnclamped(noiseRotZOld, noiseRotZNew, noiseCurve.Evaluate(noiseRotZLerp));
		noiseRotZLerp += noiseRotZSpeed * noiseSpeed * Time.deltaTime;
		if (noiseRotZLerp >= 1f)
		{
			noiseRotZOld = noiseRotZNew;
			noiseRotZNew = Random.Range(noiseRotZAmountMin, noiseRotZAmountMax) * noiseStrength;
			noiseRotZSpeed = Random.Range(noiseRotZSpeedMin, noiseRotZSpeedMax);
			noiseRotZLerp = 0f;
		}
		float num4 = Mathf.LerpUnclamped(noisePosXOld, noisePosXNew, noiseCurve.Evaluate(noisePosXLerp));
		noisePosXLerp += noisePosXSpeed * noiseSpeed * Time.deltaTime;
		if (noisePosXLerp >= 1f)
		{
			noisePosXOld = noisePosXNew;
			noisePosXNew = Random.Range(noisePosXAmountMin, noisePosXAmountMax) * noiseStrength;
			noisePosXSpeed = Random.Range(noisePosXSpeedMin, noisePosXSpeedMax);
			noisePosXLerp = 0f;
		}
		float num5 = Mathf.LerpUnclamped(noisePosYOld, noisePosYNew, noiseCurve.Evaluate(noisePosYLerp));
		noisePosYLerp += noisePosYSpeed * noiseSpeed * Time.deltaTime;
		if (noisePosYLerp >= 1f)
		{
			noisePosYOld = noisePosYNew;
			noisePosYNew = Random.Range(noisePosYAmountMin, noisePosYAmountMax) * noiseStrength;
			noisePosYSpeed = Random.Range(noisePosYSpeedMin, noisePosYSpeedMax);
			noisePosYLerp = 0f;
		}
		float num6 = Mathf.LerpUnclamped(noisePosZOld, noisePosZNew, noiseCurve.Evaluate(noisePosZLerp));
		noisePosZLerp += noisePosZSpeed * noiseSpeed * Time.deltaTime;
		if (noisePosZLerp >= 1f)
		{
			noisePosZOld = noisePosZNew;
			noisePosZNew = Random.Range(noisePosZAmountMin, noisePosZAmountMax) * noiseStrength;
			noisePosZSpeed = Random.Range(noisePosZSpeedMin, noisePosZSpeedMax);
			noisePosZLerp = 0f;
		}
		if (noiseOverrideTimer > 0f)
		{
			noiseOverrideLerp = Mathf.Clamp01(noiseOverrideLerp + noiseOverrideIntroSpeed * Time.deltaTime);
			noiseOverrideTimer -= Time.deltaTime;
		}
		else
		{
			noiseOverrideLerp = Mathf.Clamp01(noiseOverrideLerp - noiseOverrideOutroSpeed * Time.deltaTime);
		}
		noiseStrength = Mathf.Lerp(noiseStrengthDefault, noiseOverrideStrength, noiseOverrideCurve.Evaluate(noiseOverrideLerp));
		noiseSpeed = Mathf.Lerp(noiseSpeedDefault, noiseOverrideSpeed, noiseOverrideCurve.Evaluate(noiseOverrideLerp));
		base.transform.localPosition = new Vector3(num4 * MasterAmount, num5 * MasterAmount, num6 * MasterAmount);
		base.transform.localRotation = Quaternion.Euler(num * MasterAmount, num2 * MasterAmount, num3 * MasterAmount);
	}
}
