using System;
using Photon.Pun;
using UnityEngine;

public class FrogTrap : Trap
{
	private PhysGrabObject physgrabobject;

	[Space]
	[Header("Frog Components")]
	public GameObject Frog;

	public GameObject FrogFeet;

	public GameObject FrogCrank;

	[Space]
	[Header("Sounds")]
	public Sound CrankStart;

	public Sound CrankEnd;

	public Sound CrankLoop;

	public Sound Jump;

	[Space]
	public AnimationCurve FrogJumpCurve;

	private float FrogJumpLerp;

	public float FrogJumpSpeed;

	private bool FrogJumpActive;

	public float FrogJumpIntensity;

	private Quaternion initialFrogRotation;

	private Rigidbody rb;

	private bool LoopPlaying;

	private bool everPickedUp;

	private float frogJumpTimer;

	private PhysGrabObjectImpactDetector impactDetector;

	private bool grabbedPrev;

	protected override void Start()
	{
		base.Start();
		impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		initialFrogRotation = Frog.transform.localRotation;
		rb = GetComponent<Rigidbody>();
		physgrabobject = GetComponent<PhysGrabObject>();
	}

	protected override void Update()
	{
		base.Update();
		CrankLoop.PlayLoop(LoopPlaying, 0.8f, 0.8f);
		if (physGrabObject.grabbed)
		{
			if (!grabbedPrev)
			{
				Jump.Play(physgrabobject.centerPoint);
				if (SemiFunc.IsMasterClientOrSingleplayer())
				{
					Quaternion turnX = Quaternion.Euler(45f, 180f, 0f);
					Quaternion turnY = Quaternion.Euler(0f, 0f, 0f);
					Quaternion identity = Quaternion.identity;
					physGrabObject.TurnXYZ(turnX, turnY, identity);
				}
				grabbedPrev = true;
				if (physGrabObject.grabbedLocal)
				{
					PhysGrabber.instance.OverrideGrabDistance(0.8f);
				}
			}
			everPickedUp = true;
			LoopPlaying = false;
			if (trapActive)
			{
				TrapStop();
			}
		}
		else
		{
			grabbedPrev = false;
			if (everPickedUp)
			{
				trapStart = true;
			}
		}
		if (trapStart && !impactDetector.inCart)
		{
			TrapActivate();
		}
		if (!trapActive || physGrabObject.grabbed)
		{
			return;
		}
		enemyInvestigate = true;
		LoopPlaying = true;
		if (FrogJumpActive)
		{
			FrogJumpLerp += FrogJumpSpeed * Time.deltaTime;
			if (FrogJumpLerp >= 1f)
			{
				FrogJumpLerp = 0f;
				FrogJumpActive = false;
			}
		}
		FrogFeet.transform.localEulerAngles = new Vector3(0f, 0f, FrogJumpCurve.Evaluate(FrogJumpLerp) * FrogJumpIntensity);
		FrogCrank.transform.Rotate(0f, 0f, 80f * Time.deltaTime);
		float num = 40f;
		float num2 = 1f * Mathf.Sin(Time.time * num);
		float z = 1f * Mathf.Sin(Time.time * num + MathF.PI / 2f);
		Frog.transform.localRotation = initialFrogRotation * Quaternion.Euler(num2, 0f, z);
		Frog.transform.localPosition = new Vector3(Frog.transform.localPosition.x, Frog.transform.localPosition.y - num2 * 0.005f * Time.deltaTime, Frog.transform.localPosition.z);
		if (frogJumpTimer > 0f)
		{
			frogJumpTimer -= Time.deltaTime;
		}
		else if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("FrogJumpRPC", RpcTarget.All);
			}
		}
		else
		{
			FrogJump();
		}
	}

	public void FrogJump()
	{
		if (impactDetector.inCart)
		{
			TrapStop();
			return;
		}
		frogJumpTimer = UnityEngine.Random.Range(0.5f, 0.8f);
		enemyInvestigate = true;
		Jump.Play(physgrabobject.centerPoint);
		FrogJumpActive = true;
		FrogJumpLerp = 0f;
		enemyInvestigateRange = 15f;
		if (isLocal)
		{
			if (Vector3.Dot(Frog.transform.up, Vector3.up) > 0.5f)
			{
				rb.AddForce(Vector3.up * 1f, ForceMode.Impulse);
				rb.AddForce(base.transform.forward * 1.5f, ForceMode.Impulse);
				Vector3 vector = UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(0.05f, 0.1f);
				vector.z = 0f;
				vector.x = 0f;
				rb.AddTorque(vector * 0.25f, ForceMode.Impulse);
			}
			else
			{
				rb.AddForce(Vector3.up * 1f, ForceMode.Impulse);
				Vector3 normalized = UnityEngine.Random.insideUnitSphere.normalized;
				rb.AddTorque(normalized * 0.03f, ForceMode.Impulse);
			}
		}
	}

	[PunRPC]
	public void FrogJumpRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			FrogJump();
		}
	}

	public void TrapStop()
	{
		trapActive = false;
		trapStart = false;
		LoopPlaying = false;
		trapTriggered = false;
		CrankEnd.Play(physgrabobject.centerPoint);
	}

	public void TrapActivate()
	{
		if (!trapTriggered)
		{
			CrankStart.Play(physgrabobject.centerPoint);
			trapActive = true;
			trapTriggered = true;
			frogJumpTimer = 0f;
		}
	}
}
