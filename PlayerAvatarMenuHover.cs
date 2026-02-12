using UnityEngine;

public class PlayerAvatarMenuHover : MonoBehaviour
{
	private RectTransform rectTransform;

	private MenuPage parentPage;

	public Transform pointer;

	private bool startClick;

	private Vector2 mouseClickPos;

	public PlayerAvatarMenu playerAvatarMenu;

	private MenuElementHover menuElementHover;

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
		parentPage = GetComponentInParent<MenuPage>();
		menuElementHover = GetComponent<MenuElementHover>();
	}

	private void Update()
	{
		Vector2 vector = SemiFunc.UIMouseGetLocalPositionWithinRectTransform(rectTransform);
		pointer.localPosition = new Vector3(vector.x * 0.98f, vector.y * 1.035f, 0f) / SemiFunc.UIMulti() * 2.23f;
		pointer.localPosition += new Vector3(-0.065f, -0.06f, 0f);
		pointer.GetComponent<MeshRenderer>().enabled = false;
		if (SemiFunc.InputHold(InputKey.Grab) && menuElementHover.isHovering)
		{
			if (!startClick)
			{
				startClick = true;
				mouseClickPos = vector;
			}
			Vector2 vector2 = (vector - mouseClickPos) * 25f;
			playerAvatarMenu.Rotate(new Vector3(0f, 0f - vector2.x, 0f));
		}
		else
		{
			startClick = false;
		}
	}
}
