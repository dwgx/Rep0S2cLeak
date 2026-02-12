using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpinny : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Roam,
		Investigate,
		PlayerNotice,
		GoToPlayer,
		Leave,
		WaitForRoulette,
		Roulette,
		RouletteEndPause,
		RouletteEnd,
		RouletteEffect,
		CloseMouth,
		Stunned,
		Despawn
	}

	public enum Colors
	{
		Red,
		Yellow,
		Green,
		White,
		Black
	}

	public State currentState;

	public readonly float[] SliceBorders = new float[9] { 22f, 67f, 90f, 112f, 157f, 202f, 247f, 292f, 337f };

	public readonly Colors[] SliceColours = new Colors[9]
	{
		Colors.Red,
		Colors.White,
		Colors.Black,
		Colors.Red,
		Colors.Green,
		Colors.Red,
		Colors.Yellow,
		Colors.Red,
		Colors.Green
	};

	public Color[] colors = new Color[9]
	{
		Color.red,
		Color.white,
		Color.black,
		Color.red,
		Color.green,
		Color.red,
		Color.yellow,
		Color.red,
		Color.green
	};

	[Header("Components")]
	public Transform spinnyWheel;

	[Space]
	public Transform[] pieces;

	[Space]
	public Transform playerLockPoint;

	public SemiLine semiLine1;

	public SemiLine semiLine2;

	[Space]
	public GameObject moneyBag;

	public GameObject surplusValuableSmall;

	public Transform moneyBagSpawnPoint;

	[Space]
	public EnemySpinnyAnim enemySpinnyAnim;

	[Header("Collision check")]
	public Transform collisionCheck;

	public Transform moneyCollisionCheck;

	[Header("Spin variables")]
	public float spinDurationSeconds = 3.8f;

	[Space]
	public Transform lineOfSightChecker;

	[Header("Lock in variables")]
	public float _torqueMultiplier = 0.5f;

	public float _followForceMultiplier = 0.5f;

	private float _grabbedTorqueMultiplier = 0.025f;

	private float _followForceGrabbedMultiplier = 0.025f;

	[HideInInspector]
	public PlayerAvatar playerTarget;

	[HideInInspector]
	public int extraSpins;

	[HideInInspector]
	public float targetAngleDegrees;

	private float stateTimer;

	private float lockPointTimer;

	private float offLockPointTimer;

	private float grabAggroTimer;

	private float minZPosition = 0.784f;

	private float maxStateTimer;

	private int minExtraSpins = 2;

	private int maxExtraSpins = 4;

	private bool stateImpulse;

	private bool reachedPoint;

	private bool jinglePlayed;

	private bool effectDone;

	private bool isColliding;

	private bool isCollidingOffset;

	private bool moneyToBeSpawned;

	private bool isCollidingMoney;

	private Vector3 agentDestination;

	private Vector3 playerLockPointOriginalPosition;

	private Enemy enemy;

	private PhotonView photonView;

	[HideInInspector]
	public ParticleScriptExplosion explosionParticle;

	private float lockLerpSpeed = 1f;

	private float lockHoldSpeed = 0.3f;

	private float lockLerp;

	private float lockResetTimer;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
		playerLockPointOriginalPosition = playerLockPoint.localPosition;
		explosionParticle = GetComponent<ParticleScriptExplosion>();
	}

	private void FixedUpdate()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (currentState == State.WaitForRoulette)
		{
			if (stateTimer <= 2f && !reachedPoint)
			{
				LockInPlayer(_horizontalPull: true, _fixedUpdate: true);
			}
			else
			{
				LockInPlayer(_horizontalPull: false, _fixedUpdate: true);
			}
		}
		else if (RouletteGoingOn())
		{
			LockInPlayer(_horizontalPull: false, _fixedUpdate: true);
		}
	}

	private void Update()
	{
		if (RouletteGoingOn())
		{
			playerTarget.tumble.OverrideEnemyHurt(1f);
			semiLine1.LineActive(playerTarget.tumble.physGrabObject.rb.transform);
			semiLine2.LineActive(playerTarget.tumble.physGrabObject.rb.transform);
			SemiFunc.PlayerEyesOverride(playerTarget, enemy.Vision.VisionTransform.position, 0.1f, base.gameObject);
			if (playerTarget.isLocal)
			{
				OverrideTargetPlayerCameraAim(7f, 10f);
			}
			if (currentState == State.WaitForRoulette)
			{
				jinglePlayed = false;
				effectDone = false;
			}
			else if (currentState == State.RouletteEnd)
			{
				if (!jinglePlayed)
				{
					switch (GetCurrentColorbyAngle(targetAngleDegrees))
					{
					case Colors.Green:
						enemySpinnyAnim.jingleGreen.Play(enemy.CenterTransform.position);
						enemySpinnyAnim.healParticle.PlayParticles();
						break;
					case Colors.Red:
						enemySpinnyAnim.jingleRed.Play(enemy.CenterTransform.position);
						enemySpinnyAnim.hurtParticle.PlayParticles();
						break;
					case Colors.Black:
						enemySpinnyAnim.jingleBlackGlobal.Play(enemy.CenterTransform.position);
						enemySpinnyAnim.jingleBlack.Play(enemy.CenterTransform.position);
						enemySpinnyAnim.deathParticle.PlayParticles();
						break;
					case Colors.White:
						enemySpinnyAnim.jingleWhite.Play(enemy.CenterTransform.position);
						enemySpinnyAnim.fullHealParticle.PlayParticles();
						break;
					case Colors.Yellow:
						enemySpinnyAnim.jingleYellow.Play(enemy.CenterTransform.position);
						enemySpinnyAnim.moneyPrizeParticle.PlayParticles();
						break;
					}
					jinglePlayed = true;
				}
			}
			else if (currentState == State.RouletteEffect)
			{
				Colors currentColorbyAngle = GetCurrentColorbyAngle(targetAngleDegrees);
				if (!effectDone)
				{
					switch (currentColorbyAngle)
					{
					case Colors.Green:
						if (playerTarget.isLocal)
						{
							playerTarget.playerHealth.Heal(25);
						}
						else
						{
							enemySpinnyAnim.healingSmokeParticle.Play(withChildren: true);
						}
						break;
					case Colors.Red:
						if (playerTarget.isLocal)
						{
							playerTarget.playerHealth.Hurt(50, savingGrace: false);
						}
						else
						{
							enemySpinnyAnim.smallhurtParticle.Play(withChildren: true);
						}
						break;
					case Colors.Black:
						if (playerTarget.isLocal)
						{
							int damage = playerTarget.playerHealth.health - 1;
							playerTarget.playerHealth.Hurt(damage, savingGrace: false);
						}
						enemySpinnyAnim.lightningParticle.Play(withChildren: true);
						break;
					case Colors.White:
						if (playerTarget.isLocal)
						{
							int maxHealth = playerTarget.playerHealth.maxHealth;
							playerTarget.playerHealth.HealOther(maxHealth, effect: true);
						}
						enemySpinnyAnim.fullHealSmokeParticle.Play(withChildren: true);
						break;
					case Colors.Yellow:
						moneyToBeSpawned = true;
						isCollidingMoney = IsCollidingMoneyBag();
						break;
					}
					CameraShake();
					effectDone = true;
				}
			}
		}
		if (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		if (currentState == State.PlayerNotice)
		{
			LerpToFaceTargetPlayer(6f);
		}
		else if (RouletteGoingOn() || currentState == State.GoToPlayer)
		{
			LerpToFaceTargetPlayer();
		}
		MovePlayerLockPoint();
		if (moneyToBeSpawned)
		{
			if (SemiFunc.FPSImpulse1())
			{
				isCollidingMoney = IsCollidingMoneyBag();
			}
			if (!isCollidingMoney)
			{
				SpawnMoneyBag();
			}
		}
		if (grabAggroTimer > 0f)
		{
			grabAggroTimer -= Time.deltaTime;
		}
		if (enemy.CurrentState == EnemyState.Despawn)
		{
			UpdateState(State.Despawn);
		}
		else if (enemy.IsStunned())
		{
			if (Interruptable())
			{
				InterruptExplosion();
			}
			UpdateState(State.Stunned);
		}
		else if (RouletteGoingOn() && (!playerTarget || playerTarget.isDisabled))
		{
			UpdateState(State.CloseMouth);
		}
		switch (currentState)
		{
		case State.Spawn:
			StateSpawn();
			break;
		case State.Idle:
			StateIdle();
			break;
		case State.Roam:
			StateRoam();
			break;
		case State.Investigate:
			StateInvestigate();
			break;
		case State.PlayerNotice:
			StatePlayerNotice();
			break;
		case State.GoToPlayer:
			StateGoToPlayer();
			break;
		case State.Leave:
			StateLeave();
			break;
		case State.Roulette:
			StateRoulette();
			break;
		case State.WaitForRoulette:
			StateWaitForRoulette();
			break;
		case State.RouletteEndPause:
			StateRouletteEndPause();
			break;
		case State.RouletteEnd:
			StateRouletteEnd();
			break;
		case State.RouletteEffect:
			StateRouletteEffect();
			break;
		case State.CloseMouth:
			StateCloseMouth();
			break;
		case State.Stunned:
			StateStunned();
			break;
		case State.Despawn:
			StateDespawn();
			break;
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
		if (stateTimer <= 0f)
		{
			UpdateState(State.Idle);
		}
	}

	private void StateIdle()
	{
		if (stateImpulse)
		{
			ResetNavMesh();
			stateTimer = Random.Range(2f, 4f);
			stateImpulse = false;
		}
		if (SemiFunc.EnemySpawnIdlePause())
		{
			return;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Roam);
			return;
		}
		if (enemy.IsStunned())
		{
			UpdateState(State.Stunned);
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StateRoam()
	{
		if (stateImpulse)
		{
			ResetNavMesh();
			LevelPoint levelPoint = SemiFunc.LevelPointGet(base.transform.position, 5f, 15f);
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
			}
			if (!levelPoint || !NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1))
			{
				UpdateState(State.Idle);
				return;
			}
			if (Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				agentDestination = hit.position;
			}
			stateTimer = 5f;
			stateImpulse = false;
			enemy.Rigidbody.notMovingTimer = 0f;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		JumpIfStuck();
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		if (stateTimer <= 0f || Vector3.Distance(base.transform.position, agentDestination) < 1f)
		{
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.Idle);
			return;
		}
		if (enemy.IsStunned())
		{
			UpdateState(State.Stunned);
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StateInvestigate()
	{
		if (stateImpulse)
		{
			stateTimer = 5f;
			enemy.Rigidbody.notMovingTimer = 0f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.SetDestination(agentDestination);
		JumpIfStuck();
		enemy.NavMeshAgent.OverrideAgent(2.5f, 12f, 0.25f);
		if (enemy.Rigidbody.notMovingTimer > 2f)
		{
			stateTimer -= Time.deltaTime;
		}
		if (stateTimer <= 0f || Vector3.Distance(base.transform.position, agentDestination) < 2f)
		{
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.Idle);
			return;
		}
		if (enemy.IsStunned())
		{
			UpdateState(State.Stunned);
		}
		if (SemiFunc.EnemyForceLeave(enemy))
		{
			UpdateState(State.Leave);
		}
	}

	private void StatePlayerNotice()
	{
		if (stateImpulse)
		{
			stateTimer = 1f;
			stateImpulse = false;
		}
		enemy.NavMeshAgent.ResetPath();
		enemy.NavMeshAgent.Stop(0.1f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f && !enemy.IsStunned())
		{
			enemy.NavMeshAgent.Stop(0f);
			stateTimer = 4f;
			UpdateState(State.GoToPlayer);
		}
		else if (enemy.IsStunned())
		{
			UpdateState(State.Stunned);
		}
	}

	private void StateGoToPlayer()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			enemy.Rigidbody.notMovingTimer = 0f;
			enemy.NavMeshAgent.SetDestination(playerTarget.transform.position);
			maxStateTimer = 7f;
			return;
		}
		enemy.NavMeshAgent.SetDestination(playerTarget.transform.position);
		if (CloseToPlayerTarget() && !enemy.IsStunned() && PlayerTargetInSight())
		{
			enemy.NavMeshAgent.ResetPath();
			GetAndSyncRandomRotation();
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.WaitForRoulette);
			return;
		}
		JumpIfStuck();
		if (Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetDestination()) < 1f && enemy.Rigidbody.transform.position.y - playerTarget.transform.position.y < -0.5f)
		{
			enemy.Jump.StuckTrigger(playerTarget.transform.position - enemy.Rigidbody.transform.position);
			enemy.Rigidbody.DisableFollowPosition(1f, 10f);
		}
		enemy.NavMeshAgent.OverrideAgent(4f, 10f, 0.25f);
		stateTimer -= Time.deltaTime;
		maxStateTimer -= Time.deltaTime;
		if (stateTimer <= 0f || maxStateTimer <= 0f)
		{
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.Idle);
		}
	}

	private void StateWaitForRoulette()
	{
		float num = 1.5f;
		if (stateImpulse)
		{
			stateTimer = 5f;
			lockPointTimer = 2f;
			offLockPointTimer = 0f;
			reachedPoint = false;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 2f && !reachedPoint)
		{
			LockInPlayer(_horizontalPull: true);
		}
		else
		{
			LockInPlayer();
		}
		if (Vector3.Distance(playerTarget.tumble.rb.position, playerLockPoint.position) < 1f)
		{
			reachedPoint = true;
			offLockPointTimer = 0f;
			lockPointTimer -= Time.deltaTime;
			if (lockPointTimer <= 0f)
			{
				if (HasLineOfSight())
				{
					UpdateState(State.Roulette);
				}
				else
				{
					UpdateState(State.CloseMouth);
				}
				return;
			}
		}
		else if (reachedPoint)
		{
			offLockPointTimer += Time.deltaTime;
			lockPointTimer = 3f;
		}
		if (enemy.Rigidbody.physGrabObject.grabbed && enemySpinnyAnim.mouthOpened)
		{
			UpdateState(State.Roulette);
		}
		else if (stateTimer <= 0f && !reachedPoint)
		{
			UpdateState(State.CloseMouth);
		}
		else if (enemy.IsStunned() || offLockPointTimer > num)
		{
			UpdateState(State.CloseMouth);
		}
	}

	private void StateRoulette()
	{
		LockInPlayer();
		if (stateImpulse)
		{
			stateTimer = spinDurationSeconds;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer < 0f)
		{
			UpdateState(State.RouletteEndPause);
		}
	}

	private void StateRouletteEndPause()
	{
		LockInPlayer();
		if (stateImpulse)
		{
			stateTimer = 1.5f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer < 0f)
		{
			UpdateState(State.RouletteEnd);
		}
	}

	private void StateRouletteEnd()
	{
		LockInPlayer();
		if (stateImpulse)
		{
			stateTimer = 0.5f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer < 0f)
		{
			UpdateState(State.RouletteEffect);
		}
	}

	private void StateRouletteEffect()
	{
		if (stateImpulse)
		{
			stateTimer = 1f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		LockInPlayer();
		if (stateTimer <= 0f)
		{
			UpdateState(State.CloseMouth);
		}
	}

	private void StateCloseMouth()
	{
		if (stateImpulse)
		{
			stateTimer = 1.5f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer < 0f)
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
				levelPoint = SemiFunc.LevelPointGet(base.transform.position, 20f, 999f);
				if (!levelPoint)
				{
					levelPoint = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
				}
				agentDestination = levelPoint.transform.position;
			}
			stateTimer = 10f;
			stateImpulse = false;
			SemiFunc.EnemyLeaveStart(enemy);
			enemy.Rigidbody.notMovingTimer = 0f;
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

	private bool Interruptable()
	{
		if (currentState != State.Roulette && currentState != State.RouletteEnd && currentState != State.RouletteEndPause)
		{
			return currentState == State.RouletteEffect;
		}
		return true;
	}

	private void LockInPlayer(bool _horizontalPull = false, bool _fixedUpdate = false)
	{
		if (!playerTarget)
		{
			return;
		}
		if (!playerTarget.isTumbling)
		{
			playerTarget.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
			lockLerp = 0f;
			lockResetTimer = 0f;
			return;
		}
		playerTarget.tumble.TumbleOverrideTime(1f);
		playerTarget.tumble.GetComponent<PhysGrabObject>().OverrideZeroGravity();
		Rigidbody rb = playerTarget.tumble.rb;
		Vector3 position = rb.position;
		Vector3 position2 = playerLockPoint.position;
		Vector3 normalized = (position2 - position).normalized;
		if (_horizontalPull)
		{
			normalized.y = 0f;
		}
		Vector3 vector = SemiFunc.PhysFollowDirection(rb.transform, normalized, rb, 10f);
		Vector3 vector2 = SemiFunc.PhysFollowPosition(position, position2, rb.velocity, 5f);
		float num = (_fixedUpdate ? Time.fixedDeltaTime : Time.deltaTime);
		lockLerp = Mathf.Clamp01(lockLerp + lockLerpSpeed * num);
		Vector3 vector3 = Vector3.Lerp(Vector3.zero, vector2, lockLerp);
		Vector3 vector4 = Vector3.Lerp(Vector3.zero, vector, lockLerp);
		bool flag = Vector3.Distance(position, position2) > 0.2f || (playerTarget.rbVelocity.magnitude > lockHoldSpeed && _fixedUpdate);
		if (_fixedUpdate && !playerTarget.tumble.physGrabObject.grabbed)
		{
			rb.AddTorque(vector4 / rb.mass * _torqueMultiplier, ForceMode.Force);
			rb.AddForce(vector3 * _followForceMultiplier, ForceMode.Impulse);
		}
		else if (_fixedUpdate)
		{
			rb.AddTorque(vector4 / rb.mass * _grabbedTorqueMultiplier, ForceMode.Force);
			rb.AddForce(vector3 * _followForceGrabbedMultiplier, ForceMode.Impulse);
			lockLerp = 0f;
			lockResetTimer = 0f;
		}
		bool flag2 = Vector3.Distance(rb.position, position2) > 0.1f || Quaternion.Angle(rb.rotation, playerLockPoint.rotation) > 1f;
		if (lockLerp >= 1f && flag2)
		{
			lockResetTimer += Time.fixedDeltaTime;
			if (lockResetTimer > 3f)
			{
				lockResetTimer = 0f;
				lockLerp = 0f;
			}
		}
		else
		{
			lockResetTimer = 0f;
		}
		if (playerTarget.tumble.physGrabObject.playerGrabbing.Count <= 0 || currentState != State.WaitForRoulette)
		{
			if (!flag && _fixedUpdate)
			{
				rb.velocity *= 0.98f;
				rb.angularVelocity *= 0.98f;
			}
			playerTarget.tumble.physGrabObject.OverrideGrabForceZero();
		}
	}

	private Collider[] GetCollidingColliders(float _zOffset = 0f)
	{
		Vector3 position = collisionCheck.position;
		position.z += _zOffset;
		return Physics.OverlapBox(position, collisionCheck.lossyScale / 2f, collisionCheck.rotation);
	}

	private Collider[] GetCollidingCollidersMoneyBag()
	{
		return Physics.OverlapBox(moneyCollisionCheck.position, moneyCollisionCheck.lossyScale / 2f, moneyCollisionCheck.rotation);
	}

	private bool IsColliding(float _zOffset = 0f)
	{
		Collider[] collidingColliders = GetCollidingColliders(_zOffset);
		foreach (Collider collider in collidingColliders)
		{
			if (collider.gameObject.layer != LayerMask.NameToLayer("RoomVolume") && !collider.transform.IsChildOf(base.transform) && collider.gameObject.layer != LayerMask.NameToLayer("CollisionCheck") && collider.gameObject.layer != LayerMask.NameToLayer("Triggers") && collider.gameObject.layer != LayerMask.NameToLayer("PlayerVisuals") && !HasParentWithTag(collider.transform, "Player"))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsCollidingMoneyBag()
	{
		Collider[] collidingCollidersMoneyBag = GetCollidingCollidersMoneyBag();
		foreach (Collider collider in collidingCollidersMoneyBag)
		{
			if (collider.gameObject.layer != LayerMask.NameToLayer("RoomVolume") && collider.gameObject.layer != LayerMask.NameToLayer("Triggers"))
			{
				return true;
			}
		}
		return false;
	}

	private void LerpToFaceTargetPlayer(float _speed = 3f)
	{
		Vector3 forward = playerTarget.transform.position - base.transform.position;
		forward.y = 0f;
		Quaternion quaternion = Quaternion.LookRotation(forward);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, Time.deltaTime * _speed);
	}

	private void OverrideTargetPlayerCameraAim(float _strenght = 3f, float _strenghtNoAim = 1.5f)
	{
		CameraAim.Instance.AimTargetSoftSet(spinnyWheel.position, 0.1f, _strenght, _strenghtNoAim, base.gameObject, 100);
	}

	public float GetRotation360()
	{
		float num = spinnyWheel.localEulerAngles.z;
		if (num < 0f)
		{
			num += 360f;
		}
		if (num >= 360f)
		{
			num -= 360f;
		}
		return num;
	}

	private void MovePlayerLockPoint(float _lerpSpeed = 0.1f)
	{
		if (SemiFunc.FPSImpulse1())
		{
			isColliding = IsColliding();
			isCollidingOffset = IsColliding(0.1f);
		}
		if (!Interruptable())
		{
			if (isColliding)
			{
				Vector3 localPosition = playerLockPoint.localPosition;
				Vector3 localPosition2 = playerLockPoint.localPosition;
				localPosition2.z = minZPosition;
				playerLockPoint.localPosition = Vector3.Lerp(localPosition, localPosition2, _lerpSpeed);
			}
			else if (!isCollidingOffset)
			{
				Vector3 localPosition3 = playerLockPoint.localPosition;
				Vector3 vector = playerLockPointOriginalPosition;
				playerLockPoint.localPosition = Vector3.Lerp(localPosition3, vector, _lerpSpeed);
			}
		}
	}

	public bool RouletteGoingOn()
	{
		if (currentState != State.WaitForRoulette && currentState != State.Roulette && currentState != State.RouletteEndPause && currentState != State.RouletteEnd)
		{
			return currentState == State.RouletteEffect;
		}
		return true;
	}

	private void CameraShake()
	{
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
	}

	private void ResetNavMesh()
	{
		enemy.NavMeshAgent.ResetPath();
		enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
	}

	private void JumpIfStuck(bool _cartJump = true)
	{
		if (_cartJump)
		{
			SemiFunc.EnemyCartJump(enemy);
		}
		Vector3 destination = enemy.NavMeshAgent.GetDestination();
		if (!enemy.Jump.jumping && !enemy.IsStunned() && enemy.Rigidbody.notMovingTimer > 2f)
		{
			enemy.Jump.StuckTrigger(destination - enemy.Rigidbody.transform.position);
		}
	}

	private bool HasLineOfSight()
	{
		return SemiFunc.PlayerVisionCheck(lineOfSightChecker.position, 10f, playerTarget, _previouslySeen: false);
	}

	private bool CloseToPlayerTarget(float _treshold = 1.5f)
	{
		return Vector3.Distance(enemy.Rigidbody.transform.position, playerTarget.transform.position) < _treshold;
	}

	private bool PlayerTargetInSight()
	{
		return enemy.Vision.VisionsTriggered[playerTarget.photonView.ViewID] != 0;
	}

	private bool HasParentWithTag(Transform _transform, string _tag)
	{
		Transform parent = _transform.parent;
		while (parent != null)
		{
			if (parent.CompareTag(_tag))
			{
				return true;
			}
			parent = parent.parent;
		}
		return false;
	}

	private void SpawnMoneyBag()
	{
		MoneyEffect();
		moneyToBeSpawned = false;
		GameObject gameObject = AssetManager.instance.surplusValuableSmall;
		GameObject gameObject2 = (SemiFunc.IsMultiplayer() ? PhotonNetwork.InstantiateRoomObject("Valuables/" + gameObject.name, moneyBagSpawnPoint.position, Quaternion.identity, 0) : Object.Instantiate(gameObject, moneyBagSpawnPoint.position, Quaternion.identity));
		gameObject2.GetComponent<ValuableObject>().dollarValueOverride = 5000;
	}

	public Color GetCurrentColorColor(float _angle = -1f)
	{
		Color result = Color.blue;
		switch ((_angle != -1f) ? GetCurrentColorbyAngle(_angle) : GetCurrentColor())
		{
		case Colors.White:
			result = Color.white;
			break;
		case Colors.Black:
			result = Color.black;
			break;
		case Colors.Red:
			result = Color.red;
			break;
		case Colors.Yellow:
			result = Color.yellow;
			break;
		case Colors.Green:
			result = Color.green;
			break;
		}
		return result;
	}

	public Colors GetCurrentColor()
	{
		return GetSliceAndColour(GetRotation360()).colour;
	}

	public Colors GetCurrentColorbyAngle(float _angle)
	{
		return GetSliceAndColour(_angle).colour;
	}

	public (int sliceIndex, Colors colour) GetSliceAndColour(float zAngle360)
	{
		zAngle360 = Mathf.Repeat(zAngle360, 360f);
		int i = 0;
		if (zAngle360 < 22f)
		{
			return (sliceIndex: 8, colour: SliceColours[8]);
		}
		for (; i < SliceBorders.Length - 1 && zAngle360 >= SliceBorders[i + 1]; i++)
		{
		}
		return (sliceIndex: i, colour: SliceColours[i]);
	}

	private void InterruptExplosion()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				InterruptExplosionRPC();
			}
			else
			{
				photonView.RPC("InterruptExplosionRPC", RpcTarget.All);
			}
		}
	}

	[PunRPC]
	private void InterruptExplosionRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		explosionParticle.Spawn(spinnyWheel.position, 1.3f, 60, 0);
		enemySpinnyAnim.interruptParticle.PlayParticles();
		enemySpinnyAnim.interruptionSound.Play(base.transform.position);
	}

	private void MoneyEffect()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				MoneyEffectRPC();
			}
			else
			{
				photonView.RPC("MoneyEffectRPC", RpcTarget.All);
			}
		}
	}

	[PunRPC]
	private void MoneyEffectRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			enemySpinnyAnim.moneyParticle.Play(withChildren: true);
		}
	}

	private void GetAndSyncRandomRotation()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			int num = Random.Range(minExtraSpins, maxExtraSpins + 1);
			float num2 = Random.Range(0f, 360f);
			if (!SemiFunc.IsMultiplayer())
			{
				GetAndSyncRandomRotationRPC(num, num2);
				return;
			}
			photonView.RPC("GetAndSyncRandomRotationRPC", RpcTarget.All, num, num2);
		}
	}

	[PunRPC]
	private void GetAndSyncRandomRotationRPC(int _extraSpins, float _targetAngleDegrees, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			extraSpins = _extraSpins;
			targetAngleDegrees = _targetAngleDegrees;
		}
	}

	private void UpdateState(State _nextState)
	{
		if (_nextState != currentState)
		{
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

	private void UpdatePlayerTarget(int photonViewID)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				UpdatePlayerTargetRPC(photonViewID);
				return;
			}
			photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.All, photonViewID);
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

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			UpdateState(State.Spawn);
		}
		if (enemySpinnyAnim.isActiveAndEnabled)
		{
			enemySpinnyAnim.SetSpawn();
		}
	}

	public void OnHurt()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && currentState == State.Leave)
		{
			UpdateState(State.Idle);
		}
		enemySpinnyAnim.HurtSound();
	}

	public void OnDeath()
	{
		enemySpinnyAnim.DeathSound();
		enemySpinnyAnim.deathJingleSound.Play(base.transform.position);
		enemySpinnyAnim.PlayDeathParticles();
		enemySpinnyAnim.animator.SetBool("close_mouth_bool", value: true);
		enemySpinnyAnim.animator.SetBool("open_mouth_bool", value: false);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
		}
	}

	public void OnVision()
	{
		if (currentState == State.GoToPlayer)
		{
			stateTimer = 4f;
		}
		else if (currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate)
		{
			playerTarget = enemy.Vision.onVisionTriggeredPlayer;
			if (SemiFunc.IsMultiplayer())
			{
				UpdatePlayerTarget(playerTarget.photonView.ViewID);
			}
			SemiFunc.EnemyCartJumpReset(enemy);
			UpdateState(State.PlayerNotice);
		}
	}

	public void OnInvestigate()
	{
		if (currentState == State.Roam || currentState == State.Idle || currentState == State.Investigate)
		{
			UpdateState(State.Investigate);
			agentDestination = enemy.StateInvestigate.onInvestigateTriggeredPosition;
		}
	}

	public void OnGrabbed()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !(grabAggroTimer > 0f) && currentState == State.Leave)
		{
			grabAggroTimer = 60f;
			playerTarget = enemy.Rigidbody.onGrabbedPlayerAvatar;
			if (SemiFunc.IsMultiplayer())
			{
				UpdatePlayerTarget(playerTarget.photonView.ViewID);
			}
			UpdateState(State.PlayerNotice);
		}
	}
}
