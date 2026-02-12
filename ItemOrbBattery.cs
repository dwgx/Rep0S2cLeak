using UnityEngine;

public class ItemOrbBattery : MonoBehaviour, ITargetingCondition
{
	private ItemOrb itemOrb;

	private PhysGrabObject physGrabObject;

	private ItemToggle itemToggle;

	private ItemBattery itemBattery;

	private bool didCharge;

	public bool CustomTargetingCondition(GameObject target)
	{
		return SemiFunc.BatteryChargeCondition(target.GetComponent<ItemBattery>());
	}

	private void Start()
	{
		itemToggle = GetComponent<ItemToggle>();
		itemOrb = GetComponent<ItemOrb>();
		physGrabObject = GetComponent<PhysGrabObject>();
		itemBattery = GetComponent<ItemBattery>();
	}

	private void BatteryDrain(float amount)
	{
		itemBattery.batteryLife -= amount * Time.deltaTime;
	}

	private void Update()
	{
		if (!itemOrb.itemActive)
		{
			didCharge = false;
		}
		else
		{
			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				return;
			}
			bool flag = false;
			foreach (PhysGrabObject item in itemOrb.objectAffected)
			{
				if ((bool)item && physGrabObject != item)
				{
					ItemBattery component = item.GetComponent<ItemBattery>();
					if (component.batteryLife < 100f)
					{
						component.ChargeBattery(base.gameObject, SemiFunc.BatteryGetChargeRate(3));
						BatteryDrain(1.5f);
						flag = true;
						didCharge = true;
					}
				}
			}
			if (didCharge && !flag)
			{
				itemToggle.ToggleItem(toggle: false);
				didCharge = false;
			}
		}
	}
}
