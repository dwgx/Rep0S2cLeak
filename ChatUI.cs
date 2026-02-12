using TMPro;

public class ChatUI : SemiUI
{
	public static ChatUI instance;

	public TextMeshProUGUI chatText;

	protected override void Start()
	{
		base.Start();
		instance = this;
	}

	protected override void Update()
	{
		base.Update();
	}
}
