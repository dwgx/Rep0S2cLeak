using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyGnomeDirector : MonoBehaviour
{
	public enum State
	{
		Idle,
		Leave,
		ChangeDestination,
		Investigate,
		AttackSet,
		AttackPlayer,
		AttackValuable
	}

	public static EnemyGnomeDirector instance;

	public bool debugDraw;

	public bool debugOneOnly;

	public bool debugShortIdle;

	public bool debugLongIdle;

	[Space]
	public List<EnemyGnome> gnomes = new List<EnemyGnome>();

	internal List<Vector3> destinations = new List<Vector3>();

	[Space]
	public State currentState = State.ChangeDestination;

	private bool stateImpulse = true;

	private float stateTimer;

	internal bool setup;

	private PlayerAvatar playerTarget;

	private PhysGrabObject valuableTarget;

	internal Vector3 attackPosition;

	internal Vector3 attackVisionPosition;

	private float valuableAttackPositionTimer;

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
			if (!Application.isEditor || (SemiFunc.IsMultiplayer() && !GameManager.instance.localTest))
			{
				debugDraw = false;
				debugOneOnly = false;
				debugShortIdle = false;
				debugLongIdle = false;
			}
			base.transform.parent = LevelGenerator.Instance.EnemyParent.transform;
			StartCoroutine(Setup());
		}
		else
		{
			Object.Destroy(this);
		}
	}

	internal void SetupSingle(EnemyGnome _gnome)
	{
		if ((bool)_gnome)
		{
			if (debugOneOnly && gnomes.Count > 0)
			{
				Object.Destroy(_gnome.enemy.EnemyParent.gameObject);
			}
			else if (!gnomes.Contains(_gnome))
			{
				gnomes.Add(_gnome);
				destinations.Add(Vector3.zero);
				_gnome.directorIndex = gnomes.IndexOf(_gnome);
			}
		}
	}

	private IEnumerator Setup()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
		{
			SetupSingle(item?.Enemy.GetComponent<EnemyGnome>());
		}
		setup = true;
	}

	private void Update()
	{
		if (!setup)
		{
			return;
		}
		if (debugDraw)
		{
			if (currentState == State.Idle)
			{
				Debug.DrawRay(base.transform.position, Vector3.up * 2f, Color.green);
				foreach (EnemyGnome gnome in gnomes)
				{
					Debug.DrawRay(destinations[gnomes.IndexOf(gnome)], Vector3.up * 2f, Color.yellow);
				}
			}
			else if (currentState == State.AttackPlayer)
			{
				Debug.DrawRay(attackPosition, Vector3.up * 2f, Color.red);
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			switch (currentState)
			{
			case State.Idle:
				StateIdle();
				break;
			case State.Leave:
				StateLeave();
				break;
			case State.ChangeDestination:
				StateChangeDestination();
				break;
			case State.Investigate:
				StateInvestigate();
				break;
			case State.AttackSet:
				StateAttackSet();
				break;
			case State.AttackPlayer:
				StateAttackPlayer();
				break;
			case State.AttackValuable:
				StateAttackValuable();
				break;
			}
		}
	}

	private void StateIdle()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = Random.Range(20f, 30f);
			if (debugShortIdle)
			{
				stateTimer *= 0.5f;
			}
			if (debugLongIdle)
			{
				stateTimer *= 2f;
			}
		}
		if (!SemiFunc.EnemySpawnIdlePause())
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.ChangeDestination);
			}
			LeaveCheck();
		}
	}

	private void StateLeave()
	{
		if (stateImpulse)
		{
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f);
			if ((bool)levelPoint)
			{
				flag = SetPosition(levelPoint.transform.position);
			}
			if (flag)
			{
				stateImpulse = false;
				UpdateState(State.Idle);
			}
		}
	}

	private void StateChangeDestination()
	{
		if (stateImpulse)
		{
			bool flag = false;
			LevelPoint levelPoint = SemiFunc.LevelPointGet(base.transform.position, 10f, 25f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
			}
			if ((bool)levelPoint)
			{
				flag = SetPosition(levelPoint.transform.position);
			}
			if (flag)
			{
				stateImpulse = false;
				UpdateState(State.Idle);
			}
		}
	}

	private void StateInvestigate()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			UpdateState(State.Idle);
		}
	}

	private void StateAttackSet()
	{
		if (!stateImpulse)
		{
			return;
		}
		stateImpulse = false;
		valuableTarget = null;
		float num = 0f;
		Collider[] array = Physics.OverlapSphere(playerTarget.transform.position, 3f, LayerMask.GetMask("PhysGrabObject"));
		for (int i = 0; i < array.Length; i++)
		{
			ValuableObject componentInParent = array[i].GetComponentInParent<ValuableObject>();
			if ((bool)componentInParent && componentInParent.dollarValueCurrent > num)
			{
				num = componentInParent.dollarValueCurrent;
				valuableTarget = componentInParent.physGrabObject;
			}
		}
		if ((bool)valuableTarget)
		{
			UpdateState(State.AttackValuable);
		}
		else
		{
			UpdateState(State.AttackPlayer);
		}
	}

	private void StateAttackPlayer()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 3f;
		}
		PauseGnomeSpawnedTimers();
		if ((bool)playerTarget && !playerTarget.isDisabled)
		{
			if (stateTimer > 0.5f)
			{
				attackPosition = playerTarget.transform.position;
				attackVisionPosition = playerTarget.PlayerVisionTarget.VisionTransform.position;
			}
		}
		else
		{
			stateTimer = 0f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			SetPosition(attackPosition);
			UpdateState(State.Idle);
		}
	}

	private void StateAttackValuable()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 10f;
			valuableAttackPositionTimer = 0f;
		}
		PauseGnomeSpawnedTimers();
		if ((bool)valuableTarget)
		{
			if (valuableAttackPositionTimer <= 0f)
			{
				valuableAttackPositionTimer = 0.2f;
				attackPosition = valuableTarget.centerPoint;
				if (Physics.Raycast(valuableTarget.centerPoint, Vector3.down, out var hitInfo, 2f, LayerMask.GetMask("Default")))
				{
					attackPosition = hitInfo.point;
				}
			}
			else
			{
				valuableAttackPositionTimer -= Time.deltaTime;
			}
			attackVisionPosition = valuableTarget.centerPoint;
		}
		else
		{
			stateTimer = 0f;
		}
		bool flag = false;
		foreach (EnemyGnome gnome in gnomes)
		{
			if ((bool)gnome && Vector3.Distance(gnome.enemy.Rigidbody.transform.position, attackPosition) <= 1f)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			stateTimer -= Time.deltaTime;
		}
		bool flag2 = true;
		foreach (EnemyGnome gnome2 in gnomes)
		{
			if ((bool)gnome2 && gnome2.isActiveAndEnabled)
			{
				flag2 = false;
				break;
			}
		}
		if (stateTimer <= 0f || flag2)
		{
			SetPosition(attackPosition);
			UpdateState(State.Idle);
		}
	}

	private void UpdateState(State _state)
	{
		currentState = _state;
		stateImpulse = true;
		stateTimer = 0f;
	}

	private bool SetPosition(Vector3 _initialPosition)
	{
		if (NavMesh.SamplePosition(_initialPosition, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")) && !SemiFunc.EnemyPhysObjectSphereCheck(hit.position, 1f))
		{
			base.transform.position = hit.position;
			base.transform.rotation = Quaternion.identity;
			float num = 360f / (float)gnomes.Count;
			foreach (EnemyGnome gnome in gnomes)
			{
				float num2 = 0f;
				Vector3 value = base.transform.position;
				Vector3 vector = base.transform.position;
				for (; num2 < 2f; num2 += 0.1f)
				{
					value = vector;
					vector = hit.position + base.transform.forward * num2;
					if (!NavMesh.SamplePosition(vector, out var _, 5f, -1) || !Physics.Raycast(vector, Vector3.down, 5f, LayerMask.GetMask("Default")))
					{
						break;
					}
					Vector3 normalized = (vector + Vector3.up * 0.5f - (hit.position + Vector3.up * 0.5f)).normalized;
					if (Physics.Raycast(vector + Vector3.up * 0.5f, normalized, normalized.magnitude, LayerMask.GetMask("Default", "PhysGrabObjectHinge")) || (num2 > 0.5f && Random.Range(0, 100) < 15))
					{
						break;
					}
				}
				destinations[gnomes.IndexOf(gnome)] = value;
				base.transform.rotation = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y + num, 0f);
			}
			return true;
		}
		return false;
	}

	public void Investigate(Vector3 _position)
	{
		if (currentState != State.Investigate && currentState != State.AttackSet && currentState != State.AttackPlayer && currentState != State.AttackValuable)
		{
			SetPosition(_position);
			UpdateState(State.Investigate);
		}
	}

	public void SetTarget(PlayerAvatar _player)
	{
		if (currentState != State.AttackSet && currentState != State.AttackPlayer && currentState != State.AttackValuable)
		{
			playerTarget = _player;
			UpdateState(State.AttackSet);
		}
		else if (currentState == State.AttackPlayer && playerTarget == _player)
		{
			stateTimer = 2f;
		}
	}

	public void SeeTarget()
	{
		if (currentState == State.AttackPlayer)
		{
			stateTimer = 1f;
		}
	}

	public bool CanAttack(EnemyGnome _gnome)
	{
		if (_gnome.attackCooldown > 0f || _gnome.enemy.Jump.jumping)
		{
			return false;
		}
		if (currentState == State.AttackPlayer)
		{
			if (Vector3.Distance(_gnome.enemy.Rigidbody.transform.position, attackPosition) <= 0.7f)
			{
				return true;
			}
		}
		else if ((bool)valuableTarget)
		{
			_gnome.overlapCheckCooldown = 1f;
			if (_gnome.overlapCheckTimer <= 0f)
			{
				_gnome.overlapCheckTimer = 0.5f;
				_gnome.overlapCheckPrevious = false;
				Collider[] array = Physics.OverlapSphere(_gnome.enemy.Rigidbody.transform.position, 0.7f, LayerMask.GetMask("PhysGrabObject"));
				for (int i = 0; i < array.Length; i++)
				{
					ValuableObject componentInParent = array[i].GetComponentInParent<ValuableObject>();
					if ((bool)componentInParent && componentInParent.physGrabObject == valuableTarget)
					{
						_gnome.overlapCheckPrevious = true;
					}
				}
			}
			return _gnome.overlapCheckPrevious;
		}
		return false;
	}

	private void PauseGnomeSpawnedTimers()
	{
		foreach (EnemyGnome gnome in gnomes)
		{
			gnome?.enemy.EnemyParent.SpawnedTimerPause(0.1f);
		}
	}

	private void LeaveCheck()
	{
		bool flag = false;
		foreach (EnemyGnome gnome in gnomes)
		{
			if ((bool)gnome && SemiFunc.EnemyForceLeave(gnome.enemy))
			{
				flag = true;
			}
		}
		if (flag)
		{
			UpdateState(State.Leave);
		}
	}

	public void OnSpawn(EnemyGnome _gnomeSpawn)
	{
		bool flag = true;
		foreach (EnemyGnome gnome in gnomes)
		{
			if ((bool)gnome)
			{
				if (_gnomeSpawn != gnome && gnome.enemy.EnemyParent.Spawned)
				{
					flag = false;
				}
				gnome.enemy.EnemyParent.DespawnedTimerSet(gnome.enemy.EnemyParent.DespawnedTimer - 30f);
			}
		}
		if (flag)
		{
			UpdateState(State.ChangeDestination);
		}
	}
}
