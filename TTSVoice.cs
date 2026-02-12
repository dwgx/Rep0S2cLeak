using System.Collections.Generic;
using Strobotnik.Klattersynth;
using UnityEngine;
using UnityEngine.UI;

public class TTSVoice : MonoBehaviour
{
	internal PlayerAvatar playerAvatar;

	internal bool isPlayerAvatarDisabledPrev;

	public Text baseFreqHzLabel;

	public Speech[] voices;

	internal string voiceText;

	internal int voiceTextWordIndex;

	internal bool isSpeaking;

	internal AudioSource audioSource;

	private Speech activeVoice;

	private int activeVoiceNum;

	private int voiceBaseFrequency;

	private SpeechSynth.VoicingSource voicingSource;

	private List<string> words;

	internal float currentWordTime;

	private float tumbleCooldown;

	private float noClipLoudnessTimer;

	private void Init()
	{
		if (voices.Length != 0)
		{
			setVoice(0);
			return;
		}
		Debug.LogError("Empty voices array!", this);
		base.gameObject.SetActive(value: false);
	}

	public void TTSSpeakNow(string text, bool crouch)
	{
		StartSpeakingWithHighlight(text, crouch);
	}

	public void StopAndClearVoice()
	{
		voices[0].stop(allScheduled: true);
		voices[0].cacheClear();
		voices[0].scheduleClear();
		voices[1].stop(allScheduled: true);
		voices[1].cacheClear();
		voices[1].scheduleClear();
	}

	public void StartSpeakingWithHighlight(string text, bool crouch)
	{
		StopAndClearVoice();
		if (crouch)
		{
			voicingSource = SpeechSynth.VoicingSource.whisper;
			setVoice(1);
		}
		else
		{
			voicingSource = SpeechSynth.VoicingSource.natural;
			setVoice(0);
		}
		if (!activeVoice)
		{
			Debug.LogError("Active voice is not set.");
			return;
		}
		text = TranslateSpecialLetters(text);
		words = new List<string>(text.Split(' '));
		foreach (string word in words)
		{
			activeVoice.schedule(word);
		}
	}

	public void setVoice(int num)
	{
		if (num >= 0 && num < voices.Length)
		{
			activeVoice = voices[num];
			activeVoiceNum = num;
		}
		else
		{
			Debug.LogWarning("Invalid voice: " + num, this);
		}
	}

	public void VoiceText(string text, float wordTime)
	{
		if ((bool)playerAvatar && (bool)WorldSpaceUIParent.instance)
		{
			WorldSpaceUIParent.instance.TTS(playerAvatar, text, wordTime);
		}
		voiceText = text;
		currentWordTime = wordTime;
	}

	private void Start()
	{
		Init();
		audioSource = GetComponent<AudioSource>();
	}

	private string TranslateSpecialLetters(string _text)
	{
		if (_text.Contains("ö") || _text.Contains("Ö"))
		{
			_text.Replace("ö", "oe");
			_text.Replace("Ö", "OE");
		}
		if (_text.Contains("ä") || _text.Contains("Ä"))
		{
			_text.Replace("ä", "ae");
			_text.Replace("Ä", "AE");
		}
		if (_text.Contains("å") || _text.Contains("Å"))
		{
			_text.Replace("å", "oa");
			_text.Replace("Å", "OA");
		}
		if (_text.Contains("ü") || _text.Contains("Ü"))
		{
			_text.Replace("ü", "ue");
			_text.Replace("Ü", "UE");
		}
		if (_text.Contains("ß"))
		{
			_text.Replace("ß", "ss");
		}
		if (_text.Contains("æ") || _text.Contains("Æ"))
		{
			_text.Replace("æ", "ae");
			_text.Replace("Æ", "AE");
		}
		if (_text.Contains("ø") || _text.Contains("Ø"))
		{
			_text.Replace("ø", "oe");
			_text.Replace("Ø", "OE");
		}
		return _text;
	}

	private void Update()
	{
		if ((bool)playerAvatar && isPlayerAvatarDisabledPrev != playerAvatar.isDisabled)
		{
			StopAndClearVoice();
			isPlayerAvatarDisabledPrev = playerAvatar.isDisabled;
		}
		isSpeaking = audioSource.isPlaying;
		if (isSpeaking && playerAvatar.voiceChat.clipLoudnessTTS <= 0.01f)
		{
			if (noClipLoudnessTimer > 2f)
			{
				StopAndClearVoice();
			}
			noClipLoudnessTimer += Time.deltaTime;
		}
		else
		{
			noClipLoudnessTimer = 0f;
		}
	}
}
