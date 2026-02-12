using UnityEngine;

public class ItemDroneHeal : MonoBehaviour, ITargetingCondition
{
	private ItemDrone itemDrone;

	private PhysGrabObject myPhysGrabObject;

	private float healRate = 2f;

	private float healTimer;

	private int healAmount = 10;

	private ItemEquippable itemEquippable;

	private void Start()
	{
		itemEquippable = GetComponent<ItemEquippable>();
		itemDrone = GetComponent<ItemDrone>();
		myPhysGrabObject = GetComponent<PhysGrabObject>();
	}

	public bool CustomTargetingCondition(GameObject target)
	{
		PlayerAvatar component = target.GetComponent<PlayerAvatar>();
		return component.playerHealth.health < component.playerHealth.maxHealth;
	}

	private void Update()
	{
		if (itemEquippable.isEquipped || !itemDrone.itemActivated)
		{
			return;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			myPhysGrabObject.OverrideZeroGravity();
			myPhysGrabObject.OverrideDrag(1f);
			myPhysGrabObject.OverrideAngularDrag(10f);
		}
		if (!itemDrone.magnetActive || !itemDrone.playerAvatarTarget)
		{
			return;
		}
		healTimer += Time.deltaTime;
		if (healTimer > healRate)
		{
			itemDrone.playerAvatarTarget.playerHealth.Heal(healAmount);
			if (itemDrone.playerAvatarTarget.playerHealth.health >= itemDrone.playerAvatarTarget.playerHealth.maxHealth)
			{
				itemDrone.MagnetActiveToggle(toggleBool: false);
			}
			healTimer = 0f;
		}
	}
}
