using Audial.Utils;
using UnityEngine;

namespace Audial;

public class AudioEffectPhaser : MonoBehaviour
{
	private float output;

	private float sampleRate;

	public LFO lfo;

	public AllPassFilter[] allPassFilters = new AllPassFilter[4];

	[SerializeField]
	private float _rate = 0.3f;

	[SerializeField]
	private float _width = 0.5f;

	[SerializeField]
	private float _intensity = 0.25f;

	[SerializeField]
	private float _dryWet = 0.75f;

	private float _offset;

	private float fromMin = 0.43f;

	private float fromMax = 0.193f;

	private float toMin = 0.772f;

	private float toMax = 0.962f;

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

	public float Width
	{
		get
		{
			return _width;
		}
		set
		{
			if (_width != value)
			{
				_width = Mathf.Clamp(value, 0f, 1f);
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
			_intensity = Mathf.Clamp(value, 0f, 1f);
			SetIntensity(_intensity);
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
		allPassFilters[0] = new AllPassFilter(Rate, Intensity);
		allPassFilters[1] = new AllPassFilter(Rate, Intensity);
		allPassFilters[2] = new AllPassFilter(Rate, Intensity);
		allPassFilters[3] = new AllPassFilter(Rate, Intensity);
		SetIntensity(Intensity);
		lfo = new LFO(Rate);
	}

	private void SetIntensity(float i)
	{
		for (int j = 0; j < allPassFilters.Length; j++)
		{
			allPassFilters[j].gain = i * 0.6f;
		}
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		for (int i = 0; i < data.Length; i += channels)
		{
			float num = Mathf.Lerp(Mathf.Lerp(fromMin, fromMax, Width), Mathf.Lerp(toMin, toMax, Width), lfo.GetValue()) * sampleRate / 1000f;
			for (int j = 0; j < allPassFilters.Length; j++)
			{
				allPassFilters[j].Offset = (int)num;
				for (int k = 0; k < channels; k++)
				{
					float num2 = data[i + k];
					float num3 = allPassFilters[j].ProcessSample(k, num2);
					output = num2 * (1f - DryWet / 2f) + num3 * DryWet / 2f;
					data[i + k] = output;
				}
				allPassFilters[j].MoveIndex();
			}
			lfo.MoveIndex();
		}
	}
}
