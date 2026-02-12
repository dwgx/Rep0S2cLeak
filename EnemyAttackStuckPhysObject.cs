using UnityEngine;
using UnityEngine.Events;

public class EnemyAttackStuckPhysObject : MonoBehaviour
{
	private Enemy Enemy;

	public float Range = 1f;

	public int StuckCount = 3;

	[Space]
	public UnityEvent OnActiveImpulse;

	internal bool Active;

	internal PhysGrabObject TargetObject;

	internal float AttackedTimer;

	private float CheckTimer;

	private void Start()
	{
		Enemy = GetComponent<Enemy>();
	}

	private void Update()
	{
		if (AttackedTimer > 0f)
		{
			AttackedTimer -= Time.deltaTime;
		}
		if (CheckTimer > 0f)
		{
			CheckTimer -= Time.deltaTime;
			if (CheckTimer <= 0f)
			{
				CheckTimer = 0f;
			}
		}
		else if (Active)
		{
			Reset();
		}
	}

	public bool Check()
	{
		CheckTimer = 0.1f;
		if (Active)
		{
			return false;
		}
		if (Enemy.StuckCount >= StuckCount)
		{
			Get();
			return true;
		}
		return false;
	}

	public void Get()
	{
		if (Active)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(Enemy.Vision.VisionTransform.position, Range, LayerMask.GetMask("PhysGrabObject"));
		float num = 1000f;
		PhysGrabObject physGrabObject = null;
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (!collider.GetComponentInParent<EnemyRigidbody>())
			{
				PhysGrabObject componentInParent = collider.GetComponentInParent<PhysGrabObject>();
				float num2 = Vector3.Distance(Enemy.Vision.VisionTransform.position, componentInParent.centerPoint);
				if (num2 < num)
				{
					num = num2;
					physGrabObject = componentInParent;
				}
			}
		}
		if ((bool)physGrabObject)
		{
			Active = true;
			TargetObject = physGrabObject;
			OnActiveImpulse.Invoke();
			Enemy.StuckCount = 0;
		}
	}

	public void Reset()
	{
		Enemy.StuckCount = 0;
		TargetObject = null;
		Active = false;
	}
}
