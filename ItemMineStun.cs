using UnityEngine;

public class ItemMineStun : MonoBehaviour
{
	private ItemMine itemMine;

	private PhysGrabObject bitPhysGrabObject;

	private Transform bitTransform;

	private Vector3 startPosition;

	private bool triggered;

	public AnimationCurve jawAnimationCurve;

	private Rigidbody rb;

	private PhysGrabObject physGrabObject;

	public Transform jaw1Tranform;

	public Transform jaw2Tranform;

	private float jawEval;

	private float jaw1CurrentRot;

	private float jaw2CurrentRot;

	public GameObject hurtCollider;

	private bool bite;

	public ParticleSystem particleFlash;

	public ParticleSystem particleLightning;

	private bool chomp;

	public Sound soundChomp;

	public Sound soundElectricity;

	private Quaternion jaw1StartRot;

	private Quaternion jaw2StartRot;

	private void Start()
	{
		itemMine = GetComponent<ItemMine>();
		physGrabObject = GetComponent<PhysGrabObject>();
		rb = GetComponent<Rigidbody>();
		jaw1CurrentRot = jaw1Tranform.localRotation.eulerAngles.x;
		jaw2CurrentRot = jaw2Tranform.localRotation.eulerAngles.x;
		jaw2StartRot = jaw2Tranform.localRotation;
		jaw1StartRot = jaw1Tranform.localRotation;
	}

	private void Reset()
	{
		if (triggered)
		{
			chomp = false;
			bite = false;
			jawEval = 0f;
			jaw1Tranform.localRotation = jaw1StartRot;
			jaw2Tranform.localRotation = jaw2StartRot;
			jaw1CurrentRot = jaw1Tranform.localRotation.eulerAngles.x;
			jaw2CurrentRot = jaw2Tranform.localRotation.eulerAngles.x;
			triggered = false;
			hurtCollider.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (physGrabObject.grabbed && SemiFunc.IsMasterClientOrSingleplayer())
		{
			Quaternion turnX = Quaternion.Euler(0f, 0f, 0f);
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
		}
		if (itemMine.state == ItemMine.States.Disarmed)
		{
			Reset();
			if (jawEval > 0f)
			{
				jawEval -= Time.deltaTime * 2f;
				if (jawEval < 0f)
				{
					jawEval = 0f;
				}
				jaw1Tranform.localRotation = Quaternion.LerpUnclamped(Quaternion.Euler(jaw1CurrentRot, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), jawAnimationCurve.Evaluate(jawEval));
				jaw2Tranform.localRotation = Quaternion.LerpUnclamped(Quaternion.Euler(jaw2CurrentRot, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), jawAnimationCurve.Evaluate(jawEval));
			}
		}
		if (itemMine.state == ItemMine.States.Armed && jawEval < 1f)
		{
			jawEval += Time.deltaTime * 2f;
			if (jawEval > 1f)
			{
				jawEval = 1f;
			}
			jaw1Tranform.localRotation = Quaternion.LerpUnclamped(Quaternion.Euler(jaw1CurrentRot, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), jawAnimationCurve.Evaluate(jawEval));
			jaw2Tranform.localRotation = Quaternion.LerpUnclamped(Quaternion.Euler(jaw2CurrentRot, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), jawAnimationCurve.Evaluate(jawEval));
		}
		if (itemMine.state != ItemMine.States.Triggered)
		{
			return;
		}
		if (jawEval < 1f)
		{
			jawEval += Time.deltaTime * 2f;
			if (jawEval > 1f)
			{
				jawEval = 1f;
			}
			jaw1Tranform.localRotation = Quaternion.Euler(-90f * jawAnimationCurve.Evaluate(jawEval), 0f, 0f);
			jaw2Tranform.localRotation = Quaternion.Euler(90f * jawAnimationCurve.Evaluate(jawEval), 0f, 0f);
			return;
		}
		float num = Mathf.PingPong(Time.time * 5f, 1f);
		float num2 = jawAnimationCurve.Evaluate(num);
		if (num > 0.1f)
		{
			if (!chomp)
			{
				soundChomp.Play(base.transform.position);
			}
			chomp = true;
		}
		else
		{
			chomp = false;
		}
		if (num > 0.5f)
		{
			if (!bite)
			{
				ElectricityEffect();
			}
			bite = true;
		}
		else
		{
			bite = false;
		}
		jaw1Tranform.localRotation = Quaternion.Euler(-64f * num2, 0f, 0f);
		jaw2Tranform.localRotation = Quaternion.Euler(64f * num2, 0f, 0f);
	}

	private void ElectricityEffect()
	{
		soundElectricity.Play(base.transform.position);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 3f, 8f, base.transform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.1f);
		particleLightning.Play();
		particleFlash.Play();
		if (SemiFunc.IsMasterClientOrSingleplayer() && (bool)bitTransform && Vector3.Distance(base.transform.position, bitTransform.position) < 1.5f)
		{
			Vector3 insideUnitSphere = Random.insideUnitSphere;
			rb.AddTorque(insideUnitSphere * 1f, ForceMode.Impulse);
			Vector3 insideUnitSphere2 = Random.insideUnitSphere;
			rb.AddForce(insideUnitSphere2 * 1f, ForceMode.Impulse);
		}
	}

	private void FixedUpdate()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (itemMine.state == ItemMine.States.Armed && Vector3.Angle(base.transform.up, Vector3.up) > 65f)
		{
			Vector3 right = base.transform.right;
			rb.AddTorque(right * Time.fixedDeltaTime * 20f, ForceMode.Force);
		}
		if (!triggered)
		{
			return;
		}
		if (!itemMine.triggeredTransform)
		{
			Transform transform = SemiFunc.PlayerGetNearestTransformWithinRange(10f, base.transform.position, doRaycastCheck: true);
			if ((bool)transform)
			{
				itemMine.wasTriggeredByPlayer = true;
				itemMine.triggeredPlayerAvatar = transform.GetComponentInParent<PlayerAvatar>();
				bitTransform = transform;
			}
			else
			{
				Enemy enemy = SemiFunc.EnemyGetNearest(base.transform.position, 10f, _raycast: true);
				if ((bool)enemy)
				{
					itemMine.wasTriggeredByEnemy = true;
					bitPhysGrabObject = enemy.GetComponentInParent<PhysGrabObject>();
					bitTransform = enemy.CenterTransform;
				}
			}
		}
		if (itemMine.wasTriggeredByEnemy || itemMine.wasTriggeredByRigidBody)
		{
			if (!bitPhysGrabObject)
			{
				itemMine.DestroyMine();
				return;
			}
			if ((bool)bitPhysGrabObject && !bitPhysGrabObject.gameObject.activeInHierarchy)
			{
				itemMine.DestroyMine();
				return;
			}
		}
		if (itemMine.wasTriggeredByPlayer)
		{
			if (!itemMine.triggeredPlayerAvatar && !itemMine.triggeredPlayerTumble)
			{
				itemMine.DestroyMine();
				return;
			}
			if (itemMine.triggeredPlayerAvatar.isDisabled)
			{
				itemMine.DestroyMine();
				return;
			}
		}
		if (itemMine.wasTriggeredByPlayer)
		{
			if ((bool)bitTransform && (bool)itemMine.triggeredPlayerTumble && !itemMine.triggeredPlayerTumble.isActiveAndEnabled)
			{
				itemMine.DestroyMine();
				return;
			}
			if (!bitTransform)
			{
				if ((bool)itemMine.triggeredPlayerAvatar)
				{
					bitTransform = itemMine.triggeredPlayerAvatar.PlayerVisionTarget.VisionTransform;
				}
				if ((bool)itemMine.triggeredPlayerTumble)
				{
					bitTransform = itemMine.triggeredPlayerTumble.playerAvatar.PlayerVisionTarget.VisionTransform;
				}
			}
		}
		if (!bitTransform)
		{
			itemMine.DestroyMine();
		}
		else if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			Vector3 position = base.transform.position;
			if (itemMine.wasTriggeredByPlayer)
			{
				position = bitTransform.position;
			}
			if (itemMine.wasTriggeredByEnemy)
			{
				position = bitTransform.position;
			}
			Vector3 vector = bitTransform.position;
			if ((bool)bitPhysGrabObject)
			{
				vector = bitPhysGrabObject.midPoint;
			}
			Vector3 vector2 = position - new Vector3(vector.x, position.y, vector.z);
			physGrabObject.OverrideZeroGravity();
			Vector3 vector3 = SemiFunc.PhysFollowPosition(base.transform.position, position, rb.velocity, 10f);
			rb.AddForce(vector3 * Time.fixedDeltaTime, ForceMode.Impulse);
			if (vector2 != Vector3.zero)
			{
				Vector3 vector4 = SemiFunc.PhysFollowRotation(base.transform, Quaternion.LookRotation(vector2), rb, 20f);
				rb.AddTorque(vector4 * Time.fixedDeltaTime, ForceMode.Impulse);
			}
		}
	}

	public void OnTriggered()
	{
		ElectricityEffect();
		jawEval = 0f;
		triggered = true;
		bitTransform = itemMine.triggeredTransform;
		bitPhysGrabObject = itemMine.triggeredPhysGrabObject;
		hurtCollider.SetActive(value: true);
	}
}
