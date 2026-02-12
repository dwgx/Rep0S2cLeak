using Photon.Pun;
using UnityEngine;

public class RoachOrbit : MonoBehaviour
{
	[Header("Roach Smash")]
	public GameObject roachSmashPrefab;

	public float radius = 5f;

	public float rotationSpeed = 1f;

	public float noiseScale = 1f;

	public float noiseSpeed = 0.5f;

	public float noiseScale2 = 0.5f;

	public float noiseSpeed2 = 1f;

	private Vector3 startPosition;

	private float noiseOffsetX;

	private float noiseOffsetZ;

	private float noiseOffsetX2;

	private float noiseOffsetZ2;

	private PhotonView photonView;

	public Sound squashSound;

	public Sound roachLoopSound;

	private void Start()
	{
		startPosition = base.transform.position;
		noiseOffsetX = base.transform.position.x;
		noiseOffsetZ = base.transform.position.z;
		noiseOffsetX2 = base.transform.position.x * 1.5f;
		noiseOffsetZ2 = base.transform.position.z * 1.5f;
		photonView = GetComponent<PhotonView>();
	}

	[PunRPC]
	private void SquashRPC()
	{
		squashSound.Play(base.transform.position);
		Object.Instantiate(roachSmashPrefab, base.transform.position, Quaternion.identity);
		Object.Destroy(base.gameObject);
	}

	public void Squash()
	{
		if (GameManager.instance.gameMode == 0)
		{
			squashSound.Play(base.transform.position);
			Object.Instantiate(roachSmashPrefab, base.transform.position, Quaternion.identity);
			Object.Destroy(base.gameObject);
		}
		else
		{
			photonView.RPC("SquashRPC", RpcTarget.AllBuffered);
		}
	}

	private void Update()
	{
		roachLoopSound.PlayLoop(playing: true, 1f, 2f);
		float num = ((GameManager.instance.gameMode != 0) ? NetworkManager.instance.gameTime : Time.time);
		float num2 = num * noiseSpeed;
		float num3 = Mathf.PerlinNoise(noiseOffsetX + num2 * noiseScale, 0f) * 2f - 1f;
		float num4 = Mathf.PerlinNoise(0f, noiseOffsetZ + num2 * noiseScale) * 2f - 1f;
		num2 = num * noiseSpeed2;
		float num5 = Mathf.PerlinNoise(noiseOffsetX2 + num2 * noiseScale2, 0f) * 2f - 1f;
		float num6 = Mathf.PerlinNoise(0f, noiseOffsetZ2 + num2 * noiseScale2) * 2f - 1f;
		float x = (num3 + num5) / 2f;
		float z = (num4 + num6) / 2f;
		Vector3 vector = startPosition + new Vector3(x, 0f, z) * radius;
		Vector3 vector2 = vector - base.transform.position;
		base.transform.position = vector;
		if (vector2 != Vector3.zero)
		{
			Quaternion quaternion = Quaternion.LookRotation(vector2);
			Quaternion quaternion2 = Quaternion.Euler(0f, -90f, 0f);
			quaternion *= quaternion2;
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, rotationSpeed * Time.deltaTime);
		}
	}
}
