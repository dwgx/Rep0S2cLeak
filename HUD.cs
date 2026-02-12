using UnityEngine;

public class HUD : MonoBehaviour
{
	public static HUD instance;

	public bool hidden;

	public GameObject hideParent;

	private void Awake()
	{
		instance = this;
	}

	public void Hide()
	{
		hideParent.SetActive(value: false);
		hidden = true;
	}

	public void Show()
	{
		hideParent.SetActive(value: true);
		hidden = false;
	}
}
