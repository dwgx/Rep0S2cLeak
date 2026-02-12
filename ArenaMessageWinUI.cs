using TMPro;
using UnityEngine;

public class ArenaMessageWinUI : SemiUI
{
	private TextMeshProUGUI Text;

	public static ArenaMessageWinUI instance;

	private string messagePrev = "prev";

	private float messageTimer;

	private GameObject bigMessageEmojiObject;

	private TextMeshProUGUI emojiText;

	private VertexGradient originalGradient;

	public GameObject kingObject;

	public GameObject loserObject;

	public GameObject backgroundObject;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
		Text.text = "";
		originalGradient = Text.colorGradient;
	}

	public void ArenaText(string message, bool _kingCrowned = false)
	{
		if (message != Text.text)
		{
			messageTimer = 0f;
			SemiUIResetAllShakeEffects();
		}
		messageTimer = 0.1f;
		if (_kingCrowned)
		{
			if (!kingObject.activeSelf)
			{
				kingObject.SetActive(value: true);
				loserObject.transform.localPosition = new Vector3(0f, 3000f, 0f);
				loserObject.SetActive(value: false);
				backgroundObject.SetActive(value: true);
			}
		}
		else if (!loserObject.activeSelf)
		{
			loserObject.SetActive(value: true);
			kingObject.transform.localPosition = new Vector3(0f, 3000f, 0f);
			kingObject.SetActive(value: false);
			backgroundObject.SetActive(value: true);
		}
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
