using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavMeshAgent : MonoBehaviour
{
	internal NavMeshAgent Agent;

	internal Vector3 AgentVelocity;

	public bool updateRotation;

	private float StopTimer;

	private float DisableTimer;

	internal float DefaultSpeed;

	internal float DefaultAcceleration;

	private float OverrideTimer;

	private float SetPathTimer;

	private void Awake()
	{
		Agent = GetComponent<NavMeshAgent>();
		if (!updateRotation)
		{
			Agent.updateRotation = false;
		}
		if (GameManager.instance.gameMode == 0 || PhotonNetwork.IsMasterClient)
		{
			Agent.enabled = true;
		}
		else
		{
			Agent.enabled = false;
		}
		DefaultSpeed = Agent.speed;
		DefaultAcceleration = Agent.acceleration;
	}

	private void OnEnable()
	{
		if (Debug.isDebugBuild && !Agent.isOnNavMesh && !NavMesh.SamplePosition(Agent.transform.position, out var _, 10f, -1))
		{
			string text = "Enemy: Not on navmesh: ";
			text = text + "\n     Enemy Name: " + GetComponentInParent<EnemyParent>()?.name;
			if (SemiFunc.GetRoomVolumeAtPosition(Agent.transform.position, out var room, out var localPosition))
			{
				text = text + "\n     Module Name: " + room.Module?.name;
				string text2 = text;
				Vector3 vector = localPosition;
				text = text2 + "\n     Local Position: " + vector.ToString();
			}
			else
			{
				text += "\n     Module Name: N/A";
				text = text + "\n     Position: " + Agent.transform.position.ToString();
			}
			Debug.LogError(text + "\n", base.gameObject);
		}
	}

	private void Update()
	{
		AgentVelocity = Agent.velocity;
		if (SetPathTimer > 0f)
		{
			SetPathTimer -= Time.deltaTime;
		}
		if (DisableTimer > 0f)
		{
			Agent.enabled = false;
			DisableTimer -= Time.deltaTime;
			return;
		}
		if (!Agent.enabled)
		{
			Agent.enabled = true;
		}
		if (StopTimer > 0f)
		{
			if (Agent.enabled && Agent.isOnNavMesh)
			{
				Agent.isStopped = true;
			}
			StopTimer -= Time.deltaTime;
		}
		else if (Agent.enabled && Agent.isOnNavMesh && Agent.isStopped)
		{
			Agent.isStopped = false;
		}
		if (OverrideTimer > 0f)
		{
			OverrideTimer -= Time.deltaTime;
			if (OverrideTimer <= 0f)
			{
				Agent.speed = DefaultSpeed;
				Agent.acceleration = DefaultAcceleration;
			}
		}
	}

	public void OverrideAgent(float speed, float acceleration, float time)
	{
		Agent.speed = speed;
		Agent.acceleration = acceleration;
		OverrideTimer = time;
	}

	public void UpdateAgent(float speed, float acceleration)
	{
		Agent.speed = speed;
		Agent.acceleration = acceleration;
	}

	public void AgentMove(Vector3 position)
	{
		Vector3 velocity = Agent.velocity;
		Vector3 destination = Agent.destination;
		if (OnNavmesh(position))
		{
			Warp(position);
			SetDestination(destination);
			Agent.velocity = velocity;
		}
	}

	public bool OnNavmesh(Vector3 position, float range = 5f, bool _checkPit = false)
	{
		if (NavMesh.SamplePosition(position, out var hit, range, -1))
		{
			if (!_checkPit)
			{
				return true;
			}
			if (Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
			{
				return true;
			}
		}
		return false;
	}

	public void Warp(Vector3 position, bool _force = false)
	{
		if (!_force && Vector3.Distance(base.transform.position, position) < 1f)
		{
			return;
		}
		if (DisableTimer > 0f)
		{
			Agent.enabled = true;
		}
		if (OnNavmesh(position))
		{
			Agent.Warp(position);
			if (DisableTimer > 0f)
			{
				Agent.enabled = false;
			}
		}
	}

	public void ResetPath()
	{
		if (Agent.enabled && Agent.isOnNavMesh && HasPath())
		{
			Agent.ResetPath();
		}
	}

	public bool CanReach(Vector3 _target, float _range)
	{
		if (!Agent.enabled)
		{
			return true;
		}
		if (!Agent.hasPath)
		{
			return true;
		}
		if (Vector3.Distance(GetPoint(), _target) > _range)
		{
			return false;
		}
		return true;
	}

	public void SetDestination(Vector3 position)
	{
		if (Agent.enabled && Agent.isOnNavMesh)
		{
			if (!Agent.hasPath)
			{
				SetPathTimer = 0.1f;
			}
			Agent.SetDestination(position);
		}
	}

	public void Stop(float time)
	{
		if (Agent.enabled && Agent.isOnNavMesh)
		{
			StopTimer = time;
			if (StopTimer == 0f)
			{
				Agent.isStopped = false;
			}
			else
			{
				Agent.isStopped = true;
			}
		}
	}

	public bool IsStopped()
	{
		if (StopTimer > 0f)
		{
			return true;
		}
		return false;
	}

	public void Disable(float time)
	{
		Agent.enabled = false;
		DisableTimer = time;
	}

	public void Enable()
	{
		if (DisableTimer > 0f)
		{
			Agent.enabled = true;
			DisableTimer = 0f;
		}
	}

	public bool IsDisabled()
	{
		if (DisableTimer > 0f)
		{
			return true;
		}
		return false;
	}

	public Vector3 GetPoint()
	{
		if (Agent.hasPath)
		{
			return Agent.path.corners[Agent.path.corners.Length - 1];
		}
		return new Vector3(-1000f, 1000f, 1000f);
	}

	public Vector3 GetDestination()
	{
		if (Agent.hasPath)
		{
			return Agent.destination;
		}
		return base.transform.position;
	}

	public bool HasPath()
	{
		if (SetPathTimer > 0f || Agent.hasPath)
		{
			return true;
		}
		return false;
	}

	public NavMeshPath CalculatePath(Vector3 position)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		if (!Agent.enabled)
		{
			return navMeshPath;
		}
		Agent.CalculatePath(position, navMeshPath);
		return navMeshPath;
	}
}
