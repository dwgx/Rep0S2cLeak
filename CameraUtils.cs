using UnityEngine;

public class CameraUtils : MonoBehaviour
{
	public static CameraUtils Instance;

	public Camera MainCamera;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		MainCamera = Camera.main;
	}
}
