using UnityEngine;
using UnityEngine.UI;

public class InventoryBattery : MonoBehaviour
{
	public int inventorySpot;

	private int batteryState;

	internal RawImage batteryImage;

	private float redBlinkTimer;

	private float batteryShowTimer;

	private Vector3 originalLocalScale;

	private void Start()
	{
		batteryState = 6;
		batteryImage = GetComponent<RawImage>();
		batteryImage.enabled = false;
		originalLocalScale = base.transform.localScale;
		base.transform.localScale = Vector3.zero;
	}

	public void BatteryFetch()
	{
		if ((bool)Inventory.instance && (bool)batteryImage)
		{
			int batteryStateFromInventorySpot = Inventory.instance.GetBatteryStateFromInventorySpot(inventorySpot);
			if (batteryStateFromInventorySpot != -1 && redBlinkTimer == 0f)
			{
				batteryState = batteryStateFromInventorySpot;
				batteryImage.color = new Color(1f, 1f, 1f, 1f);
			}
		}
	}

	public void BatteryShow()
	{
		batteryShowTimer = 0.2f;
	}

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (batteryShowTimer > 0f)
		{
			batteryShowTimer -= Time.deltaTime;
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, originalLocalScale, Time.deltaTime * 30f);
			batteryImage.enabled = true;
		}
		else if (batteryImage.enabled)
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, Vector3.zero, Time.deltaTime * 30f);
			if (base.transform.localScale.x < 0.01f)
			{
				base.transform.localScale = Vector3.zero;
				batteryImage.enabled = false;
			}
		}
		if (batteryState == 0)
		{
			batteryImage.uvRect = new Rect(-0.006f, -0.921f, 0.4f, 0.2f);
		}
		if (batteryState == 1)
		{
			batteryImage.uvRect = new Rect(0.369f, -0.687f, 0.4f, 0.2f);
		}
		if (batteryState == 2)
		{
			batteryImage.uvRect = new Rect(-0.006f, -0.687f, 0.4f, 0.2f);
		}
		if (batteryState == 3)
		{
			batteryImage.uvRect = new Rect(0.369f, -0.4523f, 0.4f, 0.2f);
		}
		if (batteryState == 4)
		{
			batteryImage.uvRect = new Rect(-0.006f, -0.4523f, 0.4f, 0.2f);
		}
		if (batteryState == 5)
		{
			batteryImage.uvRect = new Rect(0.369f, -0.218f, 0.4f, 0.2f);
		}
		if (batteryState == 6)
		{
			batteryImage.uvRect = new Rect(-0.006f, -0.218f, 0.4f, 0.2f);
		}
		if (batteryState > 6)
		{
			batteryState = 6;
		}
		if (batteryState < 0)
		{
			batteryState = 0;
		}
		if (batteryState <= 3 && SemiFunc.RunIsLobby())
		{
			redBlinkTimer += Time.deltaTime;
			if (redBlinkTimer > 0.5f)
			{
				batteryImage.color = new Color(1f, 0f, 0f, 1f);
			}
			else
			{
				batteryImage.color = new Color(1f, 1f, 1f, 1f);
			}
			if (redBlinkTimer > 1f)
			{
				redBlinkTimer = 0f;
			}
		}
	}
}
