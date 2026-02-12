using System;
using Audial.Utils;
using UnityEngine;

namespace Audial;

[Serializable]
public class AudioEffectFlanger : MonoBehaviour
{
	private float sampleRate;

	private float output;

	private LFO lfo;

	[SerializeField]
	private CombFilter combFilter;

	[SerializeField]
	private float _rate = 0.3f;

	[SerializeField]
	private float _intensity = 0.25f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _dryWet = 0.75f;

	private float _offset;

	public float Rate
	{
		get
		{
			return _rate;
		}
		set
		{
			if (_rate != value)
			{
				_rate = Mathf.Clamp(value, 0.1f, 8f);
				lfo.SetRate(_rate);
			}
		}
	}

	public float Intensity
	{
		get
		{
			return _intensity;
		}
		set
		{
			_intensity = Mathf.Clamp(value, 0.1f, 0.9f);
			combFilter.gain = _intensity;
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

	private int Offset
	{
		get
		{
			return (int)_offset;
		}
		set
		{
			_offset = value;
		}
	}

	private void Awake()
	{
		sampleRate = (Settings.SampleRate = AudioSettings.outputSampleRate);
		ResetUtils();
	}

	private void ResetUtils()
	{
		combFilter = new CombFilter(Intensity, 0.5f);
		lfo = new LFO(Rate);
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		for (int i = 0; i < data.Length; i += channels)
		{
			combFilter.Offset = (int)Mathf.Lerp(1f * sampleRate / 1000f, 5f * sampleRate / 1000f, lfo.GetValue());
			for (int j = 0; j < channels; j++)
			{
				float num = data[i + j];
				float num2 = combFilter.ProcessSample(j, num);
				output = num * (1f - DryWet / 2f) + num2 * DryWet / 2f;
				data[i + j] = output;
			}
			combFilter.MoveIndex();
			lfo.MoveIndex();
		}
	}
}
