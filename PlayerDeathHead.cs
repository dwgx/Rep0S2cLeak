using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class PlayerDeathHead : MonoBehaviour, IPunObservable
{
	public PlayerAvatar playerAvatar;

	public PlayerEyes playerEyes;

	public MeshRenderer[] colorMeshRenderers;

	public ParticleSystem smokeParticles;

	public MapCustom mapCustom;

	public GameObject arenaCrown;

	public GameObject meshHeadParent;

	public GameObject meshParent;

	private float smokeParticleTime = 3f;

	private float smokeParticleTimer;

	private float smokeParticleRateOverTimeDefault;

	private float smokeParticleRateOverTimeCurrent;

	private float smokeParticleRateOverDistanceDefault;

	private float smokeParticleRateOverDistanceCurrent;

	internal PhysGrabObject physGrabObject;

	internal PhotonView photonView;

	private RoomVolumeCheck roomVolumeCheck;

	private bool setup;

	private bool triggered;

	private float triggeredTimer;

	private Vector3 triggeredPosition;

	private Quaternion triggeredRotation;

	internal bool inExtractionPoint;

	private bool inExtractionPointPrevious;

	internal bool inTruck;

	private bool inTruckPrevious;

	public GameObject spectatedParticle;

	public Sound spectatedSoundStart;

	public Sound spectatedSoundStop;

	public Color spectatedColorEye;

	public Color spectatedColorPupil;

	public AnimationCurve spectatedIntensityCurve;

	private float spectatedIntensityLerp;

	[Space(20f)]
	public Transform spectatedJumpSpinTransform;

	public AnimationCurve spectatedJumpSpinCurve;

	[Space(20f)]
	public Sound spectatedJumpChargeSound;

	public Sound spectatedJumpShortSound;

	public Sound spectatedJumpLongSound;

	private float spectatedJumpSoundAmount;

	private float spectatedJumpSpinLerp;

	internal bool spectated;

	internal bool spectatedLowEnergy;

	private float spectatedTimer;

	private float spectatedEyeFlashAmount;

	private float spectatedLookAtCooldown;

	private float spectatedLookAtLerp;

	private Quaternion spectatedMeshShakeTarget;

	private float spectatedMeshShakeCooldown;

	private bool spectatedJumpLocalPlayerInput;

	internal bool spectatedJumpCharging;

	private bool spectatedJumpGrounded;

	private float spectatedJumpGroundedTimer;

	internal float spectatedJumpChargeAmount;

	internal float spectatedJumpChargeAmountMax = 2f;

	private float spectatedJumpForce;

	private float spectatedJumpedTimer;

	private float spectatedJumpCooldown;

	internal bool overrideSpectated;

	private bool overrideSpectatedPrevious;

	private float overrideSpectatedTimer;

	[Space]
	public MeshRenderer[] eyeRenderers;

	public MeshRenderer[] pupilRenderers;

	public Transform eyeIdleTargetTransform;

	public Light eyeFlashLight;

	public Color eyeFlashPositiveColor;

	public Color eyeFlashNegativeColor;

	public float eyeFlashStrength;

	public float eyeFlashLightIntensity;

	public Sound eyeFlashPositiveSound;

	public Sound eyeFlashNegativeSound;

	[Space]
	public Transform eyeIdleTransformRight;

	public Transform eyeIdleTransformLeft;

	public AnimationCurve eyeIdleCurve;

	private float eyeIdleNew;

	private float eyeIdlePrev;

	private float eyeIdleLerp;

	[Space]
	public Transform pupilScaleTransformRight;

	public Transform pupilScaleTransformLeft;

	private Vector3 pupilScaleDefault;

	public AnimationCurve pupilScaleCurveIntro;

	public AnimationCurve pupilScaleCurveOutro;

	private float pupilScaleLerp = 1f;

	[Space]
	public MeshRenderer[] eyeNoiseRenderers;

	private bool eyeNoiseActive = true;

	private bool eyeNoiseActivePrevious;

	private Material eyeNoiseMaterial;

	private float eyeNoiseAlpha;

	private Material eyeMaterial;

	private Material pupilMaterial;

	private int eyeMaterialAmount;

	private int eyeMaterialColor;

	private AnimationCurve eyeFlashCurve;

	private float eyeFlashLerp;

	private bool eyeFlash;

	public Transform[] legRaycastPoints;

	private Vector3[] legRaycastHitPositions = new Vector3[2];

	private Vector3[] legRaycastHitPositionsAnimated = new Vector3[2];

	private Vector3[] legRaycastHitPositionsAnimatedPrev = new Vector3[2];

	private Vector3[] legRaycastHitPositionsAnimatedNew = new Vector3[2];

	public AnimationCurve legRaycastHitPositionCurve;

	private float[] legRaycastHitPositionLerp = new float[2];

	private float[] legRaycastTimers = new float[2];

	public Transform[] legGroundTransforms;

	private bool[] legGroundActive = new bool[2];

	[Space(20f)]
	public Transform[] legStepAnimTransforms;

	public AnimationCurve legStepAnimCurve;

	public Sound legFootstepSound;

	private float[] legStepAnimLerp = new float[2];

	private bool[] legStepAnimFootstep = new bool[2];

	[Space(20f)]
	public Transform[] legMeshTransforms;

	public AnimationCurve legMeshFollowCurve;

	private float[] legFollowLerp = new float[2];

	public Transform[] legTipTransforms;

	[Space(20f)]
	public SpringQuaternion[] legSprings;

	public Transform[] legSpringTransforms;

	public Transform[] legSpringTargetTransforms;

	public Transform[] legSpringTargetAnimTransforms;

	public AnimationCurve legSpringTargetAnimCurve;

	private float legSpringTargetAnimLerp;

	[Space(20f)]
	public Transform[] legShowTransforms;

	public AnimationCurve legShowCurve;

	public AnimationCurve legHideCurve;

	private float legShowLerp;

	private bool legShow = true;

	private bool legsActive;

	private float legsOverrideDisableTimer;

	public AudioClip seenSound;

	private bool serverSeen;

	private float seenCooldownTime = 2f;

	private float seenCooldownTimer;

	private bool localSeen;

	private bool localSeenEffect;

	private float localSeenEffectTime = 2f;

	private float localSeenEffectTimer;

	private float outsideLevelTimer;

	private bool tutorialPossible;

	private float tutorialTimer;

	private float inTruckReviveTimer;

	private Collider[] colliders;

	private List<MeshRenderer> meshRenderers;

	private Transform overridePositionTransform;

	private Vector3 overridePositionStart;

	private Vector3 overridePositionEnd;

	private Quaternion overrideRotationStart;

	private Quaternion overrideRotationEnd;

	private float overridePositionRotationLerp = 1f;

	private float overridePositionRotationTimer;

	public AnimationCurve overridePositionRotationCurve;

	private void Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
		photonView = GetComponent<PhotonView>();
		roomVolumeCheck = GetComponent<RoomVolumeCheck>();
		smokeParticleRateOverTimeDefault = smokeParticles.emission.rateOverTime.constant;
		smokeParticleRateOverDistanceDefault = smokeParticles.emission.rateOverDistance.constant;
		localSeenEffectTimer = localSeenEffectTime;
		MeshRenderer[] array = eyeRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (!eyeMaterial)
			{
				eyeMaterial = meshRenderer.material;
			}
			meshRenderer.material = eyeMaterial;
		}
		array = pupilRenderers;
		foreach (MeshRenderer meshRenderer2 in array)
		{
			if (!pupilMaterial)
			{
				pupilMaterial = meshRenderer2.material;
			}
			meshRenderer2.material = pupilMaterial;
		}
		array = eyeNoiseRenderers;
		foreach (MeshRenderer meshRenderer3 in array)
		{
			if (!eyeNoiseMaterial)
			{
				eyeNoiseMaterial = meshRenderer3.material;
			}
			meshRenderer3.material = eyeNoiseMaterial;
		}
		eyeMaterialAmount = Shader.PropertyToID("_ColorOverlayAmount");
		eyeMaterialColor = Shader.PropertyToID("_ColorOverlay");
		eyeFlashCurve = AssetManager.instance.animationCurveImpact;
		pupilMaterial.SetFloat(eyeMaterialAmount, 1f);
		pupilMaterial.SetColor(eyeMaterialColor, spectatedColorPupil);
		smokeParticleTimer = smokeParticleTime;
		physGrabObject.impactDetector.destroyDisableTeleport = false;
		colliders = GetComponentsInChildren<Collider>();
		SetColliders(_enabled: false);
		StartCoroutine(Setup());
		meshRenderers = new List<MeshRenderer>(GetComponentsInChildren<MeshRenderer>());
		pupilScaleDefault = pupilScaleTransformRight.localScale;
	}

	private IEnumerator Setup()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			if (GameManager.Multiplayer())
			{
				photonView.RPC("SetupRPC", RpcTarget.OthersBuffered, playerAvatar.steamID);
			}
			SetupDone();
			physGrabObject.Teleport(new Vector3(0f, 3000f, 0f), Quaternion.identity);
			if (SemiFunc.RunIsArena())
			{
				physGrabObject.impactDetector.destroyDisable = false;
			}
			setup = true;
		}
	}

	private IEnumerator SetupClient()
	{
		while (!physGrabObject)
		{
			yield return new WaitForSeconds(0.1f);
		}
		while (!physGrabObject.impactDetector)
		{
			yield return new WaitForSeconds(0.1f);
		}
		while (!physGrabObject.impactDetector.particles)
		{
			yield return new WaitForSeconds(0.1f);
		}
		SetupDone();
	}

	private void SetupDone()
	{
		if (!playerAvatar)
		{
			Debug.LogError("PlayerDeathHead: PlayerAvatar not found", base.gameObject);
			return;
		}
		if (SemiFunc.RunIsLevel() && TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialReviving, 1) && !playerAvatar.isLocal)
		{
			tutorialPossible = true;
		}
		base.transform.parent = playerAvatar.transform.parent;
		if (SemiFunc.IsMultiplayer() && playerAvatar == SessionManager.instance.CrownedPlayerGet())
		{
			arenaCrown.SetActive(value: true);
		}
		playerAvatar.SoundSetup(spectatedSoundStart);
		playerAvatar.SoundSetup(spectatedSoundStop);
		playerAvatar.SoundSetup(legFootstepSound);
		playerAvatar.SoundSetup(spectatedJumpChargeSound);
		playerAvatar.SoundSetup(spectatedJumpShortSound);
		playerAvatar.SoundSetup(spectatedJumpLongSound);
		playerEyes.playerAvatar = playerAvatar;
	}

	private void Update()
	{
		if (!serverSeen)
		{
			mapCustom.Hide();
		}
		if ((!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient) && setup)
		{
			if (!triggered)
			{
				physGrabObject.OverrideDeactivate();
			}
			else if (triggeredTimer > 0f)
			{
				physGrabObject.OverrideDeactivateReset();
				physGrabObject.Teleport(triggeredPosition, triggeredRotation);
				triggeredTimer -= Time.deltaTime;
				if (triggeredTimer <= 0f)
				{
					physGrabObject.rb.AddForce(playerAvatar.localCamera.transform.up * 2f, ForceMode.Impulse);
					physGrabObject.rb.AddForce(physGrabObject.transform.forward * 0.5f, ForceMode.Impulse);
					physGrabObject.rb.AddTorque(physGrabObject.transform.right * 0.2f, ForceMode.Impulse);
					if (SemiFunc.IsMultiplayer())
					{
						photonView.RPC("TriggerDoneRPC", RpcTarget.All);
					}
					else
					{
						TriggerDoneRPC();
					}
				}
			}
		}
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			if (triggered)
			{
				inExtractionPoint = roomVolumeCheck.inExtractionPoint;
				if (inExtractionPoint != inExtractionPointPrevious)
				{
					int num = 0;
					if (inExtractionPoint)
					{
						num = 1;
					}
					if (GameManager.Multiplayer())
					{
						photonView.RPC("FlashEyeRPC", RpcTarget.All, num);
					}
					else
					{
						FlashEyeRPC(num);
					}
					inExtractionPointPrevious = inExtractionPoint;
				}
			}
			else
			{
				inExtractionPoint = false;
				inExtractionPointPrevious = false;
			}
		}
		if (smokeParticles.isPlaying)
		{
			smokeParticleTimer -= Time.deltaTime;
			if (smokeParticleTimer <= 0f)
			{
				smokeParticleRateOverTimeCurrent -= 1f * Time.deltaTime;
				smokeParticleRateOverTimeCurrent = Mathf.Max(smokeParticleRateOverTimeCurrent, 0f);
				smokeParticleRateOverDistanceCurrent -= 10f * Time.deltaTime;
				smokeParticleRateOverDistanceCurrent = Mathf.Max(smokeParticleRateOverDistanceCurrent, 0f);
				ParticleSystem.EmissionModule emission = smokeParticles.emission;
				emission.rateOverTime = new ParticleSystem.MinMaxCurve(smokeParticleRateOverTimeCurrent);
				emission.rateOverDistance = new ParticleSystem.MinMaxCurve(smokeParticleRateOverDistanceCurrent);
				if (smokeParticleRateOverTimeCurrent <= 0f && smokeParticleRateOverDistanceCurrent <= 0f)
				{
					smokeParticles.Stop();
				}
			}
		}
		if (eyeFlash)
		{
			eyeFlashLerp += 2f * Time.deltaTime;
			eyeFlashLerp = Mathf.Clamp01(eyeFlashLerp);
			eyeMaterial.SetFloat(eyeMaterialAmount, eyeFlashCurve.Evaluate(eyeFlashLerp));
			pupilMaterial.SetFloat(eyeMaterialAmount, eyeFlashCurve.Evaluate(eyeFlashLerp));
			eyeFlashLight.intensity = eyeFlashCurve.Evaluate(eyeFlashLerp) * eyeFlashLightIntensity;
			if (eyeFlashLerp >= 1f)
			{
				eyeFlash = false;
				eyeMaterial.SetFloat(eyeMaterialAmount, 0f);
				pupilMaterial.SetFloat(eyeMaterialAmount, 0f);
				eyeFlashLight.gameObject.SetActive(value: false);
			}
		}
		else if (spectated)
		{
			spectatedIntensityLerp += 0.5f * Time.deltaTime;
			if (spectatedIntensityLerp > 1f)
			{
				spectatedIntensityLerp = 0f;
			}
			if (SemiFunc.Photosensitivity())
			{
				spectatedIntensityLerp = 0f;
			}
			if (!eyeFlashLight.gameObject.activeSelf)
			{
				eyeFlashLight.gameObject.SetActive(value: true);
				eyeFlashLight.color = spectatedColorEye;
				spectatedEyeFlashAmount = 0f;
			}
			float num2 = Mathf.Max(0.5f, 0.5f + playerAvatar.voiceChat.clipLoudness * 10f);
			if (SemiFunc.Photosensitivity())
			{
				num2 = 1f;
			}
			if (spectatedLowEnergy)
			{
				num2 *= 0.25f;
			}
			if (num2 > spectatedEyeFlashAmount)
			{
				spectatedEyeFlashAmount = Mathf.Lerp(spectatedEyeFlashAmount, num2, 10f * Time.deltaTime);
			}
			else
			{
				spectatedEyeFlashAmount = Mathf.Lerp(spectatedEyeFlashAmount, num2, 30f * Time.deltaTime);
			}
			float num3 = spectatedEyeFlashAmount + spectatedIntensityCurve.Evaluate(spectatedIntensityLerp) * 0.25f;
			eyeMaterial.SetFloat(eyeMaterialAmount, num3);
			pupilMaterial.SetFloat(eyeMaterialAmount, num3);
			eyeFlashLight.intensity = num3 * eyeFlashLightIntensity;
			eyeMaterial.SetColor(eyeMaterialColor, spectatedColorEye);
		}
		else if (spectatedEyeFlashAmount > 0f)
		{
			spectatedIntensityLerp += 0.5f * Time.deltaTime;
			if (spectatedIntensityLerp > 1f)
			{
				spectatedIntensityLerp = 0f;
			}
			if (SemiFunc.Photosensitivity())
			{
				spectatedIntensityLerp = 0f;
			}
			spectatedEyeFlashAmount = Mathf.Lerp(spectatedEyeFlashAmount, 0f, 10f * Time.deltaTime);
			float num4 = spectatedEyeFlashAmount + spectatedIntensityCurve.Evaluate(spectatedIntensityLerp) * 0.25f;
			eyeMaterial.SetFloat(eyeMaterialAmount, num4);
			pupilMaterial.SetFloat(eyeMaterialAmount, num4);
			eyeFlashLight.intensity = num4 * eyeFlashLightIntensity;
			eyeMaterial.SetColor(eyeMaterialColor, spectatedColorEye);
			if (spectatedEyeFlashAmount <= 0f)
			{
				eyeFlashLight.gameObject.SetActive(value: false);
			}
		}
		if (spectated)
		{
			if (pupilScaleLerp < 1f)
			{
				if (pupilScaleLerp <= 0f)
				{
					pupilScaleTransformLeft.gameObject.SetActive(value: true);
					pupilScaleTransformRight.gameObject.SetActive(value: true);
					playerEyes.enabled = true;
				}
				pupilScaleLerp += 2f * Time.deltaTime;
				pupilScaleLerp = Mathf.Clamp01(pupilScaleLerp);
				pupilScaleTransformLeft.localScale = Vector3.Lerp(Vector3.zero, pupilScaleDefault, pupilScaleCurveIntro.Evaluate(pupilScaleLerp));
				pupilScaleTransformRight.localScale = pupilScaleTransformLeft.localScale;
			}
		}
		else if (pupilScaleLerp > 0f)
		{
			pupilScaleLerp -= 2f * Time.deltaTime;
			pupilScaleLerp = Mathf.Clamp01(pupilScaleLerp);
			if (pupilScaleLerp <= 0f)
			{
				pupilScaleTransformLeft.gameObject.SetActive(value: false);
				pupilScaleTransformRight.gameObject.SetActive(value: false);
				playerEyes.enabled = false;
			}
			pupilScaleTransformLeft.localScale = Vector3.Lerp(Vector3.zero, pupilScaleDefault, pupilScaleCurveOutro.Evaluate(pupilScaleLerp));
			pupilScaleTransformRight.localScale = pupilScaleTransformLeft.localScale;
		}
		if (spectated)
		{
			eyeNoiseActive = true;
			eyeNoiseAlpha -= 2f * Time.deltaTime;
			eyeNoiseAlpha = Mathf.Clamp01(eyeNoiseAlpha);
			eyeNoiseMaterial.SetColor("_Color", new Color(1f, 1f, 1f, eyeNoiseAlpha + 0.05f - spectatedIntensityCurve.Evaluate(spectatedIntensityLerp) * 0.5f));
			if (SemiFunc.FPSImpulse20() && !SemiFunc.Photosensitivity())
			{
				eyeNoiseMaterial.SetTextureOffset("_MainTex", new Vector2(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f)));
			}
		}
		else if (eyeNoiseActive)
		{
			eyeNoiseAlpha -= 2f * Time.deltaTime;
			eyeNoiseAlpha = Mathf.Clamp01(eyeNoiseAlpha);
			eyeNoiseMaterial.SetColor("_Color", new Color(1f, 1f, 1f, eyeNoiseAlpha - spectatedIntensityCurve.Evaluate(spectatedIntensityLerp) * eyeNoiseAlpha * 0.5f));
			if (SemiFunc.FPSImpulse20() && !SemiFunc.Photosensitivity())
			{
				eyeNoiseMaterial.SetTextureOffset("_MainTex", new Vector2(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f)));
			}
			if (eyeNoiseAlpha <= 0f)
			{
				eyeNoiseActive = false;
			}
		}
		if (spectated && !playerEyes.lookAtActive)
		{
			if (eyeIdleNew != 30f)
			{
				eyeIdleNew = 30f;
				eyeIdlePrev = eyeIdleTransformRight.localEulerAngles.y;
				eyeIdleLerp = 0f;
			}
		}
		else if (eyeIdleNew != 0f)
		{
			eyeIdleNew = 0f;
			eyeIdlePrev = eyeIdleTransformRight.localEulerAngles.y;
			eyeIdleLerp = 0f;
		}
		if (eyeIdleLerp < 1f)
		{
			eyeIdleLerp += 0.5f * Time.deltaTime;
			eyeIdleLerp = Mathf.Clamp01(eyeIdleLerp);
			eyeIdleTransformRight.localEulerAngles = new Vector3(0f, Mathf.Lerp(eyeIdlePrev, eyeIdleNew, eyeIdleCurve.Evaluate(eyeIdleLerp)), 0f);
			eyeIdleTransformLeft.localEulerAngles = new Vector3(0f, 0f - eyeIdleTransformRight.localEulerAngles.y, 0f);
		}
		if (triggered && !localSeen && !PlayerController.instance.playerAvatarScript.isDisabled)
		{
			if (seenCooldownTimer > 0f)
			{
				seenCooldownTimer -= Time.deltaTime;
			}
			else
			{
				Vector3 position = PlayerController.instance.playerAvatarScript.localCamera.transform.position;
				float num5 = Vector3.Distance(base.transform.position, position);
				if (num5 <= 10f && SemiFunc.OnScreen(base.transform.position, -0.15f, -0.15f))
				{
					Vector3 normalized = (position - base.transform.position).normalized;
					if (!Physics.Raycast(physGrabObject.centerPoint, normalized, out var _, num5, LayerMask.GetMask("Default")))
					{
						localSeen = true;
						TutorialDirector.instance.playerSawHead = true;
						if (!serverSeen && SemiFunc.RunIsLevel())
						{
							if (SemiFunc.IsMultiplayer())
							{
								photonView.RPC("SeenSetRPC", RpcTarget.All, true);
							}
							else
							{
								SeenSetRPC(_toggle: true);
							}
							if (PlayerController.instance.deathSeenTimer <= 0f)
							{
								localSeenEffect = true;
								PlayerController.instance.deathSeenTimer = 30f;
								GameDirector.instance.CameraImpact.Shake(2f, 0.5f);
								GameDirector.instance.CameraShake.Shake(2f, 1f);
								AudioScare.instance.PlayCustom(seenSound, 0.3f, 60f);
								ValuableDiscover.instance.New(physGrabObject, ValuableDiscoverGraphic.State.Bad);
							}
						}
					}
				}
			}
		}
		if (localSeenEffect)
		{
			localSeenEffectTimer -= Time.deltaTime;
			CameraZoom.Instance.OverrideZoomSet(75f, 0.1f, 0.25f, 0.25f, base.gameObject, 150);
			PostProcessing.Instance.VignetteOverride(Color.black, 0.4f, 1f, 1f, 0.5f, 0.1f, base.gameObject);
			PostProcessing.Instance.SaturationOverride(-50f, 1f, 0.5f, 0.1f, base.gameObject);
			PostProcessing.Instance.ContrastOverride(5f, 1f, 0.5f, 0.1f, base.gameObject);
			GameDirector.instance.CameraImpact.Shake(10f * Time.deltaTime, 0.1f);
			GameDirector.instance.CameraShake.Shake(10f * Time.deltaTime, 1f);
			if (localSeenEffectTimer <= 0f)
			{
				localSeenEffect = false;
			}
		}
		if (triggered && SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (roomVolumeCheck.CurrentRooms.Count <= 0)
			{
				outsideLevelTimer += Time.deltaTime;
				if (outsideLevelTimer >= 5f)
				{
					if (RoundDirector.instance.extractionPointActive)
					{
						physGrabObject.Teleport(RoundDirector.instance.extractionPointCurrent.safetySpawn.position, RoundDirector.instance.extractionPointCurrent.safetySpawn.rotation);
					}
					else
					{
						physGrabObject.Teleport(TruckSafetySpawnPoint.instance.transform.position, TruckSafetySpawnPoint.instance.transform.rotation);
					}
				}
			}
			else
			{
				outsideLevelTimer = 0f;
			}
		}
		if (tutorialPossible)
		{
			if (triggered && localSeen)
			{
				tutorialTimer -= Time.deltaTime;
				if (tutorialTimer <= 0f)
				{
					if (!RoundDirector.instance.allExtractionPointsCompleted && TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialReviving, 1))
					{
						TutorialDirector.instance.ActivateTip("Reviving", 0.5f, _interrupt: false);
					}
					tutorialPossible = false;
				}
			}
			else
			{
				tutorialTimer = 5f;
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (RoundDirector.instance.allExtractionPointsCompleted && triggered && !playerAvatar.finalHeal)
			{
				inTruck = roomVolumeCheck.inTruck;
				if (inTruck != inTruckPrevious)
				{
					int num6 = 0;
					if (inTruck)
					{
						num6 = 1;
					}
					if (GameManager.Multiplayer())
					{
						photonView.RPC("FlashEyeRPC", RpcTarget.All, num6);
					}
					else
					{
						FlashEyeRPC(num6);
					}
					inTruckPrevious = inTruck;
				}
			}
			else
			{
				inTruck = false;
				inTruckPrevious = false;
			}
			if (inTruck)
			{
				inTruckReviveTimer -= Time.deltaTime;
				if (inTruckReviveTimer <= 0f)
				{
					playerAvatar.Revive(_revivedByTruck: true);
				}
			}
			else
			{
				inTruckReviveTimer = 2f;
			}
		}
		if (spectated)
		{
			playerAvatar.voiceChat.OverridePosition(physGrabObject.centerPoint, 0.1f);
			if (playerAvatar.photonView.IsMine)
			{
				if (spectatedTimer <= 0f)
				{
					SpectatedSet(_active: false);
				}
				else
				{
					spectatedTimer -= Time.deltaTime;
				}
				if (SpectateCamera.instance.headEnergy <= 0.1f)
				{
					SpectatedLowEnergySet(_active: true);
				}
			}
			if (spectatedLowEnergy)
			{
				playerAvatar.voiceChat.OverridePitch(0.5f, 3f, 0.25f);
			}
			eyeIdleTargetTransform.position = playerAvatar.localCamera.transform.position + playerAvatar.localCamera.transform.forward * 2f;
			eyeIdleTargetTransform.rotation = playerAvatar.localCamera.transform.rotation;
			if (spectatedMeshShakeCooldown <= 0f)
			{
				float num7 = 2f + spectatedJumpChargeAmount * 3f;
				spectatedMeshShakeTarget = Quaternion.Euler(UnityEngine.Random.Range(0f - num7, num7), UnityEngine.Random.Range(0f - num7, num7), UnityEngine.Random.Range(0f - num7, num7));
				spectatedMeshShakeCooldown = UnityEngine.Random.Range(0.01f, 0.05f);
			}
			else
			{
				spectatedMeshShakeCooldown -= Time.deltaTime;
			}
		}
		else
		{
			playerEyes.Override(base.transform.position + base.transform.up, 0.25f, base.gameObject);
			spectatedMeshShakeTarget = Quaternion.identity;
		}
		meshHeadParent.transform.localRotation = Quaternion.Slerp(meshHeadParent.transform.localRotation, spectatedMeshShakeTarget, 20f * Time.deltaTime);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (overrideSpectatedTimer > 0f)
			{
				overrideSpectatedTimer -= Time.deltaTime;
				if (overrideSpectatedTimer <= 0f)
				{
					overrideSpectated = false;
				}
			}
			if (overrideSpectated != overrideSpectatedPrevious)
			{
				if (SemiFunc.IsMultiplayer())
				{
					photonView.RPC("OverrideSpectatedRPC", RpcTarget.Others, overrideSpectated);
				}
				overrideSpectatedPrevious = overrideSpectated;
			}
		}
		if (overrideSpectated)
		{
			LegsOverrideDisable(0.25f);
		}
		if ((bool)playerAvatar && playerAvatar.isLocal)
		{
			if (!overrideSpectated && spectated && SemiFunc.InputHold(InputKey.Jump))
			{
				SpectatedJumpLocalInput(_input: true);
			}
			else
			{
				SpectatedJumpLocalInput(_input: false);
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (spectated && spectatedJumpLocalPlayerInput && spectatedJumpChargeAmount <= spectatedJumpChargeAmountMax)
			{
				if (spectatedJumpGroundedTimer <= 0f)
				{
					spectatedJumpGroundedTimer = 0.2f;
					spectatedJumpGrounded = SemiFunc.OnGroundCheck(physGrabObject.centerPoint, 0.5f, physGrabObject);
				}
				else
				{
					spectatedJumpGroundedTimer -= Time.deltaTime;
				}
				if (spectatedJumpGrounded && spectatedJumpCooldown <= 0f)
				{
					spectatedJumpChargeAmount += Time.deltaTime;
				}
			}
			else if (spectatedJumpChargeAmount > 0f)
			{
				if (SemiFunc.IsMultiplayer())
				{
					photonView.RPC("SpectatedJumpSpinRPC", RpcTarget.All, spectatedJumpChargeAmount);
				}
				else
				{
					SpectatedJumpSpinRPC(spectatedJumpChargeAmount);
				}
				spectatedJumpForce = spectatedJumpChargeAmount;
				spectatedJumpChargeAmount = 0f;
				spectatedJumpCooldown = 2f;
				spectatedJumpGrounded = false;
				spectatedJumpGroundedTimer = 0f;
			}
			if (spectatedJumpedTimer > 0f)
			{
				spectatedJumpedTimer -= Time.deltaTime;
			}
			if (spectatedJumpCooldown > 0f)
			{
				spectatedJumpCooldown -= Time.deltaTime;
			}
		}
		Transform[] array;
		if (spectated && spectatedJumpChargeAmount > 0f)
		{
			if (!spectatedJumpCharging)
			{
				spectatedJumpCharging = true;
				array = legRaycastPoints;
				foreach (Transform value in array)
				{
					legRaycastTimers[Array.IndexOf(legRaycastPoints, value)] = 0f;
				}
			}
		}
		else if (spectatedJumpCharging)
		{
			spectatedJumpCharging = false;
			if (spectatedJumpSoundAmount < 1f)
			{
				spectatedJumpShortSound.Play(base.transform.position);
			}
			else
			{
				spectatedJumpLongSound.Play(base.transform.position);
			}
		}
		spectatedJumpChargeSound.PlayLoop(spectatedJumpCharging, 10f, 10f, 0.5f + spectatedJumpChargeAmount * 0.5f);
		if (spectatedJumpChargeAmount > 0f)
		{
			spectatedJumpSoundAmount = spectatedJumpChargeAmount;
		}
		if (spectatedJumpSpinLerp < 1f)
		{
			spectatedJumpSpinLerp += 1f * Time.deltaTime;
			spectatedJumpSpinLerp = Mathf.Clamp01(spectatedJumpSpinLerp);
			spectatedJumpSpinTransform.localRotation = Quaternion.Euler(Mathf.LerpUnclamped(0f, 360f, spectatedJumpSpinCurve.Evaluate(spectatedJumpSpinLerp)), 0f, 0f);
		}
		bool flag = spectated;
		if (legsOverrideDisableTimer > 0f)
		{
			flag = false;
			legsOverrideDisableTimer -= Time.deltaTime;
		}
		if (legsActive != flag)
		{
			legsActive = flag;
			legShowLerp = 0f;
		}
		float num8 = 1f;
		array = legRaycastPoints;
		foreach (Transform transform in array)
		{
			int num9 = Array.IndexOf(legRaycastPoints, transform);
			float num10 = legRaycastTimers[num9];
			if (legsActive)
			{
				Vector3 vector = legRaycastHitPositions[num9];
				Vector3 vector2 = legRaycastHitPositionsAnimated[num9];
				Vector3 vector3 = legRaycastHitPositionsAnimatedPrev[num9];
				Vector3 vector4 = legRaycastHitPositionsAnimatedNew[num9];
				float num11 = legRaycastHitPositionLerp[num9];
				Transform transform2 = legStepAnimTransforms[num9];
				float num12 = legStepAnimLerp[num9];
				bool flag2 = legStepAnimFootstep[num9];
				Transform transform3 = legGroundTransforms[num9];
				float num13 = legFollowLerp[num9];
				Transform transform4 = legMeshTransforms[num9];
				Transform transform5 = legTipTransforms[num9];
				Transform transform6 = legSpringTransforms[num9];
				SpringQuaternion attributes = legSprings[num9];
				Transform transform7 = legSpringTargetTransforms[num9];
				Transform transform8 = legSpringTargetAnimTransforms[num9];
				bool flag3 = legGroundActive[num9];
				float num14 = physGrabObject.rbAngularVelocity.magnitude + physGrabObject.rbVelocity.magnitude;
				if (num10 > 0f)
				{
					num10 -= Time.deltaTime + num14 * 0.0025f;
					if (Vector3.Distance(transform.position, vector) > 0.5f)
					{
						num10 = 0f;
					}
				}
				else
				{
					num10 = UnityEngine.Random.Range(0.25f, 1f);
					if (spectatedJumpChargeAmount > 0f)
					{
						num10 = Mathf.Max(0.1f, 0.5f - spectatedJumpChargeAmount);
					}
					vector = Vector3.zero;
					int mask = LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "Enemy", "Player");
					Collider[] array2 = Physics.OverlapSphere(transform.position, 0.01f, mask);
					for (int j = 0; j < array2.Length; j++)
					{
						PhysGrabObject componentInParent = array2[j].GetComponentInParent<PhysGrabObject>();
						if (!componentInParent || componentInParent != physGrabObject)
						{
							vector = transform.position;
							break;
						}
					}
					if (vector == Vector3.zero)
					{
						float maxDistance = 0.75f;
						Vector3 direction = Vector3.Lerp(-base.transform.up, Vector3.down, 0.75f);
						RaycastHit[] array3 = Physics.RaycastAll(transform.position, direction, maxDistance, mask);
						float num15 = 0.5f;
						RaycastHit[] array4 = array3;
						for (int j = 0; j < array4.Length; j++)
						{
							RaycastHit raycastHit = array4[j];
							if (Vector3.Distance(raycastHit.point, transform4.position) < num15)
							{
								PhysGrabObject componentInParent2 = raycastHit.collider.GetComponentInParent<PhysGrabObject>();
								if (!componentInParent2 || componentInParent2 != physGrabObject)
								{
									num15 = Vector3.Distance(raycastHit.point, transform.position);
									vector = raycastHit.point;
								}
							}
						}
					}
					if (vector != Vector3.zero && Vector3.Distance(vector, transform.position) < 0.2f)
					{
						vector += new Vector3(base.transform.right.x, 0f, base.transform.right.z) * num8 * 0.1f;
					}
					flag3 = true;
					if (vector == Vector3.zero)
					{
						array2 = Physics.OverlapSphere(transform3.position, 0.05f, mask);
						for (int j = 0; j < array2.Length; j++)
						{
							PhysGrabObject componentInParent3 = array2[j].GetComponentInParent<PhysGrabObject>();
							if (!componentInParent3 || componentInParent3 != physGrabObject)
							{
								vector = transform.position;
								vector.y = transform3.position.y;
								break;
							}
						}
						if (vector == Vector3.zero)
						{
							vector = vector3;
							flag3 = false;
						}
					}
				}
				if (vector != vector4 || vector2 == Vector3.zero)
				{
					if (vector4 == Vector3.zero || (Vector3.Distance(vector, vector4) < 0.05f && spectatedJumpChargeAmount <= 0f))
					{
						num11 = 1f;
						vector2 = vector;
					}
					else
					{
						num11 = 0f;
						if (num12 > 0.75f)
						{
							num12 = 0f;
							flag2 = true;
						}
					}
					vector4 = vector;
					vector3 = vector2;
				}
				float num16 = 4f + spectatedJumpChargeAmount * 2f;
				if (num11 < 1f)
				{
					num11 += (num16 + num14 * 0.05f) * Time.deltaTime;
					num11 = Mathf.Clamp01(num11);
					vector2 = Vector3.Lerp(vector3, vector4, legRaycastHitPositionCurve.Evaluate(num11));
					if ((num11 > 0.75f || spectatedJumpChargeAmount > 0f) && flag2)
					{
						legFootstepSound.Pitch = 2f + spectatedJumpChargeAmount;
						legFootstepSound.Play(transform5.position);
						flag2 = false;
					}
				}
				if (num12 < 1f)
				{
					num12 += (num16 + num14 * 0.05f) * Time.deltaTime;
					num12 = Mathf.Clamp01(num12);
					transform2.localEulerAngles = new Vector3(Mathf.LerpUnclamped(0f, -15f, legStepAnimCurve.Evaluate(num12)), 0f, 0f);
				}
				transform3.LookAt(vector2);
				transform7.localRotation = Quaternion.Slerp(transform3.localRotation, Quaternion.Euler(new Vector3(90f, 0f, 0f)), legMeshFollowCurve.Evaluate(num13));
				transform8.localEulerAngles = new Vector3(legSpringTargetAnimCurve.Evaluate(legSpringTargetAnimLerp) * 20f * num8, 0f, 0f);
				transform6.rotation = SemiFunc.SpringQuaternionGet(attributes, transform8.rotation);
				num13 = ((!flag3) ? (num13 + 5f * Time.deltaTime) : (num13 - 5f * Time.deltaTime));
				num13 = Mathf.Clamp01(num13);
				transform4.rotation = Quaternion.Slerp(transform3.rotation, transform6.rotation, legMeshFollowCurve.Evaluate(num13));
				Quaternion quaternion = Quaternion.Euler(transform4.eulerAngles.x + spectatedMeshShakeTarget.eulerAngles.x * num8, transform4.eulerAngles.y + spectatedMeshShakeTarget.eulerAngles.y * num8, transform4.eulerAngles.z + spectatedMeshShakeTarget.eulerAngles.z * num8);
				float t = 0.3f;
				if (playerAvatar.isLocal)
				{
					t = 0.1f;
				}
				transform4.rotation = Quaternion.Slerp(transform4.rotation, quaternion, t);
				legRaycastHitPositions[num9] = vector;
				legRaycastHitPositionsAnimated[num9] = vector2;
				legRaycastHitPositionsAnimatedPrev[num9] = vector3;
				legRaycastHitPositionsAnimatedNew[num9] = vector4;
				legRaycastHitPositionLerp[num9] = num11;
				legStepAnimLerp[num9] = num12;
				legStepAnimFootstep[num9] = flag2;
				legFollowLerp[num9] = num13;
				legGroundActive[num9] = flag3;
			}
			else
			{
				num10 = 0f;
			}
			legRaycastTimers[num9] = num10;
			num8 = 0f - num8;
		}
		if (spectated)
		{
			legSpringTargetAnimLerp += 2f * Time.deltaTime;
			if (legSpringTargetAnimLerp >= 1f)
			{
				legSpringTargetAnimLerp = 0f;
			}
		}
		if (!legsActive)
		{
			if (legShow)
			{
				legShowLerp += 5f * Time.deltaTime;
				legShowLerp = Mathf.Clamp01(legShowLerp);
				array = legShowTransforms;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].localScale = Vector3.LerpUnclamped(Vector3.one, Vector3.zero, legHideCurve.Evaluate(legShowLerp));
				}
				if (legShowLerp >= 1f)
				{
					array = legShowTransforms;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].gameObject.SetActive(value: false);
					}
					legShow = false;
				}
			}
		}
		else
		{
			if (!legShow)
			{
				array = legShowTransforms;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].gameObject.SetActive(value: true);
				}
				legShow = true;
			}
			if (legShowLerp < 1f)
			{
				legShowLerp += 3f * Time.deltaTime;
				legShowLerp = Mathf.Clamp01(legShowLerp);
				array = legShowTransforms;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, legShowCurve.Evaluate(legShowLerp));
				}
			}
		}
		if (overridePositionRotationTimer > 0f)
		{
			base.transform.position = overridePositionStart;
			base.transform.rotation = overrideRotationStart;
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				physGrabObject.Teleport(overridePositionStart, overrideRotationStart);
			}
			overridePositionRotationTimer -= Time.deltaTime;
			if (overridePositionRotationTimer <= 0f)
			{
				OverridePositionRotationReset();
			}
		}
		else if (overridePositionRotationLerp < 1f)
		{
			overridePositionRotationLerp += 5f * Time.deltaTime;
			float t2 = overridePositionRotationCurve.Evaluate(overridePositionRotationLerp);
			meshParent.transform.position = Vector3.Lerp(overridePositionStart, base.transform.position, t2);
			meshParent.transform.rotation = Quaternion.Slerp(overrideRotationStart, base.transform.rotation, t2);
			if (overridePositionRotationLerp >= 1f)
			{
				meshParent.transform.localPosition = Vector3.zero;
				meshParent.transform.localRotation = Quaternion.identity;
			}
		}
	}

	private void LateUpdate()
	{
		if (overridePositionRotationTimer > 0f)
		{
			if ((bool)overridePositionTransform)
			{
				meshParent.transform.position = overridePositionTransform.position;
				meshParent.transform.rotation = overridePositionTransform.rotation;
			}
			else
			{
				meshParent.transform.position = overridePositionStart;
				meshParent.transform.rotation = overrideRotationStart;
			}
		}
	}

	private void FixedUpdate()
	{
		if (!SemiFunc.IsMasterClient())
		{
			return;
		}
		if (spectated && !overrideSpectated)
		{
			if (spectatedLookAtCooldown <= 0f)
			{
				float num = playerAvatar.voiceChat.clipLoudnessNoTTS + playerAvatar.voiceChat.clipLoudnessTTS * 0.2f;
				Quaternion targetRotation = Quaternion.Euler(playerAvatar.localCamera.transform.rotation.eulerAngles.x - Mathf.Min(num * 100f, 40f), playerAvatar.localCamera.transform.rotation.eulerAngles.y, playerAvatar.localCamera.transform.rotation.eulerAngles.z);
				float num2 = 10f * spectatedLookAtLerp;
				if (physGrabObject.playerGrabbing.Count <= 0 && physGrabObject.rb.velocity.magnitude >= 2f && spectatedJumpedTimer <= 0f)
				{
					num2 = 10f - physGrabObject.rb.velocity.magnitude * 20f;
					spectatedLookAtLerp = 0f;
				}
				else
				{
					spectatedLookAtLerp += 0.25f * Time.fixedDeltaTime;
					spectatedLookAtLerp = Mathf.Clamp01(spectatedLookAtLerp);
				}
				Vector3 vector = SemiFunc.PhysFollowRotation(base.transform, targetRotation, physGrabObject.rb, 5f);
				vector = Vector3.Lerp(Vector3.zero, vector, num2 * Time.fixedDeltaTime);
				physGrabObject.rb.AddTorque(vector, ForceMode.Impulse);
				physGrabObject.OverrideTorqueStrength(0f);
			}
			else
			{
				spectatedLookAtCooldown -= Time.fixedDeltaTime;
			}
		}
		else
		{
			spectatedLookAtCooldown = 1f;
		}
		if (spectatedJumpForce > 0f)
		{
			if (!overrideSpectated)
			{
				physGrabObject.rb.AddForce(playerAvatar.localCamera.transform.forward * (4f * spectatedJumpForce), ForceMode.Impulse);
			}
			spectatedJumpForce = 0f;
			spectatedJumpedTimer = 0.5f;
			spectatedLookAtLerp = 1f;
		}
	}

	private void UpdateColor()
	{
		MeshRenderer[] array = colorMeshRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			if ((bool)meshRenderer)
			{
				meshRenderer.material = playerAvatar.playerHealth.bodyMaterial;
				meshRenderer.material.SetFloat(Shader.PropertyToID("_ColorOverlayAmount"), 0f);
			}
		}
		Color color = playerAvatar.playerAvatarVisuals.color;
		physGrabObject.impactDetector.particles.gradient = new Gradient
		{
			colorKeys = new GradientColorKey[2]
			{
				new GradientColorKey(color, 0f),
				new GradientColorKey(color, 1f)
			}
		};
	}

	public void Revive()
	{
		if (triggered && inExtractionPoint)
		{
			playerAvatar.Revive();
		}
	}

	public void Trigger()
	{
		seenCooldownTimer = seenCooldownTime;
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			if (playerAvatar.isLocal)
			{
				PlayerController.instance.col.enabled = false;
			}
			else
			{
				playerAvatar.playerAvatarCollision.Collider.enabled = false;
			}
			Collider[] array = playerAvatar.tumble.colliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			triggeredPosition = playerAvatar.playerAvatarCollision.deathHeadPosition;
			triggeredRotation = playerAvatar.localCamera.transform.rotation;
			triggeredTimer = 0.1f;
		}
		physGrabObject.DisableDeathPitEffect(5f);
		UpdateColor();
		triggered = true;
		SetColliders(_enabled: true);
	}

	public void Reset()
	{
		triggered = false;
		smokeParticleTimer = smokeParticleTime;
		localSeenEffectTimer = localSeenEffectTime;
		localSeen = false;
		localSeenEffect = false;
		SetColliders(_enabled: false);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			physGrabObject.Teleport(new Vector3(0f, 3000f, 0f), Quaternion.identity);
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("SeenSetRPC", RpcTarget.All, false);
			}
			else
			{
				SeenSetRPC(_toggle: false);
			}
		}
	}

	private void SetColliders(bool _enabled)
	{
		if (!_enabled)
		{
			foreach (PhysGrabber item in physGrabObject.playerGrabbing.ToList())
			{
				if (item.isLocal)
				{
					item.ReleaseObject(photonView.ViewID, 0.25f);
				}
			}
		}
		Collider[] array = colliders;
		foreach (Collider collider in array)
		{
			if ((bool)collider)
			{
				collider.enabled = _enabled;
			}
		}
	}

	private void OnDestroy()
	{
		if (spectated)
		{
			SpectatedSetRPCLogic(_active: false);
		}
	}

	public void SpectatedSet(bool _active)
	{
		if (_active)
		{
			spectatedTimer = 0.1f;
		}
		else
		{
			spectatedTimer = 0f;
		}
		if (spectated != _active)
		{
			if (!_active)
			{
				SpectatedLowEnergySet(_active: false);
			}
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("SpectatedSetRPC", RpcTarget.All, _active);
			}
			else
			{
				SpectatedSetRPC(_active);
			}
		}
	}

	[PunRPC]
	private void SpectatedSetRPC(bool _active, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, playerAvatar.photonView))
		{
			SpectatedSetRPCLogic(_active);
		}
	}

	private void SpectatedSetRPCLogic(bool _active)
	{
		if (_active)
		{
			if (playerAvatar.photonView.IsMine)
			{
				foreach (MeshRenderer meshRenderer in meshRenderers)
				{
					meshRenderer.gameObject.SetActive(value: false);
				}
				arenaCrown.SetActive(value: false);
			}
			else
			{
				spectatedParticle.SetActive(value: true);
			}
			spectatedSoundStart.Play(physGrabObject.centerPoint);
		}
		else
		{
			if (playerAvatar.photonView.IsMine)
			{
				foreach (MeshRenderer meshRenderer2 in meshRenderers)
				{
					meshRenderer2.gameObject.SetActive(value: true);
				}
			}
			if (SemiFunc.IsMultiplayer() && playerAvatar == SessionManager.instance.CrownedPlayerGet())
			{
				arenaCrown.SetActive(value: true);
			}
			spectatedSoundStop.Play(physGrabObject.centerPoint);
			spectatedParticle.SetActive(value: true);
		}
		if (SemiFunc.IsMasterClient())
		{
			physGrabObject.rb.AddForce(Vector3.up * 1f, ForceMode.Impulse);
			physGrabObject.rb.AddTorque(-physGrabObject.transform.right * 0.2f, ForceMode.Impulse);
		}
		if ((bool)playerAvatar.voiceChat && playerAvatar.isDisabled)
		{
			playerAvatar.voiceChat.ToggleMixer(!_active, _distorted: true);
		}
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 2f, 8f, base.transform.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 2f, 8f, base.transform.position, 0.1f);
		eyeNoiseAlpha = 1f;
		spectated = _active;
	}

	public void SpectatedLowEnergySet(bool _active)
	{
		if (spectatedLowEnergy != _active)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("SpectatedLowEnergySetRPC", RpcTarget.All, _active);
			}
			else
			{
				SpectatedLowEnergySetRPC(_active);
			}
		}
	}

	[PunRPC]
	private void SpectatedLowEnergySetRPC(bool _active, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, playerAvatar.photonView))
		{
			spectatedLowEnergy = _active;
		}
	}

	[PunRPC]
	private void SpectatedJumpSpinRPC(float _force, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			if (playerAvatar.isLocal)
			{
				GameDirector.instance.CameraImpact.Shake(3f, 0.05f);
				GameDirector.instance.CameraShake.Shake(5f, 0.1f);
			}
			spectatedJumpSpinLerp = 0f;
		}
	}

	private void SpectatedJumpLocalInput(bool _input)
	{
		if (_input != spectatedJumpLocalPlayerInput)
		{
			spectatedJumpLocalPlayerInput = _input;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("SpectatedJumpLocalInputRPC", RpcTarget.MasterClient, spectatedJumpLocalPlayerInput);
			}
			else
			{
				SpectatedJumpLocalInputRPC(spectatedJumpLocalPlayerInput);
			}
		}
	}

	[PunRPC]
	private void SpectatedJumpLocalInputRPC(bool _input, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, playerAvatar.photonView))
		{
			spectatedJumpLocalPlayerInput = _input;
		}
	}

	public void OverrideSpectated(float _time)
	{
		if (!overrideSpectated)
		{
			SetColliders(_enabled: false);
		}
		overrideSpectated = true;
		overrideSpectatedTimer = _time;
		physGrabObject.rb.isKinematic = true;
	}

	public void OverrideSpectatedReset()
	{
		if (overrideSpectated)
		{
			SetColliders(_enabled: true);
		}
		overrideSpectated = false;
		overrideSpectatedTimer = 0f;
		physGrabObject.rb.isKinematic = false;
	}

	[PunRPC]
	private void OverrideSpectatedRPC(bool _active, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			overrideSpectated = _active;
			SetColliders(!overrideSpectated);
		}
	}

	public void LegsOverrideDisable(float _time)
	{
		legsOverrideDisableTimer = _time;
	}

	public void OverridePositionRotation(Transform _followTransform, Vector3 _releasePosition, Quaternion _releaseRotation, float _time)
	{
		overridePositionTransform = _followTransform;
		overridePositionStart = _followTransform.position;
		overrideRotationStart = _followTransform.rotation;
		overridePositionEnd = _releasePosition;
		overrideRotationEnd = _releaseRotation;
		overridePositionRotationTimer = _time;
	}

	public void OverridePositionRotationReset()
	{
		overridePositionRotationTimer = 0f;
		overridePositionRotationLerp = 0f;
		base.transform.position = overridePositionEnd;
		base.transform.rotation = overrideRotationEnd;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			physGrabObject.Teleport(overridePositionEnd, overrideRotationEnd);
		}
		meshParent.transform.position = overridePositionStart;
		meshParent.transform.rotation = overrideRotationStart;
	}

	[PunRPC]
	public void TriggerDoneRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			if ((bool)smokeParticles)
			{
				smokeParticles.Play();
			}
			smokeParticleRateOverTimeCurrent = smokeParticleRateOverTimeDefault;
			smokeParticleRateOverDistanceCurrent = smokeParticleRateOverDistanceDefault;
		}
	}

	[PunRPC]
	public void SetupRPC(string _steamID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.steamID == _steamID)
			{
				playerAvatar = player;
				playerAvatar.playerDeathHead = this;
				break;
			}
		}
		StartCoroutine(SetupClient());
	}

	[PunRPC]
	public void FlashEyeRPC(int _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			switch (_state)
			{
			case 0:
				inExtractionPoint = false;
				break;
			case 1:
				inExtractionPoint = true;
				break;
			}
			switch (_state)
			{
			case 0:
				eyeMaterial.SetColor(eyeMaterialColor, eyeFlashNegativeColor);
				eyeFlashNegativeSound.Play(base.transform.position);
				eyeFlashLight.color = eyeFlashNegativeColor;
				break;
			case 1:
				eyeMaterial.SetColor(eyeMaterialColor, eyeFlashPositiveColor);
				eyeFlashPositiveSound.Play(base.transform.position);
				eyeFlashLight.color = eyeFlashPositiveColor;
				break;
			case 2:
				eyeMaterial.SetColor(eyeMaterialColor, spectatedColorEye);
				eyeFlashLight.color = spectatedColorEye;
				break;
			}
			eyeFlash = true;
			eyeFlashLerp = 0f;
			eyeFlashLight.gameObject.SetActive(value: true);
			GameDirector.instance.CameraImpact.ShakeDistance(1f, 2f, 8f, base.transform.position, 0.25f);
			GameDirector.instance.CameraShake.ShakeDistance(1f, 2f, 8f, base.transform.position, 0.5f);
		}
	}

	[PunRPC]
	public void SeenSetRPC(bool _toggle, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info) || (triggered && _toggle))
		{
			serverSeen = _toggle;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(spectatedJumpChargeAmount);
				return;
			}
			spectatedJumpChargeAmount = (float)stream.ReceiveNext();
			spectatedJumpChargeAmount = Mathf.Min(spectatedJumpChargeAmount, spectatedJumpChargeAmountMax);
		}
	}
}
