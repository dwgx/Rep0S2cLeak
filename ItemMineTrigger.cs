using UnityEngine;

public class ItemMineTrigger : MonoBehaviour
{
	private enum TargetType
	{
		None,
		Enemy,
		RigidBody,
		Player
	}

	private PhysGrabObject parentPhysGrabObject;

	private ItemMine itemMine;

	public bool enemyTrigger;

	private bool targetAcquired;

	private float visionCheckTimer;

	private void Start()
	{
		parentPhysGrabObject = GetComponentInParent<PhysGrabObject>();
		itemMine = GetComponentInParent<ItemMine>();
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			Object.Destroy(this);
		}
	}

	private void Update()
	{
		if (SemiFunc.RunIsShop() && targetAcquired && (bool)itemMine && itemMine.state == ItemMine.States.Disarmed)
		{
			targetAcquired = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!targetAcquired && (bool)itemMine && itemMine.state == ItemMine.States.Armed && PassesTriggerChecks(other))
		{
			TryAcquireTarget(other);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (!targetAcquired && (bool)itemMine && itemMine.state == ItemMine.States.Armed && PassesTriggerChecks(other))
		{
			visionCheckTimer += Time.deltaTime;
			if (visionCheckTimer > 0.5f)
			{
				visionCheckTimer = 0f;
				TryAcquireTarget(other);
			}
		}
	}

	private bool PassesTriggerChecks(Collider other)
	{
		PhysGrabObject componentInParent = other.GetComponentInParent<PhysGrabObject>();
		if (enemyTrigger)
		{
			if (!componentInParent || !componentInParent.isEnemy)
			{
				return false;
			}
		}
		else if ((bool)componentInParent && componentInParent.isEnemy && !itemMine.triggeredByEnemies)
		{
			return false;
		}
		if ((bool)componentInParent && !itemMine.triggeredByRigidBodies && !componentInParent.isEnemy && !componentInParent.isPlayer)
		{
			return false;
		}
		PlayerAvatar playerAvatar = other.GetComponentInParent<PlayerAvatar>();
		PlayerController componentInParent2 = other.GetComponentInParent<PlayerController>();
		if ((bool)componentInParent2)
		{
			playerAvatar = componentInParent2.playerAvatarScript;
		}
		if ((bool)componentInParent && !itemMine.triggeredByPlayers && (componentInParent.isPlayer || (bool)playerAvatar))
		{
			return false;
		}
		if ((bool)componentInParent && !componentInParent.isEnemy && !componentInParent.grabbed && componentInParent.rb.velocity.magnitude < 0.1f && componentInParent.rb.angularVelocity.magnitude < 0.1f)
		{
			return false;
		}
		if ((bool)(componentInParent ? componentInParent.GetComponent<PlayerTumble>() : null) && !itemMine.triggeredByPlayers)
		{
			return false;
		}
		return true;
	}

	private void TryAcquireTarget(Collider other)
	{
		if (targetAcquired)
		{
			return;
		}
		PhysGrabObject componentInParent = other.GetComponentInParent<PhysGrabObject>();
		PlayerAvatar componentInParent2 = other.GetComponentInParent<PlayerAvatar>();
		PlayerAccess componentInParent3 = other.GetComponentInParent<PlayerAccess>();
		PlayerController playerController = (componentInParent3 ? componentInParent3.GetComponentInChildren<PlayerController>() : null);
		Vector3 position = itemMine.transform.position;
		if ((bool)componentInParent)
		{
			Vector3 midPoint = componentInParent.midPoint;
			if (!VisionObstruct(position, midPoint, componentInParent))
			{
				if (componentInParent.isEnemy)
				{
					LockOnTarget(TargetType.Enemy, componentInParent, componentInParent2, playerController);
					return;
				}
				if (!componentInParent.isPlayer && componentInParent != parentPhysGrabObject)
				{
					LockOnTarget(TargetType.RigidBody, componentInParent, componentInParent2, playerController);
					return;
				}
			}
		}
		if ((bool)componentInParent2)
		{
			Vector3 position2 = componentInParent2.PlayerVisionTarget.VisionTransform.position;
			if (!VisionObstruct(position, position2, null))
			{
				LockOnTarget(TargetType.Player, componentInParent, componentInParent2, playerController);
				return;
			}
		}
		if (!playerController)
		{
			return;
		}
		componentInParent2 = playerController.playerAvatarScript;
		if ((bool)componentInParent2)
		{
			Vector3 position3 = componentInParent2.PlayerVisionTarget.VisionTransform.position;
			if (!VisionObstruct(position, position3, null))
			{
				LockOnTarget(TargetType.Player, componentInParent, componentInParent2, playerController);
			}
		}
	}

	private void LockOnTarget(TargetType type, PhysGrabObject physObj, PlayerAvatar playerAvatar, PlayerController playerController)
	{
		if (!itemMine)
		{
			return;
		}
		switch (type)
		{
		case TargetType.Enemy:
			itemMine.wasTriggeredByEnemy = true;
			itemMine.triggeredPhysGrabObject = physObj;
			itemMine.triggeredTransform = physObj.transform;
			itemMine.triggeredPosition = physObj.transform.position;
			break;
		case TargetType.RigidBody:
			itemMine.wasTriggeredByRigidBody = true;
			itemMine.triggeredPhysGrabObject = physObj;
			itemMine.triggeredTransform = physObj.transform;
			itemMine.triggeredPosition = physObj.transform.position;
			break;
		case TargetType.Player:
			itemMine.wasTriggeredByPlayer = true;
			if ((bool)playerAvatar)
			{
				itemMine.triggeredPlayerAvatar = playerAvatar;
				PlayerTumble tumble = playerAvatar.tumble;
				if ((bool)tumble)
				{
					itemMine.triggeredPlayerTumble = tumble;
					itemMine.triggeredPhysGrabObject = tumble.physGrabObject;
				}
				itemMine.triggeredTransform = playerAvatar.PlayerVisionTarget.VisionTransform;
				itemMine.triggeredPosition = playerAvatar.PlayerVisionTarget.VisionTransform.position;
			}
			else if ((bool)physObj)
			{
				PlayerTumble componentInParent = physObj.GetComponentInParent<PlayerTumble>();
				if ((bool)componentInParent)
				{
					itemMine.triggeredPlayerAvatar = componentInParent.playerAvatar;
					itemMine.triggeredPlayerTumble = componentInParent;
					itemMine.triggeredPhysGrabObject = componentInParent.physGrabObject;
					itemMine.triggeredTransform = componentInParent.playerAvatar.PlayerVisionTarget.VisionTransform;
					itemMine.triggeredPosition = componentInParent.playerAvatar.PlayerVisionTarget.VisionTransform.position;
				}
			}
			break;
		}
		targetAcquired = true;
		itemMine.SetTriggered();
	}

	private bool VisionObstruct(Vector3 start, Vector3 end, PhysGrabObject targetPhysObj)
	{
		int layerMask = SemiFunc.LayerMaskGetVisionObstruct();
		Vector3 normalized = (end - start).normalized;
		float maxDistance = Vector3.Distance(start, end);
		RaycastHit[] array = Physics.RaycastAll(start, normalized, maxDistance, layerMask);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			if (raycastHit.collider.CompareTag("Wall") || raycastHit.collider.CompareTag("Ceiling"))
			{
				return true;
			}
		}
		return false;
	}
}
