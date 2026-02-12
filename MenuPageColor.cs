using System.Collections.Generic;
using UnityEngine;

public class MenuPageColor : MonoBehaviour
{
	public GameObject colorButtonPrefab;

	public RectTransform colorButtonHolder;

	public MenuColorSelected menuColorSelected;

	private MenuPage menuPage;

	private void Start()
	{
		menuPage = GetComponent<MenuPage>();
		List<Color> playerColors = AssetManager.instance.playerColors;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < playerColors.Count; i++)
		{
			GameObject obj = Object.Instantiate(colorButtonPrefab, colorButtonHolder);
			MenuButtonColor component = obj.GetComponent<MenuButtonColor>();
			MenuButton component2 = obj.GetComponent<MenuButton>();
			component.colorID = i;
			component.color = playerColors[i];
			component2.colorNormal = playerColors[i] + Color.black * 0.5f;
			component2.colorHover = playerColors[i];
			component2.colorClick = playerColors[i] + Color.white * 0.95f;
			RectTransform component3 = obj.GetComponent<RectTransform>();
			component3.SetSiblingIndex(0);
			component3.anchoredPosition = new Vector2(num, 224 + num2);
			num += 38;
			if ((float)num > colorButtonHolder.rect.width)
			{
				num = 0;
				num2 -= 30;
			}
		}
		Object.Destroy(colorButtonPrefab);
	}

	private void Update()
	{
		if (SemiFunc.InputDown(InputKey.Back) && MenuManager.instance.currentMenuPageIndex == MenuPageIndex.Color)
		{
			ConfirmButton();
		}
	}

	public void SetColor(int colorID, RectTransform buttonTransform)
	{
		MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm);
		menuColorSelected.SetColor(AssetManager.instance.playerColors[colorID], buttonTransform.position);
	}

	public void ConfirmButton()
	{
		MenuManager.instance.PageReactivatePageUnderThisPage(menuPage);
		MenuManager.instance.MenuEffectPopUpClose();
		menuPage.PageStateSet(MenuPage.PageState.Closing);
	}
}
