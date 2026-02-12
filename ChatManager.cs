using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ChatManager : MonoBehaviour
{
	public enum PossessChatID
	{
		None,
		LovePotion,
		Ouch,
		SelfDestruct,
		Betrayal,
		SelfDestructCancel
	}

	public enum ChatState
	{
		Inactive,
		Active,
		Possessed,
		Send
	}

	public class PossessMessage
	{
		public PossessChatID possessChatID;

		public string message;

		public float typingSpeed;

		public Color possessColor;

		public float messageDelay;

		public bool sendInTaxmanChat;

		public int sendInTaxmanChatEmojiInt;

		public UnityEvent eventExecutionAfterMessageIsDone;

		public PossessMessage(PossessChatID _possessChatID, string message, float typingSpeed, Color possessColor, float messageDelay, bool sendInTaxmanChat, int sendInTaxmanChatEmojiInt, UnityEvent eventExecutionAfterMessageIsDone)
		{
			possessChatID = _possessChatID;
			this.message = message;
			this.typingSpeed = typingSpeed;
			this.possessColor = possessColor;
			this.messageDelay = messageDelay;
			this.sendInTaxmanChat = sendInTaxmanChat;
			this.sendInTaxmanChatEmojiInt = sendInTaxmanChatEmojiInt;
			this.eventExecutionAfterMessageIsDone = eventExecutionAfterMessageIsDone;
		}
	}

	public class PossessMessageBatch
	{
		public List<PossessMessage> messages = new List<PossessMessage>();

		public int messagePrio;

		public bool isProcessing;

		public PossessMessageBatch(int messagePrio)
		{
			this.messagePrio = messagePrio;
		}
	}

	public static ChatManager instance;

	internal bool chatActive;

	internal bool localPlayerAvatarFetched;

	internal bool textMeshFetched;

	internal PlayerAvatar playerAvatar;

	internal string prevChatMessage = "";

	internal string chatMessage = "";

	public TextMeshProUGUI chatText;

	private float spamTimer;

	private List<string> chatHistory = new List<string>();

	private int chatHistoryIndex;

	private float possessLetterDelay;

	private bool wasPossessed;

	private int wasPossessedPrio;

	private bool betrayalActive;

	internal PossessChatID activePossession;

	internal float activePossessionTimer;

	public PossessChatID currentPossessChatID;

	private List<PossessMessageBatch> possessBatchQueue = new List<PossessMessageBatch>();

	private PossessMessageBatch currentBatch;

	private int currentMessageIndex;

	private bool isScheduling;

	private PossessMessageBatch scheduledBatch;

	private float isSpeakingTimer;

	private ChatState chatState;

	private PossessMessage currentPossessMessage;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void SetChatColor(Color color)
	{
		chatText.color = color;
	}

	public void ClearAllChatBatches()
	{
		possessBatchQueue.Clear();
		currentBatch = null;
	}

	public void ForceSendMessage(string _message)
	{
		chatMessage = _message;
		ForceConfirmChat();
	}

	private void CharRemoveEffect()
	{
		ChatUI.instance.SemiUITextFlashColor(Color.red, 0.2f);
		ChatUI.instance.SemiUISpringShakeX(5f, 5f, 0.2f);
		MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Dud, null, 2f, 1f, soundOnly: true);
	}

	public void AddLetterToChat(string letter)
	{
		prevChatMessage = chatMessage;
		chatMessage += letter;
		chatText.text = chatMessage;
	}

	public void ForceConfirmChat()
	{
		StateSet(ChatState.Send);
	}

	private void ChatReset()
	{
		chatMessage = "";
	}

	private void PossessChatLovePotion()
	{
		playerAvatar.OverridePupilSize(3f, 4, 1f, 1f, 15f, 0.3f);
		playerAvatar.playerHealth.EyeMaterialOverride(PlayerHealth.EyeOverrideState.Love, 0.25f, 0);
	}

	private void PossessChatCustomLogic()
	{
		switch (activePossession)
		{
		case PossessChatID.LovePotion:
			PossessChatLovePotion();
			break;
		case PossessChatID.SelfDestruct:
			if (!playerAvatar)
			{
				return;
			}
			playerAvatar.playerHealth.EyeMaterialOverride(PlayerHealth.EyeOverrideState.Red, 0.25f, 0);
			break;
		case PossessChatID.Betrayal:
			if (!playerAvatar)
			{
				return;
			}
			playerAvatar.playerHealth.EyeMaterialOverride(PlayerHealth.EyeOverrideState.Red, 0.25f, 0);
			break;
		case PossessChatID.SelfDestructCancel:
			if (!playerAvatar)
			{
				return;
			}
			playerAvatar.playerHealth.EyeMaterialOverride(PlayerHealth.EyeOverrideState.Green, 0.25f, 0);
			break;
		}
		if (isSpeakingTimer > 0f)
		{
			isSpeakingTimer -= Time.deltaTime;
		}
		if (isSpeakingTimer < 0.2f && (bool)playerAvatar && (bool)playerAvatar.voiceChat && (bool)playerAvatar.voiceChat.ttsVoice && playerAvatar.voiceChat.ttsVoice.isSpeaking)
		{
			isSpeakingTimer = 0.2f;
		}
		if (isSpeakingTimer <= 0f && possessBatchQueue.Count == 0 && currentBatch == null)
		{
			currentPossessChatID = PossessChatID.None;
		}
	}

	public void PossessChatScheduleStart(int messagePrio)
	{
		bool flag = false;
		if (currentBatch != null && messagePrio < currentBatch.messagePrio)
		{
			InterruptCurrentPossessBatch();
			ChatReset();
			flag = true;
		}
		if (currentBatch == null)
		{
			flag = true;
		}
		if (flag)
		{
			isScheduling = true;
			scheduledBatch = new PossessMessageBatch(messagePrio);
		}
	}

	public void PossessChatScheduleEnd()
	{
		if (isScheduling)
		{
			isScheduling = false;
			EnqueuePossessBatch(scheduledBatch);
			scheduledBatch = null;
		}
	}

	public void PossessChat(PossessChatID _possessChatID, string message, float typingSpeed, Color _possessColor, float _messageDelay = 0f, bool sendInTaxmanChat = false, int sendInTaxmanChatEmojiInt = 0, UnityEvent eventExecutionAfterMessageIsDone = null)
	{
		isSpeakingTimer = 1f;
		PossessMessage item = new PossessMessage(_possessChatID, message, typingSpeed, _possessColor, _messageDelay, sendInTaxmanChat, sendInTaxmanChatEmojiInt, eventExecutionAfterMessageIsDone);
		if (isScheduling)
		{
			scheduledBatch.messages.Add(item);
		}
	}

	private void EnqueuePossessBatch(PossessMessageBatch batch)
	{
		if (currentBatch == null)
		{
			StartPossessBatch(batch);
		}
		else if (batch.messagePrio < currentBatch.messagePrio)
		{
			InterruptCurrentPossessBatch();
			StartPossessBatch(batch);
		}
		else if (batch.messagePrio <= currentBatch.messagePrio)
		{
			possessBatchQueue.Add(batch);
		}
	}

	private void StartPossessBatch(PossessMessageBatch batch)
	{
		currentBatch = batch;
		currentBatch.isProcessing = true;
		currentMessageIndex = 0;
		StartPossessMessage(currentBatch.messages[currentMessageIndex]);
	}

	private void InterruptCurrentPossessBatch()
	{
		ChatReset();
		currentBatch = null;
		possessBatchQueue.Clear();
		wasPossessed = false;
		wasPossessedPrio = 0;
	}

	private void StartPossessMessage(PossessMessage message)
	{
		ChatReset();
		possessLetterDelay = 0f;
		SetChatColor(message.possessColor);
		currentPossessMessage = message;
		StateSet(ChatState.Possessed);
		currentPossessChatID = message.possessChatID;
	}

	private void PossessionReset()
	{
		currentPossessChatID = PossessChatID.None;
		currentBatch = null;
		possessBatchQueue.Clear();
		wasPossessed = false;
		wasPossessedPrio = 0;
		ChatReset();
		StateSet(ChatState.Inactive);
	}

	private void TypeEffect(Color _color)
	{
		ChatUI.instance.SemiUITextFlashColor(_color, 0.2f);
		ChatUI.instance.SemiUISpringShakeY(2f, 5f, 0.2f);
		MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 2f, 0.2f, soundOnly: true);
	}

	public void TumbleInterruption()
	{
		if (!(activePossessionTimer > 0f))
		{
			PossessionReset();
			if ((bool)playerAvatar && playerAvatar.voiceChatFetched && playerAvatar.voiceChat.ttsVoice.isSpeaking)
			{
				List<string> list = new List<string>
				{
					"Ouch! Ouch! Ouch!", "Ow! Ow! Ow!", "Oof! Oof! Oof!", "Owie! Wowie! Zowie!", "Ouchie! Ouchie! Ouchie!", "error error error", "system error", "fatal error", "error 404", "runtime error",
					"imma falling", "falling over", "ooooooooh!", "oh nooooo!", "AAAAAAH! AAH!", "AAAAAAAAAAAAAAH!", "AAAAAAAAAAAAAAAAAAAAAAAAAAAH!", "OH! OH! OH!", "AH! AH! AH!"
				};
				int index = Random.Range(0, list.Count);
				string message = list[index];
				PossessChatScheduleStart(3);
				PossessChat(PossessChatID.Ouch, message, 1f, Color.red);
				PossessChatScheduleEnd();
			}
		}
	}

	private void StateInactive()
	{
		ChatUI.instance.Hide();
		chatMessage = "";
		chatActive = false;
		if ((!MenuManager.instance || !MenuManager.instance.currentMenuPage || (MenuManager.instance.currentMenuPage.menuPageIndex != MenuPageIndex.Escape && MenuManager.instance.currentMenuPage.menuPageIndex != MenuPageIndex.Settings)) && SemiFunc.NoTextInputsActive() && SemiFunc.InputDown(InputKey.Chat))
		{
			TutorialDirector.instance.playerChatted = true;
			MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Action, null, 1f, 1f, soundOnly: true);
			chatActive = !chatActive;
			StateSet(ChatState.Active);
			chatHistoryIndex = 0;
		}
	}

	private void StateActive()
	{
		if (SemiFunc.InputDown(InputKey.Back))
		{
			StateSet(ChatState.Inactive);
			ChatUI.instance.SemiUISpringShakeX(10f, 10f, 0.3f);
			ChatUI.instance.SemiUISpringScale(0.05f, 5f, 0.2f);
			MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Deny, null, 1f, 1f, soundOnly: true);
			return;
		}
		if (Input.GetKeyDown(KeyCode.UpArrow) && chatHistory.Count > 0)
		{
			if (chatHistoryIndex > 0)
			{
				chatHistoryIndex--;
			}
			else
			{
				chatHistoryIndex = chatHistory.Count - 1;
			}
			chatMessage = chatHistory[chatHistoryIndex];
			chatText.text = chatMessage;
			ChatUI.instance.SemiUITextFlashColor(Color.cyan, 0.2f);
			ChatUI.instance.SemiUISpringShakeY(2f, 5f, 0.2f);
			MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 1f, 0.2f, soundOnly: true);
		}
		if (Input.GetKeyDown(KeyCode.DownArrow) && chatHistory.Count > 0)
		{
			if (chatHistoryIndex < chatHistory.Count - 1)
			{
				chatHistoryIndex++;
			}
			else
			{
				chatHistoryIndex = 0;
			}
			chatMessage = chatHistory[chatHistoryIndex];
			chatText.text = chatMessage;
			ChatUI.instance.SemiUITextFlashColor(Color.cyan, 0.2f);
			ChatUI.instance.SemiUISpringShakeY(2f, 5f, 0.2f);
			MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 1f, 0.2f, soundOnly: true);
		}
		SemiFunc.InputDisableMovement();
		if (SemiFunc.InputDown(InputKey.ChatDelete))
		{
			if (chatMessage.Length > 0)
			{
				chatMessage = chatMessage.Remove(chatMessage.Length - 1);
				chatText.text = chatMessage;
				CharRemoveEffect();
			}
		}
		else
		{
			if (chatMessage == "\b")
			{
				chatMessage = "";
			}
			prevChatMessage = chatMessage;
			string text = chatMessage;
			chatMessage += Input.inputString;
			chatMessage = chatMessage.Replace("\n", "");
			if (chatMessage.Length > 50)
			{
				ChatUI.instance.SemiUITextFlashColor(Color.red, 0.2f);
				ChatUI.instance.SemiUISpringShakeX(10f, 10f, 0.3f);
				ChatUI.instance.SemiUISpringScale(0.05f, 5f, 0.2f);
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Deny, null, 1f, 1f, soundOnly: true);
				chatMessage = text;
			}
			if (prevChatMessage != chatMessage)
			{
				bool flag = false;
				if (Input.inputString == "\b")
				{
					chatMessage = chatMessage.Remove(Mathf.Max(chatMessage.Length - 2, 0));
					flag = true;
				}
				else
				{
					chatText.text = chatMessage;
				}
				chatMessage = chatMessage.Replace("\r", "");
				prevChatMessage = chatMessage;
				if (!flag)
				{
					TypeEffect(Color.yellow);
				}
				else
				{
					CharRemoveEffect();
				}
			}
		}
		if (SemiFunc.InputDown(InputKey.Confirm))
		{
			if (chatMessage != "")
			{
				StateSet(ChatState.Send);
			}
			else
			{
				StateSet(ChatState.Inactive);
			}
		}
		if (Mathf.Sin(Time.time * 10f) > 0f)
		{
			chatText.text = chatMessage + "|";
		}
		else
		{
			chatText.text = chatMessage;
		}
		if (SemiFunc.InputDown(InputKey.Back))
		{
			StateSet(ChatState.Inactive);
		}
	}

	private void StatePossessed()
	{
		chatActive = true;
		spamTimer = 0f;
		if (currentPossessMessage != null)
		{
			SetChatColor(currentPossessMessage.possessColor);
		}
		if (currentPossessMessage == null)
		{
			currentMessageIndex++;
			if (currentBatch != null && currentMessageIndex < currentBatch.messages.Count)
			{
				StartPossessMessage(currentBatch.messages[currentMessageIndex]);
				return;
			}
			if (currentBatch != null && currentBatch.messages.Count == currentMessageIndex && currentBatch.isProcessing)
			{
				currentBatch.isProcessing = false;
				currentBatch = null;
			}
			if (possessBatchQueue.Count > 0)
			{
				StartPossessBatch(possessBatchQueue[0]);
				possessBatchQueue.RemoveAt(0);
			}
			else
			{
				StateSet(ChatState.Inactive);
				currentBatch = null;
			}
			return;
		}
		bool flag = false;
		if (currentPossessMessage.typingSpeed == -1f)
		{
			flag = true;
		}
		if (possessLetterDelay <= 0f)
		{
			if (currentPossessMessage.message.Length > 0 && !flag)
			{
				string letter = currentPossessMessage.message[0].ToString();
				currentPossessMessage.message = currentPossessMessage.message.Substring(1);
				possessLetterDelay = Random.Range(0.005f, 0.05f);
				TypeEffect(currentPossessMessage.possessColor);
				AddLetterToChat(letter);
			}
			else
			{
				if (isSpeakingTimer > 0f && wasPossessed && wasPossessedPrio <= currentBatch.messagePrio)
				{
					return;
				}
				if (currentPossessMessage.messageDelay > 0f)
				{
					currentPossessMessage.messageDelay -= Time.deltaTime;
					return;
				}
				if (flag)
				{
					chatMessage = currentPossessMessage.message;
				}
				wasPossessed = true;
				if (currentBatch != null)
				{
					wasPossessedPrio = currentBatch.messagePrio;
				}
				StateSet(ChatState.Send);
			}
		}
		else
		{
			possessLetterDelay -= Time.deltaTime * currentPossessMessage.typingSpeed;
			if (currentPossessMessage.typingSpeed == -1f)
			{
				possessLetterDelay = 0f;
			}
		}
	}

	private void SelfDestruct()
	{
		float delay = Random.Range(0.2f, 3f);
		StartCoroutine(SelfDestructCoroutine(delay));
	}

	private void BetrayalSelfDestruct()
	{
		float delay = Random.Range(0.2f, 3f);
		StartCoroutine(SelfDestructCoroutine(delay));
	}

	public void PossessLeftBehind()
	{
		if ((bool)playerAvatar && !playerAvatar.isDisabled && !playerAvatar.RoomVolumeCheck.inTruck)
		{
			betrayalActive = true;
			PossessChatScheduleStart(2);
			string message = SemiFunc.MessageGeneratedGetLeftBehind();
			PossessChat(PossessChatID.Betrayal, message, 0.5f, Color.red, 0f, sendInTaxmanChat: true, 2);
			PossessChat(PossessChatID.Betrayal, "I need to get to the truck in...", 0.4f, Color.red, 0f, sendInTaxmanChat: true, 2);
			PossessChat(PossessChatID.Betrayal, "10...", 0.25f, Color.red, 0f, sendInTaxmanChat: true, 2);
			PossessChat(PossessChatID.Betrayal, "9...", 0.25f, Color.red, 0.3f, sendInTaxmanChat: true, 2);
			PossessChat(PossessChatID.Betrayal, "8...", 0.25f, Color.red, 0.3f, sendInTaxmanChat: true, 2);
			PossessChat(PossessChatID.Betrayal, "7...", 0.25f, Color.red, 0.3f, sendInTaxmanChat: true, 2);
			PossessChat(PossessChatID.Betrayal, "6...", 0.25f, Color.red, 0.3f, sendInTaxmanChat: true, 2);
			PossessChat(PossessChatID.Betrayal, "5...", 0.25f, Color.red, 0.3f, sendInTaxmanChat: true, 2);
			PossessChat(PossessChatID.Betrayal, "4...", 0.25f, Color.red, 0.3f, sendInTaxmanChat: true, 2);
			PossessChat(PossessChatID.Betrayal, "3...", 0.25f, Color.red, 0.3f, sendInTaxmanChat: true, 2);
			PossessChat(PossessChatID.Betrayal, "2...", 0.25f, Color.red, 0.3f, sendInTaxmanChat: true, 2);
			PossessChat(PossessChatID.Betrayal, "1...", 0.5f, Color.red, 0.3f, sendInTaxmanChat: true, 2);
			UnityEvent unityEvent = new UnityEvent();
			unityEvent.AddListener(BetrayalSelfDestruct);
			List<string> list = new List<string> { "betrayal", "betrayal detected", "i'm sorry", "I failed", "oh no, not again", "teamwork makes the dream work", "I thought we were friends", "I thought we were a team", "I thought we were in this together", "I thought we were a family" };
			string message2 = list[Random.Range(0, list.Count)];
			PossessChat(PossessChatID.SelfDestruct, message2, 2f, Color.red, 0f, sendInTaxmanChat: true, 2, unityEvent);
			PossessChatScheduleEnd();
		}
	}

	public void PossessCancelSelfDestruction()
	{
		if ((bool)playerAvatar && !playerAvatar.isDisabled)
		{
			PossessChatScheduleEnd();
			possessBatchQueue.Clear();
			currentBatch = null;
			betrayalActive = false;
			PossessChatScheduleStart(1);
			PossessChat(PossessChatID.SelfDestructCancel, "SELF DESTRUCT SEQUENCE CANCELLED!", 2f, Color.green);
			PossessChatScheduleEnd();
		}
	}

	public void PossessSelfDestruction()
	{
		if ((bool)playerAvatar && !playerAvatar.isDisabled)
		{
			PossessChatScheduleStart(-1);
			UnityEvent unityEvent = new UnityEvent();
			unityEvent.AddListener(SelfDestruct);
			List<string> list = new List<string>
			{
				"i'm out", "Farewell", "Adieu", "sayonara", "Auf Wiedersehen", "adios", "ciao", "Au Revoir", "hasta la vista", "see You Later",
				"later", "peace OUT", "catch you later", "later gator", "toodles", "bye bye", "bye", "AAAAAAAAAAAAH!", "AAAAAAAAAAAAAAAAAAAAAAAH!", "bye... ... oh?",
				"this will hurt", "it's over for me", "why me?", "I'm sorry", "i see the light", "sad but necessary", "HEJ DÅ!"
			};
			string message = list[Random.Range(0, list.Count)];
			PossessChat(PossessChatID.SelfDestruct, message, 2f, Color.red, 0f, sendInTaxmanChat: true, 2, unityEvent);
			PossessChatScheduleEnd();
		}
	}

	private IEnumerator BetrayalSelfDestructCoroutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		if (betrayalActive)
		{
			PlayerAvatar.instance.playerHealth.health = 0;
			PlayerAvatar.instance.playerHealth.Hurt(1, savingGrace: false);
		}
	}

	private IEnumerator SelfDestructCoroutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		PlayerAvatar.instance.playerHealth.health = 0;
		PlayerAvatar.instance.playerHealth.Hurt(1, savingGrace: false);
	}

	public bool IsPossessed(PossessChatID _possessChatID)
	{
		return activePossession == _possessChatID;
	}

	private void StateSend()
	{
		bool possessed = false;
		if (currentPossessMessage != null && currentPossessMessage.sendInTaxmanChat && (bool)TruckScreenText.instance)
		{
			TruckScreenText.instance.MessageSendCustom(PlayerController.instance.playerName, chatMessage, currentPossessMessage.sendInTaxmanChatEmojiInt);
		}
		if (currentPossessMessage != null)
		{
			possessed = true;
		}
		MessageSend(possessed);
		if (currentPossessMessage != null && currentPossessMessage.eventExecutionAfterMessageIsDone != null)
		{
			currentPossessMessage.eventExecutionAfterMessageIsDone.Invoke();
		}
		currentPossessMessage = null;
		StateSet(ChatState.Possessed);
	}

	private void StateSet(ChatState state)
	{
		chatState = state;
	}

	private void ImportantFetches()
	{
		if (!chatText)
		{
			textMeshFetched = false;
		}
		if (!playerAvatar)
		{
			localPlayerAvatarFetched = false;
		}
		if (!textMeshFetched && (bool)ChatUI.instance && (bool)ChatUI.instance.chatText)
		{
			chatText = ChatUI.instance.chatText;
			textMeshFetched = true;
		}
		if (localPlayerAvatarFetched)
		{
			return;
		}
		if (SemiFunc.IsMultiplayer())
		{
			List<PlayerAvatar> list = SemiFunc.PlayerGetList();
			if (list.Count <= 0)
			{
				return;
			}
			{
				foreach (PlayerAvatar item in list)
				{
					if (item.isLocal)
					{
						playerAvatar = item;
						localPlayerAvatarFetched = true;
						break;
					}
				}
				return;
			}
		}
		playerAvatar = PlayerAvatar.instance;
		localPlayerAvatarFetched = true;
	}

	private void NewLevelResets()
	{
		betrayalActive = false;
		localPlayerAvatarFetched = false;
		textMeshFetched = false;
		PossessionReset();
	}

	private void PossessionActive()
	{
		if (activePossessionTimer <= 0f)
		{
			activePossession = PossessChatID.None;
		}
		if (activePossessionTimer > 0f)
		{
			activePossessionTimer -= Time.deltaTime;
		}
		if (currentPossessChatID != PossessChatID.None || (activePossession != PossessChatID.None && isSpeakingTimer > 0f))
		{
			activePossessionTimer = 0.5f;
			activePossession = currentPossessChatID;
		}
	}

	private void Update()
	{
		PossessionActive();
		if ((bool)playerAvatar && playerAvatar.isDisabled && (possessBatchQueue.Count > 0 || currentBatch != null))
		{
			InterruptCurrentPossessBatch();
		}
		if (!SemiFunc.IsMultiplayer())
		{
			ChatUI.instance.Hide();
			return;
		}
		if (!LevelGenerator.Instance.Generated)
		{
			NewLevelResets();
			return;
		}
		ImportantFetches();
		PossessChatCustomLogic();
		if (!textMeshFetched || !localPlayerAvatarFetched)
		{
			return;
		}
		switch (chatState)
		{
		case ChatState.Inactive:
			StateInactive();
			break;
		case ChatState.Active:
			StateActive();
			break;
		case ChatState.Possessed:
			StatePossessed();
			break;
		case ChatState.Send:
			StateSend();
			break;
		}
		PossessChatCustomLogic();
		if (!SemiFunc.IsMultiplayer())
		{
			if (chatState != ChatState.Inactive)
			{
				StateSet(ChatState.Inactive);
			}
			chatActive = false;
			return;
		}
		if (spamTimer > 0f)
		{
			spamTimer -= Time.deltaTime;
		}
		if (SemiFunc.FPSImpulse15() && betrayalActive && PlayerController.instance.playerAvatarScript.RoomVolumeCheck.inTruck)
		{
			PossessCancelSelfDestruction();
		}
	}

	public bool StateIsActive()
	{
		return chatState == ChatState.Active;
	}

	public bool StateIsPossessed()
	{
		return chatState == ChatState.Possessed;
	}

	public bool StateIsSend()
	{
		return chatState == ChatState.Send;
	}

	public bool StateIsInactive()
	{
		return chatState == ChatState.Inactive;
	}

	private void MessageSend(bool _possessed = false)
	{
		if (!(chatMessage == "") && spamTimer <= 0f)
		{
			playerAvatar.ChatMessageSend(chatMessage);
			if (!_possessed)
			{
				chatHistory.Add(chatMessage);
			}
			if (chatHistory.Count > 20)
			{
				chatHistory.RemoveAt(0);
			}
			chatHistory = chatHistory.AsEnumerable().Reverse().Distinct()
				.Reverse()
				.ToList();
			ChatReset();
			chatText.text = chatMessage;
			chatActive = false;
			isSpeakingTimer = 0.2f;
			ChatUI.instance.SemiUITextFlashColor(Color.green, 0.2f);
			ChatUI.instance.SemiUISpringShakeX(10f, 10f, 0.3f);
			ChatUI.instance.SemiUISpringScale(0.05f, 5f, 0.2f);
			MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm, null, 1f, 1f, soundOnly: true);
			spamTimer = 1f;
		}
	}
}
