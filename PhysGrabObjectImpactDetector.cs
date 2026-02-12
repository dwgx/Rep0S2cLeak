using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class PhysGrabObjectImpactDetector : MonoBehaviour, IPunObservable
{
	public enum ImpactState
	{
		None,
		Light,
		Medium,
		Heavy
	}

	public bool particleDisable;

	[Range(0f, 4f)]
	public float particleMultiplier = 1f;

	[Space]
	public bool playerHurtDisable;

	public bool slidingDisable;

	public bool destroyDisable;

	internal int destroyDisableLaunches;

	internal float destroyDisableLaunchesTimer;

	internal bool destroyDisableTeleport = true;

	public bool indestructibleBreakEffects = true;

	public bool canHurtLogic = true;

	[HideInInspector]
	public PhysObjectParticles particles;

	private List<Transform> colliderTransforms = new List<Transform>();

	internal EnemyRigidbody enemyRigidbody;

	[HideInInspector]
	public bool isEnemy;

	internal float enemyInteractionTimer;

	private Rigidbody rb;

	private Materials.MaterialTrigger materialTrigger = new Materials.MaterialTrigger();

	[HideInInspector]
	public float fragility = 50f;

	[HideInInspector]
	public float durability = 100f;

	private float impactLevel1 = 300f;

	private float impactLevel2 = 400f;

	private float impactLevel3 = 500f;

	private float breakLevel1Cooldown;

	private float breakLevel2Cooldown;

	private float breakLevel3Cooldown;

	private float impactLightCooldown;

	private float impactMediumCooldown;

	private float impactHeavyCooldown;

	private Vector3 previousPosition;

	private Vector3 previousRotation;

	private Camera mainCamera;

	private float impactCooldown;

	internal bool isIndestructible;

	internal float impulseTimerDeactivateImpacts = 5f;

	internal float highestVelocity;

	internal float impactForce;

	internal float resetPrevPositionTimer;

	private PhysGrabObject physGrabObject;

	private PhotonView photonView;

	internal bool isHinge;

	internal bool isBrokenHinge;

	private ValuableObject valuableObject;

	private NotValuableObject notValuableObject;

	private bool isNotValuable;

	private bool breakLogic;

	[HideInInspector]
	public bool isValuable;

	private bool collisionsActive;

	private float collisionsActiveTimer;

	private float collisionActivatedBuffer;

	[HideInInspector]
	public bool isSliding;

	private float slidingTimer;

	private float slidingGain;

	private float slidingSpeedThreshold = 0.1f;

	private float slidingAudioSpeed;

	private Vector3 previousSlidingPosition;

	internal Vector3 previousVelocity;

	internal Vector3 previousAngularVelocity;

	internal Vector3 previousVelocityRaw;

	internal Vector3 previousPreviousVelocityRaw;

	private bool impactHappened;

	internal float impactDisabledTimer;

	private Vector3 contactPoint;

	internal PhysAudio impactAudio;

	private float impactAudioPitch = 1f;

	private bool audioActive;

	private float colliderVolume;

	private float timerInCart;

	private float timerInSafeArea;

	internal int breakLevelHeavy;

	internal int breakLevelMedium = 1;

	internal int breakLevelLight = 2;

	private Vector3 prevPos;

	private Quaternion prevRot;

	private bool isMoving;

	private float breakForce;

	private Vector3 originalPosition;

	private Quaternion originalRotation;

	public UnityEvent onAllImpacts;

	public UnityEvent onImpactLight;

	public UnityEvent onImpactMedium;

	public UnityEvent onImpactHeavy;

	[Space(15f)]
	public UnityEvent onAllBreaks;

	public UnityEvent onBreakLight;

	public UnityEvent onBreakMedium;

	public UnityEvent onBreakHeavy;

	[Space(15f)]
	public UnityEvent onDestroy;

	[HideInInspector]
	public bool inCart;

	private bool inCartPrevious;

	[HideInInspector]
	public bool isCart;

	private PhysGrabCart cart;

	private float inCartVolumeMultiplier;

	private float impactCheckTimer;

	internal PhysGrabCart currentCart;

	internal PhysGrabCart currentCartPrev;

	internal float indestructibleSpawnTimer = 5f;

	internal bool isColliding;

	private float isCollidingTimer;

	[HideInInspector]
	public float fragilityMultiplier = 1f;

	[HideInInspector]
	public float impactFragilityMultiplier = 1f;

	private float playerHurtMultiplier = 1f;

	private float playerHurtMultiplierTimer;

	[Space(15f)]
	public bool centerPointNeedsToBeInsideCart;

	internal Vector3 centerPoint = Vector3.zero;

	private bool playerHitDisable;

	private float playerHitDisableTimer;

	private void Start()
	{
		inCartVolumeMultiplier = 0.6f;
		if ((bool)GetComponent<PhysGrabHinge>())
		{
			isHinge = true;
		}
		cart = GetComponent<PhysGrabCart>();
		if ((bool)cart)
		{
			isCart = true;
		}
		enemyRigidbody = GetComponent<EnemyRigidbody>();
		if ((bool)enemyRigidbody)
		{
			isEnemy = true;
		}
		previousSlidingPosition = base.transform.position;
		valuableObject = GetComponent<ValuableObject>();
		Transform transform = base.transform.Find("ForceCenterPoint");
		if ((bool)transform)
		{
			centerPoint = transform.position;
		}
		if ((bool)valuableObject)
		{
			isValuable = true;
			breakLogic = true;
			fragility = valuableObject.durabilityPreset.fragility;
			durability = valuableObject.durabilityPreset.durability;
			impactAudio = valuableObject.audioPreset;
			impactAudioPitch = valuableObject.audioPresetPitch;
		}
		else
		{
			notValuableObject = GetComponent<NotValuableObject>();
			isNotValuable = true;
			if ((bool)notValuableObject)
			{
				if ((bool)notValuableObject.durabilityPreset)
				{
					breakLogic = true;
					fragility = notValuableObject.durabilityPreset.fragility;
					durability = notValuableObject.durabilityPreset.durability;
				}
				impactAudio = notValuableObject.audioPreset;
				impactAudioPitch = notValuableObject.audioPresetPitch;
			}
		}
		if ((bool)impactAudio)
		{
			audioActive = true;
		}
		else
		{
			audioActive = false;
		}
		photonView = GetComponent<PhotonView>();
		physGrabObject = GetComponent<PhysGrabObject>();
		rb = GetComponent<Rigidbody>();
		mainCamera = Camera.main;
		ColliderGet(base.transform);
		colliderVolume /= 200000f;
		GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>("Phys Object Particles"), new Vector3(0f, 0f, 0f), Quaternion.identity);
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		particles = gameObject.GetComponent<PhysObjectParticles>();
		particles.multiplier = particleMultiplier;
		if (isValuable)
		{
			particles.gradient = valuableObject.particleColors;
		}
		if ((bool)notValuableObject)
		{
			particles.gradient = notValuableObject.particleColors;
		}
		particles.colliderTransforms = colliderTransforms;
		originalPosition = rb.position;
		originalRotation = rb.rotation;
		materialTrigger.OverrideLayerMask = true;
		materialTrigger.LayerMask = LayerMask.GetMask("Default", "PlayerOnlyCollision");
	}

	private void ColliderGet(Transform transform)
	{
		if (transform.CompareTag("Phys Grab Object") && (bool)transform.GetComponent<Collider>())
		{
			colliderTransforms.Add(transform);
			Bounds bounds = transform.transform.GetComponent<Collider>().bounds;
			float num = bounds.size.x * 100f * (bounds.size.y * 100f) * (bounds.size.z * 100f);
			if ((bool)transform.GetComponent<SphereCollider>())
			{
				num *= 0.55f;
			}
			colliderVolume += num;
		}
		foreach (Transform item in transform)
		{
			ColliderGet(item);
		}
	}

	[PunRPC]
	private void InCartRPC(bool inCartState, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			inCart = inCartState;
		}
	}

	private void IndestructibleSpawnTimer()
	{
		if (indestructibleSpawnTimer > 0f)
		{
			physGrabObject.OverrideIndestructible();
			indestructibleSpawnTimer -= Time.deltaTime;
		}
	}

	private void Update()
	{
		IndestructibleSpawnTimer();
		if (GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		if (inCartPrevious != inCart)
		{
			inCartPrevious = inCart;
			if (GameManager.instance.gameMode == 1)
			{
				photonView.RPC("InCartRPC", RpcTarget.Others, inCart);
			}
		}
		bool flag = false;
		if (timerInCart > 0f)
		{
			flag = true;
			timerInCart -= Time.deltaTime;
		}
		else if ((bool)currentCart)
		{
			currentCartPrev = currentCart;
			currentCart = null;
		}
		if (timerInSafeArea > 0f)
		{
			flag = true;
			timerInSafeArea -= Time.deltaTime;
		}
		inCart = flag;
		if (isValuable && !valuableObject.dollarValueSet)
		{
			return;
		}
		if (isCollidingTimer > 0f)
		{
			isColliding = true;
			isCollidingTimer -= Time.deltaTime;
		}
		else
		{
			isColliding = false;
		}
		if (isSliding)
		{
			Vector3 vector = previousSlidingPosition;
			vector.y = 0f;
			Vector3 position = base.transform.position;
			position.y = 0f;
			float num = (position - vector).magnitude / Time.deltaTime;
			if (num >= slidingSpeedThreshold)
			{
				slidingAudioSpeed = Mathf.Lerp(slidingAudioSpeed, 1f + num * 0.01f, 10f * Time.deltaTime);
				Materials.Instance.SlideLoop(rb.worldCenterOfMass, materialTrigger, 1f, 1f + slidingAudioSpeed);
			}
			if (GameManager.instance.gameMode == 0 || PhotonNetwork.IsMasterClient)
			{
				slidingTimer -= Time.deltaTime;
				if (slidingTimer < 0f)
				{
					isSliding = false;
				}
			}
		}
		previousSlidingPosition = base.transform.position;
		if (playerHurtMultiplierTimer > 0f)
		{
			playerHurtMultiplierTimer -= Time.deltaTime;
			if (playerHurtMultiplierTimer <= 0f)
			{
				playerHurtMultiplier = 1f;
			}
		}
		if (physGrabObject.grabbed)
		{
			collisionsActiveTimer = 0.5f;
		}
		if (rb.velocity.magnitude > 0.01f || rb.angularVelocity.magnitude > 0.1f)
		{
			collisionsActiveTimer = 0.5f;
		}
		if (collisionsActiveTimer > 0f)
		{
			if (!collisionsActive)
			{
				collisionActivatedBuffer = 0.1f;
			}
			collisionsActive = true;
			collisionsActiveTimer -= Time.deltaTime;
		}
		else
		{
			collisionsActive = false;
		}
		if (collisionActivatedBuffer > 0f)
		{
			collisionActivatedBuffer -= Time.deltaTime;
		}
		if (playerHitDisableTimer > 0f)
		{
			playerHitDisableTimer -= Time.deltaTime;
		}
		else if (playerHitDisable)
		{
			playerHitDisable = false;
			physGrabObject.PhysRidingDisabledSet(_state: false);
		}
		if (breakLevel1Cooldown > 0f)
		{
			breakLevel1Cooldown -= Time.deltaTime;
		}
		if (breakLevel2Cooldown > 0f)
		{
			breakLevel2Cooldown -= Time.deltaTime;
		}
		if (breakLevel3Cooldown > 0f)
		{
			breakLevel3Cooldown -= Time.deltaTime;
		}
		if (impactLightCooldown > 0f)
		{
			impactLightCooldown -= Time.deltaTime;
		}
		if (impactMediumCooldown > 0f)
		{
			impactMediumCooldown -= Time.deltaTime;
		}
		if (impactHeavyCooldown > 0f)
		{
			impactHeavyCooldown -= Time.deltaTime;
		}
		if (impactCooldown > 0f)
		{
			impactCooldown -= Time.deltaTime;
		}
		if (impulseTimerDeactivateImpacts > 0f)
		{
			impulseTimerDeactivateImpacts -= Time.deltaTime;
		}
		if (resetPrevPositionTimer > 0f)
		{
			resetPrevPositionTimer -= Time.deltaTime;
			previousPosition = Vector3.zero;
		}
		if (enemyInteractionTimer > 0f)
		{
			enemyInteractionTimer -= Time.deltaTime;
		}
		if (destroyDisableLaunchesTimer > 0f)
		{
			destroyDisableLaunchesTimer -= Time.deltaTime;
			if (destroyDisableLaunchesTimer <= 0f)
			{
				destroyDisableLaunches = 0;
			}
		}
	}

	private void FixedUpdate()
	{
		if (GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		if (inCart && !isEnemy && physGrabObject.playerGrabbing.Count == 0 && (bool)currentCart && !rb.isKinematic && !GetComponent<PlayerTumble>())
		{
			PhysGrabCart component = currentCart.GetComponent<PhysGrabCart>();
			if (component.actualVelocity.magnitude > 1f)
			{
				Vector3 velocity = rb.velocity;
				rb.velocity = Vector3.Lerp(rb.velocity, component.actualVelocity, 30f * Time.fixedDeltaTime);
				if (rb.velocity.y > velocity.y)
				{
					rb.velocity = new Vector3(rb.velocity.x, velocity.y, rb.velocity.z);
				}
			}
		}
		impactHappened = false;
		breakForce = 0f;
		impactForce = 0f;
		if (impactDisabledTimer <= 0f)
		{
			Vector3 vector = rb.velocity / Time.fixedDeltaTime;
			Vector3 vector2 = rb.angularVelocity / Time.fixedDeltaTime;
			float magnitude = previousVelocity.magnitude;
			float num = Mathf.Abs(magnitude - vector.magnitude);
			float magnitude2 = previousAngularVelocity.magnitude;
			float num2 = Mathf.Abs(magnitude2 - vector2.magnitude);
			Vector3 normalized = vector.normalized;
			Vector3 normalized2 = previousVelocity.normalized;
			float num3 = Vector3.Angle(normalized, normalized2);
			Vector3 normalized3 = vector2.normalized;
			Vector3 normalized4 = previousAngularVelocity.normalized;
			float num4 = Vector3.Angle(normalized3, normalized4);
			num *= 1f;
			num2 *= 0.4f * rb.mass;
			num3 *= 0.2f;
			num4 *= 0.02f * rb.mass;
			if ((num > 1f && magnitude > 1f) || (num2 > 1f && magnitude2 > 1f) || (num3 > 1f && magnitude > 1f) || (num4 > 1f && magnitude2 > 1f))
			{
				impactHappened = true;
				float num5 = num * 2f;
				float num6 = Mathf.Max(rb.mass, 1f);
				breakForce += num5 * num6;
			}
			breakForce *= 8f;
			impactForce = breakForce / 8f * impactFragilityMultiplier;
			breakForce = breakForce * (fragility / 100f) * fragilityMultiplier;
			if (impactHappened)
			{
				if (inCart)
				{
					breakForce = 0f;
				}
				if (inCart || isCart)
				{
					impactForce *= 0.3f;
				}
			}
		}
		else
		{
			impactDisabledTimer -= Time.fixedDeltaTime;
		}
		previousPreviousVelocityRaw = previousVelocityRaw;
		previousVelocityRaw = rb.velocity;
		previousVelocity = rb.velocity / Time.fixedDeltaTime;
		previousAngularVelocity = rb.angularVelocity / Time.fixedDeltaTime;
		if (Vector3.Distance(prevPos, base.transform.position) > 0.01f || Quaternion.Angle(prevRot, base.transform.rotation) > 0.1f)
		{
			isMoving = true;
		}
		prevPos = base.transform.position;
		prevRot = base.transform.rotation;
	}

	public void ImpactDisable(float time)
	{
		impactDisabledTimer = time;
	}

	private void EnemyInvestigate(float radius)
	{
		if (!(physGrabObject.enemyInteractTimer > 0f) && !inCart && !isCart)
		{
			EnemyDirector.instance.SetInvestigate(base.transform.position, radius);
		}
	}

	public void DestroyObject(bool effects = true)
	{
		if (destroyDisable || !SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		EnemyRigidbody component = GetComponent<EnemyRigidbody>();
		if ((bool)component)
		{
			if (component.enemy.HasHealth)
			{
				component.enemy.Health.Hurt(999999, physGrabObject.centerPoint);
			}
			else
			{
				component.enemy.EnemyParent.Despawn();
			}
		}
		else if (!physGrabObject.dead)
		{
			physGrabObject.dead = true;
			EnemyInvestigate(15f);
			if (!SemiFunc.IsMultiplayer())
			{
				DestroyObjectRPC(effects);
			}
			else if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("DestroyObjectRPC", RpcTarget.All, effects);
			}
		}
	}

	[PunRPC]
	public void DestroyObjectRPC(bool effects, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		physGrabObject.dead = true;
		if (effects)
		{
			GameDirector.instance.CameraImpact.ShakeDistance(10f, 1f, 6f, base.transform.position, 0.1f);
		}
		if ((bool)particles)
		{
			particles.transform.parent = null;
			particles.DestroyParticles();
		}
		if (audioActive && effects)
		{
			AudioSource audioSource = impactAudio.destroy.Play(physGrabObject.centerPoint);
			if ((bool)audioSource)
			{
				audioSource.pitch *= impactAudioPitch;
			}
		}
		onDestroy.Invoke();
	}

	public void BreakHeavy(Vector3 contactPoint, bool _forceBreak = false, float minimumValue = 0f)
	{
		float num = 0.1f * (1f + 9f * (100f - durability) / 100f);
		bool flag = false;
		if (isValuable || breakLogic)
		{
			float valueLost = 0f;
			if (isValuable)
			{
				valueLost = Mathf.Round(valuableObject.dollarValueOriginal * num);
				valueLost += Mathf.Round(Random.Range((0f - valueLost) * 0.1f, valueLost * 0.1f));
				if (minimumValue != 0f)
				{
					valueLost = Mathf.Max(valueLost, minimumValue);
				}
				valueLost = Mathf.Clamp(valueLost, 1f, valuableObject.dollarValueCurrent);
			}
			Break(valueLost, contactPoint, breakLevelHeavy, _forceBreak);
			flag = true;
		}
		if (isNotValuable && notValuableObject.hasHealth)
		{
			notValuableObject.Impact(ImpactState.Heavy);
			flag = true;
		}
		if (flag)
		{
			EnemyInvestigate(10f);
		}
		breakLevel3Cooldown = 0.6f;
		breakLevel2Cooldown = 0.4f;
		breakLevel1Cooldown = 0.3f;
	}

	public void BreakMedium(Vector3 contactPoint, bool _forceBreak = false)
	{
		float num = 0.05f * (1f + 9f * (100f - durability) / 100f);
		bool flag = false;
		if (isValuable || breakLogic)
		{
			float valueLost = 0f;
			if (isValuable)
			{
				valueLost = Mathf.Round(valuableObject.dollarValueOriginal * num);
				valueLost += Mathf.Round(Random.Range((0f - valueLost) * 0.1f, valueLost * 0.1f));
				valueLost = Mathf.Clamp(valueLost, 0f, valuableObject.dollarValueCurrent);
			}
			Break(valueLost, contactPoint, breakLevelMedium, _forceBreak);
			flag = true;
		}
		if (isNotValuable && notValuableObject.hasHealth)
		{
			notValuableObject.Impact(ImpactState.Medium);
			flag = true;
		}
		if (flag)
		{
			EnemyInvestigate(5f);
		}
		breakLevel2Cooldown = 0.4f;
		breakLevel1Cooldown = 0.3f;
	}

	public void BreakLight(Vector3 contactPoint, bool _forceBreak = false)
	{
		float num = 0.01f * (1f + 9f * (100f - durability) / 100f);
		bool flag = false;
		if (isValuable || breakLogic)
		{
			float valueLost = 0f;
			if (isValuable)
			{
				valueLost = Mathf.Round(valuableObject.dollarValueOriginal * num);
				valueLost += Mathf.Round(Random.Range((0f - valueLost) * 0.1f, valueLost * 0.1f));
				valueLost = Mathf.Clamp(valueLost, 0f, valuableObject.dollarValueCurrent);
			}
			Break(valueLost, contactPoint, breakLevelLight, _forceBreak);
			flag = true;
		}
		if (isNotValuable && notValuableObject.hasHealth)
		{
			notValuableObject.Impact(ImpactState.Light);
			flag = true;
		}
		if (flag)
		{
			EnemyInvestigate(3f);
		}
		breakLevel1Cooldown = 0.3f;
	}

	internal void Break(float valueLost, Vector3 _contactPoint, int breakLevel, bool _forceBreak = false)
	{
		bool flag = false;
		if (isValuable && (_forceBreak || (!isIndestructible && !destroyDisable)))
		{
			flag = true;
		}
		if (GameManager.instance.gameMode == 0)
		{
			BreakRPC(valueLost, _contactPoint, breakLevel, flag);
			return;
		}
		photonView.RPC("BreakRPC", RpcTarget.All, valueLost, _contactPoint, breakLevel, flag);
	}

	private void HealLogic(float healAmount, Vector3 healingPoint)
	{
		valuableObject.dollarValueCurrent += Mathf.Floor(healAmount);
		valuableObject.dollarValueCurrent = Mathf.Clamp(valuableObject.dollarValueCurrent, 0f, valuableObject.dollarValueOriginal);
	}

	public float Heal(float healPercent, Vector3 healingPoint)
	{
		float result = 0f;
		if (isValuable)
		{
			if (GameManager.Multiplayer())
			{
				if (PhotonNetwork.IsMasterClient)
				{
					float value = valuableObject.dollarValueOriginal * healPercent;
					value = Mathf.Clamp(value, 0f, valuableObject.dollarValueOriginal - valuableObject.dollarValueCurrent);
					if (value > 0f)
					{
						photonView.RPC("HealRPC", RpcTarget.All, valuableObject.dollarValueOriginal * healPercent);
					}
					result = value;
				}
			}
			else
			{
				float value2 = valuableObject.dollarValueOriginal * healPercent;
				value2 = Mathf.Clamp(value2, 0f, valuableObject.dollarValueOriginal - valuableObject.dollarValueCurrent);
				if (value2 > 0f)
				{
					HealLogic(value2, healingPoint);
				}
				result = value2;
			}
		}
		return result;
	}

	public void PlayerHitDisableSet()
	{
		if (!playerHitDisable)
		{
			physGrabObject.PhysRidingDisabledSet(_state: true);
			playerHitDisable = true;
		}
		playerHitDisableTimer = 0.5f;
	}

	[PunRPC]
	private void HealRPC(float healAmount, Vector3 healingPoint)
	{
		HealLogic(healAmount, healingPoint);
	}

	private void ResetObject()
	{
		foreach (PhysGrabber item in physGrabObject.playerGrabbing.ToList())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				item.ReleaseObject(photonView.ViewID);
				continue;
			}
			item.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, 0.1f, photonView.ViewID);
		}
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		valuableObject.dollarValueCurrent = valuableObject.dollarValueOriginal;
		rb.position = originalPosition;
		rb.rotation = originalRotation;
		base.transform.position = originalPosition;
		AssetManager.instance.soundUnequip.Play(originalPosition);
		BreakEffect(breakLevelLight, originalPosition);
		Vector3 position = physGrabObject.transform.TransformPoint(physGrabObject.midPointOffset);
		Object.Instantiate(AssetManager.instance.prefabTeleportEffect, position, Quaternion.identity).transform.localScale = Vector3.one * 2f;
	}

	[PunRPC]
	private void BreakRPC(float valueLost, Vector3 _contactPoint, int breakLevel, bool _loseValue, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		if (_loseValue)
		{
			if ((bool)valuableObject)
			{
				float dollarValueCurrent = valuableObject.dollarValueCurrent;
				valuableObject.dollarValueCurrent -= valueLost;
				bool flag = false;
				if (valuableObject.dollarValueCurrent < valuableObject.dollarValueOriginal * 0.15f)
				{
					if (!SemiFunc.RunIsTutorial())
					{
						DestroyObject();
					}
					else
					{
						if ((bool)particles)
						{
							particles.DestroyParticles();
						}
						ResetObject();
						ImpactHeavy(1000f, _contactPoint);
					}
					flag = true;
				}
				if (flag)
				{
					valueLost = dollarValueCurrent;
				}
			}
			WorldSpaceUIParent.instance.ValueLostCreate(_contactPoint, (int)valueLost);
		}
		if (_loseValue || !valuableObject)
		{
			onAllBreaks.Invoke();
		}
		if (breakLevel == breakLevelHeavy)
		{
			if (_loseValue || !valuableObject)
			{
				onBreakHeavy.Invoke();
			}
			if ((bool)physGrabObject)
			{
				physGrabObject.heavyBreakImpulse = false;
			}
		}
		if (breakLevel == breakLevelMedium)
		{
			if (_loseValue || !valuableObject)
			{
				onBreakMedium.Invoke();
			}
			if ((bool)physGrabObject)
			{
				physGrabObject.mediumBreakImpulse = false;
			}
		}
		if (breakLevel == breakLevelLight)
		{
			if (_loseValue || !valuableObject)
			{
				onBreakLight.Invoke();
			}
			if ((bool)physGrabObject)
			{
				physGrabObject.lightBreakImpulse = false;
			}
		}
		BreakEffect(breakLevel, _contactPoint);
	}

	public void BreakEffect(int breakLevel, Vector3 contactPoint)
	{
		if (!particleDisable && (bool)particles)
		{
			particles.ImpactSmoke(5, contactPoint, colliderVolume);
		}
		if (breakLevel == breakLevelHeavy)
		{
			if (audioActive && (bool)impactAudio)
			{
				impactAudio.breakHeavy.Play(contactPoint);
			}
			if ((bool)physGrabObject)
			{
				SemiFunc.PlayerEyesOverrideSoft(physGrabObject.centerPoint, 1f, base.gameObject, 10f);
			}
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, contactPoint, 0.1f);
			MaterialImpact(contactPoint, Materials.SoundType.Heavy);
		}
		if (breakLevel == breakLevelMedium)
		{
			if (audioActive && (bool)impactAudio)
			{
				AudioSource audioSource = impactAudio.breakMedium.Play(contactPoint);
				if ((bool)audioSource)
				{
					audioSource.pitch *= impactAudioPitch;
				}
			}
			if ((bool)physGrabObject)
			{
				SemiFunc.PlayerEyesOverrideSoft(physGrabObject.centerPoint, 1f, base.gameObject, 5f);
			}
			GameDirector.instance.CameraImpact.ShakeDistance(3f, 1f, 6f, contactPoint, 0.1f);
			MaterialImpact(contactPoint, Materials.SoundType.Medium);
		}
		if (breakLevel != breakLevelLight)
		{
			return;
		}
		if (audioActive && (bool)impactAudio)
		{
			AudioSource audioSource2 = impactAudio.breakLight.Play(contactPoint);
			if ((bool)audioSource2)
			{
				audioSource2.pitch *= impactAudioPitch;
			}
		}
		if ((bool)physGrabObject)
		{
			SemiFunc.PlayerEyesOverrideSoft(physGrabObject.centerPoint, 1f, base.gameObject, 3f);
		}
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 1f, 6f, contactPoint, 0.1f);
		MaterialImpact(contactPoint, Materials.SoundType.Light);
	}

	public void ImpactHeavy(float force, Vector3 contactPoint)
	{
		if (GameManager.instance.gameMode == 0)
		{
			ImpactHeavyRPC(force, contactPoint);
		}
		else
		{
			photonView.RPC("ImpactHeavyRPC", RpcTarget.All, force, contactPoint);
		}
		physGrabObject.impactHappenedTimer = 0.1f;
		physGrabObject.impactHeavyTimer = 0.1f;
	}

	[PunRPC]
	private void ImpactHeavyRPC(float force, Vector3 contactPoint, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info) || !physGrabObject)
		{
			return;
		}
		SemiFunc.PlayerEyesOverrideSoft(physGrabObject.centerPoint, 1f, base.gameObject, 8f);
		if (audioActive && !isHinge && (bool)impactAudio)
		{
			float volumeMultiplier = ImpactSoundGetVolume(force, impactAudio.impactHeavy.Volume);
			AudioSource audioSource = impactAudio.impactHeavy.Play(contactPoint, volumeMultiplier);
			if ((bool)audioSource)
			{
				audioSource.pitch *= impactAudioPitch;
			}
		}
		if (!particleDisable && !inCart && (bool)particles)
		{
			particles.ImpactSmoke(5, contactPoint, colliderVolume);
		}
		onAllImpacts.Invoke();
		onImpactHeavy.Invoke();
		EnemyInvestigate(1f);
		physGrabObject.impactHappenedTimer = 0.1f;
		physGrabObject.impactHeavyTimer = 0.1f;
	}

	public void ImpactMedium(float force, Vector3 contactPoint)
	{
		if (GameManager.instance.gameMode == 0)
		{
			ImpactMediumRPC(force, contactPoint);
		}
		else
		{
			photonView.RPC("ImpactMediumRPC", RpcTarget.All, force, contactPoint);
		}
		physGrabObject.impactHappenedTimer = 0.1f;
		physGrabObject.impactMediumTimer = 0.1f;
	}

	[PunRPC]
	private void ImpactMediumRPC(float force, Vector3 contactPoint, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info) || !physGrabObject)
		{
			return;
		}
		SemiFunc.PlayerEyesOverrideSoft(physGrabObject.centerPoint, 1f, base.gameObject, 5f);
		if (audioActive && !isHinge && (bool)impactAudio)
		{
			float volumeMultiplier = ImpactSoundGetVolume(force, impactAudio.impactMedium.Volume);
			AudioSource audioSource = impactAudio.impactMedium.Play(contactPoint, volumeMultiplier);
			if ((bool)audioSource)
			{
				audioSource.pitch *= impactAudioPitch;
			}
		}
		onImpactMedium.Invoke();
		onAllImpacts.Invoke();
		if (!rb.isKinematic)
		{
			rb.angularVelocity *= 0.55f;
		}
		EnemyInvestigate(0.5f);
		physGrabObject.impactHappenedTimer = 0.1f;
		physGrabObject.impactMediumTimer = 0.1f;
	}

	private float ImpactSoundGetVolume(float force, float volume)
	{
		float num = Mathf.Clamp01(force * 0.01f);
		if (inCart)
		{
			num *= inCartVolumeMultiplier;
		}
		return Mathf.Clamp(num, 0.1f, 1f);
	}

	public void ImpactLight(float force, Vector3 contactPoint)
	{
		if (GameManager.instance.gameMode == 0)
		{
			ImpactLightRPC(force, contactPoint);
		}
		else
		{
			photonView.RPC("ImpactLightRPC", RpcTarget.All, force, contactPoint);
		}
		EnemyInvestigate(0.2f);
		physGrabObject.impactHappenedTimer = 0.1f;
		physGrabObject.impactLightTimer = 0.1f;
	}

	private void MaterialImpact(Vector3 _position, Materials.SoundType _type)
	{
		Materials.Instance.Impulse(_position + Vector3.up * 0.1f, Vector3.down, _type, footstep: false, footstepParticles: false, materialTrigger, Materials.HostType.Enemy);
	}

	[PunRPC]
	private void ImpactLightRPC(float force, Vector3 contactPoint, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info) || !physGrabObject)
		{
			return;
		}
		SemiFunc.PlayerEyesOverrideSoft(physGrabObject.centerPoint, 1f, base.gameObject, 3f);
		if (audioActive && !isHinge && (bool)impactAudio)
		{
			float num = ImpactSoundGetVolume(force, impactAudio.impactLight.Volume);
			if (inCart)
			{
				num *= inCartVolumeMultiplier;
			}
			AudioSource audioSource = impactAudio.impactLight.Play(contactPoint, num);
			if ((bool)audioSource)
			{
				audioSource.pitch *= impactAudioPitch;
			}
		}
		if (!rb.isKinematic)
		{
			rb.angularVelocity *= 0.6f;
		}
		onAllImpacts.Invoke();
		onImpactLight.Invoke();
		physGrabObject.impactHappenedTimer = 0.1f;
		physGrabObject.impactLightTimer = 0.1f;
	}

	private void OnTriggerStay(Collider other)
	{
		if ((GameManager.instance.gameMode != 0 && !PhotonNetwork.IsMasterClient) || !other.CompareTag("Cart"))
		{
			return;
		}
		bool flag = true;
		if (centerPointNeedsToBeInsideCart)
		{
			Vector3 point = base.transform.TransformPoint(centerPoint);
			Collider component = other.GetComponent<Collider>();
			if ((bool)component && !component.bounds.Contains(point))
			{
				flag = false;
			}
		}
		if (flag)
		{
			PhysGrabCart componentInParent = other.GetComponentInParent<PhysGrabCart>();
			if ((bool)componentInParent)
			{
				currentCartPrev = currentCart;
				currentCart = componentInParent;
				currentCart.physGrabInCart.Add(physGrabObject);
				timerInCart = 0.1f;
			}
			else
			{
				timerInSafeArea = 0.1f;
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if ((GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient) || !collisionsActive || !isMoving)
		{
			return;
		}
		isCollidingTimer = 0.1f;
		if (!isHinge && !slidingDisable && !isCart && !inCart && !isCart && isValuable && valuableObject.volumeType >= ValuableVolume.Type.Medium && (bool)collision.gameObject.GetComponent<MaterialSurface>())
		{
			if (rb.velocity.magnitude > slidingSpeedThreshold && (Mathf.Abs(rb.velocity.x) > Mathf.Abs(rb.velocity.y) || Mathf.Abs(rb.velocity.z) > Mathf.Abs(rb.velocity.y)))
			{
				isSliding = true;
				slidingTimer = 0.1f;
			}
			PhysGrabObject component = collision.gameObject.GetComponent<PhysGrabObject>();
			if ((bool)component && component.rb.velocity.magnitude > rb.velocity.magnitude * 0.8f)
			{
				isSliding = false;
			}
		}
		Vector3 zero = Vector3.zero;
		ContactPoint[] contacts = collision.contacts;
		foreach (ContactPoint contactPoint in contacts)
		{
			zero += contactPoint.point;
		}
		if (collision.contacts.Length != 0)
		{
			this.contactPoint = zero / collision.contacts.Length;
		}
		else
		{
			this.contactPoint = Vector3.zero;
		}
		PhysGrabObjectImpactDetector component2 = collision.gameObject.GetComponent<PhysGrabObjectImpactDetector>();
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		bool flag3 = false;
		flag3 = isCart && this.contactPoint.y < base.transform.position.y;
		if (impactHappened && (!isCart || !component2 || !component2.inCart) && !flag3)
		{
			if (impulseTimerDeactivateImpacts <= 0f)
			{
				if (impactForce > 150f && impactHeavyCooldown <= 0f)
				{
					flag2 = true;
					ImpactHeavy(impactForce, this.contactPoint);
					impactHeavyCooldown = 0.5f;
					impactMediumCooldown = 0.5f;
					impactLightCooldown = 0.5f;
				}
				if (impactForce > 80f && impactMediumCooldown <= 0f)
				{
					flag2 = true;
					ImpactMedium(impactForce, this.contactPoint);
					impactMediumCooldown = 0.5f;
					impactLightCooldown = 0.5f;
				}
				if (impactForce > 20f && impactLightCooldown <= 0f)
				{
					flag2 = true;
					ImpactLight(impactForce, this.contactPoint);
					impactLightCooldown = 0.5f;
				}
			}
			if (indestructibleSpawnTimer <= 0f)
			{
				float num2 = Mathf.Max(rb.mass, 1f);
				if (breakForce > impactLevel3 * num2 && breakLevel3Cooldown <= 0f && !inCart)
				{
					flag = true;
					num = 3;
				}
				if (breakForce > impactLevel2 * num2 && breakLevel2Cooldown <= 0f && !flag && !inCart)
				{
					flag = true;
					num = 2;
				}
				if (breakForce > impactLevel1 * num2 && breakLevel1Cooldown <= 0f && !flag && !inCart)
				{
					flag = true;
					num = 1;
				}
			}
		}
		bool flag4 = false;
		bool flag5 = false;
		if (flag && (!isEnemy || this.enemyRigidbody.enemy.IsStunned()))
		{
			flag4 = true;
		}
		if (flag2 && (!isEnemy || this.enemyRigidbody.enemy.IsStunned()))
		{
			flag4 = true;
		}
		if (flag && isBrokenHinge)
		{
			flag5 = true;
		}
		if (!canHurtLogic)
		{
			flag4 = false;
		}
		bool flag6 = false;
		if (playerHitDisable && (collision.transform.CompareTag("Player") || (bool)collision.transform.GetComponent<PlayerTumble>()))
		{
			playerHitDisableTimer = 0.5f;
		}
		if (!playerHitDisable && flag4 && (flag || (flag2 && isCart)))
		{
			bool flag7 = false;
			PlayerTumble playerTumble = null;
			if (collision.transform.CompareTag("Player"))
			{
				flag7 = true;
			}
			else
			{
				playerTumble = collision.transform.GetComponent<PlayerTumble>();
				if ((bool)playerTumble)
				{
					flag7 = true;
				}
			}
			if (flag7 && isCart)
			{
				if (physGrabObject.playerGrabbing.Count <= 0)
				{
					flag7 = false;
				}
				else if (Vector3.Distance(cart.inCart.GetComponent<BoxCollider>().ClosestPoint(collision.transform.position), collision.transform.position) < 0.01f)
				{
					flag7 = false;
				}
			}
			if (flag7)
			{
				PlayerController componentInParent = collision.transform.GetComponentInParent<PlayerController>();
				PlayerAvatar playerAvatar;
				if ((bool)playerTumble)
				{
					playerAvatar = playerTumble.playerAvatar;
				}
				else if ((bool)componentInParent)
				{
					playerAvatar = componentInParent.playerAvatarScript;
				}
				else
				{
					playerAvatar = collision.transform.GetComponentInParent<PlayerAvatar>();
					if (!playerAvatar)
					{
						playerAvatar = collision.transform.GetComponent<PlayerAvatar>();
					}
					if (!playerAvatar)
					{
						playerAvatar = collision.transform.GetComponentInChildren<PlayerAvatar>();
					}
					if (!playerAvatar)
					{
						PlayerPhysPusher component3 = collision.transform.GetComponent<PlayerPhysPusher>();
						if ((bool)component3)
						{
							playerAvatar = component3.Player;
						}
					}
				}
				bool flag8 = false;
				foreach (PhysGrabber item in physGrabObject.playerGrabbing)
				{
					if (item.playerAvatar == playerAvatar)
					{
						flag8 = true;
						break;
					}
				}
				if ((bool)playerAvatar && !flag8)
				{
					Vector3 vector = playerAvatar.PlayerVisionTarget.VisionTransform.transform.position - this.contactPoint;
					float magnitude = previousPreviousVelocityRaw.magnitude;
					Vector3 direction = Vector3.Lerp(previousPreviousVelocityRaw.normalized, vector.normalized, 0f);
					if (magnitude >= 3f)
					{
						PlayerAvatar playerAvatar2 = null;
						RaycastHit[] array = rb.SweepTestAll(direction, 1f, QueryTriggerInteraction.Collide);
						foreach (RaycastHit raycastHit in array)
						{
							playerAvatar2 = ImpactGetPlayer(raycastHit.collider, componentInParent, playerTumble);
							if (playerAvatar2 == playerAvatar)
							{
								break;
							}
						}
						if (!playerAvatar2)
						{
							Collider[] array2 = Physics.OverlapSphere(this.contactPoint, 0.2f, LayerMask.GetMask("Player"));
							foreach (Collider hit in array2)
							{
								playerAvatar2 = ImpactGetPlayer(hit, componentInParent, playerTumble);
								if (playerAvatar2 == playerAvatar)
								{
									break;
								}
							}
						}
						if (playerAvatar2 == playerAvatar)
						{
							bool flag9 = false;
							if (!playerAvatar.isTumbling)
							{
								foreach (PhysGrabObject physGrabObject in playerAvatar.physObjectFinder.physGrabObjects)
								{
									if (physGrabObject == this.physGrabObject)
									{
										flag9 = true;
										break;
									}
								}
							}
							if (!flag9)
							{
								bool flag10 = false;
								float time = 0.1f;
								if (!playerHurtDisable && !isIndestructible && !destroyDisable && isValuable)
								{
									time = 0.15f;
									int damage = Mathf.RoundToInt((float)(5 * num) * (rb.mass * 0.5f) * playerHurtMultiplier);
									playerAvatar.playerHealth.HurtOther(damage, this.contactPoint, savingGrace: true);
									flag10 = true;
								}
								bool flag11 = false;
								if (isHinge)
								{
									if (magnitude >= 3f)
									{
										flag11 = true;
									}
								}
								else if (isCart)
								{
									if (magnitude >= 3f)
									{
										flag11 = true;
									}
								}
								else if (magnitude >= 6f)
								{
									flag11 = true;
								}
								if (flag10 || flag11)
								{
									if (!playerTumble)
									{
										playerTumble = playerAvatar.tumble;
									}
									playerTumble.TumbleRequest(_isTumbling: true, _playerInput: false);
									playerTumble.TumbleOverrideTime(2f);
									Vector3 force = vector.normalized * 4f * num;
									Vector3 torque = Vector3.Cross((playerAvatar.localCamera.transform.position - this.contactPoint).normalized, playerAvatar.transform.forward) * 5f * force.magnitude;
									playerTumble.physGrabObject.FreezeForces(time, force, torque);
									playerAvatar.playerHealth.HurtFreezeOverride(time);
									this.physGrabObject.FreezeForces(time, Vector3.zero, Vector3.zero);
									flag6 = true;
								}
							}
						}
					}
				}
			}
		}
		if ((flag4 || flag5) && !playerHurtDisable && enemyInteractionTimer <= 0f && !isIndestructible && !destroyDisable && (isValuable || isEnemy || flag5) && collision.transform.CompareTag("Enemy"))
		{
			EnemyRigidbody component4 = collision.transform.GetComponent<EnemyRigidbody>();
			if ((bool)component4 && component4.enemy.HasHealth && component4.enemy.Health.objectHurt && component4.enemy.Health.objectHurtDisableTimer <= 0f)
			{
				Vector3 vector2 = component4.physGrabObject.centerPoint - this.contactPoint;
				float magnitude2 = previousPreviousVelocityRaw.magnitude;
				Vector3 direction2 = Vector3.Lerp(previousPreviousVelocityRaw.normalized, vector2.normalized, 0.5f);
				if (magnitude2 > 2f)
				{
					EnemyRigidbody enemyRigidbody = null;
					RaycastHit[] array = rb.SweepTestAll(direction2, 1f, QueryTriggerInteraction.Collide);
					foreach (RaycastHit raycastHit2 in array)
					{
						enemyRigidbody = raycastHit2.transform.GetComponent<EnemyRigidbody>();
					}
					if (!enemyRigidbody)
					{
						Collider[] array2 = Physics.OverlapSphere(this.contactPoint, 0.2f, SemiFunc.LayerMaskGetPhysGrabObject());
						for (int i = 0; i < array2.Length; i++)
						{
							enemyRigidbody = array2[i].GetComponentInParent<EnemyRigidbody>();
						}
					}
					if (enemyRigidbody == component4)
					{
						flag6 = true;
						int num3 = Mathf.RoundToInt((float)(10 * num) * (rb.mass * 0.5f));
						num3 = Mathf.RoundToInt((float)num3 * component4.enemy.Health.objectHurtMultiplier);
						component4.enemy.Health.Hurt(num3, -vector2.normalized);
						if (isValuable)
						{
							float num4 = 0f;
							switch (valuableObject.volumeType)
							{
							case ValuableVolume.Type.Tiny:
								num4 = 0.2f;
								break;
							case ValuableVolume.Type.Small:
								num4 = 0.2f;
								break;
							case ValuableVolume.Type.Medium:
								num4 = 1f / 7f;
								break;
							case ValuableVolume.Type.Big:
								num4 = 0.1f;
								break;
							case ValuableVolume.Type.Wide:
								num4 = 0.1f;
								break;
							case ValuableVolume.Type.Tall:
								num4 = 0.1f;
								break;
							case ValuableVolume.Type.VeryTall:
								num4 = 0.1f;
								break;
							}
							num4 += Random.Range(-0.05f, 0.05f);
							BreakHeavy(this.contactPoint, _forceBreak: false, Mathf.CeilToInt(valuableObject.dollarValueOriginal * num4));
						}
						flag2 = false;
						flag = false;
						if (component4.enemy.Health.onObjectHurt != null)
						{
							if (this.physGrabObject.grabbedTimer > 0f)
							{
								component4.enemy.Health.onObjectHurtPlayer = this.physGrabObject.lastPlayerGrabbing;
							}
							else
							{
								component4.enemy.Health.onObjectHurtPlayer = null;
							}
							component4.enemy.Health.onObjectHurt.Invoke();
						}
						Vector3 force2 = vector2.normalized * (2f * (float)num);
						component4.rb.AddForce(force2, ForceMode.Impulse);
						Vector3 normalized = vector2.normalized;
						Vector3 rhs = -component4.rb.transform.up;
						Vector3 torque2 = Vector3.Cross(normalized, rhs) * (2f * (float)num);
						component4.rb.AddTorque(torque2, ForceMode.Impulse);
						EnemyType type = component4.enemy.Type;
						if (isValuable)
						{
							if (component4.enemy.HasStateStunned && component4.enemy.Health.objectHurtStun)
							{
								float mass = valuableObject.physAttributePreset.mass;
								float num5 = -1.5f;
								if (SemiFunc.MoonLevel() >= 1)
								{
									num5 = 0f;
								}
								bool flag12 = false;
								switch (type)
								{
								case EnemyType.VeryLight:
									if (mass >= 0.5f + num5)
									{
										flag12 = true;
									}
									break;
								case EnemyType.Light:
									if (mass >= 1f + num5)
									{
										flag12 = true;
									}
									break;
								case EnemyType.Medium:
									if (mass >= 2f + num5)
									{
										flag12 = true;
									}
									break;
								case EnemyType.Heavy:
									if (mass >= 3.5f + num5)
									{
										flag12 = true;
									}
									break;
								case EnemyType.VeryHeavy:
									if (mass >= 5f + num5)
									{
										flag12 = true;
									}
									break;
								}
								if (flag12)
								{
									component4.enemy.StateStunned.Set(2f);
								}
							}
						}
						else if (isBrokenHinge)
						{
							if (type <= EnemyType.Medium && component4.enemy.HasStateStunned && component4.enemy.Health.objectHurtStun)
							{
								component4.enemy.StateStunned.Set(2f);
							}
							DestroyObject();
						}
					}
				}
			}
		}
		if (flag6)
		{
			if (!SemiFunc.IsMultiplayer())
			{
				ImpactEffectRPC(this.contactPoint);
			}
			else
			{
				photonView.RPC("ImpactEffectRPC", RpcTarget.All, this.contactPoint);
			}
		}
		if (!flag || !(this.physGrabObject.overrideDisableBreakEffectsTimer <= 0f))
		{
			return;
		}
		if ((destroyDisable || isIndestructible || !isValuable) && !indestructibleBreakEffects)
		{
			if (!flag2)
			{
				if (num == 1)
				{
					ImpactLight(impactForce, this.contactPoint);
				}
				if (num == 2)
				{
					ImpactMedium(impactForce, this.contactPoint);
				}
				if (num == 3)
				{
					ImpactHeavy(impactForce, this.contactPoint);
				}
			}
		}
		else
		{
			if (num == 1)
			{
				BreakLight(this.contactPoint);
			}
			if (num == 2)
			{
				BreakMedium(this.contactPoint);
			}
			if (num == 3)
			{
				BreakHeavy(this.contactPoint);
			}
		}
	}

	[PunRPC]
	private void ImpactEffectRPC(Vector3 _position, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			AssetManager.instance.PhysImpactEffect(_position);
		}
	}

	private PlayerAvatar ImpactGetPlayer(Collider _hit, PlayerController _playerController, PlayerTumble _playerTumble)
	{
		PlayerAvatar playerAvatar = null;
		if ((bool)_playerTumble)
		{
			PlayerTumble playerTumble = _hit.transform.GetComponent<PlayerTumble>();
			if (!playerTumble)
			{
				playerTumble = _hit.transform.GetComponentInParent<PlayerTumble>();
			}
			if ((bool)playerTumble)
			{
				return playerTumble.playerAvatar;
			}
		}
		playerAvatar = _hit.transform.GetComponentInParent<PlayerAvatar>();
		if (!playerAvatar)
		{
			playerAvatar = _hit.transform.GetComponent<PlayerAvatar>();
		}
		if (!playerAvatar)
		{
			playerAvatar = _hit.transform.GetComponentInChildren<PlayerAvatar>();
		}
		if (!playerAvatar)
		{
			PlayerPhysPusher component = _hit.transform.GetComponent<PlayerPhysPusher>();
			if ((bool)component)
			{
				playerAvatar = component.Player;
			}
		}
		if (!playerAvatar && (bool)_playerController)
		{
			PlayerController playerController = _hit.transform.GetComponentInParent<PlayerController>();
			if (!playerController && (bool)_hit.transform.GetComponentInParent<PlayerCollisionController>())
			{
				playerController = PlayerController.instance;
			}
			if ((bool)playerController)
			{
				playerAvatar = playerController.playerAvatarScript;
			}
		}
		return playerAvatar;
	}

	public void PlayerHurtMultiplier(float _multiplier, float _time)
	{
		playerHurtMultiplier = _multiplier;
		playerHurtMultiplierTimer = _time;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	public void ChangePhysAudio(PhysAudio _physAudio)
	{
		impactAudio = _physAudio;
	}
}
