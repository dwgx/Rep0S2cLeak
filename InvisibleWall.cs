using UnityEngine;

public class InvisibleWall : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		Gizmos.color = new Color(0.1f, 1f, 0.4f);
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(component.center, component.size);
		Gizmos.color = new Color(0.1f, 1f, 0.4f, 0.5f);
		Gizmos.DrawCube(component.center, component.size);
	}
}
