using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateRoaming : MonoBehaviour
{
	private Enemy Enemy;

	private PlayerController Player;

	private bool Active;

	[Header("Movement")]
	public float Speed;

	public float Acceleration;

	[Header("Roaming")]
	public float RoamingCooldownMin;

	public float RoamingCooldownMax;

	internal LevelPoint RoamingLevelPoint;

	private Vector3 RoamingTargetPosition;

	internal float RoamingCooldown;

	[Space]
	public float RoamingPathRadiusMin;

	public float RoamingPathRadiusMax;

	[Space]
	public int RoamingChangeMin;

	public int RoamingChangeMax;

	internal int RoamingChangeCurrent;

	private Vector3 RoamingStuckPosition;

	[Space]
	public float RoamingTeleportChance;

	[Space]
	public float RoamingOnScreenTime;

	private float RoamingOnScreenTimer;

	private float RoamingOnScreenCooldownTimer;

	public float RoamingOnScreenCooldown;

	private float RoamingTurnWaitTimer;

	public float RoamingTurnWaitTime;

	private PlayerAvatar RoamingTurnPlayer;

	[Header("Player Near")]
	public float PlayerNearTimeMax;

	public float PlayerNearDistance;

	public float PlayerNearDecrease;

	private float PlayerNearTimer;

	[Header("Player Far")]
	public float PlayerFarTimeMin;

	public float PlayerFarTimeMax;

	private float PlayerFarTime;

	public float PlayerFarDistance;

	private float PlayerFarTimer;

	private bool PlayerFarMove;

	[Header("Phys Object")]
	public int PhysObjectHitMax = 3;

	private bool PhysObjectHitImpulse;

	private int PhysObjectHitCount;

	private void Start()
	{
		Enemy = GetComponent<Enemy>();
		Player = PlayerController.instance;
	}

	private void Update()
	{
		if (GameDirector.instance.currentState < GameDirector.gameState.Main)
		{
			return;
		}
		if (Enemy.CurrentState != EnemyState.Roaming)
		{
			if (Active)
			{
				RoamingLevelPoint = null;
				RoamingCooldown = 0f;
				RoamingChangeCurrent = 0;
				Active = false;
			}
			return;
		}
		if (!Active)
		{
			PhysObjectHitImpulse = true;
			PhysObjectHitCount = 0;
			Active = true;
		}
		if (Enemy.MasterClient)
		{
			if (Enemy.HasRigidbody)
			{
				Enemy.Rigidbody.IdleSet(0.1f);
			}
			Enemy.NavMeshAgent.UpdateAgent(Speed, Acceleration);
			PlayerNear();
			PlayerFar();
			PlayerTurn();
			PickPath();
			Stuck();
		}
	}

	private void PlayerNear()
	{
		if (SemiFunc.EnemyForceLeave(Enemy))
		{
			PlayerFarTimer = 0f;
			PlayerFarTime = Random.Range(PlayerFarTimeMin, PlayerFarTimeMax);
			PlayerFarMove = true;
			RoamingChangeCurrent = 0;
			return;
		}
		if (Enemy.PlayerDistance.PlayerDistanceClosest <= PlayerNearDistance)
		{
			PlayerNearTimer += Time.deltaTime;
		}
		else
		{
			PlayerNearTimer -= PlayerNearDecrease * Time.deltaTime;
			PlayerNearTimer = Mathf.Max(PlayerNearTimer, 0f);
		}
		if (PlayerNearTimer >= PlayerNearTimeMax)
		{
			PlayerFarTimer = 0f;
			PlayerFarTime = Random.Range(PlayerFarTimeMin, PlayerFarTimeMax);
			PlayerFarMove = true;
			RoamingChangeCurrent = 0;
		}
	}

	private void PlayerFar()
	{
		if (PlayerFarMove)
		{
			PlayerFarTimer += Time.deltaTime;
			if (PlayerFarTimer >= PlayerFarTime)
			{
				PlayerFarMove = false;
				PlayerFarTimer = 0f;
			}
		}
	}

	private void PlayerTurn()
	{
		if (RoamingOnScreenCooldownTimer > 0f)
		{
			RoamingOnScreenCooldownTimer -= Time.deltaTime;
		}
		else if (RoamingTurnWaitTimer > 0f)
		{
			RoamingCooldown = 1f;
			RoamingTurnWaitTimer -= Time.deltaTime;
			if (RoamingTurnWaitTimer <= 0f)
			{
				RoamingChangeCurrent = Random.Range(RoamingChangeMin, RoamingChangeMax + 1);
				RoamingLevelPoint = Enemy.GetLevelPointAhead(RoamingTurnPlayer.transform.position);
				RoamingCooldown = 0f;
				RoamingOnScreenCooldownTimer = RoamingOnScreenCooldown;
			}
		}
		else if (Enemy.OnScreen.OnScreenAny)
		{
			RoamingOnScreenTimer += Time.deltaTime;
			if (!(RoamingOnScreenTimer >= RoamingOnScreenTime))
			{
				return;
			}
			if (GameManager.instance.gameMode == 1)
			{
				List<PlayerAvatar> list = new List<PlayerAvatar>();
				foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
				{
					if (!player.isDisabled && Enemy.OnScreen.OnScreenPlayer[player.photonView.ViewID])
					{
						list.Add(player);
					}
				}
				if (list.Count <= 0)
				{
					RoamingOnScreenTimer = 0f;
					return;
				}
				RoamingTurnPlayer = list[Random.Range(0, list.Count)];
			}
			else
			{
				RoamingTurnPlayer = PlayerController.instance.playerAvatarScript;
			}
			RoamingOnScreenTimer = 0f;
			RoamingTurnWaitTimer = RoamingTurnWaitTime;
			Enemy.NavMeshAgent.ResetPath();
			RoamingCooldown = 1f;
		}
		else
		{
			RoamingOnScreenTimer -= Time.deltaTime;
			RoamingOnScreenTimer = Mathf.Clamp01(RoamingOnScreenTimer);
		}
	}

	private void PickPath()
	{
		if (SemiFunc.EnemySpawnIdlePause() || Enemy.NavMeshAgent.HasPath())
		{
			return;
		}
		if (RoamingCooldown <= 0f || !RoamingLevelPoint)
		{
			LevelPoint levelPoint = RoamingLevelPoint;
			if (RoamingChangeCurrent <= 0 || !RoamingLevelPoint)
			{
				levelPoint = ((!PlayerFarMove) ? LevelGenerator.Instance.LevelPathPoints[Random.Range(0, LevelGenerator.Instance.LevelPathPoints.Count)] : SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f));
				RoamingChangeCurrent = Random.Range(RoamingChangeMin, RoamingChangeMax + 1);
			}
			else
			{
				RoamingChangeCurrent--;
			}
			if ((bool)levelPoint)
			{
				Vector3 position = levelPoint.transform.position;
				position += Random.insideUnitSphere * Random.Range(RoamingPathRadiusMin, RoamingPathRadiusMax);
				if (Enemy.NavMeshAgent.CalculatePath(position).status == NavMeshPathStatus.PathComplete)
				{
					RoamingCooldown = Random.Range(RoamingCooldownMin, RoamingCooldownMax);
					RoamingLevelPoint = levelPoint;
					RoamingTargetPosition = position;
					Enemy.NavMeshAgent.SetDestination(RoamingTargetPosition);
				}
			}
		}
		else
		{
			RoamingCooldown -= Time.deltaTime;
		}
	}

	private void Stuck()
	{
		if (!Enemy.NavMeshAgent.HasPath())
		{
			return;
		}
		Enemy.AttackStuckPhysObject.Check();
		if (Enemy.AttackStuckPhysObject.Active)
		{
			if (PhysObjectHitImpulse)
			{
				PhysObjectHitCount++;
				PhysObjectHitImpulse = false;
			}
		}
		else
		{
			PhysObjectHitImpulse = true;
		}
		if (PhysObjectHitCount >= PhysObjectHitMax)
		{
			PhysObjectHitImpulse = true;
			PhysObjectHitCount = 0;
			Enemy.NavMeshAgent.ResetPath();
			RoamingChangeCurrent = 1;
		}
	}
}
