using TMPro;
using UnityEngine;

public class EnergyUI : SemiUI
{
	private TextMeshProUGUI Text;

	public static EnergyUI instance;

	private TextMeshProUGUI textEnergyMax;

	private float energyPrev;

	private Vector3 maxEnergyBasePosition;

	private float maxEnergyPrevOffset;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
		textEnergyMax = base.transform.Find("EnergyMax").GetComponent<TextMeshProUGUI>();
		maxEnergyBasePosition = textEnergyMax.transform.localPosition;
	}

	protected override void Update()
	{
		base.Update();
		Text.text = Mathf.Ceil(PlayerController.instance.EnergyCurrent).ToString();
		float num = ((Text.text.Length > 3) ? ((float)(Text.text.Length - 3) * 20f) : 0f);
		textEnergyMax.transform.localPosition = maxEnergyBasePosition + new Vector3(num, 0f, 0f);
		textEnergyMax.text = "<b><color=orange>/</color></b>" + Mathf.Ceil(PlayerController.instance.EnergyStart);
		if (SemiFunc.FPSImpulse30() && PlayerAvatar.instance.upgradeCrouchRestActive)
		{
			if ((int)PlayerController.instance.EnergyCurrent > (int)energyPrev && (int)PlayerController.instance.EnergyCurrent < (int)PlayerController.instance.EnergyStart)
			{
				SemiUITextFlashColor(Color.white, 0.1f);
				SemiUISpringScale(0.5f, 0.2f, 0.1f);
			}
			energyPrev = (int)PlayerController.instance.EnergyCurrent;
		}
		if (maxEnergyPrevOffset != num)
		{
			maxEnergyPrevOffset = num;
			SemiUISpringScale(0.5f, 0.2f, 0.1f);
		}
	}
}
