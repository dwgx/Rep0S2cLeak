using System;
using UnityEngine;

public class SemiLine : MonoBehaviour
{
	internal Transform lineTarget;

	private LineRenderer lineRenderer;

	private bool outro;

	public float curveHeight = 1f;

	public float lineWidth = 0.2f;

	public float lineWobbleSpeed = 30f;

	public float lineWobbleAmount = 0.02f;

	public int linePoints = 20;

	public float textureScrollSpeed = 2f;

	private float activeTimer;

	private bool activeStart;

	private void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.widthMultiplier = 0f;
		lineRenderer.enabled = false;
	}

	public void LineActive(Transform _lineTarget)
	{
		if (!lineTarget)
		{
			activeStart = true;
		}
		lineTarget = _lineTarget;
		activeTimer = 0.2f;
	}

	private void Update()
	{
		if (activeTimer <= 0f)
		{
			lineTarget = null;
			outro = true;
		}
		if (activeTimer > 0f)
		{
			if (activeStart)
			{
				lineRenderer.widthMultiplier = 0f;
				lineRenderer.enabled = true;
				activeStart = false;
				outro = false;
			}
			activeTimer -= Time.deltaTime;
		}
		if ((bool)lineTarget && lineRenderer.enabled)
		{
			Vector3 position = base.transform.position;
			Vector3 position2 = lineTarget.position;
			Vector3[] array = new Vector3[linePoints];
			lineRenderer.positionCount = linePoints;
			for (int i = 0; i < linePoints; i++)
			{
				float num = (float)i / (float)(linePoints - 1);
				array[i] = Vector3.Lerp(position, position2, num) + Vector3.up * Mathf.Sin(num * MathF.PI) * curveHeight;
				float num2 = 1f - Mathf.Abs(num - 0.5f) * 2f;
				array[i] += Vector3.right * Mathf.Sin(Time.time * lineWobbleSpeed + (float)i) * lineWobbleAmount * num2;
				array[i] += Vector3.forward * Mathf.Cos(Time.time * lineWobbleSpeed + (float)i) * lineWobbleAmount * num2;
			}
			lineRenderer.material.mainTextureOffset = new Vector2(Time.time * 2f, 0f);
			lineRenderer.positionCount = linePoints;
			lineRenderer.SetPositions(array);
		}
		if (!lineRenderer.enabled)
		{
			return;
		}
		if (!outro)
		{
			if (lineRenderer.widthMultiplier < lineWidth - 0.005f)
			{
				lineRenderer.widthMultiplier = Mathf.Lerp(lineRenderer.widthMultiplier, lineWidth, Time.deltaTime * textureScrollSpeed);
			}
			else
			{
				lineRenderer.widthMultiplier = lineWidth;
			}
		}
		else if (lineRenderer.widthMultiplier > 0.005f)
		{
			lineRenderer.widthMultiplier = Mathf.Lerp(lineRenderer.widthMultiplier, 0f, Time.deltaTime * textureScrollSpeed);
		}
		else
		{
			lineRenderer.widthMultiplier = 0f;
			lineRenderer.enabled = false;
		}
	}
}
