using UnityEngine;

public class EnemyHeadHairTilt : MonoBehaviour
{
	public Transform EnemyTransform;

	private Vector3 ForwardPrev;

	[Space]
	public float Amount = -500f;

	public float MaxAmount = 20f;

	[Space]
	public float RandomMin;

	public float RandomMax;

	private float RandomCurrent;

	private float RandomTimer;

	public float RandomTimeMin;

	public float RandomTimeMax;

	[Space]
	public float SpringFreq = 15f;

	public float SpringDamping = 0.5f;

	private float SpringTarget;

	private float SpringCurrent;

	private float SpringVelocity;

	private SpringUtils.tDampedSpringMotionParams SpringParams = new SpringUtils.tDampedSpringMotionParams();

	private void Update()
	{
		float num = Mathf.Clamp(Vector3.Cross(ForwardPrev, EnemyTransform.forward).y * Amount, 0f - MaxAmount, MaxAmount);
		if (RandomTimer <= 0f && num > 0.1f)
		{
			RandomTimer = Random.Range(RandomTimeMin, RandomTimeMax);
			RandomCurrent = Random.Range(RandomMin, RandomMax);
		}
		else
		{
			RandomTimer -= Time.deltaTime;
		}
		num += RandomCurrent;
		SpringUtils.CalcDampedSpringMotionParams(ref SpringParams, Time.deltaTime, SpringFreq, SpringDamping);
		SpringUtils.UpdateDampedSpringMotion(ref SpringCurrent, ref SpringVelocity, num, in SpringParams);
		base.transform.localRotation = Quaternion.Euler(0f, SpringCurrent, 0f);
		ForwardPrev = EnemyTransform.forward;
	}
}
