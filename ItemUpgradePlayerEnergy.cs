using UnityEngine;

public class ItemUpgradePlayerEnergy : MonoBehaviour
{
	private ItemToggle itemToggle;

	private void Start()
	{
		itemToggle = GetComponent<ItemToggle>();
	}

	public void Upgrade()
	{
		PunManager.instance.UpgradePlayerEnergy(SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarGetFromPhotonID(itemToggle.playerTogglePhotonID)));
	}
}
