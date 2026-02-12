using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class WizardBroomValuable : Trap
{
	public enum States
	{
		Idle,
		MoveForward,
		Turn,
		Unstick,
		grabbed,
		Sleep
	}

	public UnityEvent broomTimer;

	private Rigidbody rb;

	private LayerMask visionObstruct;

	private Vector3 rayOffset = new Vector3(-1.92f, 0f, 0f);

	private float rayDistance = 1f;

	private float raycastCooldown = 0.2f;

	private float raycastTimer;

	private PhysGrabObjectImpactDetector impactDetector;

	public GameObject box;

	public GameObject broom;

	public ParticleSystem plankParticles;

	public ParticleSystem bitParticles;

	public Sound broomBoxBreakSound;

	private float stuckTimer = 2f;

	private float unStickTimer;

	private Vector3 randomDirection;

	private Vector3 randomTorque;

	public States currentState;

	private bool stateStart;

	private bool CheckForwardRaycast(out RaycastHit _hit)
	{
		_hit = default(RaycastHit);
		if (raycastTimer < raycastCooldown)
		{
			return false;
		}
		raycastTimer = 0f;
		Vector3 vector = broom.transform.TransformPoint(rayOffset);
		Vector3 vector2 = -broom.transform.right;
		Debug.DrawRay(vector, vector2, Color.red);
		return Physics.Raycast(vector, vector2, out _hit, rayDistance, visionObstruct);
	}

	protected override void Start()
	{
		base.Start();
		rb = GetComponent<Rigidbody>();
		impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		visionObstruct = SemiFunc.LayerMaskGetVisionObstruct();
	}

	protected override void Update()
	{
		base.Update();
	}

	private void FixedUpdate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			switch (currentState)
			{
			case States.MoveForward:
				StateMoveForward();
				break;
			case States.Turn:
				StateTurn();
				break;
			case States.Unstick:
				StateUnstick();
				break;
			case States.grabbed:
				StateGrabbed();
				break;
			case States.Idle:
				StateIdle();
				break;
			case States.Sleep:
				StateSleep();
				break;
			}
			if (stuckTimer > 0f)
			{
				stuckTimer -= Time.fixedDeltaTime;
			}
			if (physGrabObject.grabbed && trapActive)
			{
				SetState(States.grabbed);
			}
			if (impactDetector.inCart && trapTriggered)
			{
				TrapStop();
			}
		}
	}

	private void StateMoveForward()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		raycastTimer += Time.fixedDeltaTime;
		physGrabObject.OverrideZeroGravity();
		if (stuckTimer <= 0f)
		{
			if (rb.velocity.magnitude > 0.1f)
			{
				stuckTimer = Random.Range(0.1f, 2f);
				return;
			}
			SetState(States.Unstick);
		}
		if (CheckForwardRaycast(out var _) || Vector3.Dot(broom.transform.right, Vector3.up) > 0.1f)
		{
			SetState(States.Turn);
		}
		else
		{
			rb.AddForce(-broom.transform.right * 2000f * Time.fixedDeltaTime, ForceMode.Force);
		}
	}

	private void StateTurn()
	{
		if (stateStart)
		{
			stateStart = false;
			rb.AddForce(Vector3.up * 50f * Time.fixedDeltaTime, ForceMode.Impulse);
		}
		raycastTimer += Time.fixedDeltaTime;
		physGrabObject.OverrideZeroGravity();
		if (CheckForwardRaycast(out var _hit))
		{
			Vector3 vector = Vector3.Cross(_hit.normal, broom.transform.right);
			rb.AddTorque(vector * 200f * Time.fixedDeltaTime, ForceMode.Force);
			rb.AddForce(broom.transform.right * 500f * Time.fixedDeltaTime, ForceMode.Force);
		}
		else
		{
			SetState(States.MoveForward);
		}
	}

	private void StateUnstick()
	{
		if (stateStart)
		{
			stateStart = false;
			unStickTimer = Random.Range(1f, 2f);
			randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
			rb.AddForce(randomDirection * 500f * Time.fixedDeltaTime, ForceMode.Impulse);
			randomTorque = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
		}
		if (unStickTimer > 0f)
		{
			rb.AddTorque(randomTorque * 5000f * Time.fixedDeltaTime, ForceMode.Force);
			unStickTimer -= Time.fixedDeltaTime;
		}
		else
		{
			stuckTimer = Random.Range(0.1f, 2f);
			SetState(States.MoveForward);
		}
	}

	private void StateIdle()
	{
		stateStart = false;
	}

	private void StateGrabbed()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		physGrabObject.OverrideZeroGravity();
		physGrabObject.OverrideDrag(0.2f);
		physGrabObject.OverrideAngularDrag(0.2f);
		if (!physGrabObject.grabbed)
		{
			SetState(States.MoveForward);
		}
	}

	private void StateSleep()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		if (!impactDetector.inCart && trapTriggered)
		{
			trapActive = true;
			SetState(States.MoveForward);
		}
	}

	public void TrapActivate()
	{
		if (!trapTriggered)
		{
			GrabRelease();
			broomBoxBreakSound.Play(physGrabObject.centerPoint);
			plankParticles.Play();
			bitParticles.Play();
			box.SetActive(value: false);
			broom.SetActive(value: true);
			trapActive = true;
			trapTriggered = true;
			SetState(States.MoveForward);
		}
	}

	public void TrapStop()
	{
		trapActive = false;
		SetState(States.Sleep);
	}

	public void GrabRelease()
	{
		bool flag = false;
		foreach (PhysGrabber item in physGrabObject.playerGrabbing.ToList())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				item.ReleaseObject(photonView.ViewID);
			}
			else
			{
				item.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, 0.1f, photonView.ViewID);
			}
			flag = true;
		}
		if (flag)
		{
			if (GameManager.instance.gameMode == 0)
			{
				GrabReleaseRPC();
			}
			else
			{
				photonView.RPC("GrabReleaseRPC", RpcTarget.All);
			}
		}
	}

	[PunRPC]
	private void GrabReleaseRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
			physGrabObject.grabDisableTimer = 1f;
		}
	}

	private void SetState(States state)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			currentState = state;
			stateStart = true;
		}
	}
}
