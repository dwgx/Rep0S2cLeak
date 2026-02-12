using Audial.Utils;
using UnityEngine;

namespace Audial;

public class AudioEffectCompressor : MonoBehaviour
{
	[SerializeField]
	public Envelope envelope;

	[SerializeField]
	private float _inputGain = 1f;

	[SerializeField]
	private float _threshold = 0.247f;

	[SerializeField]
	public float _slope = 1.727f;

	[SerializeField]
	private float _attack = 0.0001f;

	[SerializeField]
	public float _release = 0.68f;

	[SerializeField]
	private float _dryGain;

	[SerializeField]
	private float _compressedGain = 1f;

	[SerializeField]
	private float _outputGain = 1f;

	private float env;

	private float mergedData;

	private float[] input;

	private float rms;

	private float[] compressed;

	private float[] dry;

	private float compressMod;

	public float InputGain
	{
		get
		{
			return _inputGain;
		}
		set
		{
			_inputGain = Mathf.Clamp(value, 0f, 3f);
		}
	}

	public float Threshold
	{
		get
		{
			return _threshold;
		}
		set
		{
			_threshold = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public float Slope
	{
		get
		{
			return _slope;
		}
		set
		{
			_slope = Mathf.Clamp(value, 0f, 2f);
		}
	}

	public float Attack
	{
		get
		{
			return _attack;
		}
		set
		{
			_attack = Mathf.Clamp(value, 0f, 1f);
			envelope.Attack = _attack;
		}
	}

	public float Release
	{
		get
		{
			return _release;
		}
		set
		{
			_release = Mathf.Clamp(value, 0f, 1f);
			envelope.Release = _release;
		}
	}

	public float DryGain
	{
		get
		{
			return _dryGain;
		}
		set
		{
			_dryGain = Mathf.Clamp(value, 0f, 5f);
		}
	}

	public float CompressedGain
	{
		get
		{
			return _compressedGain;
		}
		set
		{
			_compressedGain = Mathf.Clamp(value, 0f, 5f);
		}
	}

	public float OutputGain
	{
		get
		{
			return _outputGain;
		}
		set
		{
			_outputGain = Mathf.Clamp(value, 0f, 5f);
		}
	}

	private void Awake()
	{
		Settings.SampleRate = AudioSettings.outputSampleRate;
		envelope = new Envelope(Attack, Release);
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (envelope == null)
		{
			return;
		}
		if (input == null || input.Length != channels)
		{
			input = new float[channels];
			compressed = new float[channels];
			dry = new float[channels];
		}
		for (int i = 0; i < data.Length; i += channels)
		{
			rms = 0f;
			for (int j = 0; j < channels; j++)
			{
				input[j] = data[i + j] * InputGain;
				rms += input[j] * input[j];
			}
			rms = Mathf.Pow(rms, 1f / (float)channels);
			env = envelope.ProcessSample(rms);
			compressMod = 1f;
			if (env > Threshold)
			{
				compressMod = Mathf.Clamp(compressMod - (env - Threshold) * Slope, 0f, 1f);
			}
			mergedData = 0f;
			for (int k = 0; k < channels; k++)
			{
				compressed[k] = input[k] * compressMod;
				mergedData += compressed[k] * compressed[k];
				data[i + k] = (compressed[k] * CompressedGain + input[k] * DryGain) * OutputGain;
			}
			mergedData = Mathf.Pow(mergedData, 1f / (float)channels);
		}
	}
}
