using UnityEngine;

public class GameplayButtonRichPresence : MonoBehaviour
{
	public void ButtonPressed()
	{
		SteamManager.instance?.UpdateSteamFriendGrouping(SteamManager.instance.currentLobby);
		RunManager.instance?.UpdateSteamRichPresence();
	}
}
