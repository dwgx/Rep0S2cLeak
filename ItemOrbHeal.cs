using UnityEngine;

public class ItemOrbHeal : MonoBehaviour
{
	private ItemOrb itemOrb;

	private ItemBattery itemBattery;

	private PhysGrabObject physGrabObject;

	private float healRate = 2f;

	private float healTimer;

	private int healAmount = 10;

	private void Start()
	{
		itemOrb = GetComponent<ItemOrb>();
		physGrabObject = GetComponent<PhysGrabObject>();
		itemBattery = GetComponent<ItemBattery>();
	}

	private void Update()
	{
		if (itemOrb.itemActive && !(itemBattery.batteryLife <= 0f) && itemOrb.localPlayerAffected)
		{
			if (healTimer > healRate)
			{
				PlayerController.instance.playerAvatarScript.playerHealth.Heal(healAmount);
				healTimer = 0f;
			}
			healTimer += Time.deltaTime;
		}
	}
}
