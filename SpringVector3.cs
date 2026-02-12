using System;
using UnityEngine;

[Serializable]
public class SpringVector3
{
	public float damping = 0.5f;

	public float speed = 10f;

	[Space]
	public bool clamp;

	public float maxDistance = 1f;

	internal Vector3 lastPosition;

	internal Vector3 springVelocity = Vector3.zero;
}
