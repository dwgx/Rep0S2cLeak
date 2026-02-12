using TMPro;
using UnityEngine;

public class HaulUI : SemiUI
{
	private TextMeshProUGUI Text;

	public static HaulUI instance;

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
		if (SemiFunc.RunIsLevel())
		{
			if (!RoundDirector.instance.extractionPointActive)
			{
				Hide();
			}
			int currentHaul = RoundDirector.instance.currentHaul;
			int extractionHaulGoal = RoundDirector.instance.extractionHaulGoal;
			currentHaul = (currentHaulValue = Mathf.Max(0, currentHaul));
			string text2 = "<color=#558B2F>$</color>";
			text = "<size=30>" + text2 + SemiFunc.DollarGetString(currentHaul) + "<color=#616161> <size=45>/</size> </color>" + text2 + "<u>" + SemiFunc.DollarGetString(extractionHaulGoal);
			if (currentHaulValue > prevHaulValue)
			{
				SemiUISpringShakeY(10f, 10f, 0.3f);
				SemiUISpringScale(0.05f, 5f, 0.2f);
				SemiUITextFlashColor(Color.green, 0.2f);
				prevHaulValue = currentHaulValue;
			}
			if (currentHaulValue < prevHaulValue)
			{
				SemiUISpringShakeY(10f, 10f, 0.3f);
				SemiUISpringScale(0.05f, 5f, 0.2f);
				SemiUITextFlashColor(Color.red, 0.2f);
				prevHaulValue = currentHaulValue;
			}
		}
		else
		{
			text = SemiFunc.DollarGetString(SemiFunc.StatGetRunCurrency());
			Hide();
		}
		Text.text = text;
	}
}
