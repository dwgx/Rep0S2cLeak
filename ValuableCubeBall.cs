using Photon.Pun;
using UnityEngine;

public class ValuableCubeBall : MonoBehaviour
{
	public Sound bounceSound;

	private Vector2 bigForceRange = new Vector2(2f, 4f);

	private Vector2 smallForceRange = new Vector2(1f, 1.5f);

	private Vector2 mediumForceRange = new Vector2(1f, 2f);

	private Vector2 bigTorqueRange = new Vector2(0.1f, 0.5f);

	private Vector2 smallTorqueRange = new Vector2(0f, 0.1f);

	private Vector2 mediumTorqueRange = new Vector2(0.05f, 0.2f);

	private float torqueMultiplier = 0.1f;

	private int bounceAmount = 3;

	private int bounces;

	private Rigidbody rb;

	private PhotonView photonView;

	private PhysGrabObject physGrabObject;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		photonView = GetComponent<PhotonView>();
		physGrabObject = GetComponent<PhysGrabObject>();
		bounceAmount = Random.Range(2, 6);
	}

	public void BigBounce()
	{
		bounceSound.Play(physGrabObject.centerPoint);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			torqueMultiplier = Random.Range(bigTorqueRange.x, bigTorqueRange.y);
			if (bounces < bounceAmount && !physGrabObject.grabbed)
			{
				bounces++;
				rb.AddForce(Vector3.up * Random.Range(bigForceRange.x, bigForceRange.y), ForceMode.Impulse);
				rb.AddTorque(Random.insideUnitSphere * torqueMultiplier, ForceMode.Impulse);
			}
			else if (rb.velocity.magnitude < 0.1f || physGrabObject.grabbed)
			{
				bounces = 0;
				bounceAmount = Random.Range(2, 6);
			}
		}
	}

	public void SmallBounce()
	{
		bounceSound.Play(physGrabObject.centerPoint);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			torqueMultiplier = Random.Range(smallTorqueRange.x, smallTorqueRange.y);
			if (bounces < bounceAmount && !physGrabObject.grabbed && rb.velocity.magnitude > 0.2f)
			{
				bounces++;
				rb.AddForce(Vector3.up * Random.Range(smallForceRange.x, smallForceRange.y), ForceMode.Impulse);
				rb.AddTorque(Random.insideUnitSphere * torqueMultiplier, ForceMode.Impulse);
				bounces = bounceAmount;
			}
			else if (rb.velocity.magnitude <= 0.2f || physGrabObject.grabbed)
			{
				bounces = 0;
				bounceAmount = Random.Range(2, 6);
			}
		}
	}

	public void MediumBounce()
	{
		bounceSound.Play(physGrabObject.centerPoint);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			torqueMultiplier = Random.Range(mediumTorqueRange.x, mediumTorqueRange.y);
			if (bounces < bounceAmount && !physGrabObject.grabbed)
			{
				bounces++;
				rb.AddForce(Vector3.up * Random.Range(mediumForceRange.x, mediumForceRange.y), ForceMode.Impulse);
				rb.AddTorque(Random.insideUnitSphere * torqueMultiplier, ForceMode.Impulse);
			}
			else if (rb.velocity.magnitude < 0.1f || physGrabObject.grabbed)
			{
				bounces = 0;
				bounceAmount = Random.Range(2, 6);
			}
		}
	}
}
