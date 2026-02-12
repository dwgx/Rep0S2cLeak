using UnityEngine;

public class MilkValuable : Trap
{
	[Header("Physics Settings")]
	public float milkMass = 1f;

	public float gravity = 9.81f;

	public float forceMultiplier = 10f;

	public float springStrength = 50f;

	public float friction = 5f;

	public float angularInfluence = 2f;

	public float impactForceMultiplier = 4f;

	public float gravityForceMultiplier = 0.8f;

	[Header("Center of mass")]
	public float centerOfMassOffset = 1f;

	public float centerOfMassMargin = 0.2f;

	[Header("Milk Movement Range")]
	public float bottomY;

	public float topY = 3f;

	[Header("Sounds")]
	public Sound sloshSound;

	[Header("Sound frequency variables")]
	public float sloshVelocity = 2f;

	public float sloshCooldown = 0.3f;

	private Rigidbody rb;

	private float milkYPos;

	private float targetMilkY;

	private float acceleration;

	private float nextSloshTime;

	private float netForce;

	private float springForce;

	private float frictionForce;

	protected override void Start()
	{
		base.Start();
		rb = GetComponent<Rigidbody>();
		milkYPos = bottomY;
	}

	protected override void Update()
	{
		if (physGrabObject.rbVelocity.magnitude > sloshVelocity && Time.time >= nextSloshTime)
		{
			sloshSound.Play(physGrabObject.centerPoint);
			nextSloshTime = Time.time + sloshCooldown;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			physGrabObject.OverrideTorqueStrength(0.5f);
		}
	}
}
