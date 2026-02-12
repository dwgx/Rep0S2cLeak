using TMPro;
using UnityEngine;

public class ItemInfoExtraUI : SemiUI
{
	private TextMeshProUGUI Text;

	public static ItemInfoExtraUI instance;

	private string messagePrev = "prev";

	private float messageTimer;

	private GameObject bigMessageEmojiObject;

	private TextMeshProUGUI emojiText;

	private Color textColor;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
		Text.text = "";
	}

	public void ItemInfoText(string message, Color color)
	{
		if (!(messageTimer > 0f))
		{
			messageTimer = 0.2f;
			if (message != messagePrev)
			{
				Text.text = message;
				SemiUISpringShakeY(20f, 10f, 0.3f);
				SemiUISpringScale(0.4f, 5f, 0.2f);
				textColor = color;
				Text.color = textColor;
				messagePrev = message;
			}
		}
	}

	protected override void Update()
	{
		if (Inventory.instance.InventorySpotsOccupied() > 0)
		{
			SemiUIScoot(new Vector2(0f, 5f));
		}
		else
		{
			SemiUIScoot(new Vector2(0f, -10f));
		}
		base.Update();
		if (!SemiFunc.RunIsShop())
		{
			if (!SemiFunc.RunIsShop())
			{
				Text.fontSize = 12f;
			}
			if (messageTimer > 0f)
			{
				messageTimer -= Time.deltaTime;
				return;
			}
			Text.color = Color.white;
			messagePrev = "prev";
			Hide();
		}
	}
}
