using UnityEngine;

public class EnemyTriggerAttack : MonoBehaviour
{
	public Enemy Enemy;

	public LayerMask VisionMask;

	public Transform VisionTransform;

	private bool TriggerCheckTimerSet;

	private float TriggerCheckTimer;

	internal bool Attack;

	private void OnTriggerStay(Collider other)
	{
		if (!LevelGenerator.Instance.Generated || TriggerCheckTimer > 0f)
		{
			return;
		}
		TriggerCheckTimerSet = true;
		if (Enemy.CurrentState == EnemyState.Chase || Enemy.CurrentState == EnemyState.LookUnder)
		{
			PlayerTrigger component = other.GetComponent<PlayerTrigger>();
			if ((bool)component)
			{
				bool flag = false;
				if (Enemy.CurrentState == EnemyState.LookUnder && Enemy.StateLookUnder.WaitDone)
				{
					flag = true;
				}
				bool chaseCanReach = Enemy.StateChase.ChaseCanReach;
				PlayerAvatar playerAvatar = component.PlayerAvatar;
				if (playerAvatar.isDisabled || (!Enemy.Vision.VisionTriggered[playerAvatar.photonView.ViewID] && !flag))
				{
					return;
				}
				bool flag2 = true;
				bool flag3 = false;
				if (!chaseCanReach || flag)
				{
					flag2 = false;
					flag3 = true;
				}
				Vector3 position = playerAvatar.PlayerVisionTarget.VisionTransform.transform.position;
				RaycastHit[] array = Physics.RaycastAll(VisionTransform.position, position - VisionTransform.position, (position - VisionTransform.position).magnitude, VisionMask);
				bool flag4 = false;
				RaycastHit[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					RaycastHit raycastHit = array2[i];
					if (!raycastHit.transform.CompareTag("Enemy") && !raycastHit.transform.GetComponent<PlayerTumble>())
					{
						flag4 = true;
					}
				}
				if (flag4)
				{
					if (!flag3)
					{
						flag2 = false;
					}
				}
				else if (flag3)
				{
					flag2 = true;
				}
				if (flag2)
				{
					Attack = true;
				}
			}
		}
		if (Enemy.CurrentState == EnemyState.ChaseBegin)
		{
			return;
		}
		bool flag5 = false;
		int num = 0;
		Vector3 vector = Vector3.zero;
		PhysGrabObject componentInParent = other.GetComponentInParent<PhysGrabObject>();
		StaticGrabObject componentInParent2 = other.GetComponentInParent<StaticGrabObject>();
		if ((bool)componentInParent)
		{
			flag5 = true;
			num = componentInParent.playerGrabbing.Count;
			vector = componentInParent.midPoint;
			if ((bool)componentInParent.GetComponent<EnemyRigidbody>())
			{
				flag5 = false;
			}
		}
		else if ((bool)componentInParent2)
		{
			flag5 = true;
			num = componentInParent2.playerGrabbing.Count;
			vector = componentInParent2.transform.position;
		}
		if (!flag5 || num <= 0 || !(Vector3.Distance(base.transform.position, vector) < Enemy.Vision.VisionDistance))
		{
			return;
		}
		Vector3 direction = vector - VisionTransform.position;
		if (!(Vector3.Dot(VisionTransform.forward, direction.normalized) > 0.8f))
		{
			return;
		}
		RaycastHit hitInfo;
		bool num2 = Physics.Raycast(Enemy.Vision.VisionTransform.position, direction, out hitInfo, direction.magnitude, VisionMask);
		bool flag6 = true;
		if (num2)
		{
			if ((bool)componentInParent)
			{
				if (hitInfo.collider.GetComponentInParent<PhysGrabObject>() != componentInParent)
				{
					flag6 = false;
				}
			}
			else if ((bool)componentInParent2 && hitInfo.collider.GetComponentInParent<StaticGrabObject>() != componentInParent2)
			{
				flag6 = false;
			}
		}
		if (flag6 && Enemy.HasStateInvestigate)
		{
			Enemy.StateInvestigate.Set(vector, _pathFindOnly: false);
		}
	}

	private void Update()
	{
		if (TriggerCheckTimerSet)
		{
			TriggerCheckTimer = 0.2f;
			TriggerCheckTimerSet = false;
		}
		else if (TriggerCheckTimer > 0f)
		{
			TriggerCheckTimer -= Time.deltaTime;
		}
	}
}
