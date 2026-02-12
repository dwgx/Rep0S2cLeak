using System;
using UnityEngine;

namespace Audial;

public class AudioEffectStateVariableFilter : MonoBehaviour
{
	private float sampleFrequency;

	private int passes = 2;

	[SerializeField]
	[Range(50f, 12000f)]
	private float _frequency = 440f;

	private double freq;

	[SerializeField]
	[Range(0f, 1f)]
	private float _resonance = 0.5f;

	[SerializeField]
	[Range(0f, 0.1f)]
	private float _drive = 0.1f;

	public FilterState Filter = FilterState.BandPass;

	[SerializeField]
	[Range(-1f, 1f)]
	private float _additiveGain = 0.25f;

	private double[] notch = new double[2];

	private double[] low = new double[2];

	private double[] high = new double[2];

	private double[] band = new double[2];

	private double[] output = new double[2];

	private double damp;

	public float Frequency
	{
		get
		{
			return _frequency;
		}
		set
		{
			_frequency = Mathf.Clamp(value, 50f, 12000f);
			UpdateFrequency();
		}
	}

	public float Resonance
	{
		get
		{
			return _resonance;
		}
		set
		{
			_resonance = Mathf.Clamp(value, 0f, 1f);
			UpdateDamp();
		}
	}

	public float Drive
	{
		get
		{
			return _drive;
		}
		set
		{
			_drive = Mathf.Clamp(value, 0f, 0.1f);
		}
	}

	public float AdditiveGain
	{
		get
		{
			return _additiveGain;
		}
		set
		{
			_additiveGain = Mathf.Clamp(value, -1f, 1f);
		}
	}

	private void Awake()
	{
		sampleFrequency = AudioSettings.outputSampleRate;
		UpdateFrequency();
		UpdateDamp();
	}

	private void UpdateFrequency()
	{
		freq = 2.0 * Math.Sin(Math.PI * (double)Frequency / (double)(sampleFrequency * (float)passes));
		UpdateDamp();
	}

	private void UpdateDamp()
	{
		damp = Math.Min(2.0 * (1.0 - Math.Pow(Resonance, 0.25)), Math.Min(2.0 - freq, 2.0 / freq - freq * 0.5));
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (Filter == FilterState.Bypass)
		{
			return;
		}
		double[] array = new double[channels];
		for (int i = 0; i < data.Length; i += channels)
		{
			for (int j = 0; j < channels; j++)
			{
				array[j] = (((double)Math.Abs(data[i + j]) > 1E-07) ? data[i + j] : 0f);
				output[j] = 0.0;
				for (int k = 0; k < passes; k++)
				{
					high[j] = array[j] - low[j] - damp * band[j];
					band[j] = freq * high[j] + band[j] - (double)(0.1f - Drive + 0.001f) * Math.Pow(band[j], 3.0);
					low[j] = freq * band[j] + low[j];
				}
				switch (Filter)
				{
				case FilterState.LowPass:
				case FilterState.LowShelf:
					output[j] = low[j];
					break;
				case FilterState.HighPass:
				case FilterState.HighShelf:
					output[j] = high[j];
					break;
				case FilterState.BandPass:
				case FilterState.BandAdd:
					output[j] = band[j];
					break;
				}
				if (Filter == FilterState.HighShelf || Filter == FilterState.LowShelf || Filter == FilterState.BandAdd)
				{
					data[i + j] += (float)output[j] * AdditiveGain;
				}
				else
				{
					data[i + j] = (float)output[j];
				}
			}
		}
	}
}
