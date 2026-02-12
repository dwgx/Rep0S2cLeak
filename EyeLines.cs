using System;
using System.Collections.Generic;
using UnityEngine;

public class EyeLines : MonoBehaviour
{
	private List<Transform> targetsList = new List<Transform>();

	private PlayerAvatar targetPlayer;

	private bool isActive;

	public LineRenderer lineRendererLeft;

	public LineRenderer lineRendererRight;

	public float textureScrollSpeed = 0.5f;

	public bool IsActive()
	{
		return isActive;
	}

	public void SetIsActive(bool _isActive)
	{
		if (_isActive && !isActive)
		{
			lineRendererLeft.widthMultiplier = 0f;
			lineRendererRight.widthMultiplier = 0f;
		}
		isActive = _isActive;
	}

	private void Awake()
	{
		lineRendererLeft.widthMultiplier = 0f;
		lineRendererRight.widthMultiplier = 0f;
	}

	public void InitializeLine(PlayerAvatar _targetPlayer)
	{
		targetPlayer = _targetPlayer;
		isActive = true;
		base.transform.localPosition = Vector3.zero;
	}

	public void DrawLine(LineRenderer _lineRenderer, Vector3 _startPoint, Vector3 _endPoint)
	{
		if (isActive)
		{
			FadeLineIn(_lineRenderer);
			Vector3[] array = new Vector3[20];
			for (int i = 0; i < 20; i++)
			{
				float num = (float)i / 19f;
				array[i] = Vector3.Lerp(_startPoint, _endPoint, num) - Vector3.up * Mathf.Sin(num * MathF.PI) * 0.5f;
				float num2 = 1f - Mathf.Abs(num - 0.5f) * 2f;
				float num3 = 1f;
				array[i] += Vector3.right * Mathf.Sin(Time.time * (30f * num3) + (float)i) * 0.02f * num2;
				array[i] += Vector3.forward * Mathf.Cos(Time.time * (30f * num3) + (float)i) * 0.02f * num2;
			}
			_lineRenderer.material.mainTextureOffset = new Vector2(Time.time * textureScrollSpeed, 0f);
			_lineRenderer.positionCount = 20;
			_lineRenderer.SetPositions(array);
		}
		else
		{
			FadeLineOut(_lineRenderer);
		}
	}

	public void DrawLines()
	{
		DrawLine(lineRendererLeft, base.transform.position, targetPlayer.playerAvatarVisuals.playerEyes.pupilLeft.position);
		DrawLine(lineRendererRight, base.transform.position, targetPlayer.playerAvatarVisuals.playerEyes.pupilRight.position);
	}

	private void Update()
	{
		base.transform.localPosition = Vector3.zero;
	}

	private void FadeLineIn(LineRenderer _lineRenderer)
	{
		if (_lineRenderer.widthMultiplier < 0.195f)
		{
			_lineRenderer.widthMultiplier = Mathf.Lerp(_lineRenderer.widthMultiplier, 0.2f, Time.deltaTime * 2f);
		}
		else
		{
			_lineRenderer.widthMultiplier = 0.2f;
		}
	}

	private void FadeLineOut(LineRenderer _lineRenderer)
	{
		if (_lineRenderer.widthMultiplier > 0.005f)
		{
			_lineRenderer.widthMultiplier = Mathf.Lerp(_lineRenderer.widthMultiplier, 0f, Time.deltaTime * 15f);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
