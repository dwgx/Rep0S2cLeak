using UnityEngine;

public class GizmoBall : MonoBehaviour
{
	public Color color = Color.red;

	public float radius = 0.5f;

	public Vector3 offset;

	private void OnDrawGizmos()
	{
		color.a = 0.5f;
		Gizmos.color = color;
		Gizmos.DrawSphere(base.transform.position + offset, radius);
		color.a = 1f;
		Gizmos.color = color;
		Gizmos.DrawWireSphere(base.transform.position + offset, radius);
	}
}
