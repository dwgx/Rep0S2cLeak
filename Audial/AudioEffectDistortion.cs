using UnityEngine;

namespace Audial;

public class AudioEffectDistortion : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 3f)]
	private float _inputGain = 1f;

	[SerializeField]
	[Range(1E-05f, 1f)]
	private float _threshold = 0.036f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _dryWet = 0.258f;

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

	public float Threshold
	{
		get
		{
			return _threshold;
		}
		set
		{
			_threshold = Mathf.Clamp(value, 1E-05f, 1f);
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

	private void OnAudioFilterRead(float[] data, int channels)
	{
		for (int i = 0; i < data.Length; i += channels)
		{
			for (int j = 0; j < channels; j++)
			{
				data[i + j] *= InputGain;
				float num = data[i + j];
				if (Mathf.Abs(num) > Threshold)
				{
					num = Mathf.Sign(num);
				}
				data[i + j] = (1f - DryWet) * data[i + j] + DryWet * num;
				data[i + j] *= OutputGain;
			}
		}
	}
}
