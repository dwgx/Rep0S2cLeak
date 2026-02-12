using System;
using UnityEngine;

[Serializable]
public class SpringQuaternion
{
	public float damping = 0.5f;

	public float speed = 10f;

	[Space]
	public bool clamp;

	public float maxAngle = 20f;

	internal Quaternion lastRotation;

	internal Vector3 springVelocity = Vector3.zero;

	internal bool setup;
}
