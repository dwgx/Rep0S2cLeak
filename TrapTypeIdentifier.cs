using Photon.Pun;
using UnityEngine;

public class TrapTypeIdentifier : MonoBehaviour
{
	public string trapType;

	[Header("Must add the trigger!")]
	public GameObject Trigger;

	public bool OnlyRemoveTrigger;

	[HideInInspector]
	public bool TriggerRemoved;

	private void Start()
	{
		Module componentInParent = GetComponentInParent<Module>();
		Debug.LogError("Remove + '" + trapType + "' in '" + componentInParent.gameObject.name + "'");
		TrapDirector.instance.TrapList.Add(base.gameObject);
	}

	[PunRPC]
	private void DestroyTrigger()
	{
		Object.Destroy(Trigger);
	}

	[PunRPC]
	private void DestroyTrap()
	{
		Object.Destroy(base.gameObject);
	}
}
