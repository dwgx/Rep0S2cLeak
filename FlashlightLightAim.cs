using UnityEngine;

public class FlashlightLightAim : MonoBehaviour
{
	public PlayerAvatar playerAvatar;

	public Vector3 clientAimPoint;

	private Vector3 clientAimPointCurrent;

	private Light lightComponent;

	private bool setBias;

	private void Start()
	{
		lightComponent = GetComponent<Light>();
	}

	private void Update()
	{
		clientAimPointCurrent = Vector3.Lerp(clientAimPointCurrent, clientAimPoint, Time.deltaTime * 20f);
		RaycastHit hitInfo;
		if (!playerAvatar.isLocal)
		{
			Vector3 direction = clientAimPointCurrent - base.transform.position;
			direction = SemiFunc.ClampDirection(direction, base.transform.parent.forward, 45f);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
		}
		else if (Physics.Raycast(base.transform.position, base.transform.forward, out hitInfo, 100f, SemiFunc.LayerMaskGetVisionObstruct()) && !hitInfo.transform.GetComponentInParent<PlayerController>())
		{
			clientAimPoint = hitInfo.point;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
		Gizmos.DrawSphere(clientAimPointCurrent, 0.1f);
	}
}
