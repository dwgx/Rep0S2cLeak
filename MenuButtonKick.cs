using UnityEngine;

public class MenuButtonKick : MonoBehaviour
{
	private MenuButton button;

	private CanvasGroup canvasGroup;

	private PlayerAvatar playerAvatar;

	private bool setup;

	public RectTransform backgroundRect;

	public SpringFloat hoverSpring;

	private void Awake()
	{
		button = GetComponentInChildren<MenuButton>();
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
	}

	private void Update()
	{
		if (setup)
		{
			float num = ((!button.hovering) ? SemiFunc.SpringFloatGet(hoverSpring, 0f) : SemiFunc.SpringFloatGet(hoverSpring, 1f));
			backgroundRect.localScale = new Vector3(1f + num * 0.25f, 1f + num * 0.25f, 1f);
		}
	}

	public void Setup(PlayerAvatar _playerAvatar)
	{
		playerAvatar = _playerAvatar;
		if (SemiFunc.IsMasterClient())
		{
			if (playerAvatar.isLocal)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			GetComponentInChildren<MenuButtonPopUp>().bodyText = "Do you really want to kick\n" + playerAvatar.playerName;
			canvasGroup.alpha = 1f;
			setup = true;
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void Kick()
	{
		NetworkManager.instance.BanPlayer(playerAvatar);
	}
}
