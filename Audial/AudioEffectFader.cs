using Audial.Utils;
using UnityEngine;

namespace Audial;

public class AudioEffectFader : MonoBehaviour
{
	[SerializeField]
	private float _gain = 1f;

	public bool Mute;

	private LFO lfo = new LFO();

	public float Gain
	{
		get
		{
			return _gain;
		}
		set
		{
			_gain = Mathf.Clamp(value, 0f, 3f);
		}
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (Mute)
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = 0f;
			}
		}
		else
		{
			for (int j = 0; j < data.Length; j++)
			{
				data[j] *= Gain;
			}
		}
	}
}
