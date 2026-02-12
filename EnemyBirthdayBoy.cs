using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBirthdayBoy : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Despawn,
		Idle,
		GoToBalloonPlacement,
		PlaceBalloon,
		LeaveAfterBalloon,
		GoToPlayerAngry,
		GoToBalloonAngry,
		PlayerNotice,
		LookAround,
		SeekPlayer,
		Investigate,
		CreepyStare,
		Attack,
		AttackUnderStart,
		AttackUnder,
		AttackUnderEnd,
		StandUp,
		AttackOver,
		Leave,
		Stunned,
		FlyBackUp,
		FlyBackToNavMesh
	}

	[HideInInspector]
	public State currentState;

	[Header("Scripts & Components")]
	public BirthdayBoyAnim anim;

	public EnemyParent enemyParent;

	public MeshRenderer mr;

	[Header("Balloon Placement")]
	public int maxBalloons = 30;

	public GameObject balloonPrefab;

	public List<Transform> balloonSpawnPoints;

	[Header("Collision Checkers")]
	public Transform balloonCollisionChecker;

	public Transform standUpCollisionChecker;

	public Transform doorCollisionChecker;

	[Header("Collider")]
	public CapsuleCollider BBcollider;

	public CapsuleCollider colliderSmall;

	[Header("Code Anim components")]
	public Transform visualMesh;

	public Transform headPivot;

	public Transform leftEyePivot;

	public Transform rightEyePivot;

	public Transform bottom;

	public Transform animatingBalloon;

	[Header("Rotation Logic")]
	private Quaternion horizontalRotationTarget = Quaternion.identity;

	public SpringQuaternion horizontalRotationSpring;

	public GameObject balloonPopEffect;

	[HideInInspector]
	public bool blowing;

	[HideInInspector]
	public PlayerAvatar playerTarget;

	private State previousState;

	private Enemy enemy;

	private PhotonView photonView;

	public Dictionary<Vector3, GameObject> balloons = new Dictionary<Vector3, GameObject>();

	private List<Vector3> balloonsToRemove = new List<Vector3>();

	private List<int> balloonPlacements;

	private float stateTimer;

	private float secondStateTimer;

	private float thirdStateTimer;

	private float popAggroRadius = 10f;

	private float stateTimerClient;

	private float balloonInterval = 3f;

	private float onScreenTimer;

	private float creepyStareTimer;

	private float maxHeadRotationUpDown = 40f;

	private float maxHeadRotationLeftRight = 70f;

	private float idleBreakerTimer;

	private float breakerPlayTime = 2f;

	private float breakerPlayTimer;

	private float preferredBalloonDistance = 1.8f;

	private bool stateImpulse;

	private bool startOfStateClient;

	private bool SkipStateImpulse;

	private bool nonAggroInvestigate;

	private bool hasPendingBalloon;

	private bool checkedForDuplicates;

	private bool replaceOldBalloons;

	private bool onNavMesh;

	private bool playerOnNavMesh;

	private bool angrySpawn;

	private Vector3 targetPosition;

	private Vector3 stuckAttackTarget;

	private Vector3 agentDestination;

	private Vector3 moveBackPosition;

	private Vector3 pendingBalloonPoint;

	private Vector3 bPos;

	private Vector3 bScale;

	private int minBalloonsPerPlacement = 2;

	private int maxBalloonsPerPlacement = 5;

	private int balloonCount;

	private int balloonsPlaced;

	private int balloonsPlacedTotal;

	private int myIndex;

	private Color[] colors = new Color[8]
	{
		Color.red,
		Color.blue,
		Color.green,
		Color.cyan,
		Color.yellow,
		new Color(0.5f, 0f, 1f),
		Color.magenta,
		Color.white
	};

	private Color balloonColor = Color.red;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
	}

	private void FixedUpdate()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		enemy.Rigidbody.gravity = !FlyState();
		if (currentState == State.FlyBackUp)
		{
			if (enemy.Rigidbody.rb.velocity.y < 0f)
			{
				enemy.Rigidbody.rb.velocity *= 0.8f;
			}
			if (enemy.Rigidbody.rb.angularVelocity.magnitude > 5f)
			{
				enemy.Rigidbody.rb.angularVelocity *= 0.95f;
			}
		}
	}

	private void Update()
	{
		stateTimerClient += Time.deltaTime;
		CheckForStartOfStateClient();
		BodyRotationLogic();
		HeadRotationLogic();
		if (currentState == State.Stunned && startOfStateClient)
		{
			anim.animator.SetTrigger("stun_trigger");
		}
		if (currentState == State.CreepyStare && startOfStateClient)
		{
			anim.CrackSound();
		}
		if (anim.breaker)
		{
			breakerPlayTimer += Time.deltaTime;
			if (breakerPlayTimer > breakerPlayTime)
			{
				anim.breaker = false;
				breakerPlayTimer = 0f;
			}
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		ColliderSizeLogic();
		if (enemy.IsStunned() && currentState != State.Stunned && currentState != State.Despawn)
		{
			replaceOldBalloons = false;
			if (currentState == State.PlaceBalloon)
			{
				InterruptBalloonPop();
				hasPendingBalloon = false;
			}
			UpdateState(State.Stunned);
		}
		if (PlayerTargetState() && (!playerTarget || playerTarget.isDisabled))
		{
			if (currentState == State.AttackUnder)
			{
				UpdateState(State.AttackUnderEnd);
			}
			else
			{
				UpdateState(State.Idle);
			}
		}
		else if (enemy.CurrentState == EnemyState.Despawn)
		{
			UpdateState(State.Despawn);
		}
		if (enemy.Rigidbody.transform.position.y - moveBackPosition.y < -2.5f && !enemy.IsStunned() && currentState != State.Despawn && !FlyState())
		{
			float num = float.PositiveInfinity;
			if (Physics.Raycast(enemy.Rigidbody.transform.position, Vector3.down, out var hitInfo, 5f, LayerMask.GetMask("Default")))
			{
				num = Vector3.Distance(enemy.Rigidbody.transform.position, hitInfo.point);
			}
			if (num > 2f)
			{
				UpdateState(State.FlyBackUp);
			}
		}
		CheckForCreepyStareStart();
		if (NavMeshState() && !enemy.Jump.jumping)
		{
			UpdateMoveBackPosition();
		}
		CheckForPoppedBalloons();
		if (!FlyState())
		{
			RotationLogic();
		}
		switch (currentState)
		{
		case State.Spawn:
			StateSpawn();
			break;
		case State.Despawn:
			StateDespawn();
			break;
		case State.Idle:
			StateIdle();
			break;
		case State.GoToBalloonPlacement:
			StateGoToBalloonPlacement();
			break;
		case State.PlaceBalloon:
			StatePlaceBalloon();
			break;
		case State.LeaveAfterBalloon:
			StateLeaveAfterBalloon();
			break;
		case State.Leave:
			StateLeave();
			break;
		case State.PlayerNotice:
			StatePlayerNotice();
			break;
		case State.GoToPlayerAngry:
			StateGoToPlayerAngry();
			break;
		case State.GoToBalloonAngry:
			StateGoToBalloonAngry();
			break;
		case State.SeekPlayer:
			StateSeekPlayer();
			break;
		case State.Investigate:
			StateInvestigate();
			break;
		case State.Attack:
			StateAttack();
			break;
		case State.AttackUnderStart:
			StateAttackUnderStart();
			break;
		case State.AttackUnder:
			StateAttackUnder();
			break;
		case State.AttackUnderEnd:
			StateAttackUnderEnd();
			break;
		case State.AttackOver:
			StateAttackOver();
			break;
		case State.StandUp:
			StateStandUp();
			break;
		case State.CreepyStare:
			StateCreepyStare();
			break;
		case State.LookAround:
			StateLookAround();
			break;
		case State.Stunned:
			StateStunned();
			break;
		case State.FlyBackUp:
			StateFlyBackUp();
			break;
		case State.FlyBackToNavMesh:
			StateFlyBackToNavMesh();
			break;
		}
	}

	private void PlayIdleBreaker(int index)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("PlayIdleBreakerRPC", RpcTarget.All, index);
			}
			else
			{
				PlayIdleBreakerRPC(index);
			}
		}
	}

	[PunRPC]
	private void PlayIdleBreakerRPC(int index, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			anim.breaker = true;
			AudioClip audioClip = anim.idleBreakers[index];
			AudioClip[] sounds = new AudioClip[1] { audioClip };
			anim.idleBreaker.Sounds = sounds;
			anim.idleBreaker.Play(base.transform.position);
		}
	}

	private void InterruptBalloonPop()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			anim.InterruptBalloonPop();
			if (GameManager.Multiplayer())
			{
				photonView.RPC("InterruptBalloonPopRPC", RpcTarget.Others);
			}
		}
	}

	[PunRPC]
	private void InterruptBalloonPopRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			anim.InterruptBalloonPop();
		}
	}

	public void UpdateState(State _nextState)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && _nextState != currentState)
		{
			onScreenTimer = 0f;
			stateImpulse = true;
			currentState = _nextState;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdateStateRPC", RpcTarget.Others, _nextState);
			}
		}
	}

	[PunRPC]
	private void UpdateStateRPC(State _nextState, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _nextState;
		}
	}

	private void BalloonPopEffect(Vector3 _pos, bool _global = true)
	{
		GameObject obj = Object.Instantiate(balloonPopEffect, _pos, Quaternion.identity);
		obj.GetComponent<BirthdayBoyBalloonPopParticle>().ChangeParticleColor(balloonColor);
		obj.GetComponent<BirthdayBoyBalloonPopParticle>().PlayParticle();
		anim.balloonPopSound.Play(_pos);
		if (_global)
		{
			anim.balloonPopGlobal.Play(_pos);
		}
		GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, _pos, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 8f, _pos, 0.1f);
	}

	private void RemoveBalloon(Vector3 _position, bool _manualRemove = false)
	{
		float num = CheckIfPlayersNearbyPop(_position);
		if (!_manualRemove && num > popAggroRadius)
		{
			return;
		}
		bool poppedWhileDespawned = balloons[_position].GetComponentInChildren<BirthdayBoyBalloon>().poppedWhileDespawned;
		if (GameManager.Multiplayer())
		{
			photonView.RPC("RemoveBalloonRPC", RpcTarget.All, _position, !_manualRemove);
		}
		else
		{
			RemoveBalloonRPC(_position, !_manualRemove);
		}
		if (_manualRemove)
		{
			return;
		}
		if (poppedWhileDespawned)
		{
			UpdateState(State.Spawn);
			angrySpawn = true;
		}
		else if (currentState != State.GoToPlayerAngry && currentState != State.Despawn && !AttackState())
		{
			if ((bool)playerTarget)
			{
				replaceOldBalloons = false;
				UpdateState(State.GoToPlayerAngry);
			}
			else if (num < popAggroRadius / 2f)
			{
				replaceOldBalloons = false;
				UpdateState(State.GoToBalloonAngry);
			}
		}
	}

	[PunRPC]
	private void RemoveBalloonRPC(Vector3 _position, bool _global, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			BalloonPopEffect(_position, _global);
			Object.Destroy(balloons[_position]);
			balloons.Remove(_position);
			if (_global)
			{
				EnemyDirector.instance.SetInvestigate(_position, 5f);
			}
		}
	}

	private void SpawnBalloon(Vector3 _position)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			float num = Random.Range(0.5f, 1.5f);
			if (GameManager.Multiplayer())
			{
				photonView.RPC("SpawnBalloonRPC", RpcTarget.All, _position, bPos, bScale, num);
			}
			else
			{
				SpawnBalloonRPC(_position, bPos, bScale, num);
			}
			balloons[_position].GetComponentInChildren<BirthdayBoyBalloon>().balloonIndex = balloonsPlacedTotal;
		}
	}

	[PunRPC]
	private void SpawnBalloonRPC(Vector3 _keyPos, Vector3 _spawnPos, Vector3 _spawnScale, float _randomSpeed, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			balloons[_keyPos] = Object.Instantiate(balloonPrefab, _keyPos, Quaternion.identity);
			BirthdayBoyBalloon componentInChildren = balloons[_keyPos].GetComponentInChildren<BirthdayBoyBalloon>();
			componentInChildren.enemyParent = enemy.EnemyParent;
			componentInChildren.placerIndex = myIndex;
			componentInChildren.randomSpeed = _randomSpeed;
			if (myIndex != 0 && myIndex < 8)
			{
				componentInChildren.ChangeColor(balloonColor);
			}
			balloons[_keyPos].GetComponentInChildren<BirthdayBoyBalloon>().TakeToSpawnPoint(_spawnPos, _spawnScale);
		}
	}

	private void SyncBalloonPlacements()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			List<int> nonCollidingBalloonSpawnPoints = GetNonCollidingBalloonSpawnPoints();
			int[] array = GetBalloonSpawnPointIndexes(nonCollidingBalloonSpawnPoints).ToArray();
			if (GameManager.Multiplayer())
			{
				photonView.RPC("SyncBalloonPlacementsRPC", RpcTarget.All, array);
			}
			else
			{
				SyncBalloonPlacementsRPC(array);
			}
		}
	}

	[PunRPC]
	private void SyncBalloonPlacementsRPC(int[] _spawnPoints, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			List<int> list = new List<int>(_spawnPoints);
			balloonPlacements = list;
		}
	}

	private void BlowBalloonAnimation()
	{
		if (GameManager.Multiplayer())
		{
			photonView.RPC("BlowBalloonAnimationRPC", RpcTarget.All);
		}
		else
		{
			BlowBalloonAnimationRPC();
		}
	}

	[PunRPC]
	private void BlowBalloonAnimationRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			anim.BlowBalloonAnimationTrigger();
		}
	}

	private void UpdatePlayerTarget(PlayerAvatar _target)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer() && _target != null && _target != playerTarget)
			{
				photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.Others, _target.photonView.ViewID);
			}
			playerTarget = _target;
		}
	}

	[PunRPC]
	private void UpdatePlayerTargetRPC(int photonViewID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			playerTarget = SemiFunc.PlayerAvatarGetFromPhotonID(photonViewID);
		}
	}

	private void UpdateLookTarget(PlayerAvatar _target)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				anim.lookTarget = _target;
			}
			else if (!anim.lookTarget || anim.lookTarget.photonView.ViewID != _target.photonView.ViewID)
			{
				anim.lookTarget = _target;
				photonView.RPC("UpdateLookTargetRPC", RpcTarget.Others, _target.photonView.ViewID);
			}
		}
	}

	[PunRPC]
	private void UpdateLookTargetRPC(int photonViewID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			anim.lookTarget = SemiFunc.PlayerAvatarGetFromPhotonID(photonViewID);
		}
	}

	private void PopAllBalloons()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (GameManager.Multiplayer())
			{
				photonView.RPC("PopAllBalloonsRPC", RpcTarget.All);
			}
			else
			{
				PopAllBalloonsRPC();
			}
		}
	}

	[PunRPC]
	private void PopAllBalloonsRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		foreach (Vector3 item in new List<Vector3>(balloons.Keys))
		{
			BalloonPopEffect(item, _global: false);
			Object.Destroy(balloons[item]);
			balloons.Remove(item);
		}
	}

	[PunRPC]
	private void ToggleShrinkColliderRPC(bool _active, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			BBcollider.gameObject.SetActive(!_active);
			colliderSmall.gameObject.SetActive(_active);
		}
	}

	private void StateSpawn()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1f;
		}
		stateTimer -= Time.deltaTime;
		if (!(stateTimer <= 0f))
		{
			return;
		}
		if (angrySpawn)
		{
			angrySpawn = false;
			if ((bool)playerTarget)
			{
				UpdateState(State.GoToPlayerAngry);
			}
			else
			{
				UpdateState(State.GoToBalloonAngry);
			}
		}
		else
		{
			UpdateState(State.Idle);
		}
	}

	private void StateDespawn()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			ResetNavMesh();
		}
	}

	private void StateIdle()
	{
		if (stateImpulse && !SkipStateImpulse)
		{
			ResetNavMesh();
			stateTimer = Random.Range(4f, 8f);
			stateImpulse = false;
			if (Random.Range(0, 10) > 4)
			{
				int index = Random.Range(0, 3);
				PlayIdleBreaker(index);
			}
		}
		SkipStateImpulse = false;
		if (!SemiFunc.EnemySpawnIdlePause())
		{
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.GoToBalloonPlacement);
			}
			if (SemiFunc.EnemyForceLeave(enemy))
			{
				UpdateState(State.Leave);
			}
		}
	}

	private void StateGoToBalloonPlacement()
	{
		if (stateImpulse)
		{
			LevelPoint levelPoint = SemiFunc.LevelPointGet(base.transform.position, 5f, 15f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
			}
			if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				agentDestination = hit.position;
			}
			stateTimer = 5f;
			enemy.Rigidbody.notMovingTimer = 0f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		JumpIfStuck();
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
		else if (Vector3.Distance(base.transform.position, agentDestination) < 1f)
		{
			UpdateState(State.PlaceBalloon);
		}
		else if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StatePlaceBalloon()
	{
		if (stateImpulse && !SkipStateImpulse)
		{
			ResetNavMesh();
			SyncBalloonPlacements();
			balloonCount = balloonPlacements.Count;
			balloonsPlaced = 0;
			balloonsToRemove = new List<Vector3>();
			if (balloons.Count >= maxBalloons)
			{
				for (int i = 0; i < balloonCount; i++)
				{
					List<Vector3> list = new List<Vector3>(balloons.Keys);
					Vector3 vector = list[0];
					foreach (Vector3 item in list)
					{
						if (balloons[item].GetComponentInChildren<BirthdayBoyBalloon>().balloonIndex < balloons[vector].GetComponentInChildren<BirthdayBoyBalloon>().balloonIndex && !balloonsToRemove.Contains(item))
						{
							vector = item;
						}
					}
					balloonsToRemove.Add(vector);
				}
				replaceOldBalloons = true;
			}
			stateTimer = (float)balloonCount * balloonInterval + 0.5f;
			secondStateTimer = 0f;
			stateImpulse = false;
		}
		SkipStateImpulse = false;
		stateTimer -= Time.deltaTime;
		secondStateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			replaceOldBalloons = false;
			UpdateState(State.LeaveAfterBalloon);
		}
		else
		{
			if (balloonsPlaced >= balloonCount || !(secondStateTimer <= 0f))
			{
				return;
			}
			int index = balloonPlacements[balloonsPlaced];
			Vector3 position = balloonSpawnPoints[index].position;
			if (balloons.ContainsKey(position))
			{
				return;
			}
			if (!IsColliding(balloonCollisionChecker, position) && !NearbyDoor(position))
			{
				pendingBalloonPoint = position;
				hasPendingBalloon = true;
				BlowBalloonAnimation();
			}
			else
			{
				SemiLogger.LogMonika("Didnt spawn, was colliding");
			}
			secondStateTimer = balloonInterval;
			if (replaceOldBalloons)
			{
				Vector3 vector2 = balloonsToRemove[Mathf.Min(balloonsPlaced, balloonsToRemove.Count - 1)];
				if (balloons.ContainsKey(vector2))
				{
					RemoveBalloon(vector2, _manualRemove: true);
				}
			}
			balloonsPlaced++;
			balloonsPlacedTotal++;
		}
	}

	private void StateLeaveAfterBalloon()
	{
		if (stateImpulse)
		{
			LevelPoint levelPoint = SemiFunc.LevelPointGet(base.transform.position, 5f, 15f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
			}
			if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				agentDestination = hit.position;
			}
			stateTimer = 5f;
			enemy.Rigidbody.notMovingTimer = 0f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		JumpIfStuck();
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		if (stateTimer <= 0f || Vector3.Distance(base.transform.position, agentDestination) < 1f)
		{
			UpdateState(State.Idle);
		}
		else if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StateLeave()
	{
		if (stateImpulse)
		{
			LevelPoint levelPoint = SemiFunc.LevelPointGetPlayerDistance(base.transform.position, 25f, 50f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f);
			}
			if ((bool)levelPoint)
			{
				agentDestination = levelPoint.transform.position;
			}
			else
			{
				enemy.EnemyParent.SpawnedTimerSet(0f);
			}
			stateTimer = 10f;
			stateImpulse = false;
			enemy.Rigidbody.notMovingTimer = 0f;
			SemiFunc.EnemyLeaveStart(enemy);
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		JumpIfStuck();
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		enemy.NavMeshAgent.OverrideAgent(6f, 12f, 0.25f);
		if (stateTimer <= 0f || Vector3.Distance(base.transform.position, agentDestination) < 1f)
		{
			UpdateState(State.Idle);
		}
	}

	private void StateStunned()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			ResetNavMesh();
		}
		if (!enemy.IsStunned())
		{
			stateTimer = 20f;
			UpdateState(State.SeekPlayer);
		}
	}

	private void StateGoToPlayerAngry()
	{
		if (stateImpulse)
		{
			stateTimer = 30f;
			stateImpulse = false;
			enemy.Rigidbody.notMovingTimer = 0f;
		}
		enemy.NavMeshAgent.SetDestination(playerTarget.transform.position);
		JumpIfStuck();
		if (SemiFunc.FPSImpulse5())
		{
			playerOnNavMesh = PlayerOnNavMesh();
		}
		if (Vector3.Distance(base.transform.position, playerTarget.transform.position) < 2f && !enemy.IsStunned())
		{
			enemy.NavMeshAgent.ResetPath();
			stateTimer = 20f;
			UpdateState(State.Attack);
			return;
		}
		if (playerTarget.isCrawling && enemy.Rigidbody.notMovingTimer > 0.5f && Mathf.Abs(base.transform.position.x - playerTarget.transform.position.x) < 2f && Mathf.Abs(base.transform.position.z - playerTarget.transform.position.z) < 2f && !enemy.Jump.jumping && !playerOnNavMesh)
		{
			stateTimer = 20f;
			UpdateState(State.AttackUnderStart);
			return;
		}
		enemy.NavMeshAgent.OverrideAgent(4f, 10f, 0.25f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f || enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer = 20f;
			UpdateState(State.SeekPlayer);
		}
	}

	private void StateGoToBalloonAngry()
	{
		if (stateImpulse)
		{
			stateTimer = 30f;
			stateImpulse = false;
			enemy.Rigidbody.notMovingTimer = 0f;
			enemy.NavMeshAgent.SetDestination(targetPosition);
			return;
		}
		JumpIfStuck();
		if (CloseToNavMeshTarget() && !enemy.IsStunned())
		{
			stateTimer = 20f;
			UpdateState(State.LookAround);
			return;
		}
		enemy.NavMeshAgent.OverrideAgent(4f, 10f, 0.25f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	private void StateAttack()
	{
		if (stateImpulse)
		{
			secondStateTimer = 0f;
			enemy.Rigidbody.notMovingTimer = 0f;
			stateImpulse = false;
		}
		if (secondStateTimer > 3f)
		{
			UpdateState(State.SeekPlayer);
			return;
		}
		enemy.NavMeshAgent.SetDestination(playerTarget.transform.position);
		enemy.NavMeshAgent.OverrideAgent(4f, 5f, 0.25f);
		if (enemy.Vision.VisionsTriggered[playerTarget.photonView.ViewID] != 0)
		{
			secondStateTimer = 0f;
		}
		else
		{
			secondStateTimer += Time.deltaTime;
		}
		JumpIfStuck();
		if (SemiFunc.FPSImpulse5())
		{
			playerOnNavMesh = PlayerOnNavMesh();
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
		else if (secondStateTimer > 3f)
		{
			UpdateState(State.SeekPlayer);
		}
		else if (playerTarget.isCrawling && enemy.Rigidbody.notMovingTimer > 0.5f && Mathf.Abs(base.transform.position.x - playerTarget.transform.position.x) < 2f && Mathf.Abs(base.transform.position.z - playerTarget.transform.position.z) < 2f && !enemy.Jump.jumping && !playerOnNavMesh)
		{
			UpdateState(State.AttackUnderStart);
		}
		else if (playerTarget.transform.position.y > enemy.Rigidbody.transform.position.y && !playerOnNavMesh && Mathf.Abs(base.transform.position.x - playerTarget.transform.position.x) < 2f && Mathf.Abs(base.transform.position.z - playerTarget.transform.position.z) < 2f)
		{
			UpdateState(State.AttackOver);
		}
	}

	private void StateAttackUnderStart()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
		}
		enemy.Vision.StandOverride(0.25f);
	}

	private void StateAttackUnder()
	{
		if (stateImpulse)
		{
			thirdStateTimer = 10f;
			enemy.Rigidbody.notMovingTimer = 0f;
			secondStateTimer = 0f;
			stateImpulse = false;
		}
		if (enemy.Vision.VisionsTriggered[playerTarget.photonView.ViewID] != 0)
		{
			secondStateTimer = 0f;
		}
		else
		{
			secondStateTimer += Time.deltaTime;
		}
		enemy.Vision.StandOverride(0.25f);
		float num = Vector3.Distance(base.transform.position, playerTarget.transform.position);
		if (num > 0.5f)
		{
			enemy.NavMeshAgent.Disable(0.1f);
			float num2 = enemy.NavMeshAgent.DefaultSpeed * 0.5f;
			base.transform.position = Vector3.MoveTowards(base.transform.position, playerTarget.transform.position, num2 * Time.deltaTime);
			Vector3 forward = playerTarget.transform.position - enemy.Rigidbody.transform.position;
			forward.y = 0f;
			forward.Normalize();
			Quaternion quaternion = Quaternion.LookRotation(forward, Vector3.up);
			base.transform.rotation = Quaternion.Slerp(enemy.Rigidbody.transform.rotation, quaternion, Time.deltaTime * 15f);
		}
		else
		{
			enemy.NavMeshAgent.OverrideAgent(0.5f, 12f, 0.25f);
		}
		if (SemiFunc.FPSImpulse5())
		{
			onNavMesh = enemy.NavMeshAgent.OnNavmesh(enemy.Rigidbody.transform.position, 1f, _checkPit: true);
		}
		stateTimer -= Time.deltaTime;
		if (secondStateTimer > 3f || !playerTarget.isCrawling || stateTimer <= 0f)
		{
			UpdateState(State.AttackUnderEnd);
		}
		else if (onNavMesh && thirdStateTimer <= 0f && !IsColliding(standUpCollisionChecker, standUpCollisionChecker.position, _collideWithPlayer: false))
		{
			UpdateState(State.StandUp);
		}
		else if (num > 0.5f && enemy.Rigidbody.notMovingTimer > 2f)
		{
			thirdStateTimer -= Time.deltaTime;
			if (thirdStateTimer <= 0f)
			{
				UpdateState(State.AttackUnderEnd);
			}
		}
		else
		{
			thirdStateTimer = 10f;
		}
	}

	private void StateAttackUnderEnd()
	{
		if (stateImpulse)
		{
			secondStateTimer = 0f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		enemy.Vision.StandOverride(0.25f);
		base.transform.position = Vector3.MoveTowards(base.transform.position, moveBackPosition, enemy.NavMeshAgent.DefaultSpeed * Time.deltaTime);
		stateTimer -= Time.deltaTime;
		secondStateTimer += Time.deltaTime;
		if (Vector3.Distance(enemy.Rigidbody.transform.position, base.transform.position) > 2f)
		{
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		if (SemiFunc.FPSImpulse5())
		{
			onNavMesh = enemy.NavMeshAgent.OnNavmesh(enemy.Rigidbody.transform.position, 1f, _checkPit: true);
		}
		if (Vector3.Distance(enemy.Rigidbody.transform.position, moveBackPosition) <= 1f || onNavMesh)
		{
			UpdateState(State.StandUp);
		}
		else if (secondStateTimer > 30f)
		{
			enemy.EnemyParent.SpawnedTimerSet(0f);
			UpdateState(State.Despawn);
		}
	}

	private void StateStandUp()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			secondStateTimer = 0f;
		}
		enemy.Vision.StandOverride(0.25f);
		secondStateTimer += Time.deltaTime;
		if (secondStateTimer > 1f)
		{
			if (stateTimer > 0f)
			{
				UpdateState(State.Attack);
			}
			else
			{
				UpdateState(State.Idle);
			}
		}
	}

	private void StateAttackOver()
	{
		if (stateImpulse)
		{
			secondStateTimer = 0f;
			thirdStateTimer = 10f;
			stateImpulse = false;
		}
		if (SemiFunc.FPSImpulse5())
		{
			playerOnNavMesh = PlayerOnNavMesh();
			onNavMesh = enemy.NavMeshAgent.OnNavmesh(enemy.Rigidbody.transform.position, 1f, _checkPit: true);
		}
		if (enemy.Vision.VisionsTriggered[playerTarget.photonView.ViewID] != 0)
		{
			secondStateTimer = 0f;
		}
		else
		{
			secondStateTimer += Time.deltaTime;
		}
		Vector3 position = playerTarget.transform.position;
		enemy.NavMeshAgent.Disable(0.1f);
		if (Vector3.Distance(enemy.Rigidbody.transform.position, position) > 1.5f)
		{
			float num = enemy.NavMeshAgent.DefaultSpeed * 0.75f;
			base.transform.position = Vector3.MoveTowards(base.transform.position, position, num * Time.deltaTime);
		}
		else
		{
			base.transform.position = enemy.Rigidbody.transform.position;
			enemy.Rigidbody.DisableFollowPosition(0.1f, 5f);
		}
		Vector3 forward = playerTarget.transform.position - enemy.Rigidbody.transform.position;
		forward.y = 0f;
		forward.Normalize();
		Quaternion quaternion = Quaternion.LookRotation(forward, Vector3.up);
		base.transform.rotation = Quaternion.Slerp(enemy.Rigidbody.transform.rotation, quaternion, Time.deltaTime * 15f);
		SemiFunc.EnemyCartJump(enemy);
		if (position.y > enemy.Rigidbody.transform.position.y + 0.3f && !enemy.Jump.jumping && !enemy.Jump.jumpingDelay && !enemy.Jump.landDelay && !enemy.Jump.stuckJumpImpulse)
		{
			Vector3 normalized = (position - enemy.Rigidbody.transform.position).normalized;
			enemy.Jump.StuckTrigger(normalized);
			enemy.Rigidbody.WarpDisable(0.25f);
			base.transform.position = enemy.Rigidbody.transform.position;
			base.transform.position = Vector3.MoveTowards(base.transform.position, position, 2f);
			stateTimer -= 0.5f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
		else
		{
			if (enemy.Jump.jumping || enemy.Jump.jumpingDelay || enemy.Jump.landDelay)
			{
				return;
			}
			if (onNavMesh && playerOnNavMesh)
			{
				UpdateState(State.Attack);
			}
			else if (secondStateTimer > 3f || enemy.Rigidbody.notMovingTimer > 2f)
			{
				thirdStateTimer -= Time.deltaTime;
				if (thirdStateTimer <= 0f)
				{
					UpdateState(State.Idle);
				}
			}
			else
			{
				thirdStateTimer = 10f;
			}
		}
	}

	private void StateLookAround()
	{
		if (stateImpulse)
		{
			ResetNavMesh();
			stateImpulse = false;
			secondStateTimer = 3f;
		}
		stateTimer -= Time.deltaTime;
		secondStateTimer -= Time.deltaTime;
		if (secondStateTimer <= 0f)
		{
			UpdateState(State.SeekPlayer);
		}
	}

	private void StateSeekPlayer()
	{
		if (stateImpulse)
		{
			targetPosition = base.transform.position;
			LevelPoint levelPoint = SemiFunc.LevelPointInTargetRoomGet(enemy.Rigidbody.physGrabObject.roomVolumeCheck, 2f, 15f);
			if (!levelPoint)
			{
				UpdateState(State.Idle);
				return;
			}
			targetPosition = levelPoint.transform.position;
			Debug.DrawRay(targetPosition, Vector3.up, Color.green, 5f);
			stateImpulse = false;
		}
		if (Vector3.Distance(base.transform.position, targetPosition) < 1f)
		{
			UpdateState(State.LookAround);
			return;
		}
		JumpIfStuck();
		enemy.NavMeshAgent.OverrideAgent(3f, enemy.NavMeshAgent.DefaultAcceleration, 0.2f);
		enemy.NavMeshAgent.SetDestination(targetPosition);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f || enemy.Rigidbody.notMovingTimer > 3f)
		{
			UpdateState(State.Leave);
		}
	}

	private void StateInvestigate()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer += 3f;
			if (nonAggroInvestigate)
			{
				stateTimer = 10f;
			}
			enemy.Rigidbody.notMovingTimer = 0f;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		JumpIfStuck();
		enemy.NavMeshAgent.OverrideAgent(2.5f, 12f, 0.25f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
		else if (Vector3.Distance(base.transform.position, agentDestination) < 2f)
		{
			if (nonAggroInvestigate)
			{
				nonAggroInvestigate = false;
				UpdateState(State.Idle);
			}
			else
			{
				UpdateState(State.SeekPlayer);
			}
		}
		else if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StateCreepyStare()
	{
		SkipStateImpulse = true;
		if (stateImpulse)
		{
			stateImpulse = false;
			thirdStateTimer = 4f;
			creepyStareTimer = 30f;
		}
		thirdStateTimer -= Time.deltaTime;
		if (thirdStateTimer <= 0f)
		{
			SkipStateImpulse = true;
			UpdateState(State.Idle);
		}
	}

	private void StatePlayerNotice()
	{
		if (stateImpulse)
		{
			secondStateTimer = 1.2f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.ResetPath();
		enemy.NavMeshAgent.Stop(0.1f);
		secondStateTimer -= Time.deltaTime;
		if (secondStateTimer <= 0f)
		{
			UpdateState(State.Attack);
		}
	}

	private void StateFlyBackUp()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 30f;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		Vector3 target = new Vector3(base.transform.position.x, moveBackPosition.y + 1.5f, base.transform.position.z);
		base.transform.position = Vector3.MoveTowards(base.transform.position, target, Time.deltaTime * 5f);
		enemy.Rigidbody.OverrideFollowPosition(0.1f, 1f);
		enemy.Rigidbody.OverrideFollowRotation(0.1f, 0.25f);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * 5f);
		if (Vector3.Distance(enemy.Rigidbody.transform.position, base.transform.position) > 2f)
		{
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Despawn);
		}
		if (enemy.Rigidbody.transform.position.y - moveBackPosition.y > 1f)
		{
			UpdateState(State.FlyBackToNavMesh);
		}
	}

	private void StateFlyBackToNavMesh()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 20f;
			secondStateTimer = 0f;
		}
		enemy.NavMeshAgent.Disable(0.1f);
		base.transform.position = Vector3.MoveTowards(base.transform.position, moveBackPosition + Vector3.up * 0.5f, 0.75f * Time.deltaTime);
		enemy.Rigidbody.OverrideFollowPosition(0.1f, 1f);
		enemy.Rigidbody.OverrideFollowRotation(0.1f, 0.25f);
		if (Vector3.Distance(enemy.Rigidbody.transform.position, base.transform.position) > 2f)
		{
			base.transform.position = enemy.Rigidbody.transform.position;
		}
		if (secondStateTimer <= 0f)
		{
			secondStateTimer = 0.25f;
			if (Physics.Raycast(enemy.Rigidbody.transform.position, Vector3.down, out var hitInfo, 5f, LayerMask.GetMask("Default")))
			{
				if (Physics.Raycast(enemy.Rigidbody.transform.position + enemy.Rigidbody.transform.forward * 0.5f, Vector3.down, out hitInfo, 5f, LayerMask.GetMask("Default")) && NavMesh.SamplePosition(hitInfo.point, out var hit, 0.5f, -1))
				{
					moveBackPosition = hit.position;
					UpdateState(State.Idle);
					return;
				}
			}
			else if (Vector3.Distance(base.transform.position, moveBackPosition) < 0.1f)
			{
				UpdateState(State.Idle);
			}
		}
		else
		{
			secondStateTimer -= Time.deltaTime;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Despawn);
		}
	}

	public void OnSpawn()
	{
		if (!checkedForDuplicates)
		{
			CheckForDuplicates();
			checkedForDuplicates = true;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			UpdateState(State.Spawn);
		}
		if (anim.isActiveAndEnabled)
		{
			anim.SetSpawn();
		}
	}

	public void OnHurt()
	{
		anim.hurtSound.Play(base.transform.position);
		if (SemiFunc.IsMasterClientOrSingleplayer() && !AttackState() && !enemy.IsStunned())
		{
			stateTimer = 20f;
			UpdateState(State.SeekPlayer);
		}
	}

	public void OnVision()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState == State.GoToPlayerAngry && enemy.Vision.onVisionTriggeredPlayer == playerTarget)
		{
			UpdateState(State.PlayerNotice);
		}
	}

	public void OnDeath()
	{
		anim.deathSound.Play(base.transform.position);
		anim.PlayDeathParticles();
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			PopAllBalloons();
			enemy.EnemyParent.Despawn();
			UpdateState(State.Despawn);
		}
	}

	public void OnInvestigate()
	{
		if (currentState == State.SeekPlayer)
		{
			agentDestination = enemy.StateInvestigate.onInvestigateTriggeredPosition;
			UpdateState(State.Investigate);
		}
		else if (Vector3.Distance(base.transform.position, enemy.StateInvestigate.onInvestigateTriggeredPosition) > 15f)
		{
			agentDestination = enemy.StateInvestigate.onInvestigateTriggeredPosition;
			nonAggroInvestigate = true;
			UpdateState(State.Investigate);
		}
	}

	public void OnBalloonBlowComplete()
	{
		if (hasPendingBalloon)
		{
			SpawnBalloon(pendingBalloonPoint);
			hasPendingBalloon = false;
		}
	}

	public void CrouchedDown()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateState(State.AttackUnder);
		}
	}

	public void DoneWithBalloonBlow()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateState(State.PlaceBalloon);
		}
	}

	public void StoreBalloonLocation()
	{
		bPos = animatingBalloon.position;
		bScale = animatingBalloon.localScale;
	}

	private void ColliderSizeLogic()
	{
		if (startOfStateClient)
		{
			if (currentState == State.AttackUnder)
			{
				ToggleShrinkCollider(_active: true);
			}
			else if (currentState == State.StandUp)
			{
				ToggleShrinkCollider(_active: false);
			}
		}
	}

	private void RotationLogic()
	{
		if (enemy.NavMeshAgent.AgentVelocity.normalized.magnitude > 0.1f)
		{
			horizontalRotationTarget = Quaternion.LookRotation(enemy.NavMeshAgent.AgentVelocity.normalized);
			horizontalRotationTarget.eulerAngles = new Vector3(0f, horizontalRotationTarget.eulerAngles.y, 0f);
		}
		base.transform.rotation = SemiFunc.SpringQuaternionGet(horizontalRotationSpring, horizontalRotationTarget);
	}

	private void CheckForCreepyStareStart()
	{
		if (!CreepyStareStartState())
		{
			return;
		}
		if (enemy.OnScreen.OnScreenAny)
		{
			onScreenTimer += Time.deltaTime;
		}
		else
		{
			onScreenTimer = 0f;
		}
		creepyStareTimer -= Time.deltaTime;
		if (onScreenTimer > 3f && CreepyStareStartState() && creepyStareTimer <= 0f)
		{
			PlayerAvatar playerAvatar = PlayerNearby();
			if ((bool)playerAvatar)
			{
				UpdateLookTarget(playerAvatar);
				UpdateState(State.CreepyStare);
			}
			else
			{
				onScreenTimer = 0f;
			}
		}
	}

	private void CheckForStartOfStateClient()
	{
		if (currentState != previousState)
		{
			startOfStateClient = true;
			if (currentState != State.CreepyStare)
			{
				stateTimerClient = 0f;
			}
			previousState = currentState;
		}
		else
		{
			startOfStateClient = false;
		}
	}

	private void CheckForPoppedBalloons()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		foreach (KeyValuePair<Vector3, GameObject> balloon in balloons)
		{
			if (balloon.Value.GetComponentInChildren<BirthdayBoyBalloon>().popped)
			{
				if (currentState != State.GoToPlayerAngry && !AttackState())
				{
					playerTarget = balloon.Value.GetComponentInChildren<BirthdayBoyBalloon>().popper;
					UpdatePlayerTarget(balloon.Value.GetComponentInChildren<BirthdayBoyBalloon>().popper);
				}
				targetPosition = balloon.Value.transform.position;
				RemoveBalloon(balloon.Key);
				break;
			}
		}
	}

	private void JumpIfStuck(bool _cartJump = true)
	{
		if (_cartJump)
		{
			SemiFunc.EnemyCartJump(enemy);
		}
		Vector3 destination = enemy.NavMeshAgent.GetDestination();
		if (!enemy.Jump.jumping && !enemy.Jump.jumpingDelay && !enemy.Jump.landDelay && enemy.Rigidbody.notMovingTimer > 2f)
		{
			enemy.Jump.StuckTrigger(destination - enemy.Rigidbody.transform.position);
		}
	}

	private void ResetNavMesh()
	{
		enemy.NavMeshAgent.ResetPath();
		enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
	}

	private void UpdateMoveBackPosition()
	{
		if (SemiFunc.FPSImpulse1() && NavMesh.SamplePosition(base.transform.position, out var hit, 1.5f, -1) && Physics.Raycast(base.transform.position, Vector3.down, 2f, LayerMask.GetMask("Default")))
		{
			moveBackPosition = hit.position;
		}
	}

	private void RotateBodyTowards(Vector3 _target, float _slerpSpeed = 1f)
	{
		Vector3 forward = _target - visualMesh.position;
		forward.y = 0f;
		Quaternion quaternion = Quaternion.LookRotation(forward, Vector3.up);
		visualMesh.rotation = Quaternion.Slerp(visualMesh.rotation, quaternion, Time.deltaTime * _slerpSpeed);
	}

	private void BodyRotationLogic()
	{
		if (currentState == State.CreepyStare)
		{
			if (!(stateTimerClient < 1f) && (bool)anim.lookTarget)
			{
				RotateBodyTowards(anim.lookTarget.localCamera.transform.position, 0.2f);
			}
		}
		else if (currentState == State.PlaceBalloon && balloonPlacements != null && balloonPlacements.Count != 0)
		{
			int num = (int)(stateTimerClient / balloonInterval);
			if (num >= balloonPlacements.Count)
			{
				visualMesh.localRotation = Quaternion.Slerp(visualMesh.localRotation, Quaternion.identity, Time.deltaTime * 2f);
				return;
			}
			if (num >= balloonPlacements.Count)
			{
				num = balloonPlacements.Count - 1;
			}
			int index = balloonPlacements[num];
			RotateBodyTowards(balloonSpawnPoints[index].position, 2f);
		}
		else if (currentState == State.LookAround)
		{
			if (stateTimerClient > 1.5f)
			{
				Quaternion quaternion = Quaternion.Euler(0f, 180f, 0f);
				visualMesh.localRotation = Quaternion.Slerp(visualMesh.localRotation, quaternion, Time.deltaTime * 2f);
			}
		}
		else if (currentState == State.PlayerNotice && (bool)playerTarget)
		{
			RotateBodyTowards(playerTarget.localCamera.transform.position, 4f);
		}
		else
		{
			visualMesh.localRotation = Quaternion.Slerp(visualMesh.localRotation, Quaternion.identity, Time.deltaTime * 2f);
		}
	}

	private void RotateHeadTowardsPlayer(float speed = 1f, bool clamp = true)
	{
		PlayerAvatar playerAvatar = (playerTarget ? playerTarget : anim.lookTarget);
		if (!playerAvatar)
		{
			return;
		}
		Vector3 position;
		if (SemiFunc.IsMultiplayer() && !playerAvatar.isLocal)
		{
			position = playerAvatar.playerAvatarVisuals.headLookAtTransform.position;
		}
		else
		{
			position = playerAvatar.localCamera.transform.position;
			position.y -= 0.3f;
		}
		Vector3 normalized = (position - headPivot.position).normalized;
		if (!(normalized.sqrMagnitude < 1E-06f))
		{
			Quaternion quaternion = Quaternion.LookRotation(normalized, Vector3.up);
			Quaternion quaternion2 = Quaternion.Inverse(headPivot.parent ? headPivot.parent.rotation : Quaternion.identity) * quaternion;
			headPivot.localRotation = Quaternion.Slerp(headPivot.localRotation, quaternion2, Time.deltaTime * Mathf.Max(0f, speed));
			if (clamp)
			{
				Vector3 localEulerAngles = headPivot.localEulerAngles;
				float value = Mathf.DeltaAngle(0f, localEulerAngles.x);
				float value2 = Mathf.DeltaAngle(0f, localEulerAngles.y);
				value = Mathf.Clamp(value, 0f - maxHeadRotationUpDown, maxHeadRotationUpDown);
				value2 = Mathf.Clamp(value2, 0f - maxHeadRotationLeftRight, maxHeadRotationLeftRight);
				headPivot.localRotation = Quaternion.Euler(value, value2, 0f);
			}
		}
	}

	private void HeadRotationLogic()
	{
		if (currentState == State.LookAround)
		{
			Quaternion quaternion = Quaternion.Euler(0f, Mathf.Sin(Time.time * 2f) * 75f, 0f);
			headPivot.localRotation = Quaternion.Slerp(headPivot.localRotation, quaternion, Time.deltaTime * 2f);
		}
		else if (currentState == State.CreepyStare)
		{
			if (!(stateTimerClient < 0.5f))
			{
				RotateHeadTowardsPlayer();
			}
		}
		else if (LookAtTargetPlayerState() && currentState != State.AttackUnder)
		{
			RotateHeadTowardsPlayer();
		}
		else if (currentState == State.PlaceBalloon && !startOfStateClient && balloonPlacements != null && balloonPlacements.Count != 0 && !blowing)
		{
			int num = (int)(stateTimerClient / balloonInterval);
			if (num >= balloonPlacements.Count)
			{
				num = balloonPlacements.Count - 1;
			}
			if (num < 0)
			{
				num = 0;
			}
			int index = balloonPlacements[num];
			Quaternion quaternion2 = Quaternion.LookRotation(balloonSpawnPoints[index].position - headPivot.position, Vector3.up);
			headPivot.rotation = Quaternion.Slerp(headPivot.rotation, quaternion2, Time.deltaTime * 2f);
			Vector3 localEulerAngles = headPivot.localEulerAngles;
			float value = Mathf.DeltaAngle(0f, localEulerAngles.x);
			float value2 = Mathf.DeltaAngle(0f, localEulerAngles.y);
			value = Mathf.Clamp(value, 0f - maxHeadRotationUpDown, maxHeadRotationUpDown);
			value2 = Mathf.Clamp(value2, 0f - maxHeadRotationLeftRight, maxHeadRotationLeftRight);
			headPivot.localRotation = Quaternion.Euler(value, value2, 0f);
		}
		else if (currentState == State.PlayerNotice || currentState == State.GoToPlayerAngry)
		{
			RotateHeadTowardsPlayer(2f);
		}
		else if (currentState == State.Investigate)
		{
			Quaternion quaternion3 = Quaternion.LookRotation(enemy.StateInvestigate.onInvestigateTriggeredPosition - headPivot.position, Vector3.up);
			headPivot.rotation = Quaternion.Slerp(headPivot.rotation, quaternion3, Time.deltaTime * 2f);
			Vector3 localEulerAngles2 = headPivot.localEulerAngles;
			float value3 = Mathf.DeltaAngle(0f, localEulerAngles2.x);
			float value4 = Mathf.DeltaAngle(0f, localEulerAngles2.y);
			value3 = Mathf.Clamp(value3, 0f - maxHeadRotationUpDown, maxHeadRotationUpDown);
			value4 = Mathf.Clamp(value4, 0f - maxHeadRotationLeftRight, maxHeadRotationLeftRight);
			headPivot.localRotation = Quaternion.Euler(value3, value4, 0f);
		}
		else
		{
			headPivot.localRotation = Quaternion.Slerp(headPivot.localRotation, Quaternion.identity, Time.deltaTime * 2f);
		}
	}

	private void CheckForDuplicates()
	{
		List<EnemyParent> enemiesSpawned = EnemyDirector.instance.enemiesSpawned;
		int num = 0;
		int num2 = 0;
		foreach (EnemyParent item in enemiesSpawned)
		{
			if ((bool)item && item.name == "Enemy - Birthday boy(Clone)")
			{
				if (item == enemyParent)
				{
					myIndex = num2;
				}
				num++;
			}
			num2++;
		}
		if (num != 1 && myIndex != 0 && myIndex < 8)
		{
			balloonColor = colors[myIndex % colors.Length];
			animatingBalloon.GetComponentInChildren<MeshRenderer>().material.SetColor("_BaseColor", balloonColor);
			animatingBalloon.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", balloonColor * 0.28f);
			mr.material.SetColor("_AlbedoColor", balloonColor);
		}
	}

	public bool AttackState()
	{
		if (currentState != State.Attack && currentState != State.AttackUnder && currentState != State.AttackUnderStart)
		{
			return currentState == State.AttackOver;
		}
		return true;
	}

	private bool PlayerTargetState()
	{
		if (currentState != State.Attack && currentState != State.GoToPlayerAngry && currentState != State.AttackUnder && currentState != State.AttackUnderStart)
		{
			return currentState == State.AttackOver;
		}
		return true;
	}

	private bool CreepyStareStartState()
	{
		return currentState == State.Idle;
	}

	private bool CloseToNavMeshTarget(float _treshold = 1.5f)
	{
		return Mathf.Abs(Vector3.Distance(base.transform.position, enemy.NavMeshAgent.GetDestination())) < _treshold;
	}

	private bool VisionRelevantState()
	{
		if (currentState != State.SeekPlayer && currentState != State.Investigate && currentState != State.LookAround)
		{
			if (currentState == State.GoToBalloonAngry)
			{
				return CloseToNavMeshTarget(5f);
			}
			return false;
		}
		return true;
	}

	private bool LookAtTargetPlayerState()
	{
		if (currentState != State.Attack && currentState != State.AttackUnder && currentState != State.AttackUnderStart && currentState != State.AttackOver)
		{
			return currentState == State.StandUp;
		}
		return true;
	}

	private bool PlayerOnNavMesh()
	{
		if (!playerTarget)
		{
			return false;
		}
		NavMeshHit hit;
		return NavMesh.SamplePosition(playerTarget.transform.position, out hit, 0.5f, -1);
	}

	private bool NavMeshState()
	{
		if (currentState != State.AttackUnder && currentState != State.AttackOver && currentState != State.AttackUnderStart && currentState != State.FlyBackUp && currentState != State.FlyBackToNavMesh)
		{
			return currentState != State.Stunned;
		}
		return false;
	}

	private bool FlyState()
	{
		if (currentState != State.FlyBackUp)
		{
			return currentState == State.FlyBackToNavMesh;
		}
		return true;
	}

	private float CheckIfPlayersNearbyPop(Vector3 _pos)
	{
		List<PlayerAvatar> list = SemiFunc.PlayerGetList();
		float num = float.PositiveInfinity;
		foreach (PlayerAvatar item in list)
		{
			Vector3 position = item.transform.position;
			if (item.isDisabled)
			{
				position = item.playerDeathHead.transform.position;
			}
			position.y = 0f;
			Vector3 vector = _pos;
			vector.y = 0f;
			float num2 = Vector3.Distance(position, vector);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	private List<int> GetNonCollidingBalloonSpawnPoints()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < balloonSpawnPoints.Count; i++)
		{
			Vector3 position = balloonSpawnPoints[i].position;
			if (!IsColliding(balloonCollisionChecker, position) && !IsCollidingWithOtherBalloons(position) && !IsTooCloseToExistingBalloons(position, preferredBalloonDistance))
			{
				list.Add(i);
			}
		}
		return list;
	}

	private List<int> GetBalloonSpawnPointIndexes(List<int> _nonCollidingBalloonSpawnPoints)
	{
		List<int> list = new List<int>();
		int num = Mathf.Min(_nonCollidingBalloonSpawnPoints.Count, minBalloonsPerPlacement);
		int num2 = Mathf.Min(_nonCollidingBalloonSpawnPoints.Count, maxBalloonsPerPlacement);
		if (balloons.Count < maxBalloons)
		{
			num2 = Mathf.Min(num2, maxBalloons - balloons.Count);
			if (num > num2)
			{
				num = num2;
			}
		}
		int num3 = Random.Range(num, num2);
		for (int i = 0; i < num3; i++)
		{
			int num4 = 0;
			bool flag = false;
			int num5;
			do
			{
				int index = Random.Range(0, _nonCollidingBalloonSpawnPoints.Count);
				num5 = _nonCollidingBalloonSpawnPoints[index];
				if (!list.Contains(num5))
				{
					Vector3 position = balloonSpawnPoints[num5].position;
					flag = true;
					foreach (int item in list)
					{
						Vector3 position2 = balloonSpawnPoints[item].position;
						if (Vector3.Distance(position, position2) < preferredBalloonDistance)
						{
							flag = false;
							SemiLogger.LogMonika(" Too close to already selected point. Distance: " + Vector3.Distance(position, position2));
							break;
						}
						SemiLogger.LogMonika("Not too close. Distance: " + Vector3.Distance(position, position2));
					}
				}
				num4++;
			}
			while (num4 <= 50 && !flag);
			if (flag)
			{
				list.Add(num5);
			}
		}
		return list;
	}

	private PlayerAvatar PlayerNearby()
	{
		PlayerAvatar result = null;
		float num = float.MaxValue;
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (!item.isDisabled)
			{
				float num2 = Vector3.Distance(item.transform.position, base.transform.position);
				if (num2 < num)
				{
					result = item;
					num = num2;
				}
			}
		}
		if (num <= 10f)
		{
			return result;
		}
		return null;
	}

	private void ToggleShrinkCollider(bool _active)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("ToggleShrinkColliderRPC", RpcTarget.All, _active);
		}
		else
		{
			ToggleShrinkColliderRPC(_active);
		}
	}

	private Collider[] GetCollidingColliders(Transform _collisionChecker, Vector3 _pos)
	{
		return Physics.OverlapBox(_pos, _collisionChecker.lossyScale / 2f, _collisionChecker.rotation);
	}

	private bool NearbyDoor(Vector3 _pos)
	{
		Collider[] collidingColliders = GetCollidingColliders(doorCollisionChecker, _pos);
		for (int i = 0; i < collidingColliders.Length; i++)
		{
			if (collidingColliders[i].gameObject.layer == LayerMask.NameToLayer("PhysGrabObjectHinge"))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsColliding(Transform _collisionChecker, Vector3 _pos, bool _collideWithPlayer = true)
	{
		Collider[] collidingColliders = GetCollidingColliders(_collisionChecker, _pos);
		foreach (Collider collider in collidingColliders)
		{
			if (collider.gameObject.layer == LayerMask.NameToLayer("RoomVolume") || collider.gameObject.layer == LayerMask.NameToLayer("Triggers") || collider.transform.IsChildOf(base.transform) || collider.transform.IsChildOf(enemy.EnemyParent.transform) || !(collider.name != "Trigger Collider Balloon (Hurt collider)") || collider.name.Contains("Level Point"))
			{
				continue;
			}
			BirthdayBoyBalloon componentInParent = collider.GetComponentInParent<BirthdayBoyBalloon>();
			if ((bool)componentInParent && componentInParent.placerIndex != myIndex)
			{
				if (componentInParent.placerIndex != myIndex)
				{
					return true;
				}
			}
			else if ((_collideWithPlayer || !collider.CompareTag("Player")) && collider.gameObject.layer != LayerMask.NameToLayer("StaticGrabObject"))
			{
				string text = collider.name;
				Vector3 vector = _pos;
				SemiLogger.LogMonika("Colliding with: " + text + " at position: " + vector.ToString());
				return true;
			}
		}
		return false;
	}

	private bool IsCollidingWithOtherBalloons(Vector3 _pos)
	{
		foreach (KeyValuePair<Vector3, GameObject> balloon in balloons)
		{
			float num = Mathf.Abs(balloon.Key.y - _pos.y);
			if (Mathf.Abs(balloon.Key.x - _pos.x) < balloonCollisionChecker.lossyScale.x / 2f && num < balloonCollisionChecker.lossyScale.y / 2f)
			{
				string text = balloon.Key.ToString();
				Vector3 vector = _pos;
				SemiLogger.LogMonika("Colliding with other balloon at: " + text + "position: " + vector.ToString());
				return true;
			}
		}
		return false;
	}

	private bool IsTooCloseToExistingBalloons(Vector3 _pos, float _minDistance)
	{
		foreach (KeyValuePair<Vector3, GameObject> balloon in balloons)
		{
			if (Vector3.Distance(balloon.Key, _pos) < _minDistance)
			{
				return true;
			}
		}
		foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
		{
			if (!item || !(item.name == "Enemy - Birthday boy(Clone)"))
			{
				continue;
			}
			EnemyBirthdayBoy componentInChildren = item.GetComponentInChildren<EnemyBirthdayBoy>();
			if (!componentInChildren || !(componentInChildren != this))
			{
				continue;
			}
			foreach (KeyValuePair<Vector3, GameObject> balloon2 in componentInChildren.balloons)
			{
				if (Vector3.Distance(balloon2.Key, _pos) < _minDistance)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void GetAngry()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !enemy.IsStunned() && currentState != State.FlyBackUp && currentState != State.FlyBackToNavMesh && currentState != State.LookAround && currentState != State.PlayerNotice && currentState != State.Despawn && !AttackState())
		{
			stateTimer = 20f;
			UpdateState(State.LookAround);
		}
	}
}
