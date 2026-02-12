using UnityEngine;
using UnityEngine.Events;

public class EnemyStateStunned : MonoBehaviour
{
	private Enemy enemy;

	private bool active;

	[HideInInspector]
	public float stunTimer;

	private float overrideDisableTimer;

	[Space]
	public UnityEvent onStunnedStart;

	public UnityEvent onStunnedEnd;

	private void Start()
	{
		enemy = GetComponent<Enemy>();
	}

	private void Update()
	{
		if (overrideDisableTimer > 0f)
		{
			overrideDisableTimer -= Time.deltaTime;
			stunTimer = 0f;
		}
		if (stunTimer > 0f)
		{
			enemy.CurrentState = EnemyState.Stunned;
			stunTimer -= Time.deltaTime;
			if (stunTimer <= 0f)
			{
				enemy.CurrentState = EnemyState.Roaming;
			}
		}
		if (enemy.CurrentState != EnemyState.Stunned)
		{
			if (active)
			{
				if (enemy.HasRigidbody && enemy.HasNavMeshAgent)
				{
					enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
				}
				onStunnedEnd.Invoke();
				active = false;
			}
			return;
		}
		if (!active)
		{
			onStunnedStart.Invoke();
			active = true;
		}
		enemy.DisableChase(0.25f);
		if (enemy.HasRigidbody)
		{
			enemy.Rigidbody.DisableFollowPosition(0.1f, enemy.Rigidbody.stunResetSpeed);
			enemy.Rigidbody.DisableFollowRotation(0.1f, enemy.Rigidbody.stunResetSpeed);
			enemy.Rigidbody.DisableNoGravity(0.1f);
			enemy.Rigidbody.physGrabObject.OverrideDrag(0.05f);
			enemy.Rigidbody.physGrabObject.OverrideAngularDrag(0.05f);
		}
	}

	public void Spawn()
	{
		stunTimer = 0f;
	}

	public void Set(float time)
	{
		if (!(overrideDisableTimer > 0f) && time > stunTimer && enemy.TeleportedTimer <= 0f)
		{
			stunTimer = time;
		}
	}

	public void OverrideDisable(float time)
	{
		overrideDisableTimer = time;
	}

	public void Reset()
	{
		stunTimer = 0.1f;
	}
}
