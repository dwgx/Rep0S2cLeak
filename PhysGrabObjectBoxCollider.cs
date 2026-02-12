using UnityEngine;

public class PhysGrabObjectBoxCollider : MonoBehaviour
{
	public bool drawGizmos = true;

	[Range(0.2f, 1f)]
	public float gizmoTransparency = 1f;

	public bool unEquipCollider;

	private void Start()
	{
		if (unEquipCollider)
		{
			BoxCollider component = GetComponent<BoxCollider>();
			if ((bool)component)
			{
				component.enabled = false;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (!drawGizmos)
		{
			return;
		}
		BoxCollider component = GetComponent<BoxCollider>();
		if (!(component == null))
		{
			Color color = new Color(0f, 1f, 0f, 1f * gizmoTransparency);
			Color color2 = new Color(0f, 1f, 0f, 0.2f * gizmoTransparency);
			if (unEquipCollider)
			{
				color2 = (color = new Color(0f, 0.5f, 0f, 1f * gizmoTransparency));
				color2.a = 0.2f * gizmoTransparency;
			}
			Gizmos.color = color;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(component.center, component.size);
			Gizmos.color = color2;
			Gizmos.DrawCube(component.center, component.size);
			Gizmos.matrix = Matrix4x4.identity;
		}
	}
}
