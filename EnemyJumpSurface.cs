using UnityEngine;

public class EnemyJumpSurface : MonoBehaviour
{
	public Vector3 jumpDirection;

	private void OnDrawGizmos()
	{
		Vector3 position = base.transform.position;
		Vector3 vector = position + base.transform.TransformDirection(jumpDirection.normalized * 0.3f);
		Gizmos.DrawLine(position, vector);
		Gizmos.DrawWireSphere(vector, 0.03f);
	}
}
