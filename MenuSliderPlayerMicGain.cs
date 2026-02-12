using UnityEngine;

public class MenuSliderPlayerMicGain : MonoBehaviour
{
	internal MenuSlider menuSlider;

	internal PlayerAvatar playerAvatar;

	private float currentValue;

	private bool fetched;

	private void Start()
	{
		menuSlider = GetComponent<MenuSlider>();
		SetSlider(0.5f);
	}

	private void Update()
	{
		if (!SemiFunc.IsMultiplayer())
		{
			return;
		}
		if (!playerAvatar.voiceChatFetched)
		{
			SetSlider(0.5f);
			return;
		}
		if ((currentValue != (float)menuSlider.currentValue || !fetched) && playerAvatar.voiceChatFetched)
		{
			if (!fetched)
			{
				playerAvatar.voiceChat.voiceGain = GameManager.instance.PlayerMicrophoneSettingGet(playerAvatar.steamID);
				MenuButtonKick componentInChildren = GetComponentInChildren<MenuButtonKick>();
				if ((bool)componentInChildren)
				{
					componentInChildren.Setup(playerAvatar);
				}
				SetSlider(playerAvatar.voiceChat.voiceGain);
				fetched = true;
			}
			currentValue = menuSlider.currentValue;
			playerAvatar.voiceChat.voiceGain = currentValue / 200f;
			GameManager.instance.PlayerMicrophoneSettingSet(playerAvatar.steamID, playerAvatar.voiceChat.voiceGain);
		}
		menuSlider.ExtraBarSet(playerAvatar.voiceChat.clipLoudnessNoTTS * 5f);
	}

	private void SetSlider(float _value)
	{
		menuSlider.settingsValue = _value;
		menuSlider.currentValue = (int)(_value * 200f);
		menuSlider.SetBar(menuSlider.settingsValue);
		menuSlider.SetBarScaleInstant();
		currentValue = menuSlider.currentValue;
	}

	public void SliderNameSet(string name)
	{
		menuSlider = GetComponent<MenuSlider>();
		menuSlider.elementName = name;
		menuSlider.elementNameText.text = name;
	}
}
