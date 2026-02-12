using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
	public static CameraZoom Instance;

	public PlayerController PlayerController;

	public List<Camera> cams;

	public AnimNoise camNoise;

	public float playerZoomDefault;

	public float SprintZoom;

	private float SprintZoomCurrent;

	private float TumbleVelocityZoom;

	private float zoomLerp;

	private float zoomPrev;

	private float zoomCurrent;

	private float zoomNew;

	private GameObject OverrideZoomObject;

	private float OverrideZoomTimer;

	private float OverrideZoomSpeedIn;

	private float OverrideZoomSpeedOut;

	public AnimationCurve OverrideZoomCurve;

	private int OverrideZoomPriority = 999;

	private bool OverrideActive;

	public float MinimumZoom = 5f;

	public float MaximumZoom = 170f;

	private void Awake()
	{
		Instance = this;
		zoomPrev = playerZoomDefault;
		zoomNew = playerZoomDefault;
	}

	public void OverrideZoomSet(float zoom, float time, float speedIn, float speedOut, GameObject obj, int priority)
	{
		if (priority <= OverrideZoomPriority && (priority != OverrideZoomPriority || !(obj != OverrideZoomObject)))
		{
			if (obj != OverrideZoomObject)
			{
				zoomLerp = 0f;
				zoomPrev = zoomCurrent;
			}
			zoomNew = zoom;
			OverrideZoomObject = obj;
			OverrideZoomTimer = time;
			OverrideZoomSpeedIn = speedIn;
			OverrideZoomSpeedOut = speedOut;
			OverrideZoomPriority = priority;
			OverrideActive = true;
		}
	}

	private void Update()
	{
		if ((bool)SpectateCamera.instance || !LevelGenerator.Instance.Generated || PlayerController.playerAvatarScript.isDisabled)
		{
			return;
		}
		if (OverrideZoomTimer > 0f)
		{
			OverrideZoomTimer -= Time.deltaTime;
			zoomLerp += Time.deltaTime * OverrideZoomSpeedIn;
		}
		else if (OverrideZoomTimer <= 0f)
		{
			if (OverrideActive)
			{
				OverrideActive = false;
				OverrideZoomObject = null;
				OverrideZoomPriority = 999;
				zoomLerp = 0f;
				zoomPrev = zoomCurrent;
				zoomNew = playerZoomDefault;
			}
			zoomLerp += Time.deltaTime * OverrideZoomSpeedOut;
		}
		zoomLerp = Mathf.Clamp01(zoomLerp);
		if (PlayerController.CanSlide && StatsManager.instance.playerUpgradeSpeed.TryGetValue(PlayerController.instance.playerAvatarScript.steamID, out var value))
		{
			float num = SprintZoom + (float)value * 2f;
			num *= GameplayManager.instance.cameraAnimation;
			float num2 = Mathf.Lerp(0f, num, PlayerController.SprintSpeedLerp);
			SprintZoomCurrent = Mathf.Lerp(SprintZoomCurrent, num2, 2f * Time.deltaTime);
		}
		else
		{
			SprintZoomCurrent = Mathf.Lerp(SprintZoomCurrent, 0f, 2f * Time.deltaTime);
		}
		if (PlayerController.playerAvatarScript.isTumbling)
		{
			float value2 = PlayerController.playerAvatarScript.tumble.physGrabObject.rbVelocity.magnitude * 5f;
			value2 = Mathf.Clamp(value2, 0f, 30f);
			value2 *= GameplayManager.instance.cameraAnimation;
			TumbleVelocityZoom = Mathf.Lerp(TumbleVelocityZoom, value2, 2f * Time.deltaTime);
		}
		else
		{
			TumbleVelocityZoom = Mathf.Lerp(TumbleVelocityZoom, 0f, 2f * Time.deltaTime);
		}
		zoomCurrent = Mathf.LerpUnclamped(zoomPrev, zoomNew, OverrideZoomCurve.Evaluate(zoomLerp));
		if (SemiFunc.MenuLevel() && (bool)CameraNoPlayerTarget.instance)
		{
			zoomCurrent = CameraNoPlayerTarget.instance.cam.fieldOfView;
		}
		foreach (Camera cam in cams)
		{
			cam.fieldOfView = Mathf.Clamp(zoomCurrent + SprintZoomCurrent + TumbleVelocityZoom, MinimumZoom, MaximumZoom);
		}
	}
}
