using UnityEngine;

public class ItemDroneTargetingCondition : MonoBehaviour, ITargetingCondition
{
	public bool CustomTargetingCondition(GameObject target)
	{
		target.GetComponent<PhysGrabObjectImpactDetector>();
		return target.CompareTag("Enemy");
	}
}
