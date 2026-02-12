using UnityEngine;

public class SledgehammerTransition : MonoBehaviour
{
	public SledgehammerController Controller;

	private Vector3 PositionStart;

	private Quaternion RotationStart;

	private Vector3 ScaleStart;

	[Space]
	public Transform SwingTarget;

	public Transform HitTarget;

	[Space]
	public AnimationCurve IntroCurve;

	public float IntroSpeed = 1f;

	[Space]
	public AnimationCurve OutroCurve;

	public float OutroSpeed = 1f;

	private float LerpAmount;

	private bool Intro;

	public void IntroSet()
	{
		Intro = true;
		LerpAmount = 0f;
		PositionStart = SwingTarget.position;
		RotationStart = SwingTarget.rotation;
		ScaleStart = SwingTarget.localScale;
		base.transform.position = PositionStart;
		base.transform.rotation = RotationStart;
		base.transform.localScale = ScaleStart;
	}

	public void OutroSet()
	{
		Intro = false;
		LerpAmount = 0f;
		PositionStart = HitTarget.position;
		RotationStart = HitTarget.rotation;
		ScaleStart = HitTarget.localScale;
		base.transform.position = PositionStart;
		base.transform.rotation = RotationStart;
		base.transform.localScale = ScaleStart;
	}

	public void Update()
	{
		if (!(LerpAmount < 1f))
		{
			return;
		}
		if (Intro)
		{
			LerpAmount += IntroSpeed * Time.deltaTime;
			base.transform.position = Vector3.Lerp(PositionStart, HitTarget.position, IntroCurve.Evaluate(LerpAmount));
			base.transform.rotation = Quaternion.Lerp(RotationStart, HitTarget.rotation, IntroCurve.Evaluate(LerpAmount));
			base.transform.localScale = Vector3.Lerp(ScaleStart, HitTarget.localScale, IntroCurve.Evaluate(LerpAmount));
		}
		else
		{
			LerpAmount += OutroSpeed * Time.deltaTime;
			base.transform.position = Vector3.Lerp(PositionStart, SwingTarget.position, OutroCurve.Evaluate(LerpAmount));
			base.transform.rotation = Quaternion.Lerp(RotationStart, SwingTarget.rotation, OutroCurve.Evaluate(LerpAmount));
			base.transform.localScale = Vector3.Lerp(ScaleStart, SwingTarget.localScale, OutroCurve.Evaluate(LerpAmount));
		}
		if (LerpAmount >= 1f)
		{
			if (Intro)
			{
				Controller.IntroDone();
			}
			else
			{
				Controller.OutroDone();
			}
			base.gameObject.SetActive(value: false);
		}
	}
}
