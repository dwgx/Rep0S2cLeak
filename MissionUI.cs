using TMPro;
using UnityEngine;

public class MissionUI : SemiUI
{
	internal TextMeshProUGUI Text;

	public static MissionUI instance;

	private string messagePrev = "prev";

	private Color bigMessageColor = Color.white;

	private Color bigMessageFlashColor = Color.white;

	private float messageTimer;

	private GameObject bigMessageEmojiObject;

	private TextMeshProUGUI emojiText;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
		Text.text = "";
	}

	public void MissionText(string message, Color colorMain, Color colorFlash, float time = 3f)
	{
		if (!(messageTimer > 0f))
		{
			bigMessageColor = colorMain;
			bigMessageFlashColor = colorFlash;
			messageTimer = time;
			message = "<b>FOCUS > </b>" + message;
			if (message != messagePrev)
			{
				Text.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, bigMessageColor);
				Text.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, bigMessageColor);
				Text.color = bigMessageColor;
				Text.text = message;
				SemiUISpringShakeY(20f, 10f, 0.3f);
				SemiUITextFlashColor(bigMessageFlashColor, 0.2f);
				SemiUISpringScale(0.4f, 5f, 0.2f);
				messagePrev = message;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (messageTimer > 0f)
		{
			messageTimer -= Time.deltaTime;
			return;
		}
		messagePrev = "prev";
		Hide();
	}
}
