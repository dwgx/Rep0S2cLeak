using System.Collections;
using UnityEngine;

public class PlayerCollisionGrounded : MonoBehaviour
{
	public static PlayerCollisionGrounded instance;

	public PlayerCollisionController CollisionController;

	internal bool Grounded;

	private float GroundedTimer;

	public LayerMask LayerMask;

	private SphereCollider Collider;

	private float ColliderRadiusDefault;

	private float ColliderRadiusScaleTimer;

	[HideInInspector]
	public bool physRiding;

	[HideInInspector]
	public int physRidingID;

	[HideInInspector]
	public Vector3 physRidingPosition;

	private bool colliderCheckActive;

	internal bool onPhysObject;

	private void Awake()
	{
		instance = this;
		Collider = GetComponent<SphereCollider>();
		ColliderRadiusDefault = Collider.radius;
	}

	private void Start()
	{
		ColliderCheckActivate();
	}

	private void OnEnable()
	{
		ColliderCheckActivate();
	}

	private void OnDisable()
	{
		colliderCheckActive = false;
		StopAllCoroutines();
	}

	private void ColliderCheckActivate()
	{
		if (!colliderCheckActive)
		{
			colliderCheckActive = true;
			StartCoroutine(ColliderCheck());
		}
	}

	private IEnumerator ColliderCheck()
	{
		while (true)
		{
			GroundedTimer -= 1f * Time.deltaTime;
			physRiding = false;
			onPhysObject = false;
			if (CollisionController.GroundedDisableTimer <= 0f)
			{
				Collider[] array = Physics.OverlapSphere(base.transform.position, Collider.radius, LayerMask, QueryTriggerInteraction.Ignore);
				if (array.Length != 0)
				{
					int num = 0;
					if (LevelGenerator.Instance.Generated)
					{
						Collider[] array2 = array;
						foreach (Collider collider in array2)
						{
							if (!collider.gameObject.CompareTag("Phys Grab Object"))
							{
								continue;
							}
							onPhysObject = true;
							PhysGrabObject physGrabObject = collider.gameObject.GetComponent<PhysGrabObject>();
							if (!physGrabObject)
							{
								physGrabObject = collider.gameObject.GetComponentInParent<PhysGrabObject>();
							}
							if ((bool)physGrabObject && !physGrabObject.physRidingDisabled)
							{
								if (!PlayerController.instance.JumpGroundedObjects.Contains(physGrabObject))
								{
									PlayerController.instance.JumpGroundedObjects.Add(physGrabObject);
								}
								if ((bool)physGrabObject.GetComponent<PlayerTumble>())
								{
									num++;
								}
								else if (physGrabObject.roomVolumeCheck.currentSize.magnitude > 1f)
								{
									physRiding = true;
									physRidingID = physGrabObject.photonView.ViewID;
									physRidingPosition = physGrabObject.photonView.transform.InverseTransformPoint(PlayerController.instance.transform.position);
								}
							}
						}
					}
					if (num != array.Length)
					{
						GroundedTimer = 0.1f;
						Grounded = true;
					}
				}
			}
			if (GroundedTimer < 0f)
			{
				Grounded = false;
			}
			yield return null;
		}
	}

	private void Update()
	{
		CollisionController.Grounded = Grounded;
		if (!Grounded)
		{
			if (PlayerController.instance.rb.velocity.magnitude < 0.1f)
			{
				ColliderRadiusScaleTimer += Time.deltaTime;
			}
			else
			{
				ColliderRadiusScaleTimer = 0f;
			}
			if (ColliderRadiusScaleTimer >= 0.5f)
			{
				Collider.radius += 0.05f * Time.deltaTime;
			}
		}
		if (PlayerController.instance.rb.velocity.magnitude > 0.1f)
		{
			Collider.radius = ColliderRadiusDefault;
		}
	}
}
