using TMPro;
using UnityEngine;

public class CurrentTime : MonoBehaviour
{
	public TextMeshProUGUI textMesh;

	[HideInInspector]
	public int hour;

	[HideInInspector]
	public int minute;

	private void Start()
	{
		hour = Random.Range(0, 23);
		minute = Random.Range(0, 59);
	}

	private void Update()
	{
		string text = hour.ToString();
		if ((float)hour < 10f)
		{
			text = "0" + text;
		}
		string text2 = minute.ToString();
		if ((float)minute < 10f)
		{
			text2 = "0" + text2;
		}
		textMesh.text = text + ":" + text2;
	}
}
