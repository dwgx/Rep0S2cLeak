using UnityEngine;

public class ItemOrbMagnet : MonoBehaviour
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
				Vector3 normalized = (physGrabObject.transform.position - item.transform.position).normalized;
				if ((physGrabObject.transform.position - item.transform.position).magnitude > 0.45f)
				{
					item.rb.AddForce(normalized * Mathf.Clamp(item.rb.mass * 10f, 0.2f, 5f));
				}
				item.rb.velocity = physGrabObject.rb.velocity;
				item.OverrideZeroGravity();
			}
		}
	}
}
