using UnityEngine;

public class MenuSettingElement : MonoBehaviour
{
	private MenuPage parentPage;

	internal int settingElementID;

	private void Start()
	{
		parentPage = GetComponentInParent<MenuPage>();
		parentPage.settingElements.Add(this);
		settingElementID = parentPage.settingElements.Count;
	}

	private void OnDestroy()
	{
		if ((bool)parentPage)
		{
			parentPage.settingElements.Remove(this);
		}
	}
}
