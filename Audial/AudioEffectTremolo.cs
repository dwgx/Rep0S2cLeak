using Audial.Utils;
using UnityEngine;

namespace Audial;

public class AudioEffectTremolo : MonoBehaviour
{
	private float output;

	private float sampleRate;

	public LFO carrierLFO;

	[SerializeField]
	private float _carrierFrequency = 10f;

	[SerializeField]
	private float _dryWet = 0.75f;

	public float CarrierFrequency
	{
		get
		{
			return _carrierFrequency;
		}
		set
		{
			_carrierFrequency = Mathf.Clamp(value, 2f, 20f);
			carrierLFO.SetRate(1f / _carrierFrequency);
		}
	}

	public float DryWet
	{
		get
		{
			return _dryWet;
		}
		set
		{
			_dryWet = Mathf.Clamp(value, 0f, 1f);
		}
	}

	private void Awake()
	{
		sampleRate = (Settings.SampleRate = AudioSettings.outputSampleRate);
		ResetUtils();
	}

	private void ResetUtils()
	{
		carrierLFO = new LFO(1f / CarrierFrequency);
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		for (int i = 0; i < data.Length; i += channels)
		{
			for (int j = 0; j < channels; j++)
			{
				float num = data[i + j];
				float num2 = num * carrierLFO.GetValue();
				data[i + j] = num * (1f - DryWet) + num2 * DryWet;
			}
			carrierLFO.MoveIndex();
		}
	}
}
