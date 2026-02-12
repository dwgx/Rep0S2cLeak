using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessing : MonoBehaviour
{
	public static PostProcessing Instance;

	private bool setupDone;

	public PostProcessVolume volume;

	internal Grain grain;

	private float grainDisableTimer;

	internal Bloom bloom;

	private float bloomDisableTimer;

	internal ColorGrading colorGrading;

	private float colorGradingSaturation;

	private float colorGradingContrast;

	internal Vignette vignette;

	private Color vignetteColor;

	private float vignetteIntensity;

	private float vignetteSmoothness;

	internal MotionBlur motionBlur;

	internal LensDistortion lensDistortion;

	internal ChromaticAberration chromaticAberration;

	public AnimationCurve introCurve;

	public float introSpeed;

	private float introLerp;

	private float motionBlurDefault;

	private float bloomDefault;

	private float grainIntensityDefault;

	private float grainSizeDefault;

	[Space]
	private bool vignetteOverrideActive;

	private float vignetteOverrideLerp;

	private float vignetteOverrideTimer;

	private float vignetteOverrideSpeedIn;

	private float vignetteOverrideSpeedOut;

	private Color vignetteOverrideColor;

	private float vignetteOverrideIntensity;

	private float vignetteOverrideSmoothness;

	private GameObject vignetteOverrideObject;

	private bool saturationOverrideActive;

	private float saturationOverrideLerp;

	private float saturationOverrideTimer;

	private float saturationOverrideSpeedIn;

	private float saturationOverrideSpeedOut;

	private float saturationOverrideAmount;

	private GameObject saturationOverrideObject;

	private bool contrastOverrideActive;

	private float contrastOverrideLerp;

	private float contrastOverrideTimer;

	private float contrastOverrideSpeedIn;

	private float contrastOverrideSpeedOut;

	private float contrastOverrideAmount;

	private GameObject contrastOverrideObject;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		volume.profile.TryGetSettings<Grain>(out grain);
		grainSizeDefault = grain.size.value;
		grainIntensityDefault = grain.intensity.value;
		grain.intensity.value = 1f;
		volume.profile.TryGetSettings<MotionBlur>(out motionBlur);
		motionBlurDefault = motionBlur.shutterAngle.value;
		volume.profile.TryGetSettings<LensDistortion>(out lensDistortion);
		volume.profile.TryGetSettings<Bloom>(out bloom);
		volume.profile.TryGetSettings<ColorGrading>(out colorGrading);
		volume.profile.TryGetSettings<Vignette>(out vignette);
		volume.profile.TryGetSettings<ChromaticAberration>(out chromaticAberration);
	}

	private void Update()
	{
		if (!setupDone)
		{
			return;
		}
		Color value = vignetteColor;
		float value2 = vignetteIntensity;
		float value3 = vignetteSmoothness;
		if (vignetteOverrideActive)
		{
			if (vignetteOverrideTimer > 0f)
			{
				value = Color.Lerp(value, vignetteOverrideColor, vignetteOverrideLerp);
				value2 = Mathf.Lerp(value2, vignetteOverrideIntensity, vignetteOverrideLerp);
				value3 = Mathf.Lerp(value3, vignetteOverrideSmoothness, vignetteOverrideLerp);
				vignetteOverrideLerp += vignetteOverrideSpeedIn * Time.deltaTime;
				vignetteOverrideLerp = Mathf.Clamp01(vignetteOverrideLerp);
				vignetteOverrideTimer -= Time.deltaTime;
			}
			else
			{
				value = Color.Lerp(value, vignetteOverrideColor, vignetteOverrideLerp);
				value2 = Mathf.Lerp(value2, vignetteOverrideIntensity, vignetteOverrideLerp);
				value3 = Mathf.Lerp(value3, vignetteOverrideSmoothness, vignetteOverrideLerp);
				vignetteOverrideLerp -= vignetteOverrideSpeedOut * Time.deltaTime;
				if (vignetteOverrideLerp <= 0f)
				{
					vignetteOverrideActive = false;
					vignetteOverrideLerp = 0f;
				}
			}
		}
		vignette.color.value = value;
		vignette.intensity.value = value2;
		vignette.smoothness.value = value3;
		if (saturationOverrideActive)
		{
			if (saturationOverrideTimer > 0f)
			{
				colorGrading.saturation.value = Mathf.Lerp(colorGradingSaturation, saturationOverrideAmount, saturationOverrideLerp);
				saturationOverrideLerp += saturationOverrideSpeedIn * Time.deltaTime;
				saturationOverrideLerp = Mathf.Clamp01(saturationOverrideLerp);
				saturationOverrideTimer -= Time.deltaTime;
			}
			else
			{
				colorGrading.saturation.value = Mathf.Lerp(colorGradingSaturation, saturationOverrideAmount, saturationOverrideLerp);
				saturationOverrideLerp -= saturationOverrideSpeedOut * Time.deltaTime;
				if (saturationOverrideLerp <= 0f)
				{
					colorGrading.saturation.value = colorGradingSaturation;
					saturationOverrideActive = false;
					saturationOverrideLerp = 0f;
				}
			}
		}
		if (contrastOverrideActive)
		{
			if (contrastOverrideTimer > 0f)
			{
				colorGrading.contrast.value = Mathf.Lerp(colorGradingContrast, contrastOverrideAmount, contrastOverrideLerp);
				contrastOverrideLerp += contrastOverrideSpeedIn * Time.deltaTime;
				contrastOverrideLerp = Mathf.Clamp01(contrastOverrideLerp);
				contrastOverrideTimer -= Time.deltaTime;
			}
			else
			{
				colorGrading.contrast.value = Mathf.Lerp(colorGradingContrast, contrastOverrideAmount, contrastOverrideLerp);
				contrastOverrideLerp -= contrastOverrideSpeedOut * Time.deltaTime;
				if (contrastOverrideLerp <= 0f)
				{
					colorGrading.contrast.value = colorGradingContrast;
					contrastOverrideActive = false;
					contrastOverrideLerp = 0f;
				}
			}
		}
		if (bloomDisableTimer > 0f)
		{
			bloomDisableTimer -= Time.deltaTime;
			if (bloomDisableTimer <= 0f && DataDirector.instance.SettingValueFetch(DataDirector.Setting.Bloom) == 1)
			{
				bloom.active = true;
			}
		}
		if (grainDisableTimer > 0f)
		{
			grainDisableTimer -= Time.deltaTime;
			if (grainDisableTimer <= 0f && DataDirector.instance.SettingValueFetch(DataDirector.Setting.Grain) == 1)
			{
				grain.active = true;
			}
		}
	}

	public void SpectateSet()
	{
		motionBlur.shutterAngle.value = 1f;
	}

	public void SpectateReset()
	{
		motionBlur.shutterAngle.value = motionBlurDefault;
	}

	public void Setup()
	{
		colorGrading.temperature.value = LevelGenerator.Instance.Level.ColorTemperature;
		colorGrading.colorFilter.value = LevelGenerator.Instance.Level.ColorFilter;
		colorGradingSaturation = colorGrading.saturation.value;
		colorGradingContrast = colorGrading.contrast.value;
		bloom.intensity.value = LevelGenerator.Instance.Level.BloomIntensity;
		bloom.threshold.value = LevelGenerator.Instance.Level.BloomThreshold;
		vignette.color.value = LevelGenerator.Instance.Level.VignetteColor;
		vignetteColor = vignette.color.value;
		vignette.intensity.value = LevelGenerator.Instance.Level.VignetteIntensity;
		vignetteIntensity = vignette.intensity.value;
		vignette.smoothness.value = LevelGenerator.Instance.Level.VignetteSmoothness;
		vignetteSmoothness = vignette.smoothness.value;
		StartCoroutine(Intro());
		setupDone = true;
	}

	private IEnumerator Intro()
	{
		while (GameDirector.instance.currentState < GameDirector.gameState.Main)
		{
			yield return new WaitForSeconds(0.1f);
		}
		while (introLerp < 1f)
		{
			grain.intensity.value = Mathf.Lerp(0.8f, grainIntensityDefault, introCurve.Evaluate(introLerp));
			grain.size.value = Mathf.Lerp(1.5f, grainSizeDefault, introCurve.Evaluate(introLerp));
			introLerp += introSpeed * Time.deltaTime;
			yield return null;
		}
	}

	public void VignetteOverride(Color _color, float _intensity, float _smoothness, float _speedIn, float _speedOut, float _time, GameObject _obj)
	{
		if (!vignetteOverrideActive || !(_obj != vignetteOverrideObject))
		{
			_smoothness = Mathf.Clamp01(_smoothness);
			vignetteOverrideActive = true;
			vignetteOverrideObject = _obj;
			vignetteOverrideTimer = _time;
			vignetteOverrideSpeedIn = _speedIn;
			vignetteOverrideSpeedOut = _speedOut;
			vignetteOverrideColor = _color;
			vignetteOverrideIntensity = _intensity;
			vignetteOverrideSmoothness = _smoothness;
		}
	}

	public void SaturationOverride(float _amount, float _speedIn, float _speedOut, float _time, GameObject _obj)
	{
		if (!saturationOverrideActive || !(_obj != saturationOverrideObject))
		{
			saturationOverrideActive = true;
			saturationOverrideObject = _obj;
			saturationOverrideTimer = _time;
			saturationOverrideSpeedIn = _speedIn;
			saturationOverrideSpeedOut = _speedOut;
			saturationOverrideAmount = _amount;
		}
	}

	public void ContrastOverride(float _amount, float _speedIn, float _speedOut, float _time, GameObject _obj)
	{
		if (!contrastOverrideActive || !(_obj != contrastOverrideObject))
		{
			contrastOverrideActive = true;
			contrastOverrideObject = _obj;
			contrastOverrideTimer = _time;
			contrastOverrideSpeedIn = _speedIn;
			contrastOverrideSpeedOut = _speedOut;
			contrastOverrideAmount = _amount;
		}
	}

	public void BloomDisable(float _time)
	{
		bloomDisableTimer = _time;
		bloom.active = false;
	}

	public void GrainDisable(float _time)
	{
		grainDisableTimer = _time;
		grain.active = false;
	}
}
