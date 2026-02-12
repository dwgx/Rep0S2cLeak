using UnityEngine;

public class VacuumCleanerBag : MonoBehaviour
{
	[HideInInspector]
	public bool Active;

	[Space]
	public float AmountX;

	public float SpeedX;

	private float LerpX;

	private bool ActiveX;

	[Space]
	public float AmountZ;

	public float SpeedZ;

	private float LerpZ;

	private bool ActiveZ;

	[Space]
	public AnimationCurve AnimationCurve;

	private void Update()
	{
		if (ActiveX)
		{
			LerpX += SpeedX * Time.deltaTime;
			if (LerpX >= 1f)
			{
				if (!Active)
				{
					ActiveX = false;
				}
				LerpX = 0f;
			}
		}
		else if (Active)
		{
			ActiveX = true;
		}
		if (ActiveZ)
		{
			LerpZ += SpeedZ * Time.deltaTime;
			if (LerpZ >= 1f)
			{
				if (!Active)
				{
					ActiveZ = false;
				}
				LerpZ = 0f;
			}
		}
		else if (Active)
		{
			ActiveZ = true;
		}
		if (ActiveX || ActiveZ)
		{
			float num = Mathf.Lerp(0f, AmountX, AnimationCurve.Evaluate(LerpX));
			float num2 = Mathf.Lerp(0f, AmountZ, AnimationCurve.Evaluate(LerpZ));
			base.transform.localScale = new Vector3(1f + num, 1f, 1f + num2);
		}
	}
}
