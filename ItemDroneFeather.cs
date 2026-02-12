using Photon.Pun;
using UnityEngine;

public class ItemDroneFeather : MonoBehaviour
{
	private ItemDrone itemDrone;

	private PhysGrabObject myPhysGrabObject;

	private ItemEquippable itemEquippable;

	private ItemBattery itemBattery;

	private void Start()
	{
		itemDrone = GetComponent<ItemDrone>();
		myPhysGrabObject = GetComponent<PhysGrabObject>();
		itemEquippable = GetComponent<ItemEquippable>();
		itemBattery = GetComponent<ItemBattery>();
	}

	private void BatteryDrain(float amount)
	{
		itemBattery.batteryLife -= amount * Time.fixedDeltaTime;
	}

	private void FixedUpdate()
	{
		if (itemEquippable.isEquipped)
		{
			return;
		}
		if (itemDrone.itemActivated && itemDrone.magnetActive && (bool)itemDrone.playerAvatarTarget && itemDrone.targetIsLocalPlayer)
		{
			PlayerController.instance.Feather(0.1f);
			BatteryDrain(2f);
		}
		if ((GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient) || !itemDrone.itemActivated)
		{
			return;
		}
		myPhysGrabObject.OverrideZeroGravity();
		myPhysGrabObject.OverrideDrag(1f);
		myPhysGrabObject.OverrideAngularDrag(10f);
		if (!itemDrone.magnetActive || !itemDrone.magnetTargetPhysGrabObject)
		{
			return;
		}
		PlayerTumble component = itemDrone.magnetTargetPhysGrabObject.GetComponent<PlayerTumble>();
		if (!component)
		{
			itemDrone.magnetTargetPhysGrabObject.OverrideMass(1f);
			itemDrone.magnetTargetPhysGrabObject.OverrideDrag(1f);
			itemDrone.magnetTargetPhysGrabObject.OverrideAngularDrag(5f);
			return;
		}
		component.DisableCustomGravity(0.1f);
		itemDrone.magnetTargetPhysGrabObject.OverrideMass(0.5f);
		BatteryDrain(2f);
		if (component.playerAvatar.isLocal)
		{
			PlayerController.instance.Feather(0.1f);
		}
	}
}
