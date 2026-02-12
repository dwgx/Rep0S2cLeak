using TMPro;
using UnityEngine;

public class CurrencyUI : SemiUI
{
	private TextMeshProUGUI Text;

	public static CurrencyUI instance;

	private int prevHaulValue;

	private int currentHaulValue;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
	}

	protected override void Update()
	{
		base.Update();
		string text = "";
		int num = 0;
		if (SemiFunc.RunIsLevel() || SemiFunc.RunIsTutorial())
		{
			Hide();
		}
		if (!(showTimer > 0f))
		{
			return;
		}
		num = (currentHaulValue = SemiFunc.StatGetRunCurrency());
		if (currentHaulValue != prevHaulValue)
		{
			Color color = Color.green;
			if (currentHaulValue < prevHaulValue)
			{
				color = Color.red;
			}
			SemiUISpringShakeY(20f, 10f, 0.3f);
			SemiUITextFlashColor(color, 0.2f);
			SemiUISpringScale(0.4f, 5f, 0.2f);
			prevHaulValue = currentHaulValue;
		}
		text = SemiFunc.DollarGetString(num);
		Text.text = "$" + text + "K";
	}

	public void FetchCurrency()
	{
		string text = SemiFunc.DollarGetString(SemiFunc.StatGetRunCurrency());
		Text.text = "$" + text + "K";
	}
}
