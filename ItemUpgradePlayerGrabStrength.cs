using UnityEngine;

public class ItemUpgradePlayerGrabStrength : MonoBehaviour
{
	private ItemToggle itemToggle;

	private void Start()
	{
		itemToggle = GetComponent<ItemToggle>();
	}

	public void Upgrade()
	{
		PunManager.instance.UpgradePlayerGrabStrength(SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarGetFromPhotonID(itemToggle.playerTogglePhotonID)));
	}
}
