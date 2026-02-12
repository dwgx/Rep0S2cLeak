using TMPro;
using UnityEngine;

public class GoalUI : SemiUI
{
	private TextMeshProUGUI Text;

	public static GoalUI instance;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
	}

	protected override void Update()
	{
		base.Update();
		if (SemiFunc.RunIsLevel() || SemiFunc.RunIsTutorial())
		{
			int extractionPoints = RoundDirector.instance.extractionPoints;
			int extractionPointsCompleted = RoundDirector.instance.extractionPointsCompleted;
			Text.text = extractionPointsCompleted + "<color=#7D250B> <size=45>/</size> </color><b>" + extractionPoints;
		}
		else
		{
			Hide();
		}
		if (HaulUI.instance.hideTimer > 0f)
		{
			SemiUIScoot(new Vector2(0f, 45f));
		}
	}
}
