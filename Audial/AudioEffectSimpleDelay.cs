using Audial.Utils;
using UnityEngine;

namespace Audial;

public class AudioEffectSimpleDelay : MonoBehaviour
{
	private float sampleRate;

	private CombFilter combFilter;

	[SerializeField]
	private int _delayLengthMS = 120;

	private int DelayLengthMSPrev = 10;

	[SerializeField]
	private float _dryWet = 0.5f;

	[SerializeField]
	private float _decayLength = 0.25f;

	private float delayLength;

	private int delaySamples;

	private float output;

	public int DelayLengthMS
	{
		get
		{
			return _delayLengthMS;
		}
		set
		{
			_delayLengthMS = Mathf.Clamp(value, 10, 3000);
			ChangeDelay();
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

	public float DecayLength
	{
		get
		{
			return _decayLength;
		}
		set
		{
			_decayLength = Mathf.Clamp(value, 0.1f, 1f);
		}
	}

	private void Awake()
	{
		sampleRate = (Settings.SampleRate = AudioSettings.outputSampleRate);
		combFilter = new CombFilter(DelayLengthMS, 0.5f);
		ChangeDelay();
	}

	private void ChangeDelay()
	{
		combFilter.DelayLength = DelayLengthMS;
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		combFilter.gain = DecayLength;
		for (int i = 0; i < data.Length; i += channels)
		{
			for (int j = 0; j < channels; j++)
			{
				float num = data[i + j];
				float num2 = combFilter.ProcessSample(j, num);
				output = num * (1f - DryWet / 2f) + num2 * DryWet / 2f;
				data[i + j] = output;
			}
			combFilter.MoveIndex();
		}
	}
}
