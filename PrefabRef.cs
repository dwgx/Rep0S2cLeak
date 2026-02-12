using System;
using UnityEngine;

[Serializable]
public class PrefabRef
{
	[SerializeField]
	private string prefabName;

	[SerializeField]
	private string resourcePath;

	public string PrefabName => prefabName;

	public string ResourcePath => resourcePath;

	public GameObject Prefab
	{
		get
		{
			if (RunManager.instance.singleplayerPool.TryGetValue(resourcePath, out var value))
			{
				return value;
			}
			value = Resources.Load<GameObject>(resourcePath);
			if (value == null)
			{
				Debug.LogError("PrefabRef failed to load \"" + resourcePath + "\". Make sure it's in a \"Resources\" folder.");
				return null;
			}
			RunManager.instance.singleplayerPool.Add(resourcePath, value);
			return value;
		}
	}

	public bool IsValid()
	{
		return !string.IsNullOrEmpty(resourcePath);
	}

	public void SetPrefab(GameObject go, string _resourcePath = null)
	{
		if (go != null)
		{
			prefabName = go.name;
			resourcePath = _resourcePath;
		}
		else
		{
			prefabName = null;
			resourcePath = null;
		}
	}
}
