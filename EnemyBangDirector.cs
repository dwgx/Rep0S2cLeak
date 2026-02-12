using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBangDirector : MonoBehaviour, IPunObservable
{
	public enum State
	{
		Idle,
		Leave,
		ChangeDestination,
		Investigate,
		AttackSet,
		AttackPlayer,
		AttackCart
	}

	public static EnemyBangDirector instance;

	public bool debugDraw;

	public bool debugOneOnly;

	public bool debugShortIdle;

	public bool debugLongIdle;

	public bool debugNoFuseProgress;

	[Space]
	public List<EnemyBang> units = new List<EnemyBang>();

	internal List<Vector3> destinations = new List<Vector3>();

	[Space]
	public State currentState = State.ChangeDestination;

	private bool stateImpulse = true;

	private float stateTimer;

	internal bool setup;

	internal int headIndex = -1;

	internal PlayerAvatar playerTarget;

	internal bool playerTargetCrawling;

	internal Vector3 attackPosition;

	internal Vector3 attackVisionPosition;

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
				debugNoFuseProgress = false;
			}
			base.transform.parent = LevelGenerator.Instance.EnemyParent.transform;
			StartCoroutine(Setup());
		}
		else
		{
			Object.Destroy(this);
		}
	}

	internal void SetupSingle(EnemyBang _unit)
	{
		if (!_unit)
		{
			return;
		}
		if (debugOneOnly && units.Count > 0)
		{
			Object.Destroy(_unit.enemy.EnemyParent.gameObject);
		}
		else
		{
			if (units.Contains(_unit))
			{
				return;
			}
			units.Add(_unit);
			destinations.Add(Vector3.zero);
			_unit.directorIndex = units.IndexOf(_unit);
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				if (headIndex == -1)
				{
					headIndex = Random.Range(0, _unit.headObjects.Length);
				}
				if (SemiFunc.IsMultiplayer())
				{
					_unit.photonView.RPC("SetHeadRPC", RpcTarget.All, headIndex);
				}
				else
				{
					_unit.SetHeadRPC(headIndex);
				}
				headIndex++;
				if (headIndex >= _unit.headObjects.Length)
				{
					headIndex = 0;
				}
				float num = Random.Range(0.8f, 1.25f);
				if (SemiFunc.IsMultiplayer())
				{
					_unit.photonView.RPC("SetVoicePitchRPC", RpcTarget.All, num);
				}
				else
				{
					_unit.SetVoicePitchRPC(num);
				}
			}
			EnemyBangFuse[] componentsInChildren = _unit.enemy.EnemyParent.GetComponentsInChildren<EnemyBangFuse>(includeInactive: true);
			foreach (EnemyBangFuse obj in componentsInChildren)
			{
				obj.controller = _unit;
				obj.particleParent.parent = _unit.particleParent;
				obj.setup = true;
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
			SetupSingle(item?.Enemy.GetComponent<EnemyBang>());
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
			Debug.DrawRay(base.transform.position, Vector3.up * 2f, Color.green);
			foreach (EnemyBang unit in units)
			{
				Debug.DrawRay(destinations[units.IndexOf(unit)], Vector3.up * 2f, Color.yellow);
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			switch (currentState)
			{
			case State.Idle:
				StateIdle();
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
			case State.AttackCart:
				StateAttackCart();
				break;
			case State.Leave:
				StateLeave();
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
		if (stateImpulse)
		{
			stateImpulse = false;
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
		PauseSpawnedTimers();
		if ((bool)playerTarget && !playerTarget.isDisabled)
		{
			playerTargetCrawling = playerTarget.isCrawling;
			if (stateTimer > 0.5f)
			{
				attackPosition = playerTarget.transform.position;
				attackVisionPosition = playerTarget.PlayerVisionTarget.VisionTransform.position;
				if (!playerTargetCrawling)
				{
					attackVisionPosition += Vector3.up * 0.25f;
				}
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

	private void StateAttackCart()
	{
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
			float num = 360f / (float)units.Count;
			foreach (EnemyBang unit in units)
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
				destinations[units.IndexOf(unit)] = value;
				base.transform.rotation = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y + num, 0f);
			}
			return true;
		}
		return false;
	}

	private void LeaveCheck()
	{
		bool flag = false;
		foreach (EnemyBang unit in units)
		{
			if ((bool)unit && SemiFunc.EnemyForceLeave(unit.enemy))
			{
				flag = true;
			}
		}
		if (flag)
		{
			UpdateState(State.Leave);
		}
	}

	public void OnSpawn(EnemyBang _unitSpawn)
	{
		bool flag = true;
		foreach (EnemyBang unit in units)
		{
			if ((bool)unit)
			{
				if (_unitSpawn != unit && unit.enemy.EnemyParent.Spawned)
				{
					flag = false;
				}
				unit.enemy.EnemyParent.DespawnedTimerSet(unit.enemy.EnemyParent.DespawnedTimer - 30f);
			}
		}
		if (flag)
		{
			UpdateState(State.ChangeDestination);
		}
	}

	public void Investigate(Vector3 _position)
	{
		if (currentState != State.Investigate)
		{
			SetPosition(_position);
			UpdateState(State.Investigate);
		}
	}

	public void SetTarget(PlayerAvatar _player)
	{
		if (currentState != State.AttackSet && currentState != State.AttackPlayer && currentState != State.AttackCart)
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

	public void TriggerNearby(Vector3 _position)
	{
		foreach (EnemyBang unit in units)
		{
			if ((bool)unit && Vector3.Distance(unit.transform.position, _position) < 2f)
			{
				unit.OnVision();
			}
		}
	}

	private void PauseSpawnedTimers()
	{
		foreach (EnemyBang unit in units)
		{
			unit?.enemy.EnemyParent.SpawnedTimerPause(0.1f);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(attackVisionPosition);
			}
			else
			{
				attackVisionPosition = (Vector3)stream.ReceiveNext();
			}
		}
	}
}
