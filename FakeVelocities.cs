using System;
using UnityEngine;

public class FakeVelocities : MonoBehaviour
{
	public Transform trackedTransform;

	public Vector3 velocity;

	public Vector3 angularVelocity;

	private Quaternion _lastRotation;

	private Vector3 _lastPosition;

	private void Start()
	{
		if (trackedTransform == null)
		{
			trackedTransform = base.transform;
		}
		_lastRotation = trackedTransform.rotation;
		_lastPosition = trackedTransform.position;
	}

	private void Update()
	{
		UpdateVelocity();
		UpdateAngularVelocity();
	}

	private void UpdateVelocity()
	{
		if (SemiFunc.FPSImpulse30())
		{
			Vector3 position = trackedTransform.position;
			Vector3 vector = position - _lastPosition;
			velocity = vector * 30f;
			_lastPosition = position;
		}
	}

	private void UpdateAngularVelocity()
	{
		if (SemiFunc.FPSImpulse30())
		{
			Quaternion rotation = trackedTransform.rotation;
			(rotation * Quaternion.Inverse(_lastRotation)).ToAngleAxis(out var angle, out var axis);
			if (angle > 180f)
			{
				angle -= 360f;
			}
			angularVelocity = axis * (angle * (MathF.PI / 180f)) * 30f;
			_lastRotation = rotation;
		}
	}
}
