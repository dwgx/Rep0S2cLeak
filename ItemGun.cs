using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class ItemGun : MonoBehaviour
{
	public enum State
	{
		Idle,
		OutOfAmmo,
		Buildup,
		Shooting,
		Reloading
	}

	private PhysGrabObject physGrabObject;

	private ItemToggle itemToggle;

	public bool hasOneShot = true;

	public float shootTime = 1f;

	public bool hasBuildUp;

	public float buildUpTime = 1f;

	public int numberOfBullets = 1;

	[Range(0f, 65f)]
	public float gunRandomSpread;

	public float gunRange = 50f;

	public float distanceKeep = 0.8f;

	public float gunRecoilForce = 1f;

	public float cameraShakeMultiplier = 1f;

	public float torqueMultiplier = 1f;

	public float grabStrengthMultiplier = 1f;

	public float shootCooldown = 1f;

	public float batteryDrain = 0.1f;

	public bool batteryDrainFullBar;

	public int batteryDrainFullBars = 1;

	[Range(0f, 100f)]
	public float misfirePercentageChange = 50f;

	public AnimationCurve shootLineWidthCurve;

	public float grabVerticalOffset = -0.2f;

	public float aimVerticalOffset = -5f;

	public float investigateRadius = 20f;

	private float investigateCooldown;

	public Transform gunMuzzle;

	public GameObject bulletPrefab;

	public GameObject muzzleFlashPrefab;

	public Transform gunTrigger;

	internal HurtCollider hurtCollider;

	public Sound soundShoot;

	public Sound soundShootGlobal;

	public Sound soundNoAmmoClick;

	public Sound soundHit;

	private ItemBattery itemBattery;

	private PhotonView photonView;

	private PhysGrabObjectImpactDetector impactDetector;

	private bool prevToggleState;

	private AnimationCurve triggerAnimationCurve;

	private float triggerAnimationEval;

	private bool triggerAnimationActive;

	public UnityEvent onStateIdleStart;

	public UnityEvent onStateIdleUpdate;

	public UnityEvent onStateIdleFixedUpdate;

	[Space(20f)]
	public UnityEvent onStateOutOfAmmoStart;

	public UnityEvent onStateOutOfAmmoUpdate;

	public UnityEvent onStateOutOfAmmoFixedUpdate;

	[Space(20f)]
	public UnityEvent onStateBuildupStart;

	public UnityEvent onStateBuildupUpdate;

	public UnityEvent onStateBuildupFixedUpdate;

	[Space(20f)]
	public UnityEvent onStateShootingStart;

	public UnityEvent onStateShootingUpdate;

	public UnityEvent onStateShootingFixedUpdate;

	[Space(20f)]
	public UnityEvent onStateReloadingStart;

	public UnityEvent onStateReloadingUpdate;

	public UnityEvent onStateReloadingFixedUpdate;

	private bool hasIdleUpdate = true;

	private bool hasIdleFixedUpdate = true;

	private bool hasOutOfAmmoUpdate = true;

	private bool hasOutOfAmmoFixedUpdate = true;

	private bool hasBuildupUpdate = true;

	private bool hasBuildupFixedUpdate = true;

	private bool hasShootingUpdate = true;

	private bool hasShootingFixedUpdate = true;

	private bool hasReloadingUpdate = true;

	private bool hasReloadingFixedUpdate = true;

	private RoomVolumeCheck roomVolumeCheck;

	internal float stateTimer;

	internal float stateTimeMax;

	internal State stateCurrent;

	private State statePrev;

	private bool stateStart;

	private ItemEquippable itemEquippable;

	private void Start()
	{
		roomVolumeCheck = GetComponent<RoomVolumeCheck>();
		itemEquippable = GetComponent<ItemEquippable>();
		physGrabObject = GetComponent<PhysGrabObject>();
		itemToggle = GetComponent<ItemToggle>();
		itemBattery = GetComponent<ItemBattery>();
		photonView = GetComponent<PhotonView>();
		impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		triggerAnimationCurve = AssetManager.instance.animationCurveClickInOut;
		if (onStateIdleUpdate == null)
		{
			hasIdleUpdate = false;
		}
		if (onStateIdleFixedUpdate == null)
		{
			hasIdleFixedUpdate = false;
		}
		if (onStateOutOfAmmoUpdate == null)
		{
			hasOutOfAmmoUpdate = false;
		}
		if (onStateOutOfAmmoFixedUpdate == null)
		{
			hasOutOfAmmoFixedUpdate = false;
		}
		if (onStateBuildupUpdate == null)
		{
			hasBuildupUpdate = false;
		}
		if (onStateBuildupFixedUpdate == null)
		{
			hasBuildupFixedUpdate = false;
		}
		if (onStateShootingUpdate == null)
		{
			hasShootingUpdate = false;
		}
		if (onStateShootingFixedUpdate == null)
		{
			hasShootingFixedUpdate = false;
		}
		if (onStateReloadingUpdate == null)
		{
			hasReloadingUpdate = false;
		}
		if (onStateReloadingFixedUpdate == null)
		{
			hasReloadingFixedUpdate = false;
		}
	}

	private void FixedUpdate()
	{
		StateMachine(_fixedUpdate: true);
	}

	private void Update()
	{
		StateMachine(_fixedUpdate: false);
		if (physGrabObject.grabbed && physGrabObject.grabbedLocal)
		{
			PhysGrabber.instance.OverrideGrabDistance(distanceKeep);
		}
		if (triggerAnimationActive)
		{
			float num = 45f;
			triggerAnimationEval += Time.deltaTime * 4f;
			gunTrigger.localRotation = Quaternion.Euler(num * triggerAnimationCurve.Evaluate(triggerAnimationEval), 0f, 0f);
			if (triggerAnimationEval >= 1f)
			{
				gunTrigger.localRotation = Quaternion.Euler(0f, 0f, 0f);
				triggerAnimationActive = false;
				triggerAnimationEval = 1f;
			}
		}
		UpdateMaster();
	}

	private void UpdateMaster()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || physGrabObject.playerGrabbing.Count <= 0)
		{
			return;
		}
		Quaternion turnX = Quaternion.Euler(aimVerticalOffset, 0f, 0f);
		Quaternion turnY = Quaternion.Euler(0f, 0f, 0f);
		Quaternion identity = Quaternion.identity;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = true;
		foreach (PhysGrabber item in physGrabObject.playerGrabbing)
		{
			if (flag4)
			{
				if (item.playerAvatar.isCrouching)
				{
					flag2 = true;
				}
				if (item.playerAvatar.isCrawling)
				{
					flag3 = true;
				}
				flag4 = false;
			}
			if (item.isRotating)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			physGrabObject.TurnXYZ(turnX, turnY, identity);
		}
		float num = grabVerticalOffset;
		if (flag2)
		{
			num += 0.5f;
		}
		if (flag3)
		{
			num -= 0.5f;
		}
		physGrabObject.OverrideGrabVerticalPosition(num);
		if (!flag)
		{
			if (stateCurrent == State.OutOfAmmo)
			{
				physGrabObject.OverrideTorqueStrength(0.01f);
				physGrabObject.OverrideExtraTorqueStrengthDisable();
				physGrabObject.OverrideExtraGrabStrengthDisable();
			}
			else if (physGrabObject.grabbed)
			{
				physGrabObject.OverrideTorqueStrength(12f);
				physGrabObject.OverrideAngularDrag(20f);
			}
		}
		if (flag)
		{
			physGrabObject.OverrideTorqueStrength(2f);
			physGrabObject.OverrideAngularDrag(20f);
		}
	}

	public void Misfire()
	{
		if (!roomVolumeCheck.inTruck && !physGrabObject.grabbed && !physGrabObject.hasNeverBeenGrabbed && SemiFunc.IsMasterClientOrSingleplayer() && (float)Random.Range(0, 100) < misfirePercentageChange)
		{
			Shoot();
		}
	}

	public void Shoot()
	{
		bool flag = false;
		if (itemBattery.batteryLifeInt <= 0)
		{
			flag = true;
		}
		if (Random.Range(0, 10000) == 0)
		{
			flag = false;
		}
		if (flag)
		{
			return;
		}
		if (hasOneShot)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("ShootRPC", RpcTarget.All);
			}
			else
			{
				ShootRPC();
			}
			StateSet(State.Reloading);
		}
		else if (hasBuildUp)
		{
			StateSet(State.Buildup);
		}
		else
		{
			StateSet(State.Shooting);
		}
	}

	private void MuzzleFlash()
	{
		Object.Instantiate(muzzleFlashPrefab, gunMuzzle.position, gunMuzzle.rotation, gunMuzzle).GetComponent<ItemGunMuzzleFlash>().ActivateAllEffects();
	}

	private void StartTriggerAnimation()
	{
		triggerAnimationActive = true;
		triggerAnimationEval = 0f;
	}

	[PunRPC]
	public void ShootRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		float distanceMin = 3f * cameraShakeMultiplier;
		float distanceMax = 16f * cameraShakeMultiplier;
		SemiFunc.CameraShakeImpactDistance(gunMuzzle.position, 5f * cameraShakeMultiplier, 0.1f, distanceMin, distanceMax);
		SemiFunc.CameraShakeDistance(gunMuzzle.position, 0.1f * cameraShakeMultiplier, 0.1f * cameraShakeMultiplier, distanceMin, distanceMax);
		soundShoot.Play(gunMuzzle.position);
		soundShootGlobal.Play(gunMuzzle.position);
		MuzzleFlash();
		StartTriggerAnimation();
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (investigateRadius > 0f)
		{
			EnemyDirector.instance.SetInvestigate(base.transform.position, investigateRadius);
		}
		physGrabObject.rb.AddForceAtPosition(-gunMuzzle.forward * gunRecoilForce, gunMuzzle.position, ForceMode.Impulse);
		if (!batteryDrainFullBar)
		{
			itemBattery.batteryLife -= batteryDrain;
		}
		else
		{
			itemBattery.RemoveFullBar(batteryDrainFullBars);
		}
		for (int i = 0; i < numberOfBullets; i++)
		{
			Vector3 endPosition = gunMuzzle.position;
			bool hit = false;
			bool flag = false;
			Vector3 vector = gunMuzzle.forward;
			if (gunRandomSpread > 0f)
			{
				float angle = Random.Range(0f, gunRandomSpread / 2f);
				float angle2 = Random.Range(0f, 360f);
				Vector3 normalized = Vector3.Cross(vector, Random.onUnitSphere).normalized;
				Quaternion quaternion = Quaternion.AngleAxis(angle, normalized);
				vector = (Quaternion.AngleAxis(angle2, vector) * quaternion * vector).normalized;
			}
			if (Physics.Raycast(gunMuzzle.position, vector, out var hitInfo, gunRange, (int)SemiFunc.LayerMaskGetVisionObstruct() + LayerMask.GetMask("Enemy")))
			{
				endPosition = hitInfo.point;
				hit = true;
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				endPosition = gunMuzzle.position + gunMuzzle.forward * gunRange;
				hit = true;
			}
			ShootBullet(endPosition, hit);
		}
	}

	private void ShootBullet(Vector3 _endPosition, bool _hit)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("ShootBulletRPC", RpcTarget.All, _endPosition, _hit);
			}
			else
			{
				ShootBulletRPC(_endPosition, _hit);
			}
		}
	}

	[PunRPC]
	public void ShootBulletRPC(Vector3 _endPosition, bool _hit, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			if (physGrabObject.playerGrabbing.Count > 1 && physGrabObject.grabbedLocal)
			{
				PlayerAvatar.instance.physGrabber.OverrideGrabRelease(photonView.ViewID);
			}
			ItemGunBullet component = Object.Instantiate(bulletPrefab, gunMuzzle.position, gunMuzzle.rotation).GetComponent<ItemGunBullet>();
			component.hitPosition = _endPosition;
			component.bulletHit = _hit;
			hurtCollider = component.GetComponentInChildren<HurtCollider>();
			soundHit.Play(_endPosition);
			component.shootLineWidthCurve = shootLineWidthCurve;
			component.ActivateAll();
		}
	}

	private void StateSet(State _state)
	{
		if (_state == stateCurrent)
		{
			return;
		}
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("StateSetRPC", RpcTarget.All, (int)_state);
			}
		}
		else
		{
			StateSetRPC((int)_state);
		}
	}

	private void ShootLogic()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && itemToggle.toggleState != prevToggleState)
		{
			if (itemBattery.batteryLifeInt <= 0)
			{
				soundNoAmmoClick.Play(base.transform.position);
				StartTriggerAnimation();
				SemiFunc.CameraShakeImpact(1f, 0.1f);
				physGrabObject.rb.AddForceAtPosition(-gunMuzzle.forward * 1f, gunMuzzle.position, ForceMode.Impulse);
			}
			else
			{
				Shoot();
			}
			prevToggleState = itemToggle.toggleState;
		}
	}

	[PunRPC]
	private void StateSetRPC(int state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			stateStart = true;
			statePrev = stateCurrent;
			stateCurrent = (State)state;
		}
	}

	private void StateMachine(bool _fixedUpdate)
	{
		switch (stateCurrent)
		{
		case State.Idle:
			StateIdle(_fixedUpdate);
			break;
		case State.OutOfAmmo:
			StateOutOfAmmo(_fixedUpdate);
			break;
		case State.Buildup:
			StateBuildup(_fixedUpdate);
			break;
		case State.Shooting:
			StateShooting(_fixedUpdate);
			break;
		case State.Reloading:
			StateReloading(_fixedUpdate);
			break;
		}
	}

	private void StateIdle(bool _fixedUpdate)
	{
		if (stateStart && !_fixedUpdate)
		{
			if (onStateIdleStart != null)
			{
				onStateIdleStart.Invoke();
			}
			stateStart = false;
			prevToggleState = itemToggle.toggleState;
		}
		if (!_fixedUpdate)
		{
			ShootLogic();
			if (hasIdleUpdate)
			{
				onStateIdleUpdate.Invoke();
			}
		}
		if (_fixedUpdate && hasIdleFixedUpdate)
		{
			onStateIdleFixedUpdate.Invoke();
		}
	}

	private void StateOutOfAmmo(bool _fixedUpdate)
	{
		if (stateStart && !_fixedUpdate)
		{
			if (onStateOutOfAmmoStart != null)
			{
				onStateOutOfAmmoStart.Invoke();
			}
			stateStart = false;
			prevToggleState = itemToggle.toggleState;
		}
		if (!_fixedUpdate)
		{
			if (itemBattery.batteryLifeInt > 0)
			{
				StateSet(State.Idle);
				return;
			}
			ShootLogic();
			if (hasOutOfAmmoUpdate)
			{
				onStateOutOfAmmoUpdate.Invoke();
			}
		}
		if (_fixedUpdate && hasOutOfAmmoFixedUpdate)
		{
			onStateOutOfAmmoFixedUpdate.Invoke();
		}
	}

	private void StateBuildup(bool _fixedUpdate)
	{
		if (stateStart && !_fixedUpdate)
		{
			if (onStateBuildupStart != null)
			{
				onStateBuildupStart.Invoke();
			}
			stateTimer = 0f;
			stateTimeMax = buildUpTime;
			stateStart = false;
		}
		if (!_fixedUpdate)
		{
			if (hasBuildupUpdate)
			{
				onStateBuildupUpdate.Invoke();
			}
			stateTimer += Time.deltaTime;
			if ((bool)itemEquippable && itemEquippable.isEquipped)
			{
				StateSet(State.Idle);
			}
			if (stateTimer >= stateTimeMax && itemBattery.batteryLifeInt > 0)
			{
				StateSet(State.Shooting);
			}
		}
		if (_fixedUpdate && hasBuildupFixedUpdate)
		{
			onStateBuildupFixedUpdate.Invoke();
		}
	}

	private void StateShooting(bool _fixedUpdate)
	{
		if (stateStart && !_fixedUpdate)
		{
			stateStart = false;
			if (onStateShootingStart != null)
			{
				onStateShootingStart.Invoke();
			}
			if (!hasOneShot)
			{
				stateTimeMax = shootTime;
				stateTimer = 0f;
				investigateCooldown = 0f;
			}
			else
			{
				stateTimer = 0.001f;
			}
			if (itemBattery.batteryLifeInt > 0)
			{
				itemBattery.RemoveFullBar(1);
			}
		}
		if (!_fixedUpdate)
		{
			if (investigateRadius > 0f)
			{
				if (investigateCooldown <= 0f)
				{
					EnemyDirector.instance.SetInvestigate(base.transform.position, investigateRadius);
					investigateCooldown = 0.5f;
				}
				else
				{
					investigateCooldown -= Time.deltaTime;
				}
			}
			stateTimer += Time.deltaTime;
			if (stateTimer >= stateTimeMax || ((bool)itemEquippable && itemEquippable.isEquipped))
			{
				StateSet(State.Reloading);
			}
			if (hasShootingUpdate)
			{
				onStateShootingUpdate.Invoke();
			}
		}
		if (_fixedUpdate && hasShootingFixedUpdate)
		{
			onStateShootingFixedUpdate.Invoke();
		}
	}

	private void StateReloading(bool _fixedUpdate)
	{
		if (stateStart && !_fixedUpdate)
		{
			stateStart = false;
			if (onStateReloadingStart != null)
			{
				onStateReloadingStart.Invoke();
			}
			stateTimeMax = shootCooldown;
			stateTimer = 0f;
		}
		if (!_fixedUpdate)
		{
			stateTimer += Time.deltaTime;
			if (stateTimer >= stateTimeMax)
			{
				if (itemBattery.batteryLifeInt > 0)
				{
					StateSet(State.Idle);
				}
				else
				{
					StateSet(State.OutOfAmmo);
				}
			}
			if (hasReloadingUpdate)
			{
				onStateReloadingUpdate.Invoke();
			}
		}
		if (_fixedUpdate && hasReloadingFixedUpdate)
		{
			onStateReloadingFixedUpdate.Invoke();
		}
	}
}
