using UnityEngine;
using UnityEngine.Events;

public class EnemyStateSpawn : MonoBehaviour
{
	private Enemy Enemy;

	private bool Active;

	private float WaitTimer;

	public UnityEvent OnSpawn;

	private void Start()
	{
		Enemy = GetComponent<Enemy>();
	}

	private void Update()
	{
		if (Enemy.CurrentState != EnemyState.Spawn)
		{
			if (Active)
			{
				Active = false;
			}
			return;
		}
		if (!Active)
		{
			WaitTimer = 2f;
			Active = true;
		}
		if (WaitTimer <= 0f)
		{
			Enemy.CurrentState = EnemyState.Roaming;
			WaitTimer = 0f;
		}
		else
		{
			WaitTimer -= Time.deltaTime;
		}
	}
}
