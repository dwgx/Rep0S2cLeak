using UnityEngine;

public class EnemyHeadBotTilt : MonoBehaviour
{
	private Vector3 ForwardPrev;

	[Space]
	public float Amount = -500f;

	public float MaxAmount = 20f;

	[Space]
	public float SpringFreq = 15f;

	public float SpringDamping = 0.5f;

	private float SpringTarget;

	private float SpringCurrent;

	private float SpringVelocity;

	private SpringUtils.tDampedSpringMotionParams SpringParams = new SpringUtils.tDampedSpringMotionParams();

	private void Update()
	{
		float equilibriumPos = Mathf.Clamp(Vector3.Cross(ForwardPrev, base.transform.forward).y * Amount, 0f - MaxAmount, MaxAmount);
		SpringUtils.CalcDampedSpringMotionParams(ref SpringParams, Time.deltaTime, SpringFreq, SpringDamping);
		SpringUtils.UpdateDampedSpringMotion(ref SpringCurrent, ref SpringVelocity, equilibriumPos, in SpringParams);
		base.transform.localRotation = Quaternion.Euler(0f, (0f - SpringCurrent) * 0.5f, SpringCurrent);
		ForwardPrev = base.transform.forward;
	}
}
