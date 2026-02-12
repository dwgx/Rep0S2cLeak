using System;
using UnityEngine;

public class WiggleRotation : MonoBehaviour
{
	public Vector3 wiggleAxis = Vector3.up;

	public float maxRotation = 10f;

	public float wiggleFrequency = 1f;

	[Range(0f, 100f)]
	public float wiggleOffsetPercentage;

	public float wiggleMultiplier = 1f;

	private void LateUpdate()
	{
		float num = wiggleOffsetPercentage / 100f * 2f * MathF.PI;
		Quaternion localRotation = Quaternion.AngleAxis(Mathf.Sin(Time.time * wiggleFrequency + num) * maxRotation * wiggleMultiplier, wiggleAxis);
		base.transform.localRotation = localRotation;
	}
}
