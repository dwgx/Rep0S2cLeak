using System.Linq;
using Photon.Pun;
using UnityEngine;

public class ChompBookTrap : Trap
{
	private Animator animator;

	[Space]
	[Header("Book Components")]
	public GameObject closedBookTop;

	public GameObject closedBookBot;

	public GameObject chainLock;

	public GameObject biteBookTop;

	public GameObject biteBookBot;

	[Space]
	[Header("Sounds")]
	public Sound chomp;

	public Sound lockBreak;

	[Space]
	private Quaternion initialBookRotation;

	private Rigidbody rb;

	public ParticleSystem lockParticle;

	private Transform targetTransform;

	private Vector3 playerDirection;

	public int biteAmount;

	private int biteCount;

	private Quaternion lookRotation;

	private float attackedTimer;

	private bool trapStopped;

	protected override void Start()
	{
		base.Start();
		initialBookRotation = closedBookTop.transform.localRotation;
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
	}

	protected override void Update()
	{
		base.Update();
		if (trapStart)
		{
			TrapActivate();
		}
		if (trapActive)
		{
			physGrabObject.OverrideIndestructible();
			enemyInvestigate = true;
		}
	}

	private void FixedUpdate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && trapActive && (bool)targetTransform)
		{
			playerDirection = (targetTransform.position - physGrabObject.midPoint).normalized;
			Quaternion quaternion = Quaternion.LookRotation(targetTransform.position - physGrabObject.midPoint);
			lookRotation = Quaternion.Slerp(lookRotation, quaternion, Time.deltaTime * 5f);
			Vector3 torque = SemiFunc.PhysFollowRotation(base.transform, lookRotation, rb, 0.3f);
			if (physGrabObject.playerGrabbing.Count > 0)
			{
				torque *= 0.25f;
			}
			rb.AddTorque(torque, ForceMode.Impulse);
			if (attackedTimer <= 0f)
			{
				Vector3 vector = SemiFunc.PhysFollowPosition(base.transform.position, targetTransform.position, rb.velocity, 1.5f);
				rb.AddForce(vector * 10f * Time.fixedDeltaTime, ForceMode.Impulse);
			}
			else
			{
				attackedTimer -= Time.fixedDeltaTime;
			}
			physGrabObject.OverrideZeroGravity();
		}
	}

	public void Attack()
	{
		if (isLocal)
		{
			targetTransform = SemiFunc.PlayerGetNearestTransformWithinRange(10f, physGrabObject.centerPoint, doRaycastCheck: true, LayerMask.GetMask("Default"));
			if ((bool)targetTransform)
			{
				attackedTimer = 0.5f;
				rb.AddForce(playerDirection * 2f, ForceMode.Impulse);
			}
			else
			{
				rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
				Vector3 normalized = Random.insideUnitSphere.normalized;
				rb.AddForce(normalized * 3f, ForceMode.Impulse);
				rb.AddTorque(normalized * 1f, ForceMode.Impulse);
			}
			biteCount++;
			if (biteCount >= biteAmount)
			{
				TrapStop();
			}
		}
	}

	public void ChompSound()
	{
		chomp.Play(physGrabObject.centerPoint);
	}

	public void StopAnimation()
	{
		if (trapStopped)
		{
			animator.enabled = false;
		}
	}

	private void TrapStopLogic()
	{
		trapActive = false;
		trapStopped = true;
		DeparentAndDestroy(lockParticle);
	}

	public void TrapStop()
	{
		if (!GameManager.Multiplayer())
		{
			TrapStopRPC();
		}
		else
		{
			photonView.RPC("TrapStopRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void TrapStopRPC()
	{
		TrapStopLogic();
	}

	private void DeparentAndDestroy(ParticleSystem particleSystem)
	{
		if ((bool)particleSystem && particleSystem.isPlaying)
		{
			particleSystem.gameObject.transform.parent = null;
			ParticleSystem.MainModule main = particleSystem.main;
			main.stopAction = ParticleSystemStopAction.Destroy;
			particleSystem.Stop(withChildren: false);
		}
	}

	public void TrapActivate()
	{
		if (!trapTriggered)
		{
			GrabRelease();
			trapActive = true;
			trapTriggered = true;
			biteBookTop.SetActive(value: true);
			biteBookBot.SetActive(value: true);
			closedBookTop.SetActive(value: false);
			closedBookBot.SetActive(value: false);
			chainLock.SetActive(value: false);
			lockBreak.Play(physGrabObject.centerPoint);
			lockParticle.Play(withChildren: false);
			animator.enabled = true;
		}
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
}
