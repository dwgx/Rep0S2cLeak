using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TruckScreenText : MonoBehaviour
{
	public enum ScreenType
	{
		TruckScreen,
		TruckLobbyScreen,
		TruckShopScreen
	}

	public enum TruckScreenPage
	{
		Start,
		EndNotEnough,
		EndEnough,
		AllPlayersInTruck
	}

	public enum LobbyScreenPage
	{
		Start,
		FailFirst,
		FailSecond,
		FailThird,
		HitRoad
	}

	public enum ShopScreenPage
	{
		Start,
		NotEnough,
		Enough,
		Stealing,
		AllPlayersInTruck
	}

	public enum PlayerChatBoxState
	{
		Idle,
		Typing,
		LockedDestroySlackers,
		LockedStartingTruck
	}

	[Serializable]
	public class CustomEmojiSounds
	{
		public string emojiString;

		public Sound emojiSound;
	}

	[Serializable]
	public class TextLine
	{
		public string messageName;

		[Multiline]
		public List<string> textLines = new List<string>();

		public bool customSoundEffects;

		public bool customEvents;

		public bool customRevealSettings;

		public Sound customMessageSoundEffect;

		public Sound customTypingSoundEffect;

		public UnityEvent onLineStart;

		public UnityEvent onLineEnd;

		public ScreenTextRevealDelaySettings typingSpeed;

		[HideInInspector]
		public float letterRevealDelay;

		public ScreenNextMessageDelaySettings messageDelayTime;

		[HideInInspector]
		public float delayAfter;

		[HideInInspector]
		public string text;

		[HideInInspector]
		public string textOriginal;
	}

	[Serializable]
	public class TextPages
	{
		public string pageName;

		public List<TextLine> textLines = new List<TextLine>();

		public bool goToNextPageAutomatically;
	}

	public ScreenTextRevealDelaySettings textRevealNormalSetting;

	public ScreenNextMessageDelaySettings nextMessageDelayNormalSetting;

	public GameObject staticGrabCollider;

	public static TruckScreenText instance;

	private StaticGrabObject staticGrabObject;

	public ScreenType screenType;

	public string chatMessageString;

	public TextMeshProUGUI chatMessage;

	public UnityEvent onChatMessage;

	private float chatMessageTimer;

	private float chatMessageTimerMax = 2f;

	private bool chatActive;

	private PhotonView photonView;

	private int chatCharacterIndex;

	private float chatDeactivatedTimer;

	private string nicknameTaxman = "\n\n<color=#4d0000><b>TAXMAN:</b></color>\n";

	private string currentNickname = "";

	private bool screenActive;

	private int nextPageOverride = -1;

	public Transform chatMessageLoadingBar;

	public Transform chatMessageResultBar;

	public Light chatMessageResultBarLight;

	internal float chatMessageResultBarTimer;

	private float chatActiveTimer;

	private bool selfDestructingPlayers;

	private TuckScreenLocked truckScreenLocked;

	private string chatMessageIdleString1 = "<sprite name=message>";

	private string chatMessageIdleString2 = "<sprite name=message_arrow>";

	private string chatMessageIdleStringCurrent = "<sprite name=message>";

	private float chatMessageIdleStringTimer;

	internal PlayerChatBoxState playerChatBoxState;

	private bool playerChatBoxStateStart;

	public List<CustomEmojiSounds> customEmojiSounds = new List<CustomEmojiSounds>();

	public Sound typingSound;

	public Sound emojiSound;

	public Sound newLineSound;

	public Sound newPageSound;

	public Sound chargeChatLoop;

	public Sound chatMessageResultSuccess;

	public Sound chatMessageResultFail;

	public RawImage background;

	public Color mainBackgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f);

	public Color offBackgroundColor = new Color(0f, 0f, 0f, 1f);

	public Color evilBackgroundColor = new Color(0.5f, 0f, 0f, 1f);

	public Color transitionBackgroundColor = new Color(0.5f, 0.5f, 0f, 1f);

	private float arrowPointAtGoalTimer;

	private float engineSoundTimer;

	public Transform engineSoundTransform;

	public Sound engineRevSound;

	public Sound engineSuccessSound;

	public TextMeshProUGUI textMesh;

	public string testingText;

	public List<TextPages> pages = new List<TextPages>();

	private int currentPageIndex;

	private int currentLineIndex;

	private int currentCharIndex;

	private float typingTimer;

	internal float delayTimer;

	internal bool isTyping;

	private float backgroundColorChangeTimer;

	private float backgroundColorChangeDuration = 0.5f;

	private float startWaitTimer;

	private bool started;

	private bool lobbyStarted;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		truckScreenLocked = GetComponentInChildren<TuckScreenLocked>();
		staticGrabObject = GetComponent<StaticGrabObject>();
		foreach (TextPages page in pages)
		{
			foreach (TextLine textLine in page.textLines)
			{
				if ((bool)textLine.typingSpeed)
				{
					textLine.letterRevealDelay = textLine.typingSpeed.GetDelay();
				}
				else
				{
					textLine.letterRevealDelay = textRevealNormalSetting.GetDelay();
				}
				if ((bool)textLine.messageDelayTime)
				{
					textLine.delayAfter = textLine.messageDelayTime.GetDelay();
				}
				else
				{
					textLine.delayAfter = nextMessageDelayNormalSetting.GetDelay();
				}
			}
		}
		screenActive = true;
		if (textMesh == null)
		{
			Debug.LogError("TextMeshProUGUI component is not assigned.");
		}
		photonView = GetComponent<PhotonView>();
		currentNickname = nicknameTaxman;
		chatMessageString = SemiFunc.EmojiText(chatMessageString);
	}

	private void LobbyScreenStartLogic()
	{
		if (lobbyStarted)
		{
			return;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && screenType == ScreenType.TruckLobbyScreen && RunManager.instance.levelFailed)
		{
			if (RunManager.instance.runLives == 2)
			{
				GotoPage(1);
			}
			if (RunManager.instance.runLives == 1)
			{
				GotoPage(2);
			}
			if (RunManager.instance.runLives == 0)
			{
				GotoPage(3);
			}
		}
		lobbyStarted = true;
	}

	public void TutorialFinish()
	{
		GameDirector.instance.OutroStart();
		NetworkManager.instance.leavePhotonRoom = true;
	}

	public void ArrowPointAtGoal()
	{
		arrowPointAtGoalTimer = 4f;
	}

	private void ArrowPointAtGoalLogic()
	{
		if (arrowPointAtGoalTimer > 0f)
		{
			if (PlayerAvatar.instance.RoomVolumeCheck.inTruck)
			{
				SemiFunc.UIShowArrow(new Vector3(340f, 90f, 0f), new Vector3(610f, 330f, 0f), 45f);
			}
			arrowPointAtGoalTimer -= Time.deltaTime;
		}
	}

	private void ChatMessageIdleStringTick()
	{
		if (chatActive)
		{
			return;
		}
		if (chatMessageIdleStringTimer < 1f)
		{
			chatMessageIdleStringTimer += Time.deltaTime;
			return;
		}
		if (chatMessageIdleStringCurrent == chatMessageIdleString1)
		{
			chatMessageIdleStringCurrent = chatMessageIdleString2;
		}
		else
		{
			chatMessageIdleStringCurrent = chatMessageIdleString1;
		}
		chatMessage.text = chatMessageIdleStringCurrent;
		chatMessageIdleStringTimer = 0f;
	}

	private void PlayerSelfDestruction()
	{
		if (!SemiFunc.FPSImpulse5() || !selfDestructingPlayers)
		{
			return;
		}
		bool flag = true;
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (!item.isDisabled)
			{
				if (item.RoomVolumeCheck.inTruck && !item.selfDestructPrevented)
				{
					item.selfDestructPrevented = true;
				}
				else if (!item.selfDestructPrevented)
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			ChatMessageResultSuccess();
			PlayerChatBoxStateUpdate(PlayerChatBoxState.LockedStartingTruck);
			GotoPage(2);
			selfDestructingPlayers = false;
		}
	}

	private void Update()
	{
		PlayerChatBoxStateMachine();
		PlayerSelfDestruction();
		ChatMessageResultTick();
		ChatMessageIdleStringTick();
		HoldChat();
		float num = chatMessageTimer / chatMessageTimerMax;
		chatMessageLoadingBar.localScale = new Vector3(Mathf.Lerp(chatMessageLoadingBar.localScale.x, num, Time.deltaTime * 10f), chatMessageLoadingBar.localScale.y, chatMessageLoadingBar.localScale.z);
		if (chatActive)
		{
			chatActiveTimer = 0.2f;
		}
		else
		{
			chatActiveTimer -= Time.deltaTime;
			if (chatActiveTimer <= 0f)
			{
				chatMessageTimer = 0f;
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && !started && GameDirector.instance.currentState == GameDirector.gameState.Main)
		{
			if (startWaitTimer < 1f)
			{
				startWaitTimer += Time.deltaTime;
			}
			else
			{
				InitializeTextTyping();
				LobbyScreenStartLogic();
			}
		}
		if (started)
		{
			UpdateBackgroundColor();
			if (isTyping)
			{
				TypingUpdate();
			}
			else
			{
				DelayUpdate();
			}
			CheckTextMeshLines();
			ArrowPointAtGoalLogic();
		}
	}

	private void InitializeTextTypingLogic()
	{
		textMesh.text = "";
		currentPageIndex = 0;
		currentLineIndex = 0;
		currentCharIndex = 0;
		typingTimer = 0f;
		foreach (TextPages page in pages)
		{
			foreach (TextLine textLine in page.textLines)
			{
				if (textLine.textLines.Count > 0)
				{
					textLine.text = textLine.textLines[0];
				}
				else
				{
					textLine.text = "Missing line!!";
				}
				textLine.text = SemiFunc.EmojiText(textLine.text);
				textLine.textOriginal = textLine.text;
			}
		}
		started = true;
		NextLine(currentLineIndex);
	}

	private void InitializeTextTyping()
	{
		if (GameManager.instance.gameMode == 0)
		{
			InitializeTextTypingLogic();
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("InitializeTextTypingRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	public void InitializeTextTypingRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			InitializeTextTypingLogic();
		}
	}

	private void CheckTextMeshLines()
	{
		if (textMesh.textInfo.lineCount > 12)
		{
			int num = textMesh.text.IndexOf('\n');
			if (num != -1)
			{
				textMesh.text = textMesh.text.Substring(num + 1);
			}
		}
	}

	private void ChatMessageResultSuccess()
	{
		chatMessageLoadingBar.localScale = new Vector3(0f, chatMessageLoadingBar.localScale.y, chatMessageLoadingBar.localScale.z);
		chatMessageResultBar.gameObject.SetActive(value: true);
		chatMessageResultSuccess.Play(chatMessageResultBar.position);
		chatMessageResultBarLight.color = new Color(0f, 1f, 0f, 1f);
		chatMessageResultBar.GetComponent<RawImage>().color = new Color(0f, 1f, 0f, 1f);
		chatMessageResultBarTimer = 0.2f;
	}

	private void ChatMessageResultFail()
	{
		chatMessageLoadingBar.localScale = new Vector3(0f, chatMessageLoadingBar.localScale.y, chatMessageLoadingBar.localScale.z);
		chatMessageResultBar.gameObject.SetActive(value: true);
		chatMessageResultFail.Play(chatMessageResultBar.position);
		chatMessageResultBarLight.color = new Color(1f, 0f, 0f, 1f);
		chatMessageResultBar.GetComponent<RawImage>().color = new Color(1f, 0f, 0f, 1f);
		chatMessageResultBarTimer = 0.2f;
	}

	private void ChatMessageResultTick()
	{
		if (chatMessageResultBarTimer > 0f)
		{
			chatMessageResultBarTimer -= Time.deltaTime;
		}
		else if (chatMessageResultBarTimer != -123f)
		{
			chatMessageResultBar.gameObject.SetActive(value: false);
			chatMessageResultBarTimer = -123f;
		}
	}

	public void ChatMessageLevel()
	{
		if (RoundDirector.instance.extractionPointsCompleted == RoundDirector.instance.extractionPoints)
		{
			if (SemiFunc.PlayersAllInTruck())
			{
				ChatMessageResultSuccess();
				PlayerChatBoxStateUpdate(PlayerChatBoxState.LockedStartingTruck);
				GotoPage(2);
			}
			else
			{
				ChatMessageResultFail();
				PlayerChatBoxStateUpdate(PlayerChatBoxState.LockedDestroySlackers);
				GotoPage(3);
			}
		}
		else
		{
			ChatMessageResultFail();
			GotoPage(1);
		}
	}

	public void ChatMessageLobby()
	{
		ChatMessageResultSuccess();
		PlayerChatBoxStateUpdate(PlayerChatBoxState.LockedStartingTruck);
		GotoPage(4);
	}

	public void ChatMessageTutorial()
	{
		PlayerChatBoxStateUpdate(PlayerChatBoxState.LockedStartingTruck);
		ChatMessageResultSuccess();
		GotoPage(2);
	}

	public void ChatMessageShop()
	{
		if (SemiFunc.PlayersAllInTruck())
		{
			ChatMessageResultSuccess();
			PlayerChatBoxStateUpdate(PlayerChatBoxState.LockedStartingTruck);
			GotoPage(2);
		}
		else
		{
			ChatMessageResultFail();
			PlayerChatBoxStateUpdate(PlayerChatBoxState.LockedDestroySlackers);
			GotoPage(4);
		}
	}

	private void OnValidate()
	{
		if (!SemiFunc.OnValidateCheck())
		{
			textMesh.text = SemiFunc.EmojiText(testingText);
		}
	}

	private void ReplaceStringWithVariable(string variableString, string variableValueString, TextLine currentLine)
	{
		currentLine.text = currentLine.text.Remove(currentCharIndex, variableString.Length - 1);
		currentLine.text = currentLine.text.Insert(currentCharIndex + 1, variableValueString);
	}

	private void TypingUpdate()
	{
		if (!screenActive || currentPageIndex >= pages.Count)
		{
			return;
		}
		TextPages textPages = pages[currentPageIndex];
		if (currentLineIndex >= textPages.textLines.Count)
		{
			return;
		}
		TextLine textLine = textPages.textLines[currentLineIndex];
		if (currentCharIndex >= textLine.text.Length)
		{
			return;
		}
		if (currentCharIndex == 0 && typingTimer == 0f)
		{
			if (currentLineIndex != 0)
			{
				textMesh.text = textMesh.text;
			}
			textMesh.text += currentNickname;
			NewLineSoundEffect();
		}
		typingTimer += Time.deltaTime;
		if (!(typingTimer >= textLine.letterRevealDelay))
		{
			return;
		}
		string emojiString = "";
		bool flag = false;
		int i = currentCharIndex;
		if (textLine.text[i] == '<')
		{
			for (; textLine.text[i] != '>'; i++)
			{
				emojiString += textLine.text[i];
			}
			flag = true;
		}
		if (flag)
		{
			textMesh.text += emojiString;
			currentCharIndex = i;
		}
		string text = "";
		int num = 0;
		bool flag2 = false;
		if (textLine.text[currentCharIndex] == '[')
		{
			flag2 = true;
			int num2 = currentCharIndex;
			int num3 = num;
			while (textLine.text[currentCharIndex] != ']')
			{
				text += textLine.text[currentCharIndex];
				currentCharIndex++;
				num++;
				if (currentCharIndex >= textLine.text.Length)
				{
					flag2 = false;
					currentCharIndex = num2;
					num = num3;
					break;
				}
			}
			text += textLine.text[currentCharIndex];
			currentCharIndex++;
			num++;
			currentCharIndex -= num;
			if (text == "[haul]")
			{
				int num4 = RoundDirector.instance.extractionPointsCompleted;
				if (num4 == RoundDirector.instance.extractionPoints && playerChatBoxState != PlayerChatBoxState.LockedStartingTruck)
				{
					num4 = RoundDirector.instance.extractionPoints - 1;
				}
				string text2 = " <color=#FFAA00>" + num4 + "</color><color=#3F2B00> / </color><color=#FFAA00>" + RoundDirector.instance.extractionPoints + "</color> ";
				text2 += "<sprite name=extraction>";
				ReplaceStringWithVariable(text, text2, textLine);
				if (!RoundDirector.instance.extractionPointCurrent && RoundDirector.instance.extractionPointsCompleted > 0 && RoundDirector.instance.extractionPoints > 1 && RoundDirector.instance.extractionPointsCompleted != RoundDirector.instance.extractionPoints)
				{
					TutorialDirector.instance.ActivateTip("Multiple Extractions", 0f, _interrupt: true);
				}
			}
			if (text == "[goal]")
			{
				int haulGoal = RoundDirector.instance.haulGoal;
				string valueString = haulGoal.ToString();
				valueString = FormatDollarValueStrings(valueString);
				ReplaceStringWithVariable(text, valueString, textLine);
			}
			if (text == "[goalmax]")
			{
				string valueString2 = RoundDirector.instance.haulGoalMax.ToString();
				valueString2 = FormatDollarValueStrings(valueString2);
				ReplaceStringWithVariable(text, valueString2, textLine);
			}
			if (text == "[hitroad]")
			{
				if (currentNickname == nicknameTaxman)
				{
					chatMessageString = chatMessageString.Replace("?", "!");
				}
				ReplaceStringWithVariable(text, chatMessageString, textLine);
				currentNickname = nicknameTaxman;
			}
			if (text == "[allplayerintruck]")
			{
				List<string> list = new List<string>();
				foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
				{
					if (!player.isDisabled && !player.RoomVolumeCheck.inTruck)
					{
						list.Add(player.playerName);
					}
				}
				string text3 = "";
				for (int j = 0; j < list.Count; j++)
				{
					text3 = ((j != 0) ? ((j != list.Count - 1) ? (text3 + ", " + list[j]) : (text3 + " & " + list[j])) : (text3 + list[j]));
				}
				text3 += "...<sprite name=fedup>";
				text3 += "\n";
				text3 += SemiFunc.EmojiText("{pointright}{truck}{pointleft}");
				ReplaceStringWithVariable(text, text3, textLine);
			}
			if (text == "[betrayplayers]")
			{
				List<string> list2 = new List<string>();
				foreach (PlayerAvatar player2 in GameDirector.instance.PlayerList)
				{
					if (!player2.isDisabled && !player2.RoomVolumeCheck.inTruck)
					{
						list2.Add(player2.playerName);
					}
				}
				string text4 = "";
				for (int k = 0; k < list2.Count; k++)
				{
					text4 = ((k != 0) ? ((k != list2.Count - 1) ? (text4 + ", " + list2[k]) : (text4 + " & " + list2[k])) : (text4 + list2[k]));
				}
				text4 += "... <sprite name=fedup>";
				text4 += "\n";
				text4 += SemiFunc.EmojiText("{pointright}{truck}{pointleft}");
				ReplaceStringWithVariable(text, text4, textLine);
			}
		}
		if (!flag2)
		{
			textMesh.text += textLine.text[currentCharIndex];
		}
		if (flag)
		{
			emojiString += textLine.text[currentCharIndex];
		}
		if (textLine.text[currentCharIndex] != ' ')
		{
			if (!flag)
			{
				TypeSoundEffect();
			}
			else if (customEmojiSounds.Any((CustomEmojiSounds x) => x.emojiString == emojiString))
			{
				customEmojiSounds.Find((CustomEmojiSounds x) => x.emojiString == emojiString).emojiSound.Play(textMesh.transform.position);
			}
			else
			{
				emojiSound.Play(textMesh.transform.position);
			}
		}
		currentCharIndex++;
		typingTimer = 0f;
		if (currentCharIndex >= textLine.text.Length)
		{
			textMesh.text = textMesh.text;
			isTyping = false;
			delayTimer = 0f;
		}
	}

	private void UpdateBackgroundColor()
	{
		if (backgroundColorChangeTimer < backgroundColorChangeDuration)
		{
			backgroundColorChangeTimer += Time.deltaTime;
		}
		else if (screenActive)
		{
			background.color = mainBackgroundColor;
		}
		else
		{
			background.color = offBackgroundColor;
		}
	}

	private void DelayUpdate()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || currentLineIndex >= pages[currentPageIndex].textLines.Count)
		{
			return;
		}
		TextLine textLine = pages[currentPageIndex].textLines[currentLineIndex];
		delayTimer += Time.deltaTime;
		if (!(delayTimer >= textLine.delayAfter))
		{
			return;
		}
		pages[currentPageIndex].textLines[currentLineIndex].onLineEnd?.Invoke();
		currentLineIndex++;
		if ((currentLineIndex >= pages[currentPageIndex].textLines.Count && pages[currentPageIndex].goToNextPageAutomatically) || nextPageOverride != -1)
		{
			if (nextPageOverride != -1)
			{
				GotoPage(nextPageOverride);
				nextPageOverride = -1;
			}
			else
			{
				GotoPage(currentPageIndex + 1);
			}
			currentLineIndex = 0;
			if (currentPageIndex >= pages.Count)
			{
				currentPageIndex = pages.Count;
			}
		}
		if (currentLineIndex < pages[currentPageIndex].textLines.Count)
		{
			NextLine(currentLineIndex);
		}
	}

	private void RestartTyping()
	{
		InitializeTextTyping();
	}

	private void TypeSoundEffect()
	{
		if (pages[currentPageIndex].textLines[currentLineIndex].customTypingSoundEffect.Sounds.Length != 0)
		{
			pages[currentPageIndex].textLines[currentLineIndex].customTypingSoundEffect.Play(textMesh.transform.position);
		}
		else
		{
			typingSound.Play(textMesh.transform.position);
		}
	}

	private void NewLineSoundEffect()
	{
		if (pages[currentPageIndex].textLines[currentLineIndex].customMessageSoundEffect.Sounds.Length != 0)
		{
			pages[currentPageIndex].textLines[currentLineIndex].customMessageSoundEffect.Play(textMesh.transform.position);
		}
		else
		{
			newLineSound.Play(textMesh.transform.position);
		}
	}

	public void StartChat()
	{
		if (screenActive)
		{
			if (GameManager.instance.gameMode == 0)
			{
				chatActive = true;
			}
			else
			{
				photonView.RPC("StartChatRPC", RpcTarget.All);
			}
		}
	}

	[PunRPC]
	public void StartChatRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			chatActive = true;
		}
	}

	public void HoldChat()
	{
		chargeChatLoop.LoopPitch = 1f + chatMessageTimer / chatMessageTimerMax;
		chargeChatLoop.PlayLoop(chatActive, 0.9f, 0.9f);
		if (!screenActive && chatActiveTimer <= 0f)
		{
			return;
		}
		if (chatDeactivatedTimer > 0f && chatActiveTimer <= 0f)
		{
			chatDeactivatedTimer -= Time.deltaTime;
		}
		else
		{
			if (!chatActive && chatActiveTimer <= 0f)
			{
				return;
			}
			if (playerChatBoxState != PlayerChatBoxState.Idle)
			{
				ForceReleaseChat();
				staticGrabCollider.SetActive(value: false);
				chatActive = false;
				return;
			}
			chatMessageTimer += 1.5f * Time.deltaTime;
			if (chatMessageTimer >= chatMessageTimerMax)
			{
				chatMessageTimer = 0f;
				chatActiveTimer = 0f;
				chatActive = false;
				if (staticGrabObject.playerGrabbing.Count <= 0)
				{
					return;
				}
				PhysGrabber physGrabber = staticGrabObject.playerGrabbing[0];
				if ((bool)physGrabber)
				{
					string playerName = physGrabber.playerAvatar.playerName;
					if (GameManager.instance.gameMode == 0)
					{
						ChatMessageSend(playerName);
					}
					else if (PhotonNetwork.IsMasterClient)
					{
						photonView.RPC("ChatMessageSendRPC", RpcTarget.All, playerName);
					}
				}
				return;
			}
			int num = (int)(chatMessageTimer / chatMessageTimerMax * (float)chatMessageString.Length);
			num = Mathf.Min(num, chatMessageString.Length);
			bool flag = false;
			string emojiString = "";
			while (chatCharacterIndex < num)
			{
				if (chatMessageString[chatCharacterIndex] == '<')
				{
					flag = true;
					int num2 = chatMessageString.IndexOf('>', chatCharacterIndex);
					if (num2 != -1)
					{
						num = Mathf.Min(num + (num2 - chatCharacterIndex), chatMessageString.Length);
						chatCharacterIndex = num2 + 1;
					}
					else
					{
						chatCharacterIndex++;
						emojiString += chatMessageString[chatCharacterIndex];
					}
				}
				else
				{
					chatCharacterIndex++;
				}
				if (!flag)
				{
					TypeSoundEffect();
				}
				else if (customEmojiSounds.Any((CustomEmojiSounds x) => x.emojiString == emojiString))
				{
					customEmojiSounds.Find((CustomEmojiSounds x) => x.emojiString == emojiString).emojiSound.Play(textMesh.transform.position);
				}
				else
				{
					emojiSound.Play(textMesh.transform.position);
				}
				chatMessage.text = chatMessageString.Substring(0, chatCharacterIndex);
			}
		}
	}

	private void ForceReleaseChat()
	{
		if (staticGrabObject.playerGrabbing.Count <= 0)
		{
			return;
		}
		List<PhysGrabber> list = new List<PhysGrabber>();
		list.AddRange(staticGrabObject.playerGrabbing);
		foreach (PhysGrabber item in list)
		{
			if (!SemiFunc.IsMultiplayer())
			{
				item.ReleaseObject(photonView.ViewID);
				continue;
			}
			item.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, true, 0.1f, photonView.ViewID);
		}
	}

	private void NextLineLogic(int _lineIndex, int index)
	{
		pages[currentPageIndex].textLines[currentLineIndex].onLineStart?.Invoke();
		pages[currentPageIndex].textLines[currentLineIndex].text = SemiFunc.EmojiText(pages[currentPageIndex].textLines[currentLineIndex].textLines[index]);
		currentCharIndex = 0;
		currentLineIndex = _lineIndex;
		isTyping = true;
		typingTimer = 0f;
		delayTimer = 0f;
	}

	private void NextLine(int _currentLineIndex)
	{
		if (pages[currentPageIndex].textLines.Count != 0)
		{
			if (GameManager.instance.gameMode == 0)
			{
				int count = pages[currentPageIndex].textLines[currentLineIndex].textLines.Count;
				int index = UnityEngine.Random.Range(0, count);
				NextLineLogic(_currentLineIndex, index);
			}
			else if (PhotonNetwork.IsMasterClient)
			{
				int count2 = pages[currentPageIndex].textLines[currentLineIndex].textLines.Count;
				int num = UnityEngine.Random.Range(0, count2);
				photonView.RPC("NextLineRPC", RpcTarget.All, num, _currentLineIndex);
			}
		}
	}

	[PunRPC]
	public void NextLineRPC(int index, int _currentLineIndex, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				currentLineIndex = _currentLineIndex;
			}
			NextLineLogic(_currentLineIndex, index);
		}
	}

	private void ForceCompleteChatMessage()
	{
		int length = pages[currentPageIndex].textLines[currentLineIndex].text.Length;
		if (currentCharIndex <= length)
		{
			for (int i = currentCharIndex; i < length; i++)
			{
				TypingUpdate();
			}
			currentCharIndex = length;
			isTyping = false;
			typingTimer = 0f;
		}
		TypeSoundEffect();
	}

	private void ChatMessageSend(string playerName)
	{
		string text = ColorUtility.ToHtmlStringRGB(SemiFunc.PlayerGetFromName(playerName).playerAvatarVisuals.color);
		currentNickname = "\n\n<color=#" + text + "><b>" + playerName + ":</b></color>\n";
		onChatMessage.Invoke();
		chatDeactivatedTimer = 3f;
		chatMessage.text = "";
	}

	public void SelfDestructPlayersOutsideTruck()
	{
		if (!SemiFunc.IsMultiplayer())
		{
			SelfDestructPlayersOutsideTruckRPC();
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("SelfDestructPlayersOutsideTruckRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	public void SelfDestructPlayersOutsideTruckRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			selfDestructingPlayers = true;
			ChatManager.instance.PossessLeftBehind();
		}
	}

	[PunRPC]
	public void ChatMessageSendRPC(string playerName, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			ChatMessageSend(playerName);
		}
	}

	public void GotoNextLevel()
	{
		EngineStartSound();
		RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false);
	}

	public void ShopCompleted()
	{
		StartCoroutine(ShopGotoNextLevel());
	}

	private void EngineStartSound()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("EngineStartRPC", RpcTarget.All);
			}
			else
			{
				EngineStartRPC();
			}
		}
	}

	[PunRPC]
	public void EngineStartRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			engineSuccessSound.Play(textMesh.transform.position);
		}
	}

	public IEnumerator ShopGotoNextLevel()
	{
		yield return new WaitForSeconds(0.5f);
		RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false);
	}

	private void PageTransitionEffect()
	{
		background.color = transitionBackgroundColor;
		backgroundColorChangeTimer = 0f;
		backgroundColorChangeDuration = 0.5f;
		newPageSound.Play(textMesh.transform.position);
	}

	public void GotoPage(int pageIndex)
	{
		if (GameManager.instance.gameMode == 0)
		{
			GotoPageLogic(pageIndex);
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("GotoPageRPC", RpcTarget.All, pageIndex);
		}
	}

	[PunRPC]
	public void MessageSendCustomRPC(string playerName, string message, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (isTyping)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (GameManager.Multiplayer() && _info.Sender != PhotonNetwork.MasterClient)
		{
			flag = true;
			foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
			{
				if (item.photonView.Owner == _info.Sender)
				{
					playerName = item.playerName;
				}
			}
		}
		if (playerName != "" || flag)
		{
			foreach (PlayerAvatar item2 in SemiFunc.PlayerGetList())
			{
				if (item2.playerName == playerName)
				{
					string text = ColorUtility.ToHtmlStringRGB(item2.playerAvatarVisuals.color);
					currentNickname = "\n\n<color=#" + text + "><b>" + playerName + ":</b></color>\n";
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				return;
			}
		}
		else
		{
			currentNickname = nicknameTaxman;
		}
		TextMeshProUGUI textMeshProUGUI = textMesh;
		textMeshProUGUI.text = textMeshProUGUI.text + currentNickname + SemiFunc.EmojiText(message);
		newLineSound.Play(textMesh.transform.position);
		currentNickname = nicknameTaxman;
	}

	public void MessageSendCustom(string playerName, string message, int emojis)
	{
		if (playerName != "")
		{
			List<string> list = new List<string> { "{:)}", "{:D}", "{:P}", "{eyes}", "{:o}", "{heart}" };
			List<string> list2 = new List<string> { "{:(}", "{:'(}", "{heartbreak}", "{fedup}" };
			string text = "";
			if (emojis != 0)
			{
				List<string> list3 = list;
				if (emojis == 1)
				{
					list3 = list;
				}
				if (emojis == 2)
				{
					list3 = list2;
				}
				text += list3[UnityEngine.Random.Range(0, list3.Count)];
				if (UnityEngine.Random.Range(0, 2) == 1)
				{
					text += list3[UnityEngine.Random.Range(0, list3.Count)];
				}
				if (UnityEngine.Random.Range(0, 5) == 1)
				{
					text += list3[UnityEngine.Random.Range(0, list3.Count)];
				}
				if (UnityEngine.Random.Range(0, 30) == 1)
				{
					text = list3[UnityEngine.Random.Range(0, list3.Count)];
					text += text;
					text += text;
				}
			}
			message += text;
		}
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("MessageSendCustomRPC", RpcTarget.All, playerName, message);
		}
		else
		{
			MessageSendCustomRPC(playerName, message);
		}
	}

	private void GotoPageLogic(int pageIndex)
	{
		currentPageIndex = pageIndex;
		currentLineIndex = 0;
		currentCharIndex = 0;
		typingTimer = 0f;
		isTyping = true;
		foreach (TextPages page in pages)
		{
			foreach (TextLine textLine in page.textLines)
			{
				textLine.text = textLine.textOriginal;
			}
		}
		NextLine(currentLineIndex);
	}

	[PunRPC]
	public void GotoPageRPC(int pageIndex)
	{
		GotoPageLogic(pageIndex);
	}

	public void ReleaseChat()
	{
		if (GameManager.instance.gameMode == 0)
		{
			ReleaseChatRPC();
		}
		else
		{
			photonView.RPC("ReleaseChatRPC", RpcTarget.All);
		}
	}

	private string FormatDollarValueStrings(string valueString)
	{
		valueString = SemiFunc.DollarGetString(int.Parse(valueString));
		return valueString;
	}

	[PunRPC]
	public void ReleaseChatRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			chatActive = false;
			chatMessageTimer = 0f;
			chatCharacterIndex = 0;
			chatMessage.text = "";
		}
	}

	private void GotoPageAfterPageIsDone(int pageIndex)
	{
		nextPageOverride = pageIndex;
	}

	private void PlayerChatBoxStateUpdate(PlayerChatBoxState _state)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("PlayerChatBoxStateUpdateRPC", RpcTarget.All, _state);
			}
			else
			{
				PlayerChatBoxStateUpdateRPC(_state);
			}
		}
	}

	public void PlayerChatBoxStateUpdateToLockedDestroySlackers()
	{
		PlayerChatBoxStateUpdate(PlayerChatBoxState.LockedDestroySlackers);
	}

	public void PlayerChatBoxStateUpdateToLockedStartingTruck()
	{
		PlayerChatBoxStateUpdate(PlayerChatBoxState.LockedStartingTruck);
	}

	[PunRPC]
	private void PlayerChatBoxStateUpdateRPC(PlayerChatBoxState _state)
	{
		playerChatBoxState = _state;
		playerChatBoxStateStart = true;
	}

	private void PlayerChatBoxStateIdle()
	{
		if (playerChatBoxStateStart)
		{
			truckScreenLocked.LockChatToggle(_lock: false);
			staticGrabCollider.SetActive(value: true);
			playerChatBoxStateStart = false;
		}
	}

	private void PlayerChatBoxStateLockedStartingTruck()
	{
		if (playerChatBoxStateStart)
		{
			ForceReleaseChat();
			Color darkColor = new Color(0.8f, 0.2f, 0.1f, 1f);
			Color lightColor = new Color(1f, 0.8f, 0f, 1f);
			string lockedText = "STARTING ENGINE";
			if (SemiFunc.RunIsLobby())
			{
				lockedText = "HITTING THE ROAD";
			}
			truckScreenLocked.LockChatToggle(_lock: true, lockedText, lightColor, darkColor);
			playerChatBoxStateStart = false;
		}
		if (!SemiFunc.RunIsLobby())
		{
			if (engineSoundTimer > 0f)
			{
				engineSoundTimer -= Time.deltaTime;
				return;
			}
			engineSoundTimer = UnityEngine.Random.Range(2f, 4f);
			engineRevSound.Play(truckScreenLocked.transform.position);
		}
	}

	private void PlayerChatBoxStateLockedDestroySlackers()
	{
		if (playerChatBoxStateStart)
		{
			ForceReleaseChat();
			Color darkColor = new Color(0.4f, 0f, 0.3f, 1f);
			Color lightColor = new Color(1f, 0f, 0f, 1f);
			truckScreenLocked.LockChatToggle(_lock: true, "DESTROYING SLACKERS", lightColor, darkColor);
			playerChatBoxStateStart = false;
		}
	}

	private void PlayerChatBoxStateMachine()
	{
		switch (playerChatBoxState)
		{
		case PlayerChatBoxState.Idle:
			PlayerChatBoxStateIdle();
			break;
		case PlayerChatBoxState.LockedStartingTruck:
			PlayerChatBoxStateLockedStartingTruck();
			break;
		case PlayerChatBoxState.LockedDestroySlackers:
			PlayerChatBoxStateLockedDestroySlackers();
			break;
		case PlayerChatBoxState.Typing:
			break;
		}
	}
}
