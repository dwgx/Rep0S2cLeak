using System;
using UnityEngine;

[Serializable]
public class FlightSettings
{
	[Header("Basic Flight")]
	public float moveForce = 100f;

	public float turnSmoothness = 2f;

	public float maxSpeed = 20f;

	[Header("Height Control")]
	public float minFlightHeight = 5f;

	public float maxFlightHeight = 25f;

	public float preferredHeight = 15f;

	[Header("Stabilization")]
	public float stabilizationForce = 100f;

	public float groundAvoidanceDistance = 8f;

	public float stabilizationRange = 6f;

	[Header("Movement Smoothing")]
	public float positionDamping = 0.5f;

	public float rotationDamping = 0.5f;

	public float positionSpeed = 20f;

	public float rotationSpeed = 40f;
}
