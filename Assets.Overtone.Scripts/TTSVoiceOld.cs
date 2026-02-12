using LeastSquares.Overtone;
using UnityEngine;

namespace Assets.Overtone.Scripts;

public class TTSVoiceOld : MonoBehaviour
{
	public string voiceName;

	public int speakerId;

	private string oldVoiceName;

	private int oldSpeakerId;

	public TTSVoiceNative VoiceModel { get; private set; }

	private void Update()
	{
		if (voiceName != oldVoiceName)
		{
			oldVoiceName = voiceName;
			VoiceModel = TTSVoiceNative.LoadVoiceFromResources(voiceName);
		}
		if (speakerId != oldSpeakerId)
		{
			oldSpeakerId = speakerId;
			VoiceModel.SetSpeakerId(speakerId);
		}
	}

	private void OnDestroy()
	{
		VoiceModel?.Dispose();
	}
}
