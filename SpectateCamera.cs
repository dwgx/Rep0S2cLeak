using System.Linq;
using UnityEngine;

public class SpectateCamera : MonoBehaviour
{
	public enum State
	{
		Death,
		Normal,
		Head
	}

	public static SpectateCamera instance;

	private State currentState;

	private float stateTimer;

	private bool stateImpulse = true;

	internal PlayerAvatar player;

	private PlayerAvatar playerOverride;

	private float previousFarClipPlane = 0.01f;

	private float previousFieldOfView;

	private Camera MainCamera;

	private Camera TopCamera;

	private Transform ParentObject;

	private Transform PreviousParent;

	private float cameraFieldOfView = 10f;

	private int currentPlayerListIndex;

	private Transform deathPlayerSpectatePoint;

	private Vector3 deathCameraOffset;

	private float deathCameraNearClipPlane;

	private float deathCurrentY;

	private Vector3 deathVelocity;

	private float deathSpeed = 6f;

	private Vector3 deathFollowPoint;

	private Vector3 deathFollowPointTarget;

	private Vector3 deathFollowPointVelocity;

	private Vector3 deathSmoothLookAtPoint;

	private Quaternion deathSmoothOrbit;

	private Quaternion deathTargetOrbit;

	private bool deathOrbitInstantSet;

	private Vector3 deathPosition;

	public Transform normalTransformPivot;

	public Transform normalTransformDistance;

	private Vector3 normalPreviousPosition;

	private float normalAimHorizontal;

	private float normalAimVertical;

	private float normalMinDistance = 1f;

	private float normalMaxDistance = 3f;

	private float normalDistanceTarget;

	private float normalDistanceCheckTimer;

	internal bool headEnergyEnough;

	internal float headEnergy;

	internal float headEnergyPauseTimer;

	private bool headOverride;

	private void Awake()
	{
		instance = this;
		normalTransformPivot.parent = null;
		MainCamera = GameDirector.instance.MainCamera;
		Camera[] componentsInChildren = MainCamera.GetComponentsInChildren<Camera>();
		foreach (Camera camera in componentsInChildren)
		{
			if (camera != MainCamera)
			{
				TopCamera = camera;
			}
		}
		ParentObject = CameraAim.Instance.transform;
		PreviousParent = ParentObject.parent;
		ParentObject.parent = base.transform;
		ParentObject.localPosition = Vector3.zero;
		ParentObject.localRotation = Quaternion.identity;
	}

	private void OnDestroy()
	{
		QualitySettings.shadowDistance = 15f;
	}

	private void LateUpdate()
	{
		SemiFunc.UIHideHealth();
		SemiFunc.UIHideOvercharge();
		SemiFunc.UIHideEnergy();
		SemiFunc.UIHideInventory();
		SemiFunc.UIHideAim();
		MissionUI.instance.Hide();
		switch (currentState)
		{
		case State.Death:
			StateDeath();
			break;
		case State.Normal:
			StateNormal();
			break;
		case State.Head:
			StateHead();
			break;
		}
		RoomVolumeLogic();
	}

	private void Update()
	{
		HeadEnergyLogic();
	}

	private void StateDeath()
	{
		if (stateImpulse)
		{
			MainCamera.transform.localPosition = new Vector3(0f, 0f, -50f);
			MainCamera.transform.localRotation = Quaternion.identity;
			MainCamera.nearClipPlane = 0.01f;
			previousFarClipPlane = MainCamera.farClipPlane;
			MainCamera.farClipPlane = 70f;
			MainCamera.farClipPlane = 90f;
			deathCameraNearClipPlane = 70f;
			MainCamera.nearClipPlane = 70f;
			QualitySettings.shadowDistance = 90f;
			RenderSettings.fog = false;
			PostProcessing.Instance.SpectateSet();
			DeathNearClipLogic(_instant: true);
			LightManager.instance.UpdateInstant();
			CameraGlitch.Instance.PlayShort();
			previousFieldOfView = MainCamera.fieldOfView;
			cameraFieldOfView = 8f;
			MainCamera.fieldOfView = 16f;
			TopCamera.fieldOfView = MainCamera.fieldOfView;
			stateImpulse = false;
			if (SemiFunc.RunIsArena())
			{
				stateTimer = 1.5f;
			}
			else
			{
				stateTimer = 4f;
			}
			AudioManager.instance.AudioListener.TargetPositionTransform = deathPlayerSpectatePoint;
			GameDirector.instance.CameraImpact.Shake(2f, 0.5f);
			GameDirector.instance.CameraShake.Shake(2f, 1f);
			PlayerController.instance.playerAvatarScript.localCamera.Teleported();
			PlayerController.instance.playerAvatarScript.playerDeathHead.SpectatedSet(_active: false);
		}
		stateTimer -= Time.deltaTime;
		CameraAim.Instance.OverridePlayerAimDisable(_set: true);
		CameraNoise.Instance.Override(0.03f, 0.25f);
		SemiFunc.UIShowSpectate();
		deathCurrentY += SemiFunc.InputMouseX() * CameraAim.Instance.AimSpeedMouse;
		Vector3 position = base.transform.position;
		if ((bool)deathPlayerSpectatePoint)
		{
			position = deathPlayerSpectatePoint.position;
		}
		if (CheckState(State.Death))
		{
			position = deathPosition;
		}
		Vector3 vector = position;
		Quaternion quaternion = Quaternion.Euler(88f, deathCurrentY, 0f);
		deathTargetOrbit = quaternion;
		float num = Mathf.Lerp(50f, 2.5f, GameplayManager.instance.cameraSmoothing / 100f);
		deathSmoothOrbit = Quaternion.Slerp(deathSmoothOrbit, deathTargetOrbit, num * Time.deltaTime);
		if (deathOrbitInstantSet)
		{
			deathSmoothOrbit = deathTargetOrbit;
			deathOrbitInstantSet = false;
		}
		quaternion = deathSmoothOrbit;
		Vector3 vector2 = quaternion * Vector3.back * 2f;
		deathFollowPointTarget = vector;
		deathFollowPoint = Vector3.SlerpUnclamped(deathFollowPoint, deathFollowPointTarget, Time.deltaTime * deathSpeed);
		base.transform.position = deathFollowPoint + vector2;
		deathSmoothLookAtPoint = Vector3.SlerpUnclamped(deathSmoothLookAtPoint, position, Time.deltaTime * deathSpeed);
		base.transform.LookAt(deathSmoothLookAtPoint);
		MainCamera.fieldOfView = Mathf.Lerp(MainCamera.fieldOfView, cameraFieldOfView, Time.deltaTime * 10f);
		TopCamera.fieldOfView = MainCamera.fieldOfView;
		DeathNearClipLogic(_instant: false);
		ExtractionPoint extractionPointCurrent = RoundDirector.instance.extractionPointCurrent;
		if ((bool)extractionPointCurrent && extractionPointCurrent.currentState != ExtractionPoint.State.Idle && extractionPointCurrent.currentState != ExtractionPoint.State.Active)
		{
			bool flag = false;
			foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
			{
				if (!item.isDisabled)
				{
					flag = false;
					break;
				}
				if (item.playerDeathHead.inExtractionPoint)
				{
					flag = true;
				}
			}
			if (flag)
			{
				stateTimer = Mathf.Clamp(stateTimer, 0.25f, stateTimer);
			}
		}
		if (!(stateTimer <= 0f))
		{
			return;
		}
		if (SemiFunc.RunIsTutorial())
		{
			foreach (PlayerAvatar item2 in SemiFunc.PlayerGetList())
			{
				item2.Revive();
			}
			return;
		}
		bool flag2 = true;
		if (SemiFunc.RunIsArena())
		{
			flag2 = false;
			foreach (PlayerAvatar item3 in SemiFunc.PlayerGetList())
			{
				if (!item3.isDisabled)
				{
					flag2 = true;
					break;
				}
			}
		}
		if (flag2)
		{
			UpdateState(State.Normal);
		}
		else
		{
			stateTimer = 1f;
		}
	}

	private void StateNormal()
	{
		PlayerDeathHead playerDeathHead = PlayerController.instance.playerAvatarScript.playerDeathHead;
		if (stateImpulse)
		{
			PlayerSwitch();
			if (!player)
			{
				return;
			}
			RenderSettings.fog = true;
			MainCamera.farClipPlane = previousFarClipPlane;
			MainCamera.fieldOfView = previousFieldOfView;
			TopCamera.fieldOfView = MainCamera.fieldOfView;
			MainCamera.transform.localPosition = Vector3.zero;
			MainCamera.transform.localRotation = Quaternion.identity;
			AudioManager.instance.AudioListener.TargetPositionTransform = MainCamera.transform;
			PlayerController.instance.playerAvatarScript.localCamera.Teleported();
			PlayerController.instance.playerAvatarScript.playerDeathHead.SpectatedSet(_active: false);
			stateTimer = 0.5f;
			stateImpulse = false;
			if (TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialHeadSpectate, 1))
			{
				TutorialDirector.instance.ActivateTip("HeadSpectate1", 1f, _interrupt: false, 7.5f, _scaleDown: false);
				TutorialDirector.instance.ScheduleTip("HeadSpectate2", 8.5f, _interrupt: false, 7.5f, _scaleDown: false);
			}
		}
		CameraAim.Instance.OverridePlayerAimDisable(_set: true);
		CameraNoise.Instance.Override(0.03f, 0.25f);
		SemiFunc.UIShowSpectate();
		float num = SemiFunc.InputMouseX();
		float num2 = SemiFunc.InputMouseY();
		float num3 = SemiFunc.InputScrollY();
		if (CameraAim.Instance.overrideAimStop)
		{
			num = 0f;
			num2 = 0f;
			num3 = 0f;
		}
		normalAimHorizontal += num * CameraAim.Instance.AimSpeedMouse * 1.5f;
		if (normalAimHorizontal > 360f)
		{
			normalAimHorizontal -= 360f;
		}
		if (normalAimHorizontal < -360f)
		{
			normalAimHorizontal += 360f;
		}
		if (GameplayManager.instance.aimInvertVertical)
		{
			num2 *= -1f;
		}
		float num4 = normalAimVertical;
		float num5 = (0f - num2 * CameraAim.Instance.AimSpeedMouse) * 1.5f;
		normalAimVertical += num5;
		normalAimVertical = Mathf.Clamp(normalAimVertical, -70f, 70f);
		if (num3 != 0f)
		{
			normalMaxDistance = Mathf.Clamp(normalMaxDistance - num3 * 0.0025f, normalMinDistance, 6f);
		}
		Vector3 vector = normalPreviousPosition;
		if ((bool)player.spectatePoint)
		{
			vector = player.spectatePoint.position;
		}
		else if (player.isTumbling)
		{
			vector = player.tumble.physGrabObject.centerPoint;
		}
		else if (player.isCrouching && !player.isCrawling)
		{
			vector += Vector3.up * 0.3f;
		}
		else if (!player.isCrawling)
		{
			vector += Vector3.down * 0.15f;
		}
		normalPreviousPosition = vector;
		normalTransformPivot.position = Vector3.Lerp(normalTransformPivot.position, vector, 10f * Time.deltaTime);
		Quaternion quaternion = Quaternion.Euler(normalAimVertical, normalAimHorizontal, 0f);
		float num6 = Mathf.Lerp(50f, 6.25f, GameplayManager.instance.cameraSmoothing / 100f);
		normalTransformPivot.rotation = Quaternion.Lerp(normalTransformPivot.rotation, quaternion, num6 * Time.deltaTime);
		normalTransformPivot.localRotation = Quaternion.Euler(normalTransformPivot.localRotation.eulerAngles.x, normalTransformPivot.localRotation.eulerAngles.y, 0f);
		bool flag = false;
		float num7 = normalMaxDistance;
		RaycastHit[] array = Physics.SphereCastAll(normalTransformPivot.position, 0.1f, -normalTransformPivot.forward, normalMaxDistance, SemiFunc.LayerMaskGetVisionObstruct());
		if (array.Length != 0)
		{
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				if (!raycastHit.transform.GetComponent<PlayerHealthGrab>() && !raycastHit.transform.GetComponent<PlayerAvatar>() && !raycastHit.transform.GetComponent<PlayerTumble>() && !raycastHit.transform.GetComponent<EnemyRigidbody>())
				{
					num7 = Mathf.Min(num7, raycastHit.distance);
					if (raycastHit.transform.CompareTag("Wall"))
					{
						flag = true;
					}
					if (raycastHit.collider.bounds.size.magnitude > 2f)
					{
						flag = true;
					}
				}
			}
			normalDistanceTarget = Mathf.Max(normalMinDistance, num7);
		}
		else
		{
			normalDistanceTarget = normalMaxDistance;
		}
		Vector3 vector2 = new Vector3(0f, 0f, 0f - normalDistanceTarget);
		normalTransformDistance.localPosition = Vector3.Lerp(normalTransformDistance.localPosition, vector2, Time.deltaTime * 5f);
		float num8 = 0f - normalTransformDistance.localPosition.z;
		Vector3 direction = normalTransformPivot.position - normalTransformDistance.position;
		float num9 = direction.magnitude;
		if (Physics.SphereCast(normalTransformDistance.position, 0.15f, direction, out var hitInfo, normalMaxDistance, LayerMask.GetMask("PlayerVisuals"), QueryTriggerInteraction.Collide))
		{
			num9 = hitInfo.distance;
		}
		num9 = num8 - num9 - 0.1f;
		if (flag)
		{
			float num10 = Mathf.Max(num7, num9);
			MainCamera.nearClipPlane = Mathf.Max(num8 - num10, 0.01f);
		}
		else
		{
			MainCamera.nearClipPlane = 0.01f;
		}
		RenderSettings.fogStartDistance = MainCamera.nearClipPlane;
		base.transform.position = normalTransformDistance.position;
		base.transform.rotation = normalTransformDistance.rotation;
		if ((bool)player && base.transform.position.y < player.transform.position.y + 0.25f && num5 < 0f)
		{
			normalAimVertical = num4;
		}
		if (SemiFunc.InputDown(InputKey.Jump))
		{
			PlayerSwitch();
		}
		if (SemiFunc.InputDown(InputKey.SpectateNext))
		{
			PlayerSwitch();
		}
		if (SemiFunc.InputDown(InputKey.SpectatePrevious))
		{
			PlayerSwitch(_next: false);
		}
		if ((bool)player && player.voiceChatFetched)
		{
			player.voiceChat.SpatialDisable(0.1f);
		}
		if (stateTimer <= 0f && ((headEnergyEnough && SemiFunc.InputDown(InputKey.Interact)) || playerDeathHead.overrideSpectated))
		{
			headOverride = playerDeathHead.overrideSpectated;
			if (headOverride)
			{
				GameDirector.instance.CameraImpact.Shake(5f, 0.1f);
				GameDirector.instance.CameraShake.Shake(2f, 0.2f);
				AudioScare.instance.PlayImpact();
				headEnergy = 1f;
			}
			UpdateState(State.Head);
		}
		else
		{
			stateTimer -= Time.deltaTime;
		}
	}

	private void StateHead()
	{
		PlayerDeathHead playerDeathHead = PlayerController.instance.playerAvatarScript.playerDeathHead;
		if (playerDeathHead.overrideSpectated)
		{
			headOverride = true;
			headEnergy = 1f;
		}
		if (stateImpulse)
		{
			base.transform.position = playerDeathHead.physGrabObject.centerPoint;
			MainCamera.nearClipPlane = 0.01f;
			CameraTeleportImpulse();
			SemiFunc.LightManagerSetCullTargetTransform(base.transform);
			CameraAim.Instance.OverridePlayerAimDisableReset();
			CameraAim.Instance.SetPlayerAim(playerDeathHead.transform.rotation, _setRotation: true);
			PlayerController.instance.playerAvatarScript.localCamera.Teleported();
			stateImpulse = false;
			stateTimer = 0.5f;
			headEnergyPauseTimer = 1f;
		}
		if (!headOverride)
		{
			PostProcessing.Instance.VignetteOverride(Color.black, 0.4f, 1f, 5f, 5f, 0.1f, base.gameObject);
			PostProcessing.Instance.SaturationOverride(-50f, 20f, 20f, 0.1f, base.gameObject);
		}
		else
		{
			PostProcessing.Instance.VignetteOverride(Color.red, 0.3f, 0.5f, 5f, 5f, 0.1f, base.gameObject);
			PostProcessing.Instance.SaturationOverride(-20f, 20f, 20f, 0.1f, base.gameObject);
			PostProcessing.Instance.ContrastOverride(50f, 5f, 5f, 0.1f, base.gameObject);
			MusicEnemyNear.instance.OverrideActive(0.5f);
			if (SemiFunc.FPSImpulse5() && !SemiFunc.Photosensitivity() && Random.Range(0, 10) == 0)
			{
				CameraGlitch.Instance.PlayTiny();
				GameDirector.instance.CameraImpact.Shake(1f, 0.1f);
			}
		}
		CameraNoise.Instance.Override(0.03f, 0.25f);
		base.transform.localRotation = Quaternion.identity;
		PlayerController.instance.playerAvatarScript.playerDeathHead.SpectatedSet(_active: true);
		float num = 25f;
		if (headOverride)
		{
			num = 5f;
		}
		base.transform.position = Vector3.Lerp(base.transform.position, playerDeathHead.physGrabObject.centerPoint, num * Time.deltaTime);
		if (!playerDeathHead || (stateTimer <= 0f && ((!headOverride && (SemiFunc.InputDown(InputKey.Interact) || headEnergy <= 0f)) || (headOverride && !playerDeathHead.overrideSpectated))))
		{
			if (headOverride)
			{
				headEnergy = 0f;
			}
			headEnergyPauseTimer = 1f;
			player = null;
			if ((bool)playerDeathHead)
			{
				PlayerAvatar playerAvatar = SemiFunc.PlayerGetNearestPlayerAvatarWithinRange(999f, playerDeathHead.transform.position);
				if ((bool)playerAvatar)
				{
					playerOverride = playerAvatar;
				}
			}
			UpdateState(State.Normal);
		}
		stateTimer -= Time.deltaTime;
	}

	private void UpdateState(State _state)
	{
		if (currentState != _state)
		{
			currentState = _state;
			stateImpulse = true;
			stateTimer = 0f;
		}
	}

	public bool CheckState(State _state)
	{
		return currentState == _state;
	}

	private void PlayerSwitch(bool _next = true)
	{
		if (GameDirector.instance.PlayerList.All((PlayerAvatar p) => p.isDisabled))
		{
			return;
		}
		int num = 0;
		int num2 = currentPlayerListIndex;
		for (int count = GameDirector.instance.PlayerList.Count; num < count; num++)
		{
			num2 = ((!_next) ? ((num2 - 1 + count) % count) : ((num2 + 1) % count));
			PlayerAvatar playerAvatar = GameDirector.instance.PlayerList[num2];
			if ((bool)playerOverride && !(playerAvatar == playerOverride))
			{
				continue;
			}
			playerOverride = null;
			if (player != playerAvatar && !playerAvatar.isDisabled)
			{
				currentPlayerListIndex = num2;
				player = playerAvatar;
				normalTransformPivot.position = player.spectatePoint.position;
				normalAimHorizontal = player.transform.eulerAngles.y;
				normalAimVertical = 0f;
				normalTransformPivot.rotation = Quaternion.Euler(normalAimVertical, normalAimHorizontal, 0f);
				normalTransformPivot.localRotation = Quaternion.Euler(normalTransformPivot.localRotation.eulerAngles.x, normalTransformPivot.localRotation.eulerAngles.y, 0f);
				normalTransformDistance.localPosition = new Vector3(0f, 0f, -2f);
				base.transform.position = normalTransformDistance.position;
				base.transform.rotation = normalTransformDistance.rotation;
				if (SemiFunc.IsMultiplayer())
				{
					SemiFunc.HUDSpectateSetName(player.playerName);
				}
				SemiFunc.LightManagerSetCullTargetTransform(player.transform);
				CameraTeleportImpulse();
				normalMaxDistance = 3f;
				PlayerController.instance.playerAvatarScript.localCamera.Teleported();
				break;
			}
		}
	}

	private void CameraTeleportImpulse()
	{
		CameraGlitch.Instance.PlayTiny();
		GameDirector.instance.CameraImpact.Shake(1f, 0.1f);
		AudioManager.instance.RestartAudioLoopDistances();
		LevelGenerator.Instance.RestartParticleDistances();
	}

	public void UpdatePlayer(PlayerAvatar deadPlayer)
	{
		if (deadPlayer == player)
		{
			SetDeath(deadPlayer.spectatePoint);
		}
	}

	public void StopSpectate()
	{
		ParentObject.parent = PreviousParent;
		ParentObject.localPosition = Vector3.zero;
		ParentObject.localRotation = Quaternion.identity;
		MainCamera.nearClipPlane = 0.001f;
		MainCamera.farClipPlane = previousFarClipPlane;
		MainCamera.transform.localPosition = Vector3.zero;
		MainCamera.transform.localRotation = Quaternion.identity;
		MainCamera.fieldOfView = previousFieldOfView;
		RenderSettings.fog = true;
		RenderSettings.fogStartDistance = 0f;
		PostProcessing.Instance.SpectateReset();
		PlayerAvatar.instance.spectating = false;
		SemiFunc.LightManagerSetCullTargetTransform(PlayerAvatar.instance.transform);
		AudioManager.instance.AudioListener.TargetPositionTransform = MainCamera.transform;
		Object.Destroy(normalTransformPivot.gameObject);
		Object.Destroy(base.gameObject);
	}

	private void DeathNearClipLogic(bool _instant)
	{
		if (!CheckState(State.Death))
		{
			return;
		}
		Vector3 direction = MainCamera.transform.position - deathSmoothLookAtPoint;
		RaycastHit[] array = Physics.RaycastAll(deathSmoothLookAtPoint, direction, direction.magnitude, LayerMask.GetMask("Default"));
		float num = float.PositiveInfinity;
		Vector3 vector = Vector3.zero;
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			if (raycastHit.transform.CompareTag("Ceiling") && raycastHit.distance < num)
			{
				num = raycastHit.distance;
				vector = raycastHit.point;
			}
		}
		if (vector != Vector3.zero)
		{
			deathCameraNearClipPlane = (MainCamera.transform.position - vector).magnitude + 0.5f;
		}
		else
		{
			deathCameraNearClipPlane = 1f;
		}
		if (_instant)
		{
			MainCamera.nearClipPlane = deathCameraNearClipPlane;
		}
		else
		{
			MainCamera.nearClipPlane = Mathf.Lerp(MainCamera.nearClipPlane, deathCameraNearClipPlane, Time.deltaTime * 10f);
		}
	}

	public void SetDeath(Transform _spectatePoint)
	{
		deathPosition = _spectatePoint.position;
		deathPlayerSpectatePoint = _spectatePoint;
		base.transform.position = _spectatePoint.position;
		base.transform.rotation = _spectatePoint.rotation;
		deathFollowPoint = deathPosition;
		deathFollowPointTarget = deathPosition;
		deathSmoothLookAtPoint = deathPosition;
		deathOrbitInstantSet = true;
		SemiFunc.LightManagerSetCullTargetTransform(deathPlayerSpectatePoint);
		deathSmoothLookAtPoint = deathPlayerSpectatePoint.position;
		base.transform.position = deathFollowPointTarget;
		deathFollowPoint = deathFollowPointTarget;
		deathSmoothLookAtPoint = deathPlayerSpectatePoint.position;
		DeathNearClipLogic(_instant: true);
		UpdateState(State.Death);
	}

	private void RoomVolumeLogic()
	{
		RoomVolumeCheck roomVolumeCheck = PlayerController.instance.playerAvatarScript.RoomVolumeCheck;
		roomVolumeCheck.PauseCheckTimer = 1f;
		if ((bool)player)
		{
			RoomVolumeCheck roomVolumeCheck2 = player.RoomVolumeCheck;
			roomVolumeCheck.CurrentRooms.Clear();
			roomVolumeCheck.CurrentRooms.AddRange(roomVolumeCheck2.CurrentRooms);
		}
	}

	private void HeadEnergyLogic()
	{
		if ((bool)PlayerController.instance && (bool)PlayerController.instance.playerAvatarScript && !PlayerController.instance.playerAvatarScript.playerDeathHead)
		{
			return;
		}
		if (headEnergyPauseTimer <= 0f)
		{
			if (CheckState(State.Head))
			{
				if (!headOverride)
				{
					float num = 25f;
					float num2 = 5f;
					for (float num3 = PlayerController.instance.playerAvatarScript.upgradeDeathHeadBattery; num3 > 0f; num3 -= 1f)
					{
						num += num2;
						num2 *= 0.95f;
					}
					headEnergy -= Time.deltaTime / num;
					if (PlayerController.instance.playerAvatarScript.playerDeathHead.spectatedJumpCharging)
					{
						headEnergy -= Time.deltaTime / num;
					}
				}
				headEnergyEnough = false;
			}
			else if (CheckState(State.Normal))
			{
				headEnergy += Time.deltaTime / 100f;
				if (headEnergy >= 0.25f)
				{
					headEnergyEnough = true;
				}
				else
				{
					headEnergyEnough = false;
				}
			}
		}
		else
		{
			headEnergyPauseTimer -= Time.deltaTime;
		}
		headEnergy = Mathf.Clamp01(headEnergy);
	}
}
