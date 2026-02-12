using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Other/Item")]
public class Item : ScriptableObject
{
	public bool disabled;

	[Space]
	public string itemName = "N/A";

	public string description;

	[Space]
	public SemiFunc.itemType itemType;

	public SemiFunc.emojiIcon emojiIcon;

	public SemiFunc.itemVolume itemVolume;

	public SemiFunc.itemSecretShopType itemSecretShopType;

	[Space]
	public ColorPresets colorPreset;

	public PrefabRef prefab;

	public Value value;

	[Space]
	public int maxAmount = 1;

	public int maxAmountInShop = 1;

	[Space]
	public bool maxPurchase;

	public int maxPurchaseAmount = 1;

	[Space]
	public Quaternion spawnRotationOffset;

	public bool physicalItem = true;
}
