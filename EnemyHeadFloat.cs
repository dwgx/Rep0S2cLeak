using UnityEngine;

public class EnemyHeadFloat : MonoBehaviour
{
	public AnimationCurve CurvePos;

	public float SpeedPos;

	public float AmountPos;

	private float LerpPos;

	private bool ReversePos;

	[Space]
	public AnimationCurve CurveRot;

	public float SpeedRot;

	public float AmountRot;

	private float LerpRot;

	private float DisableTimer;

	public void Disable(float time)
	{
		DisableTimer = time;
	}

	private void Update()
	{
		if (DisableTimer > 0f)
		{
			DisableTimer -= Time.deltaTime;
			return;
		}
		if (!ReversePos)
		{
			LerpPos += SpeedPos * Time.deltaTime;
			if (LerpPos >= 1f)
			{
				ReversePos = true;
				LerpPos = 1f;
			}
		}
		else
		{
			LerpPos -= SpeedPos * Time.deltaTime;
			if (LerpPos <= 0f)
			{
				ReversePos = false;
				LerpPos = 0f;
			}
		}
		base.transform.localPosition = new Vector3(0f, CurvePos.Evaluate(LerpPos) * AmountPos, 0f);
		LerpRot += SpeedRot * Time.deltaTime;
		if (LerpRot >= 1f)
		{
			LerpRot = 0f;
		}
		base.transform.localRotation = Quaternion.Euler(CurveRot.Evaluate(LerpRot) * AmountRot, 0f, 0f);
	}
}
