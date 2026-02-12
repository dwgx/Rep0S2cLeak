using UnityEngine;
using UnityEngine.UI;

public class MenuElementSliderPlayerMicGainSteam : MonoBehaviour
{
	private MenuElementHover menuElementHover;

	private MenuSliderPlayerMicGain menuSliderPlayerMicGain;

	public RawImage icon;

	public RectTransform iconTransform;

	[Space]
	public AnimationCurve hoverIntroCurve;

	public AnimationCurve hoverOutroCurve;

	private float hoverLerp;

	private bool hovering;

	private Vector3 previousScale;

	private bool colorFetched;

	private void Start()
	{
		if (SemiFunc.RunIsLobbyMenu())
		{
			base.gameObject.SetActive(value: false);
		}
		menuElementHover = GetComponent<MenuElementHover>();
		menuSliderPlayerMicGain = GetComponentInParent<MenuSliderPlayerMicGain>();
	}

	private void Update()
	{
		if (menuElementHover.isHovering)
		{
			if (!hovering)
			{
				hovering = true;
				previousScale = iconTransform.localScale;
			}
			if (Input.GetMouseButtonDown(0) || (SemiFunc.InputDown(InputKey.Confirm) && SemiFunc.NoTextInputsActive()))
			{
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm);
				SteamManager.instance.OpenProfile(menuSliderPlayerMicGain.playerAvatar);
			}
			if (hoverLerp < 1f)
			{
				hoverLerp = Mathf.Clamp01(hoverLerp + 3f * Time.deltaTime);
				iconTransform.localScale = Vector3.LerpUnclamped(previousScale, Vector3.one * 1.2f, hoverIntroCurve.Evaluate(hoverLerp));
			}
		}
		else if (hoverLerp > 0f)
		{
			if (hovering)
			{
				hovering = false;
				previousScale = iconTransform.localScale;
			}
			hoverLerp = Mathf.Clamp01(hoverLerp - 5f * Time.deltaTime);
			iconTransform.localScale = Vector3.LerpUnclamped(Vector3.one, previousScale, hoverOutroCurve.Evaluate(hoverLerp));
		}
	}
}
