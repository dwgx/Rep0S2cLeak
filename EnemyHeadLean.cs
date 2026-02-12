using UnityEngine;

public class EnemyHeadLean : MonoBehaviour
{
	public Enemy Enemy;

	[Space]
	public float Amount = -500f;

	public float MaxAmount = 20f;

	public float Speed = 10f;

	private void Update()
	{
		if (!(Enemy.FreezeTimer > 0f))
		{
			if (Enemy.NavMeshAgent.AgentVelocity.magnitude < 0.1f)
			{
				base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, Quaternion.Euler(0f, 0f, 0f), 50f * Time.deltaTime);
				return;
			}
			float x = Mathf.Clamp(Enemy.NavMeshAgent.AgentVelocity.magnitude * Amount, 0f - MaxAmount, MaxAmount);
			base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, Quaternion.Euler(x, 0f, 0f), Speed * Time.deltaTime);
		}
	}
}
