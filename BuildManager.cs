using UnityEngine;

public class BuildManager : MonoBehaviour
{
	public static BuildManager instance;

	public Version version;

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			Debug.Log("VERSION: " + version.title);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
