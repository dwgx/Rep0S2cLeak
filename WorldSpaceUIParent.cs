using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceUIParent : MonoBehaviour
{
	public static WorldSpaceUIParent instance;

	internal CanvasGroup canvasGroup;

	[Space]
	public GameObject valueLostPrefab;

	public GameObject TTSPrefab;

	public GameObject playerNamePrefab;

	internal List<WorldSpaceUIValueLost> valueLostList = new List<WorldSpaceUIValueLost>();

	private float hideTimer;

	internal float hideAlpha = 1f;

	private void Awake()
	{
		instance = this;
		canvasGroup = GetComponent<CanvasGroup>();
	}

	private void Update()
	{
		float num = 1f;
		if (hideTimer > 0f)
		{
			num = 0f;
			hideTimer -= Time.deltaTime;
		}
		hideAlpha = Mathf.Lerp(hideAlpha, num, Time.deltaTime * 20f);
		canvasGroup.alpha = hideAlpha;
	}

	public void ValueLostCreate(Vector3 _worldPosition, int _value)
	{
		if (PlayerController.instance.isActiveAndEnabled && Vector3.Distance(_worldPosition, PlayerController.instance.transform.position) > 10f)
		{
			return;
		}
		foreach (WorldSpaceUIValueLost valueLost in valueLostList)
		{
			if (Vector3.Distance(valueLost.worldPosition, _worldPosition) < 1f && valueLost.timer > 0f)
			{
				valueLost.timer = 0f;
				_value += valueLost.value;
			}
		}
		WorldSpaceUIValueLost component = Object.Instantiate(valueLostPrefab, base.transform.position, base.transform.rotation, base.transform).GetComponent<WorldSpaceUIValueLost>();
		component.worldPosition = _worldPosition;
		component.value = _value;
		valueLostList.Add(component);
	}

	public void Hide()
	{
		hideTimer = 0.1f;
	}

	public void TTS(PlayerAvatar _player, string _text, float _time)
	{
		if (GameDirector.instance.currentState == GameDirector.gameState.Main && (!_player.isDisabled || _player.playerDeathHead.spectated) && !_player.isLocal)
		{
			WorldSpaceUITTS component = Object.Instantiate(TTSPrefab, base.transform.position, base.transform.rotation, base.transform).GetComponent<WorldSpaceUITTS>();
			component.text.text = _text;
			component.playerAvatar = _player;
			Transform tTSTransform = _player.playerAvatarVisuals.TTSTransform;
			if (_player.isDisabled)
			{
				tTSTransform = _player.voiceChat.transform;
				component.offsetPosition = Vector3.down * 0.3f;
			}
			component.followTransform = tTSTransform;
			component.worldPosition = component.followTransform.position + component.offsetPosition;
			component.followPosition = component.worldPosition;
			component.wordTime = _time;
			component.ttsVoice = _player.voiceChat.ttsVoice;
		}
	}

	public void PlayerName(PlayerAvatar _player)
	{
		if (!_player.isLocal && !SemiFunc.MenuLevel())
		{
			WorldSpaceUIPlayerName component = Object.Instantiate(playerNamePrefab, base.transform.position, base.transform.rotation, base.transform).GetComponent<WorldSpaceUIPlayerName>();
			component.playerAvatar = _player;
			component.text.text = _player.playerName;
			_player.worldSpaceUIPlayerName = component;
		}
	}
}
