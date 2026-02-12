using UnityEngine;

public class DusterDusting : MonoBehaviour
{
	public bool Active;

	private bool ActivePrev;

	public float AmountZ;

	public float SpeedZ;

	private float LerpZ;

	private bool ReverseZ;

	[Space]
	public float AmountX;

	public float SpeedX;

	private float LerpX;

	private bool ReverseX;

	[Space]
	public AnimationCurve Curve;

	public float ActiveAmount;

	[Header("Sounds")]
	[Space]
	public Sound Start;

	public Sound Loop;

	public Sound Stop;

	private void Update()
	{
		if (Active)
		{
			GameDirector.instance.CameraShake.Shake(1f, 0.25f);
			if (ActivePrev != Active)
			{
				Start.Play(base.transform.position);
			}
			ActiveAmount += 2f * Time.deltaTime;
		}
		else
		{
			if (ActivePrev != Active)
			{
				Stop.Play(base.transform.position);
			}
			ActiveAmount -= 1.5f * Time.deltaTime;
		}
		ActiveAmount = Mathf.Clamp01(ActiveAmount);
		ActivePrev = Active;
		Loop.PlayLoop(Active, 2f, 2f);
		if (ActiveAmount > 0f)
		{
			if (!ReverseZ)
			{
				LerpZ += SpeedZ * Time.deltaTime;
				if (LerpZ >= 1f)
				{
					ReverseZ = true;
				}
			}
			else
			{
				LerpZ -= SpeedZ * Time.deltaTime;
				if (LerpZ <= 0f)
				{
					ReverseZ = false;
				}
			}
		}
		if (ActiveAmount > 0f)
		{
			if (!ReverseX)
			{
				LerpX += SpeedX * Time.deltaTime;
				if (LerpX >= 1f)
				{
					ReverseX = true;
				}
			}
			else
			{
				LerpX -= SpeedX * Time.deltaTime;
				if (LerpX <= 0f)
				{
					ReverseX = false;
				}
			}
		}
		if (ActiveAmount > 0f)
		{
			base.transform.localRotation = Quaternion.Euler((Curve.Evaluate(LerpX) * AmountX - AmountX * 0.5f) * Curve.Evaluate(ActiveAmount), 0f, (Curve.Evaluate(LerpZ) * AmountZ - AmountX * 0.5f) * Curve.Evaluate(ActiveAmount));
		}
		else
		{
			base.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
	}
}
