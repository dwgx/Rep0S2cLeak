using UnityEngine;

public class DebugSphere : MonoBehaviour
{
	public Transform gizmoTransform;

	internal Color color = Color.white;

	private void OnDrawGizmos()
	{
		Gizmos.color = color;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireSphere(Vector3.zero, base.transform.localScale.x);
		Gizmos.color = new Color(color.r, color.g, color.b, 0.2f);
		Gizmos.DrawSphere(Vector3.zero, base.transform.localScale.x);
		Gizmos.color = color;
		Gizmos.matrix = gizmoTransform.localToWorldMatrix;
		Gizmos.DrawWireSphere(Vector3.zero, base.transform.localScale.x);
	}
}
