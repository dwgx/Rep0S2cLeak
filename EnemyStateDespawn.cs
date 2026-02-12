using UnityEngine;
using UnityEngine.Events;

public class EnemyStateDespawn : MonoBehaviour
{
	private Enemy Enemy;

	private bool Active;

	public bool StuckDespawn = true;

	public int StuckDespawnCount = 10;

	[Space]
	public bool ChaseDespawn = true;

	public float DespawnAfterChaseTime = 10f;

	internal float ChaseTimer;

	internal float ChaseResetTimer;

	[Space]
	public UnityEvent OnDespawn;

	private void Start()
	{
		Enemy = GetComponent<Enemy>();
	}

	private void Update()
	{
		if (Enemy.MasterClient)
		{
			if (ChaseDespawn)
			{
				ChaseDespawnLogic();
			}
			if (StuckDespawn)
			{
				StuckDespawnLogic();
			}
		}
		if (Enemy.CurrentState != EnemyState.Despawn)
		{
			if (Active)
			{
				Active = false;
			}
			return;
		}
		if (!Active)
		{
			Active = true;
		}
		_ = Enemy.MasterClient;
	}

	public void Despawn()
	{
		OnDespawn.Invoke();
		Enemy.EnemyParent.Despawn();
	}

	private void ChaseDespawnLogic()
	{
		if (DespawnAfterChaseTime == 0f)
		{
			return;
		}
		if (Enemy.CurrentState == EnemyState.Chase || Enemy.CurrentState == EnemyState.ChaseSlow || Enemy.CurrentState == EnemyState.LookUnder)
		{
			ChaseTimer += Time.deltaTime;
			ChaseResetTimer = 10f;
			if (ChaseTimer >= DespawnAfterChaseTime)
			{
				Enemy.CurrentState = EnemyState.Despawn;
				ChaseTimer = 0f;
			}
		}
		else if (ChaseResetTimer <= 0f)
		{
			ChaseTimer = 0f;
		}
		else
		{
			ChaseResetTimer -= Time.deltaTime;
		}
	}

	private void StuckDespawnLogic()
	{
		if (Enemy.StuckCount >= StuckDespawnCount && Enemy.CurrentState != EnemyState.Despawn)
		{
			Enemy.Vision.DisableTimer = 0.25f;
			Enemy.CurrentState = EnemyState.Despawn;
		}
	}
}
