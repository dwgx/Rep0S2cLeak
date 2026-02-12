using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpectateHeadUI : SemiUI
{
	public static SpectateHeadUI instance;

	private bool ready;

	private bool center = true;

	private float focusTimer;

	private bool spectateOverride;

	[Space(20f)]
	public Sound soundReady;

	[Space(20f)]
	public RawImage[] colorImages;

	public RawImage[] colorImagesDark;

	private int colorIndex = -1;

	private Color color;

	public AnimationCurve colorCurve;

	private Color colorNew;

	private Color colorDarkNew;

	private Color colorPrev;

	private Color colorDarkPrev;

	private float colorLerp;

	[Space(20f)]
	public Transform graphicTransform;

	public AnimationCurve graphicScaleCurve;

	private float graphicScaleLerp;

	private float graphicScaleNew;

	private float graphicScalePrev;

	[Space(20f)]
	public RectTransform lidTransformTop;

	public RectTransform lidTransformBot;

	[Space(20f)]
	public RectTransform pupilTransformRight;

	public RectTransform pupilTransformLeft;

	public RawImage[] pupilImages;

	public AnimationCurve pupilAnimationCurve;

	public AnimationCurve pupilAnimationCurveHide;

	private float pupilScaleLerp;

	private float pupilScaleTargetPrev;

	private float pupilScaleTargetNew;

	[Space(20f)]
	public CanvasGroup glowCanvasGroup;

	public AnimationCurve glowAlphaCurve;

	private float glowAlphaLerp = 1f;

	public RectTransform glowTransform01;

	public RectTransform glowTransform02;

	public RectTransform glowTransform03;

	public RawImage[] glowImages;

	[Space(20f)]
	public RectTransform parentTransform;

	public AnimationCurve parentCenterCurve;

	public float parentCenterWidth = 200f;

	private float parentDefaultPosition;

	private float parentCenterLerp;

	private float parentCenterPrev;

	private float parentCenterNew;

	[Space(20f)]
	public TextMeshProUGUI promptText;

	public RectTransform promptTransform;

	public Transform promptTargetTransform;

	public AnimationCurve promptCurveIntro;

	public AnimationCurve promptCurveOutro;

	private float promptLerp;

	private bool promptHidden;

	[Space(20f)]
	public Transform shakeTransform;

	private float shakeAmount;

	private float shakeCooldown;

	private float shakeUpdate;

	private Vector3 shakeTarget;

	[Space(20f)]
	public Transform jumpTransform;

	public Transform squashTransform;

	public AnimationCurve squashIntroCurve;

	public AnimationCurve squashOutroCurve;

	private bool squashActive;

	private float squashLerp;

	private float squashAmount;

	private float squashAmountMax;

	[Space(20f)]
	public Transform[] legAnimTransforms;

	public AnimationCurve legAnimPositionCurveIntro;

	public AnimationCurve legAnimPositionCurveJump;

	public AnimationCurve legAnimPositionCurveOutro;

	private float legAnimPositionLerp;

	private float legAnimPositionPrev;

	private float legAnimPositionNew = -1f;

	private bool legAnimPositionJump;

	[Space]
	public AnimationCurve legAnimRotationCurveIntro;

	public AnimationCurve legAnimRotationCurveJump;

	public AnimationCurve legAnimRotationCurveOutro;

	private float legAnimRotationLerp;

	private float legAnimRotationPrev;

	private float legAnimRotationNew = -1f;

	private bool legAnimRotationJump;

	private void Awake()
	{
		instance = this;
	}

	protected override void Start()
	{
		base.Start();
		parentDefaultPosition = parentTransform.anchoredPosition.x;
		promptTransform.gameObject.SetActive(value: false);
		promptHidden = true;
	}

	protected override void Update()
	{
		base.Update();
		if (!SpectateCamera.instance || SpectateCamera.instance.CheckState(SpectateCamera.State.Death) || ((bool)PlayerController.instance && (bool)PlayerController.instance.playerAvatarScript && !PlayerController.instance.playerAvatarScript.playerDeathHead))
		{
			Hide();
		}
		if ((bool)SpectateCamera.instance && SpectateCamera.instance.CheckState(SpectateCamera.State.Head))
		{
			if (!center)
			{
				center = true;
				shakeAmount = 100f;
				parentCenterLerp = 0f;
				parentCenterPrev = parentTransform.anchoredPosition.x;
				parentCenterNew = 0f;
			}
			shakeAmount = Mathf.Max(shakeAmount, 2f);
		}
		else if (center)
		{
			center = false;
			parentCenterLerp = 0f;
			shakeAmount = 100f;
			parentCenterPrev = parentTransform.anchoredPosition.x;
			parentCenterNew = parentDefaultPosition;
		}
		parentCenterLerp += 2f * Time.deltaTime;
		parentCenterLerp = Mathf.Clamp01(parentCenterLerp);
		parentTransform.anchoredPosition = new Vector2(Mathf.LerpUnclamped(parentCenterPrev, parentCenterNew, parentCenterCurve.Evaluate(parentCenterLerp)), parentTransform.anchoredPosition.y);
		if (!SpectateCamera.instance)
		{
			return;
		}
		if (ready != SpectateCamera.instance.headEnergyEnough)
		{
			ready = SpectateCamera.instance.headEnergyEnough;
			if (ready)
			{
				soundReady.Play(base.transform.position);
				focusTimer = 5f;
				shakeAmount = 25f;
			}
		}
		if (focusTimer > 0f)
		{
			focusTimer -= Time.deltaTime;
		}
		if (center)
		{
			focusTimer = 0f;
		}
		if (!spectateOverride && (ready || SpectateCamera.instance.CheckState(SpectateCamera.State.Head)))
		{
			if (promptHidden)
			{
				promptHidden = false;
				promptTransform.gameObject.SetActive(value: true);
				promptText.text = InputManager.instance.InputDisplayReplaceTags("<color=#FF8C00>Press</color> [interact]", "<color=white>", "</color>");
			}
			promptLerp += 3f * Time.deltaTime;
			promptLerp = Mathf.Clamp01(promptLerp);
			promptTransform.position = new Vector2(promptTransform.position.x, Mathf.LerpUnclamped(graphicTransform.position.y, promptTargetTransform.position.y + 10f, promptCurveIntro.Evaluate(promptLerp)));
		}
		else if (!promptHidden)
		{
			promptLerp -= 3f * Time.deltaTime;
			promptLerp = Mathf.Clamp01(promptLerp);
			promptTransform.position = new Vector2(promptTransform.position.x, Mathf.LerpUnclamped(graphicTransform.position.y, promptTargetTransform.position.y + 10f, promptCurveOutro.Evaluate(promptLerp)));
			if (promptLerp <= 0f)
			{
				promptTransform.gameObject.SetActive(value: false);
				promptHidden = true;
			}
		}
		if (colorIndex != PlayerController.instance.playerAvatarScript.playerAvatarVisuals.colorIndex)
		{
			colorIndex = PlayerController.instance.playerAvatarScript.playerAvatarVisuals.colorIndex;
			this.color = AssetManager.instance.playerColors[colorIndex];
		}
		if (colorNew != this.color)
		{
			colorNew = this.color;
			colorDarkNew = Color.Lerp(this.color, Color.black, 0.5f);
			colorPrev = colorImages[0].color;
			colorDarkPrev = colorImagesDark[0].color;
			colorLerp = 0f;
		}
		RawImage[] array;
		if (colorLerp < 1f)
		{
			colorLerp += 2f * Time.deltaTime;
			colorLerp = Mathf.Clamp01(colorLerp);
			float t = colorCurve.Evaluate(colorLerp);
			Color color = Color.LerpUnclamped(colorPrev, colorNew, t);
			array = colorImages;
			foreach (RawImage rawImage in array)
			{
				rawImage.color = new Color(color.r, color.g, color.b, rawImage.color.a);
			}
			Color color2 = Color.LerpUnclamped(colorDarkPrev, colorDarkNew, t);
			array = colorImagesDark;
			foreach (RawImage rawImage2 in array)
			{
				rawImage2.color = new Color(color2.r, color2.g, color2.b, rawImage2.color.a);
			}
		}
		lidTransformTop.localPosition = new Vector2(0f, Mathf.LerpUnclamped(-5f, 60f, SpectateCamera.instance.headEnergy));
		lidTransformBot.localPosition = new Vector2(0f, Mathf.LerpUnclamped(5f, -50f, SpectateCamera.instance.headEnergy));
		if (center)
		{
			if (graphicScaleNew != 0.2f)
			{
				graphicScaleNew = 0.2f;
				graphicScalePrev = graphicTransform.localScale.x;
				graphicScaleLerp = 0f;
			}
		}
		else if (ready && focusTimer > 0f)
		{
			if (graphicScaleNew != 0.15f)
			{
				graphicScaleNew = 0.15f;
				graphicScalePrev = graphicTransform.localScale.x;
				graphicScaleLerp = 0f;
			}
		}
		else if (graphicScaleNew != 0.125f)
		{
			graphicScaleNew = 0.125f;
			graphicScalePrev = graphicTransform.localScale.x;
			graphicScaleLerp = 0f;
		}
		if (graphicScaleLerp < 1f)
		{
			graphicScaleLerp += 2f * Time.deltaTime;
			graphicScaleLerp = Mathf.Clamp01(graphicScaleLerp);
			float num = Mathf.LerpUnclamped(graphicScalePrev, graphicScaleNew, graphicScaleCurve.Evaluate(graphicScaleLerp));
			graphicTransform.localScale = new Vector3(num, num, 1f);
		}
		AnimationCurve animationCurve = pupilAnimationCurve;
		if (!spectateOverride && center)
		{
			if (pupilScaleTargetNew != 1f)
			{
				pupilScaleTargetNew = 1f;
				pupilScaleTargetPrev = pupilTransformRight.localScale.x;
				pupilScaleLerp = 0f;
			}
		}
		else if (spectateOverride || ready)
		{
			if (pupilScaleTargetNew != 0.3f)
			{
				pupilScaleTargetNew = 0.3f;
				pupilScaleTargetPrev = pupilTransformRight.localScale.x;
				pupilScaleLerp = 0f;
			}
		}
		else
		{
			if (pupilScaleTargetNew != 0f)
			{
				pupilScaleTargetNew = 0f;
				pupilScaleTargetPrev = pupilTransformRight.localScale.x;
				pupilScaleLerp = 0f;
			}
			animationCurve = pupilAnimationCurveHide;
		}
		if (pupilScaleLerp < 1f)
		{
			pupilScaleLerp += 2f * Time.deltaTime;
			pupilScaleLerp = Mathf.Clamp01(pupilScaleLerp);
			float num2 = Mathf.LerpUnclamped(pupilScaleTargetPrev, pupilScaleTargetNew, animationCurve.Evaluate(pupilScaleLerp));
			pupilTransformRight.localScale = new Vector3(num2, num2, 1f);
			pupilTransformLeft.localScale = new Vector3(num2, num2, 1f);
		}
		if (center)
		{
			if (glowAlphaLerp < 1f)
			{
				glowAlphaLerp += 2f * Time.deltaTime;
				glowAlphaLerp = Mathf.Clamp01(glowAlphaLerp);
				glowCanvasGroup.alpha = glowAlphaCurve.Evaluate(glowAlphaLerp);
			}
		}
		else if (glowAlphaLerp > 0f)
		{
			glowAlphaLerp -= 2f * Time.deltaTime;
			glowAlphaLerp = Mathf.Clamp01(glowAlphaLerp);
			glowCanvasGroup.alpha = glowAlphaCurve.Evaluate(glowAlphaLerp);
		}
		if (glowAlphaLerp > 0f)
		{
			glowTransform01.localEulerAngles = new Vector3(0f, 0f, glowTransform01.localEulerAngles.z + 50f * Time.deltaTime);
			glowTransform02.localEulerAngles = new Vector3(0f, 0f, glowTransform01.localEulerAngles.z + 45f + 25f * Time.deltaTime);
			glowTransform03.localEulerAngles = new Vector3(0f, 0f, glowTransform01.localEulerAngles.z + 225f + 5f * Time.deltaTime);
		}
		if (shakeAmount > 0f)
		{
			if (shakeCooldown <= 0f)
			{
				shakeCooldown = Random.Range(0.01f, 0.05f);
				shakeTarget = new Vector3(Random.Range(0f - shakeAmount, shakeAmount), Random.Range(0f - shakeAmount, shakeAmount), Random.Range(0f - shakeAmount, shakeAmount) * 0.5f);
				shakeAmount -= shakeAmount * 0.25f;
			}
			else
			{
				shakeCooldown -= Time.deltaTime;
			}
			if (shakeUpdate <= 0f)
			{
				shakeUpdate = 0.02f;
				shakeTransform.localPosition = Vector3.Lerp(shakeTransform.localPosition, new Vector3(shakeTarget.x, shakeTarget.y, 0f), 0.5f);
				shakeTransform.localRotation = Quaternion.Slerp(shakeTransform.localRotation, Quaternion.Euler(new Vector3(0f, 0f, shakeTarget.z)), 0.5f);
			}
			else
			{
				shakeUpdate -= Time.deltaTime;
			}
		}
		bool flag;
		if (center)
		{
			PlayerDeathHead playerDeathHead = PlayerController.instance.playerAvatarScript.playerDeathHead;
			if (playerDeathHead.spectatedJumpCharging)
			{
				flag = true;
				squashAmount = playerDeathHead.spectatedJumpChargeAmount;
				squashAmountMax = playerDeathHead.spectatedJumpChargeAmountMax;
			}
			else
			{
				flag = false;
			}
		}
		else
		{
			flag = false;
		}
		if (squashActive != flag)
		{
			squashActive = flag;
			squashLerp = 0f;
			if (!squashActive)
			{
				shakeAmount = 50f;
			}
		}
		if (squashActive)
		{
			float num3 = squashIntroCurve.Evaluate(squashAmount / squashAmountMax) * 0.2f;
			float y = squashIntroCurve.Evaluate(squashAmount / squashAmountMax) * -75f;
			squashTransform.localScale = new Vector3(1f + num3, 1f - num3, 1f);
			jumpTransform.localPosition = new Vector3(0f, y, 0f);
			shakeAmount = Mathf.Max(shakeAmount, squashIntroCurve.Evaluate(squashAmount / squashAmountMax) * 5f);
		}
		else if (squashLerp < 1f)
		{
			squashLerp += 1f * Time.deltaTime;
			squashLerp = Mathf.Clamp01(squashLerp);
			float num4 = squashIntroCurve.Evaluate(squashAmount / squashAmountMax) * 0.2f;
			float y2 = squashIntroCurve.Evaluate(squashAmount / squashAmountMax) * -75f;
			squashTransform.localScale = Vector3.LerpUnclamped(new Vector3(1f + num4, 1f - num4, 1f), Vector3.one, squashOutroCurve.Evaluate(squashLerp));
			jumpTransform.localPosition = Vector3.LerpUnclamped(new Vector3(0f, y2, 0f), Vector3.zero, squashOutroCurve.Evaluate(squashLerp));
		}
		float num5 = 2f;
		AnimationCurve animationCurve2 = legAnimPositionCurveIntro;
		float num6 = 2f;
		AnimationCurve animationCurve3 = legAnimRotationCurveIntro;
		float num7 = 0f;
		if (center && squashActive)
		{
			if (legAnimPositionNew != 150f)
			{
				legAnimPositionNew = 150f;
				legAnimPositionPrev = legAnimTransforms[0].localPosition.x;
				legAnimPositionLerp = 0f;
			}
			if (legAnimRotationNew != 80f)
			{
				legAnimRotationNew = 80f;
				legAnimRotationPrev = legAnimTransforms[0].localRotation.z;
				legAnimRotationLerp = 0f;
			}
			legAnimRotationJump = true;
			legAnimPositionJump = true;
			num7 = squashAmount * 30f;
		}
		else
		{
			if (legAnimPositionJump)
			{
				animationCurve2 = legAnimPositionCurveJump;
				num5 = 3f;
				if (legAnimPositionNew != 100f)
				{
					legAnimPositionNew = 100f;
					legAnimPositionPrev = legAnimTransforms[0].localPosition.x;
					legAnimPositionLerp = 0f;
				}
				if (legAnimPositionLerp >= 1f)
				{
					legAnimPositionJump = false;
				}
			}
			else
			{
				animationCurve2 = legAnimPositionCurveOutro;
				num5 = 2f;
				if (legAnimPositionNew != 0f)
				{
					legAnimPositionNew = 0f;
					legAnimPositionPrev = legAnimTransforms[0].localPosition.x;
					legAnimPositionLerp = 0f;
				}
			}
			if (legAnimRotationJump)
			{
				animationCurve3 = legAnimRotationCurveJump;
				num6 = 3f;
				if (legAnimRotationNew != 0f)
				{
					legAnimRotationNew = 0f;
					legAnimRotationPrev = legAnimTransforms[0].localEulerAngles.z;
					legAnimRotationLerp = 0f;
				}
				if (legAnimRotationLerp >= 1f)
				{
					legAnimRotationJump = false;
				}
			}
			else
			{
				animationCurve3 = legAnimRotationCurveOutro;
				num6 = 2f;
				if (legAnimRotationNew != 90f)
				{
					legAnimRotationNew = 90f;
					legAnimRotationPrev = legAnimTransforms[0].localEulerAngles.z;
					legAnimRotationLerp = 0f;
				}
			}
		}
		Transform[] array2;
		if (legAnimPositionLerp < 1f)
		{
			legAnimPositionLerp += num5 * Time.deltaTime;
			legAnimPositionLerp = Mathf.Clamp01(legAnimPositionLerp);
			float x = Mathf.LerpUnclamped(legAnimPositionPrev, legAnimPositionNew, animationCurve2.Evaluate(legAnimPositionLerp));
			array2 = legAnimTransforms;
			foreach (Transform transform in array2)
			{
				transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
			}
		}
		if (legAnimRotationLerp < 1f)
		{
			legAnimRotationLerp += num6 * Time.deltaTime;
			legAnimRotationLerp = Mathf.Clamp01(legAnimRotationLerp);
		}
		float num8 = Mathf.LerpUnclamped(legAnimRotationPrev, legAnimRotationNew, animationCurve3.Evaluate(legAnimRotationLerp));
		array2 = legAnimTransforms;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].localEulerAngles = new Vector3(0f, 0f, num8 + num7);
		}
		PlayerDeathHead playerDeathHead2 = PlayerController.instance.playerAvatarScript.playerDeathHead;
		if (playerDeathHead2.overrideSpectated == spectateOverride)
		{
			return;
		}
		spectateOverride = playerDeathHead2.overrideSpectated;
		array = pupilImages;
		foreach (RawImage rawImage3 in array)
		{
			if (spectateOverride)
			{
				rawImage3.color = new Color(1f, 0f, 0.18f);
			}
			else
			{
				rawImage3.color = Color.white;
			}
		}
		array = glowImages;
		foreach (RawImage rawImage4 in array)
		{
			if (spectateOverride)
			{
				rawImage4.color = new Color(1f, 0f, 0.18f, rawImage4.color.a);
			}
			else
			{
				rawImage4.color = new Color(1f, 1f, 1f, rawImage4.color.a);
			}
		}
	}
}
