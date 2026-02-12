using UnityEngine;

public class ToolFollowPush : MonoBehaviour
{
	private Vector3 PushPosition;

	private Quaternion PushRotation;

	public float SettleSpeed;

	public void Push(Vector3 position, Quaternion rotation, float amount)
	{
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, position, amount);
		base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, rotation, amount);
	}

	private void Update()
	{
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, Vector3.zero, SettleSpeed * Time.deltaTime);
		base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, Quaternion.identity, SettleSpeed * Time.deltaTime);
	}
}
