using UnityEngine;

namespace Audial;

public class AudioTester : MonoBehaviour
{
	[HideInInspector]
	public bool hasAudioSource = true;

	[HideInInspector]
	public AudioSource audioSource;

	public bool playAudio
	{
		set
		{
			base.gameObject.SendMessage("ClearBuffer");
			base.gameObject.SendMessage("ResetUtils", SendMessageOptions.DontRequireReceiver);
			if (hasAudioSource && audioSource.clip != null)
			{
				audioSource.Play();
			}
		}
	}

	public bool stopAudio
	{
		set
		{
			base.gameObject.SendMessage("ClearBuffer");
			if (hasAudioSource)
			{
				audioSource.Stop();
			}
		}
	}

	public void ClearBuffer()
	{
	}

	public void SetRunEffectInEditMode()
	{
	}
}
