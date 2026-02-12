using TMPro;
using UnityEngine;

public class ShopCostUI : SemiUI
{
	private TextMeshProUGUI Text;

	public static ShopCostUI instance;

	public int animatedValue;

	private Color originalColor;

	private int currentValue;

	private int prevValue;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
		originalColor = Text.color;
	}

	protected override void Update()
	{
		base.Update();
		string text = "";
		int num = 0;
		if (SemiFunc.RunIsShop())
		{
			num = SemiFunc.ShopGetTotalCost();
			text = SemiFunc.DollarGetString(num);
			if (num > 0)
			{
				Text.text = "-$" + text + "K";
				Text.color = originalColor;
			}
			else
			{
				Hide();
			}
			currentValue = num;
			if (currentValue != prevValue)
			{
				Color color = Color.white;
				if (currentValue > prevValue)
				{
					color = Color.red;
				}
				SemiUISpringShakeY(20f, 10f, 0.3f);
				SemiUITextFlashColor(color, 0.2f);
				SemiUISpringScale(0.4f, 5f, 0.2f);
				prevValue = currentValue;
			}
		}
		if (!SemiFunc.RunIsShop())
		{
			Hide();
		}
		if (showTimer > 0f && SemiFunc.RunIsLevel())
		{
			Text.text = "+$" + animatedValue + "K";
			Text.color = Color.green;
		}
	}
}
