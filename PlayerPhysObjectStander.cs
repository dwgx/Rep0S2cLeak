using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysObjectStander : MonoBehaviour
{
	public LayerMask layerMask;

	private SphereCollider Collider;

	internal List<PhysGrabObject> physGrabObjects = new List<PhysGrabObject>();

	private float checkTimer;

	private void Awake()
	{
		Collider = GetComponent<SphereCollider>();
	}

	private void Update()
	{
		if (checkTimer <= 0f)
		{
			physGrabObjects.Clear();
			Collider[] array = Physics.OverlapSphere(base.transform.position, Collider.radius, layerMask);
			if (array.Length != 0)
			{
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					PhysGrabObject physGrabObject = collider.gameObject.GetComponent<PhysGrabObject>();
					if (!physGrabObject)
					{
						physGrabObject = collider.gameObject.GetComponentInParent<PhysGrabObject>();
					}
					if ((bool)physGrabObject)
					{
						physGrabObjects.Add(physGrabObject);
					}
				}
			}
			checkTimer = 0.1f;
		}
		else
		{
			checkTimer -= 1f * Time.deltaTime;
		}
	}
}
