using UnityEngine;
using UnityEngine.UI;

public class TumbleWingsUI : SemiUI
{
	public RawImage imageBar;

	public RectTransform rectTransformLeftWing;

	public RectTransform rectTransformRightWing;

	public static TumbleWingsUI instance;

	internal ItemUpgradePlayerTumbleWingsLogic itemUpgradePlayerTumbleWingsLogic;

	protected override void Start()
	{
		base.Start();
		instance = this;
	}

	protected override void Update()
	{
		base.Update();
		if ((bool)PlayerAvatar.instance)
		{
			if (!PlayerAvatar.instance.isTumbling || PlayerAvatar.instance.isDisabled)
			{
				Hide();
			}
			if (!PlayerAvatar.instance.upgradeTumbleWingsVisualsActive || ((bool)itemUpgradePlayerTumbleWingsLogic && itemUpgradePlayerTumbleWingsLogic.tumbleWingTimer < 0f))
			{
				Hide();
			}
			else
			{
				imageBar.rectTransform.localScale = new Vector3(itemUpgradePlayerTumbleWingsLogic.tumbleWingTimer / 1f, 1f, 1f);
			}
			float num = -54f;
			float num2 = 50f;
			float num3 = Mathf.Sin(Time.time * num2) * num;
			rectTransformLeftWing.localRotation = Quaternion.Euler(0f, 0f, -24f - num3);
			rectTransformRightWing.localRotation = Quaternion.Euler(0f, 0f, 24f + num3);
		}
	}
}
