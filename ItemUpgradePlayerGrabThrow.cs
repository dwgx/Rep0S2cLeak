using UnityEngine;

public class ItemUpgradePlayerGrabThrow : MonoBehaviour
{
	private ItemToggle itemToggle;

	private void Start()
	{
		itemToggle = GetComponent<ItemToggle>();
	}

	public void Upgrade()
	{
		PunManager.instance.UpgradePlayerThrowStrength(SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarGetFromPhotonID(itemToggle.playerTogglePhotonID)));
	}
}
