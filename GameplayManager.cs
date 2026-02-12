using UnityEngine;

public class GameplayManager : MonoBehaviour
{
	public static GameplayManager instance;

	internal bool tips;

	internal bool playerNames;

	internal bool photosensitivity;

	internal bool arachnophobia;

	internal bool itemUnequipAutoHold;

	internal bool aimInvertVertical;

	internal float cameraSmoothing;

	internal float aimSensitivity;

	internal float cameraAnimation;

	internal float cameraNoise;

	internal float cameraShake;

	private float cameraAnimationOverrideTimer;

	private float cameraNoiseOverrideTimer;

	private float cameraShakeOverrideTimer;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		UpdateAll();
	}

	private void Update()
	{
		if (cameraAnimationOverrideTimer > 0f)
		{
			cameraAnimationOverrideTimer -= Time.deltaTime;
			if (cameraAnimationOverrideTimer <= 0f)
			{
				UpdateCameraAnimation();
			}
		}
		if (cameraNoiseOverrideTimer > 0f)
		{
			cameraNoiseOverrideTimer -= Time.deltaTime;
		}
		else
		{
			cameraNoise = DataDirector.instance.SettingValueFetchFloat(DataDirector.Setting.CameraNoise);
		}
		if (cameraShakeOverrideTimer > 0f)
		{
			cameraShakeOverrideTimer -= Time.deltaTime;
			return;
		}
		cameraShake = DataDirector.instance.SettingValueFetchFloat(DataDirector.Setting.CameraShake);
		if ((bool)SpectateCamera.instance)
		{
			if (SpectateCamera.instance.CheckState(SpectateCamera.State.Death))
			{
				cameraShake *= 0.1f;
			}
			else
			{
				cameraShake *= 0.5f;
			}
		}
	}

	public void UpdateAll()
	{
		UpdateTips();
		UpdateCameraSmoothing();
		UpdateAimSensitivity();
		UpdateCameraAnimation();
		UpdatePlayerNames();
		UpdateAimInvertVertical();
		UpdatePhotosensitivity();
		UpdateArachnophobia();
		UpdateItemUnequipAutoHold();
	}

	public void UpdateTips()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.Tips) == 1)
		{
			tips = true;
		}
		else
		{
			tips = false;
		}
	}

	public void UpdateCameraSmoothing()
	{
		cameraSmoothing = DataDirector.instance.SettingValueFetch(DataDirector.Setting.CameraSmoothing);
	}

	public void UpdateAimSensitivity()
	{
		aimSensitivity = DataDirector.instance.SettingValueFetch(DataDirector.Setting.AimSensitivity);
	}

	public void UpdateCameraAnimation()
	{
		switch (DataDirector.instance.SettingValueFetch(DataDirector.Setting.CameraAnimation))
		{
		case 0:
			cameraAnimation = 0f;
			break;
		case 1:
			cameraAnimation = 0.25f;
			break;
		case 2:
			cameraAnimation = 0.5f;
			break;
		case 3:
			cameraAnimation = 0.75f;
			break;
		case 4:
			cameraAnimation = 1f;
			break;
		}
	}

	public void UpdatePlayerNames()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.PlayerNames) == 1)
		{
			playerNames = true;
		}
		else
		{
			playerNames = false;
		}
	}

	public void UpdateAimInvertVertical()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.AimInvertVertical) == 1)
		{
			aimInvertVertical = true;
		}
		else
		{
			aimInvertVertical = false;
		}
	}

	public void UpdatePhotosensitivity()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.Photosensitivity) == 1)
		{
			photosensitivity = true;
		}
		else
		{
			photosensitivity = false;
		}
	}

	public void UpdateArachnophobia()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.Arachnophobia) == 1)
		{
			arachnophobia = true;
		}
		else
		{
			arachnophobia = false;
		}
	}

	public void UpdateItemUnequipAutoHold()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.ItemUnequipAutoHold) == 1)
		{
			itemUnequipAutoHold = true;
		}
		else
		{
			itemUnequipAutoHold = false;
		}
	}

	public void OverrideCameraAnimation(float _value, float _time)
	{
		cameraAnimation = _value;
		cameraAnimationOverrideTimer = _time;
	}

	public void OverrideCameraNoise(float _value, float _time)
	{
		cameraNoise = _value;
		cameraNoiseOverrideTimer = _time;
	}

	public void OverrideCameraShake(float _value, float _time)
	{
		cameraShake = _value;
		cameraShakeOverrideTimer = _time;
	}
}
