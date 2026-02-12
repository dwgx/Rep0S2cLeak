using UnityEngine;

public class ValuablePropSwitch : MonoBehaviour
{
	public GameObject ValuableParent;

	public GameObject PropParent;

	internal bool SetupComplete;

	[HideInInspector]
	public string DebugState = "...";

	[HideInInspector]
	public bool DebugSwitch;

	[HideInInspector]
	public string ChildValuableString = "...";

	private void Awake()
	{
		if (!SetupComplete)
		{
			ValuableParent.SetActive(value: true);
			PropParent.SetActive(value: false);
		}
	}

	public void Setup()
	{
		ValuablePropSwitch[] componentsInParent = base.gameObject.GetComponentsInParent<ValuablePropSwitch>(includeInactive: true);
		for (int i = 0; i < componentsInParent.Length; i++)
		{
			if (componentsInParent[i] != this)
			{
				Debug.LogError("ValuablePropSwitch: Switches inside switches is not supported...", base.gameObject);
			}
		}
		if (!base.gameObject.GetComponentInChildren<ValuableVolume>(includeInactive: true))
		{
			Debug.LogError(base.gameObject.GetComponentInParent<Module>().gameObject.name + "  |  ValuablePropSwitch: No ValuableVolume found in children...", base.gameObject);
			return;
		}
		bool flag = false;
		ValuableObject[] componentsInChildren = GetComponentsInChildren<ValuableObject>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].gameObject.activeSelf)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			PropParent.SetActive(value: false);
			ValuableParent.SetActive(value: true);
		}
		else
		{
			ValuableParent.SetActive(value: false);
			PropParent.SetActive(value: true);
		}
		SetupComplete = true;
	}

	public void DebugToggle()
	{
		if (DebugSwitch)
		{
			DebugSwitch = false;
			DebugState = "Valuable Active";
			if (ValuableParent != null)
			{
				ValuableParent.SetActive(value: true);
			}
			if (PropParent != null)
			{
				PropParent.SetActive(value: false);
			}
		}
		else
		{
			DebugSwitch = true;
			DebugState = "Prop Active";
			if (PropParent != null)
			{
				PropParent.SetActive(value: true);
			}
			if (ValuableParent != null)
			{
				ValuableParent.SetActive(value: false);
			}
		}
	}
}
