using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuElementRegion : MonoBehaviour
{
	public Image fadePanel;

	public MenuPageRegions parentPage;

	private MenuElementHover menuElementHover;

	private float initialFadeAlpha;

	public TextMeshProUGUI textName;

	public TextMeshProUGUI textPing;

	[Space]
	public bool animationSkip;

	public RectTransform animationTransform;

	public AnimationCurve introCurve;

	private float introLerp;

	internal string regionCode = "";

	private void Start()
	{
		menuElementHover = GetComponent<MenuElementHover>();
		initialFadeAlpha = fadePanel.color.a;
		UpdateIntro();
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
				parentPage.PickRegion(regionCode);
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm);
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
		if (!animationSkip)
		{
			introLerp += Time.deltaTime * 5f;
			if (introLerp > 1f)
			{
				animationSkip = true;
			}
			animationTransform.anchoredPosition = new Vector3((0f - introCurve.Evaluate(introLerp)) * 10f, 0f, 0f);
		}
	}
}
