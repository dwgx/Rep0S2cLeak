using System.Collections;
using UnityEngine;

public class EnemyGrounded : MonoBehaviour
{
	public Enemy enemy;

	internal bool grounded;

	public BoxCollider boxCollider;

	private bool logicActive;

	private float groundedDisableTimer;

	internal Vector3 colliderSizeOriginal;

	private float colliderIncreaseAmount = 1f;

	private float colliderIncreaseTimer;

	private void Awake()
	{
		enemy.Grounded = this;
		enemy.HasGrounded = true;
		if (!boxCollider.isTrigger)
		{
			Debug.LogError("EnemyGrounded: Collider is not a trigger on " + enemy.EnemyParent.name);
		}
		if (boxCollider.transform.localScale != Vector3.one)
		{
			Debug.LogError("EnemyGrounded: Scale is not 1 on " + enemy.EnemyParent.name);
		}
		if (boxCollider.transform.localPosition != Vector3.zero)
		{
			Debug.LogError("EnemyGrounded: Position is not 0 on " + enemy.EnemyParent.name);
		}
		colliderSizeOriginal = boxCollider.size;
		StartCoroutine(ColliderCheck());
	}

	private void Update()
	{
		if (!grounded)
		{
			if (enemy.Rigidbody.velocity.magnitude < 0.1f)
			{
				colliderIncreaseTimer += Time.deltaTime;
			}
			else
			{
				colliderIncreaseTimer = 0f;
			}
			if (colliderIncreaseTimer >= 0.5f)
			{
				colliderIncreaseAmount += 0.5f * Time.deltaTime;
			}
		}
		if (enemy.Rigidbody.rb.velocity.magnitude > 0.1f)
		{
			colliderIncreaseAmount = 1f;
		}
		boxCollider.size = colliderSizeOriginal * colliderIncreaseAmount;
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		logicActive = false;
	}

	private void OnEnable()
	{
		if (!logicActive)
		{
			StartCoroutine(ColliderCheck());
		}
	}

	private IEnumerator ColliderCheck()
	{
		logicActive = true;
		yield return new WaitForSeconds(0.1f);
		while (true)
		{
			grounded = false;
			Vector3 halfExtents = boxCollider.transform.TransformVector(boxCollider.size * 0.5f);
			halfExtents.x = Mathf.Abs(halfExtents.x);
			halfExtents.y = Mathf.Abs(halfExtents.y);
			halfExtents.z = Mathf.Abs(halfExtents.z);
			Collider[] array = Physics.OverlapBox(boxCollider.bounds.center, halfExtents, boxCollider.transform.rotation, LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectHinge", "PhysGrabObjectCart"), QueryTriggerInteraction.Ignore);
			if (array.Length != 0)
			{
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					if ((bool)collider.GetComponentInParent<EnemyRigidbody>())
					{
						continue;
					}
					if (enemy.HasJump && enemy.Jump.surfaceJump)
					{
						EnemyJumpSurface component = collider.GetComponent<EnemyJumpSurface>();
						if ((bool)component)
						{
							Vector3 rhs = enemy.transform.forward;
							if (enemy.HasRigidbody)
							{
								rhs = enemy.transform.position - enemy.Rigidbody.transform.position;
							}
							if (Vector3.Dot(component.transform.TransformDirection(component.jumpDirection), rhs) > 0.5f)
							{
								enemy.Jump.SurfaceJumpTrigger(component.transform.TransformDirection(component.jumpDirection));
							}
						}
					}
					if (groundedDisableTimer <= 0f)
					{
						grounded = true;
					}
				}
			}
			if (enemy.HasJump && (bool)enemy.Jump)
			{
				groundedDisableTimer -= 0.05f;
				yield return new WaitForSeconds(0.05f);
			}
			else
			{
				groundedDisableTimer -= 0.25f;
				yield return new WaitForSeconds(0.25f);
			}
		}
	}

	public void GroundedDisable(float _time)
	{
		groundedDisableTimer = _time;
	}
}
