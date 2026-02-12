using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoonUI : MonoBehaviour
{
	public enum State
	{
		None,
		Start,
		FadeIn,
		Show,
		Turn,
		Title,
		Attributes,
		Wait,
		Hide
	}

	public static MoonUI instance;

	internal bool debugNoWait;

	internal bool debugDisable;

	internal State state;

	private float stateTimer;

	private bool stateImpulse = true;

	public GameObject objectActive;

	[Space]
	public RawImage backgroundImage;

	public AnimationCurve backgroundCurve;

	private float backgroundLerp;

	[Space]
	public RectTransform showTransform;

	public AnimationCurve showCurve;

	public float showStartPosition;

	private float showLerp;

	[Space]
	public RectTransform turnTransform;

	public AnimationCurve turnCurve;

	public float turnStartPosition;

	private float turnLerp;

	[Space]
	public RectTransform turnBackTransform;

	public float turnBackStartPosition;

	[Space]
	public RectTransform turnPatternTransform;

	public float turnPatternStartPosition;

	[Space]
	public AnimationCurve pieceFadeCurve;

	private float pieceFadeLerp;

	[Space]
	public RectTransform titleTransform;

	public AnimationCurve titleCurve;

	private float titleLerp;

	[Space]
	public AnimationCurve hideCurve;

	public float hidePosition;

	private float hideLerp;

	private Vector3 hideStartPosition;

	[Space]
	public RectTransform shakeTransform;

	private float shakeTargetY;

	private float shakeTargetX;

	private float shakeTimer;

	private float shakeAmount;

	private float shakeAmountDecrease;

	private bool shakeImpulse;

	[Space]
	public RectTransform topCornerTransform;

	public RectTransform topCornerShakeTransform;

	public float topCornerStartPosition;

	public RawImage topCornerGraphicRight;

	public RawImage topCornerGraphicLeft;

	public RectTransform topCornerTransformRight;

	public RectTransform topCornerTransformLeft;

	[Space]
	public RectTransform botCornerTransform;

	public RectTransform botCornerShakeTransform;

	public float botCornerStartPosition;

	public RawImage botCornerGraphicRight;

	public RawImage botCornerGraphicLeft;

	public RectTransform botCornerTransformRight;

	public RectTransform botCornerTransformLeft;

	[Space]
	public RawImage moonGraphicPreviousest;

	public RawImage moonGraphicPrevious;

	public RawImage moonGraphicCurrent;

	public RawImage moonGraphicNext;

	[Space]
	public Color pieceFadeDefault;

	[Space]
	public TextMeshProUGUI textTitle;

	public GameObject attributesParent;

	public GameObject attributesPrefab;

	private List<string> attributes = new List<string>();

	private int attributesIndex;

	[Space]
	public RectTransform skipTransform;

	public TextMeshProUGUI skipText;

	public float skipStartPosition;

	private bool skip;

	[Space]
	public Sound soundShow;

	public Sound soundTurnStart;

	public Sound soundTurning;

	private bool soundTurningImpulse = true;

	public Sound soundTurnEnd;

	public Sound soundTitle;

	public Sound soundAttribute;

	public Sound soundHide;

	private bool tutorialImpulse = true;

	private void Awake()
	{
		instance = this;
		objectActive.SetActive(value: false);
	}

	private void Update()
	{
		switch (state)
		{
		case State.None:
			StateNone();
			break;
		case State.Start:
			StateStart();
			break;
		case State.FadeIn:
			StateFadeIn();
			break;
		case State.Show:
			StateShow();
			break;
		case State.Turn:
			StateTurn();
			break;
		case State.Title:
			StateTitle();
			break;
		case State.Attributes:
			StateAttributes();
			break;
		case State.Wait:
			StateWait();
			break;
		case State.Hide:
			StateHide();
			break;
		}
		if (state != State.None)
		{
			BackgroundLogic();
			MoonRotateLogic();
			TurnLogic();
			ShakeLogic();
			RotateLogic();
			SkipLogic();
		}
	}

	private void StateNone()
	{
		if (!stateImpulse)
		{
			return;
		}
		objectActive.SetActive(value: false);
		backgroundImage.color = new Color(0f, 0f, 0f, 0f);
		backgroundLerp = 0f;
		showTransform.anchoredPosition = new Vector3(0f, showStartPosition, 0f);
		showLerp = 0f;
		topCornerTransform.anchoredPosition = new Vector3(0f, topCornerStartPosition, 0f);
		topCornerGraphicRight.color = new Color(topCornerGraphicRight.color.r, topCornerGraphicRight.color.g, topCornerGraphicRight.color.b, 0f);
		topCornerGraphicLeft.color = topCornerGraphicRight.color;
		topCornerTransformRight.rotation = Quaternion.identity;
		topCornerTransformLeft.rotation = Quaternion.identity;
		botCornerTransform.anchoredPosition = new Vector3(0f, botCornerStartPosition, 0f);
		botCornerGraphicRight.color = topCornerGraphicLeft.color;
		botCornerGraphicLeft.color = topCornerGraphicLeft.color;
		botCornerTransformRight.rotation = Quaternion.identity;
		botCornerTransformLeft.rotation = Quaternion.identity;
		turnTransform.localEulerAngles = new Vector3(0f, 0f, turnStartPosition);
		turnBackTransform.localEulerAngles = new Vector3(0f, 0f, turnBackStartPosition);
		turnPatternTransform.localEulerAngles = new Vector3(0f, 0f, turnPatternStartPosition);
		turnLerp = 0f;
		soundTurningImpulse = true;
		moonGraphicPreviousest.color = pieceFadeDefault;
		moonGraphicPrevious.color = new Color(1f, 1f, 1f, 1f);
		moonGraphicCurrent.color = pieceFadeDefault;
		moonGraphicNext.color = pieceFadeDefault;
		pieceFadeLerp = 0f;
		titleTransform.gameObject.SetActive(value: false);
		titleLerp = 0f;
		attributesIndex = 0;
		foreach (Transform item in attributesParent.transform)
		{
			Object.Destroy(item.gameObject);
		}
		hideLerp = 0f;
		skip = false;
		skipTransform.anchoredPosition = new Vector3(0f, skipStartPosition, 0f);
		tutorialImpulse = true;
		stateImpulse = false;
	}

	private void StateStart()
	{
		if (stateImpulse)
		{
			stateTimer = 1f;
			if (debugNoWait)
			{
				stateTimer = 0.1f;
			}
			objectActive.SetActive(value: true);
			skipText.text = InputManager.instance.InputDisplayReplaceTags("[menu] <color=white>to skip</color>");
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			SetState(State.FadeIn);
		}
	}

	private void StateFadeIn()
	{
		if (stateImpulse)
		{
			stateTimer = 0.5f;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			SetState(State.Show);
		}
	}

	private void StateShow()
	{
		if (stateImpulse)
		{
			soundShow.Play(Vector3.zero);
			stateTimer = 1f;
			shakeAmount = 1f;
			shakeAmountDecrease = 0.1f;
			stateImpulse = false;
		}
		if (showLerp < 1f)
		{
			showLerp += Time.deltaTime * 1.5f;
			showTransform.anchoredPosition = Vector3.LerpUnclamped(new Vector3(0f, showStartPosition, 0f), Vector3.zero, showCurve.Evaluate(showLerp));
			skipTransform.anchoredPosition = Vector3.LerpUnclamped(new Vector3(0f, skipStartPosition, 0f), Vector3.zero, showCurve.Evaluate(showLerp));
			topCornerTransform.anchoredPosition = Vector3.LerpUnclamped(new Vector3(0f, topCornerStartPosition, 0f), Vector3.zero, showCurve.Evaluate(showLerp));
			botCornerTransform.anchoredPosition = Vector3.LerpUnclamped(new Vector3(0f, botCornerStartPosition, 0f), Vector3.zero, showCurve.Evaluate(showLerp));
			topCornerGraphicRight.color = Color.Lerp(new Color(topCornerGraphicRight.color.r, topCornerGraphicRight.color.g, topCornerGraphicRight.color.b, 0f), new Color(topCornerGraphicRight.color.r, topCornerGraphicRight.color.g, topCornerGraphicRight.color.b, 1f), showCurve.Evaluate(showLerp));
			topCornerGraphicLeft.color = topCornerGraphicRight.color;
			botCornerGraphicRight.color = topCornerGraphicLeft.color;
			botCornerGraphicLeft.color = topCornerGraphicLeft.color;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			SetState(State.Turn);
		}
	}

	private void StateTurn()
	{
		if (stateImpulse)
		{
			stateTimer = 2.5f;
			soundTurnStart.Play(Vector3.zero);
			shakeAmount = 2f;
			shakeAmountDecrease = 0.5f;
			shakeImpulse = true;
			stateImpulse = false;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			SetState(State.Title);
		}
	}

	private void StateTitle()
	{
		if (stateImpulse)
		{
			titleTransform.gameObject.SetActive(value: true);
			soundTitle.Play(Vector3.zero);
			shakeAmount = 1f;
			shakeAmountDecrease = 0.2f;
			stateTimer = 1f;
			stateImpulse = false;
		}
		if (titleLerp < 1f)
		{
			titleLerp += Time.deltaTime * 5f;
			titleTransform.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0f, -10f), Vector2.zero, titleCurve.Evaluate(titleLerp));
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			SetState(State.Attributes);
		}
	}

	private void StateAttributes()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 1f;
			if (attributesIndex >= attributes.Count)
			{
				SetState(State.Wait);
				return;
			}
			soundAttribute.Play(Vector3.zero);
			MoonAttributeUI component = Object.Instantiate(attributesPrefab, attributesParent.transform).GetComponent<MoonAttributeUI>();
			component.text.text = attributes[attributesIndex];
			component.rect.anchoredPosition = new Vector2(0f, -18f * (float)attributesIndex);
			attributesIndex++;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			stateImpulse = true;
		}
	}

	private void StateWait()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 4f;
		}
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			SetState(State.Hide);
		}
	}

	private void StateHide()
	{
		if (stateImpulse)
		{
			tutorialImpulse = true;
			stateImpulse = false;
			stateTimer = 4f;
			if (skip)
			{
				soundHide.Play(Vector3.zero).pitch *= 2f;
			}
			else
			{
				soundHide.Play(Vector3.zero);
			}
			shakeAmount = 1.5f;
			shakeAmountDecrease = 0.1f;
			hideStartPosition = showTransform.anchoredPosition;
		}
		if (hideLerp <= 1f)
		{
			float num = 1f;
			if (skip)
			{
				num = 5f;
			}
			hideLerp += num * Time.deltaTime;
			showTransform.anchoredPosition = Vector3.LerpUnclamped(hideStartPosition, new Vector3(0f, hidePosition, 0f), hideCurve.Evaluate(hideLerp));
			skipTransform.anchoredPosition = Vector3.LerpUnclamped(Vector3.zero, new Vector3(0f, skipStartPosition, 0f), hideCurve.Evaluate(hideLerp));
			topCornerTransform.anchoredPosition = Vector3.LerpUnclamped(Vector3.zero, new Vector3(0f, topCornerStartPosition, 0f), hideCurve.Evaluate(hideLerp));
			botCornerTransform.anchoredPosition = Vector3.LerpUnclamped(Vector3.zero, new Vector3(0f, botCornerStartPosition, 0f), hideCurve.Evaluate(hideLerp));
			topCornerGraphicRight.color = Color.Lerp(new Color(topCornerGraphicRight.color.r, topCornerGraphicRight.color.g, topCornerGraphicRight.color.b, 1f), new Color(topCornerGraphicRight.color.r, topCornerGraphicRight.color.g, topCornerGraphicRight.color.b, 0f), hideCurve.Evaluate(hideLerp));
			topCornerGraphicLeft.color = topCornerGraphicRight.color;
			botCornerGraphicRight.color = topCornerGraphicLeft.color;
			botCornerGraphicLeft.color = topCornerGraphicLeft.color;
		}
		stateTimer -= Time.deltaTime;
		if (tutorialImpulse && hideLerp > 0.5f)
		{
			Tutorials();
			tutorialImpulse = false;
		}
		if (stateTimer <= 0f)
		{
			SetState(State.None);
		}
	}

	public void Check()
	{
		if (!SemiFunc.RunIsLevel() || !RunManager.instance.moonLevelChanged)
		{
			return;
		}
		if (!debugDisable && RunManager.instance.moonLevel > 0 && RunManager.instance.moonLevel <= RunManager.instance.moons.Count)
		{
			moonGraphicPreviousest.texture = RunManager.instance.MoonGetIcon(RunManager.instance.moonLevel - 2);
			if (!moonGraphicPreviousest.texture)
			{
				moonGraphicPreviousest.color = Color.clear;
			}
			moonGraphicPrevious.texture = RunManager.instance.MoonGetIcon(RunManager.instance.moonLevel - 1);
			if (!moonGraphicPrevious.texture)
			{
				moonGraphicPrevious.color = Color.clear;
			}
			moonGraphicCurrent.texture = RunManager.instance.MoonGetIcon(RunManager.instance.moonLevel);
			if (!moonGraphicCurrent.texture)
			{
				moonGraphicCurrent.color = Color.clear;
			}
			moonGraphicNext.texture = RunManager.instance.MoonGetIcon(RunManager.instance.moonLevel + 1);
			if (!moonGraphicNext.texture)
			{
				moonGraphicNext.color = Color.clear;
			}
			textTitle.text = RunManager.instance.MoonGetName(RunManager.instance.moonLevel);
			attributes = RunManager.instance.MoonGetAttributes(RunManager.instance.moonLevel);
			SetState(State.Start);
		}
		RunManager.instance.moonLevelChanged = false;
	}

	public void SetState(State _state)
	{
		if (state != _state)
		{
			state = _state;
			stateImpulse = true;
		}
	}

	private void BackgroundLogic()
	{
		if (state >= State.Hide)
		{
			if (backgroundLerp > 0f)
			{
				float num = 0.5f;
				if (skip)
				{
					num = 3f;
				}
				backgroundLerp -= num * Time.deltaTime;
				backgroundImage.color = Color.Lerp(new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 0.85f), backgroundCurve.Evaluate(backgroundLerp));
			}
		}
		else if (state >= State.FadeIn && backgroundLerp < 1f)
		{
			backgroundLerp += Time.deltaTime * 1f;
			backgroundImage.color = Color.Lerp(new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 0.85f), backgroundCurve.Evaluate(backgroundLerp));
		}
	}

	private void MoonRotateLogic()
	{
		moonGraphicPreviousest.transform.rotation = Quaternion.identity;
		moonGraphicPrevious.transform.rotation = Quaternion.identity;
		moonGraphicCurrent.transform.rotation = Quaternion.identity;
		moonGraphicNext.transform.rotation = Quaternion.identity;
	}

	private void TurnLogic()
	{
		if (state < State.Turn)
		{
			return;
		}
		if (turnLerp < 1f)
		{
			turnLerp += Time.deltaTime * 0.3f;
			turnTransform.localEulerAngles = Vector3.LerpUnclamped(new Vector3(0f, 0f, turnStartPosition), Vector3.zero, turnCurve.Evaluate(turnLerp));
			turnBackTransform.localEulerAngles = Vector3.LerpUnclamped(new Vector3(0f, 0f, turnBackStartPosition), Vector3.zero, turnCurve.Evaluate(turnLerp));
			turnPatternTransform.localEulerAngles = Vector3.LerpUnclamped(new Vector3(0f, 0f, turnPatternStartPosition), Vector3.zero, turnCurve.Evaluate(turnLerp));
			if (shakeImpulse && turnLerp > 0.5f)
			{
				soundTurnEnd.Play(Vector3.zero);
				shakeImpulse = false;
				shakeAmount = 3f;
				shakeAmountDecrease = 0.5f;
			}
			if (soundTurningImpulse && turnLerp > 0.3f)
			{
				soundTurning.Play(Vector3.zero);
				soundTurningImpulse = false;
			}
		}
		if (pieceFadeLerp < 1f)
		{
			pieceFadeLerp += Time.deltaTime * 0.3f;
			if ((bool)moonGraphicPrevious.texture)
			{
				moonGraphicPrevious.color = Color.Lerp(new Color(1f, 1f, 1f, 1f), pieceFadeDefault, pieceFadeCurve.Evaluate(pieceFadeLerp));
			}
			if ((bool)moonGraphicCurrent.texture)
			{
				moonGraphicCurrent.color = Color.Lerp(pieceFadeDefault, new Color(1f, 1f, 1f, 1f), pieceFadeCurve.Evaluate(pieceFadeLerp));
			}
		}
	}

	private void ShakeLogic()
	{
		if (shakeAmount > 0f)
		{
			if (shakeTimer <= 0f)
			{
				shakeTimer = 0.05f;
				shakeTargetX = Random.Range(0f - shakeAmount, shakeAmount);
				shakeTargetY = Random.Range(0f - shakeAmount, shakeAmount);
				shakeAmount -= shakeAmountDecrease;
			}
			else
			{
				shakeTimer -= Time.deltaTime;
			}
			shakeTransform.anchoredPosition = Vector2.Lerp(shakeTransform.anchoredPosition, new Vector2(shakeTargetX, shakeTargetY), 30f * Time.deltaTime);
			topCornerShakeTransform.anchoredPosition = Vector2.Lerp(shakeTransform.anchoredPosition, new Vector2(shakeTargetX, shakeTargetY) * 0.25f, 30f * Time.deltaTime);
			botCornerShakeTransform.anchoredPosition = Vector2.Lerp(shakeTransform.anchoredPosition, new Vector2(shakeTargetX, shakeTargetY) * 0.25f, 30f * Time.deltaTime);
		}
	}

	private void RotateLogic()
	{
		topCornerTransformRight.Rotate(0f, 0f, Time.deltaTime * 5f);
		topCornerTransformLeft.Rotate(0f, 0f, (0f - Time.deltaTime) * 5f);
		botCornerTransformRight.Rotate(0f, 0f, Time.deltaTime * 5f);
		botCornerTransformLeft.Rotate(0f, 0f, (0f - Time.deltaTime) * 5f);
	}

	private void SkipLogic()
	{
		if (state != State.Hide && state != State.None)
		{
			if (SemiFunc.InputDown(InputKey.Menu))
			{
				skip = true;
				SetState(State.Hide);
			}
			if (state != State.None)
			{
				GameDirector.instance.SetDisableEscMenu(1f);
			}
		}
	}

	private void Tutorials()
	{
		if (SemiFunc.MoonLevel() == 2 && TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialOvercharge1, 1))
		{
			TutorialDirector.instance.ActivateTip("Overcharge1", 0f, _interrupt: false);
			TutorialDirector.instance.ScheduleTip("Overcharge2", 8f, _interrupt: false);
		}
	}
}
