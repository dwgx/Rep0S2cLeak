using UnityEngine;

[ExecuteAlways]
public class DeathPitForceEditor : MonoBehaviour
{
	public DeathPitForce deathPitForce;

	private void Update()
	{
		if (Application.isEditor)
		{
			deathPitForce.forceDirectionObject.transform.localPosition = deathPitForce.boxCollider.center;
		}
	}
}
