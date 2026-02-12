using System.Collections;
using UnityEngine;

public class TrapPropSwitch : MonoBehaviour
{
	public GameObject TrapParent;

	public GameObject PropParent;

	[HideInInspector]
	public string DebugState = "...";

	[HideInInspector]
	public bool DebugSwitch;

	private void Start()
	{
		TrapParent.SetActive(value: true);
		PropParent.SetActive(value: true);
		StartCoroutine(Setup());
	}

	public IEnumerator Setup()
	{
		while (!TrapDirector.instance.TrapListUpdated)
		{
			yield return new WaitForSeconds(0.5f);
		}
		yield return new WaitForSeconds(0.5f);
		Trap componentInChildren = GetComponentInChildren<Trap>();
		if (componentInChildren != null && componentInChildren.gameObject.activeSelf)
		{
			PropParent.gameObject.SetActive(value: false);
		}
	}

	public void DebugToggle()
	{
		if (DebugSwitch)
		{
			DebugSwitch = false;
			DebugState = "Trap Active";
			if (TrapParent != null)
			{
				TrapParent.SetActive(value: true);
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
			if (TrapParent != null)
			{
				TrapParent.SetActive(value: false);
			}
		}
	}
}
