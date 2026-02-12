using System.Collections;
using UnityEngine;

public class CameraTopFade : MonoBehaviour
{
	public static CameraTopFade Instance;

	public Transform MeshTransform;

	public MeshRenderer Mesh;

	[Space]
	public AnimationCurve Curve;

	public float Speed = 1f;

	private bool Fading;

	private bool Active;

	private float ActiveTimer;

	private float Amount;

	private float AmountCurrent;

	private float AmountStart;

	private float AmountEnd;

	private float LerpAmount;

	private void Awake()
	{
		Instance = this;
	}

	public void Set(float amount, float time)
	{
		ActiveTimer = time;
		if (!Active)
		{
			Active = true;
			AmountStart = AmountCurrent;
			AmountEnd = amount;
			LerpAmount = 0f;
		}
		if (!Fading)
		{
			Color color = Mesh.material.color;
			color.a = 0f;
			Mesh.material.color = color;
			MeshTransform.gameObject.SetActive(value: true);
			Fading = true;
			StartCoroutine(Fade());
		}
	}

	private IEnumerator Fade()
	{
		while (Fading)
		{
			if (Active)
			{
				AmountCurrent = Mathf.Lerp(AmountStart, AmountEnd, Curve.Evaluate(LerpAmount));
				LerpAmount += Speed * Time.deltaTime;
				LerpAmount = Mathf.Clamp01(LerpAmount);
				if (ActiveTimer > 0f)
				{
					ActiveTimer -= Time.deltaTime;
				}
				else
				{
					AmountStart = AmountCurrent;
					AmountEnd = 0f;
					Active = false;
					LerpAmount = 0f;
				}
			}
			else
			{
				AmountCurrent = Mathf.Lerp(AmountStart, AmountEnd, Curve.Evaluate(LerpAmount));
				LerpAmount += Speed * Time.deltaTime;
				LerpAmount = Mathf.Clamp01(LerpAmount);
				if (LerpAmount >= 1f)
				{
					Fading = false;
					MeshTransform.gameObject.SetActive(value: false);
				}
			}
			Color color = Mesh.material.color;
			color.a = AmountCurrent;
			Mesh.material.color = color;
			yield return null;
		}
	}
}
