using UnityEngine;

[RequireComponent(typeof(Transform))]
public class Roach : MonoBehaviour
{
	[Space]
	[Header("Orbit Parameters")]
	public float minOrbitDistance = 1f;

	public float maxOrbitDistance = 5f;

	public float minOrbitSpeed = 1f;

	public float maxOrbitSpeed = 5f;

	public float orbitWiggleFrequency = 1f;

	public float speedWiggleFrequency = 1f;

	[Space]
	[Header("Roach Parameters")]
	public float minRoachSpeed = 1f;

	public float maxRoachSpeed = 3f;

	public float roachSpeedFluctuationFrequency = 0.5f;

	public float overshootMultiplier = 1.5f;

	public float turnMultiplier = 0.5f;

	[Space]
	[Header("Roach Smash")]
	public GameObject roachSmashPrefab;

	private Vector3 origin;

	private float currentOrbitDistance;

	private float currentOrbitSpeed;

	private float angle;

	private float roachSpeedTarget;

	private float currentRoachSpeed;

	private Vector3 targetPosition;

	private Vector3 velocity;

	private void Start()
	{
		origin = base.transform.position;
		roachSpeedTarget = Random.Range(minRoachSpeed, maxRoachSpeed);
		targetPosition = GetOrbitPoint(angle);
		base.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		base.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		base.transform.rotation = Quaternion.identity;
	}

	private void Update()
	{
		currentOrbitDistance = Mathf.Lerp(minOrbitDistance, maxOrbitDistance, (Mathf.Sin(Time.time * orbitWiggleFrequency) + 1f) * 0.5f);
		currentOrbitSpeed = Mathf.Lerp(minOrbitSpeed, maxOrbitSpeed, (Mathf.Sin(Time.time * speedWiggleFrequency) + 1f) * 0.5f);
		angle += Time.deltaTime * currentOrbitSpeed;
		Vector3 orbitPoint = GetOrbitPoint(angle);
		if (Vector3.Distance(base.transform.position, targetPosition) < 0.1f)
		{
			Vector3 normalized = (orbitPoint - targetPosition).normalized;
			targetPosition = orbitPoint + normalized * overshootMultiplier;
		}
		currentRoachSpeed = Mathf.Lerp(currentRoachSpeed, roachSpeedTarget, Time.deltaTime * roachSpeedFluctuationFrequency);
		if (Mathf.Abs(currentRoachSpeed - roachSpeedTarget) < 0.1f)
		{
			roachSpeedTarget = Random.Range(minRoachSpeed, maxRoachSpeed);
		}
		Vector3 vector = ((targetPosition - base.transform.position).normalized * currentRoachSpeed - velocity) * turnMultiplier;
		velocity += vector * Time.deltaTime;
		base.transform.position += velocity * Time.deltaTime;
		if (velocity != Vector3.zero)
		{
			base.transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
		}
	}

	private Vector3 GetOrbitPoint(float angle)
	{
		return origin + new Vector3(Mathf.Sin(angle) * currentOrbitDistance, 0f, Mathf.Cos(angle) * currentOrbitDistance);
	}
}
