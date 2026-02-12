using UnityEngine;

public class ValuablePills : MonoBehaviour
{
	private PhysGrabObject physgrabobject;

	private Vector3 previousVelocity;

	private Vector3 lastRattlePosition;

	public Sound soundPillsRattle;

	public Sound soundPillsRattleShort;

	private Vector3 currentVelocity;

	private float distanceTraveled;

	private float currentSpeed;

	private float previousSpeed;

	private float accelerationMagnitude;

	private float directionChangeThreshold = 0.1f;

	private float minimumVelocityMagnitude = 2f;

	private float shortRattleDistanceThreshold = 1.5f;

	private float minDistanceThreshold = 0.75f;

	private float minimumAccelerationMagnitude = 3f;

	private void Start()
	{
		physgrabobject = GetComponent<PhysGrabObject>();
		previousVelocity = Vector3.zero;
		lastRattlePosition = base.transform.position;
	}

	private void Update()
	{
		if (physgrabobject.playerGrabbing.Count == 0)
		{
			return;
		}
		currentVelocity = physgrabobject.rbVelocity;
		currentSpeed = currentVelocity.magnitude;
		previousSpeed = previousVelocity.magnitude;
		accelerationMagnitude = ((currentVelocity - previousVelocity) / Time.deltaTime).magnitude;
		if (currentSpeed > minimumVelocityMagnitude && previousSpeed > minimumVelocityMagnitude)
		{
			if (Vector3.Dot(currentVelocity.normalized, previousVelocity.normalized) < directionChangeThreshold)
			{
				TryPlayRattleSound();
			}
		}
		else if (currentSpeed > minimumVelocityMagnitude != previousSpeed > minimumVelocityMagnitude && accelerationMagnitude >= minimumAccelerationMagnitude)
		{
			TryPlayRattleSound();
		}
		previousVelocity = currentVelocity;
	}

	private void TryPlayRattleSound()
	{
		distanceTraveled = Vector3.Distance(base.transform.position, lastRattlePosition);
		if (distanceTraveled >= minDistanceThreshold)
		{
			((distanceTraveled < shortRattleDistanceThreshold) ? soundPillsRattleShort : soundPillsRattle).Play(base.transform.position);
		}
		lastRattlePosition = base.transform.position;
	}
}
