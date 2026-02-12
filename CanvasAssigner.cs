using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class CanvasAssigner : MonoBehaviour
{
	public static List<Texture> AvailableTextures = new List<Texture>();

	private void Awake()
	{
		AssignRandomTexture();
	}

	private void AssignRandomTexture()
	{
		if (AvailableTextures.Count == 0)
		{
			CanvasList.PopulateAvailableTextures();
		}
		int index = Random.Range(0, AvailableTextures.Count);
		Texture mainTexture = AvailableTextures[index];
		AvailableTextures.RemoveAt(index);
		Renderer component = GetComponent<Renderer>();
		if ((bool)component && (bool)component.material)
		{
			component.material.mainTexture = mainTexture;
		}
	}
}
