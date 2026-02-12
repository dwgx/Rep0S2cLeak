using UnityEngine;

public class CameraPosition : MonoBehaviour
{
	public static CameraPosition instance;

	public Transform playerTransform;

	public Vector3 playerOffset;

	public CameraTarget camController;

	public float positionSmooth = 2f;

	private float tumbleSetTimer;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		float num = positionSmooth;
		if (tumbleSetTimer > 0f)
		{
			num *= 0.5f;
			tumbleSetTimer -= Time.deltaTime;
		}
		Vector3 localPosition = playerTransform.localPosition + playerOffset;
		if (SemiFunc.MenuLevel() && (bool)CameraNoPlayerTarget.instance)
		{
			localPosition = CameraNoPlayerTarget.instance.transform.position;
		}
		base.transform.localPosition = Vector3.Slerp(base.transform.localPosition, localPosition, num * Time.deltaTime);
		base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, Quaternion.identity, num * Time.deltaTime);
		if (SemiFunc.MenuLevel())
		{
			base.transform.localPosition = localPosition;
		}
	}

	public void TumbleSet()
	{
		tumbleSetTimer = 0.5f;
	}
}
