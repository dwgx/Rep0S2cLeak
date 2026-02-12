using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ItemGrenade : MonoBehaviour
{
	public Color blinkColor;

	public UnityEvent onDetonate;

	private ItemToggle itemToggle;

	private ItemAttributes itemAttributes;

	internal bool isActive;

	private float grenadeTimer;

	public float tickTime = 3f;

	private PhotonView photonView;

	private PhysGrabObjectImpactDetector physGrabObjectImpactDetector;

	public Sound soundSplinter;

	public Sound soundTick;

	private float splinterAnimationProgress;

	public AnimationCurve splinterAnimationCurve;

	private Transform splinterTransform;

	private Material grenadeEmissionMaterial;

	private ItemEquippable itemEquippable;

	private Vector3 grenadeStartPosition;

	private Quaternion grenadeStartRotation;

	private PhysGrabObject physGrabObject;

	private Vector3 prevPosition;

	[FormerlySerializedAs("isThiefGrenade")]
	[HideInInspector]
	public bool isSpawnedGrenade;

	public GameObject throwLine;

	private Rigidbody rb;

	private float throwLineTimer;

	private TrailRenderer throwLineTrail;

	private void Start()
	{
		itemEquippable = GetComponent<ItemEquippable>();
		itemToggle = GetComponent<ItemToggle>();
		itemAttributes = GetComponent<ItemAttributes>();
		photonView = GetComponent<PhotonView>();
		physGrabObjectImpactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		splinterTransform = base.transform.Find("Splinter");
		GameObject gameObject = base.transform.Find("Mesh").gameObject;
		grenadeEmissionMaterial = gameObject.GetComponent<Renderer>().material;
		grenadeStartPosition = base.transform.position;
		grenadeStartRotation = base.transform.rotation;
		physGrabObject = GetComponent<PhysGrabObject>();
		rb = GetComponent<Rigidbody>();
		throwLineTrail = throwLine.GetComponent<TrailRenderer>();
	}

	private void FixedUpdate()
	{
		if (itemEquippable.isEquipped || itemEquippable.wasEquippedTimer > 0f)
		{
			prevPosition = rb.position;
			return;
		}
		Vector3 vector = (rb.position - prevPosition) / Time.fixedDeltaTime;
		_ = (rb.position - prevPosition).normalized;
		prevPosition = rb.position;
		if (!physGrabObject.grabbed && vector.magnitude > 2f)
		{
			throwLineTimer = 0.2f;
		}
		if (throwLineTimer > 0f)
		{
			throwLineTrail.emitting = true;
			throwLineTimer -= Time.fixedDeltaTime;
		}
		else
		{
			throwLineTrail.emitting = false;
		}
	}

	private void Update()
	{
		soundTick.PlayLoop(isActive, 2f, 2f);
		if (itemEquippable.isEquipped)
		{
			if (isActive)
			{
				isActive = false;
				grenadeTimer = 0f;
				splinterAnimationProgress = 0f;
				itemToggle.ToggleItem(toggle: false);
				splinterTransform.localEulerAngles = new Vector3(0f, 0f, 0f);
				grenadeEmissionMaterial.SetColor("_EmissionColor", Color.black);
			}
			return;
		}
		if (isActive)
		{
			if (splinterAnimationProgress < 1f)
			{
				splinterAnimationProgress += 5f * Time.deltaTime;
				float num = splinterAnimationCurve.Evaluate(splinterAnimationProgress);
				splinterTransform.localEulerAngles = new Vector3(num * 90f, 0f, 0f);
			}
			float value = Mathf.PingPong(Time.time * 8f, 1f);
			Color value2 = blinkColor * Mathf.LinearToGammaSpace(value);
			grenadeEmissionMaterial.SetColor("_EmissionColor", value2);
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (itemToggle.toggleState && !isActive)
		{
			isActive = true;
			TickStart();
		}
		if (isActive)
		{
			grenadeTimer += Time.deltaTime;
			if (grenadeTimer >= tickTime)
			{
				grenadeTimer = 0f;
				TickEnd();
			}
		}
	}

	private void GrenadeReset()
	{
		isActive = false;
		grenadeTimer = 0f;
		throwLine.SetActive(value: false);
		splinterAnimationProgress = 0f;
		itemToggle.ToggleItem(toggle: false);
		splinterTransform.localEulerAngles = new Vector3(0f, 0f, 0f);
		grenadeEmissionMaterial.SetColor("_EmissionColor", Color.black);
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		foreach (PhysGrabber item in physGrabObject.playerGrabbing.ToList())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				item.ReleaseObject(photonView.ViewID);
				continue;
			}
			item.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, 0.1f, photonView.ViewID);
		}
	}

	private void TickStart()
	{
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("TickStartRPC", RpcTarget.All);
		}
		else
		{
			TickStartRPC();
		}
	}

	private void TickEnd()
	{
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("TickEndRPC", RpcTarget.All);
		}
		else
		{
			TickEndRPC();
		}
	}

	[PunRPC]
	private void TickStartRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			soundSplinter.Play(base.transform.position);
			isActive = true;
		}
	}

	[PunRPC]
	private void TickEndRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info) || itemEquippable.isEquipped)
		{
			return;
		}
		onDetonate.Invoke();
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!SemiFunc.RunIsShop() || isSpawnedGrenade)
			{
				if (!isSpawnedGrenade)
				{
					StatsManager.instance.ItemRemove(itemAttributes.instanceName);
				}
				physGrabObjectImpactDetector.DestroyObject();
			}
			else
			{
				physGrabObject.Teleport(grenadeStartPosition, grenadeStartRotation);
			}
		}
		if (SemiFunc.RunIsShop() && !isSpawnedGrenade)
		{
			GrenadeReset();
		}
	}
}
