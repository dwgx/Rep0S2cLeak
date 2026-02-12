using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MenuPage : MonoBehaviour
{
	public enum PageState
	{
		Opening,
		Active,
		Closing,
		Inactive,
		Activating,
		Closed
	}

	public string menuHeaderName;

	public MenuPageIndex menuPageIndex;

	private Vector2 originalPosition;

	internal RectTransform rectTransform;

	private Vector2 animateAwayPosition = new Vector2(0f, 0f);

	private Vector2 targetPosition;

	internal float bottomElementYPos;

	internal List<MenuSelectableElement> selectableElements = new List<MenuSelectableElement>();

	public TextMeshProUGUI menuHeader;

	internal bool pageIsOnTopOfOtherPage;

	internal MenuPage pageUnderThisPage;

	internal bool pageActive;

	private float pageActiveTimer;

	internal bool popUpAnimation;

	internal MenuSelectionBox selectionBox;

	private float stateTimer;

	internal bool addedPageOnTop;

	internal MenuPage parentPage;

	public bool disableIntroAnimation;

	public bool disableOutroAnimation;

	internal List<MenuSettingElement> settingElements = new List<MenuSettingElement>();

	internal int currentActiveSettingElement = -1;

	private float activeSettingElementTimer;

	public UnityEvent onPageEnd;

	internal int scrollBoxes;

	private bool stateStart = true;

	internal PageState currentPageState;

	private void Start()
	{
		selectionBox = GetComponentInChildren<MenuSelectionBox>();
		rectTransform = GetComponent<RectTransform>();
		originalPosition = rectTransform.localPosition;
		animateAwayPosition = new Vector2(originalPosition.x, originalPosition.y - rectTransform.rect.height);
		rectTransform.localPosition = new Vector2(originalPosition.x, originalPosition.y + rectTransform.rect.height);
		MenuManager.instance.PageAdd(this);
		StartCoroutine(LateStart());
	}

	private IEnumerator LateStart()
	{
		yield return null;
		if (!parentPage)
		{
			parentPage = this;
		}
	}

	private void FixedUpdate()
	{
		if (pageActiveTimer <= 0f)
		{
			pageActive = false;
		}
		if (pageActiveTimer > 0f)
		{
			pageActive = true;
			pageActiveTimer -= Time.fixedDeltaTime;
		}
	}

	private void Update()
	{
		switch (currentPageState)
		{
		case PageState.Opening:
			StateOpening();
			stateStart = false;
			break;
		case PageState.Active:
			StateActive();
			stateStart = false;
			break;
		case PageState.Closing:
			StateClosing();
			stateStart = false;
			break;
		case PageState.Inactive:
			StateInactive();
			stateStart = false;
			break;
		case PageState.Activating:
			StateActivating();
			stateStart = false;
			break;
		}
		if (activeSettingElementTimer > 0f)
		{
			activeSettingElementTimer -= Time.deltaTime;
			if (activeSettingElementTimer <= 0f)
			{
				currentActiveSettingElement = -1;
			}
		}
	}

	private void OnEnable()
	{
		ResetPage();
	}

	public void ResetPage()
	{
		if ((bool)rectTransform)
		{
			rectTransform.localPosition = new Vector2(originalPosition.x, originalPosition.y + rectTransform.rect.height);
		}
	}

	public void PageStateSet(PageState pageState)
	{
		stateTimer = 0f;
		currentPageState = pageState;
		stateStart = true;
	}

	private void StateOpening()
	{
		if (stateStart)
		{
			if (!popUpAnimation)
			{
				MenuManager.instance.MenuEffectPageIntro();
			}
			else
			{
				MenuManager.instance.MenuEffectPopUpOpen();
			}
			LockAndHide();
		}
		if (!addedPageOnTop)
		{
			MenuSelectionBox.instance.firstSelection = true;
		}
		if (Vector2.Distance(rectTransform.localPosition, originalPosition) < 0.8f)
		{
			PageStateSet(PageState.Active);
		}
		if (!disableIntroAnimation)
		{
			float deltaTime = Time.deltaTime;
			rectTransform.localPosition = Vector2.Lerp(rectTransform.localPosition, originalPosition, 40f * deltaTime);
		}
		LockAndHide();
	}

	private void StateActive()
	{
		if (stateStart && !disableIntroAnimation)
		{
			rectTransform.localPosition = originalPosition;
		}
		if (!disableIntroAnimation)
		{
			rectTransform.localPosition = originalPosition;
		}
		PageAddedOnTopLogic();
		MenuSelectionBox instance = MenuSelectionBox.instance;
		if (!instance || instance != selectionBox)
		{
			selectionBox.Reinstate();
		}
		LockAndHide();
		if (MenuManager.instance.currentMenuPageIndex != menuPageIndex)
		{
			PageStateSet(PageState.Inactive);
		}
		pageActive = true;
		pageActiveTimer = 0.1f;
	}

	public bool SettingElementActiveCheckFree(int index)
	{
		if (currentActiveSettingElement != -1)
		{
			return currentActiveSettingElement == index;
		}
		return true;
	}

	private void StateClosing()
	{
		LockAndHide();
		if (stateStart)
		{
			if (!popUpAnimation)
			{
				MenuManager.instance.MenuEffectPageOutro();
			}
			else
			{
				MenuManager.instance.MenuEffectPopUpClose();
			}
			if (MenuManager.instance.currentMenuPage == this)
			{
				MenuManager.instance.currentMenuPage = null;
				MenuManager.instance.PageRemove(this);
			}
		}
		if (Vector2.Distance(rectTransform.localPosition, animateAwayPosition) < 0.8f)
		{
			onPageEnd.Invoke();
			MenuManager.instance.PageRemove(this);
			Object.Destroy(base.gameObject);
		}
		float deltaTime = Time.deltaTime;
		rectTransform.localPosition = Vector2.Lerp(rectTransform.localPosition, animateAwayPosition, 40f * deltaTime);
	}

	private void StateInactive()
	{
		_ = stateStart;
		if (MenuManager.instance.currentMenuPageIndex == menuPageIndex)
		{
			PageStateSet(PageState.Active);
		}
	}

	private void PageAddedOnTopLogic()
	{
		if (currentPageState == PageState.Opening || currentPageState == PageState.Closing || !addedPageOnTop)
		{
			return;
		}
		if ((bool)parentPage)
		{
			if (currentPageState != parentPage.currentPageState)
			{
				PageStateSet(parentPage.currentPageState);
			}
		}
		else if (currentPageState != PageState.Closing)
		{
			PageStateSet(PageState.Closing);
		}
	}

	private void StateActivating()
	{
		_ = stateStart;
		if (stateTimer > 0.1f)
		{
			PageStateSet(PageState.Active);
		}
		stateTimer += Time.deltaTime;
	}

	public void SettingElementActiveSet(int index)
	{
		if (currentActiveSettingElement == -1)
		{
			currentActiveSettingElement = index;
		}
		activeSettingElementTimer = 0.1f;
	}

	private void LockAndHide()
	{
		SemiFunc.UIHideAim();
		SemiFunc.UIHideEnergy();
		SemiFunc.UIHideGoal();
		SemiFunc.UIHideHealth();
		SemiFunc.UIHideOvercharge();
		SemiFunc.UIHideInventory();
		SemiFunc.UIHideHaul();
		SemiFunc.UIHideCurrency();
		SemiFunc.UIHideShopCost();
		SemiFunc.UIHideTumble();
		SemiFunc.UIHideWorldSpace();
		SemiFunc.UIHideValuableDiscover();
		SemiFunc.CameraOverrideStopAim();
	}
}
