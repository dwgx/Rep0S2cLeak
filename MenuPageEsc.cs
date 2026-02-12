using System.Collections.Generic;
using UnityEngine;

public class MenuPageEsc : MonoBehaviour
{
	public static MenuPageEsc instance;

	internal MenuPage menuPage;

	public GameObject playerMicrophoneVolumeSliderPrefab;

	internal Dictionary<PlayerAvatar, MenuSliderPlayerMicGain> playerMicGainSliders = new Dictionary<PlayerAvatar, MenuSliderPlayerMicGain>();

	public RectTransform mainMenuRectTransform;

	public GameObject moonsObject;

	private void Start()
	{
		instance = this;
		menuPage = GetComponent<MenuPage>();
		PlayerGainSlidersUpdate();
		if (RunManager.instance.moonLevel < 1)
		{
			moonsObject.SetActive(value: false);
			mainMenuRectTransform.anchoredPosition = new Vector2(mainMenuRectTransform.anchoredPosition.x, mainMenuRectTransform.anchoredPosition.y + 32f);
		}
	}

	private void Update()
	{
		if (SemiFunc.MenuLevel())
		{
			menuPage.PageStateSet(MenuPage.PageState.Closing);
		}
	}

	public void ButtonEventContinue()
	{
		menuPage.PageStateSet(MenuPage.PageState.Closing);
	}

	public void PlayerGainSlidersUpdate()
	{
		List<PlayerAvatar> list = new List<PlayerAvatar>();
		foreach (PlayerAvatar key in playerMicGainSliders.Keys)
		{
			if (!key || !playerMicGainSliders[key])
			{
				list.Add(key);
			}
		}
		foreach (PlayerAvatar item in list)
		{
			playerMicGainSliders.Remove(item);
		}
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (!playerMicGainSliders.ContainsKey(player) && !player.isLocal)
			{
				float x = 375f;
				if (SemiFunc.IsMasterClient())
				{
					x = 400f;
				}
				GameObject obj = Object.Instantiate(playerMicrophoneVolumeSliderPrefab, base.transform);
				obj.transform.localPosition = new Vector3(x, 21f, 0f);
				obj.transform.localPosition += new Vector3(0f, 25f * (float)playerMicGainSliders.Count, 0f);
				MenuSliderPlayerMicGain component = obj.GetComponent<MenuSliderPlayerMicGain>();
				component.playerAvatar = player;
				component.SliderNameSet(player.playerName);
				playerMicGainSliders.Add(player, component);
			}
		}
	}

	public void ButtonEventSelfDestruct()
	{
		if (SemiFunc.IsMultiplayer())
		{
			ChatManager.instance.PossessSelfDestruction();
		}
		else
		{
			PlayerAvatar.instance.playerHealth.health = 0;
			PlayerAvatar.instance.playerHealth.Hurt(1, savingGrace: false);
		}
		MenuManager.instance.PageCloseAll();
	}

	public void ButtonEventQuit()
	{
		RunManager.instance.skipLoadingUI = true;
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			player.quitApplication = true;
		}
		GameDirector.instance.OutroStart();
	}

	public void ButtonEventQuitToMenu()
	{
		GameDirector.instance.OutroStart();
		NetworkManager.instance.leavePhotonRoom = true;
	}

	public void ButtonEventChangeColor()
	{
		MenuManager.instance.PageSwap(MenuPageIndex.Color);
	}

	public void ButtonEventSettings()
	{
		MenuManager.instance.PageSwap(MenuPageIndex.Settings);
	}

	public void ButtonEventMoons()
	{
		MenuManager.instance.PageSwap(MenuPageIndex.Moons);
	}
}
