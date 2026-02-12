using UnityEngine;

public class CameraAim : MonoBehaviour
{
	public static CameraAim Instance;

	public CameraTarget camController;

	public Transform playerTransform;

	public float AimSpeedMouse = 1f;

	public float AimSpeedGamepad = 1f;

	private float aimVertical;

	private float aimHorizontal;

	internal float aimSmoothOriginal = 2f;

	private Quaternion playerAim = Quaternion.identity;

	private Vector3 AimTargetPosition = Vector3.zero;

	public AnimationCurve AimTargetCurve;

	[Space]
	public bool AimTargetActive;

	private float AimTargetTimer;

	private float AimTargetSpeed;

	private float AimTargetLerp;

	private GameObject AimTargetObject;

	private int AimTargetPriority = 999;

	private bool AimTargetSoftActive;

	private float AimTargetSoftTimer;

	private float AimTargetSoftStrengthCurrent;

	private float AimTargetSoftStrength;

	private float AimTargetSoftStrengthNoAim;

	private Vector3 AimTargetSoftPosition;

	private GameObject AimTargetSoftObject;

	private int AimTargetSoftPriority = 999;

	private float overrideAimStopTimer;

	internal bool overrideAimStop;

	private float PlayerAimingTimer;

	private float overrideAimSmooth;

	private float overrideAimSmoothTimer;

	private float defaultSettingAmount;

	private float additiveAimY;

	private float overridePlayerAimDisableTimer;

	private float overrideNoSmoothTimer;

	private void Awake()
	{
		Instance = this;
	}

	public void AimTargetSet(Vector3 position, float time, float speed, GameObject obj, int priority)
	{
		if (priority <= AimTargetPriority && (!(obj != AimTargetObject) || AimTargetLerp == 0f))
		{
			AimTargetActive = true;
			AimTargetObject = obj;
			AimTargetPosition = position;
			AimTargetTimer = time;
			AimTargetSpeed = speed;
			AimTargetPriority = priority;
		}
	}

	public void AimTargetSoftSet(Vector3 position, float time, float strength, float strengthNoAim, GameObject obj, int priority)
	{
		if (priority <= AimTargetSoftPriority && (priority != AimTargetSoftPriority || !(obj != AimTargetSoftObject)) && (!AimTargetSoftObject || !(obj != AimTargetSoftObject)))
		{
			if (obj != AimTargetSoftObject)
			{
				PlayerAimingTimer = 0f;
			}
			AimTargetSoftPosition = position;
			AimTargetSoftTimer = time;
			AimTargetSoftStrength = strength;
			AimTargetSoftStrengthNoAim = strengthNoAim;
			AimTargetSoftObject = obj;
			AimTargetSoftPriority = priority;
		}
	}

	public void OverrideAimStop()
	{
		overrideAimStopTimer = 0.2f;
	}

	public void AdditiveAimY(float _amount)
	{
		additiveAimY += _amount;
	}

	private void OverrideAimStopTick()
	{
		if (overrideAimStopTimer > 0f)
		{
			overrideAimStop = true;
			overrideAimStopTimer -= Time.deltaTime;
		}
		else
		{
			overrideAimStop = false;
		}
	}

	private void Update()
	{
		if (AimTargetTimer > 0f || AimTargetSoftTimer > 0f || overrideAimSmoothTimer > 0f)
		{
			defaultSettingAmount = Mathf.Lerp(defaultSettingAmount, 1f, 10f * Time.deltaTime);
		}
		else
		{
			defaultSettingAmount = Mathf.Lerp(defaultSettingAmount, 0f, 10f * Time.deltaTime);
		}
		float num = Mathf.Lerp(GameplayManager.instance.cameraSmoothing / 100f, (float)DataDirector.instance.cameraSmoothingDefault / 100f, defaultSettingAmount);
		float t = Mathf.Lerp(GameplayManager.instance.aimSensitivity / 100f, (float)DataDirector.instance.aimSensitivityDefault / 100f, defaultSettingAmount);
		AimSpeedMouse = Mathf.Lerp(0.2f, 4f, t);
		if (GameDirector.instance.currentState >= GameDirector.gameState.Main)
		{
			if (!GameDirector.instance.DisableInput && AimTargetTimer <= 0f && !overrideAimStop && overridePlayerAimDisableTimer <= 0f)
			{
				InputManager.instance.mouseSensitivity = 0.05f;
				Vector2 vector = new Vector2(SemiFunc.InputMouseX(), SemiFunc.InputMouseY());
				Vector2 vector2 = new Vector2(Input.GetAxis("Gamepad Aim X"), Input.GetAxis("Gamepad Aim Y"));
				vector2 = Vector2.zero;
				if (GameplayManager.instance.aimInvertVertical)
				{
					vector.y *= -1f;
				}
				if (AimTargetSoftTimer > 0f)
				{
					vector = ((!(vector.magnitude > 1f)) ? Vector2.zero : vector.normalized);
					vector2 = ((!(vector2.magnitude > 0.1f)) ? Vector2.zero : vector2.normalized);
				}
				else
				{
					vector *= AimSpeedMouse;
					vector2 *= AimSpeedGamepad * Time.deltaTime;
				}
				aimHorizontal += vector[0];
				aimHorizontal += vector2[0];
				if (aimHorizontal > 360f)
				{
					aimHorizontal -= 360f;
				}
				if (aimHorizontal < -360f)
				{
					aimHorizontal += 360f;
				}
				aimVertical += 0f - vector[1];
				aimVertical += 0f - vector2[1];
				aimVertical = Mathf.Clamp(aimVertical, -70f, 80f);
				playerAim = Quaternion.Euler(aimVertical, aimHorizontal, 0f);
				if (num != 0f)
				{
					playerAim = Quaternion.RotateTowards(base.transform.localRotation, playerAim, 10000f * Time.deltaTime);
				}
				if (vector2.magnitude > 0f || vector.magnitude > 0f)
				{
					PlayerAimingTimer = 0.1f;
				}
			}
			if (PlayerAimingTimer > 0f)
			{
				PlayerAimingTimer -= Time.deltaTime;
			}
			if (overridePlayerAimDisableTimer > 0f)
			{
				aimHorizontal = Mathf.Lerp(aimHorizontal, 0f, 1f * Time.deltaTime);
				aimVertical = Mathf.Lerp(aimVertical, 0f, 1f * Time.deltaTime);
				playerAim = Quaternion.Euler(aimVertical, aimHorizontal, 0f);
				overridePlayerAimDisableTimer -= Time.deltaTime;
			}
			if (AimTargetTimer > 0f)
			{
				AimTargetTimer -= Time.deltaTime;
				AimTargetLerp += Time.deltaTime * AimTargetSpeed;
				AimTargetLerp = Mathf.Clamp01(AimTargetLerp);
			}
			else if (AimTargetLerp > 0f)
			{
				SetPlayerAim(base.transform.localRotation, _setRotation: false);
				AimTargetLerp = 0f;
				AimTargetPriority = 999;
				AimTargetActive = false;
			}
			Quaternion quaternion = Quaternion.LerpUnclamped(playerAim, Quaternion.LookRotation(AimTargetPosition - base.transform.position), AimTargetCurve.Evaluate(AimTargetLerp));
			if (AimTargetSoftTimer > 0f && AimTargetTimer <= 0f)
			{
				float num2 = AimTargetSoftStrength;
				if (PlayerAimingTimer <= 0f)
				{
					num2 = AimTargetSoftStrengthNoAim;
				}
				AimTargetSoftStrengthCurrent = Mathf.Lerp(AimTargetSoftStrengthCurrent, num2, 10f * Time.deltaTime);
				Quaternion quaternion2 = Quaternion.LookRotation(AimTargetSoftPosition - base.transform.position);
				quaternion = Quaternion.Lerp(quaternion, quaternion2, num2 * Time.deltaTime);
				AimTargetSoftTimer -= Time.deltaTime;
				if (AimTargetSoftTimer <= 0f)
				{
					AimTargetSoftObject = null;
					AimTargetSoftPriority = 999;
				}
			}
			float num3 = (aimSmoothOriginal = Mathf.Lerp(50f, 8f, num));
			if (overrideAimSmoothTimer > 0f)
			{
				num3 = overrideAimSmooth;
			}
			base.transform.localRotation = Quaternion.Euler(base.transform.localRotation.eulerAngles.x + additiveAimY, base.transform.localRotation.eulerAngles.y, base.transform.localRotation.eulerAngles.z);
			quaternion = Quaternion.Euler(quaternion.eulerAngles.x + additiveAimY, quaternion.eulerAngles.y, quaternion.eulerAngles.z);
			if (overrideNoSmoothTimer > 0f)
			{
				overrideNoSmoothTimer -= Time.deltaTime;
				base.transform.localRotation = quaternion;
			}
			else
			{
				base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, quaternion, num3 * Time.deltaTime);
			}
			SetPlayerAim(quaternion, _setRotation: false);
		}
		if (SemiFunc.MenuLevel() && (bool)CameraNoPlayerTarget.instance)
		{
			base.transform.localRotation = CameraNoPlayerTarget.instance.transform.rotation;
		}
		if (overrideAimSmoothTimer > 0f)
		{
			overrideAimSmoothTimer -= Time.deltaTime;
		}
		additiveAimY = 0f;
		OverrideAimStopTick();
	}

	public void SetPlayerAim(Quaternion _rotation, bool _setRotation)
	{
		if (_rotation.eulerAngles.x > 180f)
		{
			aimVertical = _rotation.eulerAngles.x - 360f;
		}
		else
		{
			aimVertical = _rotation.eulerAngles.x;
		}
		aimHorizontal = _rotation.eulerAngles.y;
		playerAim = Quaternion.Euler(aimVertical, aimHorizontal, 0f);
		if (_setRotation)
		{
			base.transform.localRotation = playerAim;
		}
	}

	public void OverrideAimSmooth(float _smooth, float _time)
	{
		overrideAimSmooth = _smooth;
		overrideAimSmoothTimer = _time;
	}

	public void OverrideNoSmooth(float _time)
	{
		overrideNoSmoothTimer = _time;
	}

	public void OverridePlayerAimDisable(bool _set)
	{
		if (_set)
		{
			aimHorizontal = 0f;
			aimVertical = 0f;
			playerAim = Quaternion.Euler(aimVertical, aimHorizontal, 0f);
			base.transform.localRotation = playerAim;
		}
		overridePlayerAimDisableTimer = 0.1f;
	}

	public void OverridePlayerAimDisableReset()
	{
		overridePlayerAimDisableTimer = 0f;
	}
}
