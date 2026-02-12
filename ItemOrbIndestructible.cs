using UnityEngine;

public class ItemOrbIndestructible : MonoBehaviour
{
	private ItemOrb itemOrb;

	private PhysGrabObject physGrabObject;

	private void Start()
	{
		itemOrb = GetComponent<ItemOrb>();
		physGrabObject = GetComponent<PhysGrabObject>();
	}

	private void Update()
	{
		if (!itemOrb.itemActive || !SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		foreach (PhysGrabObject item in itemOrb.objectAffected)
		{
			if ((bool)item && physGrabObject != item)
			{
				item.OverrideIndestructible();
			}
		}
	}
}
