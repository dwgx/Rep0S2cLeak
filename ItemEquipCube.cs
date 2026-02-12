using UnityEngine;

public class ItemEquipCube : MonoBehaviour
{
	[HideInInspector]
	public bool isObstructed;

	private float obstructedTimer;

	private void OnTriggerStay(Collider other)
	{
		isObstructed = true;
		obstructedTimer = 0.1f;
	}

	private void Update()
	{
		if (obstructedTimer > 0f)
		{
			obstructedTimer -= Time.deltaTime;
		}
		else
		{
			isObstructed = false;
		}
	}
}
