using UnityEngine;
using UnityEngine.AI;

public class EnemyHeadHairLean : MonoBehaviour
{
	public NavMeshAgent Agent;

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
		if (RandomTimer <= 0f && Agent.velocity.magnitude > 0.1f)
		{
			RandomTimer = Random.Range(RandomTimeMin, RandomTimeMax);
			RandomCurrent = Random.Range(RandomMin, RandomMax);
		}
		else
		{
			RandomTimer -= Time.deltaTime;
		}
		float equilibriumPos = 0f;
		if (Agent.velocity.magnitude > 0.1f)
		{
			equilibriumPos = Mathf.Clamp(Agent.velocity.magnitude * Amount, 0f - MaxAmount, MaxAmount) + RandomCurrent;
		}
		SpringUtils.CalcDampedSpringMotionParams(ref SpringParams, Time.deltaTime, SpringFreq, SpringDamping);
		SpringUtils.UpdateDampedSpringMotion(ref SpringCurrent, ref SpringVelocity, equilibriumPos, in SpringParams);
		base.transform.localRotation = Quaternion.Euler(SpringCurrent, 0f, 0f);
	}
}
