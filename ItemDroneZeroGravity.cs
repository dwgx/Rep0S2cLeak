using Photon.Pun;
using UnityEngine;

public class ItemDroneZeroGravity : MonoBehaviour
{
	private ItemDrone itemDrone;

	private PhysGrabObject myPhysGrabObject;

	private ItemEquippable itemEquippable;

	private float tumbleEnemyTimer;

	private ItemBattery itemBattery;

	private void Start()
	{
		itemDrone = GetComponent<ItemDrone>();
		myPhysGrabObject = GetComponent<PhysGrabObject>();
		itemEquippable = GetComponent<ItemEquippable>();
		itemBattery = GetComponent<ItemBattery>();
	}

	private void FixedUpdate()
	{
		if (itemDrone.magnetActive && (bool)itemDrone.magnetTargetPhysGrabObject)
		{
			if ((bool)itemDrone.playerTumbleTarget)
			{
				itemBattery.batteryLife -= 2f * Time.fixedDeltaTime;
				itemDrone.magnetTargetPhysGrabObject.OverrideMaterial(SemiFunc.PhysicMaterialSticky());
			}
			EnemyParent componentInParent = itemDrone.magnetTargetPhysGrabObject.GetComponentInParent<EnemyParent>();
			if ((bool)componentInParent)
			{
				SemiFunc.ItemAffectEnemyBatteryDrain(componentInParent, itemBattery, tumbleEnemyTimer, Time.fixedDeltaTime);
				tumbleEnemyTimer += Time.fixedDeltaTime;
			}
		}
	}

	private void Update()
	{
		if (!itemDrone.itemActivated)
		{
			tumbleEnemyTimer = 0f;
		}
		if (itemEquippable.isEquipped)
		{
			return;
		}
		if (itemDrone.itemActivated && itemDrone.magnetActive && (bool)itemDrone.playerAvatarTarget && itemDrone.targetIsLocalPlayer)
		{
			itemBattery.batteryLife -= 2f * Time.deltaTime;
			PlayerController.instance.AntiGravity(0.1f);
		}
		if ((GameManager.instance.gameMode != 1 || PhotonNetwork.IsMasterClient) && itemDrone.itemActivated)
		{
			myPhysGrabObject.OverrideZeroGravity();
			myPhysGrabObject.OverrideDrag(1f);
			myPhysGrabObject.OverrideAngularDrag(10f);
			if (itemDrone.magnetActive && (bool)itemDrone.magnetTargetPhysGrabObject)
			{
				itemDrone.magnetTargetPhysGrabObject.OverrideDrag(0.1f);
				itemDrone.magnetTargetPhysGrabObject.OverrideAngularDrag(0.1f);
				itemDrone.magnetTargetPhysGrabObject.OverrideZeroGravity();
			}
		}
	}
}
