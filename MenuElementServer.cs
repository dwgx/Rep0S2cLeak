using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuElementServer : MonoBehaviour
{
	public enum IntroType
	{
		Vertical,
		Right,
		Left
	}

	internal IntroType introType;

	internal string roomName;

	internal MenuButtonPopUp menuButtonPopUp;

	public Image fadePanel;

	private MenuElementHover menuElementHover;

	private float initialFadeAlpha;

	public TextMeshProUGUI textName;

	public TextMeshProUGUI textPlayers;

	[Space]
	public RectTransform animationTransform;

	public AnimationCurve introCurve;

	private float introLerp;

	private bool introDone;

	private void Awake()
	{
		menuElementHover = GetComponent<MenuElementHover>();
		initialFadeAlpha = fadePanel.color.a;
		menuButtonPopUp = GetComponent<MenuButtonPopUp>();
	}

	private void Update()
	{
		UpdateIntro();
		if (menuElementHover.isHovering)
		{
			Color color = fadePanel.color;
			color.a = Mathf.Lerp(color.a, 0f, Time.deltaTime * 10f);
			fadePanel.color = color;
			if (Input.GetMouseButtonDown(0) || (SemiFunc.InputDown(InputKey.Confirm) && SemiFunc.NoTextInputsActive()))
			{
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm);
				MenuButtonPopUp component = GetComponent<MenuButtonPopUp>();
				MenuManager.instance.PagePopUpTwoOptions(component, component.headerText, component.headerColor, component.bodyText, component.option1Text, component.option2Text, component.richText);
			}
		}
		else
		{
			Color color2 = fadePanel.color;
			color2.a = Mathf.Lerp(color2.a, initialFadeAlpha, Time.deltaTime * 10f);
			fadePanel.color = color2;
		}
	}

	private void UpdateIntro()
	{
		if (!introDone)
		{
			if (introType == IntroType.Vertical)
			{
				introLerp += Time.deltaTime * 5f;
				animationTransform.anchoredPosition = new Vector3(0f, introCurve.Evaluate(introLerp) * 8f, 0f);
			}
			else if (introType == IntroType.Right)
			{
				introLerp += Time.deltaTime * 5f;
				animationTransform.anchoredPosition = new Vector3(introCurve.Evaluate(introLerp) * 30f, 0f, 0f);
			}
			else if (introType == IntroType.Left)
			{
				introLerp += Time.deltaTime * 5f;
				animationTransform.anchoredPosition = new Vector3((0f - introCurve.Evaluate(introLerp)) * 30f, 0f, 0f);
			}
			if (introLerp > 1f)
			{
				introDone = true;
			}
		}
	}

	public void OnButton()
	{
		DataDirector.instance.networkJoinServerName = roomName;
		MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm, null, 1f, 1f);
		RunManager.instance.ResetProgress();
		StatsManager.instance.saveFileCurrent = "";
		GameManager.instance.SetConnectRandom(_connectRandom: true);
		GameManager.instance.localTest = false;
		RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.LobbyMenu);
		RunManager.instance.lobbyJoin = true;
	}
}
