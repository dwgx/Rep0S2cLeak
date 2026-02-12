using UnityEngine;

public class CameraOverlay : MonoBehaviour
{
	internal Camera overlayCamera;

	public static CameraOverlay instance;

	private void Start()
	{
		overlayCamera = GetComponent<Camera>();
		instance = this;
	}
}
