using System;
using UnityEngine;

[Serializable]
public class ExpressionSettings
{
	public string expressionName;

	[Range(0f, 100f)]
	public float weight;

	internal float timer;

	internal bool isExpressing;

	internal bool stopExpressing;

	[Space]
	public float headTiltAmount;

	[Header("Left Eye Settings")]
	public EyeSettings leftEye;

	[Header("Right Eye Settings")]
	public EyeSettings rightEye;
}
