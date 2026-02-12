using UnityEngine;

public class NavMeshBox : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		Gizmos.color = new Color(0.4f, 0.19f, 1f);
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(component.center, component.size);
		Gizmos.color = new Color(0.9f, 0.22f, 1f, 0.2f);
		Gizmos.DrawCube(component.center, component.size);
	}
}
