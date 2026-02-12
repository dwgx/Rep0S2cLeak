using UnityEngine;

public class PhysGrabObjectSideTransform : MonoBehaviour
{
	[HideInInspector]
	public Vector3 prevPosition;

	[HideInInspector]
	public float velocity;

	private float velocityResetTimer;

	private float impactTimer;

	private MeshRenderer meshRenderer;

	private void Start()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		prevPosition = base.transform.position;
	}

	private void FixedUpdate()
	{
		float num = Vector3.Distance(prevPosition, base.transform.position) / Time.fixedDeltaTime * 5f;
		if (num > velocity)
		{
			velocity = num;
			velocityResetTimer = 0.1f;
		}
		if (velocityResetTimer > 0f)
		{
			velocityResetTimer -= Time.fixedDeltaTime;
		}
		else
		{
			velocity = 0f;
		}
		prevPosition = base.transform.position;
	}
}
