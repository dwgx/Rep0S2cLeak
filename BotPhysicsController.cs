using System;
using System.Collections.Generic;
using UnityEngine;

public class BotPhysicsController : MonoBehaviour
{
	[Serializable]
	public struct DragOverride
	{
		public float value;

		public float timeRemaining;

		public float priority;
	}

	[Serializable]
	public struct AngularDragOverride
	{
		public float value;

		public float timeRemaining;

		public float priority;
	}

	[Serializable]
	public struct GravityOverride
	{
		public bool zeroGravity;

		public float timeRemaining;

		public float priority;
	}

	[Serializable]
	public struct ExcludeLayerOverride
	{
		public int layerMask;

		public float timeRemaining;

		public float priority;
	}

	private Rigidbody rb;

	private EnemyRigidbody enemyRigidbody;

	private float physMoveTowardsTimer;

	private Vector3 physMoveTowardsDirection;

	private float physMoveTowardsSpeed;

	private float physRotateTowardsTimer;

	private Vector3 physRotateTowardsDirection;

	private float physRotateTowardsSpring;

	private float physRotateTowardsDamping;

	private float continuousTorqueTimer;

	private Vector3 continuousTorqueAxis;

	private float continuousTorqueForce;

	private Dictionary<string, DragOverride> dragOverrides = new Dictionary<string, DragOverride>();

	private Dictionary<string, AngularDragOverride> angularDragOverrides = new Dictionary<string, AngularDragOverride>();

	private Dictionary<string, GravityOverride> gravityOverrides = new Dictionary<string, GravityOverride>();

	private Dictionary<string, ExcludeLayerOverride> excludeLayerOverrides = new Dictionary<string, ExcludeLayerOverride>();

	private bool originalUseGravity;

	private float originalAngularDrag;

	private float originalDrag;

	private int originalExcludeLayers;

	private PhysGrabObject physGrabObject;

	private const string LEGACY_ID = "_legacy_default";

	public bool IsMoving => physMoveTowardsTimer > 0f;

	public bool IsRotating => physRotateTowardsTimer > 0f;

	public bool IsTorqueActive => continuousTorqueTimer > 0f;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		if (rb != null)
		{
			originalUseGravity = rb.useGravity;
			originalAngularDrag = rb.angularDrag;
			originalDrag = rb.drag;
			originalExcludeLayers = rb.excludeLayers;
		}
		enemyRigidbody = GetComponent<EnemyRigidbody>();
		physGrabObject = GetComponent<PhysGrabObject>();
	}

	private void Start()
	{
		if (enemyRigidbody == null)
		{
			enemyRigidbody = GetComponent<EnemyRigidbody>();
		}
	}

	private void Update()
	{
		PhysMoveTowardsTick();
		PhysRotateTowardsTick();
		ProcessGravityOverrides();
		ProcessAngularDragOverrides();
		ProcessDragOverrides();
		ProcessExcludeLayerOverrides();
		ContinuousTorqueTick();
	}

	private void FixedUpdate()
	{
		if (physRotateTowardsTimer > 0f && (bool)rb)
		{
			SemiFunc.PhysFollowRotationTorque(rb, Quaternion.LookRotation(physRotateTowardsDirection, Vector3.up), physRotateTowardsSpring, physRotateTowardsDamping);
		}
		if (physMoveTowardsTimer > 0f && (bool)rb)
		{
			_ = base.transform.position + physMoveTowardsDirection * 5f;
			rb.AddForce(physMoveTowardsDirection * physMoveTowardsSpeed, ForceMode.Acceleration);
		}
		if (continuousTorqueTimer > 0f && (bool)rb)
		{
			rb.AddTorque(continuousTorqueAxis * continuousTorqueForce, ForceMode.Force);
		}
	}

	public void PhysRotateTowards(Vector3 direction, float spring, float damping = 10f, float time = 0.5f)
	{
		physRotateTowardsDirection = direction.normalized;
		physRotateTowardsSpring = spring;
		physRotateTowardsDamping = damping;
		physRotateTowardsTimer = time;
		DeactivateEnemyRigidbodyPhysics(time);
	}

	public void PhysMoveTowards(Vector3 direction, float speed, float time)
	{
		physMoveTowardsDirection = direction.normalized;
		physMoveTowardsSpeed = speed;
		physMoveTowardsTimer = time;
		DeactivateEnemyRigidbodyPhysics(time);
	}

	public void PhysSetContinuousTorque(Vector3 axis, float force, float time)
	{
		continuousTorqueAxis = axis.normalized;
		continuousTorqueForce = force;
		continuousTorqueTimer = time;
		DeactivateEnemyRigidbodyPhysics(time);
	}

	private void DeactivateEnemyRigidbodyPhysics(float time)
	{
		enemyRigidbody.DeactivateFollowTargetPhysics(time);
	}

	private void PhysMoveTowardsTick()
	{
		if (physMoveTowardsTimer > 0f)
		{
			physMoveTowardsTimer -= Time.deltaTime;
		}
	}

	private void PhysRotateTowardsTick()
	{
		if (physRotateTowardsTimer > 0f)
		{
			physRotateTowardsTimer -= Time.deltaTime;
		}
	}

	private void ContinuousTorqueTick()
	{
		if (continuousTorqueTimer > 0f)
		{
			continuousTorqueTimer -= Time.deltaTime;
		}
	}

	public void SetDragOverride(string id, float dragValue, float time, float priority = 1f)
	{
		dragOverrides[id] = new DragOverride
		{
			value = dragValue,
			timeRemaining = time,
			priority = priority
		};
	}

	public void SetAngularDragOverride(string id, float angularDragValue, float time, float priority = 1f)
	{
		angularDragOverrides[id] = new AngularDragOverride
		{
			value = angularDragValue,
			timeRemaining = time,
			priority = priority
		};
	}

	public void SetGravityOverride(string id, bool zeroGravity, float time, float priority = 1f)
	{
		gravityOverrides[id] = new GravityOverride
		{
			zeroGravity = zeroGravity,
			timeRemaining = time,
			priority = priority
		};
	}

	public void SetExcludeLayerOverride(string id, int layerMask, float time, float priority = 1f)
	{
		excludeLayerOverrides[id] = new ExcludeLayerOverride
		{
			layerMask = layerMask,
			timeRemaining = time,
			priority = priority
		};
	}

	public void SetExcludeLayerOverride(string id, string layerName, float time, float priority = 1f)
	{
		int num = LayerMask.NameToLayer(layerName);
		if (num != -1)
		{
			SetExcludeLayerOverride(id, 1 << num, time, priority);
		}
	}

	public void SetExcludeLayersOverride(string id, string[] layerNames, float time, float priority = 1f)
	{
		int num = 0;
		for (int i = 0; i < layerNames.Length; i++)
		{
			int num2 = LayerMask.NameToLayer(layerNames[i]);
			if (num2 != -1)
			{
				num |= 1 << num2;
			}
		}
		if (num != 0)
		{
			SetExcludeLayerOverride(id, num, time, priority);
		}
	}

	public void RemoveDragOverride(string id)
	{
		dragOverrides.Remove(id);
	}

	public void RemoveAngularDragOverride(string id)
	{
		angularDragOverrides.Remove(id);
	}

	public void RemoveGravityOverride(string id)
	{
		gravityOverrides.Remove(id);
	}

	public void RemoveExcludeLayerOverride(string id)
	{
		excludeLayerOverrides.Remove(id);
	}

	public void ClearAllOverrides()
	{
		dragOverrides.Clear();
		angularDragOverrides.Clear();
		gravityOverrides.Clear();
		excludeLayerOverrides.Clear();
		RestoreOriginalPhysicsValues();
	}

	public void StopAll()
	{
		physMoveTowardsTimer = 0f;
		physRotateTowardsTimer = 0f;
		continuousTorqueTimer = 0f;
		ClearAllOverrides();
	}

	public void SetZeroGravity(float time)
	{
		SetGravityOverride("_legacy_default", zeroGravity: true, time, 0f);
	}

	public void SetAngularDrag(float angularDrag, float time)
	{
		SetAngularDragOverride("_legacy_default", angularDrag, time, 0f);
	}

	public void SetDrag(float drag, float time)
	{
		SetDragOverride("_legacy_default", drag, time, 0f);
	}

	public void SetExcludeLayer(int layerMask, float time)
	{
		SetExcludeLayerOverride("_legacy_default", layerMask, time, 0f);
	}

	public void SetExcludeLayer(string layerName, float time)
	{
		int num = LayerMask.NameToLayer(layerName);
		if (num != -1)
		{
			SetExcludeLayer(1 << num, time);
		}
	}

	public void SetExcludeLayers(string[] layerNames, float time)
	{
		int num = 0;
		for (int i = 0; i < layerNames.Length; i++)
		{
			int num2 = LayerMask.NameToLayer(layerNames[i]);
			if (num2 != -1)
			{
				num |= 1 << num2;
			}
		}
		if (num != 0)
		{
			SetExcludeLayer(num, time);
		}
	}

	private void ProcessGravityOverrides()
	{
		if (!rb)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (string item in new List<string>(gravityOverrides.Keys))
		{
			GravityOverride value = gravityOverrides[item];
			value.timeRemaining -= Time.deltaTime;
			if (value.timeRemaining <= 0f)
			{
				list.Add(item);
			}
			else
			{
				gravityOverrides[item] = value;
			}
		}
		foreach (string item2 in list)
		{
			gravityOverrides.Remove(item2);
		}
		bool flag = false;
		foreach (GravityOverride value2 in gravityOverrides.Values)
		{
			if (value2.zeroGravity)
			{
				flag = true;
				break;
			}
		}
		if (gravityOverrides.Count > 0 && flag)
		{
			physGrabObject.OverrideZeroGravity();
		}
	}

	private void ProcessAngularDragOverrides()
	{
		if (!rb)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (string item in new List<string>(angularDragOverrides.Keys))
		{
			AngularDragOverride value = angularDragOverrides[item];
			value.timeRemaining -= Time.deltaTime;
			if (value.timeRemaining <= 0f)
			{
				list.Add(item);
			}
			else
			{
				angularDragOverrides[item] = value;
			}
		}
		foreach (string item2 in list)
		{
			angularDragOverrides.Remove(item2);
		}
		if (angularDragOverrides.Count > 0)
		{
			float num = float.MinValue;
			float num2 = float.MinValue;
			foreach (AngularDragOverride value2 in angularDragOverrides.Values)
			{
				if (value2.priority > num2 || (value2.priority == num2 && value2.value > num))
				{
					num = value2.value;
					num2 = value2.priority;
				}
			}
			rb.angularDrag = num;
		}
		else
		{
			rb.angularDrag = originalAngularDrag;
		}
	}

	private void ProcessDragOverrides()
	{
		if (!rb)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (string item in new List<string>(dragOverrides.Keys))
		{
			DragOverride value = dragOverrides[item];
			value.timeRemaining -= Time.deltaTime;
			if (value.timeRemaining <= 0f)
			{
				list.Add(item);
			}
			else
			{
				dragOverrides[item] = value;
			}
		}
		foreach (string item2 in list)
		{
			dragOverrides.Remove(item2);
		}
		if (dragOverrides.Count > 0)
		{
			float num = float.MinValue;
			float num2 = float.MinValue;
			foreach (DragOverride value2 in dragOverrides.Values)
			{
				if (value2.priority > num2 || (value2.priority == num2 && value2.value > num))
				{
					num = value2.value;
					num2 = value2.priority;
				}
			}
			rb.drag = num;
		}
		else
		{
			rb.drag = originalDrag;
		}
	}

	private void ProcessExcludeLayerOverrides()
	{
		if (!rb)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (string item in new List<string>(excludeLayerOverrides.Keys))
		{
			ExcludeLayerOverride value = excludeLayerOverrides[item];
			value.timeRemaining -= Time.deltaTime;
			if (value.timeRemaining <= 0f)
			{
				list.Add(item);
			}
			else
			{
				excludeLayerOverrides[item] = value;
			}
		}
		foreach (string item2 in list)
		{
			excludeLayerOverrides.Remove(item2);
		}
		if (excludeLayerOverrides.Count > 0)
		{
			int num = 0;
			foreach (ExcludeLayerOverride value2 in excludeLayerOverrides.Values)
			{
				num |= value2.layerMask;
			}
			rb.excludeLayers = num;
		}
		else
		{
			rb.excludeLayers = originalExcludeLayers;
		}
	}

	private void RestoreOriginalPhysicsValues()
	{
		if ((bool)rb)
		{
			rb.useGravity = originalUseGravity;
			rb.angularDrag = originalAngularDrag;
			rb.drag = originalDrag;
			rb.excludeLayers = originalExcludeLayers;
		}
	}
}
