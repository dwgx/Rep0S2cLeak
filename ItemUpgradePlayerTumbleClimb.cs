using UnityEngine;

public class ItemUpgradePlayerTumbleClimb : MonoBehaviour
{
	private ItemToggle itemToggle;

	private void Start()
	{
		itemToggle = GetComponent<ItemToggle>();
	}

	public void Upgrade()
	{
		PunManager.instance.UpgradePlayerTumbleClimb(SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarGetFromPhotonID(itemToggle.playerTogglePhotonID)));
	}
}
