using UnityEngine;
using UnityEngine.Events;

public class ToyMonkeyTrap : Trap
{
	public UnityEvent toyMonkeyTimer;

	[Space]
	[Header("Components")]
	public Transform head;

	public Transform leftArm;

	public Transform rightArm;

	[Space]
	[Header("Sounds")]
	public Sound cymbal;

	public Sound mechanicalLoop;

	[Space]
	[Header("Animation")]
	public AnimationCurve armAnimationCurve;

	public AnimationCurve headAnimationCurve;

	private float armRotationLerp = 0.5f;

	private float headRotationXLerp;

	private float headRotationZLerp = 0.25f;

	private float headRotationSpeed = 4f;

	private float spinLerp;

	[Space]
	private Rigidbody rb;

	private bool trapPlaying;

	protected override void Start()
	{
		base.Start();
		rb = GetComponent<Rigidbody>();
	}

	protected override void Update()
	{
		base.Update();
		if (trapStart)
		{
			ToyMonkeyTrapActivated();
		}
		mechanicalLoop.PlayLoop(trapPlaying, 0.8f, 0.8f);
		if (trapActive)
		{
			enemyInvestigate = true;
			trapPlaying = true;
			if (armRotationLerp < 1f)
			{
				armRotationLerp += Time.deltaTime * 3f;
			}
			if (armRotationLerp >= 1f)
			{
				armRotationLerp = 0f;
				Vector3 vector = Vector3.Slerp(Vector3.up, base.transform.right, 0.25f);
				cymbal.Play(physGrabObject.centerPoint);
				_ = Random.insideUnitSphere.normalized;
				rb.AddForce(vector * 1.3f, ForceMode.Impulse);
				rb.AddTorque(base.transform.up * Random.Range(-0.25f, 0.25f), ForceMode.Impulse);
			}
			float num = Mathf.Lerp(-15f, 40f, armAnimationCurve.Evaluate(armRotationLerp));
			leftArm.localEulerAngles = new Vector3(0f, num, 0f);
			rightArm.localEulerAngles = new Vector3(0f, 0f - num, 0f);
			if (headRotationXLerp < 1f)
			{
				headRotationXLerp += Time.deltaTime * headRotationSpeed;
			}
			if (headRotationXLerp >= 1f)
			{
				headRotationXLerp = 0f;
			}
			float x = Mathf.Lerp(-15f, 15f, headAnimationCurve.Evaluate(headRotationXLerp));
			if (headRotationZLerp < 1f)
			{
				headRotationZLerp += Time.deltaTime * headRotationSpeed;
			}
			if (headRotationZLerp >= 1f)
			{
				headRotationZLerp = 0f;
			}
			float z = Mathf.Lerp(-15f, 15f, headAnimationCurve.Evaluate(headRotationZLerp));
			head.localEulerAngles = new Vector3(x, 0f, z);
		}
	}

	private void FixedUpdate()
	{
		if (!trapActive || !isLocal)
		{
			return;
		}
		Vector3 normalized = Random.insideUnitSphere.normalized;
		if (physGrabObject.playerGrabbing.Count == 0)
		{
			if (spinLerp < 1f)
			{
				spinLerp += Time.deltaTime;
			}
			float num = Mathf.Lerp(0f, 0.5f, spinLerp);
			rb.AddTorque(base.transform.right * num + normalized * 0.05f, ForceMode.Force);
		}
		else
		{
			spinLerp = 0f;
			rb.AddTorque(normalized * 0.5f, ForceMode.Force);
		}
	}

	public void ToyMonkeyTrapStop()
	{
		trapActive = false;
		trapPlaying = false;
	}

	public void ToyMonkeyTrapActivated()
	{
		if (!trapTriggered)
		{
			toyMonkeyTimer.Invoke();
			trapTriggered = true;
			trapActive = true;
		}
	}
}
