using UnityEngine;

public class VideoGreenScreen : MonoBehaviour
{
	private float distFromPlayer = 2f;

	public Transform greenScreenFloor;

	public static VideoGreenScreen instance;

	private void Start()
	{
		instance = this;
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.KeypadMultiply))
		{
			if (greenScreenFloor.GetComponent<Renderer>().material.color == Color.green)
			{
				greenScreenFloor.GetComponent<Renderer>().material.color = Color.blue;
				GetComponentInChildren<Renderer>().material.color = Color.blue;
			}
			else
			{
				greenScreenFloor.GetComponent<Renderer>().material.color = Color.green;
				GetComponentInChildren<Renderer>().material.color = Color.green;
			}
		}
		PostProcessing.Instance.VignetteOverride(Color.black, 0f, 1f, 10f, 10f, 0.2f, base.gameObject);
		PostProcessing.Instance.BloomDisable(0.2f);
		PostProcessing.Instance.GrainDisable(0.2f);
		GameplayManager.instance.OverrideCameraAnimation(0f, 0.2f);
		GameplayManager.instance.OverrideCameraNoise(0f, 0.2f);
		GameplayManager.instance.OverrideCameraShake(0f, 0.2f);
		RaycastHit[] array = Physics.RaycastAll(base.transform.position, Vector3.down, 100f, LayerMask.GetMask("Default"));
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			if (raycastHit.collider.CompareTag("Wall"))
			{
				greenScreenFloor.position = raycastHit.point + Vector3.up * 0.01f;
			}
		}
		Transform headLookAtTransform = PlayerAvatar.instance.playerAvatarVisuals.headLookAtTransform;
		base.transform.LookAt(headLookAtTransform);
		base.transform.position = headLookAtTransform.position + headLookAtTransform.forward * distFromPlayer;
		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			distFromPlayer -= 0.1f;
		}
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			distFromPlayer += 0.1f;
		}
		distFromPlayer = Mathf.Clamp(distFromPlayer, 1f, 10f);
	}
}
