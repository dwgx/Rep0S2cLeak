using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GumballValuable : Trap
{
	[Header("Transforms")]
	public Transform closedLidsMesh;

	public Transform openLidsMesh;

	public Transform eyeBallMesh;

	public Transform bodyTransform;

	public Transform eyeLockTransform;

	public Transform globe;

	public Transform HypnosisLinesParenTransform;

	public Transform eyeLineParent;

	[Header("Line of sight transforms")]
	public Transform top;

	public Transform bottom;

	public Transform front;

	public Transform back;

	[Header("Lights")]
	public Light pointLight;

	public AnimationCurve lightIntensityCurve;

	[Header("Hypnosis Line GameObject")]
	public GameObject hypnosisLine;

	[Header("Particles")]
	public List<ParticleSystem> particles = new List<ParticleSystem>();

	[Header("Camera Shake")]
	public float cameraShakeTime = 0.2f;

	public float cameraShakeStrength = 3f;

	public Vector2 cameraShakeBounds = new Vector2(1.5f, 5f);

	[Header("Vignette")]
	public float vignetteIntensity = 0.5f;

	[Header("Camera Zoom")]
	public float cameraZoomSpeedIn = 0.5f;

	public float cameraZoomSpeedOut = 0.5f;

	public float cameraZoomAmount = 20f;

	[Header("EyeLock")]
	public float noAimStrength = 1.5f;

	[Header("Spiral on screen")]
	public GameObject screenSpiralEffect;

	[Header("Sounds")]
	public Sound eyesOpenSound;

	public Sound eyesCloseSound;

	public Sound hypnosisSoundLoop;

	private float lightIntensityOriginal;

	private float lightRangeOriginal;

	private float animationCurveEval;

	private int playerCount;

	private List<PlayerAvatar> allPlayers;

	private List<bool> playerInSight = new List<bool>();

	private List<GameObject> hypnosisLines = new List<GameObject>();

	private SpiralOnScreen spiralScreenEffect;

	private EyeLines eyeLines;

	private bool isActive;

	private int oldPlayerCount;

	private List<bool> previouslySeenList = new List<bool>();

	protected void Awake()
	{
		base.Start();
		CloseEyes();
		GetValues();
		InstantiateHypnosisLines();
	}

	private void InstantiateHypnosisLines()
	{
		for (int i = 0; i < playerCount; i++)
		{
			GameObject gameObject = Object.Instantiate(hypnosisLine, base.transform.position, Quaternion.identity);
			gameObject.transform.SetParent(allPlayers[i].localCamera.transform);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.GetComponent<EyeLines>().InitializeLine(allPlayers[i]);
			gameObject.transform.SetParent(HypnosisLinesParenTransform);
			hypnosisLines.Add(gameObject);
			gameObject.SetActive(value: false);
		}
	}

	private void GetValues()
	{
		allPlayers = SemiFunc.PlayerGetList();
		playerCount = allPlayers.Count;
		for (int i = 0; i < playerCount; i++)
		{
			playerInSight.Add(item: false);
			previouslySeenList.Add(item: false);
		}
		lightIntensityOriginal = pointLight.intensity;
		lightRangeOriginal = pointLight.range;
	}

	protected override void Update()
	{
		base.Update();
		if (physGrabObject.grabbed && !isActive)
		{
			OpenEyes();
		}
		else if (!physGrabObject.grabbed && isActive)
		{
			CloseEyes();
		}
		CheckForLeftPlayers();
		ClientEffectsLoop();
		if (SemiFunc.IsMasterClientOrSingleplayer() && isActive)
		{
			CheckForListChange();
		}
	}

	public void ClientEffectsLoop()
	{
		hypnosisSoundLoop.PlayLoop(isActive, 1f, 1f);
		for (int i = 0; i < playerCount; i++)
		{
			if (playerCount == 0)
			{
				break;
			}
			EyeLines component = hypnosisLines[i].GetComponent<EyeLines>();
			if (playerInSight[i])
			{
				hypnosisLines[i].SetActive(value: true);
				component.SetIsActive(_isActive: true);
				if (allPlayers[i].isLocal)
				{
					VignetteOverride();
					ScreenSpiralOn();
					CameraAimAndZoom(allPlayers[i]);
					PlayerAvatarOverride(allPlayers[i]);
					PostProcessing.Instance.SaturationOverride(-50f, 0.8f, 5f, 0.1f, base.gameObject);
				}
			}
			else
			{
				component.SetIsActive(_isActive: false);
			}
			if (!allPlayers[i].isLocal)
			{
				DrawHypnosisLines(allPlayers[i], hypnosisLines[i]);
			}
		}
		if (pointLight.enabled && !isActive)
		{
			LerpLightOff(pointLight);
		}
		else if (isActive)
		{
			LerpLightOn(pointLight);
		}
	}

	private void CheckForListChange()
	{
		if (SemiFunc.FPSImpulse5())
		{
			GetAffectedPlayers();
		}
	}

	private void CheckForLeftPlayers()
	{
		if (playerCount != SemiFunc.PlayerGetList().Count)
		{
			RedoPlayersInSightList();
			UpdateHypnosisLinesList();
			playerCount = SemiFunc.PlayerGetList().Count;
			allPlayers = SemiFunc.PlayerGetList();
		}
	}

	private void TurnOffHypnosisLines()
	{
		for (int i = 0; i < playerCount; i++)
		{
			hypnosisLines[i].SetActive(value: false);
		}
	}

	private void OpenEyes()
	{
		isActive = true;
		GetAffectedPlayers();
		UpdateMeshes();
		ToggleParticles(_state: true);
		ScreenSpiralOn();
		CameraShake();
		eyesOpenSound.Play(globe.position);
		if (!pointLight.enabled)
		{
			SetLightEnableState(_state: true);
		}
	}

	private void CloseEyes()
	{
		isActive = false;
		UpdateMeshes();
		ToggleParticles(_state: false);
		CameraShake();
		eyesCloseSound.Play(globe.position);
		EmptyPlayersInSightList();
	}

	private void CameraAimAndZoom(PlayerAvatar _player)
	{
		if (!physGrabObject.playerGrabbing.Contains(_player.physGrabber))
		{
			CameraAim.Instance.AimTargetSoftSet(eyeLockTransform.position, 0.1f, 2f, noAimStrength, base.gameObject, 100);
		}
		CameraZoom.Instance.OverrideZoomSet(cameraZoomAmount, 0.1f, 2f, 2f, base.gameObject, 100);
	}

	private void PlayerAvatarOverride(PlayerAvatar _player)
	{
		_player.playerHealth.EyeMaterialOverride(PlayerHealth.EyeOverrideState.Inverted, 0.25f, 0);
		_player.OverridePupilSize(3f, 4, 3f, 1f, 15f, 0.3f);
		SemiFunc.PlayerEyesOverride(_player, eyeBallMesh.position, 0.1f, base.gameObject);
	}

	private void DrawHypnosisLines(PlayerAvatar _player, GameObject _hypnosisLine)
	{
		EyeLines component = _hypnosisLine.GetComponent<EyeLines>();
		if (_hypnosisLine.activeSelf)
		{
			component.DrawLines();
		}
	}

	private void UpdateMeshes()
	{
		closedLidsMesh.gameObject.SetActive(!isActive);
		openLidsMesh.gameObject.SetActive(isActive);
		eyeBallMesh.gameObject.SetActive(isActive);
	}

	private void CameraShake()
	{
		GameDirector.instance.CameraShake.ShakeDistance(cameraShakeStrength, 3f, 8f, base.transform.position, cameraShakeTime);
		GameDirector.instance.CameraImpact.ShakeDistance(cameraShakeStrength, cameraShakeBounds.x, cameraShakeBounds.y, base.transform.position, cameraShakeTime);
	}

	private void ToggleParticles(bool _state)
	{
		if (_state)
		{
			foreach (ParticleSystem particle in particles)
			{
				particle.Play();
			}
			return;
		}
		foreach (ParticleSystem particle2 in particles)
		{
			particle2.Stop();
		}
	}

	private void VignetteOverride()
	{
		PostProcessing.Instance.VignetteOverride(new Color(0f, 0f, 0f), vignetteIntensity, 0.5f, 0.1f, 2f, 0.1f, base.gameObject);
	}

	private void ScreenSpiralOn()
	{
		if (!spiralScreenEffect && (bool)PlayerAvatar.instance)
		{
			spiralScreenEffect = PlayerAvatar.instance.localCamera.transform.GetComponentInChildren<SpiralOnScreen>();
		}
		if (!spiralScreenEffect)
		{
			for (int i = 0; i < playerCount; i++)
			{
				if (allPlayers[i].isLocal && playerInSight[i])
				{
					Transform parent = allPlayers[i].localCamera.transform;
					GameObject gameObject = Object.Instantiate(screenSpiralEffect, base.transform.position, Quaternion.identity, parent);
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localRotation = Quaternion.identity;
					spiralScreenEffect = gameObject.GetComponent<SpiralOnScreen>();
					break;
				}
			}
		}
		if ((bool)spiralScreenEffect && !SemiFunc.Photosensitivity())
		{
			spiralScreenEffect.Active();
		}
	}

	private void SetLightEnableState(bool _state)
	{
		pointLight.intensity = 0f;
		pointLight.range = 0f;
		animationCurveEval = 0f;
		pointLight.enabled = _state;
	}

	private void LerpLightOn(Light _light)
	{
		if (_light.intensity < lightIntensityOriginal - 0.01f)
		{
			animationCurveEval += Time.deltaTime * 0.2f;
			float t = lightIntensityCurve.Evaluate(animationCurveEval);
			_light.intensity = Mathf.Lerp(_light.intensity, lightIntensityOriginal, t);
			_light.range = Mathf.Lerp(_light.range, lightRangeOriginal, t);
		}
	}

	private void LerpLightOff(Light _light)
	{
		animationCurveEval += Time.deltaTime * 1f;
		float t = lightIntensityCurve.Evaluate(animationCurveEval);
		_light.intensity = Mathf.Lerp(_light.intensity, 0f, t);
		_light.range = Mathf.Lerp(_light.range, 0f, t);
		if (pointLight.intensity < 0.01f)
		{
			SetLightEnableState(_state: false);
		}
	}

	private void EmptyPlayersInSightList()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			for (int i = 0; i < playerCount; i++)
			{
				PlayerStateChanged(_state: false, allPlayers[i].photonView.ViewID);
			}
		}
	}

	private void GetAffectedPlayers()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		for (int i = 0; i < playerCount; i++)
		{
			if (!allPlayers[i].isDisabled)
			{
				if (InLineOfSight(allPlayers[i], i) && !playerInSight[i])
				{
					PlayerStateChanged(_state: true, allPlayers[i].photonView.ViewID);
				}
				else if (!InLineOfSight(allPlayers[i], i) && playerInSight[i])
				{
					PlayerStateChanged(_state: false, allPlayers[i].photonView.ViewID);
				}
			}
		}
	}

	public void PlayerStateChanged(bool _state, int _playerID)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("PlayerStateChangedRPC", RpcTarget.All, _state, _playerID);
			}
			else
			{
				PlayerStateChangedRPC(_state, _playerID);
			}
		}
	}

	[PunRPC]
	public void PlayerStateChangedRPC(bool _state, int _playerID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		for (int i = 0; i < playerCount; i++)
		{
			if (allPlayers[i].photonView.ViewID == _playerID)
			{
				playerInSight[i] = _state;
				break;
			}
		}
	}

	private bool InLineOfSight(PlayerAvatar _player, int i)
	{
		if (SemiFunc.PlayerVisionCheck(top.position, 10f, _player, previouslySeenList[i]) || SemiFunc.PlayerVisionCheck(bottom.position, 10f, _player, previouslySeenList[i]) || SemiFunc.PlayerVisionCheck(front.position, 10f, _player, previouslySeenList[i]) || SemiFunc.PlayerVisionCheck(back.position, 10f, _player, previouslySeenList[i]))
		{
			previouslySeenList[i] = true;
		}
		else
		{
			previouslySeenList[i] = false;
		}
		return previouslySeenList[i];
	}

	private void RedoPlayersInSightList()
	{
		for (int i = 0; i < playerCount; i++)
		{
			playerInSight[i] = false;
		}
	}

	private void UpdateHypnosisLinesList()
	{
		TurnOffHypnosisLines();
		List<PlayerAvatar> list = SemiFunc.PlayerGetList();
		for (int i = 0; i < list.Count; i++)
		{
			hypnosisLines[i].GetComponent<EyeLines>().InitializeLine(list[i]);
			hypnosisLines[i].SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		TurnOffHypnosisLines();
	}
}
