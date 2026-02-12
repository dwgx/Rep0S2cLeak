using UnityEngine;

public class PhysGrabObjectCapsuleCollider : MonoBehaviour
{
	private Mesh capsuleMesh;

	public bool drawGizmos = true;

	[Range(0.2f, 1f)]
	public float gizmoTransparency = 1f;

	private void OnDrawGizmos()
	{
		if (drawGizmos)
		{
			if (capsuleMesh == null)
			{
				capsuleMesh = CreateCapsuleMesh();
			}
			Gizmos.color = new Color(0f, 1f, 0f, 0.4f * gizmoTransparency);
			Vector3 localScale = base.transform.localScale;
			Vector3 position = base.transform.position;
			Quaternion rotation = base.transform.rotation;
			Gizmos.color = new Color(0f, 1f, 0f, 0.2f * gizmoTransparency);
			Gizmos.DrawMesh(capsuleMesh, position, rotation, localScale);
			float num = Mathf.Min(localScale.x, localScale.z) * 0.5f;
			float num2 = localScale.y - num * 2f;
			Gizmos.color = new Color(0f, 1f, 0f, 0.4f * gizmoTransparency);
			Gizmos.DrawWireSphere(position + rotation * Vector3.up * (num2 + num), num);
			Gizmos.DrawWireSphere(position + rotation * Vector3.down * (num2 + num), num);
			Gizmos.DrawLine(position + rotation * Vector3.up * (num2 + num) + rotation * Vector3.right * num, position + rotation * Vector3.down * (num2 + num) + rotation * Vector3.right * num);
			Gizmos.DrawLine(position + rotation * Vector3.up * (num2 + num) + rotation * Vector3.left * num, position + rotation * Vector3.down * (num2 + num) + rotation * Vector3.left * num);
			Gizmos.DrawLine(position + rotation * Vector3.up * (num2 + num) + rotation * Vector3.forward * num, position + rotation * Vector3.down * (num2 + num) + rotation * Vector3.forward * num);
			Gizmos.DrawLine(position + rotation * Vector3.up * (num2 + num) + rotation * Vector3.back * num, position + rotation * Vector3.down * (num2 + num) + rotation * Vector3.back * num);
		}
	}

	private Mesh CreateCapsuleMesh()
	{
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
		Mesh sharedMesh = obj.GetComponent<MeshFilter>().sharedMesh;
		Object.DestroyImmediate(obj);
		return sharedMesh;
	}
}
