using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysObjectFinder : MonoBehaviour
{
	private bool coroutineActive;

	private CapsuleCollider capsuleCollider;

	internal List<PhysGrabObject> physGrabObjects = new List<PhysGrabObject>();

	private void Awake()
	{
		capsuleCollider = GetComponent<CapsuleCollider>();
	}

	private void OnEnable()
	{
		CoroutineActivate();
	}

	private void OnDisable()
	{
		coroutineActive = false;
		StopAllCoroutines();
	}

	private void CoroutineActivate()
	{
		if (!coroutineActive)
		{
			coroutineActive = true;
			StartCoroutine(Logic());
		}
	}

	private IEnumerator Logic()
	{
		while (true)
		{
			physGrabObjects.Clear();
			Vector3 point = base.transform.position + Vector3.up * capsuleCollider.radius;
			Vector3 point2 = base.transform.position + Vector3.up * capsuleCollider.height - Vector3.up * capsuleCollider.radius;
			Collider[] array = Physics.OverlapCapsule(point, point2, capsuleCollider.radius, SemiFunc.LayerMaskGetPhysGrabObject());
			for (int i = 0; i < array.Length; i++)
			{
				PhysGrabObject componentInParent = array[i].GetComponentInParent<PhysGrabObject>();
				if ((bool)componentInParent)
				{
					physGrabObjects.Add(componentInParent);
				}
			}
			yield return new WaitForSeconds(0.05f);
		}
	}
}
