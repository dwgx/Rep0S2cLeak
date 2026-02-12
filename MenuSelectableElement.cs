using UnityEngine;

public class MenuSelectableElement : MonoBehaviour
{
	internal string menuID;

	internal RectTransform rectTransform;

	internal MenuPage parentPage;

	internal bool isInScrollBox;

	internal MenuScrollBox menuScrollBox;

	private void Start()
	{
		menuID = SemiFunc.MenuGetSelectableID(base.gameObject);
		rectTransform = GetComponent<RectTransform>();
		parentPage = GetComponentInParent<MenuPage>();
		if ((bool)parentPage)
		{
			parentPage.selectableElements.Add(this);
			if (rectTransform.localPosition.y < parentPage.bottomElementYPos)
			{
				parentPage.bottomElementYPos = rectTransform.localPosition.y;
			}
		}
		isInScrollBox = false;
		MenuScrollBox componentInParent = GetComponentInParent<MenuScrollBox>();
		if ((bool)componentInParent)
		{
			isInScrollBox = true;
			menuScrollBox = componentInParent;
		}
	}
}
