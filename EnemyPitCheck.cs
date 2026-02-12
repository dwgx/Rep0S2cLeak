using System.Collections.Generic;
using UnityEngine;

public class EnemyPitCheck : MonoBehaviour
{
	private class PitCheckResult
	{
		public Transform transform;

		public bool hasGround;

		public bool pitByTrigger;

		public Vector3 groundHitPoint;

		public Vector3 triggerHitPoint;

		public bool groundHit;

		public bool triggerHit;
	}

	public List<Transform> pitCheckTransforms;

	public float pitCheckDistance = 2f;

	public bool debugGizmos;

	public bool isOverPit;

	public bool transformDirection = true;

	private float checkTimer;

	private List<PitCheckResult> debugResults = new List<PitCheckResult>();

	private void Awake()
	{
		if (!Application.isEditor)
		{
			debugGizmos = false;
		}
	}

	private void Update()
	{
		if (checkTimer <= 0f)
		{
			isOverPit = false;
		}
		if (checkTimer > 0f)
		{
			checkTimer -= Time.deltaTime;
			if (SemiFunc.FPSImpulse5())
			{
				PerformPitCheck();
			}
		}
	}

	public void CheckPit()
	{
		if (checkTimer <= 0f)
		{
			checkTimer = 0.2f;
			PerformPitCheck();
		}
	}

	private void PerformPitCheck()
	{
		if (pitCheckTransforms == null || pitCheckTransforms.Count == 0)
		{
			isOverPit = false;
			debugResults.Clear();
			return;
		}
		debugResults.Clear();
		bool flag = false;
		int mask = LayerMask.GetMask("Default");
		int mask2 = LayerMask.GetMask("Triggers");
		foreach (Transform pitCheckTransform in pitCheckTransforms)
		{
			if (!pitCheckTransform)
			{
				continue;
			}
			Vector3 position = pitCheckTransform.position;
			Vector3 vector = Vector3.down;
			if (transformDirection)
			{
				vector = -pitCheckTransform.up;
			}
			RaycastHit hitInfo;
			bool flag2 = Physics.Raycast(position, vector, out hitInfo, pitCheckDistance, mask, QueryTriggerInteraction.Ignore);
			RaycastHit hitInfo2;
			bool flag3 = Physics.Raycast(position, vector, out hitInfo2, pitCheckDistance, mask2, QueryTriggerInteraction.Collide);
			bool flag4 = false;
			if (flag3)
			{
				HurtCollider componentInParent = hitInfo2.collider.GetComponentInParent<HurtCollider>();
				if ((bool)componentInParent && componentInParent.deathPit)
				{
					flag4 = true;
				}
			}
			if (flag4 || !flag2)
			{
				flag = true;
			}
			PitCheckResult pitCheckResult = new PitCheckResult
			{
				transform = pitCheckTransform,
				hasGround = flag2,
				pitByTrigger = flag4,
				groundHit = flag2,
				triggerHit = flag3
			};
			if (flag2)
			{
				pitCheckResult.groundHitPoint = hitInfo.point;
			}
			if (flag3)
			{
				pitCheckResult.triggerHitPoint = hitInfo2.point;
			}
			debugResults.Add(pitCheckResult);
			if (!debugGizmos)
			{
				continue;
			}
			if (flag2)
			{
				Debug.Log($"[{pitCheckTransform.name}] Ground Hit: {hitInfo.collider.name} on layer {LayerMask.LayerToName(hitInfo.collider.gameObject.layer)} at distance {hitInfo.distance}");
				continue;
			}
			RaycastHit hitInfo3;
			bool num = Physics.Raycast(position, vector, out hitInfo3, pitCheckDistance, -1, QueryTriggerInteraction.Ignore);
			RaycastHit hitInfo4;
			bool flag5 = Physics.Raycast(position, vector, out hitInfo4, pitCheckDistance, -1, QueryTriggerInteraction.Collide);
			if (num)
			{
				Debug.Log($"[{pitCheckTransform.name}] NO Default ground found. But hit SOLID: {hitInfo3.collider.name} on layer {LayerMask.LayerToName(hitInfo3.collider.gameObject.layer)} at distance {hitInfo3.distance}");
			}
			else if (flag5)
			{
				Debug.Log($"[{pitCheckTransform.name}] NO Default ground found. But hit TRIGGER: {hitInfo4.collider.name} (isTrigger={hitInfo4.collider.isTrigger}) on layer {LayerMask.LayerToName(hitInfo4.collider.gameObject.layer)} at distance {hitInfo4.distance}");
			}
			else
			{
				Debug.Log($"[{pitCheckTransform.name}] NO ground found at all (solid or trigger). Nothing within {pitCheckDistance} units. Origin: {position}, Dir: {vector}. Ground Y position might be below {position.y - pitCheckDistance}");
			}
		}
		isOverPit = flag;
	}
}
