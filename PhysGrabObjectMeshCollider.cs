using UnityEngine;

public class PhysGrabObjectMeshCollider : MonoBehaviour
{
	public bool showGizmo = true;

	[Range(0.2f, 1f)]
	public float gizmoAlpha = 1f;

	private void OnDrawGizmos()
	{
		if (showGizmo)
		{
			Mesh sharedMesh = GetComponent<MeshCollider>().sharedMesh;
			if (sharedMesh != null)
			{
				Gizmos.color = new Color(0f, 1f, 0f, 0.2f * gizmoAlpha);
				Gizmos.DrawMesh(sharedMesh, base.transform.position, base.transform.rotation, base.transform.localScale);
				Gizmos.color = new Color(0f, 1f, 0f, 0.4f * gizmoAlpha);
				Gizmos.DrawWireMesh(sharedMesh, base.transform.position, base.transform.rotation, base.transform.localScale);
			}
		}
	}
}
