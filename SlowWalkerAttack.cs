using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SlowWalkerAttack : MonoBehaviour
{
	public enum State
	{
		Idle,
		CheckInitial,
		Implosion,
		Delay,
		CheckAttack,
		Attack
	}

	public Transform vacuumSphere;

	[Space(10f)]
	public GameObject attackVacuumBuildup;

	public GameObject attackVacuumHurtCollider;

	public GameObject attackImpact;

	public GameObject attackImpactHurtColliders;

	private PhotonView photonView;

	[Space(10f)]
	private List<PlayerAvatar> playersBeingVacuumed = new List<PlayerAvatar>();

	private List<PlayerTumble> playerTumbles = new List<PlayerTumble>();

	private List<PhysGrabObject> physGrabObjects = new List<PhysGrabObject>();

	private List<ParticleSystem> vacuumParticles = new List<ParticleSystem>();

	private List<ParticleSystem> impactParticles = new List<ParticleSystem>();

	[Space(10f)]
	public Sound soundVacuumImpact;

	public Sound soundVacuumImpactGlobal;

	public Sound soundVacuumBuildup;

	public Sound soundImpact;

	public Sound soundImpactGlobal;

	public PhysGrabObject enemyPhysGrabObject;

	public Enemy enemy;

	internal State currentState;

	private bool stateStart;

	private bool stateFixed;

	private float stateTimer;

	public Transform clubHitPoint;

	public GameObject hurtColliderFirstHit;

	private Vector3 foundPosition;

	private bool didFindPosition;

	private Vector3 slowWalkerCenter;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		vacuumParticles.AddRange(attackVacuumBuildup.GetComponentsInChildren<ParticleSystem>());
		impactParticles.AddRange(attackImpact.GetComponentsInChildren<ParticleSystem>());
	}

	private void SuckInListUpdate()
	{
		if (!clubHitPoint)
		{
			return;
		}
		base.transform.position = clubHitPoint.position;
		RaycastHit[] array = Physics.RaycastAll(slowWalkerCenter, (clubHitPoint.position - slowWalkerCenter).normalized, 4f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
		bool flag = false;
		Vector3 point = slowWalkerCenter;
		float num = float.MaxValue;
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			if (raycastHit.collider.gameObject.CompareTag("Wall"))
			{
				float num2 = Vector3.Distance(slowWalkerCenter, raycastHit.point);
				if (num2 < num)
				{
					num = num2;
					point = raycastHit.point;
					flag = true;
				}
			}
		}
		if (flag)
		{
			foundPosition = point;
			foundPosition = Vector3.MoveTowards(foundPosition, slowWalkerCenter, 0.2f);
			didFindPosition = true;
		}
		point = slowWalkerCenter;
		num = float.MaxValue;
		RaycastHit[] array3 = Physics.RaycastAll(base.transform.position, Vector3.down, 2f, LayerMask.GetMask("Default"));
		flag = false;
		array2 = array3;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit2 = array2[i];
			if (raycastHit2.collider.gameObject.CompareTag("Wall"))
			{
				float num3 = Vector3.Distance(base.transform.position, raycastHit2.point);
				if (num3 < num)
				{
					num = num3;
					point = raycastHit2.point;
					flag = true;
				}
			}
		}
		if (flag)
		{
			foundPosition = point;
			didFindPosition = true;
		}
		if (didFindPosition)
		{
			base.transform.position = foundPosition;
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		physGrabObjects.Clear();
		foreach (PhysGrabObject item in SemiFunc.PhysGrabObjectGetAllWithinRange(vacuumSphere.localScale.x * 0.5f, vacuumSphere.position + Vector3.up * 0.5f))
		{
			RaycastHit[] array4 = Physics.RaycastAll(item.midPoint, base.transform.position + Vector3.up * 0.5f - item.midPoint, vacuumSphere.localScale.x, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
			bool flag2 = false;
			array2 = array4;
			foreach (RaycastHit raycastHit3 in array2)
			{
				if (raycastHit3.collider.gameObject.CompareTag("Wall"))
				{
					flag2 = true;
				}
			}
			if (!flag2 && !item.isPlayer && item != enemyPhysGrabObject)
			{
				physGrabObjects.Add(item);
			}
		}
	}

	private void SuckInListPlayerUpdate()
	{
		Vector3 vector = vacuumSphere.position + Vector3.up * 2f;
		Vector3 vector2 = vector;
		playersBeingVacuumed.Clear();
		List<PlayerAvatar> collection = SemiFunc.PlayerGetAllPlayerAvatarWithinRange(vacuumSphere.localScale.x, vector);
		playersBeingVacuumed.AddRange(collection);
		playerTumbles.Clear();
		foreach (PlayerAvatar item in playersBeingVacuumed)
		{
			vector = vacuumSphere.position + Vector3.up * 2f;
			Vector3 position = item.PlayerVisionTarget.VisionTransform.position;
			Vector3 normalized = (position - vector).normalized;
			float maxDistance = Vector3.Distance(vector, position);
			RaycastHit[] array = Physics.RaycastAll(vector, normalized, maxDistance, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
			bool flag = false;
			RaycastHit[] array2 = array;
			foreach (RaycastHit raycastHit in array2)
			{
				if (raycastHit.collider.gameObject.CompareTag("Wall"))
				{
					flag = true;
					break;
				}
			}
			bool flag2 = false;
			if (flag)
			{
				array2 = Physics.RaycastAll(vector, Vector3.up, vacuumSphere.localScale.x * 0.25f, LayerMask.GetMask("Default"));
				foreach (RaycastHit raycastHit2 in array2)
				{
					if (raycastHit2.collider.gameObject.CompareTag("Wall"))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					vector = vector2 + Vector3.up * vacuumSphere.localScale.x * 0.25f;
					normalized = (position - vector).normalized;
					maxDistance = Vector3.Distance(vector, position);
					RaycastHit[] array3 = Physics.RaycastAll(vector, normalized, maxDistance, LayerMask.GetMask("Default"));
					flag = false;
					array2 = array3;
					foreach (RaycastHit raycastHit3 in array2)
					{
						if (raycastHit3.collider.gameObject.CompareTag("Wall"))
						{
							flag = true;
							break;
						}
					}
				}
			}
			if (flag && !flag2)
			{
				array2 = Physics.RaycastAll(vector, Vector3.up, vacuumSphere.localScale.x * 0.5f, LayerMask.GetMask("Default"));
				foreach (RaycastHit raycastHit4 in array2)
				{
					if (raycastHit4.collider.gameObject.CompareTag("Wall"))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					vector = vector2 + Vector3.up * vacuumSphere.localScale.x * 0.5f;
					normalized = (position - vector).normalized;
					maxDistance = Vector3.Distance(vector, position);
					RaycastHit[] array4 = Physics.RaycastAll(vector, normalized, maxDistance, LayerMask.GetMask("Default"));
					flag = false;
					array2 = array4;
					foreach (RaycastHit raycastHit5 in array2)
					{
						if (raycastHit5.collider.gameObject.CompareTag("Wall"))
						{
							flag = true;
							break;
						}
					}
				}
			}
			if (flag)
			{
				continue;
			}
			if (item.isTumbling)
			{
				playerTumbles.Add(item.tumble);
				if (SemiFunc.IsMasterClientOrSingleplayer())
				{
					item.tumble.TumbleOverrideTime(2f);
					item.tumble.OverrideEnemyHurt(0.5f);
				}
			}
			if (!item.isDisabled && !item.isTumbling)
			{
				playerTumbles.Add(item.tumble);
				if (SemiFunc.IsMasterClientOrSingleplayer())
				{
					item.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
					item.tumble.TumbleOverrideTime(2f);
					item.tumble.OverrideEnemyHurt(0.5f);
				}
			}
		}
	}

	private void StateIdle()
	{
		if (!stateFixed && stateStart && !stateFixed)
		{
			ParticlesVacuum(_play: false);
			didFindPosition = false;
			stateStart = false;
		}
	}

	private void StateCheckInitial()
	{
		if (!stateFixed)
		{
			if (stateStart && !stateFixed)
			{
				didFindPosition = false;
				SuckInListUpdate();
				stateStart = false;
				stateTimer = 0.001f;
			}
			if (SemiFunc.IsMasterClientOrSingleplayer() && stateTimer <= 0f)
			{
				StateSet(State.Implosion);
			}
		}
	}

	private void StateImplosion()
	{
		if (stateStart && !stateFixed)
		{
			ParticlesVacuum(_play: true);
			stateStart = false;
			stateTimer = 1.5f;
			if ((bool)clubHitPoint)
			{
				base.transform.position = clubHitPoint.position;
			}
			ActivateImplosionHurtColliders();
			ActiveHurtColliderFirstHit();
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 6f, 15f, base.transform.position, 0.1f);
			GameDirector.instance.CameraShake.ShakeDistance(5f, 6f, 15f, base.transform.position, 0.1f);
			soundVacuumImpact.Play(base.transform.position);
			soundVacuumImpactGlobal.Play(base.transform.position);
			soundVacuumBuildup.Play(base.transform.position);
			Vector3 normalized = (base.transform.position - slowWalkerCenter).normalized;
			base.transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
			float y = base.transform.rotation.eulerAngles.y;
			base.transform.rotation = Quaternion.Euler(0f, y, 0f);
			SuckInListPlayerUpdate();
		}
		if (stateFixed)
		{
			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				return;
			}
			foreach (PlayerTumble playerTumble in playerTumbles)
			{
				if (playerTumble.isTumbling)
				{
					Vector3 normalized2 = (vacuumSphere.position - playerTumble.physGrabObject.transform.position).normalized;
					Rigidbody rb = playerTumble.physGrabObject.rb;
					rb.AddForce(normalized2 * 2500f * Time.fixedDeltaTime, ForceMode.Force);
					Vector3 vector = SemiFunc.PhysFollowDirection(rb.transform, normalized2, rb, 10f) * 2f;
					rb.AddTorque(vector / rb.mass, ForceMode.Force);
				}
			}
			foreach (PhysGrabObject physGrabObject in physGrabObjects)
			{
				if ((bool)physGrabObject)
				{
					Vector3 normalized3 = (vacuumSphere.position - physGrabObject.transform.position).normalized;
					Rigidbody rb2 = physGrabObject.rb;
					rb2.AddForce(normalized3 * 2500f * Time.fixedDeltaTime, ForceMode.Force);
					Vector3 vector2 = SemiFunc.PhysFollowDirection(rb2.transform, normalized3, rb2, 10f) * 2f;
					rb2.AddTorque(vector2 / rb2.mass, ForceMode.Force);
				}
			}
		}
		if (stateFixed)
		{
			return;
		}
		if (didFindPosition)
		{
			base.transform.position = foundPosition;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if ((bool)enemy && enemy.CurrentState == EnemyState.Stunned && currentState != State.Idle)
			{
				StateSet(State.Idle);
			}
			enemy.Health.ObjectHurtDisable(0.5f);
			if (stateTimer <= 0f)
			{
				StateSet(State.Attack);
			}
			if (SemiFunc.FPSImpulse5())
			{
				SuckInListPlayerUpdate();
			}
		}
	}

	private void StateDelay()
	{
		if (!stateFixed && stateStart && !stateFixed)
		{
			stateStart = false;
		}
	}

	private void StateCheckAttack()
	{
		if (!stateFixed && stateStart && !stateFixed)
		{
			stateStart = false;
		}
	}

	private void ActivateAttackHurtColliders()
	{
		attackImpactHurtColliders.SetActive(value: true);
	}

	private void ActivateImplosionHurtColliders()
	{
		attackVacuumHurtCollider.SetActive(value: true);
	}

	private void ActiveHurtColliderFirstHit()
	{
		hurtColliderFirstHit.SetActive(value: true);
	}

	private void StateAttack()
	{
		if (!stateFixed)
		{
			if (stateStart && !stateFixed)
			{
				stateStart = false;
				stateTimer = 3.5f;
				GameDirector.instance.CameraImpact.ShakeDistance(8f, 6f, 15f, base.transform.position, 0.1f);
				GameDirector.instance.CameraShake.ShakeDistance(8f, 6f, 15f, base.transform.position, 0.1f);
				ParticlesPlayImpact();
				soundImpact.Play(base.transform.position);
				soundImpactGlobal.Play(base.transform.position);
				Vector3 normalized = (base.transform.position - slowWalkerCenter).normalized;
				base.transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
				ActivateAttackHurtColliders();
			}
			if (stateTimer <= 0f)
			{
				StateSet(State.Idle);
			}
		}
	}

	private void StateMachine(bool _stateFixed)
	{
		if (_stateFixed)
		{
			stateFixed = true;
		}
		switch (currentState)
		{
		case State.Idle:
			StateIdle();
			break;
		case State.CheckInitial:
			StateCheckInitial();
			break;
		case State.Implosion:
			StateImplosion();
			break;
		case State.Attack:
			StateAttack();
			break;
		}
		if (_stateFixed && stateFixed)
		{
			stateFixed = false;
		}
	}

	public void SlowWalkerAttackStart()
	{
		StateSet(State.CheckInitial);
	}

	private void Update()
	{
		if ((bool)enemyPhysGrabObject)
		{
			slowWalkerCenter = enemyPhysGrabObject.midPoint;
		}
		if (SemiFunc.FPSImpulse1() && (bool)enemy && (bool)enemy.EnemyParent && !enemy.EnemyParent.Spawned && currentState != State.Idle)
		{
			StateSet(State.Idle);
		}
		StateMachine(_stateFixed: false);
		if (stateTimer > 0f)
		{
			stateTimer -= Time.deltaTime;
		}
	}

	private void FixedUpdate()
	{
		StateMachine(_stateFixed: true);
	}

	public void StateSet(State state)
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (!SemiFunc.IsMultiplayer())
		{
			if (state != currentState)
			{
				StateSetRPC(state);
			}
		}
		else if (state != currentState)
		{
			photonView.RPC("StateSetRPC", RpcTarget.All, state);
		}
	}

	[PunRPC]
	public void StateSetRPC(State state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = state;
			stateStart = true;
		}
	}

	private void ParticlesVacuum(bool _play)
	{
		foreach (ParticleSystem vacuumParticle in vacuumParticles)
		{
			if (_play)
			{
				if (vacuumParticle.isPlaying)
				{
					vacuumParticle.Stop();
				}
				vacuumParticle.Play();
			}
			else if (vacuumParticle.isPlaying)
			{
				vacuumParticle.Stop();
			}
		}
	}

	private void ParticlesPlayImpact()
	{
		foreach (ParticleSystem impactParticle in impactParticles)
		{
			impactParticle.Play();
		}
	}

	public void TrudgeAttackTest()
	{
		new List<HurtCollider>().AddRange(attackVacuumHurtCollider.GetComponentsInChildren<HurtCollider>());
	}

	private void OnDisable()
	{
		attackVacuumHurtCollider.SetActive(value: false);
		hurtColliderFirstHit.SetActive(value: false);
		attackImpactHurtColliders.SetActive(value: false);
		attackImpact.SetActive(value: false);
		attackVacuumBuildup.SetActive(value: false);
	}
}
