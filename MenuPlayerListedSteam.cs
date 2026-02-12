using UnityEngine;
using UnityEngine.UI;

public class MenuPlayerListedSteam : MonoBehaviour
{
	private MenuElementHover menuElementHover;

	private MenuPlayerListed menuPlayerListed;

	public RectTransform iconTransform;

	public RawImage icon;

	public AnimationCurve showCurve;

	public AnimationCurve hideCurve;

	private float showLerp = 0.1f;

	private void Awake()
	{
		menuElementHover = GetComponent<MenuElementHover>();
		menuPlayerListed = GetComponentInParent<MenuPlayerListed>();
	}

	private void Update()
	{
		Color color = menuPlayerListed.playerAvatar.playerAvatarVisuals.color;
		color = Color.Lerp(color, Color.white, 0.7f);
		Color color2 = new Color(color.r, color.g, color.b, 0f);
		if (menuElementHover.isHovering)
		{
			if (Input.GetMouseButtonDown(0) || (SemiFunc.InputDown(InputKey.Confirm) && SemiFunc.NoTextInputsActive()))
			{
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm);
				SteamManager.instance.OpenProfile(menuPlayerListed.playerAvatar);
				showLerp = 0.2f;
			}
			if (showLerp < 1f)
			{
				showLerp = Mathf.Clamp01(showLerp + 3f * Time.deltaTime);
				icon.color = Color.Lerp(color2, color, showCurve.Evaluate(showLerp));
				iconTransform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, showCurve.Evaluate(showLerp));
			}
		}
		else if (showLerp > 0f)
		{
			showLerp = Mathf.Clamp01(showLerp - 5f * Time.deltaTime);
			icon.color = Color.Lerp(color2, color, hideCurve.Evaluate(showLerp));
			iconTransform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, hideCurve.Evaluate(showLerp));
		}
	}
}
