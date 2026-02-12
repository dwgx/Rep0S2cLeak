using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuPageSaves : MonoBehaviour
{
	public RectTransform saveFileInfo;

	public GameObject saveInfoDefault;

	public GameObject saveInfoSelected;

	public TextMeshProUGUI saveFileHeader;

	public TextMeshProUGUI saveFileHeaderDate;

	public TextMeshProUGUI saveFileInfoRow1;

	public TextMeshProUGUI saveFileInfoRow2;

	public TextMeshProUGUI saveFileInfoRow3;

	[Space]
	public RectTransform saveFileInfoMoonRect;

	public TextMeshProUGUI saveFileInfoMoonText;

	public RawImage saveFileInfoMoonImage;

	public GameObject saveFileInfoLoadButton;

	public GameObject saveFileInfoRestoreButton;

	[Space]
	public RectTransform Scroller;

	public MenuScrollBox menuScrollBox;

	private Image saveFileInfoPanel;

	public RectTransform saveFilePosition;

	public GameObject saveFilePrefab;

	internal bool currentSaveFileValid;

	internal string currentSaveFileName;

	internal List<string> currentSaveFileBackups;

	internal List<MenuElementSaveFile> saveFiles = new List<MenuElementSaveFile>();

	internal float saveFileYOffset;

	public TextMeshProUGUI gameModeHeader;

	private MenuPage menuPage;

	private void Start()
	{
		menuPage = GetComponent<MenuPage>();
		saveFileInfoPanel = saveFileInfo.GetComponentInChildren<Image>();
		if (SemiFunc.MainMenuIsMultiplayer())
		{
			gameModeHeader.text = "Multiplayer mode";
		}
		else
		{
			gameModeHeader.text = "Singleplayer mode";
		}
		StartCoroutine(LoadSavesCoroutine());
	}

	private IEnumerator LoadSavesCoroutine()
	{
		float yOffset = 0f;
		Task<List<StatsManager.SaveFolder>> task = StatsManager.instance.SaveFileGetAllAsync();
		yield return new WaitUntil(() => task.IsCompleted);
		foreach (StatsManager.SaveFolder item in task.Result)
		{
			string saveFileName = item.name;
			GameObject gameObject = Object.Instantiate(saveFilePrefab, Scroller);
			gameObject.transform.localPosition = saveFilePosition.localPosition;
			gameObject.transform.SetSiblingIndex(3);
			MenuElementSaveFile component = gameObject.GetComponent<MenuElementSaveFile>();
			component.saveFileName = saveFileName;
			component.saveFileBackups = item.backups;
			UpdateSaveDetails(component, saveFileName);
			gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y + yOffset, gameObject.transform.localPosition.z);
			float num = gameObject.GetComponent<RectTransform>().rect.height + 2f;
			yOffset -= num;
			saveFileYOffset = num;
			saveFiles.Add(gameObject.GetComponent<MenuElementSaveFile>());
		}
		menuScrollBox.RecalculateScrollHeight();
	}

	public void UpdateSaveDetails(MenuElementSaveFile menuSaveFile, string saveFileName)
	{
		if (int.TryParse(StatsManager.instance.SaveFileGetRunLevel(saveFileName), out var result) && int.TryParse(StatsManager.instance.SaveFileGetRunCurrency(saveFileName), out var _) && int.TryParse(StatsManager.instance.SaveFileGetTotalHaul(saveFileName), out var _))
		{
			string text = StatsManager.instance.SaveFileGetTeamName(saveFileName);
			menuSaveFile.saveFileHeader.text = text;
			string text2 = StatsManager.instance.SaveFileGetDateAndTime(saveFileName);
			menuSaveFile.saveFileHeaderDate.text = text2;
			string text3 = ColorUtility.ToHtmlStringRGB(SemiFunc.ColorDifficultyGet(1f, 10f, (float)result + 1f));
			menuSaveFile.saveFileHeaderLevel.text = "<sprite name=truck> <color=#" + text3 + ">" + (result + 1) + "</color>";
			float time = StatsManager.instance.SaveFileGetTimePlayed(saveFileName);
			Color numberColor = new Color(0.1f, 0.4f, 0.8f);
			Color unitColor = new Color(0.05f, 0.3f, 0.6f);
			menuSaveFile.saveFileInfoRow1.text = "<sprite name=clock>  " + SemiFunc.TimeToString(time, fancy: true, numberColor, unitColor);
		}
	}

	public void OnGoBack()
	{
		MenuManager.instance.PageCloseAll();
		MenuManager.instance.PageOpen(MenuPageIndex.Main);
	}

	private void Update()
	{
		if (SemiFunc.InputDown(InputKey.Back) && MenuManager.instance.currentMenuPageIndex == MenuPageIndex.Saves)
		{
			OnGoBack();
		}
		if (saveFileInfoPanel.color != new Color(0f, 0f, 0f, 1f))
		{
			saveFileInfoPanel.color = Color.Lerp(saveFileInfoPanel.color, new Color(0f, 0f, 0f, 1f), Time.deltaTime * 10f);
		}
	}

	public void OnNewGame()
	{
		if (saveFiles.Count >= 10)
		{
			MenuManager.instance.PageCloseAllAddedOnTop();
			MenuManager.instance.PagePopUp("Save file limit reached", Color.red, "You can only have 10 save files at a time. Please delete some save files to make room for new ones.", "OK", richText: true);
		}
		else if (SemiFunc.MainMenuIsMultiplayer())
		{
			SemiFunc.MenuActionHostGame();
		}
		else
		{
			SemiFunc.MenuActionSingleplayerGame();
		}
	}

	public void OnRestoreSave()
	{
		foreach (MenuElementSaveFile item in saveFiles.ToList())
		{
			if (item.saveFileName == currentSaveFileName && item.saveFileBackups.Count > 0)
			{
				string text = Application.persistentDataPath + "/saves/" + currentSaveFileName;
				File.Move(text + "/" + item.saveFileBackups[0] + ".es3", text + "/" + currentSaveFileName + ".es3");
				Debug.Log("Restored save file " + currentSaveFileName + " from backup " + item.saveFileBackups[0]);
				item.saveFileBackups.RemoveAt(0);
				UpdateSaveDetails(item, item.saveFileName);
				SaveFileSelected(item.saveFileName, item.saveFileBackups);
				break;
			}
		}
	}

	public void OnLoadGame()
	{
		if (SemiFunc.MainMenuIsMultiplayer())
		{
			SemiFunc.MenuActionHostGame(currentSaveFileName, currentSaveFileBackups);
		}
		else
		{
			SemiFunc.MenuActionSingleplayerGame(currentSaveFileName, currentSaveFileBackups);
		}
	}

	public void OnDeleteGame()
	{
		if (string.IsNullOrWhiteSpace(currentSaveFileName))
		{
			return;
		}
		SemiFunc.SaveFileDelete(currentSaveFileName);
		bool flag = false;
		foreach (MenuElementSaveFile item in saveFiles.ToList())
		{
			if (flag && (bool)item)
			{
				RectTransform component = item.GetComponent<RectTransform>();
				component.localPosition = new Vector3(component.localPosition.x, component.localPosition.y + saveFileYOffset, component.localPosition.z);
				MenuElementAnimations component2 = item.GetComponent<MenuElementAnimations>();
				component2.UIAniNudgeY();
				component2.UIAniRotate();
				component2.UIAniNewInitialPosition(new Vector2(component.localPosition.x, component.localPosition.y));
			}
			if (item.saveFileName == currentSaveFileName)
			{
				Object.Destroy(item.gameObject);
				saveFiles.Remove(item);
				flag = true;
			}
		}
		GoBackToDefaultInfo();
	}

	public void GoBackToDefaultInfo()
	{
		MenuElementAnimations component = saveFileInfo.GetComponent<MenuElementAnimations>();
		component.UIAniNudgeX();
		component.UIAniRotate();
		saveInfoDefault.SetActive(value: true);
		saveInfoSelected.SetActive(value: false);
		saveFileInfoPanel.color = new Color(0.45f, 0f, 0f, 1f);
	}

	private void InfoPlayerNames(TextMeshProUGUI _textMesh, string _folderName, string _fileName = null)
	{
		_textMesh.text = "";
		List<string> list = StatsManager.instance.SaveFileGetPlayerNames(_folderName, _fileName);
		if (list != null)
		{
			list.Sort((string text, string text2) => text.Length.CompareTo(text2.Length));
			int count = list.Count;
			int num = 0;
			foreach (string item in list)
			{
				if (num == count - 1)
				{
					_textMesh.text += item;
				}
				else if (num == count - 2)
				{
					_textMesh.text = _textMesh.text + item + "<color=#444444>   and   </color>";
				}
				else
				{
					_textMesh.text = _textMesh.text + item + "<color=#444444>,</color>   ";
				}
				num++;
			}
		}
		if (list == null || list.Count == 0)
		{
			_textMesh.text += "You did it all alone!";
		}
	}

	public void SaveFileSelected(string saveFolderName, List<string> saveFileBackups)
	{
		MenuElementAnimations component = saveFileInfo.GetComponent<MenuElementAnimations>();
		component.UIAniNudgeX();
		component.UIAniRotate();
		saveInfoDefault.SetActive(value: false);
		saveInfoSelected.SetActive(value: true);
		saveFileInfoPanel.color = new Color(0f, 0.1f, 0.25f, 1f);
		currentSaveFileName = saveFolderName;
		currentSaveFileBackups = saveFileBackups;
		string text = saveFolderName;
		if (!int.TryParse(StatsManager.instance.SaveFileGetRunLevel(saveFolderName), out var result) || !int.TryParse(StatsManager.instance.SaveFileGetRunCurrency(saveFolderName), out var result2) || !int.TryParse(StatsManager.instance.SaveFileGetTotalHaul(saveFolderName), out var result3))
		{
			currentSaveFileValid = false;
			if (saveFileBackups.Count > 0)
			{
				text = saveFileBackups[0];
			}
			if (saveFolderName == text || !int.TryParse(StatsManager.instance.SaveFileGetRunLevel(saveFolderName, text), out result) || !int.TryParse(StatsManager.instance.SaveFileGetRunCurrency(saveFolderName, text), out result2) || !int.TryParse(StatsManager.instance.SaveFileGetTotalHaul(saveFolderName, text), out result3))
			{
				saveFileInfoLoadButton.SetActive(value: false);
				saveFileInfoRestoreButton.SetActive(value: false);
				saveFileHeader.text = "CORRUPTED SAVE FILE";
				saveFileHeader.color = new Color(1f, 0f, 0f);
				saveFileHeaderDate.text = ":(";
				saveFileInfoRow1.text = "Sorry!";
				saveFileInfoRow2.text = "";
				saveFileInfoMoonRect.gameObject.SetActive(value: false);
				saveFileInfoRow3.text = "Press \"Delete Save\" to delete \nthis save file.";
				return;
			}
			saveFileInfoLoadButton.SetActive(value: false);
			saveFileInfoRestoreButton.SetActive(value: true);
		}
		else
		{
			currentSaveFileValid = true;
			saveFileInfoRestoreButton.SetActive(value: false);
			saveFileInfoLoadButton.SetActive(value: true);
		}
		string text2 = StatsManager.instance.SaveFileGetTeamName(saveFolderName, text);
		saveFileHeader.text = text2;
		saveFileHeader.color = new Color(1f, 0.54f, 0f);
		string text3 = StatsManager.instance.SaveFileGetDateAndTime(saveFolderName, text);
		saveFileHeaderDate.text = text3;
		string text4 = "      ";
		string text5 = ColorUtility.ToHtmlStringRGB(SemiFunc.ColorDifficultyGet(1f, 10f, (float)result + 1f));
		saveFileInfoRow1.text = "<sprite name=truck>  <color=#" + text5 + "><b>" + (result + 1) + "</b></color>";
		saveFileInfoRow1.text += text4;
		float time = StatsManager.instance.SaveFileGetTimePlayed(saveFolderName, text);
		TextMeshProUGUI textMeshProUGUI = saveFileInfoRow1;
		textMeshProUGUI.text = textMeshProUGUI.text + "<sprite name=clock>  " + SemiFunc.TimeToString(time, fancy: true, new Color(0.1f, 0.4f, 0.8f), new Color(0.05f, 0.3f, 0.6f));
		saveFileInfoRow1.text += text4;
		string text6 = ColorUtility.ToHtmlStringRGB(new Color(0.2f, 0.5f, 0.3f));
		TextMeshProUGUI textMeshProUGUI2 = saveFileInfoRow1;
		textMeshProUGUI2.text = textMeshProUGUI2.text + "<sprite name=$$>  <b>" + result2 + "</b><color=#" + text6 + ">k</color>";
		string text7 = SemiFunc.DollarGetString(result3);
		saveFileInfoRow2.text = "<color=#" + text6 + "><sprite name=$$$> TOTAL HAUL:      <b></b>$ </color><b>" + text7 + "</b><color=#" + text6 + ">k</color>";
		int value = RunManager.instance.CalculateMoonLevel(result);
		value = Mathf.Clamp(value, 0, RunManager.instance.moons.Count);
		if (value > 0)
		{
			saveFileInfoMoonRect.gameObject.SetActive(value: true);
			saveFileInfoMoonImage.texture = RunManager.instance.MoonGetIcon(value);
		}
		else
		{
			saveFileInfoMoonRect.gameObject.SetActive(value: false);
		}
		InfoPlayerNames(saveFileInfoRow3, saveFolderName, text);
	}

	public void OnRenameGame()
	{
		if (currentSaveFileValid)
		{
			MenuPageSavesRename component = MenuManager.instance.PageOpenOnTop(MenuPageIndex.SavesRename).GetComponent<MenuPageSavesRename>();
			component.menuPageParent = menuPage;
			component.fileName = currentSaveFileName;
		}
	}
}
