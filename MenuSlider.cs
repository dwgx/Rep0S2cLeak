using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MenuSlider : MonoBehaviour
{
	[Serializable]
	public class CustomOption
	{
		[Space(25f)]
		[Header("____ Custom Option ____")]
		public string customOptionText;

		public UnityEvent onOption;

		public int customValueInt;
	}

	public string elementName = "Element Name";

	public TextMeshProUGUI elementNameText;

	public Transform sliderBG;

	public Transform barSize;

	public Transform barPointer;

	public RectTransform barSizeRectTransform;

	public Transform settingsBar;

	public Transform extraBar;

	private int settingSegments;

	public int startValue;

	public int endValue;

	public string stringAtStartOfValue;

	public string stringAtEndOfValue;

	internal int currentValue;

	internal int prevCurrentValue;

	internal bool valueChangedImpulse;

	public int buttonSegmentJump = 1;

	public int pointerSegmentJump = 1;

	internal float settingsValue = 1f;

	internal float prevSettingsValue = 1f;

	public TextMeshProUGUI segmentText;

	public TextMeshProUGUI segmentMaskText;

	public RectTransform maskRectTransform;

	public bool wrapAround;

	public bool hasBar = true;

	public bool hasCustomOptions;

	private MenuSelectableElement menuSelectableElement;

	private bool hovering;

	private RectTransform rectTransform;

	private float sneakyOffsetBecauseIWasLazy = 3f;

	private MenuPage parentPage;

	internal MenuSetting menuSetting;

	private DataDirector.Setting setting;

	private bool inputSetting;

	private MenuInputPercentSetting inputPercentSetting;

	private Vector3 originalPosition;

	private Vector3 originalPositionBarSize;

	private Vector3 originalPositionBarBG;

	private RectTransform barBGRectTransform;

	private string prevSettingString = "";

	private bool hasBigSettingText;

	internal MenuBigSettingText bigSettingText;

	private int customValue;

	private int customValueNull = -123456;

	public bool hasCustomValues;

	private bool startPositionSetup;

	[Space]
	public UnityEvent onChange;

	public List<CustomOption> customOptions;

	private float extraBarActiveTimer;

	public void Start()
	{
		inputPercentSetting = GetComponent<MenuInputPercentSetting>();
		rectTransform = GetComponent<RectTransform>();
		if ((bool)inputPercentSetting)
		{
			inputSetting = true;
		}
		menuSetting = GetComponent<MenuSetting>();
		if ((bool)menuSetting)
		{
			menuSetting.FetchValues();
			int settingValue = menuSetting.settingValue;
			if (hasCustomOptions)
			{
				int indexFromCustomValue = GetIndexFromCustomValue(menuSetting.settingValue);
				menuSetting.settingValue = indexFromCustomValue;
				settingValue = menuSetting.settingValue;
			}
			settingsValue = (float)settingValue / 100f;
			setting = menuSetting.setting;
			elementName = menuSetting.settingName;
			elementNameText.text = elementName;
		}
		bigSettingText = GetComponentInChildren<MenuBigSettingText>();
		if ((bool)bigSettingText)
		{
			hasBigSettingText = true;
		}
		prevSettingString = "";
		parentPage = GetComponentInParent<MenuPage>();
		if ((bool)elementNameText)
		{
			elementNameText.text = elementName;
		}
		settingSegments = endValue - startValue;
		menuSelectableElement = GetComponent<MenuSelectableElement>();
		if (hasCustomOptions)
		{
			settingSegments = Mathf.Max(customOptions.Count - 1, 1);
			startValue = 0;
			endValue = customOptions.Count - 1;
			buttonSegmentJump = 1;
			settingsValue = settingsValue / (float)settingSegments * 100f;
		}
		barSizeRectTransform = barSize.GetComponent<RectTransform>();
		if (hasCustomOptions)
		{
			if (Mathf.Max(customOptions.Count - 1, 1) != settingSegments)
			{
				Debug.LogWarning("Segment text count is not equal to setting segments count");
			}
			else
			{
				int index = Mathf.RoundToInt(settingsValue * (float)settingSegments);
				string text = customOptions[index].customOptionText;
				if (text.Length > 16)
				{
					text = text.Substring(0, 16) + "...";
				}
				segmentText.text = text;
			}
		}
		else
		{
			currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, settingsValue));
			segmentText.text = stringAtStartOfValue + currentValue + stringAtEndOfValue;
		}
		segmentText.enableAutoSizing = false;
		segmentMaskText.enableAutoSizing = false;
		if (!hasBar && (bool)segmentText)
		{
			UnityEngine.Object.Destroy(segmentText.gameObject);
		}
		SetStartPositions();
	}

	public int GetIndexFromCustomValue(int value)
	{
		int result = 0;
		for (int i = 0; i < customOptions.Count; i++)
		{
			if (customOptions[i].customValueInt == value)
			{
				return i;
			}
		}
		return result;
	}

	private void OnValidate()
	{
		if (!SemiFunc.OnValidateCheck())
		{
			elementNameText = GetComponentInChildren<TextMeshProUGUI>();
			elementNameText.text = elementName;
			base.gameObject.name = "Slider - " + elementName;
		}
	}

	public void SetStartPositions()
	{
		if (!startPositionSetup)
		{
			startPositionSetup = true;
			barSizeRectTransform.localPosition = new Vector3(barSizeRectTransform.localPosition.x + sneakyOffsetBecauseIWasLazy, barSizeRectTransform.localPosition.y, barSizeRectTransform.localPosition.z);
			originalPosition = rectTransform.position;
			originalPositionBarBG = sliderBG.GetComponent<RectTransform>().position;
			originalPositionBarSize = barSizeRectTransform.transform.position;
			originalPosition = new Vector3(originalPosition.x, originalPosition.y - 1.01f, originalPosition.z);
			barBGRectTransform = sliderBG.GetComponent<RectTransform>();
		}
	}

	public string CustomOptionGetCurrentString()
	{
		return customOptions[currentValue].customOptionText;
	}

	public void CustomOptionAdd(string optionText, UnityEvent onOption)
	{
		customOptions.Add(new CustomOption
		{
			customOptionText = optionText,
			onOption = onOption
		});
		settingSegments = Mathf.Max(customOptions.Count - 1, 1);
		startValue = 0;
		endValue = customOptions.Count - 1;
		buttonSegmentJump = 1;
	}

	private void Update()
	{
		if (hasBigSettingText && prevSettingString != segmentText.text)
		{
			int index = Mathf.RoundToInt(settingsValue * (float)settingSegments);
			bigSettingText.textMeshPro.text = customOptions[index].customOptionText;
			prevSettingString = segmentText.text;
		}
		if (prevCurrentValue != currentValue || valueChangedImpulse)
		{
			valueChangedImpulse = false;
			float num = Mathf.Round(settingsValue * 100f);
			if (customOptions.Count > 0)
			{
				num = Mathf.RoundToInt(settingsValue * (float)settingSegments);
			}
			if ((bool)menuSetting)
			{
				if (!hasCustomOptions)
				{
					DataDirector.instance.SettingValueSet(setting, (int)num);
				}
				else if (hasCustomValues)
				{
					CustomOption customOption = customOptions[currentValue];
					DataDirector.instance.SettingValueSet(setting, customOption.customValueInt);
					customOptions[currentValue].onOption.Invoke();
				}
				else
				{
					DataDirector.instance.SettingValueSet(setting, (int)num);
				}
			}
			if (inputSetting)
			{
				InputManager.instance.inputPercentSettings[inputPercentSetting.setting] = (int)num;
			}
			onChange.Invoke();
			prevCurrentValue = currentValue;
		}
		if (extraBarActiveTimer > 0f)
		{
			extraBarActiveTimer -= Time.deltaTime;
		}
		else if (extraBar.gameObject.activeSelf)
		{
			extraBar.gameObject.SetActive(value: false);
		}
		if (hasBar)
		{
			settingsBar.localScale = Vector3.Lerp(settingsBar.localScale, new Vector3(settingsValue, settingsBar.localScale.y, settingsBar.localScale.z), 20f * Time.deltaTime);
			maskRectTransform.sizeDelta = new Vector2(barSizeRectTransform.sizeDelta.x * settingsValue, maskRectTransform.sizeDelta.y);
		}
		Vector3 mousePosition = Input.mousePosition;
		float num2 = (float)(Screen.width / MenuManager.instance.screenUIWidth) * 1.05f;
		float num3 = (float)(Screen.height / MenuManager.instance.screenUIHeight) * 1f;
		mousePosition = new Vector3(mousePosition.x / num2, mousePosition.y / num3, 0f);
		if (SemiFunc.UIMouseHover(parentPage, barSizeRectTransform, menuSelectableElement.menuID, 5f, 5f))
		{
			if (!hovering)
			{
				MenuManager.instance.MenuEffectHover(SemiFunc.MenuGetPitchFromYPos(rectTransform));
			}
			hovering = true;
			int num4 = 10;
			new Vector3(barSizeRectTransform.localPosition.x + barSizeRectTransform.sizeDelta.x / 2f - sneakyOffsetBecauseIWasLazy, barSizeRectTransform.localPosition.y + (float)(num4 / 2), barSizeRectTransform.localPosition.z);
			new Vector2(barSizeRectTransform.sizeDelta.x + (float)num4, barSizeRectTransform.sizeDelta.y + (float)num4);
			SemiFunc.MenuSelectionBoxTargetSet(parentPage, barSizeRectTransform, new Vector2(-3f, 0f), new Vector2(20f, 10f));
			if (hasBar)
			{
				PointerLogic(mousePosition);
			}
			else if (Input.GetMouseButtonDown(0))
			{
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Action, parentPage);
				OnIncrease();
			}
		}
		else
		{
			hovering = false;
			if (barPointer.gameObject.activeSelf)
			{
				barPointer.localPosition = new Vector3(-999f, barPointer.localPosition.y, barPointer.localPosition.z);
				barPointer.gameObject.SetActive(value: false);
			}
		}
		if ((bool)segmentMaskText && segmentMaskText.text != segmentText.text)
		{
			segmentMaskText.text = segmentText.text;
		}
	}

	public void ExtraBarSet(float value)
	{
		if (!extraBar.gameObject.activeSelf)
		{
			extraBar.gameObject.SetActive(value: true);
		}
		value = Mathf.Clamp(value, 0f, 1f);
		extraBar.localScale = new Vector3(value, extraBar.localScale.y, extraBar.localScale.z);
		extraBarActiveTimer = 0.2f;
	}

	public void SetBar(float value)
	{
		settingsValue = Mathf.Clamp(value, 0f, 1f);
		int num = Mathf.RoundToInt(settingsValue * (float)settingSegments);
		currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, settingsValue));
		if (hasCustomOptions)
		{
			customValue = GetCustomValue(num);
			if (num < customOptions.Count)
			{
				string text = customOptions[num].customOptionText;
				if (text.Length > 16)
				{
					text = text.Substring(0, 16) + "...";
				}
				segmentText.text = text;
			}
		}
		else
		{
			segmentText.text = stringAtStartOfValue + currentValue + stringAtEndOfValue;
		}
	}

	public int GetCustomValue(int index)
	{
		if (!hasCustomOptions)
		{
			return customValueNull;
		}
		if (customOptions.Count == 0)
		{
			return customValueNull;
		}
		if (index >= customOptions.Count)
		{
			return customValueNull;
		}
		if (index < 0)
		{
			return customValueNull;
		}
		if (!hasCustomValues)
		{
			return customValueNull;
		}
		return customOptions[index].customValueInt;
	}

	private void PointerLogic(Vector3 mouseScreenPosition)
	{
		if (!barPointer)
		{
			return;
		}
		if (!barPointer.gameObject.activeSelf)
		{
			barPointer.gameObject.SetActive(value: true);
		}
		Vector2 vector = SemiFunc.UIMouseGetLocalPositionWithinRectTransform(barSizeRectTransform);
		int num = (endValue - startValue) / pointerSegmentJump;
		SemiFunc.UIGetRectTransformPositionOnScreen(barSizeRectTransform);
		float num2 = Mathf.Clamp01(vector.x / barSizeRectTransform.sizeDelta.x);
		num2 = Mathf.Round(num2 * (float)num) / (float)num;
		float num3 = Mathf.Clamp(barSizeRectTransform.localPosition.x + num2 * barSizeRectTransform.sizeDelta.x, barSizeRectTransform.localPosition.x, barSizeRectTransform.localPosition.x + barSizeRectTransform.sizeDelta.x);
		barPointer.localPosition = new Vector3(num3 - 2f, barPointer.localPosition.y, barPointer.localPosition.z);
		if (Input.GetMouseButton(0))
		{
			prevSettingsValue = settingsValue;
			settingsValue = num2;
			if (prevSettingsValue != settingsValue)
			{
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, parentPage);
			}
			int num4 = Mathf.RoundToInt(settingsValue * (float)num);
			if (hasCustomOptions && num4 < customOptions.Count)
			{
				segmentText.text = customOptions[num4].customOptionText;
			}
			else
			{
				segmentText.text = stringAtStartOfValue + currentValue + stringAtEndOfValue;
			}
			currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, settingsValue));
			if (hasCustomOptions)
			{
				UpdateSegmentTextAndValue();
				customValue = GetCustomValue(num4);
			}
		}
	}

	public void UpdateSegmentTextAndValue()
	{
		int num = Mathf.RoundToInt(settingsValue * (float)settingSegments);
		currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, settingsValue));
		if (hasCustomOptions)
		{
			customValue = GetCustomValue(num);
			if (num < customOptions.Count)
			{
				string text = customOptions[num].customOptionText;
				if (text.Length > 16)
				{
					text = text.Substring(0, 16) + "...";
				}
				segmentText.text = text;
			}
		}
		else
		{
			segmentText.text = stringAtStartOfValue + currentValue + stringAtEndOfValue;
		}
	}

	public void OnIncrease()
	{
		valueChangedImpulse = true;
		prevSettingsValue = settingsValue;
		float num = settingsValue;
		settingsValue += 1f / (float)settingSegments * (float)buttonSegmentJump;
		if (wrapAround)
		{
			settingsValue = ((num == 1f) ? 0f : Mathf.Clamp01(settingsValue));
		}
		else
		{
			settingsValue = Mathf.Clamp(settingsValue, 0f, 1f);
		}
		UpdateSegmentTextAndValue();
	}

	public void OnDecrease()
	{
		valueChangedImpulse = true;
		prevSettingsValue = settingsValue;
		float num = settingsValue;
		settingsValue -= 1f / (float)settingSegments * (float)buttonSegmentJump;
		if (wrapAround)
		{
			settingsValue = ((settingsValue + num < 0f) ? 1f : Mathf.Clamp01(settingsValue));
		}
		else
		{
			settingsValue = Mathf.Clamp(settingsValue, 0f, 1f);
		}
		UpdateSegmentTextAndValue();
	}

	public void SetBarScaleInstant()
	{
		settingsBar.localScale = new Vector3(settingsValue, settingsBar.localScale.y, settingsBar.localScale.z);
	}
}
