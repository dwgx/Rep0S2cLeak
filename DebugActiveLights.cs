using TMPro;
using UnityEngine;

public class DebugActiveLights : MonoBehaviour
{
	private TextMeshProUGUI text;

	private void Awake()
	{
		text = GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		text.text = "Active Lights: " + LightManager.instance.activeLightsAmount;
	}
}
