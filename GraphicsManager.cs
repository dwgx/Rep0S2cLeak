using System.Collections.Generic;
using UnityEngine;

public class GraphicsManager : MonoBehaviour
{
	public static GraphicsManager instance;

	internal float lightDistance;

	internal float shadowDistance;

	internal float gamma;

	public AnimationCurve gammaCurve;

	internal bool glitchLoop;

	private float fullscreenCheckTimer;

	private FullScreenMode windowMode;

	private bool windowFullscreen;

	private float firstSetupTimer = 1f;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		if (firstSetupTimer > 0f)
		{
			firstSetupTimer -= Time.deltaTime;
			if (firstSetupTimer <= 0f)
			{
				UpdateAll();
			}
		}
		else if (fullscreenCheckTimer <= 0f)
		{
			fullscreenCheckTimer = 1f;
			if (Screen.fullScreenMode != windowMode || Screen.fullScreen != windowFullscreen)
			{
				if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
				{
					DataDirector.instance.SettingValueSet(DataDirector.Setting.WindowMode, 0);
				}
				else if (Screen.fullScreenMode == FullScreenMode.Windowed)
				{
					DataDirector.instance.SettingValueSet(DataDirector.Setting.WindowMode, 1);
				}
				UpdateWindowMode(_setResolution: false);
				if ((bool)GraphicsButtonWindowMode.instance)
				{
					GraphicsButtonWindowMode.instance.UpdateSlider();
				}
			}
		}
		else
		{
			fullscreenCheckTimer -= Time.deltaTime;
		}
	}

	public void UpdateAll()
	{
		UpdateVsync();
		UpdateMaxFPS();
		UpdateLightDistance();
		UpdateShadowQuality();
		UpdateShadowDistance();
		UpdateMotionBlur();
		UpdateLensDistortion();
		UpdateBloom();
		UpdateChromaticAberration();
		UpdateGrain();
		UpdateWindowMode(_setResolution: false);
		UpdateRenderSize();
		UpdateGlitchLoop();
		UpdateGamma();
	}

	public void UpdateVsync()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.Vsync) == 1)
		{
			QualitySettings.vSyncCount = 1;
		}
		else
		{
			QualitySettings.vSyncCount = 0;
		}
	}

	public void UpdateMaxFPS()
	{
		Application.targetFrameRate = DataDirector.instance.SettingValueFetch(DataDirector.Setting.MaxFPS);
	}

	public void UpdateLightDistance()
	{
		switch (DataDirector.instance.SettingValueFetch(DataDirector.Setting.LightDistance))
		{
		case 0:
			lightDistance = 10f;
			break;
		case 1:
			lightDistance = 15f;
			break;
		case 2:
			lightDistance = 20f;
			break;
		case 3:
			lightDistance = 25f;
			break;
		case 4:
			lightDistance = 30f;
			break;
		}
		LightManager.instance.UpdateInstant();
	}

	public void UpdateShadowQuality()
	{
		switch (DataDirector.instance.SettingValueFetch(DataDirector.Setting.ShadowQuality))
		{
		case 0:
			QualitySettings.shadowResolution = ShadowResolution.Low;
			break;
		case 1:
			QualitySettings.shadowResolution = ShadowResolution.Medium;
			break;
		case 2:
			QualitySettings.shadowResolution = ShadowResolution.High;
			break;
		case 3:
			QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
			break;
		}
	}

	public void UpdateShadowDistance()
	{
		switch (DataDirector.instance.SettingValueFetch(DataDirector.Setting.ShadowDistance))
		{
		case 0:
			shadowDistance = 5f;
			break;
		case 1:
			shadowDistance = 10f;
			break;
		case 2:
			shadowDistance = 15f;
			break;
		case 3:
			shadowDistance = 20f;
			break;
		case 4:
			shadowDistance = 25f;
			break;
		}
		QualitySettings.shadowDistance = shadowDistance;
	}

	public void UpdateMotionBlur()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.MotionBlur) == 1)
		{
			PostProcessing.Instance.motionBlur.active = true;
		}
		else
		{
			PostProcessing.Instance.motionBlur.active = false;
		}
	}

	public void UpdateLensDistortion()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.LensEffect) == 1)
		{
			PostProcessing.Instance.lensDistortion.active = true;
		}
		else
		{
			PostProcessing.Instance.lensDistortion.active = false;
		}
	}

	public void UpdateBloom()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.Bloom) == 1)
		{
			PostProcessing.Instance.bloom.active = true;
		}
		else
		{
			PostProcessing.Instance.bloom.active = false;
		}
	}

	public void UpdateChromaticAberration()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.ChromaticAberration) == 1)
		{
			PostProcessing.Instance.chromaticAberration.active = true;
		}
		else
		{
			PostProcessing.Instance.chromaticAberration.active = false;
		}
	}

	public void UpdateGrain()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.Grain) == 1)
		{
			PostProcessing.Instance.grain.active = true;
		}
		else
		{
			PostProcessing.Instance.grain.active = false;
		}
	}

	public void UpdateWindowMode(bool _setResolution)
	{
		switch (DataDirector.instance.SettingValueFetch(DataDirector.Setting.WindowMode))
		{
		case 0:
			windowMode = FullScreenMode.FullScreenWindow;
			windowFullscreen = true;
			Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, windowFullscreen);
			break;
		case 1:
		{
			windowMode = FullScreenMode.Windowed;
			windowFullscreen = false;
			if (!_setResolution)
			{
				break;
			}
			List<Resolution> list = new List<Resolution>();
			Resolution[] resolutions = Screen.resolutions;
			for (int i = 0; i < resolutions.Length; i++)
			{
				Resolution item = resolutions[i];
				if ((float)item.width / (float)item.height == 1.7777778f)
				{
					list.Add(item);
				}
			}
			Resolution resolution = Screen.resolutions[Screen.resolutions.Length - 1];
			if (list.Count > 0)
			{
				resolution = list[list.Count / 2];
			}
			Screen.SetResolution(resolution.width, resolution.height, windowFullscreen);
			break;
		}
		}
		fullscreenCheckTimer = 1f;
	}

	public void UpdateRenderSize()
	{
		switch (DataDirector.instance.SettingValueFetch(DataDirector.Setting.RenderSize))
		{
		case 0:
			RenderTextureMain.instance.textureWidthOriginal = RenderTextureMain.instance.textureWidthLarge;
			RenderTextureMain.instance.textureHeightOriginal = RenderTextureMain.instance.textureHeightLarge;
			break;
		case 1:
			RenderTextureMain.instance.textureWidthOriginal = RenderTextureMain.instance.textureWidthMedium;
			RenderTextureMain.instance.textureHeightOriginal = RenderTextureMain.instance.textureHeightMedium;
			break;
		case 2:
			RenderTextureMain.instance.textureWidthOriginal = RenderTextureMain.instance.textureWidthSmall;
			RenderTextureMain.instance.textureHeightOriginal = RenderTextureMain.instance.textureHeightSmall;
			break;
		}
		RenderTextureMain.instance.ResetResolution();
	}

	public void UpdateGlitchLoop()
	{
		if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.GlitchLoop) == 1)
		{
			glitchLoop = true;
		}
		else
		{
			glitchLoop = false;
		}
	}

	public void UpdateGamma()
	{
		gamma = DataDirector.instance.SettingValueFetch(DataDirector.Setting.Gamma);
		PostProcessing.Instance.colorGrading.gamma.value = new Vector4(0f, 0f, 0f, gammaCurve.Evaluate(gamma / 100f));
	}
}
