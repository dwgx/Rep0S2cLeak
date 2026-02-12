using UnityEngine;

public class EnemyHeadTilt : MonoBehaviour
{
	public float Amount = -500f;

	public float MaxAmount = 20f;

	public float Speed = 10f;

	private Vector3 ForwardPrev;

	private void Update()
	{
		float z = Mathf.Clamp(Vector3.Cross(ForwardPrev, base.transform.forward).y * Amount, 0f - MaxAmount, MaxAmount);
		base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, Quaternion.Euler(0f, 0f, z), Speed * Time.deltaTime);
		ForwardPrev = base.transform.forward;
	}
}
