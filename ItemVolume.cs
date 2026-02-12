using System.Collections.Generic;
using UnityEngine;

public class ItemVolume : MonoBehaviour
{
	public SemiFunc.itemVolume itemVolume;

	public SemiFunc.itemSecretShopType itemSecretShopType;

	public List<GameObject> volumes = new List<GameObject>();

	private ItemAttributes itemAttributes;

	private void Start()
	{
		itemAttributes = GetComponentInParent<ItemAttributes>();
		if ((bool)itemAttributes)
		{
			base.gameObject.tag = "Untagged";
		}
		if (SemiFunc.IsNotMasterClient())
		{
			Object.Destroy(this);
		}
	}

	private void OnValidate()
	{
		if (SemiFunc.OnValidateCheck())
		{
			return;
		}
		ItemAttributes componentInParent = GetComponentInParent<ItemAttributes>();
		if ((bool)componentInParent)
		{
			if (itemVolume != componentInParent.item.itemVolume)
			{
				itemVolume = componentInParent.item.itemVolume;
			}
			string text = "Item Volume " + itemVolume;
			if (base.gameObject.name != text)
			{
				base.gameObject.name = text;
			}
		}
	}

	private void OnDrawGizmos()
	{
		ItemAttributes componentInParent = GetComponentInParent<ItemAttributes>();
		int num = 0;
		foreach (GameObject volume in volumes)
		{
			if (itemVolume == (SemiFunc.itemVolume)num)
			{
				Color color = (Gizmos.color = Color.yellow);
				Gizmos.matrix = Matrix4x4.TRS(volume.transform.position, volume.transform.rotation, volume.transform.localScale);
				Gizmos.DrawWireCube(new Vector3(0f, 0f, 0f), Vector3.one);
				color.a = 0.5f;
				Gizmos.color = color;
				if (!componentInParent)
				{
					Gizmos.DrawCube(Vector3.zero, Vector3.one);
				}
				Gizmos.matrix = Matrix4x4.identity;
			}
			num++;
		}
	}
}
