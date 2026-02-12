public class ToggleMuteUI : SemiUI
{
	protected override void Update()
	{
		base.Update();
		if (!DataDirector.instance.toggleMute)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}
}
