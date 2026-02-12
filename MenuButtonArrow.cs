using UnityEngine;
using UnityEngine.Events;

public class MenuButtonArrow : MonoBehaviour
{
	private MenuElementHover menuElementHover;

	private CanvasGroup canvasGroup;

	public RectTransform backgroundRect;

	public SpringFloat hoverSpring;

	private float hideTimer;

	[Space]
	public UnityEvent onClick;

	private void Awake()
	{
		menuElementHover = GetComponentInChildren<MenuElementHover>();
		canvasGroup = GetComponent<CanvasGroup>();
	}

	private void Update()
	{
		float num = ((!menuElementHover.isHovering) ? SemiFunc.SpringFloatGet(hoverSpring, 0f) : SemiFunc.SpringFloatGet(hoverSpring, 1f));
		backgroundRect.localScale = new Vector3(1f + num * 0.25f, 1f + num * 0.25f, 1f);
		if (menuElementHover.isHovering && (Input.GetMouseButtonDown(0) || (SemiFunc.InputDown(InputKey.Confirm) && SemiFunc.NoTextInputsActive())))
		{
			MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm);
			hoverSpring.springVelocity += 50f;
			onClick.Invoke();
		}
		if (hideTimer > 0f)
		{
			hideTimer -= Time.deltaTime;
			menuElementHover.Disable();
			canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * 15f);
		}
		else
		{
			canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.deltaTime * 15f);
		}
	}

	public void Hide()
	{
		hideTimer = 0.1f;
	}

	public void HideSetInstant()
	{
		Hide();
		canvasGroup.alpha = 0f;
	}
}
