using UnityEngine;

namespace Audial;

public class AudioEffectFoldbackDistortion : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 3f)]
	private float _inputGain = 1.14f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _softDistortAmount = 0.177f;

	private float softThreshold = 0.002f;

	[SerializeField]
	[Range(1E-06f, 1f)]
	private float _threshold = 0.244f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _distortAmount = 0.904f;

	[SerializeField]
	[Range(0f, 5f)]
	private float _outputGain = 1f;

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

	public float SoftDistortAmount
	{
		get
		{
			return _softDistortAmount;
		}
		set
		{
			_softDistortAmount = Mathf.Clamp(value, 0f, 1f);
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
			_threshold = Mathf.Clamp(value, 1E-06f, 1f);
		}
	}

	public float DistortAmount
	{
		get
		{
			return _distortAmount;
		}
		set
		{
			_distortAmount = Mathf.Clamp(value, 0f, 1f);
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

	private float foldBack(float sample, float threshold)
	{
		if (Mathf.Abs(sample) > Threshold)
		{
			return Mathf.Abs(Mathf.Abs(sample - Threshold % (Threshold * 4f)) - Threshold * 2f) - Threshold + 0.3f * sample;
		}
		return sample;
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		for (int i = 0; i < data.Length; i += channels)
		{
			for (int j = 0; j < channels; j++)
			{
				data[i + j] *= InputGain;
				float num = foldBack(data[i + j], softThreshold);
				data[i + j] = (1f - SoftDistortAmount) * data[i + j] + SoftDistortAmount * num;
				data[i + j] *= OutputGain;
				float num2 = foldBack(data[i + j], Threshold);
				data[i + j] = (1f - DistortAmount) * data[i + j] + DistortAmount * num2;
				data[i + j] *= OutputGain;
			}
		}
	}
}
