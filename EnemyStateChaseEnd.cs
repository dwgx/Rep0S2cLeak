using UnityEngine;

public class EnemyStateChaseEnd : MonoBehaviour
{
	private Enemy Enemy;

	private PlayerController Player;

	private bool Active;

	public float StateTimeMin;

	public float StateTimeMax;

	private float StateTimer;

	private void Start()
	{
		Enemy = GetComponent<Enemy>();
		Player = PlayerController.instance;
	}

	private void Update()
	{
		if (!Enemy.MasterClient)
		{
			return;
		}
		if (Enemy.CurrentState != EnemyState.ChaseEnd)
		{
			if (Active)
			{
				Active = false;
			}
			return;
		}
		if (!Active)
		{
			Enemy.NavMeshAgent.ResetPath();
			StateTimer = Random.Range(StateTimeMin, StateTimeMax);
			Active = true;
		}
		Enemy.NavMeshAgent.UpdateAgent(0f, 5f);
		StateTimer -= Time.deltaTime;
		if (StateTimer <= 0f)
		{
			Enemy.CurrentState = EnemyState.Roaming;
		}
	}
}
