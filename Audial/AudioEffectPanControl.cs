using UnityEngine;

namespace Audial;

public class AudioEffectPanControl : MonoBehaviour
{
	[SerializeField]
	[Range(-1f, 1f)]
	private float _panAmount;

	public float PanAmount
	{
		get
		{
			return _panAmount;
		}
		set
		{
			_panAmount = Mathf.Clamp(value, -1f, 1f);
		}
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		if (channels != 2)
		{
			return;
		}
		for (int i = 0; i < data.Length; i += channels)
		{
			if (Mathf.Sign(PanAmount) > 0f)
			{
				data[i] = (1f - Mathf.Abs(PanAmount)) * data[i];
			}
			else
			{
				data[i + 1] = (1f - Mathf.Abs(PanAmount)) * data[i + 1];
			}
		}
	}
}
