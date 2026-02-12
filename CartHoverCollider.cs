using UnityEngine;

public class CartHoverCollider : MonoBehaviour
{
	[HideInInspector]
	public bool cartHover;

	private void OnTriggerStay(Collider other)
	{
		cartHover = true;
	}

	private void OnTriggerExit(Collider other)
	{
		cartHover = false;
	}
}
