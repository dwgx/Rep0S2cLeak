using System;
using UnityEngine;
using UnityEngine.Events;

public class TrapGramophone : Trap
{
	public UnityEvent gramophoneTimer;

	[Space]
	[Header("Gramophone Components")]
	public GameObject Gramophone;

	public GameObject GramophoneRecord;

	public GameObject GramophoneCrank;

	[Space]
	[Header("Sounds")]
	public Sound GramophoneStart;

	public Sound GramophoneEnd;

	public Sound GramophoneMusic;

	[Space]
	[Header("Gramophone Animation")]
	public AnimationCurve GramophoneStartCurve;

	public float GramophoneStartIntensity;

	public float GramophoneStartDuration;

	[Space]
	public AnimationCurve GramophoneEndCurve;

	public float GramophoneEndIntensity;

	public float GramophoneEndDuration;

	private bool StartSequence;

	private bool endSequence;

	private float StartSequenceProgress;

	private float EndSequenceProgress;

	private bool MusicPlaying;

	private bool MusicStartPointFetched;

	private bool MusicStart;

	private SyncedEventRandom randomRange;

	private Quaternion initialGramophoneRotation;

	protected override void Start()
	{
		base.Start();
		initialGramophoneRotation = Gramophone.transform.localRotation;
		randomRange = GetComponent<SyncedEventRandom>();
	}

	protected override void Update()
	{
		base.Update();
		GramophoneMusic.PlayLoop(MusicPlaying, 0.5f, 0.5f);
		if (trapStart)
		{
			TrapActivate();
		}
		if (MusicStart && !MusicStartPointFetched)
		{
			randomRange.RandomRangeFloat(0f, GramophoneMusic.Source.clip.length);
			if (randomRange.resultReceivedRandomRangeFloat)
			{
				MusicStartPointFetched = true;
				GramophoneMusic.StartTimeOverride = randomRange.resultRandomRangeFloat;
				MusicPlaying = true;
			}
		}
		if (!trapActive)
		{
			return;
		}
		enemyInvestigate = true;
		GramophoneRecord.transform.Rotate(0f, 20f * Time.deltaTime, 0f);
		GramophoneCrank.transform.Rotate(0f, 0f, 20f * Time.deltaTime);
		float num = 1f;
		if (StartSequenceProgress == 0f && !StartSequence)
		{
			StartSequence = true;
			GramophoneStart.Play(physGrabObject.centerPoint);
			GramophoneMusic.LoopPitch = 0.2f;
		}
		if (StartSequence)
		{
			num += GramophoneStartCurve.Evaluate(StartSequenceProgress) * GramophoneStartIntensity;
			GramophoneMusic.LoopPitch = 1f - GramophoneStartCurve.Evaluate(StartSequenceProgress);
			StartSequenceProgress += Time.deltaTime / GramophoneStartDuration;
			if (StartSequenceProgress >= 1f)
			{
				StartSequence = false;
			}
		}
		if (endSequence)
		{
			num += GramophoneEndCurve.Evaluate(EndSequenceProgress) * GramophoneEndIntensity;
			EndSequenceProgress += Time.deltaTime / GramophoneEndDuration;
			GramophoneMusic.LoopPitch -= 0.5f * Time.deltaTime;
			if (EndSequenceProgress >= 1f)
			{
				EndSequenceDone();
			}
		}
		float num2 = 1f * num;
		float num3 = 40f;
		float num4 = num2 * Mathf.Sin(Time.time * num3);
		float z = num2 * Mathf.Sin(Time.time * num3 + MathF.PI / 2f);
		Gramophone.transform.localRotation = initialGramophoneRotation * Quaternion.Euler(num4, 0f, z);
		Gramophone.transform.localPosition = new Vector3(Gramophone.transform.localPosition.x, Gramophone.transform.localPosition.y - num4 * 0.005f * Time.deltaTime, Gramophone.transform.localPosition.z);
	}

	private void EndSequenceDone()
	{
		trapActive = false;
		endSequence = false;
		MusicPlaying = false;
	}

	public void TrapStop()
	{
		endSequence = true;
		GramophoneEnd.Play(physGrabObject.centerPoint);
	}

	public void TrapActivate()
	{
		if (!trapTriggered)
		{
			MusicStart = true;
			gramophoneTimer.Invoke();
			trapActive = true;
			trapTriggered = true;
		}
	}
}
