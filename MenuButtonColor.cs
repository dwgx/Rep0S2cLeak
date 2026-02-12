using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonColor : MonoBehaviour
{
	internal int colorID;

	internal Color color = Color.white;

	private MenuButton menuButton;

	private MenuPageColor menuPageColor;

	private MenuPage parentPage;

	private bool buttonClicked;

	private void Start()
	{
		parentPage = GetComponentInParent<MenuPage>();
		List<Color> playerColors = AssetManager.instance.playerColors;
		color = playerColors[colorID];
		menuButton = GetComponent<MenuButton>();
		menuPageColor = GetComponentInParent<MenuPageColor>();
		StartCoroutine(LateStart());
	}

	private IEnumerator LateStart()
	{
		yield return new WaitForSeconds(0.1f);
		while (parentPage.currentPageState != MenuPage.PageState.Active)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (color == PlayerAvatar.instance.playerAvatarVisuals.color)
		{
			menuPageColor.SetColor(colorID, GetComponent<RectTransform>());
		}
	}

	private void Update()
	{
		if (menuButton.clicked && !buttonClicked)
		{
			menuPageColor.SetColor(colorID, GetComponent<RectTransform>());
			PlayerAvatar.instance.PlayerAvatarSetColor(colorID);
			buttonClicked = true;
		}
		if (buttonClicked && !menuButton.clicked)
		{
			buttonClicked = false;
		}
	}
}
