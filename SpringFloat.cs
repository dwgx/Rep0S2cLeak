using System;
using UnityEngine;

[Serializable]
public class SpringFloat
{
	public float damping = 0.5f;

	public float speed = 10f;

	[Space]
	public bool clamp;

	public float min;

	public float max = 1f;

	internal float lastPosition;

	internal float springVelocity;
}
