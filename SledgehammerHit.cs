using UnityEngine;

public class SledgehammerHit : MonoBehaviour
{
	public SledgehammerController Controller;

	public Transform LookAtTransform;

	public ToolActiveOffset Intro;

	public Transform MeshTransform;

	[Space]
	public Transform Outro;

	public AnimationCurve OutroCurve;

	private Vector3 OutroPositionStart;

	private Quaternion OutroRotationStart;

	[Space]
	public bool DebugDelayDisable;

	public float DelayTime;

	private float DelayTimer;

	private bool Spawning;

	private RoachTrigger Roach;

	public void Spawn(RoachTrigger roach)
	{
		Intro.Active = true;
		Intro.ActiveLerp = 1f;
		Roach = roach;
		base.transform.position = Roach.transform.position;
		base.transform.LookAt(LookAtTransform);
		DelayTimer = DelayTime;
		MeshTransform.gameObject.SetActive(value: false);
		Spawning = true;
	}

	public void Hit()
	{
		MeshTransform.gameObject.SetActive(value: true);
		Controller.SoundHit.Play(base.transform.position);
		Spawning = false;
	}

	public void Update()
	{
		if (Spawning)
		{
			base.transform.position = Roach.RoachOrbit.transform.position;
			base.transform.LookAt(LookAtTransform);
		}
		else if (!DebugDelayDisable)
		{
			DelayTimer -= Time.deltaTime;
			if (DelayTimer <= 0f)
			{
				Controller.HitDone();
			}
		}
	}
}
