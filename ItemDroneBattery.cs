using UnityEngine;

public class ItemDroneBattery : MonoBehaviour, ITargetingCondition
{
	private ItemDrone itemDrone;

	private PhysGrabObject myPhysGrabObject;

	private ItemEquippable itemEquippable;

	private ItemBattery itemBattery;

	private void Start()
	{
		itemBattery = GetComponent<ItemBattery>();
		itemEquippable = GetComponent<ItemEquippable>();
		itemDrone = GetComponent<ItemDrone>();
		myPhysGrabObject = GetComponent<PhysGrabObject>();
	}

	public bool CustomTargetingCondition(GameObject target)
	{
		return SemiFunc.BatteryChargeCondition(target.GetComponent<ItemBattery>());
	}

	private void Update()
	{
		if (itemEquippable.isEquipped || !SemiFunc.IsMasterClientOrSingleplayer() || !itemDrone.itemActivated)
		{
			return;
		}
		myPhysGrabObject.OverrideZeroGravity();
		myPhysGrabObject.OverrideDrag(1f);
		myPhysGrabObject.OverrideAngularDrag(10f);
		if (itemDrone.magnetActive && (bool)itemDrone.magnetTargetPhysGrabObject)
		{
			ItemBattery component = itemDrone.magnetTargetPhysGrabObject.GetComponent<ItemBattery>();
			if ((bool)component)
			{
				component.ChargeBattery(base.gameObject, 5f);
				itemBattery.Drain(5f);
			}
			if (component.batteryLife > 99f)
			{
				itemDrone.MagnetActiveToggle(toggleBool: false);
			}
		}
	}
}
