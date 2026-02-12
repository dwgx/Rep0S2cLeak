using UnityEngine;

public class PhysGrabObjectSphereCollider : MonoBehaviour
{
	public bool drawGizmos = true;

	[Range(0.2f, 1f)]
	public float gizmoTransparency = 1f;

	private void OnDrawGizmos()
	{
		if (drawGizmos)
		{
			Gizmos.color = new Color(0f, 1f, 0f, 0.5f * gizmoTransparency);
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.localScale);
			Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
			Gizmos.color = new Color(0f, 1f, 0f, 0.2f * gizmoTransparency);
			Gizmos.DrawSphere(Vector3.zero, 0.5f);
			Gizmos.matrix = Matrix4x4.identity;
		}
	}
}
