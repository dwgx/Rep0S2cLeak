using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class EnemyHeadController : MonoBehaviourPunCallbacks, IPunObservable
{
	private Quaternion RotationTarget;

	private float RotationDistance;

	private Enemy Enemy;

	public EnemyHeadVisual Visual;

	public EnemyHeadAnimationSystem AnimationSystem;

	[Space]
	public Transform LookAtTransform;

	public Transform AnimationTransform;

	public Transform HairParent;

	private List<EnemyHeadHair> Hairs;

	[Space]
	public List<GameObject> DeathParticles;

	public Sound DeathSound;

	private void Awake()
	{
		Enemy = GetComponentInParent<Enemy>();
		Hairs = HairParent.GetComponentsInChildren<EnemyHeadHair>().ToList();
	}

	private void Update()
	{
		if (Enemy.CurrentState == EnemyState.Chase || Enemy.CurrentState == EnemyState.ChaseSlow || Enemy.CurrentState == EnemyState.LookUnder)
		{
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (Vector3.Distance(base.transform.position, player.transform.position) < 8f)
				{
					SemiFunc.PlayerEyesOverride(player, Enemy.Vision.VisionTransform.position, 0.1f, base.gameObject);
				}
			}
		}
		if (Enemy.TeleportedTimer > 0f)
		{
			foreach (EnemyHeadHair hair in Hairs)
			{
				hair.transform.position = hair.Target.position;
			}
		}
		if (!Enemy.MasterClient)
		{
			float num = 1f / (float)PhotonNetwork.SerializationRate;
			float num2 = RotationDistance / num;
			LookAtTransform.rotation = Quaternion.RotateTowards(LookAtTransform.rotation, RotationTarget, num2 * Time.deltaTime);
			return;
		}
		if ((bool)Enemy.AttackStuckPhysObject.TargetObject)
		{
			if (Enemy.AttackStuckPhysObject != null)
			{
				Vector3 position = Enemy.AttackStuckPhysObject.TargetObject.roomVolumeCheck.transform.position;
				position += Enemy.AttackStuckPhysObject.TargetObject.roomVolumeCheck.transform.TransformDirection(Enemy.AttackStuckPhysObject.TargetObject.roomVolumeCheck.CheckPosition);
				LookAtTransform.LookAt(position);
			}
		}
		else if (Enemy.CurrentState == EnemyState.ChaseBegin)
		{
			LookAtTransform.LookAt(Enemy.TargetPlayerAvatar.PlayerVisionTarget.VisionTransform.position);
		}
		else if (Enemy.CurrentState == EnemyState.Chase && Enemy.StateChase.VisionTimer > 0f)
		{
			if (Enemy.NavMeshAgent.Agent.velocity.normalized.magnitude > 0.1f)
			{
				Quaternion quaternion = Quaternion.LookRotation(Enemy.NavMeshAgent.Agent.velocity.normalized);
				LookAtTransform.LookAt(Enemy.TargetPlayerAvatar.PlayerVisionTarget.VisionTransform.position);
				LookAtTransform.rotation = Quaternion.Lerp(LookAtTransform.rotation, quaternion, 0.25f);
			}
			else
			{
				LookAtTransform.LookAt(Enemy.TargetPlayerAvatar.PlayerVisionTarget.VisionTransform.position);
			}
		}
		else if (Enemy.CurrentState == EnemyState.LookUnder)
		{
			LookAtTransform.LookAt(Enemy.StateChase.SawPlayerHidePosition);
			LookAtTransform.localEulerAngles = new Vector3(0f, LookAtTransform.localEulerAngles.y, 0f);
		}
		else if (Enemy.NavMeshAgent.Agent.velocity.magnitude > 0.1f)
		{
			LookAtTransform.rotation = Quaternion.LookRotation(Enemy.NavMeshAgent.Agent.velocity.normalized);
			LookAtTransform.localEulerAngles = new Vector3(0f, LookAtTransform.localEulerAngles.y, 0f);
		}
		if (Enemy.CurrentState == EnemyState.Despawn)
		{
			Enemy.Rigidbody.DisableFollowPosition(0.1f, 1f);
			Enemy.Rigidbody.DisableFollowRotation(0.1f, 1f);
		}
	}

	public void VisionTriggered()
	{
		if (Enemy.DisableChaseTimer > 0f || Enemy.CurrentState == EnemyState.Chase || Enemy.CurrentState == EnemyState.LookUnder)
		{
			return;
		}
		if (Enemy.CurrentState == EnemyState.ChaseSlow)
		{
			Enemy.CurrentState = EnemyState.Chase;
		}
		else if (Enemy.Vision.onVisionTriggeredCulled && !Enemy.Vision.onVisionTriggeredNear)
		{
			if (Enemy.CurrentState != EnemyState.Sneak)
			{
				if (Random.Range(0f, 100f) <= 30f)
				{
					Enemy.CurrentState = EnemyState.ChaseBegin;
				}
				else
				{
					Enemy.CurrentState = EnemyState.Sneak;
				}
			}
		}
		else if (Enemy.Vision.onVisionTriggeredDistance >= 7f)
		{
			Enemy.CurrentState = EnemyState.Chase;
			Enemy.StateChase.ChaseCanReach = true;
		}
		else
		{
			Enemy.CurrentState = EnemyState.ChaseBegin;
		}
		Enemy.TargetPlayerViewID = Enemy.Vision.onVisionTriggeredPlayer.photonView.ViewID;
		Enemy.TargetPlayerAvatar = Enemy.Vision.onVisionTriggeredPlayer;
	}

	public void OnSpawn()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			SemiFunc.EnemySpawn(Enemy);
		}
		Visual.Spawn();
		if (AnimationSystem.isActiveAndEnabled)
		{
			AnimationSystem.OnSpawn();
		}
	}

	public void OnHurt()
	{
		AnimationSystem.Hurt.Play(Enemy.Rigidbody.transform.position);
	}

	public void OnDeath()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			Enemy.CurrentState = EnemyState.Despawn;
		}
		foreach (GameObject deathParticle in DeathParticles)
		{
			deathParticle.SetActive(value: true);
		}
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		DeathSound.Play(base.transform.position);
		Enemy.EnemyParent.Despawn();
	}

	public void OnStunnedEnd()
	{
		Enemy.CurrentState = EnemyState.Roaming;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(LookAtTransform.rotation);
				return;
			}
			RotationTarget = (Quaternion)stream.ReceiveNext();
			RotationDistance = Quaternion.Angle(base.transform.rotation, RotationTarget);
		}
	}
}
