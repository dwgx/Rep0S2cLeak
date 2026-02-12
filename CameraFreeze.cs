using System.Collections.Generic;
using UnityEngine;

public class CameraFreeze : MonoBehaviour
{
	public static CameraFreeze instance;

	public List<Camera> cameras = new List<Camera>();

	private float timer;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		if (!(timer > 0f))
		{
			return;
		}
		timer -= Time.deltaTime;
		if (!(timer <= 0f))
		{
			return;
		}
		foreach (Camera camera in cameras)
		{
			camera.enabled = true;
		}
	}

	public static void Freeze(float _time)
	{
		if (_time <= 0f)
		{
			foreach (Camera camera in instance.cameras)
			{
				camera.enabled = true;
			}
			instance.timer = _time;
			return;
		}
		if (instance.timer <= 0f)
		{
			foreach (Camera camera2 in instance.cameras)
			{
				camera2.enabled = false;
			}
		}
		instance.timer = _time;
	}

	public static bool IsFrozen()
	{
		return instance.timer > 0f;
	}
}
