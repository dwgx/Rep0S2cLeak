using UnityEngine;

public class CursorManager : MonoBehaviour
{
	public static CursorManager instance;

	private float unlockTimer;

	private void Awake()
	{
		Cursor.visible = false;
		if (!instance)
		{
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
		if (unlockTimer > 0f)
		{
			if ((bool)MenuCursor.instance)
			{
				MenuCursor.instance.Show();
			}
			unlockTimer -= Time.deltaTime;
		}
		else if (unlockTimer != -1234f)
		{
			Cursor.lockState = CursorLockMode.Locked;
			unlockTimer = -1234f;
		}
	}

	public void Unlock(float _time)
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = false;
		unlockTimer = _time;
	}
}
