using UnityEngine;

namespace Audial;

public class AudioEffectSaturator : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 3f)]
	private float _inputGain = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _threshold = 0.247f;

	[SerializeField]
	[Range(0f, 1f)]
	public float _amount = 0.5f;

	private float input;

	private float sampleAbs;

	private float sampleSign;

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

	public float Amount
	{
		get
		{
			return _amount;
		}
		set
		{
			_amount = Mathf.Clamp(value, 0f, 1f);
		}
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (Amount == 0f)
		{
			return;
		}
		for (int i = 0; i < channels; i++)
		{
			for (int j = 0; j < data.Length; j += channels)
			{
				input = data[j + i] * InputGain;
				sampleAbs = Mathf.Abs(input);
				sampleSign = Mathf.Sign(input);
				if (sampleAbs > 1f)
				{
					input = (Threshold + 1f) / 2f * sampleSign;
				}
				else if (sampleAbs > Threshold)
				{
					input = (Threshold + (sampleAbs - Threshold) / (1f + Mathf.Pow((sampleAbs - Threshold) / (1f - Amount), 2f))) * sampleSign;
				}
				data[j + i] = input;
			}
		}
	}
}
