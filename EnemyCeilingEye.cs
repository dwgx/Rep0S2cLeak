using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class EnemyCeilingEye : MonoBehaviour
{
	public enum State
	{
		Idle,
		Move,
		TargetLost,
		HasTarget,
		Spawn,
		Despawn,
		Death
	}

	public CeilingEyeLine eyeBeamLeft;

	public CeilingEyeLine eyeBeamRight;

	public ParticleSystem eyeBeamParticles;

	[Header("References")]
	public Transform eyeTransform;

	public EnemyCeilingEyeAnim eyeAnim;

	internal Enemy enemy;

	private bool otherEnemyFetch = true;

	public List<EnemyCeilingEye> otherEnemies;

	public State currentState;

	private bool stateImpulse;

	private float stateTimer;

	internal PlayerAvatar targetPlayer;

	private PhotonView photonView;

	internal bool deathImpulse;

	private float eyeDamageTimer;

	private float eyeDamageWaitTimer;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
	}

	private void Update()
	{
		RotationAnimation();
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			TargetFailSafe();
			switch (currentState)
			{
			case State.Idle:
				StateIdle();
				break;
			case State.Move:
				StateMove();
				break;
			case State.HasTarget:
				StateHasTarget();
				break;
			case State.TargetLost:
				StateTargetLost();
				break;
			case State.Spawn:
				StateSpawn();
				break;
			case State.Despawn:
				StateDespawn();
				break;
			}
		}
		if (currentState == State.HasTarget && (bool)targetPlayer)
		{
			SemiFunc.PlayerEyesOverride(targetPlayer, enemy.CenterTransform.position, 0.1f, base.gameObject);
			PlayerAvatar playerAvatar = targetPlayer;
			if ((bool)playerAvatar.voiceChat)
			{
				playerAvatar.voiceChat.OverridePitch(1.25f, 1f, 2f);
			}
			playerAvatar.OverridePupilSize(2f, 5, 5f, 0.5f, 5f, 0.5f);
			playerAvatar.playerHealth.EyeMaterialOverride(PlayerHealth.EyeOverrideState.CeilingEye, 0.25f, 0);
			if (targetPlayer.isLocal)
			{
				Vector3 vector = targetPlayer.transform.position - enemy.CenterTransform.position;
				float num = Vector3.Dot(Vector3.down, vector.normalized);
				float strengthNoAim = 10f;
				if (num > 0.9f)
				{
					strengthNoAim = 5f;
				}
				CameraAim.Instance.AimTargetSoftSet(enemy.CenterTransform.position, 0.1f, 2f, strengthNoAim, base.gameObject, 100);
				PostProcessing.Instance.VignetteOverride(Color.black, 0.5f, 1f, 1f, 0.5f, 0.1f, base.gameObject);
				CameraZoom.Instance.OverrideZoomSet(40f, 0.1f, 1f, 1f, base.gameObject, 50);
			}
			else
			{
				eyeBeamLeft.outro = false;
				eyeBeamRight.outro = false;
				eyeBeamLeft.lineTarget = targetPlayer.playerAvatarVisuals.playerEyes.pupilLeft;
				eyeBeamRight.lineTarget = targetPlayer.playerAvatarVisuals.playerEyes.pupilRight;
			}
		}
		else
		{
			eyeBeamLeft.outro = true;
			eyeBeamRight.outro = true;
		}
	}

	private void StateIdle()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = Random.Range(20f, 60f);
		}
		if (!SemiFunc.EnemySpawnIdlePause())
		{
			if (!enemy.EnemyParent.playerClose)
			{
				stateTimer -= Time.deltaTime;
			}
			if (stateTimer <= 0f)
			{
				UpdateState(State.Move);
			}
		}
	}

	private void StateMove()
	{
	}

	private void StateHasTarget()
	{
		if (stateImpulse)
		{
			eyeDamageWaitTimer = 3f;
			stateImpulse = false;
		}
		if (eyeDamageWaitTimer <= 0f)
		{
			eyeDamageTimer -= Time.deltaTime;
			if (eyeDamageTimer <= 0f)
			{
				targetPlayer.playerHealth.HurtOther(2, targetPlayer.transform.position, savingGrace: false, SemiFunc.EnemyGetIndex(enemy));
				eyeDamageTimer = 1f;
			}
		}
		else
		{
			if (targetPlayer.isDisabled)
			{
				UpdateState(State.TargetLost);
				return;
			}
			eyeDamageWaitTimer -= Time.deltaTime;
		}
		stateTimer -= Time.deltaTime;
		if (SemiFunc.PlayerVisionCheck(enemy.Vision.VisionTransform.position, 20f, targetPlayer, _previouslySeen: true) || SemiFunc.PlayerVisionCheck(enemy.Vision.VisionTransform.position + base.transform.right * 0.25f, 20f, targetPlayer, _previouslySeen: true) || SemiFunc.PlayerVisionCheck(enemy.Vision.VisionTransform.position - base.transform.right * 0.25f, 20f, targetPlayer, _previouslySeen: true) || SemiFunc.PlayerVisionCheck(enemy.Vision.VisionTransform.position + base.transform.up * 0.25f, 20f, targetPlayer, _previouslySeen: true) || SemiFunc.PlayerVisionCheck(enemy.Vision.VisionTransform.position - base.transform.up * 0.25f, 20f, targetPlayer, _previouslySeen: true))
		{
			stateTimer = 0.5f;
		}
		if (stateTimer <= 0f)
		{
			UpdateState(State.TargetLost);
		}
	}

	private void StateTargetLost()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 3f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			enemy.EnemyParent.SpawnedTimerSet(0f);
			UpdateState(State.Despawn);
		}
	}

	private void StateSpawn()
	{
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			if (stateImpulse)
			{
				stateTimer = 5f;
				stateImpulse = false;
			}
			stateTimer -= Time.deltaTime;
			if (stateTimer <= 0f)
			{
				UpdateState(State.Idle);
			}
		}
	}

	private void StateDespawn()
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
				EnemyCeilingEye enemyCeilingEye = item?.GetComponentInChildren<EnemyCeilingEye>(includeInactive: true);
				if ((bool)enemyCeilingEye && enemyCeilingEye != this)
				{
					otherEnemies.Add(enemyCeilingEye);
				}
			}
		}
		if (!SemiFunc.EnemySpawn(enemy))
		{
			return;
		}
		if (Physics.SphereCast(base.transform.position + Vector3.up * 0.1f + new Vector3(0f, 0.5f, 0f), 0.1f, Vector3.up, out var hitInfo, 30f, LayerMask.GetMask("Default")))
		{
			foreach (EnemyCeilingEye otherEnemy in otherEnemies)
			{
				if (otherEnemy.isActiveAndEnabled && Vector3.Distance(otherEnemy.transform.position, hitInfo.point) <= 2f)
				{
					enemy.StateDespawn.Despawn();
					enemy.EnemyParent.DespawnedTimerSet(Random.Range(5f, 10f), _min: true);
					return;
				}
			}
			base.transform.position = hitInfo.point;
			base.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdatePositionRPC", RpcTarget.All, base.transform.position, base.transform.rotation);
			}
		}
		UpdateState(State.Spawn);
	}

	public void OnDeath()
	{
		deathImpulse = true;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			UpdateState(State.Death);
		}
	}

	public void OnVisionTrigger()
	{
		if (enemy.CurrentState == EnemyState.Despawn || (currentState != State.Idle && currentState != State.TargetLost))
		{
			return;
		}
		PlayerAvatar onVisionTriggeredPlayer = enemy.Vision.onVisionTriggeredPlayer;
		if (!SemiFunc.PlayerVisionCheckPosition(enemy.Vision.VisionTransform.position, onVisionTriggeredPlayer.PlayerVisionTarget.VisionTransform.position, enemy.Vision.VisionDistance, onVisionTriggeredPlayer, _previouslySeen: true))
		{
			return;
		}
		if (targetPlayer != onVisionTriggeredPlayer)
		{
			foreach (EnemyCeilingEye otherEnemy in otherEnemies)
			{
				if (otherEnemy.targetPlayer == onVisionTriggeredPlayer)
				{
					return;
				}
			}
			targetPlayer = onVisionTriggeredPlayer;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("TargetPlayerRPC", RpcTarget.All, targetPlayer.photonView.ViewID);
			}
			else
			{
				TargetPlayerRPC(targetPlayer.photonView.ViewID);
			}
		}
		UpdateState(State.HasTarget);
	}

	public void TargetFailSafe()
	{
		if (currentState != State.Death)
		{
			if (currentState != State.HasTarget)
			{
				targetPlayer = null;
			}
			else if (currentState == State.HasTarget && (!targetPlayer || targetPlayer.isDisabled))
			{
				UpdateState(State.TargetLost);
			}
		}
	}

	private void UpdateState(State _state)
	{
		if ((!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient) && currentState != _state)
		{
			currentState = _state;
			stateImpulse = true;
			stateTimer = 0f;
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdateStateRPC", RpcTarget.All, currentState);
			}
			else
			{
				UpdateStateRPC(currentState);
			}
		}
	}

	public void RotationAnimation()
	{
		if (currentState == State.Idle)
		{
			eyeTransform.localRotation = Quaternion.Slerp(eyeTransform.localRotation, Quaternion.identity, 5f * Time.deltaTime);
		}
		else if (currentState == State.HasTarget)
		{
			Vector3 forward = SemiFunc.ClampDirection(targetPlayer.transform.position - enemy.CenterTransform.position, base.transform.forward, 35f);
			eyeTransform.rotation = Quaternion.RotateTowards(eyeTransform.rotation, Quaternion.LookRotation(forward), 360f * Time.deltaTime);
		}
	}

	[PunRPC]
	private void UpdateStateRPC(State _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _state;
			stateImpulse = true;
			if (currentState == State.Spawn && eyeAnim.isActiveAndEnabled)
			{
				eyeAnim.SetSpawn();
			}
		}
	}

	[PunRPC]
	private void TargetPlayerRPC(int _playerID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.photonView.ViewID == _playerID)
			{
				if (player.isLocal)
				{
					player.physGrabber.ReleaseObject(-1, 0.5f);
					CameraAim.Instance.AimTargetSet(enemy.CenterTransform.position, 0.5f, 2f, base.gameObject, 100);
					CameraGlitch.Instance.PlayLong();
				}
				targetPlayer = player;
				break;
			}
		}
	}

	[PunRPC]
	private void UpdatePositionRPC(Vector3 _position, Quaternion _rotation, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			base.transform.position = _position;
			base.transform.rotation = _rotation;
		}
	}
}
