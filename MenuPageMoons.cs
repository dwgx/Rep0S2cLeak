using System.Collections;
using UnityEngine;

public class MenuPageMoons : MonoBehaviour
{
	public RectTransform elementParent;

	public GameObject elementPrefab;

	public GameObject linePrefab;

	[Space]
	public MenuScrollBox scrollBox;

	private void Start()
	{
		StartCoroutine(CreateElements());
	}

	private IEnumerator CreateElements()
	{
		float _spacing = 15f;
		float _yPos = 0f;
		for (int i = 1; i <= RunManager.instance.moonLevel && i <= RunManager.instance.moons.Count; i++)
		{
			if (i > 1)
			{
				Object.Instantiate(linePrefab, elementParent).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, _yPos + _spacing / 2f);
			}
			GameObject gameObject = Object.Instantiate(elementPrefab, elementParent);
			MenuElementMoon _moonElement = gameObject.GetComponent<MenuElementMoon>();
			_moonElement.title.text = RunManager.instance.MoonGetName(i);
			_moonElement.icon.texture = RunManager.instance.MoonGetIcon(i);
			float _attributePos = 0f;
			foreach (string item in RunManager.instance.MoonGetAttributes(i))
			{
				GameObject gameObject2 = Object.Instantiate(_moonElement.attributePrefab, _moonElement.attributeRectTransform);
				MenuElementMoonAttribute _attributeElement = gameObject2.GetComponent<MenuElementMoonAttribute>();
				_attributeElement.rectTransform.anchoredPosition = new Vector3(0f, _attributePos, 0f);
				_attributeElement.text.text = item;
				while (_attributeElement.text.renderedWidth <= 0f)
				{
					yield return null;
				}
				float num = 7f;
				_attributeElement.flairRight.anchoredPosition = new Vector2(_attributeElement.text.renderedWidth / 2f + num, 1f);
				_attributeElement.flairLeft.anchoredPosition = new Vector2((0f - _attributeElement.text.renderedWidth) / 2f - num, 1f);
				_attributePos -= 20f;
			}
			_moonElement.rectTransform.anchoredPosition = new Vector2(0f, _yPos);
			_moonElement.rectTransform.sizeDelta = new Vector2(_moonElement.rectTransform.sizeDelta.x, 0f - _moonElement.attributeRectTransform.anchoredPosition.y - _attributePos);
			_yPos -= _moonElement.rectTransform.sizeDelta.y + _spacing;
		}
		scrollBox.enabled = true;
	}

	private void Update()
	{
		if (SemiFunc.InputDown(InputKey.Back) && MenuManager.instance.currentMenuPageIndex == MenuPageIndex.Moons)
		{
			ExitPage();
		}
	}

	public void ExitPage()
	{
		MenuManager.instance.PageCloseAll();
		MenuManager.instance.PageOpen(MenuPageIndex.Escape);
	}
}
