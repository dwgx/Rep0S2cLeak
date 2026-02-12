using System;
using UnityEngine;

[Serializable]
public class EyeSettings
{
	[Header("Upper Eyelid Settings")]
	public float upperLidAngle;

	public float upperLidClosedPercent;

	public float upperLidClosedPercentJitterAmount;

	public float upperLidClosedPercentJitterSpeed;

	[Header("Lower Eyelid Settings")]
	public float lowerLidAngle;

	public float lowerLidClosedPercent;

	public float lowerLidClosedPercentJitterAmount;

	public float lowerLidClosedPercentJitterSpeed;

	[Header("Pupil Settings")]
	public float pupilSize;

	public float pupilSizeJitterAmount;

	public float pupilSizeJitterSpeed;

	public float pupilPositionJitter;

	public float pupilPositionJitterAmount;

	public float pupilOffsetRotationX;

	public float pupilOffsetRotationY;
}
