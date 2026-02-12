using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SemiUI : MonoBehaviour
{
	internal Vector3 initialPosition;

	private float shakeTimeX;

	private float shakeTimeY;

	private float shakeAmountX;

	private float shakeAmountY;

	private float shakeFrequencyX;

	private float shakeFrequencyY;

	private float shakeDurationX;

	private float shakeDurationY;

	public bool animateTheEntireObject;

	[HideInInspector]
	public TextMeshProUGUI uiText;

	private Color originalTextColor;

	private Color originalFontColor;

	private Color originalGlowColor;

	private Color flashColor;

	private float flashColorTime;

	private Material textMaterial;

	internal float hideTimer;

	internal float showTimer;

	private bool uiTextEnabledPrevious;

	private float scaleTime;

	private float scaleAmount;

	private float scaleFrequency;

	private float scaleDuration;

	private Vector3 originalScale;

	[HideInInspector]
	public Transform textRectTransform;

	private AnimationCurve animationCurveWooshAway;

	private AnimationCurve animationCurveWooshIn;

	private AnimationCurve animationCurveInOut;

	private float hideAnimationEvaluation;

	private float showAnimationEvaluation;

	public Vector2 hidePosition = new Vector2(0f, 0f);

	[HideInInspector]
	public Vector2 showPosition = new Vector2(0f, 0f);

	private Vector2 hidePositionCurrent = new Vector2(0f, 0f);

	private bool initialized;

	private Vector2 scootPosition = new Vector2(0f, 0f);

	private Vector2 scootPositionStart = new Vector2(0f, 0f);

	private float scootTimer = -123f;

	private Vector2 scootPositionPrev = new Vector2(0f, 0f);

	private bool scootHideImpulse = true;

	private float scootAnimationEvaluation;

	private Vector2 originalScootPosition = new Vector2(0f, 0f);

	private Vector2 scootPositionCurrent = new Vector2(0f, 0f);

	[HideInInspector]
	public bool isHidden;

	private List<GameObject> allChildren = new List<GameObject>();

	private float SpringShakeX;

	private float SpringShakeY;

	private float stopScootingTimer;

	private float stopHidingTimer;

	private float stopShowingTimer;

	private float prevShowTimer;

	private float prevHideTimer;

	private float prevScootTimer;

	private float animationEval;

	private float prevStopHidingTimer;

	private float prevStopShowingTimer;

	private float scootEval;

	public List<GameObject> doNotDisable;

	protected virtual void Start()
	{
		if (uiText == null)
		{
			uiText = GetComponent<TextMeshProUGUI>();
		}
		if (uiText == null)
		{
			uiText = GetComponentInChildren<TextMeshProUGUI>();
		}
		initialPosition = base.transform.localPosition;
		if ((bool)uiText)
		{
			originalTextColor = uiText.color;
		}
		if ((bool)uiText)
		{
			originalFontColor = uiText.fontMaterial.GetColor(ShaderUtilities.ID_FaceColor);
		}
		if ((bool)uiText)
		{
			uiTextEnabledPrevious = uiText.enabled;
		}
		if (!textRectTransform)
		{
			textRectTransform = GetComponent<RectTransform>();
		}
		originalScale = textRectTransform.localScale;
		if ((bool)uiText)
		{
			originalGlowColor = uiText.fontMaterial.GetColor(ShaderUtilities.ID_GlowColor);
		}
		if (!animateTheEntireObject)
		{
			if (showPosition == new Vector2(0f, 0f))
			{
				showPosition = textRectTransform.localPosition;
			}
		}
		else if (showPosition == new Vector2(0f, 0f))
		{
			showPosition = base.transform.localPosition;
		}
		hidePosition += showPosition;
		StartCoroutine(LateStart());
		if (!animateTheEntireObject)
		{
			textRectTransform.localPosition = hidePosition;
		}
		else
		{
			base.transform.localPosition = hidePosition;
		}
		hidePositionCurrent = hidePosition;
		hideAnimationEvaluation = 1f;
		if ((bool)uiText && !animateTheEntireObject)
		{
			uiText.enabled = false;
		}
		hideTimer = 0.2f;
		allChildren = new List<GameObject>();
		foreach (Transform item in base.transform)
		{
			allChildren.Add(item.gameObject);
		}
	}

	private void AllChildrenSetActive(bool active)
	{
		foreach (GameObject allChild in allChildren)
		{
			bool flag = false;
			foreach (GameObject item in doNotDisable)
			{
				if (allChild == item)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				allChild.SetActive(active);
			}
		}
	}

	private IEnumerator LateStart()
	{
		yield return new WaitForSeconds(0.2f);
		animationCurveWooshAway = AssetManager.instance.animationCurveWooshAway;
		animationCurveWooshIn = AssetManager.instance.animationCurveWooshIn;
		animationCurveInOut = AssetManager.instance.animationCurveInOut;
		initialized = true;
	}

	protected virtual void Update()
	{
		if (initialized)
		{
			float deltaTime = Time.deltaTime;
			if (scootTimer >= 0f)
			{
				scootTimer -= deltaTime;
			}
			FlashColorLogic(deltaTime);
			HideAnimationLogic(deltaTime);
			HideTimer(deltaTime);
			SpringScaleLogic(deltaTime);
			ScootPositionLogic(deltaTime);
			SpringShakeLogic(deltaTime);
			UpdatePositionLogic();
			prevShowTimer = showTimer;
			prevHideTimer = hideTimer;
			prevScootTimer = scootTimer;
			prevStopHidingTimer = stopHidingTimer;
			prevStopShowingTimer = stopShowingTimer;
			if (hideTimer >= 0f)
			{
				hideTimer -= deltaTime;
			}
			if (showTimer >= 0f)
			{
				showTimer -= deltaTime;
			}
			if (stopShowingTimer >= 0f)
			{
				stopShowingTimer -= deltaTime;
			}
			if (stopHidingTimer >= 0f)
			{
				stopHidingTimer -= deltaTime;
			}
			if (stopScootingTimer >= 0f)
			{
				stopScootingTimer -= deltaTime;
			}
		}
	}

	public void SemiUISpringScale(float amount, float frequency, float time)
	{
		scaleTime = 0f;
		scaleAmount = amount;
		scaleFrequency = frequency;
		scaleDuration = time;
	}

	private void ScootPositionLogic(float deltaTime)
	{
		if (scootTimer <= 0f && prevScootTimer <= 0f)
		{
			if (scootPositionCurrent != Vector2.zero)
			{
				if (scootHideImpulse)
				{
					scootPositionStart = scootPositionCurrent;
					scootHideImpulse = false;
				}
				if (scootEval >= 1f)
				{
					scootPositionCurrent = Vector2.zero;
					scootAnimationEvaluation = 0f;
					scootEval = 0f;
				}
				else
				{
					scootAnimationEvaluation += 4f * deltaTime;
					scootAnimationEvaluation = Mathf.Clamp01(scootAnimationEvaluation);
					scootEval = animationCurveInOut.Evaluate(scootAnimationEvaluation);
					scootPositionCurrent = Vector2.LerpUnclamped(scootPositionStart, Vector2.zero, scootEval);
				}
			}
			else
			{
				scootHideImpulse = true;
			}
		}
		else
		{
			scootHideImpulse = true;
		}
		if (!(scootTimer > 0f) || !(prevScootTimer > 0f))
		{
			return;
		}
		stopScootingTimer = 0.1f;
		if (scootPositionCurrent != scootPosition)
		{
			if (scootEval >= 1f)
			{
				scootPositionCurrent = scootPosition;
				scootAnimationEvaluation = 0f;
				scootEval = 0f;
			}
			else
			{
				scootAnimationEvaluation += 4f * deltaTime;
				scootAnimationEvaluation = Mathf.Clamp01(scootAnimationEvaluation);
				scootEval = animationCurveInOut.Evaluate(scootAnimationEvaluation);
				scootPositionCurrent = Vector2.LerpUnclamped(scootPositionStart, scootPosition, scootEval);
			}
		}
	}

	private void UpdatePositionLogic()
	{
		if (!animateTheEntireObject)
		{
			textRectTransform.localPosition = hidePositionCurrent + scootPositionCurrent + new Vector2(SpringShakeX, SpringShakeY);
		}
		else
		{
			base.transform.localPosition = hidePositionCurrent + scootPositionCurrent + new Vector2(SpringShakeX, SpringShakeY);
		}
	}

	private void SpringScaleLogic(float deltaTime)
	{
		if (scaleTime < scaleDuration)
		{
			float num = CalculateSpringOffset(scaleTime, scaleAmount, scaleFrequency, scaleDuration);
			Vector3 vector = originalScale * (1f + num);
			vector = new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
			if (!animateTheEntireObject)
			{
				textRectTransform.localScale = vector;
			}
			else
			{
				base.transform.localScale = vector;
			}
			scaleTime += deltaTime;
		}
		else if (!animateTheEntireObject)
		{
			textRectTransform.localScale = originalScale;
		}
		else
		{
			base.transform.localScale = originalScale;
		}
	}

	private void HideAnimationLogic(float deltaTime)
	{
		if (hideTimer <= 0f && prevHideTimer <= 0f)
		{
			if (showTimer <= 0f && prevShowTimer <= 0f)
			{
				animationEval = 0f;
				showAnimationEvaluation = 0f;
				hideAnimationEvaluation = 0f;
			}
			showTimer = 0.1f;
		}
		if (showTimer > 0f && prevShowTimer > 0f)
		{
			stopShowingTimer = 0.1f;
			if (hidePositionCurrent != showPosition)
			{
				hidePositionCurrent = Vector2.LerpUnclamped(hidePosition + scootPositionCurrent, showPosition, animationEval);
				if (showAnimationEvaluation >= 1f)
				{
					hidePositionCurrent = showPosition;
					showAnimationEvaluation = 0f;
					animationEval = 0f;
				}
				else
				{
					showAnimationEvaluation += 4f * deltaTime;
					showAnimationEvaluation = Mathf.Clamp01(showAnimationEvaluation);
					animationEval = animationCurveWooshIn.Evaluate(showAnimationEvaluation);
				}
			}
		}
		if (!(hideTimer > 0f) || !(prevHideTimer > 0f) || !(showTimer <= 0f) || !(prevShowTimer <= 0f))
		{
			return;
		}
		stopHidingTimer = 0.1f;
		if (hidePositionCurrent != hidePosition)
		{
			hidePositionCurrent = Vector2.LerpUnclamped(showPosition, hidePosition, animationEval);
			if (hideAnimationEvaluation >= 1f)
			{
				hidePositionCurrent = hidePosition;
				hideAnimationEvaluation = 0f;
				animationEval = 0f;
			}
			else
			{
				hideAnimationEvaluation += 4f * deltaTime;
				hideAnimationEvaluation = Mathf.Clamp01(hideAnimationEvaluation);
				animationEval = animationCurveWooshAway.Evaluate(hideAnimationEvaluation);
			}
		}
	}

	private void HideTimer(float deltaTime)
	{
		if (showTimer > 0f && prevShowTimer > 0f && hideTimer <= 0f && prevHideTimer <= 0f)
		{
			if (!animateTheEntireObject)
			{
				if ((bool)uiText && !uiText.enabled)
				{
					uiText.enabled = true;
					isHidden = false;
					AllChildrenSetActive(active: true);
				}
			}
			else
			{
				isHidden = false;
				AllChildrenSetActive(active: true);
			}
			hideTimer = 0f;
			return;
		}
		if (hideTimer <= 0f && prevHideTimer <= 0f && stopHidingTimer <= 0f && prevStopHidingTimer <= 0f && hideAnimationEvaluation == 0f)
		{
			if (!animateTheEntireObject)
			{
				if ((bool)uiText && !uiText.enabled)
				{
					uiText.enabled = true;
					isHidden = false;
					AllChildrenSetActive(active: true);
				}
			}
			else
			{
				isHidden = true;
				AllChildrenSetActive(active: true);
			}
		}
		if (!(hideTimer > 0f) || !(hideAnimationEvaluation >= 1f))
		{
			return;
		}
		if (!animateTheEntireObject)
		{
			if ((bool)uiText && uiText.enabled)
			{
				uiText.enabled = false;
				AllChildrenSetActive(active: false);
				isHidden = true;
			}
		}
		else
		{
			AllChildrenSetActive(active: false);
			isHidden = true;
		}
	}

	public void SemiUIResetAllShakeEffects()
	{
		shakeTimeX = 0f;
		shakeTimeY = 0f;
		shakeAmountX = 0f;
		shakeAmountY = 0f;
		shakeFrequencyX = 0f;
		shakeFrequencyY = 0f;
		shakeDurationX = 0f;
		shakeDurationY = 0f;
		SpringShakeX = 0f;
		SpringShakeY = 0f;
	}

	private void FlashColorLogic(float deltaTime)
	{
		if ((bool)uiText && flashColorTime > 0f)
		{
			flashColorTime -= deltaTime;
			uiText.color = flashColor;
			uiText.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, flashColor);
			uiText.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, flashColor);
			if (flashColorTime <= 0f)
			{
				uiText.color = originalTextColor;
				uiText.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, originalFontColor);
				uiText.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, originalGlowColor);
			}
		}
	}

	public void SemiUISpringShakeY(float amount, float frequency, float time)
	{
		shakeTimeY = 0f;
		shakeAmountY = amount;
		shakeFrequencyY = frequency;
		shakeDurationY = time;
	}

	public void SemiUISpringShakeX(float amount, float frequency, float time)
	{
		shakeTimeX = 0f;
		shakeAmountX = amount;
		shakeFrequencyX = frequency;
		shakeDurationX = time;
	}

	public void SemiUITextFlashColor(Color color, float time)
	{
		flashColor = color;
		flashColorTime = time;
	}

	private void SpringShakeLogic(float deltaTime)
	{
		float x = 0f;
		float y = 0f;
		if (shakeTimeX < shakeDurationX)
		{
			x = (SpringShakeX = CalculateSpringOffset(shakeTimeX, shakeAmountX, shakeFrequencyX, shakeDurationX));
			shakeTimeX += deltaTime;
		}
		if (shakeTimeY < shakeDurationY)
		{
			y = (SpringShakeY = CalculateSpringOffset(shakeTimeY, shakeAmountY, shakeFrequencyY, shakeDurationY));
			shakeTimeY += deltaTime;
		}
		base.transform.localPosition = initialPosition + new Vector3(x, y, 0f);
	}

	private float CalculateSpringOffset(float currentTime, float amount, float frequency, float duration)
	{
		float num = currentTime / duration;
		float num2 = frequency * (1f - num);
		return amount * Mathf.Sin(num2 * num * MathF.PI * 2f) * (1f - num);
	}

	public void Hide()
	{
		if (hideTimer <= 0f && prevHideTimer <= 0f)
		{
			hideAnimationEvaluation = 0f;
			showAnimationEvaluation = 0f;
			animationEval = 0f;
			if (!animateTheEntireObject && (bool)uiText && !uiText.enabled)
			{
				uiText.enabled = false;
				AllChildrenSetActive(active: false);
				isHidden = true;
			}
			hidePositionCurrent = showPosition;
		}
		hideTimer = 0.1f;
	}

	public void Show()
	{
		if (showTimer <= 0f && prevShowTimer <= 0f)
		{
			showAnimationEvaluation = 0f;
			hideAnimationEvaluation = 0f;
			animationEval = 0f;
			if (!animateTheEntireObject)
			{
				if ((bool)uiText && !uiText.enabled)
				{
					uiText.enabled = true;
					AllChildrenSetActive(active: true);
					isHidden = false;
				}
			}
			else
			{
				AllChildrenSetActive(active: true);
				isHidden = false;
			}
			hidePositionCurrent = hidePosition;
		}
		showTimer = 0.1f;
	}

	public void SemiUIScoot(Vector2 position)
	{
		scootPosition = position;
		if ((scootTimer <= 0f && prevScootTimer <= 0f) || scootPositionPrev != scootPosition)
		{
			scootEval = 0f;
			scootAnimationEvaluation = 0f;
			scootPositionStart = scootPositionCurrent;
			scootPositionPrev = scootPosition;
		}
		scootTimer = 0.2f;
	}
}
