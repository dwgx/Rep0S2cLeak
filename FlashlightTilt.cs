using UnityEngine;

public class FlashlightTilt : MonoBehaviour
{
	public SpringQuaternion spring;

	private void Update()
	{
		Quaternion targetRotation = Quaternion.LookRotation(base.transform.parent.forward, Vector3.up);
		base.transform.rotation = SemiFunc.SpringQuaternionGet(spring, targetRotation);
	}
}
