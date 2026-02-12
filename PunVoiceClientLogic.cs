using UnityEngine;

public class PunVoiceClientLogic : MonoBehaviour
{
	public static PunVoiceClientLogic instance;

	private void Awake()
	{
		Debug.Log("PunVoiceClientLogic Awake");
		if (!instance)
		{
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
