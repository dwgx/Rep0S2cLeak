using UnityEngine;

public class CollisionFree : MonoBehaviour
{
	[HideInInspector]
	public bool colliding;

	private void OnTriggerExit(Collider other)
	{
		colliding = false;
	}

	private void OnTriggerStay(Collider other)
	{
		colliding = true;
	}
}
