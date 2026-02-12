using UnityEngine;

public class CanvasList : MonoBehaviour
{
	private void Awake()
	{
		PopulateAvailableTextures();
	}

	public static void PopulateAvailableTextures()
	{
		CanvasAssigner.AvailableTextures.Clear();
		Texture2D[] array = Resources.LoadAll<Texture2D>("Canvas");
		if (array.Length == 0)
		{
			Debug.LogWarning("No textures were loaded from the Resources/Canvas folder.");
		}
		Texture2D[] array2 = array;
		foreach (Texture2D texture2D in array2)
		{
			if (texture2D != null)
			{
				CanvasAssigner.AvailableTextures.Add(texture2D);
			}
			else
			{
				Debug.LogWarning("A texture was found but is not a Texture2D or could not be cast to Texture2D.");
			}
		}
	}
}
