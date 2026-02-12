using UnityEngine;

public class DirtFinderCompleteDance : MonoBehaviour
{
	public bool Active;

	[Space]
	public AnimationCurve IntroCurve;

	public float IntroSpeed;

	private float IntroLerp;

	[Space]
	public AnimationCurve DanceCurveX;

	public float DanceSpeedX;

	public float DanceAmountX;

	private float DanceLerpX;

	[Space]
	public AnimationCurve DanceCurveY;

	public float DanceSpeedY;

	public float DanceAmountY;

	private float DanceLerpY;

	[Space]
	public AnimationCurve DanceCurveZ;

	public float DanceSpeedZ;

	public float DanceAmountZ;

	private float DanceLerpZ;

	private void Update()
	{
		if (Active)
		{
			IntroLerp += IntroSpeed * Time.deltaTime;
			IntroLerp = Mathf.Clamp01(IntroLerp);
			if (DanceLerpX >= 1f)
			{
				DanceLerpX = 0f;
			}
			DanceLerpX += DanceSpeedX * Time.deltaTime;
			float num = DanceCurveX.Evaluate(DanceLerpX) * IntroCurve.Evaluate(IntroLerp);
			if (DanceLerpY >= 1f)
			{
				DanceLerpY = 0f;
			}
			DanceLerpY += DanceSpeedY * Time.deltaTime;
			float num2 = DanceCurveY.Evaluate(DanceLerpY) * IntroCurve.Evaluate(IntroLerp);
			if (DanceLerpZ >= 1f)
			{
				DanceLerpZ = 0f;
			}
			DanceLerpZ += DanceSpeedZ * Time.deltaTime;
			float num3 = DanceCurveZ.Evaluate(DanceLerpZ) * IntroCurve.Evaluate(IntroLerp);
			base.transform.localRotation = Quaternion.Euler(DanceAmountX * num, DanceAmountY * num2, DanceAmountZ * num3);
		}
	}
}
