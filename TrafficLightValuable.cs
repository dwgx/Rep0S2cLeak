using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TrafficLightValuable : Trap
{
	public enum States
	{
		Red,
		Green,
		Yellow,
		RedFlickering,
		GreenFlickering,
		YellowFlickering,
		LingeringOnRed,
		LingeringOnRedFlickering,
		Off
	}

	[Header("MeshRenderers")]
	public MeshRenderer greenLight;

	public MeshRenderer redLight;

	public MeshRenderer yellowLight;

	public MeshRenderer boxLight;

	[Header("Point Lights")]
	public Light pointLightGreen;

	public Light pointLightRed;

	public Light pointLightYellow;

	[Header("Zap effect light")]
	public Light zapLight;

	[Header("Light Durations")]
	public float redLightDuration = 5f;

	public float greenLightDuration = 5f;

	public float yellowLightDuration = 2f;

	public float lingerOnRedTime = 2f;

	[Header("Particles")]
	public ParticleSystem sparkParticles;

	public ParticleSystem smokeParticles;

	public ParticleSystem zapParticles;

	[Header("Camera Shake")]
	public float cameraShakeTime = 0.2f;

	public float cameraShakeStrength = 3f;

	public Vector2 cameraShakeBounds = new Vector2(1.5f, 5f);

	[Header("Physics")]
	public float SelfTorqueMultiplier = 2f;

	public float tumbleForceMultiplier = 15f;

	[Header("Sounds")]
	public Sound zapSound;

	public Sound flickeringSound;

	public Sound turnOnSound;

	public Sound smokeSound;

	public Sound fastTickSound;

	public Sound slowTickSound;

	private Color color;

	internal States currentState = States.Off;

	internal States previousState = States.Off;

	private Rigidbody rb;

	private float lightTimer;

	private float lingerTimer;

	private bool isFlashing;

	private float flashDuration = 0.1f;

	private float flashTimer;

	private bool stateChanged;

	protected override void Start()
	{
		base.Start();
		TurnAllLightsOff();
		rb = GetComponent<Rigidbody>();
	}

	protected override void Update()
	{
		base.Update();
		CheckForStateChange();
		ClientBasedEffects();
		MasterClientStateManager();
	}

	private void CheckForStateChange()
	{
		if (currentState != previousState)
		{
			stateChanged = true;
			previousState = currentState;
		}
	}

	private void MasterClientStateManager()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			CheckForGrabActivate();
			if (IsActive() && currentState != States.LingeringOnRed && currentState != States.LingeringOnRedFlickering)
			{
				ManageLightState();
			}
			else if (currentState == States.LingeringOnRed || currentState == States.LingeringOnRedFlickering)
			{
				LingerOnRedState();
			}
			if (IsRed())
			{
				RedState();
			}
		}
	}

	private void CheckForGrabActivate()
	{
		if (!SemiFunc.FPSImpulse15() && physGrabObject.grabbed && !IsActive())
		{
			SetState(States.Green);
		}
	}

	private void ManageLightState()
	{
		lightTimer += Time.deltaTime;
		switch (currentState)
		{
		case States.Red:
			if (redLightDuration - lightTimer <= 0.5f)
			{
				SetState(States.RedFlickering);
			}
			break;
		case States.RedFlickering:
			if (lightTimer >= redLightDuration)
			{
				SetState(States.Off);
			}
			break;
		case States.Green:
			if (greenLightDuration - lightTimer <= 0.5f)
			{
				SetState(States.GreenFlickering);
			}
			break;
		case States.GreenFlickering:
			if (lightTimer >= greenLightDuration)
			{
				SetState(States.Yellow);
			}
			break;
		case States.Yellow:
			if (yellowLightDuration - lightTimer <= 0.5f)
			{
				SetState(States.YellowFlickering);
			}
			break;
		case States.YellowFlickering:
			if (lightTimer >= yellowLightDuration)
			{
				SetState(States.Red);
			}
			break;
		}
	}

	private void RedState()
	{
		if (!physGrabObject.grabbed)
		{
			return;
		}
		foreach (PhysGrabber item in new List<PhysGrabber>(physGrabObject.playerGrabbing))
		{
			ZapPlayer(item.playerAvatar);
		}
		if (currentState != States.LingeringOnRed)
		{
			SetState(States.LingeringOnRed);
		}
	}

	private void LingerOnRedState()
	{
		lingerTimer += Time.deltaTime;
		if (lingerOnRedTime - lingerTimer <= 0.5f && currentState != States.LingeringOnRedFlickering)
		{
			SetState(States.LingeringOnRedFlickering);
		}
		if (lingerTimer >= lingerOnRedTime)
		{
			lingerTimer = 0f;
			SetState(States.Off);
		}
	}

	private void ClientBasedEffects()
	{
		StartOfStateManager();
		MaterialEffects();
		SoundEffects();
		if (isFlashing)
		{
			FlashEffect();
		}
	}

	private void StartOfStateManager()
	{
		if (stateChanged)
		{
			switch (currentState)
			{
			case States.Off:
				TurnAllLightsOff();
				flickeringSound.Play(physGrabObject.centerPoint);
				break;
			case States.Red:
				color = Color.red;
				SwitchLightsBasedOnState();
				turnOnSound.Play(physGrabObject.centerPoint);
				flickeringSound.Play(physGrabObject.centerPoint);
				break;
			case States.Green:
				color = Color.green;
				SwitchLightsBasedOnState();
				turnOnSound.Play(physGrabObject.centerPoint);
				flickeringSound.Play(physGrabObject.centerPoint);
				break;
			case States.Yellow:
				color = Color.yellow;
				SwitchLightsBasedOnState();
				turnOnSound.Play(physGrabObject.centerPoint);
				flickeringSound.Play(physGrabObject.centerPoint);
				break;
			case States.LingeringOnRed:
				color = Color.red;
				SwitchLightsBasedOnState();
				zapSound.Play(physGrabObject.centerPoint);
				smokeSound.Play(physGrabObject.centerPoint);
				ZapParticles();
				ZapCameraShake();
				FlashStart();
				break;
			}
			stateChanged = false;
		}
	}

	private void MaterialEffects()
	{
		if (IsFlickering())
		{
			switch (currentState)
			{
			case States.RedFlickering:
				FlickerLight(redLight.material);
				break;
			case States.GreenFlickering:
				FlickerLight(greenLight.material);
				break;
			case States.YellowFlickering:
				FlickerLight(yellowLight.material);
				break;
			case States.LingeringOnRedFlickering:
				FlickerLight(redLight.material);
				break;
			case States.LingeringOnRed:
				break;
			}
		}
	}

	private void SoundEffects()
	{
		slowTickSound.PlayLoop(IsActive() && IsRed(), 1f, 1f);
		fastTickSound.PlayLoop(IsActive() && (IsGreen() || IsYellow()), 1f, 1f);
		if (IsFlickering())
		{
			flickeringSound.Play(physGrabObject.centerPoint);
		}
	}

	private void ZapPlayer(PlayerAvatar _player)
	{
		EnemyDirector.instance.SetInvestigate(base.transform.position, 15f);
		TumblePlayer(_player);
		TumbleTrafficLight();
	}

	private void TumblePlayer(PlayerAvatar _player)
	{
		Vector3 vector = _player.transform.localRotation * Vector3.back;
		_player.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
		_player.tumble.TumbleForce(vector * tumbleForceMultiplier);
		_player.tumble.TumbleTorque(-_player.transform.right * 45f);
		_player.tumble.TumbleOverrideTime(3f);
		_player.tumble.ImpactHurtSet(3f, 10);
	}

	private void TumbleTrafficLight()
	{
		Vector3 torque = Random.insideUnitSphere.normalized * SelfTorqueMultiplier;
		Vector3 vector = Random.insideUnitSphere.normalized * SelfTorqueMultiplier;
		rb.AddTorque(torque, ForceMode.Impulse);
		rb.AddForce(vector * SelfTorqueMultiplier, ForceMode.Impulse);
	}

	[PunRPC]
	public void SetStateRPC(States _state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = _state;
		}
	}

	private void SetState(States _state)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				SetStateRPC(_state);
				return;
			}
			photonView.RPC("SetStateRPC", RpcTarget.All, _state);
		}
	}

	private void TurnAllLightsOff()
	{
		pointLightGreen.enabled = false;
		pointLightRed.enabled = false;
		pointLightYellow.enabled = false;
		yellowLight.material.DisableKeyword("_EMISSION");
		greenLight.material.DisableKeyword("_EMISSION");
		redLight.material.DisableKeyword("_EMISSION");
		boxLight.material.DisableKeyword("_EMISSION");
	}

	private void SwitchLightsBasedOnState()
	{
		lightTimer = 0f;
		TurnAllLightsOff();
		switch (currentState)
		{
		case States.Red:
			ToggleLight(_status: true, redLight.material, pointLightRed);
			break;
		case States.Green:
			ToggleLight(_status: true, greenLight.material, pointLightGreen);
			break;
		case States.Yellow:
			ToggleLight(_status: true, yellowLight.material, pointLightYellow);
			break;
		case States.LingeringOnRed:
			ToggleLight(_status: true, redLight.material, pointLightRed);
			break;
		case States.RedFlickering:
		case States.GreenFlickering:
		case States.YellowFlickering:
			break;
		}
	}

	private void ToggleLight(bool _status, Material _material, Light _light)
	{
		if (_status)
		{
			_material.EnableKeyword("_EMISSION");
			_material.SetColor("_EmissionColor", color * 2f);
			_light.intensity = 3f;
			_light.enabled = true;
		}
		else
		{
			_material.DisableKeyword("_EMISSION");
			_light.enabled = false;
		}
	}

	private void ZapParticles()
	{
		sparkParticles.Play();
		smokeParticles.Play();
		zapParticles.Play();
	}

	private void ZapCameraShake()
	{
		GameDirector.instance.CameraShake.ShakeDistance(cameraShakeStrength, 3f, 8f, base.transform.position, cameraShakeTime);
		GameDirector.instance.CameraImpact.ShakeDistance(cameraShakeStrength, cameraShakeBounds.x, cameraShakeBounds.y, base.transform.position, cameraShakeTime);
	}

	private void FlickerLight(Material _light)
	{
		float num = Mathf.PingPong(Time.time, Random.Range(0.3f, 1.5f));
		_light.SetColor("_EmissionColor", color * num);
		if (pointLightGreen.enabled)
		{
			pointLightGreen.intensity = num * 2f;
		}
		if (pointLightRed.enabled)
		{
			pointLightRed.intensity = num * 2f;
		}
		if (pointLightYellow.enabled)
		{
			pointLightYellow.intensity = num * 2f;
		}
	}

	private void FlashStart()
	{
		isFlashing = true;
		flashTimer = 0f;
		zapLight.enabled = true;
	}

	private void FlashEffect()
	{
		flashTimer += Time.deltaTime;
		if (flashTimer >= flashDuration)
		{
			zapLight.enabled = false;
			isFlashing = false;
		}
	}

	private bool IsActive()
	{
		return currentState != States.Off;
	}

	private bool IsYellow()
	{
		if (currentState != States.Yellow)
		{
			return currentState == States.YellowFlickering;
		}
		return true;
	}

	private bool IsRed()
	{
		if (currentState != States.Red && currentState != States.RedFlickering && currentState != States.LingeringOnRed)
		{
			return currentState == States.LingeringOnRedFlickering;
		}
		return true;
	}

	private bool IsGreen()
	{
		if (currentState != States.Green)
		{
			return currentState == States.GreenFlickering;
		}
		return true;
	}

	private bool IsFlickering()
	{
		if (currentState != States.RedFlickering && currentState != States.GreenFlickering && currentState != States.YellowFlickering)
		{
			return currentState == States.LingeringOnRedFlickering;
		}
		return true;
	}
}
