using UnityEngine;

public class DeathPitForce : MonoBehaviour
{
	public float forceMagnitude = 10f;

	[Space]
	public GameObject deathPitForceEditor;

	public GameObject forceDirectionObject;

	public BoxCollider boxCollider;

	private void Awake()
	{
		deathPitForceEditor.SetActive(value: false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		DeathPitSaveEffect deathPitSaveEffect = other.GetComponentInParent<DeathPitSaveEffect>();
		if (!deathPitSaveEffect)
		{
			Transform parent = other.transform.parent;
			if ((bool)parent)
			{
				deathPitSaveEffect = parent.GetComponentInChildren<DeathPitSaveEffect>();
			}
		}
		if ((bool)deathPitSaveEffect && deathPitSaveEffect.deathPitForceTimer <= 0f && deathPitSaveEffect.physGrabObject.rb.velocity.y > 1f)
		{
			deathPitSaveEffect.deathPitForceTimer = 2f;
			deathPitSaveEffect.physGrabObject.rb.AddForce(forceDirectionObject.transform.forward * forceMagnitude * deathPitSaveEffect.physGrabObject.rb.mass, ForceMode.Impulse);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(0f, 1f, 0.98f);
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
		Gizmos.color = new Color(0f, 1f, 0.85f, 0.2f);
		Gizmos.DrawCube(boxCollider.center, boxCollider.size);
		Gizmos.color = Color.white;
		Gizmos.matrix = Matrix4x4.identity;
		Vector3 forward = forceDirectionObject.transform.forward;
		Vector3 center = boxCollider.bounds.center;
		Vector3 vector = center + forward * 0.5f;
		Vector3 normalized = Vector3.Cross(forward, Vector3.up).normalized;
		Gizmos.DrawLine(center, vector);
		Gizmos.DrawLine(vector, vector + Vector3.LerpUnclamped(-forward, -normalized, 0.5f) * 0.25f);
		Gizmos.DrawLine(vector, vector + Vector3.LerpUnclamped(-forward, normalized, 0.5f) * 0.25f);
	}
}
