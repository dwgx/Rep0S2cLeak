public class InventoryUI : SemiUI
{
	public static InventoryUI instance;

	private void Awake()
	{
		instance = this;
	}

	protected override void Start()
	{
		base.Start();
		uiText = null;
	}

	protected override void Update()
	{
		if (LevelGenerator.Instance.Generated)
		{
			base.Update();
			if (SemiFunc.RunIsShop())
			{
				Hide();
			}
		}
	}
}
