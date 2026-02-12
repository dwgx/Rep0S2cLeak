using System.Collections;
using UnityEngine;

public class MapCustom : MonoBehaviour
{
	public Sprite sprite;

	public Color color = new Color(0f, 1f, 0.92f);

	public MapCustomEntity mapCustomEntity;

	private void Start()
	{
		StartCoroutine(AddToMap());
	}

	private IEnumerator AddToMap()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		Map.Instance.AddCustom(this, sprite, color);
	}

	public void Hide()
	{
		if ((bool)mapCustomEntity)
		{
			mapCustomEntity.Hide();
		}
	}
}
