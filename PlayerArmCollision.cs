using UnityEngine;

public class PlayerArmCollision : MonoBehaviour
{
	public bool Blocked;

	public float BlockDistance;

	private float BlockedTimer;

	private void OnCollisionStay(Collision other)
	{
		Blocked = true;
		BlockedTimer = 0.25f;
	}

	private void Update()
	{
		if (BlockedTimer <= 0f)
		{
			Blocked = false;
		}
		else
		{
			BlockedTimer -= Time.deltaTime;
		}
	}
}
