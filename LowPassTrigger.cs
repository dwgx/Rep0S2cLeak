using UnityEngine;

public class LowPassTrigger : MonoBehaviour
{
	private void Awake()
	{
		ColliderSetup(GetComponent<Collider>());
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			ColliderSetup(collider);
		}
	}

	private void ColliderSetup(Collider _collider)
	{
		if ((bool)_collider)
		{
			_collider.isTrigger = true;
			_collider.tag = "LowPassTrigger";
			_collider.gameObject.layer = LayerMask.NameToLayer("LowPassTrigger");
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		BoxCollider component = GetComponent<BoxCollider>();
		if ((bool)component)
		{
			Gizmos.color = new Color(0.23f, 1f, 0.47f, 0.52f);
			Gizmos.DrawWireCube(component.center, component.size);
			Gizmos.color = new Color(0.19f, 1f, 0.68f, 0.27f);
			Gizmos.DrawCube(component.center, component.size);
			return;
		}
		SphereCollider component2 = GetComponent<SphereCollider>();
		if ((bool)component2)
		{
			Gizmos.color = new Color(0.23f, 1f, 0.47f, 0.52f);
			Gizmos.DrawWireSphere(component2.center, component2.radius);
			Gizmos.color = new Color(0.19f, 1f, 0.68f, 0.27f);
			Gizmos.DrawSphere(component2.center, component2.radius);
		}
	}
}
