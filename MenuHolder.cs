using UnityEngine;

public class MenuHolder : MonoBehaviour
{
	public static MenuHolder instance;

	private void Start()
	{
		instance = this;
	}

	private void Update()
	{
	}
}
