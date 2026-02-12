using UnityEngine;
using UnityEngine.Events;

public class GrandfatherClockTrap : Trap
{
	public UnityEvent bellRingTimer;

	[Header("Trap Activated Animation")]
	[Header("Pendulum")]
	public Transform pendulum;

	private float angle = 6.25f;

	private bool angleLerpRev;

	private float angleLerp;

	public AnimationCurve angleCurve;

	private float pendulumSpeedMin;

	public float pendulumSpeedMax = 5f;

	private float pendulumSpeed;

	public AnimationCurve pendulumSpeedCurve;

	private float pendulumSpeedLerp;

	private float offset = 0.49f;

	private bool ticPlayed;

	private bool tocPlayed;

	[Header("Sounds")]
	public Sound Tic;

	private float ticVolume;

	public Sound Toc;

	private float tocVolume;

	public Sound Bell;

	private float bellVolume;

	private int bellRingCount;

	private float masterVolume = 1f;

	private Rigidbody rb;

	protected override void Start()
	{
		base.Start();
		ticVolume = Tic.Volume;
		tocVolume = Toc.Volume;
		bellVolume = Bell.Volume;
		rb = GetComponent<Rigidbody>();
	}

	protected override void Update()
	{
		base.Update();
		if (trapStart)
		{
			GrandfatherClockActivate();
		}
		Tic.Volume = ticVolume * masterVolume;
		Toc.Volume = tocVolume * masterVolume;
		Bell.Volume = bellVolume * masterVolume;
		if (!angleLerpRev)
		{
			angleLerp += Time.deltaTime + pendulumSpeed * Time.deltaTime;
			if (angleLerp > 1f)
			{
				angleLerpRev = true;
			}
		}
		else
		{
			angleLerp -= Time.deltaTime + pendulumSpeed * Time.deltaTime;
			if (angleLerp < 0f)
			{
				angleLerpRev = false;
			}
		}
		float y = Mathf.Lerp(0f - angle, angle, angleCurve.Evaluate(angleLerp));
		pendulum.localEulerAngles = new Vector3(0f, y, 0f);
		if (angleLerp >= 1f - offset && !ticPlayed)
		{
			Tic.Play(pendulum.position);
			ticPlayed = true;
			tocPlayed = false;
		}
		if (angleLerp <= 0f + offset && !tocPlayed)
		{
			Toc.Play(pendulum.position);
			tocPlayed = true;
			ticPlayed = false;
		}
		if (trapTriggered)
		{
			pendulumSpeedLerp += Time.deltaTime / 15f;
			pendulumSpeed = Mathf.Lerp(pendulumSpeedMin, pendulumSpeedMax, pendulumSpeedCurve.Evaluate(pendulumSpeedLerp));
		}
	}

	public void GrandfatherClockBell()
	{
		if (bellRingCount < 3)
		{
			Bell.Play(pendulum.position);
			enemyInvestigate = true;
			enemyInvestigateRange = 60f;
			bellRingCount++;
			bellRingTimer.Invoke();
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 6f, 12f, pendulum.position, 0.2f);
		}
	}

	public void GrandfatherClockActivate()
	{
		if (!trapTriggered)
		{
			Bell.Play(pendulum.position);
			trapTriggered = true;
			bellRingTimer.Invoke();
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 6f, 12f, pendulum.position, 0.2f);
		}
	}
}
