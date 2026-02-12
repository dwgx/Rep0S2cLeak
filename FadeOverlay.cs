using UnityEngine;
using UnityEngine.UI;

public class FadeOverlay : MonoBehaviour
{
	public static FadeOverlay Instance;

	public Image Image;

	[Space]
	public AnimationCurve IntroCurve;

	public float IntroSpeed;

	private float IntroLerp;

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		if (GameDirector.instance.currentState == GameDirector.gameState.Load || GameDirector.instance.currentState == GameDirector.gameState.Start || GameDirector.instance.currentState == GameDirector.gameState.Outro || GameDirector.instance.currentState == GameDirector.gameState.End || GameDirector.instance.currentState == GameDirector.gameState.EndWait)
		{
			Image.color = new Color32(0, 0, 0, byte.MaxValue);
			return;
		}
		IntroLerp += Time.deltaTime * IntroSpeed;
		float num = IntroCurve.Evaluate(IntroLerp);
		Image.color = new Color32(0, 0, 0, (byte)(255f * num));
	}
}
