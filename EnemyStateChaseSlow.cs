using UnityEngine;

public class EnemyStateChaseSlow : MonoBehaviour
{
	private Enemy Enemy;

	private bool Active;

	public float Speed;

	public float Acceleration;

	[Space]
	public float StateTimeMin;

	public float StateTimeMax;

	private float StateTimer;

	private void Start()
	{
		Enemy = GetComponent<Enemy>();
	}

	private void Update()
	{
		if (!Enemy.MasterClient)
		{
			return;
		}
		if (Enemy.CurrentState != EnemyState.ChaseSlow)
		{
			if (Active)
			{
				Active = false;
			}
			return;
		}
		if (!Active)
		{
			ChaseAhead();
			StateTimer = Random.Range(StateTimeMin, StateTimeMax);
			Active = true;
		}
		Enemy.SetChaseTimer();
		Enemy.NavMeshAgent.UpdateAgent(Speed, Acceleration);
		if (Vector3.Distance(base.transform.position, Enemy.NavMeshAgent.Agent.destination) < 1f)
		{
			ChaseAhead();
		}
		StateTimer -= Time.deltaTime;
		if (StateTimer <= 0f)
		{
			Enemy.CurrentState = EnemyState.ChaseEnd;
		}
	}

	private void ChaseAhead()
	{
		LevelPoint levelPointAhead = Enemy.GetLevelPointAhead(Enemy.StateChase.ChasePosition);
		if ((bool)levelPointAhead)
		{
			Enemy.StateChase.ChasePosition = levelPointAhead.transform.position;
		}
		Enemy.NavMeshAgent.SetDestination(Enemy.StateChase.ChasePosition);
	}
}
