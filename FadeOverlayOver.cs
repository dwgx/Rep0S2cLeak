using UnityEngine;
using UnityEngine.UI;

public class FadeOverlayOver : MonoBehaviour
{
	public static FadeOverlayOver Instance;

	public Image Image;

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		if (GameDirector.instance.currentState == GameDirector.gameState.Load || GameDirector.instance.currentState == GameDirector.gameState.End || GameDirector.instance.currentState == GameDirector.gameState.EndWait)
		{
			Image.color = new Color32(0, 0, 0, byte.MaxValue);
		}
		else
		{
			Image.color = new Color32(0, 0, 0, 0);
		}
	}
}
