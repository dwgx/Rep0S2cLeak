using UnityEngine;
using UnityEngine.Serialization;

public class SemiIconMaker : MonoBehaviour
{
	[FormerlySerializedAs("camera")]
	public Camera iconCamera;

	public RenderTexture renderTexture;

	public bool iconCameraPlacementDone;

	public Sprite CreateIconFromRenderTexture()
	{
		if (!renderTexture)
		{
			Debug.LogError("RenderTexture is null");
			return null;
		}
		if (!ItemManager.instance.firstIcon)
		{
			Light[] componentsInChildren = GetComponentsInChildren<Light>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
		else
		{
			ItemManager.instance.firstIcon = false;
		}
		Transform obj = GetComponentInParent<ItemAttributes>().transform;
		Vector3 position = obj.position;
		obj.position = new Vector3(-1000f, -1000f, -1000f);
		Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = renderTexture;
		RenderSettings.fog = false;
		Color ambientLight = RenderSettings.ambientLight;
		RenderSettings.ambientLight = Color.white;
		iconCamera.Render();
		RenderSettings.fog = true;
		RenderSettings.ambientLight = ambientLight;
		texture2D.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = active;
		obj.position = position;
		Sprite result = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		base.gameObject.SetActive(value: false);
		return result;
	}
}
