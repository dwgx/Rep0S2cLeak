using UnityEngine;

public class MapObject : MonoBehaviour
{
	public Transform parent;

	public void Hide()
	{
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != base.transform)
			{
				transform.gameObject.SetActive(value: false);
			}
		}
	}

	public void Show()
	{
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != base.transform)
			{
				transform.gameObject.SetActive(value: true);
			}
		}
	}
}
