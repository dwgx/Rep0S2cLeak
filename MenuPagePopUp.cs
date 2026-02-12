using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MenuPagePopUp : MonoBehaviour
{
	public static MenuPagePopUp instance;

	internal MenuPage menuPage;

	internal UnityEvent option1Event;

	internal UnityEvent option2Event;

	public TextMeshProUGUI bodyTextMesh;

	public MenuButton okButton;

	internal bool richText = true;

	private void Start()
	{
		instance = this;
		menuPage = GetComponent<MenuPage>();
		bodyTextMesh.richText = richText;
	}

	private void Update()
	{
		if (okButton.buttonText.text != okButton.buttonTextString)
		{
			okButton.buttonText.text = okButton.buttonTextString;
		}
	}

	public void ButtonEvent()
	{
		MenuManager.instance.PageReactivatePageUnderThisPage(menuPage);
		MenuManager.instance.MenuEffectPopUpClose();
		menuPage.PageStateSet(MenuPage.PageState.Closing);
	}
}
