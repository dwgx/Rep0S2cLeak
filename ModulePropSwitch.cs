using UnityEngine;

public class ModulePropSwitch : MonoBehaviour
{
	public enum Connection
	{
		Top,
		Right,
		Bot,
		Left
	}

	internal Module Module;

	public GameObject ConnectedParent;

	public GameObject NotConnectedParent;

	private bool Connected;

	[Space(20f)]
	public Connection ConnectionSide;

	[HideInInspector]
	public string DebugState = "...";

	[HideInInspector]
	public bool DebugSwitch;

	public void Setup()
	{
		int num = 0;
		while ((float)num < Module.transform.localRotation.eulerAngles.y)
		{
			num += 90;
			ConnectionSide++;
			if (ConnectionSide > Connection.Left)
			{
				ConnectionSide = Connection.Top;
			}
		}
		if (ConnectionSide == Connection.Top && Module.ConnectingTop)
		{
			Connected = true;
		}
		else if (ConnectionSide == Connection.Right && Module.ConnectingRight)
		{
			Connected = true;
		}
		else if (ConnectionSide == Connection.Bot && Module.ConnectingBottom)
		{
			Connected = true;
		}
		else if (ConnectionSide == Connection.Left && Module.ConnectingLeft)
		{
			Connected = true;
		}
		if (Connected)
		{
			NotConnectedParent.SetActive(value: false);
			ConnectedParent.SetActive(value: true);
		}
		else
		{
			NotConnectedParent.SetActive(value: true);
			ConnectedParent.SetActive(value: false);
		}
	}

	public void Toggle()
	{
		if (DebugSwitch)
		{
			DebugSwitch = false;
			DebugState = "Connected";
			NotConnectedParent.SetActive(value: false);
			ConnectedParent.SetActive(value: true);
		}
		else
		{
			DebugSwitch = true;
			DebugState = "Not Connected";
			NotConnectedParent.SetActive(value: true);
			ConnectedParent.SetActive(value: false);
		}
	}
}
