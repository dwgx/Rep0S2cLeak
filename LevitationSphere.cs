using System.Collections.Generic;
using UnityEngine;

public class LevitationSphere : MonoBehaviour
{
	public enum State
	{
		start,
		levitate,
		end,
		inactive
	}

	public ParticleSystem upParticle;

	public Sound levitationLoop;

	public float levitateDuration = 3f;

	internal State state = State.inactive;

	private bool stateStart = true;

	public Transform sphereEffects;

	public Light attackLight;

	private float range = 3f;

	private List<PlayerAvatar> capturedPlayerAvatars = new List<PlayerAvatar>();

	private List<PhysGrabObject> capturedPhysGrabObjects = new List<PhysGrabObject>();

	private float checkTimer;

	private int particleCount;

	private float tumblePhysObjectCheckTimer;

	private float stateTimer;

	private void StateMachine()
	{
		switch (state)
		{
		case State.start:
			StateStart();
			break;
		case State.levitate:
			StateLevitate();
			break;
		case State.end:
			StateEnd();
			break;
		case State.inactive:
			StateInactive();
			break;
		}
	}

	private void Reset()
	{
		checkTimer = 0f;
		particleCount = 0;
		tumblePhysObjectCheckTimer = 0f;
		capturedPlayerAvatars.Clear();
		capturedPhysGrabObjects.Clear();
		sphereEffects.localScale = Vector3.zero;
		attackLight.intensity = 0f;
		sphereEffects.gameObject.SetActive(value: false);
	}

	private void StateInactive()
	{
		if (stateStart)
		{
			Reset();
			stateStart = false;
			base.gameObject.SetActive(value: false);
			Object.Destroy(base.gameObject, 1f);
		}
	}

	private void StateEnd()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		if (sphereEffects.gameObject.activeSelf)
		{
			sphereEffects.localScale = Vector3.Lerp(sphereEffects.localScale, Vector3.zero, Time.deltaTime * 20f);
			attackLight.intensity = Mathf.Lerp(attackLight.intensity, 0f, Time.deltaTime * 20f);
			if (sphereEffects.localScale.x < 0.01f)
			{
				StateSet(State.inactive);
			}
		}
		else
		{
			StateSet(State.inactive);
		}
	}

	private void StateStart()
	{
		if (stateStart)
		{
			Reset();
			sphereEffects.gameObject.SetActive(value: true);
			stateStart = false;
		}
		sphereEffects.localScale = Vector3.Lerp(sphereEffects.localScale, Vector3.one * 1.2f, Time.deltaTime * 6f);
		attackLight.intensity = 4f * sphereEffects.localScale.magnitude;
		if (sphereEffects.localScale.x > 1.19f)
		{
			attackLight.intensity = 4f;
			sphereEffects.localScale = Vector3.one * 1.2f;
			StateSet(State.levitate);
		}
	}

	private void StateLevitate()
	{
		if (stateStart)
		{
			stateTimer = 0f;
			stateStart = false;
			GetAllWithinRange();
		}
		if (checkTimer > 0.35f)
		{
			GetAllWithinRange();
			checkTimer = 0f;
		}
		checkTimer += Time.deltaTime;
		stateTimer += Time.deltaTime;
		if (stateTimer >= levitateDuration)
		{
			StateSet(State.end);
		}
		else
		{
			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				return;
			}
			foreach (PhysGrabObject capturedPhysGrabObject in capturedPhysGrabObjects)
			{
				if ((bool)capturedPhysGrabObject && capturedPhysGrabObject.isEnemy)
				{
					Enemy enemy = capturedPhysGrabObject.GetComponent<EnemyRigidbody>().enemy;
					if ((bool)enemy && enemy.HasStateStunned && enemy.Type < EnemyType.VeryHeavy)
					{
						enemy.StateStunned.Set(4f);
					}
				}
				capturedPhysGrabObject.OverrideZeroGravity();
			}
		}
	}

	private void RemoveAllOutOfRange()
	{
		for (int num = capturedPlayerAvatars.Count - 1; num >= 0; num--)
		{
			PlayerAvatar playerAvatar = capturedPlayerAvatars[num];
			if (!playerAvatar)
			{
				capturedPlayerAvatars.RemoveAt(num);
			}
			else if (Vector3.Distance(new Vector3(playerAvatar.transform.position.x, base.transform.position.y, playerAvatar.transform.position.z), base.transform.position) > range * 1.2f)
			{
				capturedPlayerAvatars.RemoveAt(num);
			}
		}
		for (int num2 = capturedPhysGrabObjects.Count - 1; num2 >= 0; num2--)
		{
			PhysGrabObject physGrabObject = capturedPhysGrabObjects[num2];
			if (!physGrabObject)
			{
				capturedPhysGrabObjects.RemoveAt(num2);
			}
			else if (Vector3.Distance(new Vector3(physGrabObject.transform.position.x, base.transform.position.y, physGrabObject.transform.position.z), base.transform.position) > range * 1.2f)
			{
				capturedPhysGrabObjects.RemoveAt(num2);
			}
		}
	}

	private void StateLevitateFixed()
	{
		if (state != State.levitate)
		{
			return;
		}
		if (tumblePhysObjectCheckTimer > 1f)
		{
			foreach (PlayerAvatar capturedPlayerAvatar in capturedPlayerAvatars)
			{
				if (capturedPlayerAvatar.tumble.isTumbling)
				{
					PhysGrabObject physGrabObject = capturedPlayerAvatar.tumble.physGrabObject;
					if (!capturedPhysGrabObjects.Contains(physGrabObject))
					{
						capturedPhysGrabObjects.Add(physGrabObject);
					}
				}
			}
			tumblePhysObjectCheckTimer = 0f;
		}
		else
		{
			tumblePhysObjectCheckTimer += Time.fixedDeltaTime;
		}
		foreach (PhysGrabObject capturedPhysGrabObject in capturedPhysGrabObjects)
		{
			if ((bool)capturedPhysGrabObject)
			{
				float num = 10f;
				if ((bool)capturedPhysGrabObject.GetComponent<PlayerTumble>())
				{
					num = 20f;
				}
				if ((bool)capturedPhysGrabObject && (bool)capturedPhysGrabObject.rb && !capturedPhysGrabObject.rb.isKinematic)
				{
					capturedPhysGrabObject.rb.AddForce(Vector3.up * Time.fixedDeltaTime * num, ForceMode.Force);
					capturedPhysGrabObject.rb.AddTorque(Vector3.up * Time.fixedDeltaTime * 0.2f, ForceMode.Force);
					capturedPhysGrabObject.rb.AddTorque(Vector3.left * Time.fixedDeltaTime * 0.1f, ForceMode.Force);
					capturedPhysGrabObject.rb.velocity = Vector3.Lerp(capturedPhysGrabObject.rb.velocity, new Vector3(0f, capturedPhysGrabObject.rb.velocity.y, 0f), Time.fixedDeltaTime * 2f);
				}
			}
		}
		int num2 = capturedPlayerAvatars.Count + capturedPhysGrabObjects.Count;
		if (particleCount < num2)
		{
			Vector3 position = Vector3.zero;
			bool flag = false;
			if (particleCount < capturedPlayerAvatars.Count)
			{
				if ((bool)capturedPlayerAvatars[particleCount])
				{
					position = capturedPlayerAvatars[particleCount].transform.position;
					flag = true;
				}
			}
			else
			{
				int num3 = particleCount - capturedPlayerAvatars.Count;
				if (num3 < capturedPhysGrabObjects.Count && (bool)capturedPhysGrabObjects[num3])
				{
					position = capturedPhysGrabObjects[num3].transform.position;
					flag = true;
				}
			}
			if (flag)
			{
				Vector3 vector = Random.insideUnitSphere * 2f;
				vector.y = 0f - Mathf.Abs(vector.y);
				position += vector;
				upParticle.transform.position = position;
				upParticle.Emit(1);
			}
			particleCount++;
		}
		else
		{
			particleCount = 0;
		}
	}

	private void FixedUpdate()
	{
		if (state != State.levitate)
		{
			return;
		}
		foreach (PlayerAvatar capturedPlayerAvatar in capturedPlayerAvatars)
		{
			if ((bool)capturedPlayerAvatar && capturedPlayerAvatar.isLocal && (bool)PlayerController.instance)
			{
				PlayerController.instance.AntiGravity(0.1f);
				PlayerController.instance.rb.AddForce(Vector3.up * Time.fixedDeltaTime * 200f, ForceMode.Force);
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			StateLevitateFixed();
		}
	}

	private void Update()
	{
		StateMachine();
		levitationLoop.PlayLoop(state == State.levitate || state == State.start, 1f, 1f);
	}

	public void StateSet(State _state)
	{
		if (state != _state)
		{
			state = _state;
			stateStart = true;
		}
	}

	public void GetAllWithinRange()
	{
		RemoveAllOutOfRange();
		foreach (PlayerAvatar item in SemiFunc.PlayerGetAllPlayerAvatarWithinRange(range, base.transform.position))
		{
			if (!capturedPlayerAvatars.Contains(item))
			{
				capturedPlayerAvatars.Add(item);
			}
		}
		foreach (PhysGrabObject item2 in SemiFunc.PhysGrabObjectGetAllWithinRange(range, base.transform.position))
		{
			if (!capturedPhysGrabObjects.Contains(item2))
			{
				capturedPhysGrabObjects.Add(item2);
			}
		}
	}

	private void OnEnable()
	{
		StateSet(State.start);
	}

	private void OnDisable()
	{
		StateSet(State.inactive);
		StateInactive();
	}
}
