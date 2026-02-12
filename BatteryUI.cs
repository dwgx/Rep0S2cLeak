using UnityEngine;

public class BatteryUI : SemiUI
{
	public static BatteryUI instance;

	private int batteryCurrentBars;

	private int batteryCurrenyBarsMax;

	private int batteryCurrentBarsPrev;

	private float redBlinkTimer;

	private float batteryShowTimer;

	public BatteryVisualLogic batteryVisualLogic;

	protected override void Start()
	{
		base.Start();
		instance = this;
		batteryCurrentBars = 6;
		base.transform.localScale = Vector3.zero;
	}

	private void BatteryLogic()
	{
		if (!LevelGenerator.Instance.Generated || !SemiFunc.FPSImpulse15() || !batteryVisualLogic)
		{
			return;
		}
		if (batteryCurrentBars > batteryCurrenyBarsMax)
		{
			batteryCurrentBars = batteryCurrenyBarsMax;
		}
		if (batteryCurrentBars < 0)
		{
			batteryCurrentBars = 0;
		}
		if (batteryCurrentBarsPrev != batteryCurrentBars)
		{
			batteryVisualLogic.BatteryBarsUpdate(batteryCurrentBars);
			batteryCurrentBarsPrev = batteryCurrentBars;
		}
		if (batteryCurrentBars <= batteryCurrenyBarsMax / 2 && SemiFunc.RunIsLobby())
		{
			float num = 1.8f;
			redBlinkTimer += Time.deltaTime * num;
			if (redBlinkTimer > 0.5f)
			{
				batteryVisualLogic.HideCurrentBar(_hide: true, Color.red);
			}
			else
			{
				batteryVisualLogic.BatteryColorMainReset();
			}
			if (redBlinkTimer > 1f)
			{
				redBlinkTimer = 0f;
				batteryVisualLogic.HideCurrentBar(_hide: false, Color.red);
			}
		}
	}

	protected override void Update()
	{
		if ((bool)batteryVisualLogic && SemiFunc.RunIsLobby() && batteryVisualLogic.currentBars <= batteryVisualLogic.batteryBars / 2)
		{
			batteryVisualLogic.OverrideChargeNeeded(0.2f);
		}
		if (Inventory.instance.InventorySpotsOccupied() > 0)
		{
			SemiUIScoot(new Vector2(0f, 5f));
		}
		else
		{
			SemiUIScoot(new Vector2(0f, -10f));
		}
		base.Update();
		if (SemiFunc.RunIsShop())
		{
			Hide();
		}
		else if (batteryShowTimer > 0f)
		{
			if (!PhysGrabber.instance.grabbed)
			{
				batteryShowTimer = 0f;
			}
			batteryShowTimer -= Time.deltaTime;
		}
		else
		{
			Hide();
		}
	}

	public void BatteryFetch()
	{
		if (!batteryVisualLogic || !PhysGrabber.instance.grabbed || !SemiFunc.FPSImpulse5())
		{
			return;
		}
		PhysGrabObject grabbedPhysGrabObject = PhysGrabber.instance.grabbedPhysGrabObject;
		if (!grabbedPhysGrabObject)
		{
			return;
		}
		ItemBattery component = grabbedPhysGrabObject.GetComponent<ItemBattery>();
		if ((bool)component && (!component.onlyShowWhenItemToggleIsOn || grabbedPhysGrabObject.GetComponent<ItemToggle>().toggleState))
		{
			int batteryLifeInt = component.batteryLifeInt;
			if (batteryLifeInt != -1)
			{
				batteryCurrentBars = batteryLifeInt;
				batteryCurrenyBarsMax = component.batteryBars;
			}
			batteryShowTimer = 1f;
		}
	}
}
