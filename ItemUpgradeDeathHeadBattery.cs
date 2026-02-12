using UnityEngine;

public class ItemUpgradeDeathHeadBattery : MonoBehaviour
{
	private ItemToggle itemToggle;

	private void Start()
	{
		itemToggle = GetComponent<ItemToggle>();
	}

	public void Upgrade()
	{
		PunManager.instance.UpgradeDeathHeadBattery(SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarGetFromPhotonID(itemToggle.playerTogglePhotonID)));
	}
}
