using TMPro;
using UnityEngine;

public class BigMessageUI : SemiUI
{
	private TextMeshProUGUI Text;

	public static BigMessageUI instance;

	private string bigMessagePrev = "prev";

	private string bigMessage = "big";

	private Color bigMessageColor = Color.white;

	private Color bigMessageFlashColor = Color.white;

	private float bigMessageTimer;

	private string bigMessageEmoji = "";

	private GameObject bigMessageEmojiObject;

	private TextMeshProUGUI emojiText;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
		bigMessageEmojiObject = base.transform.Find("Big Message Emoji").gameObject;
		emojiText = bigMessageEmojiObject.GetComponent<TextMeshProUGUI>();
	}

	public void BigMessage(string message, string emoji, float size, Color colorMain, Color colorFlash)
	{
		bigMessageColor = colorMain;
		bigMessageFlashColor = colorFlash;
		bigMessageTimer = 0.2f;
		bigMessage = message;
		if (bigMessage != bigMessagePrev)
		{
			Text.fontSize = size;
			Text.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, bigMessageColor);
			Text.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, bigMessageColor);
			Text.color = bigMessageColor;
			Text.text = bigMessage;
			bigMessageEmoji = SemiFunc.EmojiText(emoji);
			emojiText.text = bigMessageEmoji;
			SemiUISpringShakeY(20f, 10f, 0.3f);
			SemiUITextFlashColor(bigMessageFlashColor, 0.2f);
			SemiUISpringScale(0.4f, 5f, 0.2f);
			bigMessagePrev = bigMessage;
		}
	}

	protected override void Update()
	{
		base.Update();
		bigMessageEmojiObject.SetActive(Text.enabled);
		if (bigMessageTimer > 0f)
		{
			bigMessageTimer -= Time.deltaTime;
			return;
		}
		bigMessage = "big";
		bigMessagePrev = "prev";
		Hide();
	}
}
