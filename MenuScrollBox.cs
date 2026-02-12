using UnityEngine;

public class MenuScrollBox : MonoBehaviour
{
	public RectTransform scrollSize;

	public RectTransform scroller;

	public RectTransform scrollHandle;

	public RectTransform scrollBarBackground;

	public GameObject scrollBar;

	internal float scrollAmount;

	private float scrollAmountTarget;

	private float scrollHeight;

	internal MenuPage parentPage;

	private MenuSelectableElement menuSelectableElement;

	public MenuSelectionBox menuSelectionBox;

	internal float scrollerStartPosition;

	internal float scrollerEndPosition;

	private float scrollHandleTargetPosition;

	public MenuElementHover menuElementHover;

	internal bool scrollBoxActive = true;

	public float heightPadding;

	private bool parentIncludesScrollBox;

	private void Start()
	{
		parentPage = GetComponentInParent<MenuPage>();
		menuSelectableElement = scrollBarBackground.GetComponent<MenuSelectableElement>();
		scrollHandleTargetPosition = scrollHandle.localPosition.y;
		RecalculateScrollHeight();
	}

	internal bool RecalculateScrollHeight()
	{
		if (parentIncludesScrollBox)
		{
			parentPage.scrollBoxes--;
			parentIncludesScrollBox = false;
		}
		float num = 0f;
		foreach (RectTransform item in scroller)
		{
			float num2 = item.rect.height * item.pivot.y;
			if (item.localPosition.y - num2 < num)
			{
				num = item.localPosition.y - num2;
			}
		}
		scrollHeight = Mathf.Abs(num) + heightPadding;
		scrollerStartPosition = scrollHeight + 42f;
		scrollerEndPosition = scroller.localPosition.y;
		if (scrollHeight < scrollBarBackground.rect.height)
		{
			scrollBar.SetActive(value: false);
			return false;
		}
		parentPage.scrollBoxes++;
		parentIncludesScrollBox = true;
		scrollBar.SetActive(value: true);
		return true;
	}

	private void Update()
	{
		if (parentPage.scrollBoxes > 1)
		{
			if (menuElementHover.isHovering)
			{
				scrollBoxActive = true;
			}
			else
			{
				scrollBoxActive = false;
			}
		}
		if (!scrollBar.activeSelf || !scrollBoxActive)
		{
			return;
		}
		if (Input.GetMouseButton(0) && SemiFunc.UIMouseHover(parentPage, scrollBarBackground, menuSelectableElement.menuID))
		{
			float num = SemiFunc.UIMouseGetLocalPositionWithinRectTransform(scrollBarBackground).y;
			if (num < scrollHandle.sizeDelta.y / 2f)
			{
				num = scrollHandle.sizeDelta.y / 2f;
			}
			if (num > scrollBarBackground.rect.height - scrollHandle.sizeDelta.y / 2f)
			{
				num = scrollBarBackground.rect.height - scrollHandle.sizeDelta.y / 2f;
			}
			scrollHandleTargetPosition = num;
		}
		if ((SemiFunc.InputMovementY() != 0f && SemiFunc.NoTextInputsActive()) || SemiFunc.InputScrollY() != 0f)
		{
			if (SemiFunc.NoTextInputsActive())
			{
				scrollHandleTargetPosition += SemiFunc.InputMovementY() * 20f / (scrollHeight * 0.01f);
			}
			scrollHandleTargetPosition += SemiFunc.InputScrollY() / (scrollHeight * 0.01f);
			if (scrollHandleTargetPosition < scrollHandle.sizeDelta.y / 2f)
			{
				scrollHandleTargetPosition = scrollHandle.sizeDelta.y / 2f;
			}
			if (scrollHandleTargetPosition > scrollBarBackground.rect.height - scrollHandle.sizeDelta.y / 2f)
			{
				scrollHandleTargetPosition = scrollBarBackground.rect.height - scrollHandle.sizeDelta.y / 2f;
			}
		}
		scrollHandle.localPosition = new Vector3(scrollHandle.localPosition.x, Mathf.Lerp(scrollHandle.localPosition.y, scrollHandleTargetPosition, Time.deltaTime * 20f), scrollHandle.localPosition.z);
		scrollAmount = scrollHandle.localPosition.y / scrollBarBackground.rect.height * 1.1f;
		if (scrollAmount < 0f)
		{
			scrollAmount = 0f;
		}
		if (scrollAmount > 1f)
		{
			scrollAmount = 1f;
		}
		scroller.localPosition = new Vector3(scroller.localPosition.x, Mathf.Lerp(scrollerStartPosition, scrollerEndPosition, scrollAmount), scroller.localPosition.z);
	}
}
