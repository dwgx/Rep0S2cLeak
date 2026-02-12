using UnityEngine;

public class SplashScreenUI : MonoBehaviour
{
	public static SplashScreenUI instance;

	public RectTransform semiworkTransform;

	public RectTransform warningTransform;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		if (LevelGenerator.Instance.Generated && !SplashScreen.instance)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
