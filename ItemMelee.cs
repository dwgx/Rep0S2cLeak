using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class ItemMelee : MonoBehaviour, IPunObservable
{
	private float durabilityDrain = 2.5f;

	public float durabilityDrainOnEnemiesAndPVP = 5f;

	public float hitFreeze = 0.2f;

	public float hitFreezeDelay;

	public float swingDetectSpeedMultiplier = 1f;

	public bool turnWeapon = true;

	public bool customTorqueStrength;

	public float torqueStrength = 0.4f;

	public float turnWeaponStrength = 40f;

	public Quaternion customRotation = Quaternion.identity;

	public UnityEvent onHit;

	private HurtCollider hurtCollider;

	private Transform hurtColliderTransform;

	private Transform hurtColliderRotation;

	private PhysGrabObjectImpactDetector physGrabObjectImpactDetector;

	private PhysGrabObject physGrabObject;

	private Rigidbody rb;

	private float swingTimer = 0.1f;

	private float hitBoxTimer = 0.1f;

	private TrailRenderer trailRenderer;

	public Sound soundSwingLoop;

	public Sound soundSwing;

	public Sound soundHit;

	private Vector3 prevPosition;

	private float prevPosDistance;

	private float prevPosUpdateTimer;

	private Transform swingPoint;

	private Quaternion swingDirection;

	private PlayerAvatar playerAvatar;

	private float hitSoundDelayTimer;

	private ParticleSystem particleSystem;

	private ParticleSystem particleSystemGroundHit;

	private PhotonView photonView;

	private float swingPitch = 1f;

	private float swingPitchTarget;

	private float swingPitchTargetProgress;

	private float distanceCheckTimer;

	private ItemBattery itemBattery;

	private Vector3 swingStartDirection = Vector3.zero;

	private Transform forceGrabPoint;

	private bool isBroken;

	private Quaternion targetYRotation;

	private Quaternion currentYRotation;

	private float durabilityLossCooldown;

	private Transform meshHealthy;

	private Transform meshBroken;

	internal bool isSwinging;

	private bool newSwing;

	private float hitTimer;

	private ItemEquippable itemEquippable;

	private float hitCooldown;

	private float groundHitCooldown;

	private float groundHitSoundTimer;

	private float spawnTimer = 3f;

	private float grabbedTimer;

	private float enemyOrPVPDurabilityLossCooldown;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		hurtColliderTransform = GetComponentInChildren<HurtCollider>().transform;
		hurtCollider = GetComponentInChildren<HurtCollider>();
		hurtColliderRotation = base.transform.Find("Hurt Collider Rotation");
		physGrabObjectImpactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		hurtColliderTransform.gameObject.SetActive(value: false);
		trailRenderer = GetComponentInChildren<TrailRenderer>();
		swingPoint = base.transform.Find("Swing Point");
		physGrabObject = GetComponent<PhysGrabObject>();
		particleSystem = base.transform.Find("Particles").GetComponent<ParticleSystem>();
		particleSystemGroundHit = base.transform.Find("Particles Ground Hit").GetComponent<ParticleSystem>();
		photonView = GetComponent<PhotonView>();
		itemBattery = GetComponent<ItemBattery>();
		forceGrabPoint = base.transform.Find("Force Grab Point");
		meshHealthy = base.transform.Find("Mesh Healthy");
		meshBroken = base.transform.Find("Mesh Broken");
		itemEquippable = GetComponent<ItemEquippable>();
		if (SemiFunc.RunIsArena())
		{
			HurtCollider component = hurtColliderTransform.GetComponent<HurtCollider>();
			component.playerDamage = component.enemyDamage;
		}
		rb.mass = 0.1f;
	}

	private void DisableHurtBoxWhenEquipping()
	{
		if (itemEquippable.equipTimer > 0f || itemEquippable.unequipTimer > 0f)
		{
			hurtColliderTransform.gameObject.SetActive(value: false);
			swingTimer = 0f;
			trailRenderer.emitting = false;
		}
	}

	private void FixedUpdate()
	{
		DisableHurtBoxWhenEquipping();
		bool flag = false;
		foreach (PhysGrabber item in physGrabObject.playerGrabbing)
		{
			if (item.isRotating)
			{
				flag = true;
			}
			if (item.isLocal)
			{
				item.OverrideMinimumGrabDistance(1.2f);
			}
		}
		float pos = -0.2f;
		physGrabObject.OverrideGrabVerticalPosition(pos);
		if (!flag)
		{
			Quaternion turnY = currentYRotation;
			Quaternion turnX = Quaternion.Euler(45f, 0f, 0f);
			physGrabObject.TurnXYZ(turnX, turnY, Quaternion.identity);
		}
		if (itemEquippable.equipTimer > 0f || itemEquippable.unequipTimer > 0f)
		{
			return;
		}
		if (isBroken)
		{
			physGrabObject.OverrideExtraGrabStrengthDisable();
			physGrabObject.OverrideExtraTorqueStrengthDisable();
			physGrabObject.OverrideTorqueStrength(0.001f);
			return;
		}
		if (prevPosUpdateTimer > 0.1f)
		{
			prevPosition = swingPoint.position;
			prevPosUpdateTimer = 0f;
		}
		else
		{
			prevPosUpdateTimer += Time.fixedDeltaTime;
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (!flag && physGrabObject.playerGrabbing.Count > 0)
		{
			physGrabObject.OverrideKnockOutOfGrabDisable();
			physGrabObject.OverrideMinGrabStrength(5f);
			physGrabObject.OverrideExtraTorqueStrengthDisable();
			if (!customTorqueStrength)
			{
				physGrabObject.OverrideTorqueStrength(0.4f);
			}
			else
			{
				physGrabObject.OverrideTorqueStrength(torqueStrength);
			}
			physGrabObject.OverrideMaterial(SemiFunc.PhysicMaterialSlippery());
		}
		if (flag)
		{
			physGrabObject.OverrideMinTorqueStrength(4f);
		}
		if (distanceCheckTimer > 0.1f)
		{
			prevPosDistance = Vector3.Distance(prevPosition, swingPoint.position) * 10f * rb.mass;
			distanceCheckTimer = 0f;
		}
		distanceCheckTimer += Time.fixedDeltaTime;
		TurnWeapon();
		Vector3 vector = prevPosition - swingPoint.position;
		float num = 0.8f;
		if (!physGrabObject.grabbed)
		{
			num = 0.4f;
		}
		if (vector.magnitude > num * swingDetectSpeedMultiplier && swingPoint.position - prevPosition != Vector3.zero)
		{
			swingTimer = 0.4f;
			if (!isSwinging)
			{
				newSwing = true;
			}
			swingDirection = Quaternion.LookRotation(swingPoint.position - prevPosition);
		}
	}

	private void TurnWeapon()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (customRotation != Quaternion.identity && !turnWeapon)
		{
			Quaternion turnX = Quaternion.Euler(45f, 0f, 0f);
			physGrabObject.TurnXYZ(turnX, customRotation, Quaternion.identity);
		}
		_ = turnWeaponStrength;
		_ = 1f;
		if (!turnWeapon)
		{
			return;
		}
		if (physGrabObject.grabbed && !playerAvatar)
		{
			playerAvatar = physGrabObject.playerGrabbing[0].GetComponent<PlayerAvatar>();
		}
		if (!physGrabObject.grabbed && (bool)playerAvatar)
		{
			playerAvatar = null;
		}
		if (!physGrabObject.grabbed)
		{
			return;
		}
		_ = Vector3.forward;
		_ = Vector3.up;
		_ = playerAvatar.transform;
		Vector3 direction = rb.velocity / Time.fixedDeltaTime;
		if (direction.magnitude > 200f)
		{
			Vector3 vector = playerAvatar.transform.InverseTransformDirection(direction);
			Vector3 vector2 = new Vector3(vector.x, 0f, vector.z);
			Quaternion quaternion = Quaternion.identity;
			if (vector2 != Vector3.zero)
			{
				quaternion = Quaternion.LookRotation(vector2);
			}
			Quaternion quaternion2 = Quaternion.Euler(0f, Mathf.Round(Quaternion.Euler(0f, quaternion.eulerAngles.y + 90f, 0f).eulerAngles.y / 90f) * 90f, 0f);
			if (quaternion2.eulerAngles.y == 270f)
			{
				quaternion2 = Quaternion.Euler(0f, 90f, 0f);
			}
			if (quaternion2.eulerAngles.y == 180f)
			{
				quaternion2 = Quaternion.Euler(0f, 0f, 0f);
			}
			targetYRotation = quaternion2;
		}
		currentYRotation = Quaternion.Slerp(currentYRotation, targetYRotation, Time.deltaTime * 5f);
	}

	private void Update()
	{
		if (grabbedTimer > 0f && (bool)hurtCollider)
		{
			if (physGrabObject.playerGrabbing.Count == 1 && physGrabObject.playerGrabbing[0].isLocal)
			{
				hurtCollider.ignoreLocalPlayer = true;
			}
			else
			{
				hurtCollider.ignoreLocalPlayer = false;
			}
		}
		if (grabbedTimer > 0f)
		{
			grabbedTimer -= Time.deltaTime;
		}
		if (physGrabObject.grabbed)
		{
			grabbedTimer = 1f;
		}
		if (hitFreezeDelay > 0f)
		{
			hitFreezeDelay -= Time.deltaTime;
			if (hitFreezeDelay <= 0f)
			{
				physGrabObject.FreezeForces(hitFreeze, Vector3.zero, Vector3.zero);
			}
		}
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (spawnTimer > 0f)
		{
			prevPosition = swingPoint.position;
			swingTimer = 0f;
			spawnTimer -= Time.deltaTime;
			return;
		}
		if (hitCooldown > 0f)
		{
			hitCooldown -= Time.deltaTime;
		}
		if (enemyOrPVPDurabilityLossCooldown > 0f)
		{
			enemyOrPVPDurabilityLossCooldown -= Time.deltaTime;
		}
		if (groundHitCooldown > 0f)
		{
			groundHitCooldown -= Time.deltaTime;
		}
		if (groundHitSoundTimer > 0f)
		{
			groundHitSoundTimer -= Time.deltaTime;
		}
		DisableHurtBoxWhenEquipping();
		if (itemEquippable.equipTimer > 0f || itemEquippable.unequipTimer > 0f)
		{
			return;
		}
		soundSwingLoop.PlayLoop(hurtColliderTransform.gameObject.activeSelf, 10f, 10f, 3f);
		if (SemiFunc.IsMultiplayer() && !SemiFunc.IsMasterClient() && isSwinging)
		{
			swingTimer = 1f;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (itemBattery.batteryLife <= 0f)
			{
				MeleeBreak();
			}
			else
			{
				MeleeFix();
			}
			if (durabilityLossCooldown > 0f)
			{
				durabilityLossCooldown -= Time.deltaTime;
			}
			if (!isBroken)
			{
				if (physGrabObject.grabbedLocal)
				{
					if (itemBattery.batteryActive)
					{
					}
				}
				else
				{
					_ = itemBattery.batteryActive;
				}
			}
		}
		if (isBroken)
		{
			return;
		}
		if (hitSoundDelayTimer > 0f)
		{
			hitSoundDelayTimer -= Time.deltaTime;
		}
		if (swingPitch != swingPitchTarget && swingPitchTargetProgress >= 1f)
		{
			swingPitch = swingPitchTarget;
		}
		Vector3 vector = prevPosition - swingPoint.position;
		if (vector.magnitude > 0.1f)
		{
			hurtColliderRotation.LookAt(hurtColliderRotation.position - vector, Vector3.up);
			hurtColliderRotation.localEulerAngles = new Vector3(0f, hurtColliderRotation.localEulerAngles.y, 0f);
			hurtColliderRotation.localEulerAngles = new Vector3(0f, Mathf.Round(hurtColliderRotation.localEulerAngles.y / 90f) * 90f, 0f);
		}
		Vector3 vector2 = prevPosition - swingPoint.position;
		Vector3 normalized = swingStartDirection.normalized;
		Vector3 normalized2 = vector2.normalized;
		float num = Vector3.Dot(normalized, normalized2);
		double num2 = 0.85;
		if (!physGrabObject.grabbed)
		{
			num2 = 0.1;
		}
		if ((double)num > num2)
		{
			swingTimer = 0f;
		}
		if (isSwinging)
		{
			ActivateHitbox();
		}
		if (hitTimer > 0f)
		{
			hitTimer -= Time.deltaTime;
		}
		if (swingTimer <= 0f)
		{
			if (hitBoxTimer <= 0f)
			{
				hurtColliderTransform.gameObject.SetActive(value: false);
			}
			else
			{
				hitBoxTimer -= Time.deltaTime;
			}
			trailRenderer.emitting = false;
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				isSwinging = false;
			}
		}
		else
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				isSwinging = true;
			}
			if (hitTimer <= 0f)
			{
				hitBoxTimer = 0.4f;
			}
			swingTimer -= Time.deltaTime;
		}
	}

	public void SwingHit()
	{
		if (!SemiFunc.IsMultiplayer())
		{
			SwingHitRPC(durabilityLoss: true);
			return;
		}
		photonView.RPC("SwingHitRPC", RpcTarget.All, true);
	}

	public void EnemySwingHit()
	{
		if (enemyOrPVPDurabilityLossCooldown <= 0f)
		{
			if (!SemiFunc.IsMultiplayer())
			{
				EnemyOrPVPSwingHitRPC(_playerHit: false);
				return;
			}
			photonView.RPC("EnemyOrPVPSwingHitRPC", RpcTarget.All, false);
		}
	}

	public void PlayerSwingHit()
	{
		if (enemyOrPVPDurabilityLossCooldown <= 0f)
		{
			if (!SemiFunc.IsMultiplayer())
			{
				EnemyOrPVPSwingHitRPC(_playerHit: true);
				return;
			}
			photonView.RPC("EnemyOrPVPSwingHitRPC", RpcTarget.All, true);
		}
	}

	[PunRPC]
	public void EnemyOrPVPSwingHitRPC(bool _playerHit)
	{
		if (_playerHit)
		{
			if (SemiFunc.RunIsArena())
			{
				itemBattery.batteryLife -= durabilityDrainOnEnemiesAndPVP;
			}
		}
		else
		{
			itemBattery.batteryLife -= durabilityDrainOnEnemiesAndPVP;
		}
		enemyOrPVPDurabilityLossCooldown = 0.1f;
	}

	[PunRPC]
	public void SwingHitRPC(bool durabilityLoss)
	{
		bool flag = false;
		if (durabilityLoss)
		{
			if (hitCooldown > 0f || hitSoundDelayTimer > 0f)
			{
				return;
			}
			hitSoundDelayTimer = 0.1f;
			hitCooldown = 0.3f;
		}
		else
		{
			if (groundHitCooldown > 0f || groundHitSoundTimer > 0f)
			{
				return;
			}
			groundHitCooldown = 0.3f;
			groundHitSoundTimer = 0.1f;
			flag = true;
		}
		if (!flag)
		{
			soundHit.Pitch = 1f;
			soundHit.Play(base.transform.position);
			particleSystem.Play();
		}
		else
		{
			soundHit.Pitch = 2f;
			soundHit.Play(base.transform.position, 0.5f);
			particleSystemGroundHit.Play();
		}
		if (physGrabObject.grabbed && !rb.isKinematic)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
		if (hitBoxTimer > 0.05f)
		{
			hitBoxTimer = 0.05f;
		}
		hitTimer = 0.5f;
		if (SemiFunc.IsMasterClientOrSingleplayer() && durabilityLoss && durabilityLossCooldown <= 0f && SemiFunc.RunIsLevel())
		{
			itemBattery.batteryLife -= durabilityDrain;
			durabilityLossCooldown = 0.1f;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && hitFreeze > 0f && !flag)
		{
			hitFreezeDelay = 0.06f;
		}
		if (onHit != null)
		{
			onHit.Invoke();
		}
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 10f, base.transform.position, 0.1f);
	}

	public void GroundHit()
	{
		if (!(hitTimer > 0f) && !(hitBoxTimer <= 0f))
		{
			if (!SemiFunc.IsMultiplayer())
			{
				SwingHitRPC(durabilityLoss: false);
				return;
			}
			photonView.RPC("SwingHitRPC", RpcTarget.All, false);
		}
	}

	private void MeleeBreak()
	{
		if (!isBroken)
		{
			if (!SemiFunc.IsMultiplayer())
			{
				MeleeBreakRPC();
			}
			else if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("MeleeBreakRPC", RpcTarget.All);
			}
		}
	}

	[PunRPC]
	public void MeleeBreakRPC()
	{
		if (physGrabObject.isMelee)
		{
			particleSystem.Play();
			physGrabObject.isMelee = false;
			physGrabObject.forceGrabPoint = null;
			itemBattery.BatteryToggle(toggle: false);
			isBroken = true;
			hurtColliderTransform.gameObject.SetActive(value: false);
			trailRenderer.emitting = false;
			meshHealthy.gameObject.SetActive(value: false);
			meshBroken.gameObject.SetActive(value: true);
		}
	}

	private void MeleeFix()
	{
		if (isBroken)
		{
			if (!SemiFunc.IsMultiplayer())
			{
				MeleeFixRPC();
			}
			else if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("MeleeFixRPC", RpcTarget.All);
			}
		}
	}

	[PunRPC]
	public void MeleeFixRPC()
	{
		if (!physGrabObject.isMelee)
		{
			particleSystem.Play();
			physGrabObject.isMelee = true;
			physGrabObject.forceGrabPoint = forceGrabPoint;
			itemBattery.BatteryToggle(toggle: true);
			isBroken = false;
			meshHealthy.gameObject.SetActive(value: true);
			meshBroken.gameObject.SetActive(value: false);
		}
	}

	public void ActivateHitbox()
	{
		if (hitTimer > 0f)
		{
			return;
		}
		if (newSwing)
		{
			soundSwing.Play(base.transform.position);
			swingPitchTarget = prevPosDistance;
			swingPitchTargetProgress = 0f;
			if ((bool)swingPoint)
			{
				swingStartDirection = swingPoint.position - prevPosition;
			}
			swingTimer = 0.8f;
			hitBoxTimer = 0.8f;
			if (grabbedTimer > 0f)
			{
				float num = 150f;
				if (!physGrabObject.grabbed)
				{
					num *= 0.5f;
				}
			}
			newSwing = false;
		}
		if ((bool)hurtColliderTransform)
		{
			hurtColliderTransform.gameObject.SetActive(value: true);
		}
		if ((bool)trailRenderer)
		{
			trailRenderer.emitting = true;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!SemiFunc.MasterOnlyRPC(info))
		{
			return;
		}
		if (PhotonNetwork.IsMasterClient)
		{
			stream.SendNext(isSwinging);
			return;
		}
		bool num = isSwinging;
		isSwinging = (bool)stream.ReceiveNext();
		if (!num && isSwinging)
		{
			newSwing = true;
			ActivateHitbox();
		}
	}
}
