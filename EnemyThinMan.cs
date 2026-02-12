using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class EnemyThinMan : MonoBehaviour
{
	public enum State
	{
		Stand,
		OnScreen,
		Notice,
		Attack,
		TentacleExtend,
		Damage,
		Despawn,
		Stunned
	}

	private PhotonView photonView;

	private Enemy enemy;

	public EnemyThinManAnim anim;

	public GameObject tentacleR1;

	public GameObject tentacleR2;

	public GameObject tentacleR3;

	public GameObject tentacleL1;

	public GameObject tentacleL2;

	public GameObject tentacleL3;

	public GameObject extendedTentacles;

	public GameObject head;

	public GameObject hurtCollider;

	private float hurtColliderTimer;

	public float tentacleLerp;

	public State currentState;

	private float stateTimer;

	private bool stateImpulse;

	private float tpTimer;

	public Rigidbody rb;

	internal PlayerAvatar playerTarget;

	private bool otherEnemyFetch = true;

	public List<EnemyThinMan> otherEnemies;

	private Vector3 teleportPosition;

	private Vector3 lastTeleportPosition;

	private float teleportTimer;

	private bool teleporting;

	private float teleportRoamTimer;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
	}

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			switch (currentState)
			{
			case State.Stand:
				StateStand();
				PlayerLookAt();
				break;
			case State.OnScreen:
				StateOnScreen();
				PlayerLookAt();
				break;
			case State.Notice:
				StateNotice();
				PlayerLookAt();
				break;
			case State.Attack:
				StateAttack();
				PlayerLookAt();
				break;
			case State.TentacleExtend:
				StateTentacleExtend();
				PlayerLookAt();
				break;
			case State.Damage:
				StateDamage();
				PlayerLookAt();
				break;
			case State.Despawn:
				StateDespawn();
				break;
			case State.Stunned:
				StateStunned();
				break;
			}
			if (enemy.IsStunned())
			{
				enemy.EnemyParent.SpawnedTimerSet(0f);
				UpdateState(State.Stunned);
			}
		}
		TeleportLogic();
		SetFollowTargetToPosition();
		TentacleLogic();
		LocalEffect();
		HurtColliderLogic();
	}

	private void StateStand()
	{
		if (stateImpulse)
		{
			SetTarget(null);
			stateTimer = 5f;
			stateImpulse = false;
		}
		if (!playerTarget && !EnemyDirector.instance.debugNoVision)
		{
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (!player.isDisabled && enemy.OnScreen.GetOnScreen(player))
				{
					SetTarget(player);
					UpdateState(State.OnScreen);
					return;
				}
			}
		}
		if (!SemiFunc.EnemySpawnIdlePause())
		{
			if (teleportRoamTimer > 0f)
			{
				teleportRoamTimer -= Time.deltaTime;
			}
			else if (Teleport(_spawn: false))
			{
				SetRoamTimer();
			}
			if (SemiFunc.EnemyForceLeave(enemy))
			{
				Teleport(_spawn: false, _leave: true);
			}
		}
	}

	private void StateOnScreen()
	{
		if (stateImpulse)
		{
			tpTimer = Random.Range(0f, 5f);
			stateTimer = 1f;
			stateImpulse = false;
		}
		bool flag = false;
		if (enemy.OnScreen.GetOnScreen(playerTarget))
		{
			stateTimer = 0.2f;
			flag = true;
		}
		if (tpTimer > 0f)
		{
			tpTimer -= Time.deltaTime;
		}
		if (flag)
		{
			if (!(tentacleLerp < 1f))
			{
				UpdateState(State.Notice);
				return;
			}
			if (tentacleLerp > 0.05f && tentacleLerp < 0.15f && tpTimer <= 0f)
			{
				if (Random.Range(0f, 1f) < 0.5f)
				{
					if (Teleport(_spawn: false))
					{
						tpTimer = 5f;
					}
				}
				else
				{
					tpTimer = 5f;
				}
			}
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Stand);
		}
	}

	private void StateNotice()
	{
		if (!GameManager.Multiplayer())
		{
			NoticeRPC();
		}
		else
		{
			photonView.RPC("NoticeRPC", RpcTarget.All);
		}
		UpdateState(State.Attack);
	}

	private void StateAttack()
	{
		if (stateImpulse)
		{
			if (!playerTarget)
			{
				UpdateState(State.Despawn);
			}
			stateTimer = 1f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.TentacleExtend);
		}
	}

	private void StateTentacleExtend()
	{
		if (stateImpulse)
		{
			if (!playerTarget)
			{
				UpdateState(State.Despawn);
			}
			stateTimer = 0.1f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			UpdateState(State.Damage);
		}
	}

	private void StateDamage()
	{
		if (stateImpulse)
		{
			if (!playerTarget)
			{
				UpdateState(State.Despawn);
			}
			stateImpulse = false;
		}
		playerTarget.playerHealth.HurtOther(50, playerTarget.transform.position, savingGrace: false, SemiFunc.EnemyGetIndex(enemy));
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("ActivateHurtColliderRPC", RpcTarget.All, playerTarget.transform.position);
		}
		else
		{
			ActivateHurtColliderRPC(playerTarget.transform.position);
		}
		UpdateState(State.Despawn);
	}

	private void StateDespawn()
	{
		if (stateImpulse)
		{
			stateTimer = 0.4f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			enemy.EnemyParent.SpawnedTimerSet(0f);
		}
	}

	private void StateStunned()
	{
	}

	public void OnSpawn()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (otherEnemyFetch)
		{
			otherEnemyFetch = false;
			foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
			{
				EnemyThinMan enemyThinMan = item?.GetComponentInChildren<EnemyThinMan>(includeInactive: true);
				if ((bool)enemyThinMan && enemyThinMan != this)
				{
					otherEnemies.Add(enemyThinMan);
				}
			}
		}
		if (SemiFunc.EnemySpawnIdlePause())
		{
			lastTeleportPosition = base.transform.position;
			SemiFunc.EnemySpawn(enemy);
			teleportPosition = base.transform.position;
			teleporting = true;
			UpdateState(State.Stand);
		}
		else if (Teleport(_spawn: true))
		{
			SetRoamTimer();
			UpdateState(State.Stand);
		}
		else
		{
			enemy.EnemyParent.Despawn();
			enemy.EnemyParent.DespawnedTimerSet(3f);
		}
	}

	public void OnHurt()
	{
		anim.hurtSound.Play(anim.transform.position);
	}

	private void UpdateState(State _nextState)
	{
		stateTimer = 0f;
		stateImpulse = true;
		currentState = _nextState;
		if (GameManager.Multiplayer())
		{
			photonView.RPC("UpdateStateRPC", RpcTarget.Others, _nextState);
		}
	}

	private bool Teleport(bool _spawn, bool _leave = false)
	{
		List<LevelPoint> list = new List<LevelPoint>();
		if (_leave)
		{
			list.Add(SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f));
		}
		else
		{
			list = SemiFunc.LevelPointGetWithinDistance(base.transform.position, 3f, 30f);
			if (list == null)
			{
				list = SemiFunc.LevelPointGetWithinDistance(base.transform.position, 3f, 50f);
				if (list == null)
				{
					list = SemiFunc.LevelPointGetWithinDistance(base.transform.position, 0f, 999f);
				}
			}
		}
		if (list == null)
		{
			return false;
		}
		bool flag = Random.Range(0, 100) < 3;
		if ((bool)playerTarget)
		{
			flag = Random.Range(0, 100) < 30;
		}
		if (flag && !_leave)
		{
			list = SemiFunc.LevelPointsGetAllCloseToPlayers();
		}
		if (list == null || list.Count <= 0)
		{
			return false;
		}
		LevelPoint levelPoint = list[Random.Range(0, list.Count)];
		if (levelPoint == null)
		{
			return false;
		}
		if (Physics.Raycast(levelPoint.transform.position + Vector3.up * 0.1f, Vector3.up, out var _, 3.5f, LayerMask.GetMask("Default")))
		{
			return false;
		}
		foreach (EnemyThinMan otherEnemy in otherEnemies)
		{
			if ((bool)otherEnemy && otherEnemy.isActiveAndEnabled && Vector3.Distance(otherEnemy.rb.position, levelPoint.transform.position) <= 2f)
			{
				return false;
			}
		}
		if (Vector3.Distance(levelPoint.transform.position, lastTeleportPosition) < 1f)
		{
			return false;
		}
		if (SemiFunc.EnemyPhysObjectBoundingBoxCheck(base.transform.position, levelPoint.transform.position, enemy.Rigidbody.rb))
		{
			return false;
		}
		lastTeleportPosition = base.transform.position;
		teleportPosition = levelPoint.transform.position;
		teleporting = true;
		if (!_spawn)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("TeleportEffectRPC", RpcTarget.All, lastTeleportPosition, true);
			}
			else
			{
				TeleportEffectRPC(lastTeleportPosition, intro: true);
			}
		}
		else
		{
			enemy.EnemyTeleported(teleportPosition);
		}
		return true;
	}

	private void TentacleLogic()
	{
		if (currentState == State.OnScreen)
		{
			tentacleLerp += anim.tentacleSpeed * Time.deltaTime;
		}
		else if (currentState == State.Attack || currentState == State.TentacleExtend)
		{
			if (currentState == State.TentacleExtend)
			{
				tentacleLerp -= 10f * Time.deltaTime;
			}
		}
		else if (currentState == State.Stunned)
		{
			tentacleLerp -= 0.4f * Time.deltaTime;
		}
		else
		{
			tentacleLerp -= anim.tentacleSpeed * 0.5f * Time.deltaTime;
		}
		tentacleLerp = Mathf.Clamp(tentacleLerp, 0f, 1f);
	}

	private void TeleportLogic()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (teleportTimer <= 0f)
		{
			if (teleporting)
			{
				enemy.EnemyTeleported(teleportPosition);
				if (SemiFunc.IsMultiplayer())
				{
					photonView.RPC("TeleportEffectRPC", RpcTarget.All, teleportPosition, false);
				}
				else
				{
					TeleportEffectRPC(teleportPosition, intro: false);
				}
				teleporting = false;
			}
		}
		else
		{
			teleportTimer -= Time.deltaTime;
		}
	}

	private void PlayerLookAt()
	{
		if ((bool)playerTarget)
		{
			Quaternion quaternion = Quaternion.Euler(0f, Quaternion.LookRotation(playerTarget.PlayerVisionTarget.VisionTransform.position - enemy.Rigidbody.transform.position).eulerAngles.y, 0f);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, 50f * Time.deltaTime);
		}
	}

	private void SetFollowTargetToPosition()
	{
		enemy.transform.position = teleportPosition;
	}

	public void SmokeEffect(Vector3 pos)
	{
		anim.particleSmokeCalmFill.Play();
	}

	private void Rattle()
	{
		anim.notice.Play(base.transform.position);
		anim.rattleImpulse = true;
	}

	private void LocalEffect()
	{
		if (currentState == State.OnScreen && (bool)playerTarget && playerTarget.isLocal)
		{
			SemiFunc.DoNotLookEffect(base.gameObject, _vignette: true, _zoom: true, _saturation: true, _contrast: true, _shake: true, _glitch: false);
		}
	}

	private void SetRoamTimer()
	{
		teleportRoamTimer = Random.Range(8f, 22f);
	}

	private void HurtColliderLogic()
	{
		if (hurtColliderTimer > 0f)
		{
			hurtColliderTimer -= Time.deltaTime;
			if (hurtColliderTimer <= 0f)
			{
				hurtCollider.SetActive(value: false);
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

	[PunRPC]
	private void NoticeRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			anim.NoticeSet();
		}
	}

	[PunRPC]
	public void TeleportEffectRPC(Vector3 position, bool intro, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			SmokeEffect(position);
			if (intro)
			{
				anim.teleportIn.Play(base.transform.position);
			}
			else
			{
				anim.teleportOut.Play(base.transform.position);
			}
			anim.rattleImpulse = true;
			teleportTimer = 0.1f;
		}
	}

	private void SetTarget(PlayerAvatar _player)
	{
		if (!(_player == playerTarget))
		{
			playerTarget = _player;
			bool flag = true;
			int num = -1;
			if (!playerTarget)
			{
				flag = false;
			}
			if (flag)
			{
				Rattle();
				num = playerTarget.photonView.ViewID;
			}
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("SetTargetRPC", RpcTarget.Others, num, flag);
			}
		}
	}

	[PunRPC]
	public void SetTargetRPC(int playerID, bool hasTarget, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		if (!hasTarget)
		{
			playerTarget = null;
			return;
		}
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (item.photonView.ViewID == playerID)
			{
				playerTarget = item;
				break;
			}
		}
		Rattle();
	}

	[PunRPC]
	public void ActivateHurtColliderRPC(Vector3 _position, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			hurtCollider.transform.position = _position;
			hurtCollider.transform.rotation = Quaternion.LookRotation(enemy.Vision.VisionTransform.position - _position);
			hurtCollider.SetActive(value: true);
			hurtColliderTimer = 0.25f;
		}
	}
}
