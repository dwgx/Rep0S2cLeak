using System;
using UnityEngine;
using UnityEngine.Events;

public class TrapRadio : Trap
{
	public UnityEvent radioTimer;

	public MeshRenderer RadioDisplay;

	public Light RadioLight;

	public Transform RadioMeter;

	public AnimationCurve RadioFlickerCurve;

	public float RadioFlickerTime = 0.5f;

	private float RadioFlickerTimer;

	private bool RadioFlickerIntro = true;

	private bool RadioFlickerOutro;

	[Space]
	[Header("Gramophone Components")]
	public GameObject Radio;

	[Space]
	[Header("Sounds")]
	public Sound RadioStart;

	public Sound RadioEnd;

	public Sound RadioLoop;

	[Space]
	[Header("Radio Animation")]
	public AnimationCurve RadioStartCurve;

	public float RadioStartIntensity;

	public float RadioStartDuration;

	[Space]
	public AnimationCurve RadioEndCurve;

	public float RadioEndIntensity;

	public float RadioEndDuration;

	private bool StartSequence;

	private bool endSequence;

	private float StartSequenceProgress;

	private float EndSequenceProgress;

	private bool RadioPlaying;

	private Quaternion initialRadioRotation;

	private Quaternion initialRadioMeterRotation;

	protected override void Start()
	{
		base.Start();
		initialRadioRotation = Radio.transform.localRotation;
		initialRadioMeterRotation = RadioMeter.localRotation;
		RadioLight.enabled = false;
		RadioDisplay.enabled = false;
	}

	protected override void Update()
	{
		base.Update();
		if (trapStart)
		{
			RadioTrapActivated();
		}
		RadioLoop.PlayLoop(RadioPlaying, 2f, 2f);
		if (!trapActive)
		{
			return;
		}
		enemyInvestigate = true;
		if (RadioFlickerIntro || RadioFlickerOutro)
		{
			float num = RadioFlickerCurve.Evaluate(RadioFlickerTimer / RadioFlickerTime);
			RadioFlickerTimer += 1f * Time.deltaTime;
			if (num > 0.5f)
			{
				RadioLight.enabled = true;
				RadioDisplay.enabled = true;
			}
			else
			{
				RadioLight.enabled = false;
				RadioDisplay.enabled = false;
			}
			if (RadioFlickerTimer > RadioFlickerTime)
			{
				RadioFlickerIntro = false;
				RadioFlickerTimer = 0f;
				if (RadioFlickerOutro)
				{
					RadioLight.enabled = false;
					RadioDisplay.enabled = false;
				}
				else
				{
					RadioLight.enabled = true;
					RadioDisplay.enabled = true;
				}
			}
		}
		RadioPlaying = true;
		float num2 = 1f;
		if (StartSequenceProgress == 0f && !StartSequence)
		{
			StartSequence = true;
			RadioStart.Play(physGrabObject.centerPoint);
			RadioLight.enabled = true;
			RadioDisplay.enabled = true;
		}
		if (StartSequence)
		{
			num2 += RadioStartCurve.Evaluate(StartSequenceProgress) * RadioStartIntensity;
			StartSequenceProgress += Time.deltaTime / RadioStartDuration;
			if (StartSequenceProgress >= 1f)
			{
				StartSequence = false;
			}
		}
		if (endSequence)
		{
			num2 += RadioEndCurve.Evaluate(EndSequenceProgress) * RadioEndIntensity;
			EndSequenceProgress += Time.deltaTime / RadioEndDuration;
			if (EndSequenceProgress >= 1f)
			{
				EndSequenceDone();
			}
		}
		float num3 = 1f * num2;
		float num4 = 40f;
		float num5 = num3 * Mathf.Sin(Time.time * num4);
		float z = num3 * Mathf.Sin(Time.time * num4 + MathF.PI / 2f);
		Radio.transform.localRotation = initialRadioRotation * Quaternion.Euler(num5, 0f, z);
		num4 = 20f;
		float num6 = num3 * Mathf.Sin(Time.time * num4);
		RadioMeter.localRotation = initialRadioMeterRotation * Quaternion.Euler(0f, num6 * 90f, 0f);
		Radio.transform.localPosition = new Vector3(Radio.transform.localPosition.x, Radio.transform.localPosition.y - num5 * 0.005f * Time.deltaTime, Radio.transform.localPosition.z);
	}

	private void EndSequenceDone()
	{
		endSequence = false;
		RadioLight.enabled = false;
		RadioDisplay.enabled = false;
		RadioPlaying = false;
		trapActive = false;
		Radio.transform.localRotation = initialRadioRotation;
	}

	public void RadioTrapStop()
	{
		RadioEnd.Play(physGrabObject.centerPoint);
		endSequence = true;
	}

	public void RadioTrapActivated()
	{
		if (!trapTriggered)
		{
			radioTimer.Invoke();
			trapTriggered = true;
			trapActive = true;
		}
	}
}
