using UnityEngine;

public class OverlayState : MonoBehaviour
{
	public GameObject Play;

	public GameObject Stop;

	public GameObject Rewind;

	[Space]
	public RewindEffect RewindEffect;

	private void Update()
	{
		if (RewindEffect.PlayRewind)
		{
			Play.SetActive(value: false);
			Stop.SetActive(value: false);
			Rewind.SetActive(value: true);
		}
		else if (GameDirector.instance.currentState < GameDirector.gameState.Outro)
		{
			Play.SetActive(value: true);
			Stop.SetActive(value: false);
			Rewind.SetActive(value: false);
		}
		else
		{
			Play.SetActive(value: false);
			Stop.SetActive(value: true);
			Rewind.SetActive(value: false);
		}
	}
}
