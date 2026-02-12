using System;
using System.Collections;
using UnityEngine;

namespace Audial.Utils;

public class LFO
{
	private int tableLength = 128;

	public static float[] waveTable;

	private float _index;

	private RunState runState;

	private float _stepSize = 0.3f;

	public float Index
	{
		get
		{
			return _index;
		}
		set
		{
			_index = value;
			if (_index >= (float)tableLength - 0.5f)
			{
				_index -= tableLength;
			}
		}
	}

	public float StepSize
	{
		get
		{
			return _stepSize;
		}
		set
		{
			_stepSize = value;
		}
	}

	public IEnumerator Run()
	{
		runState = RunState.Running;
		while (runState != RunState.Stopped)
		{
			if (runState == RunState.Running)
			{
				Index += (float)tableLength / StepSize * Time.deltaTime;
			}
			yield return new WaitForSeconds(0.002f);
		}
	}

	public void Pause()
	{
		runState = RunState.Paused;
	}

	public void Resume()
	{
		runState = RunState.Running;
	}

	public void Stop()
	{
		runState = RunState.Stopped;
	}

	public void SetRate(float rate)
	{
		StepSize = rate;
	}

	public int GetIndex()
	{
		return Mathf.RoundToInt(Index) % waveTable.Length;
	}

	public float GetValue()
	{
		return waveTable[GetIndex()];
	}

	public void MoveIndex()
	{
		Index += (float)tableLength / StepSize / Settings.SampleRate;
	}

	public float[] GetChunkValue(int chunkLength)
	{
		float[] array = new float[chunkLength];
		for (int i = 0; i < chunkLength; i++)
		{
			array[i] = GetValue();
		}
		return array;
	}

	public LFO()
	{
		if (waveTable == null)
		{
			CreateWavetable();
		}
	}

	public LFO(float speed)
	{
		if (waveTable == null)
		{
			CreateWavetable();
		}
		StepSize = speed;
	}

	private void CreateWavetable()
	{
		waveTable = new float[tableLength];
		for (int i = 0; i < tableLength; i++)
		{
			waveTable[i] = 0.5f + Mathf.Sin(MathF.PI * 2f * (float)i / (float)tableLength) / 2f;
		}
	}
}
