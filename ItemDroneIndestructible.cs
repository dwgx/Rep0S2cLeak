using Photon.Pun;
using UnityEngine;

public class ItemDroneIndestructible : MonoBehaviour
{
	private ItemDrone itemDrone;

	private PhysGrabObject myPhysGrabObject;

	private ItemEquippable itemEquippable;

	private void Start()
	{
		itemDrone = GetComponent<ItemDrone>();
		myPhysGrabObject = GetComponent<PhysGrabObject>();
		itemEquippable = GetComponent<ItemEquippable>();
	}

	private void Update()
	{
		if (!itemEquippable.isEquipped && (GameManager.instance.gameMode != 1 || PhotonNetwork.IsMasterClient) && itemDrone.itemActivated)
		{
			myPhysGrabObject.OverrideZeroGravity();
			myPhysGrabObject.OverrideDrag(1f);
			myPhysGrabObject.OverrideAngularDrag(10f);
			if (itemDrone.magnetActive && (bool)itemDrone.magnetTargetPhysGrabObject)
			{
				itemDrone.magnetTargetPhysGrabObject.OverrideIndestructible();
			}
		}
	}
}
