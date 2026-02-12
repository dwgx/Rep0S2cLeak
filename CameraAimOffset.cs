using System.Collections;
using UnityEngine;

public class CameraAimOffset : MonoBehaviour
{
	public static CameraAimOffset Instance;

	private bool Animating;

	private bool Active;

	private float ActiveTimer;

	public AnimationCurve IntroCurve;

	public float IntroSpeed = 1f;

	public AnimationCurve OutroCurve;

	public float OutroSpeed = 1f;

	private float LerpAmount;

	private Vector3 PositionStart;

	private Vector3 RotationStart;

	private Vector3 PositionEnd;

	private Vector3 RotationEnd;

	private float IntroPauseTimer = 0.1f;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		StartCoroutine(LateStart());
	}

	private IEnumerator LateStart()
	{
		yield return new WaitForSeconds(0.5f);
		if (!CameraNoPlayerTarget.instance)
		{
			Set(Vector3.zero, new Vector3(45f, 0f, 0f), 0f);
			Active = false;
			LerpAmount = 0.25f;
		}
		Active = false;
		PositionStart = PositionEnd;
		RotationStart = RotationEnd;
		PositionEnd = Vector3.zero;
		RotationEnd = Vector3.zero;
	}

	public void Set(Vector3 position, Vector3 rotation, float time)
	{
		if (!Active || position != PositionEnd || rotation != RotationEnd)
		{
			PositionStart = base.transform.localPosition;
			RotationStart = base.transform.localEulerAngles;
			PositionEnd = position;
			RotationEnd = rotation;
			Active = true;
			LerpAmount = 0f;
		}
		ActiveTimer = time;
		if (!Animating)
		{
			Animating = true;
			StartCoroutine(Animate());
		}
	}

	private IEnumerator Animate()
	{
		while (GameDirector.instance.currentState != GameDirector.gameState.Main || IntroPauseTimer > 0f)
		{
			IntroPauseTimer -= Time.deltaTime;
			yield return null;
		}
		if ((bool)CameraNoPlayerTarget.instance)
		{
			yield break;
		}
		while (Animating)
		{
			if (Active)
			{
				base.transform.localPosition = Vector3.Lerp(PositionStart, PositionEnd, IntroCurve.Evaluate(LerpAmount));
				base.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(RotationStart), Quaternion.Euler(RotationEnd), IntroCurve.Evaluate(LerpAmount));
				LerpAmount += IntroSpeed * Time.deltaTime;
				LerpAmount = Mathf.Clamp01(LerpAmount);
				if (ActiveTimer > 0f)
				{
					ActiveTimer -= Time.deltaTime;
				}
				else
				{
					PositionStart = base.transform.localPosition;
					RotationStart = base.transform.localRotation.eulerAngles;
					PositionEnd = Vector3.zero;
					RotationEnd = Vector3.zero;
					Active = false;
					LerpAmount = 0f;
				}
			}
			else
			{
				base.transform.localPosition = Vector3.Lerp(PositionStart, PositionEnd, OutroCurve.Evaluate(LerpAmount));
				base.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(RotationStart), Quaternion.Euler(RotationEnd), OutroCurve.Evaluate(LerpAmount));
				LerpAmount += OutroSpeed * Time.deltaTime;
				LerpAmount = Mathf.Clamp01(LerpAmount);
				if (LerpAmount >= 1f)
				{
					Animating = false;
				}
			}
			yield return null;
		}
	}
}
