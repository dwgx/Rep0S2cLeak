using Photon.Pun;
using UnityEngine;

public class ValuableWizardStaff : MonoBehaviour
{
	private PhotonView photonView;

	private float laserTimer;

	public SemiLaser semiLaser;

	public Transform laserTransform;

	private Rigidbody rb;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (laserTimer > 0f)
		{
			laserTimer -= Time.deltaTime;
			Vector3 endPosition = laserTransform.position + laserTransform.forward * 15f;
			bool isHitting = false;
			if (Physics.Raycast(laserTransform.position, laserTransform.forward, out var hitInfo, 15f, SemiFunc.LayerMaskGetVisionObstruct()))
			{
				endPosition = hitInfo.point;
				isHitting = true;
			}
			semiLaser.LaserActive(laserTransform.position, endPosition, isHitting);
		}
	}

	private void FixedUpdate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && laserTimer > 0f)
		{
			Vector3 force = -laserTransform.forward * 1000f * Time.fixedDeltaTime;
			rb.AddForce(force, ForceMode.Force);
		}
	}

	public void StaffLaser()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			float num = Random.Range(1f, 4f);
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("StaffLaserRPC", RpcTarget.All, num);
			}
			else
			{
				StaffLaserRPC(num);
			}
		}
	}

	[PunRPC]
	public void StaffLaserRPC(float _time)
	{
		laserTimer = _time;
	}
}
