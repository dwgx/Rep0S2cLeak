using UnityEngine;

public class EnemyHeadEye : MonoBehaviour
{
	public Transform Target;

	public EnemyHeadEyeTarget EyeTarget;

	private float CurrentX;

	private float CurrentY;

	private void Update()
	{
		Quaternion quaternion = Quaternion.LookRotation(Target.position - base.transform.position);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, EyeTarget.Speed * Time.deltaTime);
		base.transform.localRotation = SemiFunc.ClampRotation(base.transform.localRotation, EyeTarget.Limit);
	}
}
