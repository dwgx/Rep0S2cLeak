using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DebugConsoleUI : MonoBehaviour
{
	internal static DebugConsoleUI instance;

	internal bool chatActive;

	public TextMeshProUGUI chatText;

	private string prevChatMessage;

	private KeyCode keyHeld;

	private float keyHeldTimer;

	private List<string> cmdSuggestions = new List<string>();

	private int cmdSuggestionIndex;

	public TextMeshProUGUI suggestionsText;

	public TextMeshProUGUI responseText;

	public TextMeshProUGUI scrollIndicator;

	public GameObject enableObject;

	private void Start()
	{
		if (!SemiFunc.DebugTester() && !SemiFunc.DebugDev())
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
		}
	}

	private void Update()
	{
		if (chatActive)
		{
			if (SemiFunc.InputDown(InputKey.Back) || Input.GetKeyDown(KeyCode.BackQuote))
			{
				ShowDebugMenu(state: false);
				MenuManager.instance?.MenuEffectClick(MenuManager.MenuClickEffectType.Deny, null, 1f, 1f, soundOnly: true);
				return;
			}
			string text = chatText.text.TrimStart('/');
			if (SemiFunc.InputDown(InputKey.Confirm))
			{
				ShowDebugMenu(state: false);
				SetResponseText("", Color.white);
				if (!DebugCommandHandler.instance.Execute(text, isDebugConsole: true))
				{
					MenuManager.instance?.MenuEffectClick(MenuManager.MenuClickEffectType.Deny, null, 1f, 1f, soundOnly: true);
				}
				return;
			}
			SemiFunc.InputDisableMovement();
			if (SemiFunc.InputHold(InputKey.ChatDelete))
			{
				if (chatText.text.Length > 0 && (keyHeld != KeyCode.Backspace || Time.time >= keyHeldTimer))
				{
					chatText.text = chatText.text.Remove(chatText.text.Length - 1);
					MenuManager.instance?.MenuEffectClick(MenuManager.MenuClickEffectType.Dud, null, 2f, 1f, soundOnly: true);
					if (keyHeld != KeyCode.Backspace)
					{
						keyHeld = KeyCode.Backspace;
						keyHeldTimer = Time.time + 0.4f;
					}
					else
					{
						keyHeldTimer = Time.time + 0.09f;
					}
				}
			}
			else
			{
				if (keyHeld == KeyCode.Backspace)
				{
					keyHeld = KeyCode.None;
				}
				string source = Input.inputString;
				if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
				{
					source = GUIUtility.systemCopyBuffer;
				}
				foreach (char item in source.Where((char c2) => char.IsLetterOrDigit(c2) || char.IsPunctuation(c2) || char.IsSymbol(c2) || char.IsSeparator(c2)))
				{
					chatText.text += item;
				}
				if (chatText.text != prevChatMessage)
				{
					MenuManager.instance?.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 2f, 0.2f, soundOnly: true);
				}
			}
			if (chatText.text != prevChatMessage)
			{
				prevChatMessage = chatText.text;
				cmdSuggestionIndex = 0;
				text = chatText.text.TrimStart('/');
				cmdSuggestions = DebugCommandHandler.instance.GetSuggestions(text, isDebugConsole: true);
				UpdateCommandSuggestionsDisplay(text);
			}
			if (cmdSuggestions.Count <= 0)
			{
				return;
			}
			if (Input.GetMouseButtonDown(2) && !string.IsNullOrWhiteSpace(DebugCommandHandler.instance.lastExecutedCommand))
			{
				chatText.text = DebugCommandHandler.instance.lastExecutedCommand;
				MenuManager.instance?.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 2f, 0.2f, soundOnly: true);
			}
			else if (Input.GetKeyDown(KeyCode.Tab))
			{
				string[] array = chatText.text.TrimStart('/').Split(' ');
				int num = ((array.Length >= 1) ? 1 : 0);
				HashSet<string> hashSet = new HashSet<string> { "spawn", "upgrade" };
				if (array.Length > 1 && hashSet.Contains(array[0]) && (array.Length > 2 || cmdSuggestions.Contains(array[1])))
				{
					num = 2;
				}
				string text2 = ((num > 0) ? (string.Join(" ", array.Take(num)) + " ") : "");
				if (num == 1 && array.Length == 1 && !chatText.text.EndsWith(" "))
				{
					chatText.text = cmdSuggestions[cmdSuggestionIndex] + " ";
				}
				else
				{
					chatText.text = text2 + cmdSuggestions[cmdSuggestionIndex] + " ";
				}
				MenuManager.instance?.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 2f, 0.2f, soundOnly: true);
			}
			else
			{
				if (cmdSuggestions.Count <= 1)
				{
					return;
				}
				float num2 = 0f;
				if (Input.GetKey(KeyCode.UpArrow))
				{
					if (keyHeld != KeyCode.UpArrow || Time.time >= keyHeldTimer)
					{
						num2 = 1f;
						if (keyHeld != KeyCode.UpArrow)
						{
							keyHeld = KeyCode.UpArrow;
							keyHeldTimer = Time.time + 0.4f;
						}
						else
						{
							keyHeldTimer = Time.time + 0.09f;
						}
					}
				}
				else if (Input.GetKey(KeyCode.DownArrow))
				{
					if (keyHeld != KeyCode.DownArrow || Time.time >= keyHeldTimer)
					{
						num2 = -1f;
						if (keyHeld != KeyCode.DownArrow)
						{
							keyHeld = KeyCode.DownArrow;
							keyHeldTimer = Time.time + 0.4f;
						}
						else
						{
							keyHeldTimer = Time.time + 0.09f;
						}
					}
				}
				else
				{
					if (keyHeld == KeyCode.UpArrow || keyHeld == KeyCode.DownArrow)
					{
						keyHeld = KeyCode.None;
					}
					num2 = Input.GetAxis("Mouse ScrollWheel");
				}
				if (num2 > 0f)
				{
					cmdSuggestionIndex = (cmdSuggestionIndex - 1 + cmdSuggestions.Count) % cmdSuggestions.Count;
					UpdateCommandSuggestionsDisplay(text);
					MenuManager.instance?.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 2f, 0.2f, soundOnly: true);
				}
				else if (num2 < 0f)
				{
					cmdSuggestionIndex = (cmdSuggestionIndex + 1) % cmdSuggestions.Count;
					UpdateCommandSuggestionsDisplay(text);
					MenuManager.instance?.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 2f, 0.2f, soundOnly: true);
				}
			}
		}
		else if (Input.GetKeyDown(KeyCode.BackQuote) && (!ChatManager.instance || !ChatManager.instance.chatActive) && (!MenuManager.instance || !MenuManager.instance.textInputActive))
		{
			ShowDebugMenu(state: true);
		}
	}

	private void UpdateCommandSuggestionsDisplay(string partial)
	{
		IEnumerable<string> source;
		if (partial.Split(' ').Length > 1)
		{
			IEnumerable<string> enumerable = cmdSuggestions;
			source = enumerable;
		}
		else
		{
			source = cmdSuggestions.Select((string s) => (!DebugCommandHandler.instance._commands.TryGetValue(s.TrimStart('/'), out var value)) ? s : (s.TrimStart('/') + " - " + value.Description));
		}
		List<string> list = source.ToList();
		int start = Math.Clamp(cmdSuggestionIndex - 5, 0, Math.Max(0, list.Count - 10));
		suggestionsText.text = string.Join('\n', list.Skip(start).Take(10).Select((string s, int i) => (start + i != cmdSuggestionIndex) ? s : ("<color=green>" + s + "</color>")));
		bool flag = start > 0;
		bool flag2 = start + 10 < list.Count;
		scrollIndicator.text = ((flag && flag2) ? "↕" : (flag ? "↑" : (flag2 ? "↓" : "")));
	}

	internal void SetResponseText(string text, Color color, float duration = 2f)
	{
		float y = (((bool)DebugTesterUI.instance && DebugTesterUI.instance.Active) ? 40f : 5f);
		responseText.rectTransform.anchoredPosition = new Vector2(responseText.rectTransform.anchoredPosition.x, y);
		responseText.text = text;
		responseText.color = color;
		if (!string.IsNullOrWhiteSpace(text))
		{
			StartCoroutine(ClearResponseText(text, duration));
		}
	}

	private IEnumerator ClearResponseText(string text, float duration)
	{
		yield return new WaitForSeconds(duration);
		if (responseText.text == text)
		{
			responseText.text = "";
		}
	}

	internal void ShowDebugMenu(bool state)
	{
		if (chatActive == state)
		{
			return;
		}
		if (!state)
		{
			chatActive = false;
			enableObject.SetActive(value: false);
			return;
		}
		cmdSuggestionIndex = 0;
		suggestionsText.text = "";
		chatText.text = "";
		if (chatText.text == prevChatMessage)
		{
			cmdSuggestions = DebugCommandHandler.instance.GetSuggestions("", isDebugConsole: true);
			UpdateCommandSuggestionsDisplay("");
		}
		chatActive = true;
		enableObject.SetActive(value: true);
		MenuManager.instance?.MenuEffectClick(MenuManager.MenuClickEffectType.Action, null, 1f, 1f, soundOnly: true);
	}
}
