using System;
using UnityEngine;

public class TruckHealerLine : MonoBehaviour
{
	public Transform lineTarget;

	private LineRenderer lineRenderer;

	public AnimationCurve wobbleCurve;

	public AnimationCurve widthCurve;

	private float curveEval;

	internal bool outro;

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.widthMultiplier = 0f;
	}

	private void Update()
	{
		if (!lineTarget)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if (curveEval < 1f)
		{
			curveEval += Time.deltaTime * 2.5f;
			lineRenderer.widthMultiplier = widthCurve.Evaluate(curveEval);
			if ((bool)lineTarget)
			{
				Vector3 position = base.transform.position;
				Vector3 position2 = lineTarget.position;
				Vector3[] array = new Vector3[20];
				for (int i = 0; i < 20; i++)
				{
					float num = (float)i / 19f;
					array[i] = Vector3.Lerp(position, position2, num) - Vector3.up * Mathf.Sin(num * MathF.PI) * 0.5f;
					float num2 = 1f - Mathf.Abs(num - 0.5f) * 2f;
					float num3 = 1f;
					float num4 = wobbleCurve.Evaluate(num) * 2f;
					array[i] += Vector3.right * Mathf.Sin(Time.time * (30f * num3) + (float)i) * 0.02f * num2 * num4;
					array[i] += Vector3.forward * Mathf.Cos(Time.time * (30f * num3) + (float)i) * 0.02f * num2 * num4;
				}
				lineRenderer.material.mainTextureOffset = new Vector2((0f - Time.time) * 2f, 0f);
				lineRenderer.positionCount = 20;
				lineRenderer.SetPositions(array);
			}
			else
			{
				outro = true;
			}
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
