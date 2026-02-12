using UnityEngine;

public class PhysGrabObjectCollider : MonoBehaviour
{
	[HideInInspector]
	public int colliderID;

	private PhysGrabObject physGrabObject;

	private void Start()
	{
		physGrabObject = GetComponentInParent<PhysGrabObject>();
	}

	private void OnDestroy()
	{
		if ((bool)physGrabObject)
		{
			physGrabObject.colliders.Remove(base.transform);
		}
	}
}
