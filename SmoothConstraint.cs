using UnityEngine;

public class SmoothConstraint : MonoBehaviour
{
	public Transform targetTransform;

	public float followSpeed = 20f;

	public float followSpeedDistanceMult = 5f;

	public float teleportDistance = 1f;

	private void LateUpdate()
	{
		float num = Vector3.Distance(base.transform.position, targetTransform.position);
		if (num > teleportDistance)
		{
			base.transform.position = targetTransform.position;
			base.transform.rotation = targetTransform.rotation;
			return;
		}
		float num2 = 1f + num / teleportDistance * followSpeedDistanceMult;
		float num3 = followSpeed * num2;
		base.transform.position = Vector3.Lerp(base.transform.position, targetTransform.position, num3 * Time.deltaTime);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, targetTransform.rotation, num3 * Time.deltaTime);
	}
}
