using TMPro;
using UnityEngine;

public class ArenaMessageUI : SemiUI
{
	private TextMeshProUGUI Text;

	public static ArenaMessageUI instance;

	private string messagePrev = "prev";

	private float messageTimer;

	private GameObject bigMessageEmojiObject;

	private TextMeshProUGUI emojiText;

	private VertexGradient originalGradient;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
		Text.text = "";
		originalGradient = Text.colorGradient;
	}

	public void ArenaText(string message)
	{
		if (message != Text.text)
		{
			messageTimer = 0f;
			SemiUIResetAllShakeEffects();
		}
		messageTimer = 0.1f;
		if (message != messagePrev)
		{
			Text.text = message;
			SemiUISpringShakeY(5f, 5f, 0.3f);
			SemiUISpringScale(0.1f, 2.5f, 0.2f);
			messagePrev = message;
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
