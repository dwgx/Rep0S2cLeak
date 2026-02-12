using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ValuablePlane : Trap
{
	public enum State
	{
		Inactive,
		Idle,
		MoveForward,
		TakeOff,
		Flying
	}

	[Header("Animation")]
	public Transform planeBody;

	public Transform pilot;

	public Transform propeller;

	public Transform wheels;

	public Transform crank;

	[Space]
	public AnimationCurve planeBodyCurve;

	[Header("Flight Settings")]
	public float takeOffSpeed = 5.5f;

	public float liftPerSpeed = 3.5f;

	public float maxLiftForce = 18f;

	public float cruiseSpeed = 6.5f;

	public float flightSpeed = 4f;

	[Header("Sounds")]
	public Sound sfxPlaneStart;

	public Sound sfxPlaneStop;

	public Sound sfxPlaneLoop;

	public Sound sfxCrankLoop;

	[Header("Colliders")]
	public List<Collider> planeColliders;

	[Header("Physics Materials")]
	public PhysicMaterial defaultPhysicMaterial;

	public PhysicMaterial movingPhysicMaterial;

	[Header("Transforms")]
	public Transform centerTransform;

	private float planeBodyLerp;

	private float wheelsLerp;

	private float pilotLerp = 0.9f;

	private bool grabbedPrev;

	private bool loopPlaying;

	private bool loopPlayingPrevious;

	private float loopPitch;

	private bool playersNearby;

	private PhysGrabObjectImpactDetector impactDetector;

	private Vector3 boxSize = new Vector3(0.16f, 0.1f, 0.42f);

	private bool activated;

	private State currentState;

	private bool stateImpulse;

	private float stateTimer;

	private float levelStrength = 4f;

	private float levelDamping = 1.2f;

	private float bankLimitDeg = 45f;

	protected override void Start()
	{
		base.Start();
		impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		currentState = State.Inactive;
	}

	protected override void Update()
	{
		base.Update();
		HandleAudio();
		HandleGrabbedOrientation();
		AnimateParts();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			ServerSideThink();
		}
	}

	public void FixedUpdate()
	{
		StateMachine(fixedUpdate: true);
	}

	private void HandleAudio()
	{
		loopPlaying = currentState == State.MoveForward || currentState == State.TakeOff || currentState == State.Flying;
		loopPitch = Mathf.Lerp(loopPitch, 1f + physGrabObject.rb.velocity.magnitude * 0.8f, Time.deltaTime * 5f);
		sfxPlaneLoop.PlayLoop(loopPlaying, 0.8f, 0.8f, loopPitch);
		sfxCrankLoop.PlayLoop(currentState == State.Idle && !impactDetector.inCart, 0.8f, 0.8f, loopPitch);
		if (loopPlaying != loopPlayingPrevious)
		{
			if (loopPlaying)
			{
				sfxPlaneStart.Play(base.transform.position);
			}
			else
			{
				sfxPlaneStop.Play(base.transform.position);
			}
			loopPlayingPrevious = loopPlaying;
		}
	}

	private void HandleGrabbedOrientation()
	{
		if (physGrabObject.grabbed)
		{
			if (!grabbedPrev)
			{
				grabbedPrev = true;
				if (SemiFunc.IsMasterClientOrSingleplayer())
				{
					Quaternion turnX = Quaternion.Euler(-45f, 0f, 0f);
					Quaternion identity = Quaternion.identity;
					Quaternion identity2 = Quaternion.identity;
					physGrabObject.TurnXYZ(turnX, identity, identity2);
				}
			}
		}
		else
		{
			grabbedPrev = false;
		}
	}

	private void AnimateParts()
	{
		if (currentState == State.Idle && !impactDetector.inCart)
		{
			crank.Rotate(Vector3.right * 360f * 2f * (0f - Time.deltaTime), Space.Self);
			planeBodyLerp = (planeBodyLerp + Time.deltaTime * 4f) % 1f;
			float y = planeBodyCurve.Evaluate(planeBodyLerp) * 0.015f;
			planeBody.localPosition = new Vector3(planeBody.localPosition.x, y, planeBody.localPosition.z);
		}
		if (currentState == State.MoveForward || currentState == State.TakeOff || currentState == State.Flying)
		{
			planeBodyLerp = (planeBodyLerp + Time.deltaTime * 4f) % 1f;
			float y2 = planeBodyCurve.Evaluate(planeBodyLerp) * 0.015f;
			planeBody.localPosition = new Vector3(planeBody.localPosition.x, y2, planeBody.localPosition.z);
			wheels.Rotate(Vector3.right * 360f * 2f * Time.deltaTime, Space.Self);
			pilotLerp = (pilotLerp + Time.deltaTime) % 1f;
			float z = planeBodyCurve.Evaluate(planeBodyLerp) * 6f;
			pilot.localRotation = Quaternion.Euler(pilot.localRotation.eulerAngles.x, pilot.localRotation.eulerAngles.y, z);
			propeller.Rotate(Vector3.forward * 360f * 5f * Time.deltaTime, Space.Self);
			crank.Rotate(Vector3.right * 360f * 2f * Time.deltaTime, Space.Self);
		}
		else if (stateImpulse)
		{
			planeBody.localPosition = new Vector3(planeBody.localPosition.x, 0f, planeBody.localPosition.z);
			wheels.localRotation = Quaternion.identity;
		}
	}

	private void ServerSideThink()
	{
		if (!SemiFunc.FPSImpulse5())
		{
			return;
		}
		if (currentState != State.Inactive)
		{
			if (impactDetector.inCart || physGrabObject.playerGrabbing.Count > 0)
			{
				UpdateState(State.Idle);
				return;
			}
			bool flag = Physics.OverlapBox(centerTransform.position, boxSize / 2f, base.transform.rotation, SemiFunc.LayerMaskGetVisionObstruct()).Length != 0;
			if (flag && currentState == State.Idle)
			{
				UpdateState(State.MoveForward);
			}
			else if (!flag && currentState == State.MoveForward)
			{
				UpdateState(State.Idle);
			}
		}
		CheckForNearbyPlayers();
		if (!playersNearby)
		{
			UpdateState(State.Inactive);
		}
		StateMachine(fixedUpdate: false);
	}

	private void StateMachine(bool fixedUpdate)
	{
		switch (currentState)
		{
		case State.Inactive:
			StateInactive(fixedUpdate);
			break;
		case State.Idle:
			StateIdle(fixedUpdate);
			break;
		case State.MoveForward:
			StateMoveForward(fixedUpdate);
			break;
		case State.TakeOff:
			StateTakeOff(fixedUpdate);
			break;
		case State.Flying:
			StateFlying(fixedUpdate);
			break;
		}
	}

	private void StateInactive(bool fixedUpdate)
	{
		if (!fixedUpdate && physGrabObject.grabbed)
		{
			UpdateState(State.Idle);
		}
	}

	private void StateIdle(bool fixedUpdate)
	{
		if (fixedUpdate || !stateImpulse)
		{
			return;
		}
		foreach (Collider planeCollider in planeColliders)
		{
			if ((bool)planeCollider)
			{
				planeCollider.material = defaultPhysicMaterial;
			}
		}
		stateImpulse = false;
	}

	private void StateMoveForward(bool fixedUpdate)
	{
		if (!fixedUpdate || !playersNearby)
		{
			return;
		}
		if (stateImpulse)
		{
			stateTimer = 2f;
			foreach (Collider planeCollider in planeColliders)
			{
				if ((bool)planeCollider)
				{
					planeCollider.material = movingPhysicMaterial;
				}
			}
			stateImpulse = false;
		}
		RandomWiggle();
		ForwardPropulsion();
		if (physGrabObject.rb.velocity.magnitude >= takeOffSpeed && Mathf.Abs(Vector3.Dot(base.transform.up, Vector3.up)) > 0.9f)
		{
			UpdateState(State.TakeOff);
		}
	}

	private void RandomWiggle()
	{
		stateTimer -= Time.fixedDeltaTime;
		if (stateTimer <= 0f)
		{
			stateTimer = 1f;
			if (UnityEngine.Random.value > 0.5f)
			{
				TurnPlane();
			}
		}
	}

	private void ForwardPropulsion()
	{
		float num = cruiseSpeed - physGrabObject.rb.velocity.magnitude;
		physGrabObject.rb.AddForce(base.transform.forward * num * Time.fixedDeltaTime * 2f, ForceMode.Impulse);
	}

	private void StateTakeOff(bool fixedUpdate)
	{
		if (!fixedUpdate || !playersNearby)
		{
			return;
		}
		if (stateImpulse)
		{
			foreach (Collider planeCollider in planeColliders)
			{
				if ((bool)planeCollider)
				{
					planeCollider.material = movingPhysicMaterial;
				}
			}
			stateImpulse = false;
		}
		PushForwardAndLift();
		PitchNoseUp();
		if (!Physics.Raycast(base.transform.position, Vector3.down, out var _, 1.5f))
		{
			UpdateState(State.Flying);
		}
	}

	private void PushForwardAndLift()
	{
		float num = Mathf.Clamp(Vector3.Dot(physGrabObject.rb.velocity, base.transform.forward) * liftPerSpeed, 0f, maxLiftForce);
		physGrabObject.rb.AddForce(base.transform.forward * 1.5f, ForceMode.Force);
		physGrabObject.rb.AddForce(Vector3.up * num, ForceMode.Force);
	}

	private void PitchNoseUp()
	{
		physGrabObject.rb.AddTorque(Vector3.left * 0.2f, ForceMode.Force);
	}

	private void StateFlying(bool fixedUpdate)
	{
		if (fixedUpdate && playersNearby)
		{
			if (!Physics.Raycast(base.transform.position, Vector3.down, out var _, 1.5f))
			{
				physGrabObject.OverrideZeroGravity();
			}
			float num = Vector3.Dot(physGrabObject.rb.velocity, base.transform.forward);
			ManageSpeedAndLift(num);
			Stabilise(levelStrength, levelDamping);
			if (num > takeOffSpeed)
			{
				stateTimer = 2f;
			}
			stateTimer -= Time.fixedDeltaTime;
			if ((num < takeOffSpeed * 0.8f && Physics.Raycast(base.transform.position, Vector3.down, 2f)) || stateTimer <= 0f)
			{
				UpdateState(State.Inactive);
			}
		}
	}

	private void ManageSpeedAndLift(float forwardSpeed)
	{
		if (forwardSpeed < flightSpeed)
		{
			physGrabObject.rb.AddForce(base.transform.forward * (flightSpeed - forwardSpeed) * Time.fixedDeltaTime, ForceMode.Impulse);
		}
		if (Physics.Raycast(base.transform.position, Vector3.down, out var _, 1.5f))
		{
			float num = Mathf.Clamp(forwardSpeed * liftPerSpeed, 0f, maxLiftForce);
			physGrabObject.rb.AddForce(Vector3.up * num, ForceMode.Force);
		}
	}

	private void Stabilise(float kp, float kd)
	{
		Rigidbody rb = physGrabObject.rb;
		Vector3 up = base.transform.up;
		Vector3 up2 = Vector3.up;
		Vector3 axis = Vector3.Cross(up, up2);
		float value = Vector3.SignedAngle(up, up2, axis);
		value = Mathf.Clamp(value, 0f - bankLimitDeg, bankLimitDeg);
		Vector3 torque = axis.normalized * value * (MathF.PI / 180f) * kp - rb.angularVelocity * kd;
		rb.AddTorque(torque, ForceMode.Acceleration);
	}

	private void UpdateState(State newState)
	{
		if (newState != currentState)
		{
			currentState = newState;
			stateImpulse = true;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UpdateStateRPC", RpcTarget.All, (int)currentState);
			}
			else
			{
				UpdateStateRPC((int)currentState);
			}
		}
	}

	[PunRPC]
	private void UpdateStateRPC(int stateIndex, PhotonMessageInfo info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			currentState = (State)stateIndex;
		}
	}

	private void TurnPlane()
	{
		physGrabObject.rb.AddTorque(new Vector3(0f, UnityEngine.Random.Range(-1f, 1f) * 0.1f, 0f), ForceMode.Impulse);
	}

	private void CheckForNearbyPlayers()
	{
		playersNearby = false;
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (Vector3.Distance(item.transform.position, base.transform.position) < 20f)
			{
				playersNearby = true;
				break;
			}
		}
	}
}
