using UnityEngine;

public class DebugCube : MonoBehaviour
{
	public Transform gizmoTransform;

	internal Color color;

	private void OnDrawGizmos()
	{
		Gizmos.color = color;
		Gizmos.matrix = gizmoTransform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		Gizmos.color = new Color(color.r, color.g, color.b, 0.2f);
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
	}
}
