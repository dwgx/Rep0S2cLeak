using UnityEngine;

public class ItemOrbFeather : MonoBehaviour
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
		if (!itemOrb.itemActive)
		{
			return;
		}
		if (itemOrb.localPlayerAffected)
		{
			PlayerController.instance.Feather(0.1f);
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		foreach (PhysGrabObject item in itemOrb.objectAffected)
		{
			if (!item || !(physGrabObject != item))
			{
				continue;
			}
			PlayerTumble component = item.GetComponent<PlayerTumble>();
			if (!component)
			{
				item.OverrideMass(1f);
				item.OverrideDrag(1f);
				item.OverrideAngularDrag(5f);
				continue;
			}
			component.DisableCustomGravity(0.1f);
			item.OverrideMass(0.05f);
			if (component.playerAvatar.isLocal)
			{
				PlayerController.instance.Feather(0.1f);
			}
		}
	}
}
