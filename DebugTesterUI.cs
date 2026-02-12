using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;

public class DebugTesterUI : MonoBehaviour
{
	public static DebugTesterUI instance;

	private string text;

	private string moduleName;

	internal bool Active;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		if (!Debug.isDebugBuild && !SemiFunc.DebugDev())
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnGUI()
	{
		Active = !RecordingDirector.instance && (!DebugUI.instance || DebugUI.instance.enableParent.activeSelf) && (!DebugCommandHandler.instance || DebugCommandHandler.instance.debugOverlay);
		if (!Active)
		{
			return;
		}
		if (string.IsNullOrEmpty(this.text))
		{
			if (!BuildManager.instance || !SteamClient.IsValid)
			{
				return;
			}
			this.text = $"{BuildManager.instance.version.title}\n{SteamClient.Name} ({SteamClient.SteamId})";
		}
		string text = this.text;
		if (!SemiFunc.MenuLevel() && !SemiFunc.RunIsLobby() && !SemiFunc.RunIsTutorial())
		{
			text = ((!SemiFunc.IsMultiplayer()) ? "Singleplayer" : (SemiFunc.IsMasterClient() ? "Multiplayer (MC)" : "Multiplayer (CL)")) + "\n" + text;
			List<RoomVolume> currentRooms = PlayerAvatar.instance?.RoomVolumeCheck.CurrentRooms.Where((RoomVolume r) => r.Module).ToList();
			List<RoomVolume> list = currentRooms;
			if (list != null && list.Any())
			{
				if (currentRooms.All((RoomVolume r) => r.MapModule == currentRooms[0].MapModule))
				{
					moduleName = currentRooms[0].Module.name.Replace("(Clone)", "");
				}
				if (moduleName != null)
				{
					text = moduleName + "\n" + text;
				}
			}
			else
			{
				moduleName = null;
			}
		}
		else
		{
			moduleName = null;
		}
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.label)
		{
			fontSize = 13,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.LowerRight,
			wordWrap = false,
			normal = new GUIStyleState
			{
				textColor = Color.white
			}
		};
		GUIStyle style = new GUIStyle(gUIStyle)
		{
			normal = new GUIStyleState
			{
				textColor = Color.black
			}
		};
		float num = 400f;
		float x = (float)Screen.width - num - 4f;
		float num2 = gUIStyle.CalcHeight(new GUIContent(text), num);
		float y = (float)Screen.height - num2 - 20f;
		Rect position = new Rect(x, y, num, num2);
		GUI.Label(new Rect(position.x + 1f, position.y + 1f, position.width, position.height), text, style);
		GUI.Label(position, text, gUIStyle);
	}
}
