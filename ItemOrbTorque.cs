using UnityEngine;

public class ItemOrbTorque : MonoBehaviour
{
	private ItemOrb itemOrb;

	private PhysGrabObject physGrabObject;

	private void Start()
	{
		itemOrb = GetComponent<ItemOrb>();
		physGrabObject = GetComponent<PhysGrabObject>();
	}

	private void FixedUpdate()
	{
		if (!itemOrb.itemActive || !SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		foreach (PhysGrabObject item in itemOrb.objectAffected)
		{
			if ((bool)item && physGrabObject != item)
			{
				float num = Vector3.Distance(new Vector3(item.rb.position.x, 0f, item.rb.position.z), new Vector3(base.transform.position.x, 0f, base.transform.position.z));
				Rigidbody rb = item.rb;
				_ = (base.transform.position - rb.position).normalized;
				float num2 = 0.5f;
				if (num < num2)
				{
					_ = Mathf.Clamp(num - 0.2f, 0f, num2) / num2;
				}
				float num3 = 0.5f;
				float num4 = 1f;
				num2 = 1f;
				if (num < num2)
				{
					num4 = Mathf.Clamp(num - 0.5f, 0f, num2) / num2;
					num3 *= num4;
				}
				if (item.isEnemy)
				{
					num3 *= 5f;
				}
				item.OverrideFragility(0.1f);
				item.OverrideMaterial(SemiFunc.PhysicMaterialSticky());
			}
		}
	}
}
