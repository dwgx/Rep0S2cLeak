using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PhysGrabHinge : MonoBehaviour
{
	public enum BounceEffect
	{
		Light,
		Medium,
		Heavy
	}

	private PhotonView photon;

	public HingeAudio hingeAudio;

	public AudioSource audioSource;

	[Space]
	public Transform hingePoint;

	private Rigidbody hingePointRb;

	private bool hingePointHasRb;

	public float hingeOffsetPositiveThreshold = 15f;

	public float hingeOffsetNegativeThreshold = -15f;

	public float hingeOffsetSpeed = 5f;

	public Vector3 hingeOffsetPositive;

	public Vector3 hingeOffsetNegative;

	private Vector3 hingePointPosition;

	[Space]
	public float hingeBreakShake = 3f;

	[Space]
	public float closeThreshold = 10f;

	public float closeMaxSpeed = 1f;

	public float closeHeavySpeed = 5f;

	public float closeShake = 3f;

	private bool closeHeavy;

	private float closeSpeed;

	internal bool closed = true;

	private float closedForceTimer;

	private bool closing;

	private float closeDisableTimer;

	[Space]
	private float openForceNeeded = 0.04f;

	public float openHeavyThreshold = 3f;

	public float openShake = 3f;

	internal HingeJoint joint;

	private PhysGrabObject physGrabObject;

	private PhysGrabObjectImpactDetector impactDetector;

	private bool moveLoopActive;

	private float moveLoopEndDisableTimer;

	[HideInInspector]
	public Sound moveLoop;

	private Vector3 restPosition;

	private Quaternion restRotation;

	internal bool dead;

	private float deadTimer = 0.1f;

	internal bool broken;

	internal float brokenTimer;

	[Space]
	public float drag;

	[Space]
	public float bounceAmount = 0.2f;

	public BounceEffect bounceEffect = BounceEffect.Medium;

	private Vector3 bounceVelocity;

	private float bounceCooldown;

	[Space]
	public PhysGrabHinge[] wallTagHinges;

	public GameObject[] wallTagObjects;

	public LowPassTrigger[] lowPassTriggers;

	private float investigateDelay;

	private float investigateRadius;

	private bool fadeOutFast;

	private void Awake()
	{
		photon = GetComponent<PhotonView>();
		Sound.CopySound(hingeAudio.moveLoop, moveLoop);
		moveLoop.Source = audioSource;
		joint = GetComponent<HingeJoint>();
		physGrabObject = GetComponent<PhysGrabObject>();
		impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		impactDetector.particleDisable = true;
		joint.anchor = hingePoint.localPosition;
		hingePointRb = hingePoint.GetComponent<Rigidbody>();
		if ((bool)hingePointRb)
		{
			hingePointHasRb = true;
			hingePointPosition = hingePoint.position;
		}
		if (SemiFunc.IsMultiplayer() && SemiFunc.IsNotMasterClient())
		{
			Object.Destroy(joint);
			joint = null;
			hingePointHasRb = false;
		}
		restPosition = base.transform.position;
		restRotation = base.transform.rotation;
		StartCoroutine(RigidBodyGet());
		base.gameObject.layer = LayerMask.NameToLayer("PhysGrabObjectHinge");
		foreach (Transform item in base.transform)
		{
			item.gameObject.layer = LayerMask.NameToLayer("PhysGrabObjectHinge");
		}
	}

	private IEnumerator RigidBodyGet()
	{
		while (!physGrabObject.spawned)
		{
			yield return new WaitForSeconds(0.1f);
		}
		hingePoint.transform.parent = base.transform.parent;
		WallTagSet();
	}

	private void OnCollisionStay(Collision other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			closeDisableTimer = 0.1f;
		}
		else if (closing && other.gameObject.CompareTag("Phys Grab Object"))
		{
			closing = false;
		}
	}

	private void OnJointBreak(float breakForce)
	{
		if (GameManager.instance.gameMode == 0 || PhotonNetwork.IsMasterClient)
		{
			physGrabObject.rb.AddForce(-physGrabObject.rb.velocity * 2f, ForceMode.Impulse);
			physGrabObject.rb.AddTorque(-physGrabObject.rb.angularVelocity * 10f, ForceMode.Impulse);
			HingeBreakImpulse();
			broken = true;
		}
	}

	private void FixedUpdate()
	{
		if (broken)
		{
			brokenTimer += Time.fixedDeltaTime;
		}
		if (dead || broken || !physGrabObject.spawned || (GameManager.instance.gameMode != 0 && !PhotonNetwork.IsMasterClient))
		{
			return;
		}
		if (GameManager.Multiplayer())
		{
			physGrabObject.photonTransformView.KinematicClientForce(0.1f);
		}
		if (hingePointHasRb)
		{
			if (joint.angle >= hingeOffsetPositiveThreshold)
			{
				Vector3 vector = hingePointPosition + hingePoint.TransformDirection(hingeOffsetPositive);
				Vector3 vector2 = Vector3.Lerp(hingePointRb.transform.position, vector, hingeOffsetSpeed * Time.fixedDeltaTime);
				if (hingePointRb.position != vector2)
				{
					hingePointRb.MovePosition(vector2);
				}
			}
			else if (joint.angle <= hingeOffsetNegativeThreshold)
			{
				Vector3 vector3 = hingePointPosition + hingePoint.TransformDirection(hingeOffsetNegative);
				Vector3 vector4 = Vector3.Lerp(hingePointRb.transform.position, vector3, hingeOffsetSpeed * Time.fixedDeltaTime);
				if (hingePointRb.position != vector4)
				{
					hingePointRb.MovePosition(vector4);
				}
			}
			else
			{
				Vector3 vector5 = Vector3.Lerp(hingePointRb.transform.position, hingePointPosition, hingeOffsetSpeed * Time.fixedDeltaTime);
				if (closed)
				{
					vector5 = hingePointPosition;
				}
				if (hingePointRb.position != vector5)
				{
					hingePointRb.MovePosition(vector5);
				}
			}
		}
		if (!closed && closeDisableTimer <= 0f && (bool)joint)
		{
			if (!closing)
			{
				float num = Vector3.Dot(physGrabObject.rb.angularVelocity.normalized, (-joint.axis * joint.angle).normalized);
				if (physGrabObject.rb.angularVelocity.magnitude < closeMaxSpeed && Mathf.Abs(joint.angle) < closeThreshold && (num > 0f || physGrabObject.rb.angularVelocity.magnitude < 0.1f))
				{
					closeHeavy = false;
					closeSpeed = Mathf.Max(physGrabObject.rb.angularVelocity.magnitude, 0.2f);
					if (closeSpeed > closeHeavySpeed)
					{
						closeHeavy = true;
					}
					closing = true;
				}
			}
			else if (physGrabObject.playerGrabbing.Count > 0)
			{
				closing = false;
			}
			else
			{
				Vector3 vector6 = restRotation.eulerAngles - physGrabObject.rb.rotation.eulerAngles;
				vector6 = Vector3.ClampMagnitude(vector6, closeSpeed);
				physGrabObject.rb.AddRelativeTorque(vector6, ForceMode.Acceleration);
				if (Mathf.Abs(joint.angle) < 2f)
				{
					closedForceTimer = 0.25f;
					closing = false;
					CloseImpulse(closeHeavy);
				}
			}
		}
		if (physGrabObject.playerGrabbing.Count > 0)
		{
			closeDisableTimer = 0.1f;
		}
		else if (closeDisableTimer > 0f)
		{
			closeDisableTimer -= 1f * Time.fixedDeltaTime;
		}
		if (closed)
		{
			if (closedForceTimer > 0f)
			{
				closedForceTimer -= 1f * Time.fixedDeltaTime;
			}
			else if (physGrabObject.rb.angularVelocity.magnitude > openForceNeeded)
			{
				OpenImpulse();
				closeDisableTimer = 2f;
				closing = false;
			}
			if (closed && !physGrabObject.rb.isKinematic && (physGrabObject.rb.position != restPosition || physGrabObject.rb.rotation != restRotation))
			{
				physGrabObject.rb.MovePosition(restPosition);
				physGrabObject.rb.MoveRotation(restRotation);
				physGrabObject.rb.angularVelocity = Vector3.zero;
				physGrabObject.rb.velocity = Vector3.zero;
			}
		}
		if (physGrabObject.playerGrabbing.Count <= 0 && !closing && !closed)
		{
			Vector3 angularVelocity = physGrabObject.rb.angularVelocity;
			if (angularVelocity.magnitude <= 0.1f && bounceVelocity.magnitude > 0.5f && bounceCooldown <= 0f)
			{
				bounceCooldown = 1f;
				physGrabObject.rb.AddTorque(bounceAmount * -bounceVelocity.normalized, ForceMode.Impulse);
				if (bounceEffect == BounceEffect.Heavy)
				{
					physGrabObject.heavyImpactImpulse = true;
				}
				else if (bounceEffect == BounceEffect.Medium)
				{
					physGrabObject.mediumImpactImpulse = true;
				}
				else
				{
					physGrabObject.lightImpactImpulse = true;
				}
				moveLoopEndDisableTimer = 1f;
			}
			bounceVelocity = angularVelocity;
		}
		else
		{
			bounceVelocity = Vector3.zero;
		}
		if (bounceCooldown > 0f)
		{
			bounceCooldown -= 1f * Time.fixedDeltaTime;
		}
		if (!closing)
		{
			physGrabObject.OverrideDrag(drag);
			physGrabObject.OverrideAngularDrag(drag);
		}
	}

	private void Update()
	{
		if (dead)
		{
			deadTimer -= 1f * Time.deltaTime;
			if (deadTimer <= 0f)
			{
				impactDetector.DestroyObject();
			}
			return;
		}
		if (broken)
		{
			moveLoop.PlayLoop(playing: false, 1f, 1f);
			return;
		}
		if (hingeAudio.moveLoopEnabled)
		{
			if (physGrabObject.rbVelocity.magnitude > hingeAudio.moveLoopThreshold)
			{
				if (!moveLoopActive)
				{
					fadeOutFast = false;
					moveLoopActive = true;
				}
				moveLoop.PlayLoop(playing: true, hingeAudio.moveLoopFadeInSpeed, hingeAudio.moveLoopFadeOutSpeed);
				moveLoop.LoopPitch = Mathf.Max(moveLoop.Pitch + physGrabObject.rbVelocity.magnitude * hingeAudio.moveLoopVelocityMult, 0.1f);
			}
			else
			{
				if (moveLoopActive)
				{
					if (moveLoopEndDisableTimer <= 0f)
					{
						hingeAudio.moveLoopEnd.Play(moveLoop.Source.transform.position);
						moveLoopEndDisableTimer = 3f;
					}
					moveLoopActive = false;
				}
				if (fadeOutFast)
				{
					moveLoop.PlayLoop(playing: false, hingeAudio.moveLoopFadeInSpeed, 20f);
				}
				else
				{
					moveLoop.PlayLoop(playing: false, hingeAudio.moveLoopFadeInSpeed, hingeAudio.moveLoopFadeOutSpeed);
				}
				moveLoopEndDisableTimer = 0.5f;
			}
			if (moveLoopEndDisableTimer > 0f)
			{
				moveLoopEndDisableTimer -= 1f * Time.deltaTime;
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && investigateDelay > 0f)
		{
			investigateDelay -= 1f * Time.deltaTime;
			if (investigateDelay <= 0f && physGrabObject.enemyInteractTimer <= 0f)
			{
				EnemyDirector.instance.SetInvestigate(physGrabObject.midPoint, investigateRadius);
			}
		}
	}

	private void WallTagSet()
	{
		string text = "Untagged";
		if (closed && !broken && !dead)
		{
			text = "Wall";
		}
		if (text == "Wall" && wallTagHinges.Length != 0)
		{
			PhysGrabHinge[] array = wallTagHinges;
			foreach (PhysGrabHinge physGrabHinge in array)
			{
				if (!physGrabHinge || !physGrabHinge.closed)
				{
					return;
				}
			}
		}
		if (wallTagObjects.Length != 0)
		{
			GameObject[] array2 = wallTagObjects;
			foreach (GameObject gameObject in array2)
			{
				if ((bool)gameObject)
				{
					gameObject.tag = text;
				}
			}
		}
		if (lowPassTriggers.Length == 0)
		{
			return;
		}
		LowPassTrigger[] array3 = lowPassTriggers;
		foreach (LowPassTrigger lowPassTrigger in array3)
		{
			if ((bool)lowPassTrigger)
			{
				if (text == "Wall")
				{
					lowPassTrigger.gameObject.SetActive(value: true);
				}
				else
				{
					lowPassTrigger.gameObject.SetActive(value: false);
				}
			}
		}
	}

	private void EnemyInvestigate(float radius)
	{
		investigateDelay = 0.1f;
		investigateRadius = radius;
	}

	private void CloseImpulse(bool heavy)
	{
		EnemyInvestigate(1f);
		if (GameManager.instance.gameMode == 0)
		{
			CloseImpulseRPC(heavy);
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photon.RPC("CloseImpulseRPC", RpcTarget.All, heavy);
		}
	}

	[PunRPC]
	private void CloseImpulseRPC(bool heavy)
	{
		fadeOutFast = true;
		GameDirector.instance.CameraImpact.ShakeDistance(closeShake * 0.5f, 3f, 10f, base.transform.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(closeShake, 3f, 10f, base.transform.position, 0.1f);
		if (heavy)
		{
			hingeAudio.CloseHeavy.Play(audioSource.transform.position);
		}
		else
		{
			hingeAudio.Close.Play(audioSource.transform.position);
		}
		moveLoopEndDisableTimer = 1f;
		closed = true;
		WallTagSet();
	}

	private void OpenImpulse()
	{
		EnemyInvestigate(0.5f);
		if (GameManager.instance.gameMode == 0)
		{
			OpenImpulseRPC();
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photon.RPC("OpenImpulseRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void OpenImpulseRPC()
	{
		GameDirector.instance.CameraImpact.ShakeDistance(openShake * 0.5f, 3f, 10f, base.transform.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(openShake, 3f, 10f, base.transform.position, 0.1f);
		if (physGrabObject.rbAngularVelocity.magnitude > openHeavyThreshold)
		{
			hingeAudio.OpenHeavy.Play(audioSource.transform.position);
		}
		else
		{
			hingeAudio.Open.Play(audioSource.transform.position);
		}
		closed = false;
		WallTagSet();
	}

	private void HingeBreakImpulse()
	{
		if (GameManager.instance.gameMode == 0)
		{
			HingeBreakRPC();
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photon.RPC("HingeBreakRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void HingeBreakRPC()
	{
		GameDirector.instance.CameraImpact.ShakeDistance(hingeBreakShake * 0.5f, 3f, 10f, base.transform.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(hingeBreakShake, 3f, 10f, base.transform.position, 0.1f);
		hingeAudio.HingeBreak.Play(audioSource.transform.position);
		physGrabObject.heavyBreakImpulse = true;
		impactDetector.isHinge = false;
		impactDetector.isBrokenHinge = true;
		impactDetector.particleDisable = false;
		broken = true;
		WallTagSet();
		int layer = LayerMask.NameToLayer("PhysGrabObject");
		base.gameObject.layer = layer;
		foreach (Transform item in base.transform)
		{
			item.gameObject.layer = layer;
		}
	}

	public void DestroyHinge()
	{
		if (GameManager.instance.gameMode == 0)
		{
			DestroyHingeRPC();
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photon.RPC("DestroyHingeRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void DestroyHingeRPC()
	{
		dead = true;
		WallTagSet();
	}
}
