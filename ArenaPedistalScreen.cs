using TMPro;
using UnityEngine;

public class ArenaPedistalScreen : MonoBehaviour
{
	public TextMeshPro screenText;

	public GameObject glitchObject;

	public MeshRenderer glitchMeshRenderer;

	private float glitchTimer;

	public Light numberLight;

	public MeshRenderer screenScanLines;

	private void Update()
	{
		if (glitchTimer > 0f)
		{
			glitchTimer -= Time.deltaTime;
			float x = Mathf.Sin(Time.time * 100f) * 0.1f;
			float y = Mathf.Sin(Time.time * 100f) * 0.1f;
			glitchMeshRenderer.material.mainTextureOffset = new Vector2(x, y);
		}
		else
		{
			glitchObject.SetActive(value: false);
		}
	}

	public void SwitchNumber(int number, bool finalPlayer = false)
	{
		if ((bool)glitchMeshRenderer)
		{
			screenText.text = number.ToString();
			float x = Random.Range(0f, 100f);
			float y = Random.Range(0f, 100f);
			glitchMeshRenderer.material.mainTextureOffset = new Vector2(x, y);
			glitchObject.SetActive(value: true);
			if (finalPlayer)
			{
				screenText.color = Color.green;
				numberLight.color = Color.green;
				Color color = new Color(0f, 1f, 0f, 0.65f);
				screenScanLines.material.color = color;
			}
			glitchTimer = 0.2f;
		}
	}
}
