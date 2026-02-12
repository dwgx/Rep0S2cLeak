using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
	private enum ButtonState
	{
		Hover,
		Clicked,
		Normal
	}

	public string buttonTextString = "BUTTON";

	internal TextMeshProUGUI buttonText;

	public bool customHoverArea;

	public bool doButtonEffect = true;

	public bool holdLogic = true;

	private Button button;

	internal bool hovering;

	private float hoverTimer;

	private float clickTimer;

	private float clickCooldown;

	internal bool clicked;

	private float buttonPitch = 1f;

	private string originalText;

	private RectTransform rectTransform;

	public bool hasHold;

	private float holdTimer;

	private float clickFrequency = 0.2f;

	private float clickFrequencyTicker;

	private MenuSelectableElement menuSelectableElement;

	private float buttonPadding;

	private MenuPage parentPage;

	private MenuButtonPopUp menuButtonPopUp;

	public bool middleAlignFix;

	public bool customColors;

	[Header("Custom Colors")]
	public Color colorNormal;

	public Color colorHover;

	public Color colorClick;

	private int buttonState = 2;

	private bool buttonStateStart;

	private float buttonTextSelectedScootPos = 1f;

	private Vector3 buttonTextSelectedOriginalPos;

	internal bool disabled;

	private void Awake()
	{
		if (!customColors)
		{
			colorNormal = Color.gray;
			colorHover = Color.white;
			colorClick = AssetManager.instance.colorYellow;
		}
		menuButtonPopUp = GetComponent<MenuButtonPopUp>();
		menuSelectableElement = GetComponent<MenuSelectableElement>();
		parentPage = GetComponentInParent<MenuPage>();
		rectTransform = GetComponent<RectTransform>();
		button = GetComponent<Button>();
		buttonText = GetComponentInChildren<TextMeshProUGUI>();
		buttonTextSelectedOriginalPos = buttonText.transform.localPosition;
		if (buttonTextString != "BUTTON")
		{
			buttonText.text = buttonTextString;
		}
		originalText = buttonText.text;
		Vector2 sizeDelta = rectTransform.sizeDelta;
		buttonPitch = SemiFunc.MenuGetPitchFromYPos(rectTransform);
		float fontSize = buttonText.fontSize;
		buttonText.fontSize = fontSize;
		buttonText.enableAutoSizing = false;
		TextAlignmentOptions alignment = buttonText.alignment;
		buttonText.alignment = TextAlignmentOptions.MidlineLeft;
		buttonPadding = 0f;
		Vector2 sizeDelta2 = rectTransform.sizeDelta;
		rectTransform.sizeDelta = new Vector2(buttonText.GetPreferredValues(originalText, 0f, 0f).x + buttonPadding, buttonText.GetPreferredValues(originalText, 0f, 0f).y + buttonPadding / 2f);
		buttonText.alignment = alignment;
		if (alignment == TextAlignmentOptions.Midline)
		{
			buttonText.enableAutoSizing = true;
		}
		if (middleAlignFix)
		{
			rectTransform.position += new Vector3((sizeDelta2.x - rectTransform.sizeDelta.x) / 2f, 0f, 0f);
			buttonText.enableAutoSizing = false;
		}
		if (customHoverArea)
		{
			rectTransform.sizeDelta = sizeDelta;
		}
	}

	private void Update()
	{
		button.image.color = new Color(0f, 0f, 0f, 0f);
		HoverLogic();
		switch (buttonState)
		{
		case 2:
			ButtonNormal();
			buttonStateStart = false;
			break;
		case 0:
			ButtonHover();
			buttonStateStart = false;
			break;
		case 1:
			ButtonClicked();
			buttonStateStart = false;
			break;
		}
		if (hoverTimer > 0f)
		{
			hoverTimer -= Time.deltaTime;
		}
		void ButtonClicked()
		{
			_ = buttonStateStart;
			HoldTimer();
			buttonText.color = colorClick;
		}
		void ButtonHover()
		{
			if (buttonStateStart)
			{
				MenuManager.instance.MenuEffectHover(buttonPitch);
			}
			HoldTimer();
			buttonText.color = colorHover;
		}
		void ButtonNormal()
		{
			_ = buttonStateStart;
			holdTimer = 0f;
			buttonText.transform.localPosition = buttonTextSelectedOriginalPos;
			buttonText.color = colorNormal;
		}
	}

	private void OnValidate()
	{
		if (!SemiFunc.OnValidateCheck() && buttonTextString != "BUTTON")
		{
			buttonText = GetComponentInChildren<TextMeshProUGUI>();
			if (buttonText.text != buttonTextString)
			{
				buttonText.text = buttonTextString;
			}
			if (base.gameObject.name != "Menu Button - " + buttonTextString)
			{
				base.gameObject.name = "Menu Button - " + buttonTextString;
			}
		}
	}

	private void HoverLogic()
	{
		int num = 0;
		if (!customHoverArea)
		{
			num = 10;
		}
		if (SemiFunc.UIMouseHover(parentPage, rectTransform, menuSelectableElement.menuID, num))
		{
			if (!hovering)
			{
				OnHoverStart();
				hovering = true;
			}
			hoverTimer = 0.01f;
		}
		if (hovering || (clicked && hovering))
		{
			if (Input.GetMouseButtonDown(0) && clickCooldown <= 0f)
			{
				OnSelect();
				holdTimer = 0f;
				clickTimer = 0.2f;
				if (!hasHold)
				{
					clickCooldown = 0.25f;
				}
			}
			if (hasHold)
			{
				if (Input.GetMouseButton(0))
				{
					holdTimer += Time.deltaTime;
				}
				else
				{
					holdTimer = 0f;
					clickFrequencyTicker = 0f;
					clickFrequency = 0.2f;
				}
			}
		}
		clickCooldown -= Time.deltaTime;
		if (clickTimer > 0f)
		{
			clickTimer -= Time.deltaTime;
			clicked = true;
		}
		else
		{
			if (clicked)
			{
				OnSelectEnd();
			}
			clicked = false;
		}
		if (hoverTimer <= 0f)
		{
			if (hovering)
			{
				OnHoverEnd();
			}
			hovering = false;
		}
		if (hoverTimer > 0f)
		{
			OnHovering();
			hovering = true;
		}
	}

	private void ButtonStateSet(int state)
	{
		buttonState = state;
		buttonStateStart = true;
	}

	private void OnHoverStart()
	{
		ButtonStateSet(0);
		buttonStateStart = true;
	}

	public void OnHovering()
	{
		buttonText.transform.localPosition = new Vector3(buttonTextSelectedOriginalPos.x, buttonTextSelectedOriginalPos.y + buttonTextSelectedScootPos, buttonTextSelectedOriginalPos.z);
		Vector2 sizeDelta = rectTransform.sizeDelta;
		_ = new Vector3(sizeDelta.x / 2f, sizeDelta.y / 2f, 0f) + (base.transform.localPosition - new Vector3(buttonPadding / 2f, 0f, 0f));
		SemiFunc.MenuSelectionBoxTargetSet(parentPage, rectTransform);
	}

	private void OnHoverEnd()
	{
		ButtonStateSet(2);
		buttonStateStart = true;
	}

	private void OnSelect()
	{
		if (disabled)
		{
			MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Deny);
			return;
		}
		if (doButtonEffect)
		{
			MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Action, parentPage);
		}
		ButtonStateSet(1);
		if (!menuButtonPopUp)
		{
			button.onClick.Invoke();
		}
		else
		{
			MenuManager.instance.PagePopUpTwoOptions(menuButtonPopUp, menuButtonPopUp.headerText, menuButtonPopUp.headerColor, menuButtonPopUp.bodyText, menuButtonPopUp.option1Text, menuButtonPopUp.option2Text, menuButtonPopUp.richText);
		}
	}

	private void OnSelectEnd()
	{
		if (!hovering)
		{
			ButtonStateSet(2);
		}
		else
		{
			ButtonStateSet(0);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnHoverStart();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnHoverEnd();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		OnSelect();
	}

	private void HoldTimer()
	{
		if (holdLogic && holdTimer > 0.5f)
		{
			if (clickFrequencyTicker <= 0f)
			{
				OnSelect();
				clickFrequencyTicker = clickFrequency;
				clickFrequency -= clickFrequency * 0.2f;
				clickFrequency = Mathf.Clamp(clickFrequency, 0.025f, 0.2f);
			}
			else
			{
				clickFrequencyTicker -= Time.deltaTime;
			}
		}
	}
}
