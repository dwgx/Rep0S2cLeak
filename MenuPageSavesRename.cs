using System.Linq;
using UnityEngine;

public class MenuPageSavesRename : MonoBehaviour
{
	public MenuButton confirmButton;

	public MenuTextInput menuTextInput;

	internal MenuPage menuPageParent;

	internal string fileName;

	private void Update()
	{
		if (SemiFunc.InputDown(InputKey.Confirm))
		{
			ButtonConfirm();
		}
		if (SemiFunc.InputDown(InputKey.Back))
		{
			ExitPage();
		}
	}

	public void ExitPage()
	{
		MenuManager.instance.PageCloseAllExcept(MenuPageIndex.Saves);
		MenuManager.instance.PageSetCurrent(MenuPageIndex.Saves, menuPageParent);
	}

	public void ButtonConfirm()
	{
		if (string.IsNullOrEmpty(menuTextInput.textCurrent))
		{
			confirmButton.OnHovering();
			MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Deny, null, 1f, 1f);
			return;
		}
		ES3Settings settings = new ES3Settings(Application.persistentDataPath + "/saves/" + fileName + "/" + fileName + ".es3", ES3.EncryptionType.AES, StatsManager.instance.totallyNormalString);
		ES3.Save("teamName", menuTextInput.textCurrent, settings);
		MenuPageSaves component = menuPageParent.GetComponent<MenuPageSaves>();
		if ((bool)component)
		{
			foreach (MenuElementSaveFile item in component.saveFiles.ToList())
			{
				if (item.saveFileName == fileName)
				{
					component.UpdateSaveDetails(item, item.saveFileName);
					component.SaveFileSelected(item.saveFileName, item.saveFileBackups);
					break;
				}
			}
		}
		MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm, null, 1f, 1f);
		ExitPage();
	}
}
