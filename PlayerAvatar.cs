using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAvatar : MonoBehaviour, IPunObservable
{
	public PhotonView photonView;

	public Transform playerTransform;

	public Transform lowPassRaycastPoint;

	public GameObject spectateCamera;

	public Transform spectatePoint;

	public PhysGrabber physGrabber;

	public PlayerPhysPusher playerPhysPusher;

	public PlayerAvatarVisuals playerAvatarVisuals;

	public PlayerExpression playerExpression;

	public PlayerHealth playerHealth;

	public FlashlightController flashlightController;

	public FlashlightLightAim flashlightLightAim;

	public MapToolController mapToolController;

	public PlayerDeathEffects playerDeathEffects;

	public PlayerReviveEffects playerReviveEffects;

	public PlayerDeathHead playerDeathHead;

	public PlayerHealthGrab healthGrab;

	public PlayerTumble tumble;

	public PlayerPhysObjectStander physObjectStander;

	public PlayerPhysObjectFinder physObjectFinder;

	public GameObject deathSpot;

	private Collider collider;

	internal string playerName;

	internal string steamID;

	[Space]
	public PlayerLocalCamera localCamera;

	[Space]
	public PlayerVisionTarget PlayerVisionTarget;

	public RoomVolumeCheck RoomVolumeCheck;

	public Materials.MaterialTrigger MaterialTrigger;

	[Space]
	internal bool isLocal;

	internal bool isDisabled;

	internal bool outroDone;

	internal bool spawned;

	private bool spawnImpulse = true;

	private int spawnFrames = 3;

	private bool spawnDoneImpulse = true;

	private Vector3 spawnPosition;

	internal Quaternion spawnRotation;

	internal bool finalHeal;

	internal bool isCrouching;

	internal bool isSprinting;

	internal bool isCrawling;

	internal bool isSliding;

	internal bool isMoving;

	internal bool isGrounded;

	internal bool isTumbling;

	private bool Interact;

	internal Vector3 InputDirection;

	internal Vector3 LastNavmeshPosition;

	internal float LastNavMeshPositionTimer;

	internal PlayerVoiceChat voiceChat;

	internal bool voiceChatFetched;

	private Rigidbody rb;

	internal Vector3 rbVelocity;

	internal Vector3 rbVelocityRaw;

	private float rbDiscreteTimer;

	internal Vector3 clientPosition = Vector3.zero;

	internal Vector3 clientPositionCurrent = Vector3.zero;

	internal float clientPositionDelta;

	internal Quaternion clientRotation = Quaternion.identity;

	internal Quaternion clientRotationCurrent = Quaternion.identity;

	public Sound jumpSound;

	public Sound extraJumpSound;

	public Sound landSound;

	public Sound slideSound;

	[Space]
	public Sound standToCrouchSound;

	public Sound crouchToStandSound;

	[Space]
	public Sound crouchToCrawlSound;

	public Sound crawlToCrouchSound;

	[Space]
	public Sound deathBuildupSound;

	public Sound deathSound;

	[Space]
	public Sound tumbleStartSound;

	public Sound tumbleStopSound;

	public Sound tumbleBreakFreeSound;

	[Space]
	public Sound truckReturn;

	public Sound truckReturnGlobal;

	internal bool clientPhysRiding;

	internal int clientPhysRidingID;

	internal Vector3 clientPhysRidingPosition;

	internal Transform clientPhysRidingTransform;

	public static PlayerAvatar instance;

	internal bool spectating;

	internal bool deadSet;

	private float deadTime = 0.5f;

	private float deadTimer;

	internal float enemyVisionFreezeTimer;

	private Transform deadEnemyLookAtTransform;

	internal int steamIDshort;

	internal PlayerAvatarCollision playerAvatarCollision;

	internal bool fallDamageResetState;

	private bool fallDamageResetStatePrevious;

	private float fallDamageResetTimer;

	private bool falling;

	internal int playerPing;

	private float playerPingTimer;

	internal bool quitApplication;

	private float overrrideAnimationSpeedTimer;

	private float overrrideAnimationSpeedTarget;

	private float overrrideAnimationSpeedIn;

	private float overrrideAnimationSpeedOut;

	private float overrideAnimationSpeedLerp;

	private bool overrideAnimationSpeedActive;

	private float overrideAnimationSpeedTime;

	private SpringFloat overridePupilSizeSpring = new SpringFloat();

	private bool overridePupilSizeActive;

	private float overridePupilSizeTimer;

	private float overridePupilSizeTime;

	private float overridePupilSizeMultiplier = 1f;

	private float overridePupilSizeMultiplierTarget = 1f;

	private float overridePupilSpringSpeedIn = 15f;

	private float overridePupilSpringDampIn = 0.3f;

	private float overridePupilSpringSpeedOut = 15f;

	private float overridePupilSpringDampOut = 0.3f;

	private int overridePupilSizePrio;

	internal int upgradeMapPlayerCount;

	internal bool levelAnimationCompleted;

	internal float upgradeTumbleClimb;

	internal float upgradeDeathHeadBattery;

	internal float upgradeCrouchRest;

	internal bool upgradeCrouchRestActive;

	internal float upgradeTumbleWings;

	internal bool upgradeTumbleWingsVisualsActive;

	internal WorldSpaceUIPlayerName worldSpaceUIPlayerName;

	internal Dictionary<int, float> playerExpressions = new Dictionary<int, float>();

	public ItemUpgradePlayerTumbleWingsLogic upgradeTumbleWingsLogic;

	internal float tumbleWingPinkTimer;

	private bool colorWasSet;

	private float noColorFailsafeTimer = 10f;

	internal bool selfDestructPrevented;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		photonView = GetComponent<PhotonView>();
		collider = GetComponentInChildren<Collider>();
		isDisabled = false;
		base.transform.position = Vector3.zero + Vector3.forward * 2f;
		playerAvatarCollision = GetComponent<PlayerAvatarCollision>();
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			PhotonView component = player.GetComponent<PhotonView>();
			if (photonView.Owner == component.Owner)
			{
				Object.Destroy(base.gameObject);
				return;
			}
		}
		GameDirector.instance.PlayerList.Add(this);
		if (!SemiFunc.IsMultiplayer() || photonView.IsMine)
		{
			isLocal = true;
		}
	}

	private void OnDestroy()
	{
		GameDirector.instance.PlayerList.Remove(this);
		foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
		{
			item?.Enemy.PlayerRemoved(photonView.ViewID);
		}
		Object.Destroy(base.transform.parent.gameObject);
	}

	private void Start()
	{
		overridePupilSizeSpring.speed = 15f;
		overridePupilSizeSpring.damping = 0.3f;
		deadTimer = deadTime;
		if (!SemiFunc.IsMultiplayer() || photonView.IsMine)
		{
			StartCoroutine(WaitForSteamID());
			playerTransform = PlayerController.instance.transform;
			playerTransform.position = base.transform.position;
			PlayerController.instance.playerAvatar = base.gameObject;
			PlayerController.instance.playerAvatarScript = base.gameObject.GetComponent<PlayerAvatar>();
			if ((bool)instance)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			instance = this;
		}
		SoundSetup(jumpSound);
		SoundSetup(extraJumpSound);
		SoundSetup(landSound);
		SoundSetup(slideSound);
		SoundSetup(standToCrouchSound);
		SoundSetup(crouchToStandSound);
		SoundSetup(crouchToCrawlSound);
		SoundSetup(crawlToCrouchSound);
		SoundSetup(tumbleStartSound);
		SoundSetup(tumbleStopSound);
		SoundSetup(tumbleBreakFreeSound);
		AddToStatsManager();
		if (SemiFunc.IsMasterClient() && LevelGenerator.Instance.Generated)
		{
			LevelGenerator.Instance.PlayerSpawn();
		}
		StartCoroutine(LateStart());
	}

	private IEnumerator WaitForSteamID()
	{
		while (steamID == null)
		{
			yield return null;
		}
		if (SemiFunc.IsMultiplayer())
		{
			PlayerAvatarSetColor(DataDirector.instance.ColorGetBody());
		}
		else if (!SemiFunc.IsMainMenu())
		{
			PlayerAvatarSetColor(DataDirector.instance.ColorGetBody());
		}
	}

	private IEnumerator LateStart()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.2f);
		if (StatsManager.instance.playerUpgradeMapPlayerCount.TryGetValue(steamID, out var value))
		{
			upgradeMapPlayerCount = value;
		}
		if (StatsManager.instance.playerUpgradeTumbleWings.TryGetValue(steamID, out var value2))
		{
			upgradeTumbleWings = value2;
		}
		if (StatsManager.instance.playerUpgradeCrouchRest.TryGetValue(steamID, out var value3))
		{
			upgradeCrouchRest = value3;
		}
		if (StatsManager.instance.playerUpgradeTumbleClimb.TryGetValue(steamID, out var value4))
		{
			upgradeTumbleClimb = value4;
		}
		if (StatsManager.instance.playerUpgradeDeathHeadBattery.TryGetValue(steamID, out var value5))
		{
			upgradeDeathHeadBattery = value5;
		}
		WorldSpaceUIParent.instance.PlayerName(this);
	}

	private void CrouchRestUpgrade()
	{
		upgradeCrouchRestActive = false;
		if ((!isCrouching && !isCrawling) || isSliding)
		{
			return;
		}
		float num = upgradeCrouchRest;
		if (!isLocal)
		{
			return;
		}
		bool flag = true;
		if (isTumbling && (tumble.notMovingTimer < 1f || physGrabber.grabState == PhysGrabber.GrabState.Climb))
		{
			flag = false;
		}
		if (flag)
		{
			float energyStart = PlayerController.instance.EnergyStart;
			num += 1f;
			float num2 = 1f;
			if (isMoving)
			{
				num2 = 0.5f;
			}
			PlayerController.instance.EnergyCurrent = Mathf.Min(PlayerController.instance.EnergyCurrent + num * Time.deltaTime * num2, energyStart);
			upgradeCrouchRestActive = true;
		}
	}

	private void AddToStatsManager()
	{
		string text = SemiFunc.PlayerGetName(this);
		string text2 = SteamClient.SteamId.Value.ToString();
		if (GameManager.Multiplayer() && GameManager.instance.localTest)
		{
			int num = 0;
			Player[] playerList = PhotonNetwork.PlayerList;
			for (int i = 0; i < playerList.Length; i++)
			{
				if (playerList[i].IsLocal)
				{
					text = text + " " + num;
					text2 += num;
				}
				num++;
			}
		}
		if (GameManager.Multiplayer())
		{
			if (photonView.IsMine)
			{
				photonView.RPC("AddToStatsManagerRPC", RpcTarget.AllBuffered, text, text2);
			}
		}
		else
		{
			AddToStatsManagerRPC(text, text2);
		}
	}

	private void FinalHealCheck()
	{
		if (isLocal && (SemiFunc.RunIsLevel() || SemiFunc.RunIsTutorial()) && SemiFunc.FPSImpulse5() && RoundDirector.instance.allExtractionPointsCompleted && RoomVolumeCheck.inTruck && !finalHeal && TruckHealer.instance.currentState == TruckHealer.State.Open)
		{
			FinalHeal();
		}
	}

	private void FinalHeal()
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("FinalHealRPC", RpcTarget.All);
		}
		else
		{
			FinalHealRPC();
		}
	}

	[PunRPC]
	public void FinalHealRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, photonView) && !finalHeal)
		{
			int healAmount = 50;
			if (SemiFunc.MoonLevel() == 1)
			{
				healAmount = 35;
			}
			else if (SemiFunc.MoonLevel() >= 2)
			{
				healAmount = 25;
			}
			if (isLocal)
			{
				playerHealth.EyeMaterialOverride(PlayerHealth.EyeOverrideState.Green, 2f, 1);
				playerHealth.Heal(healAmount);
			}
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				TruckScreenText.instance.MessageSendCustom("", playerName + " {arrowright}{truck}{check}\n {point}{shades}{pointright}<b><color=#00FF00>+" + healAmount + "</color></b>{heart}", 0);
			}
			TruckHealer.instance.Heal(this);
			truckReturn.Play(PlayerVisionTarget.VisionTransform.position);
			truckReturnGlobal.Play(PlayerVisionTarget.VisionTransform.position);
			playerAvatarVisuals.effectGetIntoTruck.gameObject.SetActive(value: true);
			finalHeal = true;
		}
	}

	[PunRPC]
	public void AddToStatsManagerRPC(string _playerName, string _steamID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, photonView))
		{
			playerName = _playerName;
			steamID = _steamID;
			if (!SemiFunc.IsMultiplayer() || (SemiFunc.IsMultiplayer() && photonView.IsMine))
			{
				PlayerController.instance.PlayerSetName(playerName, steamID);
			}
			if ((bool)StatsManager.instance)
			{
				StatsManager.instance.PlayerAdd(_steamID, _playerName);
			}
		}
	}

	[PunRPC]
	public void UpdateMyPlayerVoiceChat(int photonViewID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		photonView = GetComponent<PhotonView>();
		if (SemiFunc.OwnerOnlyRPC(_info, photonView))
		{
			voiceChat = PhotonView.Find(photonViewID).GetComponent<PlayerVoiceChat>();
			voiceChat.playerAvatar = this;
			if (voiceChat.TTSinstantiated)
			{
				voiceChat.ttsVoice.playerAvatar = this;
			}
			if (!SemiFunc.MenuLevel())
			{
				voiceChat.ToggleMixer(_lobby: false);
			}
			voiceChatFetched = true;
		}
	}

	[PunRPC]
	public void ResetPhysPusher()
	{
		playerPhysPusher.Reset = true;
	}

	public void SetDisabled()
	{
		if (GameManager.Multiplayer())
		{
			if (photonView.IsMine)
			{
				photonView.RPC("SetDisabledRPC", RpcTarget.All);
				PlayerVoiceChat.instance.OverridePitchCancel();
			}
		}
		else
		{
			SetDisabledRPC();
		}
	}

	[PunRPC]
	public void SetDisabledRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, photonView))
		{
			isDisabled = true;
		}
	}

	public void UpdateState(bool isCrouching, bool isSprinting, bool isCrawling, bool isSliding, bool isMoving)
	{
		SetState(isCrouching, isSprinting, isCrawling, isSliding, isMoving);
	}

	private void FixedUpdate()
	{
		OverridePupilSizeTick();
		OverrideAnimationSpeedTick();
		if (SemiFunc.IsMultiplayer() && isLocal)
		{
			playerPingTimer -= Time.deltaTime;
			if (playerPingTimer <= 0f)
			{
				playerPing = PhotonNetwork.GetPing();
				playerPingTimer = 6f;
			}
		}
		if (!LevelGenerator.Instance.Generated)
		{
			if (!spawned)
			{
				return;
			}
			clientPosition = spawnPosition;
			clientPositionCurrent = spawnPosition;
			clientRotation = spawnRotation;
			clientRotationCurrent = spawnRotation;
			base.transform.position = spawnPosition;
			base.transform.rotation = spawnRotation;
			rb.MovePosition(base.transform.position);
			rb.MoveRotation(base.transform.rotation);
			if (PlayerController.instance.playerAvatarScript == this)
			{
				PlayerController.instance.transform.position = spawnPosition;
				PlayerController.instance.transform.rotation = spawnRotation;
			}
			if (!spawnImpulse)
			{
				return;
			}
			if (spawnFrames <= 0)
			{
				if (GameManager.Multiplayer())
				{
					LevelGenerator.Instance.PhotonView.RPC("PlayerSpawnedRPC", RpcTarget.All);
				}
				else
				{
					LevelGenerator.Instance.playerSpawned++;
				}
				spawnImpulse = false;
			}
			else
			{
				spawnFrames--;
			}
			return;
		}
		if (spawnDoneImpulse)
		{
			if (PlayerController.instance.playerAvatarScript == this)
			{
				if ((bool)TruckScreenText.instance && !SemiFunc.MenuLevel())
				{
					Vector3 position = TruckScreenText.instance.transform.position;
					Quaternion rotation = Quaternion.LookRotation(position - base.transform.position);
					CameraAim.Instance.SetPlayerAim(rotation, _setRotation: true);
					CameraAim.Instance.AimTargetSet(position, 0.3f, 4f, base.gameObject, 0);
				}
				else
				{
					CameraAim.Instance.SetPlayerAim(spawnRotation, _setRotation: true);
				}
				if (SemiFunc.MenuLevel())
				{
					PlayerController.instance.rb.isKinematic = false;
				}
			}
			rb.isKinematic = false;
			spawnDoneImpulse = false;
		}
		if (photonView.IsMine || !SemiFunc.IsMultiplayer())
		{
			rbVelocity = PlayerController.instance.VelocityRelative;
			rbVelocityRaw = PlayerController.instance.rb.velocity;
			rb.MovePosition(base.transform.position);
			rb.MoveRotation(base.transform.rotation);
		}
		else
		{
			rb.MovePosition(clientPositionCurrent);
			rb.MoveRotation(clientRotationCurrent);
		}
	}

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		CrouchRestUpgrade();
		FinalHealCheck();
		OverrideAnimationSpeedLogic();
		OverridePupilSizeLogic();
		if (GameManager.Multiplayer() && GameDirector.instance.currentState >= GameDirector.gameState.Main)
		{
			if (voiceChatFetched)
			{
				if (!isDisabled)
				{
					Vector3 vector = PlayerVisionTarget.VisionTransform.transform.position;
					if (voiceChat.overridePositionTimer > 0f)
					{
						vector = voiceChat.overridePosition;
					}
					voiceChat.transform.position = Vector3.Lerp(voiceChat.transform.position, vector, 30f * Time.deltaTime);
				}
			}
			else if (photonView.IsMine && (bool)PlayerVoiceChat.instance)
			{
				photonView.RPC("UpdateMyPlayerVoiceChat", RpcTarget.AllBuffered, PlayerVoiceChat.instance.photonView.ViewID);
			}
		}
		if (photonView.IsMine || GameManager.instance.gameMode == 0)
		{
			if ((bool)playerTransform)
			{
				base.transform.position = playerTransform.position;
				base.transform.rotation = playerTransform.rotation;
			}
			InputDirection = PlayerController.instance.InputDirection;
		}
		else
		{
			clientPositionCurrent = clientPosition;
			clientRotationCurrent = clientRotation;
		}
		if (deadSet)
		{
			if (isLocal && (bool)deadEnemyLookAtTransform)
			{
				CameraAim.Instance.AimTargetSet(deadEnemyLookAtTransform.position, 1f, 80f, deadEnemyLookAtTransform.gameObject, 0);
			}
			deadTimer -= Time.deltaTime;
			if (deadTimer <= 0f)
			{
				PlayerDeathDone();
			}
		}
		if ((bool)tumble)
		{
			isTumbling = tumble.isTumbling;
		}
		if (isTumbling)
		{
			collider.enabled = false;
		}
		else
		{
			collider.enabled = true;
		}
		LastNavMeshPositionTimer += Time.deltaTime;
		if (Physics.Raycast(base.transform.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo, 2f, LayerMask.GetMask("Default", "NavmeshOnly", "PlayerOnlyCollision")) && NavMesh.SamplePosition(hitInfo.point, out var hit, 0.5f, -1))
		{
			LastNavmeshPosition = hit.position;
			LastNavMeshPositionTimer = 0f;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && GameDirector.instance.currentState == GameDirector.gameState.Main)
		{
			if (base.transform.position.y < -100f)
			{
				tumble.TumbleSet(_isTumbling: true, _playerInput: false);
				tumble.physGrabObject.Teleport(TruckSafetySpawnPoint.instance.transform.position, TruckSafetySpawnPoint.instance.transform.rotation);
				tumble.physGrabObject.rb.velocity = Vector3.zero;
				tumble.physGrabObject.rb.angularVelocity = Vector3.zero;
				FallDamageResetSet(2f);
			}
			FallDamageResetLogic();
		}
		if (enemyVisionFreezeTimer > 0f)
		{
			enemyVisionFreezeTimer -= Time.deltaTime;
		}
		if (isLocal)
		{
			bool flag = false;
			if (PlayerController.instance.CollisionController.fallDistance >= 8f)
			{
				float fallDistance = PlayerController.instance.CollisionController.fallDistance;
				float num = 5f;
				float num2 = 4f;
				if (fallDistance > num)
				{
					int damage = 5;
					if (fallDistance > num + num2 * 4f)
					{
						damage = 100;
					}
					else if (fallDistance > num + num2 * 3f)
					{
						damage = 50;
					}
					else if (fallDistance > num + num2 * 2f)
					{
						damage = 25;
					}
					else if (fallDistance > num + num2)
					{
						damage = 15;
					}
					tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
					if (SemiFunc.FPSImpulse15())
					{
						tumble.ImpactHurtSet(0.5f, damage);
					}
					flag = true;
				}
			}
			FallingSet(flag);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && falling)
		{
			tumble.TumbleOverrideTime(2f);
		}
		if (!colorWasSet)
		{
			noColorFailsafeTimer -= Time.deltaTime;
			if (noColorFailsafeTimer <= 0f)
			{
				playerAvatarVisuals.SetColor(0);
				colorWasSet = true;
			}
		}
	}

	public void SetState(bool crouching, bool sprinting, bool crawling, bool sliding, bool moving)
	{
		isCrouching = crouching;
		isSprinting = sprinting;
		isCrawling = crawling;
		isSliding = sliding;
		isMoving = moving;
	}

	private void OverrideAnimationSpeedActivate(bool active, float _speedMulti, float _in, float _out, float _time = 0.1f)
	{
		if (isLocal)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("OverrideAnimationSpeedActivateRPC", RpcTarget.All, active, _speedMulti, _in, _out, _time);
			}
			else
			{
				OverrideAnimationSpeedActivateRPC(active, _speedMulti, _in, _out, _time);
			}
		}
	}

	[PunRPC]
	public void OverrideAnimationSpeedActivateRPC(bool active, float _speedMulti, float _in, float _out, float _time = 0.1f)
	{
		overrideAnimationSpeedActive = active;
		overrrideAnimationSpeedTimer = _time;
		overrrideAnimationSpeedTarget = _speedMulti;
		overrrideAnimationSpeedIn = _in;
		overrrideAnimationSpeedOut = _out;
		overrideAnimationSpeedTime = _time;
	}

	public void OverrideAnimationSpeed(float _speedMulti, float _in, float _out, float _time = 0.1f)
	{
		float num = overrrideAnimationSpeedTarget;
		overrrideAnimationSpeedTimer = _time;
		overrrideAnimationSpeedTarget = _speedMulti;
		overrrideAnimationSpeedIn = _in;
		overrrideAnimationSpeedOut = _out;
		overrideAnimationSpeedTime = _time;
		if (SemiFunc.IsMultiplayer() && (!overrideAnimationSpeedActive || num != _speedMulti))
		{
			OverrideAnimationSpeedActivate(active: true, _speedMulti, _in, _out, _time);
		}
	}

	private void OverrideAnimationSpeedTick()
	{
		if (overrrideAnimationSpeedTimer > 0f)
		{
			overrrideAnimationSpeedTimer -= Time.fixedDeltaTime;
			if (overrrideAnimationSpeedTimer <= 0f && SemiFunc.IsMultiplayer() && overrideAnimationSpeedActive)
			{
				OverrideAnimationSpeedActivate(active: false, overrrideAnimationSpeedTarget, overrrideAnimationSpeedIn, overrrideAnimationSpeedOut, overrideAnimationSpeedTime);
			}
		}
	}

	private void OverrideAnimationSpeedLogic()
	{
		if ((bool)playerAvatarVisuals && (!(overrrideAnimationSpeedTimer <= 0f) || playerAvatarVisuals.animationSpeedMultiplier != 1f))
		{
			if (!isLocal && overrideAnimationSpeedActive)
			{
				OverrideAnimationSpeed(overrrideAnimationSpeedTarget, overrrideAnimationSpeedIn, overrrideAnimationSpeedOut, overrideAnimationSpeedTime);
			}
			if (overrrideAnimationSpeedTimer > 0f)
			{
				overrideAnimationSpeedLerp = Mathf.Lerp(overrideAnimationSpeedLerp, 1f, Time.deltaTime * overrrideAnimationSpeedIn);
			}
			else
			{
				overrideAnimationSpeedLerp = Mathf.Lerp(overrideAnimationSpeedLerp, 0f, Time.deltaTime * overrrideAnimationSpeedOut);
			}
			playerAvatarVisuals.animationSpeedMultiplier = Mathf.Lerp(1f, overrrideAnimationSpeedTarget, overrideAnimationSpeedLerp);
			if (playerAvatarVisuals.animationSpeedMultiplier > 0.98f)
			{
				playerAvatarVisuals.animationSpeedMultiplier = 1f;
			}
		}
	}

	private void OverridePupilSizeActivate(bool active, float _multiplier, int _prio, float springSpeedIn, float dampIn, float springSpeedOut, float dampOut, float _time = 0.1f)
	{
		if (isLocal)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("OverridePupilSizeActivateRPC", RpcTarget.All, active, _multiplier, _prio, springSpeedIn, dampIn, springSpeedOut, dampOut, _time);
			}
			else
			{
				OverridePupilSizeActivateRPC(active, _multiplier, _prio, springSpeedIn, dampIn, springSpeedOut, dampOut, _time);
			}
		}
	}

	[PunRPC]
	public void OverridePupilSizeActivateRPC(bool active, float _multiplier, int _prio, float springSpeedIn, float dampIn, float springSpeedOut, float dampOut, float _time = 0.1f)
	{
		overridePupilSizeActive = active;
		overridePupilSizeMultiplier = _multiplier;
		overridePupilSizeMultiplierTarget = _multiplier;
		overridePupilSizePrio = _prio;
		overridePupilSpringSpeedIn = springSpeedIn;
		overridePupilSpringDampIn = dampIn;
		overridePupilSpringSpeedOut = springSpeedOut;
		overridePupilSpringDampOut = dampOut;
		overridePupilSizeTime = _time;
	}

	public void OverridePupilSize(float _multiplier, int _prio, float springSpeedIn, float springDampIn, float springSpeedOut, float springDampOut, float _time = 0.1f)
	{
		if (!(overridePupilSizeTimer > 0f) || _prio >= overridePupilSizePrio)
		{
			float num = overridePupilSizeMultiplierTarget;
			overridePupilSizeMultiplier = _multiplier;
			overridePupilSizeMultiplierTarget = _multiplier;
			overridePupilSizePrio = _prio;
			overridePupilSpringSpeedIn = springSpeedIn;
			overridePupilSpringDampIn = springDampIn;
			overridePupilSpringSpeedOut = springSpeedOut;
			overridePupilSpringDampOut = springDampOut;
			overridePupilSizeTime = _time;
			overridePupilSizeTimer = _time;
			if (SemiFunc.IsMultiplayer() && (!overridePupilSizeActive || num != _multiplier))
			{
				OverridePupilSizeActivate(active: true, _multiplier, _prio, springSpeedIn, springDampIn, springSpeedOut, springDampOut, _time);
			}
		}
	}

	private void OverridePupilSizeTick()
	{
		if (overridePupilSizeTimer > 0f)
		{
			overridePupilSizeTimer -= Time.fixedDeltaTime;
			if (overridePupilSizeTimer <= 0f && SemiFunc.IsMultiplayer() && overridePupilSizeActive)
			{
				OverridePupilSizeActivate(active: false, overridePupilSizeMultiplierTarget, overridePupilSizePrio, overridePupilSpringSpeedIn, overridePupilSpringDampIn, overridePupilSpringSpeedOut, overridePupilSpringDampOut, overridePupilSizeTime);
			}
		}
	}

	private void OverridePupilSizeLogic()
	{
		if ((bool)playerAvatarVisuals)
		{
			if (!isLocal && overridePupilSizeActive)
			{
				OverridePupilSize(overridePupilSizeMultiplierTarget, overridePupilSizePrio, overridePupilSpringSpeedIn, overridePupilSpringDampIn, overridePupilSpringSpeedOut, overridePupilSpringDampOut, overridePupilSizeTime);
			}
			if (overridePupilSizeTimer > 0f)
			{
				overridePupilSizeSpring.speed = overridePupilSpringSpeedIn;
				overridePupilSizeSpring.damping = overridePupilSpringDampIn;
				playerAvatarVisuals.playerEyes.pupilSizeMultiplier = SemiFunc.SpringFloatGet(overridePupilSizeSpring, overridePupilSizeMultiplierTarget);
			}
			else
			{
				overridePupilSizeSpring.speed = overridePupilSpringSpeedOut;
				overridePupilSizeSpring.damping = overridePupilSpringDampOut;
				playerAvatarVisuals.playerEyes.pupilSizeMultiplier = SemiFunc.SpringFloatGet(overridePupilSizeSpring, 1f);
			}
		}
	}

	public void SetSpectate()
	{
		Object.Instantiate(spectateCamera).GetComponent<SpectateCamera>().SetDeath(spectatePoint);
		spectating = true;
	}

	public void SoundSetup(Sound _sound)
	{
		if (photonView.IsMine)
		{
			_sound.SpatialBlend = 0f;
			return;
		}
		_sound.Volume *= 0.5f;
		_sound.VolumeRandom *= 0.5f;
		_sound.SpatialBlend = 1f;
	}

	public void EnemyVisionFreezeTimerSet(float _time)
	{
		enemyVisionFreezeTimer = _time;
	}

	private void FallingSet(bool _falling)
	{
		if (falling != _falling)
		{
			falling = _falling;
			if (!GameManager.Multiplayer())
			{
				FallingSetRPC(falling);
				return;
			}
			photonView.RPC("FallingSetRPC", RpcTarget.MasterClient, falling);
		}
	}

	[PunRPC]
	private void FallingSetRPC(bool _falling)
	{
		falling = _falling;
	}

	public void FlashlightFlicker(float _multiplier)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("FlashlightFlickerRPC", RpcTarget.All, _multiplier);
		}
		else
		{
			FlashlightFlickerRPC(_multiplier);
		}
	}

	[PunRPC]
	public void FlashlightFlickerRPC(float _multiplier, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, photonView))
		{
			flashlightController.FlickerSet(_multiplier);
		}
	}

	public void Slide()
	{
		slideSound.Play(base.transform.position);
		if (!GameManager.Multiplayer())
		{
			Materials.Instance.Slide(base.transform.position, MaterialTrigger, 0f, isPlayer: true);
			return;
		}
		Materials.Instance.Slide(base.transform.position, MaterialTrigger, 0f, isPlayer: true);
		photonView.RPC("SlideRPC", RpcTarget.Others);
	}

	[PunRPC]
	private void SlideRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, photonView))
		{
			slideSound.Play(base.transform.position);
			Materials.Instance.Slide(base.transform.position, MaterialTrigger, 1f, isPlayer: false);
		}
	}

	public void Jump(bool _powerupEffect)
	{
		if (GameManager.instance.gameMode == 0)
		{
			JumpRPC(_powerupEffect);
			return;
		}
		photonView.RPC("JumpRPC", RpcTarget.All, _powerupEffect);
	}

	[PunRPC]
	private void JumpRPC(bool _powerupEffect, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, photonView))
		{
			playerAvatarVisuals.JumpImpulse();
			jumpSound.Play(base.transform.position);
			Materials.HostType hostType = Materials.HostType.LocalPlayer;
			if (!isLocal)
			{
				hostType = Materials.HostType.OtherPlayer;
			}
			Materials.Instance.Impulse(base.transform.position, Vector3.down, Materials.SoundType.Light, footstep: true, footstepParticles: false, MaterialTrigger, hostType);
			if (_powerupEffect)
			{
				extraJumpSound.Play(base.transform.position);
				playerAvatarVisuals.PowerupJumpEffect();
			}
		}
	}

	public void Land()
	{
		if (GameManager.instance.gameMode == 0)
		{
			LandRPC();
		}
		else
		{
			photonView.RPC("LandRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void LandRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.OwnerOnlyRPC(_info, photonView))
		{
			return;
		}
		if (Physics.Raycast(base.transform.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo, 0.25f, SemiFunc.LayerMaskGetPhysGrabObject()))
		{
			PhysGrabObject component = hitInfo.transform.GetComponent<PhysGrabObject>();
			if ((bool)component)
			{
				component.mediumBreakImpulse = true;
				return;
			}
		}
		EnemyDirector.instance.SetInvestigate(base.transform.position + Vector3.up * 0.2f, 10f);
		Materials.HostType hostType = Materials.HostType.LocalPlayer;
		if (!isLocal)
		{
			hostType = Materials.HostType.OtherPlayer;
		}
		landSound.Play(base.transform.position);
		Materials.Instance.Impulse(base.transform.position, Vector3.down, Materials.SoundType.Heavy, footstep: true, footstepParticles: false, MaterialTrigger, hostType);
		Vector3 position = PlayerVisionTarget.VisionTransform.position;
		if (isLocal)
		{
			position = localCamera.transform.position;
		}
		SemiFunc.PlayerEyesOverrideSoft(position, 2f, base.gameObject, 5f);
	}

	public void Footstep(Materials.SoundType soundType, Vector3 _position = default(Vector3))
	{
		if ((bool)RecordingDirector.instance)
		{
			return;
		}
		if (_position == default(Vector3))
		{
			_position = base.transform.position;
		}
		Materials.HostType hostType = Materials.HostType.LocalPlayer;
		if (!isLocal)
		{
			hostType = Materials.HostType.OtherPlayer;
		}
		Materials.Instance.Impulse(_position, Vector3.down, soundType, footstep: true, footstepParticles: true, MaterialTrigger, hostType);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			switch (soundType)
			{
			case Materials.SoundType.Heavy:
				EnemyDirector.instance.SetInvestigate(_position + Vector3.up * 0.2f, 5f);
				break;
			case Materials.SoundType.Medium:
				EnemyDirector.instance.SetInvestigate(_position + Vector3.up * 0.2f, 1f);
				break;
			}
		}
	}

	public void StandToCrouch()
	{
		if (!isSprinting)
		{
			standToCrouchSound.Play(base.transform.position).pitch *= playerAvatarVisuals.animationSpeedMultiplier;
		}
	}

	private float GetPitchMulti()
	{
		return Mathf.Clamp(playerAvatarVisuals.animationSpeedMultiplier, 0.5f, 1.5f);
	}

	public void CrouchToStand()
	{
		AudioSource audioSource = crouchToStandSound.Play(base.transform.position);
		float pitchMulti = GetPitchMulti();
		audioSource.pitch *= pitchMulti;
	}

	public void CrouchToCrawl()
	{
		if (!isSliding && !isSprinting)
		{
			AudioSource audioSource = crouchToCrawlSound.Play(base.transform.position);
			float pitchMulti = GetPitchMulti();
			audioSource.pitch *= pitchMulti;
		}
	}

	public void CrawlToCrouch()
	{
		if (!isSliding && !isSprinting)
		{
			AudioSource audioSource = crawlToCrouchSound.Play(base.transform.position);
			float pitchMulti = GetPitchMulti();
			audioSource.pitch *= pitchMulti;
		}
	}

	public void TumbleStart()
	{
		AudioSource audioSource = tumbleStartSound.Play(base.transform.position);
		float pitchMulti = GetPitchMulti();
		audioSource.pitch *= pitchMulti;
	}

	public void TumbleStop()
	{
		AudioSource audioSource = tumbleStopSound.Play(base.transform.position);
		float pitchMulti = GetPitchMulti();
		audioSource.pitch *= pitchMulti;
	}

	public void TumbleBreakFree()
	{
		tumbleBreakFreeSound.Play(base.transform.position).pitch *= GetPitchMulti();
		playerAvatarVisuals.TumbleBreakFreeEffect();
	}

	public void PlayerGlitchShort()
	{
		if (GameManager.instance.gameMode == 0)
		{
			CameraGlitch.Instance.PlayShort();
		}
		else
		{
			photonView.RPC("PlayerGlitchShortRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void PlayerGlitchShortRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, photonView) && photonView.IsMine)
		{
			CameraGlitch.Instance.PlayShort();
		}
	}

	public void Spawn(Vector3 position, Quaternion rotation)
	{
		if (GameManager.instance.gameMode == 0)
		{
			SpawnRPC(position, rotation);
			return;
		}
		photonView.RPC("SpawnRPC", RpcTarget.All, position, rotation);
	}

	[PunRPC]
	private void SpawnRPC(Vector3 position, Quaternion rotation, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			if (!photonView)
			{
				photonView = GetComponent<PhotonView>();
			}
			if (!rb)
			{
				rb = GetComponent<Rigidbody>();
			}
			if (!GameManager.Multiplayer() || photonView.IsMine)
			{
				PlayerController.instance.transform.position = position;
				PlayerController.instance.transform.rotation = rotation;
			}
			rb.position = position;
			rb.rotation = rotation;
			base.transform.position = position;
			base.transform.rotation = rotation;
			clientPosition = position;
			clientPositionCurrent = position;
			clientRotation = rotation;
			clientRotationCurrent = rotation;
			spawnPosition = position;
			spawnRotation = rotation;
			playerAvatarVisuals.visualPosition = position;
			spawned = true;
		}
	}

	public void PlayerDeath(int enemyIndex)
	{
		if (!deadSet)
		{
			if (GameManager.instance.gameMode == 0)
			{
				PlayerDeathRPC(enemyIndex);
				return;
			}
			photonView.RPC("PlayerDeathRPC", RpcTarget.All, enemyIndex);
		}
	}

	[PunRPC]
	public void PlayerDeathRPC(int enemyIndex, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterAndOwnerOnlyRPC(_info, photonView))
		{
			return;
		}
		playerHealth.Death();
		deadSet = true;
		if (!isLocal)
		{
			deathBuildupSound.Play(base.transform.position);
		}
		if (!isLocal)
		{
			return;
		}
		deadEnemyLookAtTransform = null;
		Enemy enemy = SemiFunc.EnemyGetFromIndex(enemyIndex);
		if ((bool)enemy)
		{
			if ((bool)enemy.KillLookAtTransform)
			{
				deadEnemyLookAtTransform = enemy.KillLookAtTransform;
			}
			else
			{
				Debug.LogError("Enemy has no kill look at transform..." + enemy.name);
			}
		}
		physGrabber.ReleaseObject(-1);
		if ((bool)playerTransform)
		{
			playerTransform.parent.gameObject.SetActive(value: false);
		}
		CameraGlitch.Instance.PlayLongHurt();
		GameDirector.instance.DeathStart();
	}

	private void PlayerDeathDone()
	{
		if (voiceChatFetched)
		{
			voiceChat.OverrideMute(1f);
			voiceChat.ToggleMixer(_lobby: true);
		}
		if (SemiFunc.RunIsTutorial())
		{
			TutorialDirector.instance.deadPlayer = true;
		}
		if (isDisabled)
		{
			return;
		}
		isDisabled = true;
		Object.Instantiate(deathSpot, base.transform.position, Quaternion.identity);
		if (GameManager.Multiplayer())
		{
			if (!isLocal)
			{
				if ((bool)SpectateCamera.instance)
				{
					SpectateCamera.instance.UpdatePlayer(this);
				}
			}
			else
			{
				physGrabber.ReleaseObject(-1);
				if (Inventory.instance?.physGrabber?.photonView.ViewID == physGrabber.photonView.ViewID)
				{
					Inventory.instance.ForceUnequip();
				}
			}
		}
		deathSound.Play(base.transform.position);
		playerDeathHead.Trigger();
		playerDeathEffects.Trigger();
		base.gameObject.SetActive(value: false);
	}

	public void OutroStart()
	{
		if (GameManager.instance.gameMode == 0)
		{
			OutroStartRPC();
		}
		else
		{
			photonView.RPC("OutroStartRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	public void OutroStartRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info) && isLocal)
		{
			GameDirector.instance.OutroStart();
		}
	}

	public void OutroDone()
	{
		if (quitApplication)
		{
			Application.Quit();
		}
		else if (NetworkManager.instance.leavePhotonRoom)
		{
			NetworkManager.instance.LeavePhotonRoom();
		}
		else if (GameManager.instance.gameMode == 0)
		{
			OutroDoneRPC();
		}
		else
		{
			photonView.RPC("OutroDoneRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	public void OutroDoneRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, photonView))
		{
			outroDone = true;
		}
	}

	public void ForceImpulse(Vector3 _force)
	{
		if (!GameManager.Multiplayer())
		{
			ForceImpulseRPC(_force);
			return;
		}
		photonView.RPC("ForceImpulseRPC", RpcTarget.All, _force);
	}

	[PunRPC]
	private void ForceImpulseRPC(Vector3 _force, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterAndOwnerOnlyRPC(_info, photonView) && (!GameManager.Multiplayer() || photonView.IsMine))
		{
			PlayerController.instance.ForceImpulse(_force);
		}
	}

	public void PlayerAvatarSetColor(int colorIndex)
	{
		if (!GameManager.Multiplayer())
		{
			SetColorRPC(colorIndex);
			return;
		}
		photonView.RPC("SetColorRPC", RpcTarget.AllBuffered, colorIndex);
	}

	[PunRPC]
	private void SetColorRPC(int colorIndex, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, photonView) && colorIndex >= 0 && colorIndex < AssetManager.instance.playerColors.Count)
		{
			colorWasSet = true;
			if (isLocal)
			{
				DataDirector.instance.ColorSetBody(colorIndex);
			}
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				StatsManager.instance.SetPlayerColor(steamID, colorIndex);
			}
			playerAvatarVisuals.SetColor(colorIndex);
		}
	}

	public void Revive(bool _revivedByTruck = false)
	{
		if (GameManager.instance.gameMode == 0)
		{
			ReviveRPC(_revivedByTruck);
			return;
		}
		photonView.RPC("ReviveRPC", RpcTarget.All, _revivedByTruck);
	}

	[PunRPC]
	public void ReviveRPC(bool _revivedByTruck, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		if (!playerDeathHead)
		{
			Debug.LogError("Tried to revive without death head...");
			return;
		}
		TutorialDirector.instance.playerRevived = true;
		if (_revivedByTruck)
		{
			TruckHealer.instance.Heal(this);
		}
		Vector3 position = playerDeathHead.physGrabObject.centerPoint - Vector3.up * 0.25f;
		Vector3 eulerAngles = playerDeathHead.physGrabObject.transform.eulerAngles;
		if (SemiFunc.RunIsTutorial())
		{
			position = Vector3.zero + Vector3.up * 2f - Vector3.right * 5f;
			playerDeathHead.transform.position = position;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			tumble.physGrabObject.Teleport(position, base.transform.rotation);
		}
		base.transform.position = position;
		clientPositionCurrent = base.transform.position;
		clientPosition = base.transform.position;
		clientPhysRiding = false;
		base.gameObject.SetActive(value: true);
		playerAvatarVisuals.gameObject.SetActive(value: true);
		playerAvatarVisuals.transform.position = base.transform.position;
		playerAvatarVisuals.visualPosition = base.transform.position;
		playerAvatarVisuals.Revive();
		isDisabled = false;
		playerDeathHead.Reset();
		playerDeathEffects.Reset();
		playerReviveEffects.Trigger();
		deadSet = false;
		deadTimer = deadTime;
		if ((bool)voiceChat)
		{
			voiceChat.ToggleMixer(_lobby: false);
		}
		playerAvatarCollision.SetCrouch();
		playerHealth.SetMaterialGreen();
		if (isLocal)
		{
			playerHealth.HealOther(1, effect: true);
			playerTransform.position = base.transform.position;
			playerTransform.parent.gameObject.SetActive(value: true);
			if (!SpectateCamera.instance || !SpectateCamera.instance.CheckState(SpectateCamera.State.Head))
			{
				CameraAim.Instance.SetPlayerAim(Quaternion.Euler(0f, eulerAngles.y, 0f), _setRotation: true);
			}
			CameraPosition.instance.transform.position = position;
			CameraAim.Instance.OverrideNoSmooth(0.25f);
			GameDirector.instance.Revive();
			SpectateCamera.instance.StopSpectate();
			PlayerController.instance.Revive(eulerAngles);
			CameraGlitch.Instance.PlayLongHeal();
		}
		else if (!_revivedByTruck && SemiFunc.RunIsLevel())
		{
			PlayerAvatar playerAvatarScript = PlayerController.instance.playerAvatarScript;
			if (!playerAvatarScript.isDisabled && Vector3.Distance(playerAvatarScript.transform.position, base.transform.position) < 10f && playerAvatarScript.playerHealth.health >= 50 && !TutorialDirector.instance.playerHealed && TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialHealing, 1))
			{
				TutorialDirector.instance.ActivateTip("Healing", 0.5f, _interrupt: false);
			}
		}
		RoomVolumeCheck.CheckSet();
	}

	private void FallDamageResetLogic()
	{
		if (fallDamageResetTimer > 0f)
		{
			fallDamageResetTimer -= Time.deltaTime;
			fallDamageResetState = true;
		}
		else
		{
			fallDamageResetState = false;
		}
		if (fallDamageResetState != fallDamageResetStatePrevious)
		{
			fallDamageResetStatePrevious = fallDamageResetState;
			if (!GameManager.Multiplayer())
			{
				FallDamageResetUpdateRPC(fallDamageResetState);
				return;
			}
			photonView.RPC("FallDamageResetUpdateRPC", RpcTarget.All, fallDamageResetState);
		}
	}

	public void FallDamageResetSet(float _time)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			fallDamageResetTimer = _time;
		}
	}

	[PunRPC]
	private void FallDamageResetUpdateRPC(bool _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			fallDamageResetState = _state;
			if (isLocal && fallDamageResetState && (bool)tumble)
			{
				tumble.impactHurtTimer = 0f;
				tumble.impactHurtDamage = 0;
			}
		}
	}

	private void ChatMessageSpeak(string _message, bool crouching)
	{
		if ((bool)voiceChat && (bool)voiceChat.ttsVoice)
		{
			voiceChat.ttsVoice.TTSSpeakNow(_message, crouching);
		}
	}

	public void ChatMessageSend(string _message)
	{
		if (_message.StartsWith('/') && (bool)DebugCommandHandler.instance && DebugCommandHandler.instance.Execute(_message.TrimStart('/')))
		{
			return;
		}
		bool flag = isCrouching;
		if (!SemiFunc.IsMultiplayer())
		{
			ChatMessageSpeak(_message, flag);
			return;
		}
		if (isDisabled)
		{
			flag = true;
		}
		photonView.RPC("ChatMessageSendRPC", RpcTarget.All, _message, flag);
	}

	[PunRPC]
	public void ChatMessageSendRPC(string _message, bool crouching, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterAndOwnerOnlyRPC(_info, photonView) && GameDirector.instance.currentState == GameDirector.gameState.Main)
		{
			ChatMessageSpeak(_message, crouching);
		}
	}

	public void LoadingLevelAnimationCompleted()
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("LoadingLevelAnimationCompletedRPC", RpcTarget.All);
		}
		else
		{
			LoadingLevelAnimationCompletedRPC();
		}
	}

	[PunRPC]
	public void LoadingLevelAnimationCompletedRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.OwnerOnlyRPC(_info, photonView))
		{
			levelAnimationCompleted = true;
		}
	}

	public void HealedOther()
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("HealedOtherRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	public void HealedOtherRPC()
	{
		if (isLocal)
		{
			TutorialDirector.instance.playerHealed = true;
		}
	}

	public void PlayerExpressionSet(int _expressionIndex, float _percent)
	{
		if (!SemiFunc.IsMultiplayer())
		{
			PlayerExpressionSetRPC(_expressionIndex, _percent);
			return;
		}
		photonView.RPC("PlayerExpressionSetRPC", RpcTarget.All, _expressionIndex, _percent);
	}

	public void PlayerExpressionReset()
	{
		if (!SemiFunc.IsMultiplayer())
		{
			PlayerExpressionResetRPC();
		}
		else
		{
			photonView.RPC("PlayerExpressionResetRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	public void PlayerExpressionSetRPC(int _expressionIndex, float _percent, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterAndOwnerOnlyRPC(_info, photonView))
		{
			playerExpression.expressions[_expressionIndex].stopExpressing = false;
			playerExpressions[_expressionIndex] = _percent;
		}
	}

	[PunRPC]
	public void PlayerExpressionStopRPC(int _expressionIndex, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterAndOwnerOnlyRPC(_info, photonView))
		{
			playerExpressions[_expressionIndex] = 0f;
			playerExpression.expressions[_expressionIndex].stopExpressing = true;
		}
	}

	public void PlayerExpressionStop(int _expressionIndex)
	{
		if (!SemiFunc.IsMultiplayer())
		{
			PlayerExpressionStopRPC(_expressionIndex);
			return;
		}
		photonView.RPC("PlayerExpressionStopRPC", RpcTarget.All, _expressionIndex);
	}

	public void UpgradeTumbleWingsVisualsActive(bool _visualsActive = true, bool _pink = false)
	{
		if (!SemiFunc.IsMultiplayer())
		{
			UpgradeTumbleWingsVisualsActiveRPC(_visualsActive, _pink);
			return;
		}
		photonView.RPC("UpgradeTumbleWingsVisualsActiveRPC", RpcTarget.All, _visualsActive, _pink);
	}

	[PunRPC]
	private void UpgradeTumbleWingsVisualsActiveRPC(bool _visualsActive = true, bool _pink = false, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterAndOwnerOnlyRPC(_info, photonView))
		{
			upgradeTumbleWingsVisualsActive = _visualsActive;
			if (!_pink)
			{
				upgradeTumbleWingsLogic.tumbleWingTimer = 1f;
				upgradeTumbleWingsLogic.WingsSetOriginalColors();
			}
			if (_visualsActive && _pink)
			{
				upgradeTumbleWingsLogic.tumbleWingPinkTimer = 1f;
				upgradeTumbleWingsLogic.WingsSetPinkColors();
			}
		}
	}

	[PunRPC]
	public void PlayerExpressionResetRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterAndOwnerOnlyRPC(_info, photonView))
		{
			playerExpressions.Clear();
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!SemiFunc.MasterAndOwnerOnlyRPC(info, this.photonView))
		{
			return;
		}
		if (stream.IsWriting)
		{
			stream.SendNext(isCrouching);
			stream.SendNext(isSprinting);
			stream.SendNext(isCrawling);
			stream.SendNext(isSliding);
			stream.SendNext(isMoving);
			stream.SendNext(isGrounded);
			stream.SendNext(Interact);
			stream.SendNext(InputDirection);
			stream.SendNext(rbVelocity);
			stream.SendNext(rbVelocityRaw);
			stream.SendNext(PlayerController.instance.transform.position);
			stream.SendNext(PlayerController.instance.transform.rotation);
			stream.SendNext(PlayerController.instance.CollisionGrounded.physRiding);
			stream.SendNext(PlayerController.instance.CollisionGrounded.physRidingID);
			stream.SendNext(PlayerController.instance.CollisionGrounded.physRidingPosition);
			stream.SendNext(flashlightLightAim.clientAimPoint);
			stream.SendNext(playerPing);
			return;
		}
		isCrouching = (bool)stream.ReceiveNext();
		isSprinting = (bool)stream.ReceiveNext();
		isCrawling = (bool)stream.ReceiveNext();
		isSliding = (bool)stream.ReceiveNext();
		isMoving = (bool)stream.ReceiveNext();
		isGrounded = (bool)stream.ReceiveNext();
		Interact = (bool)stream.ReceiveNext();
		Vector3 vector = (Vector3)stream.ReceiveNext();
		if (!float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z))
		{
			vector = Vector3.ClampMagnitude(vector, 100f);
			InputDirection = vector;
		}
		Vector3 vector2 = (Vector3)stream.ReceiveNext();
		if (!float.IsNaN(vector2.x) && !float.IsNaN(vector2.y) && !float.IsNaN(vector2.z))
		{
			vector2 = Vector3.ClampMagnitude(vector2, 100f);
			rbVelocity = vector2;
		}
		Vector3 vector3 = (Vector3)stream.ReceiveNext();
		if (!float.IsNaN(vector3.x) && !float.IsNaN(vector3.y) && !float.IsNaN(vector3.z))
		{
			vector3 = Vector3.ClampMagnitude(vector3, 100f);
			rbVelocityRaw = vector3;
		}
		Vector3 vector4 = (Vector3)stream.ReceiveNext();
		if (!float.IsNaN(vector4.x) && !float.IsNaN(vector4.y) && !float.IsNaN(vector4.z))
		{
			vector4 = Vector3.ClampMagnitude(vector4, 1000f);
			clientPosition = vector4;
		}
		clientRotation = (Quaternion)stream.ReceiveNext();
		clientPositionDelta = Vector3.Distance(clientPositionCurrent, clientPosition);
		clientPhysRiding = (bool)stream.ReceiveNext();
		clientPhysRidingID = (int)stream.ReceiveNext();
		Vector3 vector5 = (Vector3)stream.ReceiveNext();
		if (!float.IsNaN(vector5.x) && !float.IsNaN(vector5.y) && !float.IsNaN(vector5.z))
		{
			vector5 = Vector3.ClampMagnitude(vector5, 1000f);
			clientPhysRidingPosition = vector5;
		}
		if (clientPhysRiding)
		{
			PhotonView photonView = PhotonView.Find(clientPhysRidingID);
			if ((bool)photonView)
			{
				clientPhysRidingTransform = photonView.transform;
			}
			else
			{
				clientPhysRiding = false;
			}
		}
		playerAvatarVisuals.PhysRidingCheck();
		Vector3 vector6 = (Vector3)stream.ReceiveNext();
		if (!float.IsNaN(vector6.x) && !float.IsNaN(vector6.y) && !float.IsNaN(vector6.z))
		{
			vector6 = Vector3.ClampMagnitude(vector6, 1000f);
			flashlightLightAim.clientAimPoint = vector6;
		}
		playerPing = (int)stream.ReceiveNext();
	}
}
