using UnityEngine;

public class PowerCrystal : MonoBehaviour
{
	private void Start()
	{
		ItemManager.instance.powerCrystals.Add(GetComponent<PhysGrabObject>());
	}
}
