using Photon.Pun;
using UnityEngine;

public class EnemyBombThrowerHead : MonoBehaviour
{
	public enum State
	{
		Disabled,
		Spawn,
		Active,
		Explode
	}

	public EnemyBombThrower controller;

	public GameObject uraniumCloudPrefab;

	public GenericEyeLookAt eyeLookAt;

	internal PhysGrabObject physGrabObject;

	private PhotonView photonView;

	private ParticleScriptExplosion particleScriptExplosion;

	internal PlayerAvatar playerTarget;

	internal State currentState;

	private float stateTimer;

	private bool stateImpulse;

	private float jumpCooldown;

	private float lookAtLerp;

	private Animator animator;

	private int animActive = Animator.StringToHash("active");

	private int animExplode = Animator.StringToHash("explode");

	public PhysicMaterial colliderMaterial;

	private bool setColliderMaterial = true;

	public ParticleSystem particleSplash;

	public Sound soundHeadDetach;

	public Sound soundLaugh;

	public Sound soundExplosionTell;

	public Sound soundExplosion;

	private void Awake()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
		photonView = GetComponent<PhotonView>();
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
	}

	private void Update()
	{
		if (LevelGenerator.Instance.Generated)
		{
			StateMachine(_fixedUpdate: false);
			SetColliderMaterial();
			EyeLogic();
			SoundLoopLogic();
			GrabberSharedLogic();
		}
	}

	private void FixedUpdate()
	{
		if (LevelGenerator.Instance.Generated)
		{
			StateMachine(_fixedUpdate: true);
			CustomGravity();
		}
	}

	private void StateMachine(bool _fixedUpdate)
	{
		switch (currentState)
		{
		case State.Disabled:
			StateDisabled(_fixedUpdate);
			break;
		case State.Spawn:
			StateSpawn(_fixedUpdate);
			break;
		case State.Active:
			StateActive(_fixedUpdate);
			break;
		case State.Explode:
			StateExplode(_fixedUpdate);
			break;
		}
	}

	public void UpdateState(State _state)
	{
		if (_state != currentState)
		{
			if (GameManager.Multiplayer())
			{
				photonView.RPC("UpdateStateRPC", RpcTarget.All, _state);
			}
			else
			{
				UpdateStateRPC(_state);
			}
		}
	}

	private void StateDisabled(bool _fixedUpdate)
	{
		if (stateImpulse)
		{
			stateImpulse = false;
		}
		if (!_fixedUpdate)
		{
			animator.SetBool(animActive, value: false);
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				physGrabObject.OverrideDeactivate(0.25f);
			}
		}
	}

	private void StateSpawn(bool _fixedUpdate)
	{
		if (stateImpulse)
		{
			animator.speed = 1f;
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				PlayerAvatar playerAvatar = controller.playerTarget;
				if (!playerAvatar || playerAvatar.isDisabled || controller.enemy.Health.dead)
				{
					float num = float.PositiveInfinity;
					foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
					{
						if (!item.isDisabled)
						{
							float num2 = Vector3.Distance(item.transform.position, base.transform.position);
							if (num2 < num)
							{
								playerAvatar = item;
								num = num2;
							}
						}
					}
				}
				UpdatePlayerTarget(playerAvatar);
				physGrabObject.OverrideDeactivateReset();
				physGrabObject.Teleport(controller.headSpawnTransform.position, controller.headSpawnTransform.rotation);
				Vector3 vector = Vector3.Lerp(controller.headSpawnTransform.forward, Vector3.up, 0.3f);
				physGrabObject.rb.AddForce(vector * 20f, ForceMode.Impulse);
				physGrabObject.rb.AddTorque(controller.headSpawnTransform.right * 0.25f, ForceMode.Impulse);
			}
			stateImpulse = false;
			stateTimer = 2f;
		}
		if (_fixedUpdate)
		{
			return;
		}
		animator.SetBool(animActive, value: true);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			stateTimer -= Time.deltaTime;
			GrabbedLogic();
			if (stateTimer <= 0f)
			{
				UpdateState(State.Active);
			}
		}
	}

	private void StateActive(bool _fixedUpdate)
	{
		if (stateImpulse)
		{
			jumpCooldown = 0.5f;
			lookAtLerp = 0f;
			stateImpulse = false;
			stateTimer = 5f;
		}
		if (!_fixedUpdate)
		{
			animator.speed = Mathf.Clamp(physGrabObject.rbAngularVelocity.magnitude * 0.25f, 2f, 2.5f);
			animator.SetBool(animActive, value: true);
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				stateTimer -= Time.deltaTime;
				jumpCooldown -= Time.deltaTime;
				GrabbedLogic();
				if (stateTimer <= 0f)
				{
					UpdateState(State.Explode);
				}
			}
		}
		else
		{
			if (!SemiFunc.IsMasterClientOrSingleplayer() || !playerTarget || playerTarget.isDisabled)
			{
				return;
			}
			if (physGrabObject.playerGrabbing.Count <= 0 && Vector3.Distance(playerTarget.transform.position, base.transform.position) > 2f)
			{
				Vector3 normalized = (playerTarget.transform.position - base.transform.position).normalized;
				Vector3 vector = SemiFunc.PhysFollowDirection(base.transform, normalized, physGrabObject.rb, 5f) * 1.5f;
				physGrabObject.rb.AddTorque(vector / physGrabObject.rb.mass, ForceMode.Force);
				if (jumpCooldown <= 0f)
				{
					if (SemiFunc.OnGroundCheck(physGrabObject.centerPoint, 0.5f, physGrabObject))
					{
						normalized = Vector3.Lerp(normalized, Vector3.up, 0.8f);
						physGrabObject.rb.AddForce(normalized * 15f, ForceMode.Impulse);
					}
					jumpCooldown = 1f;
				}
				lookAtLerp = 0f;
			}
			else
			{
				LookAtPlayerLogic();
			}
		}
	}

	private void StateExplode(bool _fixedUpdate)
	{
		if (stateImpulse)
		{
			animator.speed = 1f;
			animator.SetTrigger(animExplode);
			stateImpulse = false;
			stateTimer = 1f;
		}
		if (!_fixedUpdate)
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				stateTimer -= Time.deltaTime;
				if (stateTimer <= 0f)
				{
					UpdateState(State.Disabled);
				}
			}
		}
		else if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			LookAtPlayerLogic();
		}
	}

	private void SetColliderMaterial()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && setColliderMaterial && LevelGenerator.Instance.Generated)
		{
			GetComponentInChildren<Collider>().material = colliderMaterial;
			setColliderMaterial = false;
		}
	}

	private void CustomGravity()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !physGrabObject.rb.isKinematic && physGrabObject.playerGrabbing.Count <= 0 && !physGrabObject.deathPitEffect)
		{
			physGrabObject.rb.AddForce(-Vector3.up * 15f, ForceMode.Force);
		}
	}

	private void EyeLogic()
	{
		if ((bool)playerTarget && !playerTarget.isDisabled)
		{
			eyeLookAt.SetTargetPlayer(playerTarget);
		}
	}

	private void UpdatePlayerTarget(PlayerAvatar _player)
	{
		playerTarget = _player;
		int num = -1;
		if ((bool)playerTarget)
		{
			num = playerTarget.photonView.ViewID;
		}
		if (GameManager.Multiplayer())
		{
			photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.Others, num);
		}
	}

	private void SoundLoopLogic()
	{
		soundLaugh.PlayLoop(currentState == State.Active, 5f, 5f, 0.75f + physGrabObject.rbAngularVelocity.magnitude * 0.02f);
	}

	private void GrabberSharedLogic()
	{
		foreach (PhysGrabber item in physGrabObject.playerGrabbing)
		{
			item.OverrideGrabPoint(physGrabObject.transform);
			item.OverrideGrabDistance(1f);
		}
		if (physGrabObject.playerGrabbing.Count > 0)
		{
			SemiFunc.EnemyOvercharge(physGrabObject, EnemyParent.Difficulty.Difficulty3, 1f);
		}
	}

	private void GrabbedLogic()
	{
		if (physGrabObject.playerGrabbing.Count > 0)
		{
			stateTimer -= 1f * Time.deltaTime;
			physGrabObject.OverrideTorqueStrength(0f, 0.2f);
			physGrabObject.OverrideGrabVerticalPosition(0.25f);
			if (physGrabObject.playerGrabbing[0].playerAvatar != playerTarget)
			{
				UpdatePlayerTarget(physGrabObject.playerGrabbing[0].playerAvatar);
			}
		}
	}

	private void LookAtPlayerLogic()
	{
		float num = 5f;
		Vector3 normalized = (playerTarget.PlayerVisionTarget.VisionTransform.position - base.transform.position).normalized;
		Vector3 vector = SemiFunc.PhysFollowRotation(base.transform, Quaternion.LookRotation(normalized), physGrabObject.rb, 10f);
		vector = Vector3.Lerp(Vector3.zero, vector, num * Time.fixedDeltaTime);
		vector = Vector3.Lerp(Vector3.zero, vector, lookAtLerp);
		lookAtLerp = Mathf.Clamp01(lookAtLerp + 2f * Time.fixedDeltaTime);
		physGrabObject.rb.AddTorque(vector, ForceMode.Impulse);
	}

	[PunRPC]
	private void UpdateStateRPC(State _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _state;
			stateImpulse = true;
			stateTimer = 0f;
			StateMachine(_fixedUpdate: false);
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
				break;
			}
		}
	}

	[PunRPC]
	private void ExplodeRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			soundExplosion.Play(physGrabObject.centerPoint);
			particleSplash.gameObject.SetActive(value: true);
			ParticlePrefabExplosion particlePrefabExplosion = particleScriptExplosion.Spawn(base.transform.position, 2f, 75, 30, 4f);
			particlePrefabExplosion.HurtCollider.ignoreObjects.Add(physGrabObject);
			particlePrefabExplosion.HurtCollider.enemyHost = controller.enemy;
			particlePrefabExplosion.particleSizeMultiplier = 0.8f;
			particlePrefabExplosion.HurtCollider.playerTumbleImpactHurtDamage = 10;
			particlePrefabExplosion.HurtCollider.playerTumbleImpactHurtTime = 2f;
			UraniumScript component = Object.Instantiate(uraniumCloudPrefab, base.transform.position, Quaternion.identity).GetComponent<UraniumScript>();
			component.hurtCollider.ignoreObjects.Add(controller.enemy.Rigidbody.physGrabObject);
			component.hurtCollider.enemyHost = controller.enemy;
		}
	}

	public void AnimationEventSpawn()
	{
		soundHeadDetach.Play(physGrabObject.centerPoint).transform.SetParent(base.transform, worldPositionStays: true);
	}

	public void AnimationEventExplosionTell()
	{
		GameDirector.instance.CameraShake.ShakeDistance(0.5f, 3f, 5f, physGrabObject.centerPoint, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 5f, physGrabObject.centerPoint, 0.05f);
		soundExplosionTell.Play(physGrabObject.centerPoint).transform.SetParent(base.transform, worldPositionStays: true);
	}

	public void AnimationEventExplode()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("ExplodeRPC", RpcTarget.All);
			}
			else
			{
				ExplodeRPC();
			}
			stateTimer = 0f;
		}
	}
}
