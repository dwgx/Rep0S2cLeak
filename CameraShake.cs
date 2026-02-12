using UnityEngine;

public class CameraShake : MonoBehaviour
{
	public AnimationCurve Curve;

	[Space]
	public bool InstantShake;

	[Space]
	public float Strength;

	public float StrengthMax = 1f;

	public float StrengthLoss = 1f;

	public float StrengthLossDelay;

	[Space]
	public float Speed = 1f;

	[Space]
	public float RotationMultiplier = 1f;

	public float PositionMultiplier = 1f;

	private float RotXLerp = 1f;

	private float RotXNew;

	private float RotXOld;

	private float RotXSpeed;

	private float RotYLerp = 1f;

	private float RotYNew;

	private float RotYOld;

	private float RotYSpeed;

	private float RotZLerp = 1f;

	private float RotZNew;

	private float RotZOld;

	private float RotZSpeed;

	private float PosXLerp = 1f;

	private float PosXNew;

	private float PosXOld;

	private float PosXSpeed;

	private float PosYLerp = 1f;

	private float PosYNew;

	private float PosYOld;

	private float PosYSpeed;

	private float PosZLerp = 1f;

	private float PosZNew;

	private float PosZOld;

	private float PosZSpeed;

	public void Shake(float strengthAdd, float time)
	{
		if (GameDirector.instance.currentState == GameDirector.gameState.Main && strengthAdd > Strength)
		{
			strengthAdd = ShakeMultiplier(strengthAdd);
			Strength += strengthAdd;
			Strength = Mathf.Min(Strength, StrengthMax);
			StrengthLossDelay = Mathf.Max(time, StrengthLossDelay);
			SetInstant();
		}
	}

	public void ShakeDistance(float strength, float distanceMin, float distanceMax, Vector3 position, float time)
	{
		if (GameDirector.instance.currentState == GameDirector.gameState.Main)
		{
			float value = Vector3.Distance(base.transform.position, position);
			float num = Mathf.InverseLerp(distanceMin, distanceMax, value);
			float num2 = strength * (1f - num);
			if (num2 > Strength)
			{
				num2 = ShakeMultiplier(num2);
				Strength += num2;
				Strength = Mathf.Min(Strength, StrengthMax);
				StrengthLossDelay = Mathf.Max(time, StrengthLossDelay);
				SetInstant();
			}
		}
	}

	private float ShakeMultiplier(float _strength)
	{
		_strength *= GameplayManager.instance.cameraShake;
		return _strength;
	}

	public void SetInstant()
	{
		if (InstantShake)
		{
			RotXNew = Random.Range(-1f, 1f) * RotationMultiplier * Strength;
			RotXLerp = 1f;
			RotYNew = Random.Range(-1f, 1f) * RotationMultiplier * Strength;
			RotYLerp = 1f;
			RotZNew = Random.Range(-1f, 1f) * RotationMultiplier * Strength;
			RotZLerp = 1f;
			PosXNew = Random.Range(-1f, 1f) * PositionMultiplier * Strength;
			PosXLerp = 1f;
			PosYNew = Random.Range(-1f, 1f) * PositionMultiplier * Strength;
			PosYLerp = 1f;
			PosZNew = Random.Range(-1f, 1f) * PositionMultiplier * Strength;
			PosZLerp = 1f;
		}
	}

	private void Update()
	{
		float num = Mathf.LerpUnclamped(RotXOld, RotXNew, Curve.Evaluate(RotXLerp));
		RotXLerp += RotXSpeed * Speed * Time.deltaTime;
		if (RotXLerp >= 1f)
		{
			RotXOld = num;
			RotXNew = Random.Range(-1f, 1f) * RotationMultiplier * Strength;
			RotXSpeed = Random.Range(0.8f, 1.2f);
			RotXLerp = 0f;
		}
		float num2 = Mathf.LerpUnclamped(RotYOld, RotYNew, Curve.Evaluate(RotYLerp));
		RotYLerp += RotYSpeed * Speed * Time.deltaTime;
		if (RotYLerp >= 1f)
		{
			RotYOld = num2;
			RotYNew = Random.Range(-1f, 1f) * RotationMultiplier * Strength;
			RotYSpeed = Random.Range(0.8f, 1.2f);
			RotYLerp = 0f;
		}
		float num3 = Mathf.LerpUnclamped(RotZOld, RotZNew, Curve.Evaluate(RotZLerp));
		RotZLerp += RotZSpeed * Speed * Time.deltaTime;
		if (RotZLerp >= 1f)
		{
			RotZOld = num3;
			RotZNew = Random.Range(-1f, 1f) * RotationMultiplier * Strength;
			RotZSpeed = Random.Range(0.8f, 1.2f);
			RotZLerp = 0f;
		}
		float num4 = Mathf.LerpUnclamped(PosXOld, PosXNew, Curve.Evaluate(PosXLerp));
		PosXLerp += PosXSpeed * Speed * Time.deltaTime;
		if (PosXLerp >= 1f)
		{
			PosXOld = num4;
			PosXNew = Random.Range(-1f, 1f) * PositionMultiplier * Strength;
			PosXSpeed = Random.Range(0.8f, 1.2f);
			PosXLerp = 0f;
		}
		float num5 = Mathf.LerpUnclamped(PosYOld, PosYNew, Curve.Evaluate(PosYLerp));
		PosYLerp += PosYSpeed * Speed * Time.deltaTime;
		if (PosYLerp >= 1f)
		{
			PosYOld = num5;
			PosYNew = Random.Range(-1f, 1f) * PositionMultiplier * Strength;
			PosYSpeed = Random.Range(0.8f, 1.2f);
			PosYLerp = 0f;
		}
		float num6 = Mathf.LerpUnclamped(PosZOld, PosZNew, Curve.Evaluate(PosZLerp));
		PosZLerp += PosZSpeed * Speed * Time.deltaTime;
		if (PosZLerp >= 1f)
		{
			PosZOld = num6;
			PosZNew = Random.Range(-1f, 1f) * PositionMultiplier * Strength;
			PosZSpeed = Random.Range(0.8f, 1.2f);
			PosZLerp = 0f;
		}
		base.transform.localPosition = new Vector3(num4, num5, num6);
		base.transform.localRotation = Quaternion.Euler(num, num2, num3);
		if (StrengthLossDelay <= 0f)
		{
			if (Strength > 0f)
			{
				Strength -= StrengthLoss * Time.deltaTime;
				if (Strength <= 0.1f)
				{
					Strength = 0f;
				}
			}
		}
		else
		{
			StrengthLossDelay -= 1f * Time.deltaTime;
		}
	}
}
