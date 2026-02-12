using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class EnemySlowMouth : MonoBehaviour
{
	public enum State
	{
		Spawn,
		Idle,
		Despawn,
		Roam,
		Investigate,
		Stun,
		Leave,
		Notice,
		Attack,
		Attached,
		Puke,
		Detach,
		IdlePuke,
		GoToPlayerOver,
		GoToPlayerUnder,
		MoveBackToNavmesh,
		GoToPlayer,
		Dead
	}

	public GameObject enemySlowMouthAttack;

	public GameObject localCameraMouthPrefab;

	public GameObject enemySlowMouthOnPlayerTop;

	public GameObject enemySlowMouthOnPlayerBot;

	public Transform particles;

	public Transform tentacles;

	private List<SpringTentacle> springTentacles = new List<SpringTentacle>();

	private List<ParticleSystem> spawnParticles = new List<ParticleSystem>();

	private EnemySlowMouthCameraVisuals cameraVisuals;

	private bool movingRight;

	private bool movingLeft = true;

	private float moveThisDirectionTimer;

	private float looseTargetTimer;

	private float looseTargetTime;

	private float randomNudgeTimer;

	[FormerlySerializedAs("state")]
	public State currentState;

	private Vector3 agentDestination;

	public SemiPuke semiPuke;

	private EnemyVision enemyVision;

	public Transform mouthTransform;

	public Collider enemyCollider;

	public Transform enemyVisuals;

	private bool stateImpulse;

	private bool stateStartFixed;

	private float stateTimer;

	private PhotonView photonView;

	private PlayerAvatar attachTarget;

	private Enemy enemy;

	public EnemyRigidbody enemyRigidbody;

	public PhysGrabObject physGrabObject;

	public Transform followTarget;

	public Vector3 followTargetStartPosition;

	public Transform centerTransform;

	public AudioSource audioSourceVO;

	private float idleBreakerVOCooldown = 20f;

	private float idlePukeCooldown = 20f;

	private State idlePukePreviousState;

	public EnemySlowMouthAnim enemySlowMouthAnim;

	private Vector3 followPointPositionPrev;

	private Transform currentTarget;

	private SpringFloat spawnDespawnScaleSpring;

	private Vector3 targetDestination;

	private bool waitForTargettingLoop;

	private float visionTimer;

	private bool visionPrevious;

	private PlayerAvatar playerTarget;

	private float enemyHiddenTimer;

	private Vector3 moveBackPosition;

	private float targetForwardOffset = 1.5f;

	private Vector3 targetPosition;

	private float targetedPlayerTime;

	private float targetedPlayerTimeMax = 10f;

	private float moveBackTimer;

	private Vector3 enemyGroundPosition;

	internal Vector3 detachPosition;

	internal Quaternion detachRotation;

	private float attachedTimer;

	private float possessCooldown;

	private float stuckTimer;

	private Vector3 stuckPosition;

	private float aggroTimer;

	public Sound soundSpawnVO;

	public Sound soundIdleBreakerVO;

	public Sound soundHurtVO;

	public Sound soundDieVO;

	public Sound soundNoticeVO;

	public Sound soundChaseLoopVO;

	public Sound soundDetachVO;

	public Sound soundDetach;

	public Sound soundStunLoopVO;

	private void Start()
	{
		spawnDespawnScaleSpring = new SpringFloat();
		spawnDespawnScaleSpring.damping = 0.5f;
		spawnDespawnScaleSpring.speed = 20f;
		photonView = GetComponent<PhotonView>();
		enemy = GetComponent<Enemy>();
		followTargetStartPosition = followTarget.localPosition;
		enemyVision = GetComponent<EnemyVision>();
		spawnParticles = new List<ParticleSystem>(particles.GetComponentsInChildren<ParticleSystem>());
		springTentacles = new List<SpringTentacle>(tentacles.GetComponentsInChildren<SpringTentacle>());
	}

	private void AnimStateIdle()
	{
		enemySlowMouthAnim.UpdateState(EnemySlowMouthAnim.State.Idle);
	}

	private void AnimStatePuke()
	{
		enemySlowMouthAnim.UpdateState(EnemySlowMouthAnim.State.Puke);
	}

	private void AnimStateStunned()
	{
		enemySlowMouthAnim.UpdateState(EnemySlowMouthAnim.State.Stunned);
	}

	private void AnimStateTargetting()
	{
		enemySlowMouthAnim.UpdateState(EnemySlowMouthAnim.State.Targetting);
	}

	private void AnimStateAttached()
	{
		enemySlowMouthAnim.UpdateState(EnemySlowMouthAnim.State.Attached);
	}

	private void AnimStateAggro()
	{
		enemySlowMouthAnim.UpdateState(EnemySlowMouthAnim.State.Aggro);
	}

	private void AnimStateSpawnDespawn()
	{
		enemySlowMouthAnim.UpdateState(EnemySlowMouthAnim.State.SpawnDespawn);
	}

	private void AnimStateDeath()
	{
		enemySlowMouthAnim.UpdateState(EnemySlowMouthAnim.State.Death);
	}

	private void AnimStateLeave()
	{
		enemySlowMouthAnim.UpdateState(EnemySlowMouthAnim.State.Leave);
	}

	private void PlaySpawnParticles()
	{
		foreach (ParticleSystem spawnParticle in spawnParticles)
		{
			spawnParticle.Play();
		}
	}

	private void StateSpawn(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				enemyVisuals.localScale = Vector3.zero;
				stateTimer = 3f;
				stateImpulse = false;
				PlaySpawnParticles();
			}
			float num = SemiFunc.SpringFloatGet(spawnDespawnScaleSpring, 1f);
			enemyVisuals.localScale = Vector3.one * num;
			AnimStateSpawnDespawn();
			if (stateTimer <= 0f)
			{
				enemyVisuals.localScale = Vector3.one;
				UpdateState(State.Idle);
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateIdle(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				stateImpulse = false;
				enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
				enemy.NavMeshAgent.ResetPath();
				stateTimer = Random.Range(5f, 10f);
			}
			AnimStateIdle();
			if (SemiFunc.IsMasterClientOrSingleplayer() && !SemiFunc.EnemySpawnIdlePause() && !IdlePukeLogic(0.1f))
			{
				IdleBreakerVOLogic();
				LookAtVelocityDirection(_moving: false);
				FloatAround();
				if (stateTimer <= 0f)
				{
					UpdateState(State.Roam);
				}
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateDespawn(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				stateImpulse = false;
				PlaySpawnParticles();
				soundDetach.Play(centerTransform.position);
				stateTimer = 1f;
			}
			float num = SemiFunc.SpringFloatGet(spawnDespawnScaleSpring, 0f);
			enemyVisuals.localScale = Vector3.one * num;
			AnimStateSpawnDespawn();
			if (SemiFunc.IsMasterClientOrSingleplayer() && stateTimer <= 0f)
			{
				enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
				enemy.NavMeshAgent.ResetPath();
				enemy.EnemyParent.Despawn();
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateRoam(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				stateImpulse = false;
				agentDestination = SemiFunc.EnemyRoamFindPoint(base.transform.position);
				stateTimer = Random.Range(5f, 10f);
				followTarget.localPosition = new Vector3(0f, 1f, 0f);
			}
			AnimStateIdle();
			if (SemiFunc.IsMasterClientOrSingleplayer() && !IdlePukeLogic(0.1f))
			{
				IdleBreakerVOLogic();
				LookAtVelocityDirection(_moving: false);
				StuckLogic();
				enemy.NavMeshAgent.SetDestination(agentDestination);
				if (Vector3.Distance(base.transform.position, enemy.NavMeshAgent.GetPoint()) < 1f || stateTimer <= 0f)
				{
					UpdateState(State.Idle);
				}
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateInvestigate(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				stateImpulse = false;
				followTarget.localPosition = new Vector3(0f, 1f, 0f);
				enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			}
			AnimStateIdle();
			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				return;
			}
			LookAtVelocityDirection(_moving: false);
			if (!IdlePukeLogic(0.2f))
			{
				IdleBreakerVOLogic();
				enemy.NavMeshAgent.SetDestination(agentDestination);
				if (Vector3.Distance(base.transform.position, enemy.NavMeshAgent.GetPoint()) < 1f)
				{
					UpdateState(State.Idle);
				}
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateStun(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
				enemy.NavMeshAgent.ResetPath();
				stateImpulse = false;
			}
			AnimStateStunned();
			foreach (SpringTentacle springTentacle in springTentacles)
			{
				if (SemiFunc.FPSImpulse5())
				{
					springTentacle.springStart.springVelocity = Random.insideUnitSphere * 25f;
					springTentacle.springMid.springVelocity = Random.insideUnitSphere * 25f;
					springTentacle.springEnd.springVelocity = Random.insideUnitSphere * 25f;
				}
			}
			if (SemiFunc.IsMasterClientOrSingleplayer() && !enemy.IsStunned())
			{
				UpdateState(State.Idle);
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateLeave(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				if (!SemiFunc.EnemyLeavePoint(enemy, out agentDestination))
				{
					return;
				}
				stateImpulse = false;
				followTarget.localPosition = new Vector3(0f, 1f, 0f);
				stateTimer = Random.Range(10f, 15f);
				PlaySpawnParticles();
				SemiFunc.EnemyLeaveStart(enemy);
			}
			enemy.NavMeshAgent.SetDestination(agentDestination);
			AnimStateLeave();
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				StuckLogic();
				IdleBreakerVOLogic();
				FastMoving(_lookAtTarget: false);
				enemyRigidbody.OverrideFollowPosition(0.1f, 5f, 10f);
				enemy.NavMeshAgent.OverrideAgent(8f, 8f, 0.1f);
				if (stateTimer <= 0f || Vector3.Distance(base.transform.position, enemy.NavMeshAgent.GetPoint()) < 1f)
				{
					UpdateState(State.Idle);
				}
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateNotice(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				TargettingPlayerStart();
				if (!audioSourceVO.isPlaying)
				{
					soundNoticeVO.Play(centerTransform.position);
				}
				if (SemiFunc.IsMasterClientOrSingleplayer())
				{
					UpdatePlayerTarget(enemyVision.onVisionTriggeredPlayer);
				}
				stateTimer = 1f;
				stateImpulse = false;
			}
			AnimStateIdle();
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				followTarget.localRotation = Quaternion.LookRotation(currentTarget.position - centerTransform.position, Vector3.up);
				if (stateTimer <= 0f)
				{
					UpdateState(State.GoToPlayer);
				}
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateAttack(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				stateImpulse = false;
				EnemySlowMouthAttaching component = Object.Instantiate(enemySlowMouthAttack, centerTransform.position, centerTransform.rotation).GetComponent<EnemySlowMouthAttaching>();
				component.targetPlayerAvatar = playerTarget;
				component.enemySlowMouth = this;
				attachedTimer = 0f;
			}
			AnimStateAttached();
			OverrideHideEnemy();
			LookAtVelocityDirection(_moving: false);
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void DetatchLogic()
	{
		if (SemiFunc.FPSImpulse5())
		{
			if (!playerTarget)
			{
				UpdateState(State.Detach);
			}
			else if (playerTarget.isDisabled)
			{
				UpdateState(State.Detach);
			}
		}
	}

	private void StateAttached(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				stateImpulse = false;
				if (attachedTimer <= 0f)
				{
					attachedTimer = Random.Range(20f, 60f);
				}
				stateTimer = Random.Range(1f, 15f);
			}
			OverrideHideEnemy();
			AnimStateAttached();
			PlayerEffects();
			enemy.EnemyParent.SpawnedTimerPause(1f);
			enemy.transform.position = base.transform.position;
			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				return;
			}
			DetatchLogic();
			if (SemiFunc.FPSImpulse5())
			{
				bool num = playerTarget.playerAvatarVisuals.GetComponentInChildren<EnemySlowMouthPlayerAvatarAttached>();
				bool flag = playerTarget.localCamera.GetComponentInChildren<EnemySlowMouthCameraVisuals>();
				if (!num && !flag)
				{
					if ((bool)currentTarget)
					{
						detachPosition = currentTarget.position;
						detachRotation = currentTarget.rotation;
					}
					UpdateState(State.Detach);
				}
			}
			IsPossessedBySeveral();
			attachedTimer -= Time.deltaTime;
			if (attachedTimer <= 0f || playerTarget.isDisabled)
			{
				if ((bool)currentTarget)
				{
					detachPosition = currentTarget.position;
					detachRotation = currentTarget.rotation;
					UpdateState(State.Detach);
				}
				else
				{
					enemy.EnemyParent.SpawnedTimerSet(0f);
					UpdateState(State.Despawn);
				}
			}
			else if (stateTimer <= 0f)
			{
				UpdateState(State.Puke);
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StatePuke(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				stateImpulse = false;
				stateTimer = Random.Range(0.5f, 3f);
				if (Random.Range(0, 30) == 0)
				{
					stateTimer = 6f;
				}
			}
			PlayerEffects();
			OverrideHideEnemy();
			AnimStateAttached();
			DetatchLogic();
			if (stateTimer <= 0f)
			{
				UpdateState(State.Attached);
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateDetach(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				stateImpulse = false;
				stateTimer = 0f;
				attachedTimer = 0f;
				possessCooldown = Random.Range(30f, 120f);
				enemy.transform.position = base.transform.position;
			}
			PlayerEffects();
			OverrideHideEnemy();
			AnimStateAttached();
			if (!(stateTimer <= 0f))
			{
				return;
			}
			if (!playerTarget)
			{
				enemy.EnemyParent.SpawnedTimerSet(0f);
				UpdateState(State.Despawn);
				return;
			}
			Vector3 forward = playerTarget.localCamera.transform.forward;
			Vector3 origin = playerTarget.localCamera.transform.position + forward * 0.3f;
			float num = 0.2f;
			if (Physics.SphereCastAll(origin, 0.45f, forward, num, LayerMask.GetMask("Default")).Length == 0)
			{
				Vector3 position = playerTarget.localCamera.transform.position + forward * num;
				if ((bool)playerTarget && (bool)playerTarget.tumble)
				{
					playerTarget.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
				}
				physGrabObject.Teleport(position, playerTarget.localCamera.transform.rotation);
				enemy.NavMeshAgent.Warp(position);
				soundDetach.Play(position);
				soundDetachVO.Play(position);
				enemy.EnemyParent.SpawnedTimerSet(2f);
				UpdateState(State.Leave);
			}
			else if (playerTarget.isDisabled)
			{
				enemy.EnemyParent.SpawnedTimerSet(0f);
				UpdateState(State.Despawn);
			}
			else
			{
				stateTimer = 0.25f;
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateIdlePuke(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				stateImpulse = false;
				stateTimer = Random.Range(0.5f, 2f);
				if (Random.Range(0, 30) == 0)
				{
					stateTimer = 4f;
				}
			}
			AnimStatePuke();
			semiPuke.PukeActive(mouthTransform.position, mouthTransform.rotation);
			if (stateTimer <= 0f)
			{
				UpdateState(idlePukePreviousState);
			}
		}
		else
		{
			if (stateStartFixed)
			{
				stateStartFixed = false;
			}
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				Vector3 vector = Random.insideUnitSphere * 80f;
				enemyRigidbody.rb.AddTorque(vector * Time.fixedDeltaTime, ForceMode.Force);
				Vector3 vector2 = -mouthTransform.forward * 400f;
				enemyRigidbody.rb.AddForce(vector2 * Time.fixedDeltaTime, ForceMode.Force);
			}
		}
	}

	private void StateGoToPlayerOver(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				stateTimer = 2f;
				stateImpulse = false;
				followTarget.localPosition = Vector3.zero;
			}
			followTarget.localPosition = currentTarget.localPosition;
			AnimStateTargetting();
			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				return;
			}
			FastMoving(_lookAtTarget: true);
			TargettingPlayer();
			if (IdlePukeLogic())
			{
				return;
			}
			AttachToPlayer();
			enemy.NavMeshAgent.Disable(0.1f);
			base.transform.position = Vector3.MoveTowards(base.transform.position, targetPosition, enemy.NavMeshAgent.DefaultSpeed * 0.5f * Time.deltaTime);
			enemy.Vision.StandOverride(0.25f);
			if (playerTarget.PlayerVisionTarget.VisionTransform.position.y > enemy.Rigidbody.transform.position.y + 1.5f)
			{
				base.transform.position = enemy.Rigidbody.transform.position;
				base.transform.position = Vector3.MoveTowards(base.transform.position, targetPosition, 2f);
			}
			if (NavMesh.SamplePosition(targetPosition, out var _, 0.5f, -1))
			{
				UpdateState(State.MoveBackToNavmesh);
			}
			else if (VisionBlocked() || !playerTarget || playerTarget.isDisabled)
			{
				if (stateTimer <= 0f || enemy.Rigidbody.notMovingTimer > 1f)
				{
					UpdateState(State.MoveBackToNavmesh);
				}
			}
			else
			{
				stateTimer = 2f;
			}
			if (SemiFunc.EnemyForceLeave(enemy))
			{
				UpdateState(State.MoveBackToNavmesh);
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateGoToPlayerUnder(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				stateTimer = 2f;
				stateImpulse = false;
				followTarget.localPosition = Vector3.zero;
			}
			AnimStateTargetting();
			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				return;
			}
			FastMoving(_lookAtTarget: true);
			TargettingPlayer();
			AttachToPlayer();
			if (IdlePukeLogic())
			{
				return;
			}
			followTarget.localPosition = new Vector3(0f, 0.2f, 0f);
			enemy.NavMeshAgent.Disable(0.1f);
			base.transform.position = Vector3.MoveTowards(base.transform.position, targetPosition, enemy.NavMeshAgent.DefaultSpeed * Time.deltaTime);
			enemy.Vision.StandOverride(0.25f);
			if (NavMesh.SamplePosition(targetPosition, out var _, 0.5f, -1))
			{
				UpdateState(State.MoveBackToNavmesh);
			}
			else if (VisionBlocked() || !playerTarget || playerTarget.isDisabled)
			{
				if (stateTimer <= 0f)
				{
					UpdateState(State.MoveBackToNavmesh);
				}
			}
			else
			{
				stateTimer = 2f;
			}
			if (SemiFunc.EnemyForceLeave(enemy))
			{
				UpdateState(State.MoveBackToNavmesh);
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateMoveBackToNavmesh(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				stateImpulse = false;
				stateTimer = 30f;
				followTarget.localPosition = Vector3.zero;
			}
			AnimStateTargetting();
			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				return;
			}
			FastMoving(_lookAtTarget: false);
			TargettingPlayer();
			enemy.NavMeshAgent.OverrideAgent(8f, 8f, 0.1f);
			if (!IdlePukeLogic())
			{
				enemy.NavMeshAgent.Disable(0.1f);
				base.transform.position = Vector3.MoveTowards(base.transform.position, moveBackPosition, enemy.NavMeshAgent.DefaultSpeed * Time.deltaTime);
				enemy.Vision.StandOverride(0.25f);
				if (Vector3.Distance(base.transform.position, enemyGroundPosition) > 2f || enemy.Rigidbody.notMovingTimer > 2f)
				{
					Vector3 normalized = (moveBackPosition - enemyGroundPosition).normalized;
					base.transform.position = enemy.Rigidbody.transform.position;
					base.transform.position += normalized * 2f;
				}
				if (Vector3.Distance(enemyGroundPosition, moveBackPosition) <= 0f || NavMesh.SamplePosition(enemyGroundPosition, out var _, 0.5f, -1))
				{
					UpdateState(State.GoToPlayer);
				}
				else if (stateTimer <= 0f)
				{
					enemy.EnemyParent.SpawnedTimerSet(0f);
				}
			}
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateGoToPlayer(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				followTarget.localPosition = Vector3.zero;
				stateImpulse = false;
				stateTimer = 5f;
				targetedPlayerTime = 0f;
				targetedPlayerTimeMax = Random.Range(8f, 22f);
			}
			AnimStateTargetting();
			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				return;
			}
			FastMoving(_lookAtTarget: true);
			TargettingPlayer();
			enemy.NavMeshAgent.OverrideAgent(8f, 8f, 0.1f);
			if (IdlePukeLogic())
			{
				return;
			}
			AttachToPlayer();
			enemy.NavMeshAgent.SetDestination(targetPosition);
			MoveBackPosition();
			enemy.Vision.StandOverride(0.25f);
			if (!enemy.NavMeshAgent.CanReach(targetPosition, 1f) && Vector3.Distance(enemy.Rigidbody.transform.position, enemy.NavMeshAgent.GetPoint()) < 2f && !VisionBlocked() && !NavMesh.SamplePosition(targetPosition, out var _, 0.5f, -1))
			{
				if (playerTarget.isCrawling && Mathf.Abs(targetPosition.y - enemy.Rigidbody.transform.position.y) < 0.3f)
				{
					UpdateState(State.GoToPlayerUnder);
					return;
				}
				if (targetPosition.y > enemy.Rigidbody.transform.position.y)
				{
					UpdateState(State.GoToPlayerOver);
					return;
				}
			}
			LeaveCheck();
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateDead(bool fixedUpdate)
	{
		if (!fixedUpdate)
		{
			if (stateImpulse)
			{
				stateImpulse = false;
			}
			OverrideHideEnemy();
			AnimStateDeath();
		}
		else if (stateStartFixed)
		{
			stateStartFixed = false;
		}
	}

	private void StateMachine(bool fixedUpdate)
	{
		if (!fixedUpdate && stateTimer > 0f)
		{
			stateTimer -= Time.deltaTime;
		}
		if (fixedUpdate)
		{
			if (enemyHiddenTimer <= 0f && !enemyCollider.enabled)
			{
				PlaySpawnParticles();
				enemySlowMouthAnim.gameObject.SetActive(value: true);
				enemyCollider.enabled = true;
			}
			if (enemyHiddenTimer > 0f)
			{
				enemyHiddenTimer -= Time.fixedDeltaTime;
				if (enemyCollider.enabled)
				{
					PlaySpawnParticles();
					enemySlowMouthAnim.gameObject.SetActive(value: false);
					enemyCollider.enabled = false;
				}
			}
		}
		switch (currentState)
		{
		case State.Spawn:
			StateSpawn(fixedUpdate);
			break;
		case State.Idle:
			StateIdle(fixedUpdate);
			break;
		case State.Despawn:
			StateDespawn(fixedUpdate);
			break;
		case State.Roam:
			StateRoam(fixedUpdate);
			break;
		case State.Investigate:
			StateInvestigate(fixedUpdate);
			break;
		case State.Stun:
			StateStun(fixedUpdate);
			break;
		case State.Leave:
			StateLeave(fixedUpdate);
			break;
		case State.Notice:
			StateNotice(fixedUpdate);
			break;
		case State.Attack:
			StateAttack(fixedUpdate);
			break;
		case State.Attached:
			StateAttached(fixedUpdate);
			break;
		case State.Puke:
			StatePuke(fixedUpdate);
			break;
		case State.Detach:
			StateDetach(fixedUpdate);
			break;
		case State.IdlePuke:
			StateIdlePuke(fixedUpdate);
			break;
		case State.GoToPlayerOver:
			StateGoToPlayerOver(fixedUpdate);
			break;
		case State.GoToPlayerUnder:
			StateGoToPlayerUnder(fixedUpdate);
			break;
		case State.MoveBackToNavmesh:
			StateMoveBackToNavmesh(fixedUpdate);
			break;
		case State.GoToPlayer:
			StateGoToPlayer(fixedUpdate);
			break;
		}
	}

	public void UpdateState(State newState)
	{
		if (currentState != newState && SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdateStateRPC", RpcTarget.All, newState);
			}
			else
			{
				UpdateStateRPC(newState);
			}
		}
	}

	private void UpdatePlayerTarget(PlayerAvatar _player)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.All, _player.photonView.ViewID);
			}
			else
			{
				UpdatePlayerTargetRPC(_player.photonView.ViewID);
			}
		}
	}

	[PunRPC]
	private void UpdatePlayerTargetRPC(int _photonViewID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (item.photonView.ViewID == _photonViewID)
			{
				playerTarget = item;
				currentTarget = SemiFunc.PlayerGetFaceEyeTransform(item);
				break;
			}
		}
	}

	[PunRPC]
	public void UpdateStateRPC(State newState, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = newState;
			stateImpulse = true;
			stateStartFixed = true;
			stateTimer = 0f;
		}
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(enemy))
		{
			UpdateState(State.Spawn);
		}
	}

	public void OnHurt()
	{
		if (audioSourceVO.isActiveAndEnabled)
		{
			soundHurtVO.Play(centerTransform.position);
		}
	}

	public void OnVision()
	{
		if (currentState == State.Idle || currentState == State.Investigate || currentState == State.Roam || currentState == State.Leave)
		{
			UpdateState(State.Notice);
		}
	}

	public void OnInvestigate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && (currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate))
		{
			agentDestination = enemy.StateInvestigate.onInvestigateTriggeredPosition;
			UpdateState(State.Investigate);
		}
	}

	public void OnDeath()
	{
		soundDieVO.Play(centerTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.05f);
		PlaySpawnParticles();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemy.EnemyParent.Despawn();
			UpdateState(State.Dead);
		}
	}

	public void OnDespawn()
	{
		soundDieVO.Play(centerTransform.position);
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 10f, enemy.CenterTransform.position, 0.05f);
		PlaySpawnParticles();
	}

	private void Update()
	{
		if (currentState != State.Stun)
		{
			physGrabObject.OverrideZeroGravity();
		}
		if (enemy.CurrentState == EnemyState.Despawn)
		{
			UpdateState(State.Despawn);
		}
		LoopSounds();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (possessCooldown > 0f)
			{
				possessCooldown -= Time.deltaTime;
			}
			if (enemy.IsStunned() && enemyHiddenTimer <= 0f)
			{
				UpdateState(State.Stun);
			}
			enemyGroundPosition = new Vector3(enemy.Rigidbody.transform.position.x, base.transform.position.y, enemy.Rigidbody.transform.position.z);
			TargetPositionLogic();
		}
		StateMachine(fixedUpdate: false);
		if (SemiFunc.FPSImpulse30())
		{
			followPointPositionPrev = followTarget.position;
		}
	}

	private void FixedUpdate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			Vector3 forward = enemy.Rigidbody.transform.forward;
			if (Physics.Raycast(centerTransform.position, forward, out var hitInfo, 1f, SemiFunc.LayerMaskGetVisionObstruct()) && !hitInfo.collider.GetComponentInParent<PhysGrabHinge>())
			{
				enemyRigidbody.rb.AddForce(-(forward * 600f) * Time.fixedDeltaTime, ForceMode.Force);
			}
		}
		StateMachine(fixedUpdate: true);
	}

	private void RandomNudge(float _force)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			soundIdleBreakerVO.Play(centerTransform.position);
			Vector3 vector = centerTransform.up;
			switch (Random.Range(0, 4))
			{
			case 0:
				vector = centerTransform.up;
				break;
			case 1:
				vector = centerTransform.right;
				break;
			case 2:
				vector = -centerTransform.right;
				break;
			case 3:
				vector = -centerTransform.up;
				break;
			}
			_ = centerTransform.position + vector * 0.5f;
			randomNudgeTimer = 0f;
			enemyRigidbody.rb.AddForce(vector * _force, ForceMode.Impulse);
		}
	}

	private void LookAtVelocityDirection(bool _moving)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			Vector3 forward = enemyRigidbody.rb.velocity.normalized;
			if (_moving)
			{
				forward = enemy.moveDirection;
			}
			if (forward.magnitude > 0.001f)
			{
				Quaternion quaternion = Quaternion.LookRotation(forward, Vector3.up);
				followTarget.localRotation = Quaternion.Slerp(followTarget.localRotation, quaternion, Time.deltaTime * 2f);
			}
		}
	}

	private void FloatAround()
	{
		followTarget.localPosition = followTargetStartPosition + Vector3.up * Mathf.Sin(Time.time * 0.5f) * 0.5f;
		followTarget.localPosition += Vector3.left * Mathf.Sin(Time.time * 0.2f) * 0.3f;
		followTarget.localPosition += Vector3.forward * Mathf.Sin(Time.time * 0.2f) * 2f;
	}

	private bool IdlePukeLogic(float _tickSpeed = 1f)
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return false;
		}
		if (idlePukeCooldown > 0f)
		{
			idlePukeCooldown -= Time.deltaTime * _tickSpeed;
			return false;
		}
		IdlePukeExecute();
		return true;
	}

	private void IdlePukeExecute()
	{
		if (!audioSourceVO.isPlaying)
		{
			idlePukePreviousState = currentState;
			UpdateState(State.IdlePuke);
			idlePukeCooldown = Random.Range(5f, 10f);
			float force = Random.Range(5f, 20f);
			RandomNudge(force);
		}
	}

	private void IdleBreakerVOLogic()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (idleBreakerVOCooldown > 0f)
			{
				idleBreakerVOCooldown -= Time.deltaTime;
			}
			else
			{
				IdleBreakerVO();
			}
		}
	}

	public void IdleBreakerVO()
	{
		if (!audioSourceVO.isPlaying && SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("IdleBreakerVORPC", RpcTarget.All);
			}
			else
			{
				IdleBreakerVORPC();
			}
			idleBreakerVOCooldown = Random.Range(15f, 45f);
		}
	}

	[PunRPC]
	public void IdleBreakerVORPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info) && enemySlowMouthAnim.enabled && audioSourceVO.enabled)
		{
			soundIdleBreakerVO.Play(centerTransform.position);
		}
	}

	private void AttachToTarget()
	{
		float num = Vector3.Distance(base.transform.position, targetDestination);
		if (!VisionBlocked() && num < 2f)
		{
			UpdateState(State.Attached);
		}
	}

	private void FastMoving(bool _lookAtTarget)
	{
		if (_lookAtTarget)
		{
			followTarget.localRotation = Quaternion.LookRotation(currentTarget.position - centerTransform.position, Vector3.up);
		}
		else
		{
			LookAtVelocityDirection(_moving: false);
		}
		enemyRigidbody.OverrideFollowPosition(0.1f, 4f, 4f);
	}

	private void OverrideHideEnemy()
	{
		enemyHiddenTimer = 0.2f;
	}

	private bool VisionBlocked()
	{
		if (SemiFunc.FPSImpulse5())
		{
			Vector3 direction = playerTarget.PlayerVisionTarget.VisionTransform.position - enemy.CenterTransform.position;
			visionPrevious = Physics.Raycast(enemy.CenterTransform.position, direction, direction.magnitude, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
		}
		return visionPrevious;
	}

	private void LeaveCheck()
	{
		if (SemiFunc.EnemyForceLeave(enemy) || targetedPlayerTime >= targetedPlayerTimeMax)
		{
			UpdateState(State.Leave);
		}
	}

	private void TargettingPlayerStart()
	{
		aggroTimer = Random.Range(8f, 15f);
		looseTargetTimer = 0f;
	}

	private void TargettingPlayer()
	{
		StuckLogic();
		float num = currentTarget.position.y - base.transform.position.y;
		if (num < 0.1f)
		{
			num = 0.1f;
		}
		if (num > 2f)
		{
			num = 2f;
		}
		followTarget.localPosition = new Vector3(0f, num, 0f);
		targetedPlayerTime += Time.deltaTime;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			float num2 = 1f;
			if (VisionBlocked())
			{
				num2 = 4f;
			}
			aggroTimer -= Time.deltaTime * num2;
			if (((bool)playerTarget && playerTarget.isDisabled) || !playerTarget || aggroTimer <= 0f)
			{
				UpdateState(State.Leave);
			}
			if (VisionBlocked())
			{
				looseTargetTimer += Time.deltaTime;
				if (looseTargetTimer > 3f)
				{
					UpdateState(State.Leave);
					looseTargetTimer = 0f;
				}
			}
			else
			{
				looseTargetTimer = 0f;
			}
		}
		AudioSourceSmoothStop();
	}

	public void TargetPositionLogic()
	{
		if ((currentState == State.GoToPlayer || currentState == State.GoToPlayerOver || currentState == State.GoToPlayerUnder) && (bool)playerTarget)
		{
			targetPosition = Vector3.Lerp(b: (currentState != State.GoToPlayer && currentState != State.GoToPlayerUnder && currentState != State.GoToPlayerOver) ? (playerTarget.transform.position + playerTarget.transform.forward * targetForwardOffset) : (playerTarget.transform.position + playerTarget.transform.forward * 1.5f), a: targetPosition, t: 20f * Time.deltaTime);
		}
	}

	private void AudioSourceSmoothStop()
	{
		if (audioSourceVO.isPlaying)
		{
			audioSourceVO.volume = Mathf.Lerp(audioSourceVO.volume, 0f, Time.deltaTime * 40f);
			if (audioSourceVO.volume <= 0.01f)
			{
				audioSourceVO.Stop();
				audioSourceVO.volume = 1f;
			}
		}
		else if (audioSourceVO.volume < 1f)
		{
			audioSourceVO.volume = 1f;
		}
	}

	private void LoopSounds()
	{
		bool playing = currentState == State.GoToPlayer || currentState == State.GoToPlayerOver || currentState == State.GoToPlayerUnder || currentState == State.MoveBackToNavmesh;
		soundChaseLoopVO.PlayLoop(playing, 2f, 2f);
		bool playing2 = currentState == State.Stun;
		soundStunLoopVO.PlayLoop(playing2, 2f, 2f);
	}

	private void AttachToPlayer()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || !playerTarget || !currentTarget || !(Vector3.Distance(centerTransform.position, currentTarget.position) < 1.8f))
		{
			return;
		}
		if (SemiFunc.FPSImpulse30())
		{
			if (movingLeft)
			{
				enemyRigidbody.rb.AddForce(-centerTransform.right * 5f, ForceMode.Force);
				if (moveThisDirectionTimer <= 0f)
				{
					movingLeft = false;
					movingRight = true;
					moveThisDirectionTimer = Random.Range(0.2f, 3f);
				}
			}
			if (movingRight)
			{
				enemyRigidbody.rb.AddForce(centerTransform.right * 5f, ForceMode.Force);
				if (moveThisDirectionTimer <= 0f)
				{
					movingRight = false;
					movingLeft = true;
					moveThisDirectionTimer = Random.Range(0.2f, 3f);
				}
			}
		}
		if (moveThisDirectionTimer > 0f)
		{
			moveThisDirectionTimer -= Time.deltaTime;
		}
		if (Random.Range(0, 2) == 0 && possessCooldown <= 0f && !IsPossessed())
		{
			UpdateState(State.Attack);
			return;
		}
		IdlePukeExecute();
		if (Random.Range(0, 3) == 0)
		{
			idlePukePreviousState = State.Leave;
		}
	}

	public bool IsPossessed()
	{
		if (!playerTarget)
		{
			return true;
		}
		bool num = playerTarget.playerAvatarVisuals.GetComponentInChildren<EnemySlowMouthPlayerAvatarAttached>();
		bool flag = playerTarget.localCamera.transform.GetComponentInChildren<EnemySlowMouthCameraVisuals>();
		if (num || flag)
		{
			return true;
		}
		return false;
	}

	private void PlayerEffects()
	{
		if ((bool)playerTarget)
		{
			if ((bool)playerTarget.voiceChat)
			{
				playerTarget.voiceChat.OverridePitch(0.75f, 1f, 2f);
			}
			playerTarget.OverridePupilSize(2f, 4, 1f, 1f, 5f, 0.5f);
		}
	}

	private void StuckLogic()
	{
		float num = Vector3.Distance(base.transform.position, stuckPosition);
		float num2 = 0.25f;
		if (currentState == State.GoToPlayer || currentState == State.GoToPlayerOver || currentState == State.GoToPlayerUnder)
		{
			num2 = 1.5f;
		}
		if (num > num2)
		{
			stuckPosition = base.transform.position;
			stuckTimer = 0f;
			randomNudgeTimer = 0f;
			return;
		}
		stuckTimer += Time.deltaTime;
		randomNudgeTimer += Time.deltaTime;
		if (randomNudgeTimer > 2.5f)
		{
			float force = Random.Range(4f, 10f);
			RandomNudge(force);
			randomNudgeTimer = 0f;
			if (currentState == State.GoToPlayer)
			{
				if (Random.Range(0, 2) == 0)
				{
					UpdateState(State.GoToPlayerUnder);
				}
				else
				{
					UpdateState(State.GoToPlayerOver);
				}
			}
		}
		if (stuckTimer > 5f)
		{
			UpdateState(State.IdlePuke);
			stuckTimer = 0f;
		}
	}

	private void IsPossessedBySeveral()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || !SemiFunc.FPSImpulse5() || !playerTarget)
		{
			return;
		}
		if (playerTarget.isDisabled)
		{
			UpdateState(State.Leave);
		}
		else if (playerTarget.isLocal)
		{
			if (playerTarget.localCamera.transform.GetComponentsInChildren<EnemySlowMouthCameraVisuals>().Length > 1)
			{
				UpdateState(State.Leave);
			}
		}
		else if (playerTarget.playerAvatarVisuals.GetComponentsInChildren<EnemySlowMouthPlayerAvatarAttached>().Length > 1)
		{
			UpdateState(State.Leave);
		}
	}

	private void MoveBackPosition()
	{
		if (moveBackTimer <= 0f)
		{
			moveBackTimer = 0.1f;
			if (NavMesh.SamplePosition(base.transform.position, out var hit, 0.5f, -1) && Physics.Raycast(base.transform.position, Vector3.down, 2f, LayerMask.GetMask("Default")))
			{
				moveBackPosition = hit.position;
			}
		}
	}
}
