using System.Collections;
using UnityEngine;

public class AnimatedOffset : MonoBehaviour
{
	internal bool Animating;

	private float ActiveTimer;

	public Vector3 PositionOffset;

	public Vector3 RotationOffset;

	[Space]
	public AnimationCurve IntroCurve;

	public float IntroSpeed;

	public Sound IntroSound;

	private float IntroLerp;

	[Space]
	public AnimationCurve OutroCurve;

	public float OutroSpeed;

	public Sound OutroSound;

	private float OutroLerp;

	public void Active(float time)
	{
		ActiveTimer = time;
		if (!Animating)
		{
			Animating = true;
			StartCoroutine(Animate());
		}
	}

	private IEnumerator Animate()
	{
		while (Animating)
		{
			if (ActiveTimer > 0f)
			{
				if (IntroLerp == 0f)
				{
					IntroSound.Play(base.transform.position);
				}
				IntroLerp = Mathf.Clamp01(IntroLerp + IntroSpeed * Time.deltaTime);
				base.transform.localPosition = Vector3.Lerp(Vector3.zero, PositionOffset, IntroCurve.Evaluate(IntroLerp));
				base.transform.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(RotationOffset), IntroCurve.Evaluate(IntroLerp));
				if (IntroLerp >= 1f)
				{
					ActiveTimer -= Time.deltaTime;
				}
				OutroLerp = 0f;
			}
			else
			{
				if (OutroLerp == 0f)
				{
					OutroSound.Play(base.transform.position);
				}
				OutroLerp = Mathf.Clamp01(OutroLerp + OutroSpeed * Time.deltaTime);
				base.transform.localPosition = Vector3.Lerp(PositionOffset, Vector3.zero, OutroCurve.Evaluate(OutroLerp));
				base.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(RotationOffset), Quaternion.identity, OutroCurve.Evaluate(OutroLerp));
				if (OutroLerp >= 1f)
				{
					Animating = false;
				}
				IntroLerp = 0f;
			}
			yield return null;
		}
	}
}
