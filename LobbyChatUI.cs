using TMPro;
using UnityEngine;

public class LobbyChatUI : SemiUI
{
	private TTSVoice ttsVoice;

	private MenuPlayerListed menuPlayerListed;

	private float prevWordTime;

	public bool isSpectate;

	public bool isGameplay;

	public TextMeshProUGUI spectateName;

	private RectTransform rectTransform;

	private float chatOffsetXPos;

	private bool offsetFetched;

	private string prevPlayerName = "";

	protected override void Start()
	{
		base.Start();
		rectTransform = GetComponent<RectTransform>();
		menuPlayerListed = GetComponentInParent<MenuPlayerListed>();
	}

	protected override void Update()
	{
		base.Update();
		if (isGameplay && (SemiFunc.RunIsLobbyMenu() || ((bool)PlayerAvatar.instance && PlayerAvatar.instance.isDisabled)))
		{
			uiText.text = "";
			return;
		}
		if ((bool)spectateName && prevPlayerName != spectateName.text)
		{
			offsetFetched = false;
		}
		if (isSpectate)
		{
			SemiUIScoot(new Vector2(-200f + chatOffsetXPos, 0f));
		}
		if (isSpectate && (bool)spectateName && !offsetFetched)
		{
			float num = spectateName.preferredWidth;
			if (num > 155f)
			{
				num = 155f;
			}
			rectTransform.localPosition = spectateName.rectTransform.localPosition + new Vector3(num, 25f, 0f);
			chatOffsetXPos = rectTransform.localPosition.x;
			offsetFetched = true;
			prevPlayerName = spectateName.text;
		}
		if (!ttsVoice)
		{
			if (!isGameplay)
			{
				if ((bool)menuPlayerListed.playerAvatar.voiceChat && menuPlayerListed.playerAvatar.voiceChat.TTSinstantiated)
				{
					ttsVoice = menuPlayerListed.playerAvatar.voiceChat.ttsVoice;
				}
			}
			else if ((bool)PlayerAvatar.instance && (bool)PlayerAvatar.instance.voiceChat && (bool)PlayerAvatar.instance.voiceChat.ttsVoice)
			{
				ttsVoice = PlayerAvatar.instance.voiceChat.ttsVoice;
			}
			return;
		}
		if (prevWordTime != ttsVoice.currentWordTime)
		{
			SemiUITextFlashColor(Color.yellow, 0.2f);
			SemiUISpringShakeY(4f, 5f, 0.2f);
			prevWordTime = ttsVoice.currentWordTime;
			uiText.text = ttsVoice.voiceText;
		}
		if (isSpectate && (bool)menuPlayerListed.playerAvatar && (bool)menuPlayerListed.playerAvatar.playerDeathHead && menuPlayerListed.playerAvatar.playerDeathHead.spectated)
		{
			uiText.text = "";
		}
		else if (ttsVoice.isSpeaking)
		{
			uiText.text = ttsVoice.voiceText;
		}
		else
		{
			uiText.text = "";
		}
	}
}
