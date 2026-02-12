using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuBigButton : MonoBehaviour
{
	public enum State
	{
		Main,
		Edit
	}

	public string buttonTitle = "";

	public string buttonName = "NewButton";

	public RawImage mainButtonBG;

	public RawImage behindButtonBG;

	public MenuButton menuButton;

	public TextMeshProUGUI buttonTitleTextMesh;

	private Color mainButtonMainColor;

	private Color behindButtonMainColor;

	public State state;

	private void Start()
	{
		behindButtonMainColor = behindButtonBG.color;
		mainButtonMainColor = mainButtonBG.color;
	}

	private void Update()
	{
		switch (state)
		{
		case State.Main:
			StateMain();
			break;
		case State.Edit:
			StateEdit();
			break;
		}
	}

	private void StateMain()
	{
		if (menuButton.hovering)
		{
			Color color = new Color(0.7f, 0.2f, 0f, 1f);
			mainButtonBG.color = color;
			behindButtonBG.color = AssetManager.instance.colorYellow;
		}
		else
		{
			mainButtonBG.color = mainButtonMainColor;
			behindButtonBG.color = behindButtonMainColor;
		}
		if (menuButton.clicked)
		{
			Color color2 = new Color(1f, 0.5f, 0f, 1f);
			mainButtonBG.color = color2;
			behindButtonBG.color = Color.white;
		}
	}

	private void StateEdit()
	{
		menuButton.buttonText.text = "[press new button]";
		if (menuButton.hovering)
		{
			Color color = new Color(0.5f, 0.1f, 0f, 1f);
			mainButtonBG.color = color;
			color = new Color(1f, 0.5f, 0f, 1f);
			behindButtonBG.color = color;
		}
		else
		{
			Color color2 = new Color(0.5f, 0.1f, 0f, 1f);
			mainButtonBG.color = color2;
			color2 = new Color(0.7f, 0.2f, 0f, 1f);
			behindButtonBG.color = color2;
		}
		if (menuButton.clicked)
		{
			Color color3 = new Color(1f, 0.5f, 0f, 1f);
			mainButtonBG.color = color3;
			behindButtonBG.color = Color.white;
		}
	}
}
