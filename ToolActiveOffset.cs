using UnityEngine;

public class ToolActiveOffset : MonoBehaviour
{
	public bool Active;

	private bool ActivePrev;

	private bool ActiveCurrent;

	[HideInInspector]
	public float ActiveLerp = 1f;

	[Space]
	public AnimationCurve IntroCurve;

	public float IntroSpeed = 1.5f;

	[Space]
	public AnimationCurve OutroCurve;

	public float OutroSpeed = 1.5f;

	[Space]
	public Vector3 InactivePosition;

	public Vector3 InactiveRotation;

	[Space]
	public Vector3 ActivePosition;

	public Vector3 ActiveRotation;

	[Space]
	[Header("Sound")]
	public bool MoveSoundAutomatic;

	[HideInInspector]
	public bool MoveSoundManual;

	public Sound MoveSound;

	private void Update()
	{
		if (Active != ActivePrev && ActiveLerp >= 1f)
		{
			if (MoveSoundAutomatic || MoveSoundManual)
			{
				MoveSoundManual = false;
				MoveSound.Play(base.transform.position);
			}
			ActiveLerp = 0f;
			ActivePrev = Active;
			ActiveCurrent = Active;
		}
		else
		{
			if (ActiveCurrent)
			{
				ActiveLerp += IntroSpeed * Time.deltaTime;
			}
			else
			{
				ActiveLerp += OutroSpeed * Time.deltaTime;
			}
			ActiveLerp = Mathf.Clamp01(ActiveLerp);
		}
		if (ActiveCurrent)
		{
			base.transform.localPosition = Vector3.LerpUnclamped(InactivePosition, ActivePosition, IntroCurve.Evaluate(ActiveLerp));
			base.transform.localRotation = Quaternion.LerpUnclamped(Quaternion.Euler(InactiveRotation.x, InactiveRotation.y, InactiveRotation.z), Quaternion.Euler(ActiveRotation.x, ActiveRotation.y, ActiveRotation.z), IntroCurve.Evaluate(ActiveLerp));
		}
		else
		{
			base.transform.localPosition = Vector3.LerpUnclamped(ActivePosition, InactivePosition, OutroCurve.Evaluate(ActiveLerp));
			base.transform.localRotation = Quaternion.LerpUnclamped(Quaternion.Euler(ActiveRotation.x, ActiveRotation.y, ActiveRotation.z), Quaternion.Euler(InactiveRotation.x, InactiveRotation.y, InactiveRotation.z), OutroCurve.Evaluate(ActiveLerp));
		}
	}
}
