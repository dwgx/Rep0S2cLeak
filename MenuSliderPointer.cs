using UnityEngine;
using UnityEngine.UI;

public class MenuSliderPointer : MonoBehaviour
{
	private RawImage rawImage;

	private void Start()
	{
		rawImage = GetComponent<RawImage>();
	}

	private void Update()
	{
		base.transform.localScale = Vector3.Lerp(base.transform.localScale, new Vector3(1f, 1f, 1f), 15f * Time.deltaTime);
		rawImage.color = Color.Lerp(rawImage.color, Color.red, 5f * Time.deltaTime);
	}

	public void Tick()
	{
		if ((bool)rawImage)
		{
			base.transform.localScale = new Vector3(1f, 3f, 1f);
			rawImage.color = new Color(0.5f, 0.5f, 1f, 1f);
		}
	}
}
