using Photon.Pun;
using UnityEngine;

public class ScreamDollValuable : MonoBehaviour
{
	public enum States
	{
		Idle,
		Active
	}

	private Animator animator;

	private PhysGrabObject physGrabObject;

	private Rigidbody rb;

	public Sound soundScreamLoop;

	private PhotonView photonView;

	internal States currentState;

	private bool stateStart;

	private bool loopPlaying;

	private void StateActive()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		loopPlaying = true;
		animator.SetBool("active", value: true);
		animator.enabled = true;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (Random.Range(0, 100) < 7)
			{
				rb.AddForce(Random.insideUnitSphere * 3f, ForceMode.Impulse);
				rb.AddTorque(Random.insideUnitSphere * 7f, ForceMode.Impulse);
			}
			Quaternion turnX = Quaternion.Euler(0f, 180f, 0f);
			Quaternion turnY = Quaternion.Euler(0f, 0f, 0f);
			Quaternion identity = Quaternion.identity;
			bool flag = false;
			foreach (PhysGrabber item in physGrabObject.playerGrabbing)
			{
				if (item.isRotating)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				physGrabObject.TurnXYZ(turnX, turnY, identity);
			}
			if (!physGrabObject.grabbed)
			{
				SetState(States.Idle);
			}
		}
		if (physGrabObject.grabbedLocal)
		{
			PhysGrabber.instance.OverridePullDistanceIncrement(-1f * Time.fixedDeltaTime);
		}
	}

	private void StateIdle()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		loopPlaying = false;
		animator.SetBool("active", value: false);
		if (SemiFunc.IsMasterClientOrSingleplayer() && physGrabObject.grabbed)
		{
			SetState(States.Active);
		}
	}

	[PunRPC]
	public void SetStateRPC(States state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = state;
			stateStart = true;
		}
	}

	private void SetState(States state)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				SetStateRPC(state);
				return;
			}
			photonView.RPC("SetStateRPC", RpcTarget.All, state);
		}
	}

	private void Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
		photonView = GetComponent<PhotonView>();
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		soundScreamLoop.PlayLoop(loopPlaying, 5f, 5f);
	}

	private void FixedUpdate()
	{
		switch (currentState)
		{
		case States.Active:
			StateActive();
			break;
		case States.Idle:
			StateIdle();
			break;
		}
	}

	public void EnemyInvestigate()
	{
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			EnemyDirector.instance.SetInvestigate(base.transform.position, 20f);
		}
	}

	public void StopAnimator()
	{
		animator.enabled = false;
	}

	public void OnHurtColliderHitEnemy()
	{
		physGrabObject.heavyBreakImpulse = true;
	}
}
