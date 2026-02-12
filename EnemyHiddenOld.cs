using System;
using Photon.Pun;
using UnityEngine;

public class EnemyHiddenOld : MonoBehaviour
{
	private enum State
	{
		Roam,
		PlayerNotice,
		GetPlayer,
		GoToTarget,
		PickUpTarget,
		FindFarawayPoint,
		KidnapTarget,
		TauntTarget,
		DropTarget,
		Despawn
	}

	private Vector3 startPosition;

	private Vector3 footStepPosition;

	public Materials.MaterialTrigger material;

	public Transform grounded;

	public Transform footstepParticlesTransform;

	public ParticleSystem footstepParticleSmoke;

	private bool rightFoot = true;

	private Vector3 previousPosition;

	public ParticleSystem footstepParticleFoot;

	private bool isSprinting;

	private int currentState;

	private bool settingState;

	private int stateSetTo = -1;

	private PhotonView photonView;

	private float stateTimer;

	private bool stateEnd;

	private bool stateStart;

	private float initialStateTime;

	private float sprintingTime;

	public ParticleSystem breathParticles;

	private float breathTimer;

	private bool isBreathing;

	private bool breatheIn = true;

	private float breathCycleTimer;

	public Sound soundBreatheIn;

	public Sound soundBreatheOut;

	public Sound soundFootstepWalk;

	public Sound soundFootstepSprint;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		startPosition = base.transform.position;
		footStepPosition = startPosition;
		previousPosition = startPosition;
	}

	private void Update()
	{
		StateRoam();
		StatePlayerNotice();
		StateGetPlayer();
		StateGoToTarget();
		StatePickUpTarget();
		StateFindFarawayPoint();
		StateKidnapTarget();
		StateTauntTarget();
		StateDropTarget();
		StateDespawn();
		FootstepLogic();
		SprintTick();
		BreathTick();
		Breathing();
		stateEnd = false;
		if (stateTimer > 0f)
		{
			if (initialStateTime == 0f)
			{
				initialStateTime = stateTimer;
			}
			stateTimer -= Time.deltaTime;
			stateTimer = Mathf.Max(0f, stateTimer);
		}
		else if (!stateEnd && stateTimer != -123f)
		{
			stateEnd = true;
			stateTimer = -123f;
			initialStateTime = 0f;
		}
		if (stateSetTo != -1)
		{
			currentState = stateSetTo;
			stateStart = true;
			settingState = false;
			stateEnd = false;
			stateSetTo = -1;
		}
		float num = 0.5f;
		base.transform.position = startPosition + new Vector3(Mathf.Sin(Time.time * num) * 1f, 0f, Mathf.Cos(Time.time * num) * 1f);
	}

	private void FootstepLogic()
	{
		Vector3 normalized = (base.transform.position - previousPosition).normalized;
		base.transform.LookAt(base.transform.position + normalized);
		Debug.DrawRay(base.transform.position, normalized, Color.green, 0.1f);
		previousPosition = base.transform.position;
		float num = 1f;
		if (isSprinting)
		{
			num = 1.8f;
		}
		if (!(Vector3.Distance(footStepPosition, grounded.position) > 0.5f * num))
		{
			return;
		}
		Vector3 vector = Vector3.Cross(Vector3.up, base.transform.forward);
		Vector3 vector2 = -vector;
		Vector3 vector3 = Vector3.down + (rightFoot ? (vector * 0.2f) : (vector2 * 0.2f));
		vector3 += base.transform.forward * 0.3f * num;
		rightFoot = !rightFoot;
		Debug.DrawRay(base.transform.position, vector3, Color.red, 0.1f);
		if (Physics.Raycast(base.transform.position, vector3 * 2f, out var hitInfo, 3f, LayerMask.GetMask("Default")))
		{
			footStepPosition = grounded.position;
			footstepParticlesTransform.position = hitInfo.point;
			footstepParticleSmoke.Play();
			footstepParticlesTransform.transform.LookAt(base.transform.position + normalized);
			footstepParticleFoot.Play();
			if (isSprinting)
			{
				Materials.Instance.Impulse(hitInfo.point, Vector3.down, Materials.SoundType.Heavy, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
				soundFootstepSprint.Play(hitInfo.point);
			}
			else
			{
				Materials.Instance.Impulse(hitInfo.point, Vector3.down, Materials.SoundType.Medium, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
				soundFootstepWalk.Play(hitInfo.point);
			}
			Quaternion.LookRotation(base.transform.forward);
			Debug.DrawRay(base.transform.position, normalized, Color.blue, 2f);
			ParticleSystem.MainModule main = footstepParticleFoot.main;
			main.startRotation3D = true;
			float num2 = Vector2.SignedAngle(to: new Vector2(base.transform.forward.x, base.transform.forward.z), from: Vector2.up) + 90f;
			float constant = (rightFoot ? (-90f) : 90f) * (MathF.PI / 180f);
			float num3 = (rightFoot ? (-90f) : 90f);
			num3 += num2;
			num3 *= MathF.PI / 180f;
			main.startRotationX = new ParticleSystem.MinMaxCurve(constant);
			main.startRotationY = new ParticleSystem.MinMaxCurve(num3);
			main.startRotationZ = new ParticleSystem.MinMaxCurve(0f);
		}
	}

	private void StateSet(State newState)
	{
		if (settingState)
		{
			return;
		}
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient() && stateSetTo == -1)
			{
				settingState = true;
				photonView.RPC("StateSetRPC", RpcTarget.All, (int)newState);
			}
		}
		else if (stateSetTo == -1)
		{
			settingState = true;
			StateSetRPC((int)newState);
		}
	}

	[PunRPC]
	public void StateSetRPC(int state)
	{
		stateSetTo = state;
		stateTimer = 0f;
		stateEnd = true;
	}

	private bool StateIs(State state)
	{
		return currentState == (int)state;
	}

	private void Sprinting()
	{
		sprintingTime = 0.2f;
		isSprinting = true;
	}

	private void SprintTick()
	{
		if (sprintingTime > 0f)
		{
			sprintingTime -= Time.deltaTime;
		}
		else
		{
			isSprinting = false;
		}
	}

	private void Breathing()
	{
		breathTimer = 0.2f;
		isBreathing = true;
	}

	private void BreathTick()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (breathTimer > 0f)
		{
			breathTimer -= Time.deltaTime;
		}
		else
		{
			isBreathing = false;
		}
		if (!isBreathing)
		{
			return;
		}
		breathCycleTimer += Time.deltaTime;
		float num = 3f;
		if (breatheIn)
		{
			num = 4.5f;
		}
		if (breathCycleTimer > num)
		{
			breathCycleTimer = 0f;
			if (breatheIn)
			{
				BreatheIn();
			}
			else
			{
				BreatheOut();
			}
			breatheIn = !breatheIn;
		}
	}

	private void BreatheIn()
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("BreatheInRPC", RpcTarget.All);
			}
		}
		else
		{
			BreatheInRPC();
		}
	}

	[PunRPC]
	public void BreatheInRPC()
	{
		soundBreatheIn.Play(base.transform.position);
	}

	private void BreatheOut()
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("BreatheOutRPC", RpcTarget.All);
			}
		}
		else
		{
			BreatheOutRPC();
		}
	}

	[PunRPC]
	public void BreatheOutRPC()
	{
		soundBreatheOut.Play(base.transform.position);
		breathParticles.Play();
	}

	private void StateRoam()
	{
		if (StateIs(State.Roam))
		{
			if (stateStart)
			{
				stateStart = false;
			}
			_ = stateEnd;
		}
	}

	private void StatePlayerNotice()
	{
		if (StateIs(State.PlayerNotice))
		{
			if (stateStart)
			{
				stateStart = false;
			}
			_ = stateEnd;
		}
	}

	private void StateGetPlayer()
	{
		if (StateIs(State.GetPlayer))
		{
			if (stateStart)
			{
				stateStart = false;
			}
			_ = stateEnd;
		}
	}

	private void StateGoToTarget()
	{
		if (StateIs(State.GoToTarget))
		{
			if (stateStart)
			{
				stateStart = false;
			}
			_ = stateEnd;
		}
	}

	private void StatePickUpTarget()
	{
		if (StateIs(State.PickUpTarget))
		{
			if (stateStart)
			{
				stateStart = false;
			}
			_ = stateEnd;
		}
	}

	private void StateFindFarawayPoint()
	{
		if (StateIs(State.FindFarawayPoint))
		{
			if (stateStart)
			{
				stateStart = false;
			}
			_ = stateEnd;
		}
	}

	private void StateKidnapTarget()
	{
		if (StateIs(State.KidnapTarget))
		{
			if (stateStart)
			{
				stateStart = false;
			}
			_ = stateEnd;
		}
	}

	private void StateTauntTarget()
	{
		if (StateIs(State.TauntTarget))
		{
			if (stateStart)
			{
				stateStart = false;
			}
			_ = stateEnd;
		}
	}

	private void StateDropTarget()
	{
		if (StateIs(State.DropTarget))
		{
			if (stateStart)
			{
				stateStart = false;
			}
			_ = stateEnd;
		}
	}

	private void StateDespawn()
	{
		if (StateIs(State.Despawn))
		{
			if (stateStart)
			{
				stateStart = false;
			}
			_ = stateEnd;
		}
	}
}
