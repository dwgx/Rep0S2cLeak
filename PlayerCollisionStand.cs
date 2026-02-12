using UnityEngine;

public class PlayerCollisionStand : MonoBehaviour
{
	public static PlayerCollisionStand instance;

	public PlayerCollisionController CollisionController;

	private CapsuleCollider Collider;

	public LayerMask LayerMask;

	public Transform TargetTransform;

	public Vector3 Offset;

	private bool checkActive;

	private float setBlockedTimer;

	private void Awake()
	{
		instance = this;
		Collider = GetComponent<CapsuleCollider>();
	}

	public bool CheckBlocked()
	{
		if (setBlockedTimer > 0f)
		{
			return true;
		}
		Vector3 point = base.transform.position + Offset + Vector3.up * Collider.radius;
		Vector3 point2 = base.transform.position + Offset + Vector3.up * Collider.height - Vector3.up * Collider.radius;
		if (Physics.OverlapCapsule(point, point2, Collider.radius, LayerMask, QueryTriggerInteraction.Ignore).Length != 0)
		{
			return true;
		}
		if (Physics.OverlapCapsule(point, point2, Collider.radius, LayerMask.GetMask("HideTriggers"), QueryTriggerInteraction.Collide).Length != 0)
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (setBlockedTimer > 0f)
		{
			setBlockedTimer -= Time.deltaTime;
		}
		base.transform.position = TargetTransform.position;
	}

	public void SetBlocked()
	{
		setBlockedTimer = 0.25f;
		PlayerCollision.instance.SetCrouchCollision();
	}
}
