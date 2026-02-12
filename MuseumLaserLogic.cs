using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class MuseumLaserLogic : MonoBehaviour
{
	public enum MuseumLaserState
	{
		Inactive,
		PoweringUp,
		Active,
		ShuttingDown,
		Overloaded
	}

	public MeshRenderer beamMeshRenderer;

	public MeshRenderer beamInnerMeshRenderer;

	private Transform beamTransform;

	private Transform beamInnerTransform;

	private Material beamMaterial;

	private Material beamInnerMaterial;

	public AnimationCurve animationCurvePowerUp;

	public AnimationCurve animationCurveShutdown;

	private float originalBeamScaleX;

	private float originalBeamScaleZ;

	private float originalBeamInnerScaleX;

	private float originalBeamInnerScaleZ;

	public Transform laserBall1Transform;

	public Transform laserBall2Transform;

	[Space(10f)]
	public Transform beamLight1Transform;

	public Transform beamLight2Transform;

	public Transform beamLightMid1Transform;

	public Transform beamLightMid2Transform;

	[Space(10f)]
	public Transform spinnerTransform;

	public Transform particleSparksTransform;

	public Transform laserHitPosTransform;

	[FormerlySerializedAs("LaserBeamVisualTransform")]
	public Transform laserBeamVisualTransform;

	public Transform laserBeamVisualHitTransform;

	public Transform laserBeamVisualHitInnerTransform;

	public Transform hurtColliderTransform;

	[Space(20f)]
	public ParticleSystem particleSparksParticleSystem;

	public ParticleSystem particleSparksBall1ParticleSystem;

	public ParticleSystem particleLaserHitParticleSystem;

	public ParticleSystem particleOverloadedSparksParticleSystem;

	[Space(20f)]
	public Light spotlight1;

	public Light spotlight2;

	public Light ballLight1;

	public Light ballLight2;

	public Light beamLight1;

	public Light beamLight2;

	public Light beamLightMid1;

	public Light beamLightMid2;

	[Space(20f)]
	private float lightOriginalIntensitySpotlight1;

	private float lightOriginalIntensitySpotlight2;

	private float lightOriginalIntensityBall1;

	private float lightOriginalIntensityBall2;

	private float lightOriginalIntensityBeam1;

	private float lightOriginalIntensityBeam2;

	private float lightOriginalIntensityBeamMid1;

	private float lightOriginalIntensityBeamMid2;

	[Space(20f)]
	public Transform audioSourceTransform;

	public Transform audioSourceCloseTransform;

	public Transform audioSourceHitTransform;

	[Space(20f)]
	public Sound soundLoop;

	public Sound soundNearLoop;

	public Sound soundHitLoop;

	public Sound soundPowerUp;

	public Sound soundOverload;

	public Sound soundShutdown;

	public Sound soundSparks;

	private float beamWidthMultiplier = 1f;

	private PhotonView photonView;

	private float originalBeamHitScale;

	private float originalBeamHitInnerScale;

	private HurtCollider hurtCollider;

	private float firstHitTimer;

	private float turnOffTimer;

	private float overloadedSparkTimer;

	internal bool playerNear;

	internal bool enemyNear;

	internal bool isHitting;

	internal bool isBeamActive;

	private static readonly RaycastHit[] _hitsBuffer = new RaycastHit[1];

	public MuseumLaserState currentState;

	private bool stateStart;

	private bool isCulled;

	private float stateTimer;

	private float stateTimerMax = 1f;

	private bool hitEnemy;

	private EnemyType hitEnemyType = EnemyType.Light;

	private float hitEnemyTimer;

	private float overloadedTimeMax = 6f;

	private void Start()
	{
		stateStart = true;
		hurtCollider = hurtColliderTransform.GetComponentInChildren<HurtCollider>();
		photonView = GetComponent<PhotonView>();
		float range = Vector3.Distance(laserBall1Transform.position, laserBall2Transform.position);
		spotlight1.range = range;
		spotlight2.range = range;
		beamMaterial = beamMeshRenderer.material;
		beamInnerMaterial = beamInnerMeshRenderer.material;
		beamTransform = beamMeshRenderer.transform;
		beamInnerTransform = beamInnerMeshRenderer.transform;
		originalBeamScaleX = beamTransform.localScale.x;
		originalBeamScaleZ = beamTransform.localScale.z;
		originalBeamInnerScaleX = beamInnerTransform.localScale.x;
		originalBeamInnerScaleZ = beamInnerTransform.localScale.z;
		lightOriginalIntensitySpotlight1 = spotlight1.intensity;
		lightOriginalIntensitySpotlight2 = spotlight2.intensity;
		lightOriginalIntensityBall1 = ballLight1.intensity;
		lightOriginalIntensityBall2 = ballLight2.intensity;
		lightOriginalIntensityBeam1 = beamLight1.intensity;
		lightOriginalIntensityBeam2 = beamLight2.intensity;
		lightOriginalIntensityBeamMid1 = beamLightMid1.intensity;
		lightOriginalIntensityBeamMid2 = beamLightMid2.intensity;
		originalBeamHitScale = laserBeamVisualHitTransform.localScale.z;
		originalBeamHitInnerScale = laserBeamVisualHitInnerTransform.localScale.z;
		laserHitPosTransform.position = laserBall2Transform.position;
		MuseumLaserEditorLogic component = GetComponent<MuseumLaserEditorLogic>();
		if ((bool)component)
		{
			component.enabled = false;
		}
		if ((bool)hurtCollider)
		{
			hurtCollider.enabled = false;
		}
	}

	private void Update()
	{
		bool flag = currentState != MuseumLaserState.Inactive && currentState != MuseumLaserState.Overloaded && !isCulled;
		bool playing = flag && playerNear;
		soundLoop.PlayLoop(flag, 2f, 2f);
		soundHitLoop.PlayLoop(isHitting && flag, 20f, 2f);
		soundNearLoop.PlayLoop(playing, 2f, 2f);
		if (firstHitTimer > 0f)
		{
			firstHitTimer -= Time.deltaTime;
		}
		StateMachine();
	}

	private void BeamToggle(bool _toggle)
	{
		beamMeshRenderer.enabled = _toggle;
		beamInnerMeshRenderer.enabled = _toggle;
		beamLight1Transform.gameObject.SetActive(_toggle);
		beamLight2Transform.gameObject.SetActive(_toggle);
		beamLightMid1Transform.gameObject.SetActive(_toggle);
		beamLightMid2Transform.gameObject.SetActive(_toggle);
		laserBeamVisualTransform.gameObject.SetActive(_toggle);
		beamWidthMultiplier = 1f;
		isBeamActive = _toggle;
		isHitting = false;
		SetLightIntensities();
		ToggleLights(_toggle);
		if (_toggle)
		{
			particleSparksParticleSystem.Play();
			particleSparksBall1ParticleSystem.Play();
			return;
		}
		particleSparksParticleSystem.Stop();
		particleSparksBall1ParticleSystem.Stop();
		if ((bool)particleLaserHitParticleSystem)
		{
			particleLaserHitParticleSystem.Stop(withChildren: true);
		}
	}

	private void SetLightIntensities(float _intensity = 1f)
	{
		spotlight1.intensity = _intensity * lightOriginalIntensitySpotlight1;
		spotlight2.intensity = _intensity * lightOriginalIntensitySpotlight2;
		beamLight1.intensity = _intensity * lightOriginalIntensityBeam1;
		beamLight2.intensity = _intensity * lightOriginalIntensityBeam2;
		beamLightMid1.intensity = _intensity * lightOriginalIntensityBeamMid1;
		beamLightMid2.intensity = _intensity * lightOriginalIntensityBeamMid2;
		ballLight1.intensity = _intensity * lightOriginalIntensityBall1;
		ballLight2.intensity = _intensity * lightOriginalIntensityBall2;
	}

	private void ToggleLights(bool _toggle)
	{
		spotlight1.enabled = _toggle;
		spotlight2.enabled = _toggle;
		beamLight1.enabled = _toggle;
		beamLight2.enabled = _toggle;
		beamLightMid1.enabled = _toggle;
		beamLightMid2.enabled = _toggle;
		ballLight1.enabled = _toggle;
		ballLight2.enabled = _toggle;
	}

	private void StateMachine()
	{
		switch (currentState)
		{
		case MuseumLaserState.Inactive:
			StateInactive();
			break;
		case MuseumLaserState.PoweringUp:
			StatePoweringUp();
			break;
		case MuseumLaserState.Active:
			StateActive();
			break;
		case MuseumLaserState.ShuttingDown:
			StateShuttingDown();
			break;
		case MuseumLaserState.Overloaded:
			StateOverloaded();
			break;
		}
	}

	private void StateInactive()
	{
		if (stateStart)
		{
			stateStart = false;
			BeamToggle(_toggle: false);
			LaserVisuals();
		}
		PlayerNearTurnOnCheck();
		GrabbedValuableNearTurnOnCheck();
	}

	private void StatePoweringUp()
	{
		if (stateStart)
		{
			stateStart = false;
			stateTimerMax = 0.6f;
			BeamToggle(_toggle: true);
			SetLightIntensities(0f);
			Vector3 rhs = laserHitPosTransform.position - laserBall1Transform.position;
			float t = Mathf.Clamp01(Vector3.Dot(PlayerController.instance.transform.position - laserBall1Transform.position, rhs) / rhs.sqrMagnitude);
			Vector3 position = Vector3.Lerp(laserBall1Transform.position, laserHitPosTransform.position, t);
			soundPowerUp.Play(position);
			turnOffTimer = 3f;
		}
		float num = animationCurvePowerUp.Evaluate(stateTimer / stateTimerMax);
		beamWidthMultiplier = Mathf.Lerp(0f, 1f, num);
		SetLightIntensities(num);
		if (stateTimer >= stateTimerMax)
		{
			StateSet(MuseumLaserState.Active);
		}
		stateTimer += Time.deltaTime;
		LaserVisuals();
		LaserHitLogic();
	}

	private void StateActive()
	{
		if (stateStart)
		{
			stateStart = false;
			turnOffTimer = 3f;
			BeamToggle(_toggle: true);
		}
		LaserVisuals();
		TurnOffCheck();
		CullingLogic();
		LaserHitLogic();
		if (hitEnemy)
		{
			overloadedSparkTimer -= Time.deltaTime;
			if (overloadedSparkTimer <= 0f)
			{
				overloadedSparkTimer = Random.Range(0.05f, 0.1f);
				particleOverloadedSparksParticleSystem.Play();
				soundSparks.Play(laserBall1Transform.position);
			}
		}
	}

	private void StateShuttingDown()
	{
		if (stateStart)
		{
			stateStart = false;
			stateTimerMax = 0.6f;
			BeamToggle(_toggle: true);
			SetLightIntensities();
			Vector3 rhs = laserHitPosTransform.position - laserBall1Transform.position;
			float t = Mathf.Clamp01(Vector3.Dot(PlayerController.instance.transform.position - laserBall1Transform.position, rhs) / rhs.sqrMagnitude);
			Vector3 position = Vector3.Lerp(laserBall1Transform.position, laserHitPosTransform.position, t);
			soundShutdown.Play(position);
		}
		float num = animationCurveShutdown.Evaluate(stateTimer / stateTimerMax);
		beamWidthMultiplier = Mathf.Lerp(0f, 1f, num);
		SetLightIntensities(num);
		LaserHitLogic();
		stateTimer += Time.deltaTime;
		if (stateTimer >= stateTimerMax)
		{
			StateSet(MuseumLaserState.Inactive);
		}
		LaserVisuals();
	}

	private void StateOverloaded()
	{
		if (stateStart)
		{
			hitEnemy = false;
			hitEnemyTimer = 0f;
			stateStart = false;
			BeamToggle(_toggle: false);
			SetLightIntensities(0f);
			Vector3 rhs = laserHitPosTransform.position - laserBall1Transform.position;
			float t = Mathf.Clamp01(Vector3.Dot(PlayerController.instance.transform.position - laserBall1Transform.position, rhs) / rhs.sqrMagnitude);
			Vector3 position = Vector3.Lerp(laserBall1Transform.position, laserHitPosTransform.position, t);
			soundOverload.Play(position);
			stateTimerMax = overloadedTimeMax;
			stateTimer = 0f;
			overloadedSparkTimer = 0f;
			hurtCollider.enabled = false;
			LaserVisuals();
			if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("HitEnemyRPC", RpcTarget.Others, false);
			}
		}
		overloadedSparkTimer -= Time.deltaTime;
		if (overloadedSparkTimer <= 0f)
		{
			overloadedSparkTimer = Random.Range(0.2f, 0.6f);
			particleOverloadedSparksParticleSystem.Play();
			soundSparks.Play(laserBall1Transform.position);
		}
		stateTimer += Time.deltaTime;
		if (stateTimer >= stateTimerMax)
		{
			StateSet(MuseumLaserState.Inactive);
		}
	}

	private void PlayerNearTurnOnCheck()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || !SemiFunc.FPSImpulse5())
		{
			return;
		}
		float num = 3f;
		Vector3 position = laserBall1Transform.position;
		Vector3 position2 = laserBall2Transform.position;
		if (SemiFunc.PlayerNearestLineDistance(position, position2) < num)
		{
			playerNear = true;
			if (currentState == MuseumLaserState.Inactive)
			{
				StateSet(MuseumLaserState.PoweringUp);
			}
		}
	}

	private void GrabbedValuableNearTurnOnCheck()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.FPSImpulse1())
		{
			float num = 3f;
			Vector3 position = laserBall1Transform.position;
			Vector3 position2 = laserBall2Transform.position;
			if (SemiFunc.PhysGrabObjectValuableGrabbedLineDistanceToNearest(position, position2, num) < num && currentState == MuseumLaserState.Inactive)
			{
				StateSet(MuseumLaserState.PoweringUp);
			}
		}
	}

	private void TurnOffCheck()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (turnOffTimer > 0f)
		{
			turnOffTimer -= Time.deltaTime;
			return;
		}
		float num = 10f;
		Vector3 position = laserBall1Transform.position;
		Vector3 position2 = laserBall2Transform.position;
		if (SemiFunc.PlayerNearestLineDistance(position, position2) > num && currentState == MuseumLaserState.Active)
		{
			StateSet(MuseumLaserState.ShuttingDown);
		}
		num = 10f;
		if (SemiFunc.PhysGrabObjectValuableGrabbedLineDistanceToNearest(position, position2, num) > num && currentState == MuseumLaserState.Active)
		{
			StateSet(MuseumLaserState.ShuttingDown);
		}
		turnOffTimer = 3f;
	}

	private void LaserVisuals()
	{
		if (!isCulled)
		{
			float num = beamWidthMultiplier;
			float num2 = 5f;
			if (SemiFunc.FPSImpulse15() && (bool)particleSparksParticleSystem)
			{
				particleSparksParticleSystem.transform.position = Vector3.Lerp(laserBall1Transform.position + laserBall1Transform.forward * 0.3f, laserHitPosTransform.position - laserHitPosTransform.forward * 0.3f, Random.Range(0f, 1f));
			}
			spinnerTransform.Rotate(Vector3.forward * 1000f * Time.deltaTime, Space.Self);
			Vector3 rhs = laserHitPosTransform.position - laserBall1Transform.position;
			float t = Mathf.Clamp01(Vector3.Dot(PlayerController.instance.transform.position - laserBall1Transform.position, rhs) / rhs.sqrMagnitude);
			audioSourceTransform.position = Vector3.Lerp(laserBall1Transform.position, laserHitPosTransform.position, t);
			audioSourceCloseTransform.position = audioSourceTransform.position;
			laserBeamVisualHitTransform.gameObject.SetActive(isHitting);
			if (isHitting)
			{
				float num3 = 60f;
				float num4 = 0.05f;
				float num5 = Mathf.Sin(Time.time * num3) * num4;
				laserBeamVisualHitTransform.localScale = new Vector3((originalBeamHitScale + num5) * num, originalBeamHitScale + num5 * num, originalBeamHitScale + num5 * num);
				num4 = 0.05f;
				num3 = 65f;
				num5 = Mathf.Sin(Time.time * num3) * num4;
				laserBeamVisualHitInnerTransform.localScale = new Vector3(originalBeamHitInnerScale + num5 * num, originalBeamHitInnerScale + num5 * num, originalBeamHitInnerScale + num5 * num);
				laserBeamVisualHitTransform.Rotate(Vector3.up * 1000f * Time.deltaTime, Space.Self);
				laserBeamVisualHitInnerTransform.Rotate(Vector3.up * 1000f * Time.deltaTime, Space.Self);
			}
			beamLight1Transform.position = Vector3.Lerp(laserBall1Transform.position, laserHitPosTransform.position, Mathf.PingPong(Time.time * num2, 1f));
			beamLight2Transform.position = Vector3.Lerp(laserBall1Transform.position, laserHitPosTransform.position, Mathf.PingPong(Time.time * num2 + 0.5f, 1f));
			beamLightMid1Transform.position = Vector3.Lerp(laserBall1Transform.position, laserHitPosTransform.position, Mathf.PingPong(Time.time * num2 + 0.25f, 1f));
			beamLightMid2Transform.position = Vector3.Lerp(laserBall1Transform.position, laserHitPosTransform.position, Mathf.PingPong(Time.time * num2 + 0.75f, 1f));
			float num6 = 0.5f;
			float num7 = Time.time * num6;
			beamMaterial.SetTextureOffset("_MainTex", new Vector2(num7, num7 * 10f));
			beamInnerMaterial.SetTextureOffset("_MainTex", new Vector2(num7 * 2f, num7 * 20f));
			float num8 = 100f;
			float num9 = 0.2f;
			float t2 = 0.5f + 0.5f * Mathf.Sin(Time.time * num8);
			float x = Mathf.Lerp(originalBeamScaleX, originalBeamScaleX * (1f + num9), t2) * num;
			float z = Mathf.Lerp(originalBeamScaleZ, originalBeamScaleZ * (1f + num9), t2) * num;
			beamTransform.localScale = new Vector3(x, beamTransform.localScale.y, z);
			num8 = 80f;
			num9 = 1f;
			float t3 = 0.5f + 0.5f * Mathf.Sin(Time.time * num8 + 0.5f);
			float x2 = Mathf.Lerp(originalBeamInnerScaleX, originalBeamInnerScaleX / 4f * (1f + num9), t3) * num;
			float z2 = Mathf.Lerp(originalBeamInnerScaleZ, originalBeamInnerScaleZ / 4f * (1f + num9), t3) * num;
			beamInnerTransform.localScale = new Vector3(x2, beamInnerTransform.localScale.y, z2);
		}
	}

	private void StateSet(MuseumLaserState newState)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("StateSetRPC", RpcTarget.All, (int)newState);
			}
			else
			{
				currentState = newState;
				stateStart = true;
				stateTimer = 0f;
			}
		}
	}

	[PunRPC]
	private void StateSetRPC(int newState)
	{
		if (currentState != (MuseumLaserState)newState)
		{
			currentState = (MuseumLaserState)newState;
			stateStart = true;
			stateTimer = 0f;
		}
	}

	private void LaserHitLogic()
	{
		if (SemiFunc.FPSImpulse15())
		{
			Vector3 direction = laserHitPosTransform.position - laserBall1Transform.position;
			float maxDistance = direction.magnitude + 0.1f;
			direction.Normalize();
			if (Physics.RaycastNonAlloc(laserBall1Transform.position, direction, _hitsBuffer, maxDistance, SemiFunc.LayerMaskGetPlayersAndPhysObjects(), QueryTriggerInteraction.Ignore) > 0)
			{
				RaycastHit raycastHit = _hitsBuffer[0];
				if (raycastHit.transform != null)
				{
					if (!isHitting)
					{
						particleLaserHitParticleSystem.Play(withChildren: true);
						hurtCollider.enabled = true;
						if (firstHitTimer <= 0f)
						{
							audioSourceHitTransform.position = raycastHit.point;
							hurtColliderTransform.position = audioSourceHitTransform.position;
							float num = Vector3.Distance(laserBall1Transform.position, laserHitPosTransform.position + laserBall1Transform.forward * 0.1f);
							Vector3 localScale = laserBeamVisualTransform.localScale;
							localScale.z = num / 2f;
							laserBeamVisualTransform.localScale = localScale;
							firstHitTimer = 0.2f;
						}
					}
					isHitting = true;
					Vector3 rhs = laserBall2Transform.position - laserBall1Transform.position;
					float num2 = Vector3.Dot(raycastHit.point - laserBall1Transform.position, rhs);
					float sqrMagnitude = rhs.sqrMagnitude;
					float t = Mathf.Clamp01(num2 / sqrMagnitude);
					laserHitPosTransform.position = Vector3.Lerp(laserBall1Transform.position, laserBall2Transform.position, t);
					laserHitPosTransform.LookAt(laserBall1Transform);
				}
				else
				{
					if (isHitting)
					{
						particleLaserHitParticleSystem.Stop(withChildren: true);
						hurtCollider.enabled = false;
					}
					isHitting = false;
				}
			}
			else
			{
				if (isHitting)
				{
					particleLaserHitParticleSystem.Stop(withChildren: true);
					hurtCollider.enabled = false;
				}
				laserHitPosTransform.position = laserBall2Transform.position;
				isHitting = false;
			}
		}
		float num3 = Vector3.Distance(laserBall1Transform.position, laserHitPosTransform.position + laserBall1Transform.forward * 0.1f);
		float num4 = 25f;
		Vector3 localScale2 = laserBeamVisualTransform.localScale;
		localScale2.z = Mathf.Lerp(localScale2.z, num3 / 2f, Time.deltaTime * num4);
		laserBeamVisualTransform.localScale = localScale2;
		audioSourceHitTransform.position = Vector3.Lerp(audioSourceHitTransform.position, laserHitPosTransform.position, Time.deltaTime * num4);
		hurtColliderTransform.position = audioSourceHitTransform.position;
		hurtColliderTransform.LookAt(laserBall2Transform);
		if (!SemiFunc.IsMasterClientOrSingleplayer() || !hitEnemy)
		{
			return;
		}
		if (hitEnemyTimer <= 0f)
		{
			if (currentState != MuseumLaserState.Overloaded)
			{
				StateSet(MuseumLaserState.Overloaded);
			}
			hitEnemy = false;
		}
		if (hitEnemyTimer > 0f)
		{
			hitEnemyTimer -= Time.deltaTime;
		}
	}

	public void OnEnemyHit()
	{
		Enemy onImpactEnemyEnemy = hurtCollider.onImpactEnemyEnemy;
		if (!onImpactEnemyEnemy)
		{
			return;
		}
		if (!hitEnemy && SemiFunc.IsMultiplayer())
		{
			photonView.RPC("HitEnemyRPC", RpcTarget.Others, true);
		}
		if (!hitEnemy)
		{
			hitEnemy = true;
			hitEnemyType = onImpactEnemyEnemy.Type;
			hitEnemyTimer = 1.5f;
			if (onImpactEnemyEnemy.Type == EnemyType.VeryLight)
			{
				overloadedTimeMax = 3f;
			}
			else if (onImpactEnemyEnemy.Type == EnemyType.Light)
			{
				overloadedTimeMax = 6f;
			}
			else if (onImpactEnemyEnemy.Type == EnemyType.Medium)
			{
				overloadedTimeMax = 12f;
			}
			else if (onImpactEnemyEnemy.Type == EnemyType.Heavy)
			{
				overloadedTimeMax = 16f;
			}
			else if (onImpactEnemyEnemy.Type == EnemyType.VeryHeavy)
			{
				overloadedTimeMax = 20f;
			}
		}
	}

	[PunRPC]
	public void HitEnemyRPC(bool _hitEnemy)
	{
		hitEnemy = _hitEnemy;
	}

	private void CullingLogic()
	{
		if (SemiFunc.FPSImpulse5())
		{
			float num = 20f;
			Vector3 position = PlayerController.instance.transform.position;
			if (SemiFunc.IsMultiplayer() && SemiFunc.IsSpectating() && (bool)SpectateCamera.instance.player)
			{
				position = SpectateCamera.instance.player.PlayerVisionTarget.transform.position;
			}
			Vector3 position2 = laserBall1Transform.position;
			Vector3 position3 = laserBall2Transform.position;
			Vector3 rhs = position3 - position2;
			float num2 = Vector3.Dot(position - position2, rhs);
			float sqrMagnitude = rhs.sqrMagnitude;
			float t = Mathf.Clamp01(num2 / sqrMagnitude);
			Vector3 vector = Vector3.Lerp(position2, position3, t);
			float num3 = Vector3.Distance(position, vector);
			if (num3 < num)
			{
				isCulled = false;
			}
			else
			{
				isCulled = true;
			}
			playerNear = false;
			if (!isCulled)
			{
				num = 3f;
				if (num3 < num)
				{
					playerNear = true;
				}
			}
		}
		if (isCulled)
		{
			if (isBeamActive)
			{
				BeamToggle(_toggle: false);
			}
		}
		else if (!isBeamActive)
		{
			BeamToggle(_toggle: true);
		}
	}
}
