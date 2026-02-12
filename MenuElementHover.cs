using UnityEngine;

public class MenuElementHover : MonoBehaviour
{
	internal bool isHovering;

	private RectTransform rectTransform;

	private MenuSelectableElement menuSelectableElement;

	private MenuPage parentPage;

	private float buttonPitch;

	public bool hasHoverEffect = true;

	internal string menuID = "-1";

	private float disableTimer;

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
		menuSelectableElement = GetComponent<MenuSelectableElement>();
		parentPage = GetComponentInParent<MenuPage>();
		buttonPitch = SemiFunc.MenuGetPitchFromYPos(rectTransform);
		if ((bool)menuSelectableElement)
		{
			menuID = menuSelectableElement.menuID;
		}
	}

	private void Update()
	{
		if (disableTimer > 0f)
		{
			disableTimer -= Time.deltaTime;
			isHovering = false;
		}
		else if (SemiFunc.UIMouseHover(parentPage, rectTransform, menuID))
		{
			if (!isHovering && hasHoverEffect)
			{
				MenuManager.instance.MenuEffectHover(buttonPitch);
			}
			isHovering = true;
		}
		else if (isHovering)
		{
			isHovering = false;
		}
		if (hasHoverEffect && isHovering)
		{
			SemiFunc.MenuSelectionBoxTargetSet(parentPage, rectTransform);
		}
	}

	public void Disable()
	{
		disableTimer = 0.1f;
	}
}
