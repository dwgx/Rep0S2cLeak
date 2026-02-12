using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class ItemMine : MonoBehaviour
{
	public enum MineType
	{
		None,
		Explosive,
		Shockwave,
		Stun
	}

	public enum States
	{
		Disarmed,
		Arming,
		Armed,
		Disarming,
		Triggering,
		Triggered
	}

	public MineType mineType;

	public Color emissionColor;

	public UnityEvent onTriggered;

	public float armingTime;

	public float triggeringTime;

	private SpringQuaternion triggerSpringQuaternion;

	private Quaternion triggerTargetRotation;

	private bool upsideDown;

	public Transform triggerTransform;

	public LineRenderer triggerLine;

	public ParticleSystem lineParticles;

	private float beepTimer;

	private float checkTimer;

	private ItemMineTrigger itemMineTrigger;

	private ItemEquippable itemEquippable;

	private ItemAttributes itemAttributes;

	private ItemToggle itemToggle;

	[Space(20f)]
	private PhotonView photonView;

	private PhysGrabObject physGrabObject;

	public MeshRenderer meshRenderer;

	public Light lightArmed;

	[Space(20f)]
	public Sound soundArmingBeep;

	public Sound soundArmedBeep;

	public Sound soundDisarmingBeep;

	public Sound soundDisarmedBeep;

	public Sound soundTriggereringBeep;

	private float initialLightIntensity;

	private ParticleScriptExplosion particleScriptExplosion;

	private bool hasBeenGrabbed;

	private Vector3 startPosition;

	private Quaternion startRotation;

	internal Vector3 triggeredPosition;

	internal Transform triggeredTransform;

	internal PlayerAvatar triggeredPlayerAvatar;

	internal PlayerTumble triggeredPlayerTumble;

	internal PhysGrabObject triggeredPhysGrabObject;

	public bool triggeredByRigidBodies = true;

	public bool triggeredByEnemies = true;

	public bool triggeredByPlayers = true;

	public bool triggeredByForces = true;

	public bool destroyAfterTimer;

	public float destroyTimer = 10f;

	internal bool wasTriggeredByEnemy;

	internal bool wasTriggeredByPlayer;

	internal bool wasTriggeredByForce;

	internal bool wasTriggeredByRigidBody;

	internal bool firstLight = true;

	private bool firstLightDone;

	private float secondArmedTimer;

	private bool wasGrabbed;

	private float targetLineLength = 1f;

	private Vector3 prevPos = Vector3.zero;

	private Quaternion prevRot = Quaternion.identity;

	internal States state;

	private bool stateStart = true;

	private float stateTimer;

	private PhysGrabObjectImpactDetector impactDetector;

	private bool mineDestroyed;

	private void Start()
	{
		triggerSpringQuaternion = new SpringQuaternion();
		triggerSpringQuaternion.damping = 0.2f;
		triggerSpringQuaternion.speed = 10f;
		itemAttributes = GetComponent<ItemAttributes>();
		physGrabObject = GetComponent<PhysGrabObject>();
		photonView = GetComponent<PhotonView>();
		lightArmed.color = emissionColor;
		meshRenderer.material.SetColor("_EmissionColor", emissionColor);
		initialLightIntensity = lightArmed.intensity;
		impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		itemMineTrigger = GetComponentInChildren<ItemMineTrigger>();
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		startPosition = base.transform.position;
		itemEquippable = GetComponent<ItemEquippable>();
		startRotation = base.transform.rotation;
		itemToggle = GetComponent<ItemToggle>();
	}

	private void StateDisarmed()
	{
		if (stateStart)
		{
			soundDisarmedBeep.Play(base.transform.position);
			stateStart = false;
			lightArmed.intensity = initialLightIntensity * 3f;
			meshRenderer.material.SetColor("_EmissionColor", Color.green);
			lightArmed.color = Color.green;
			beepTimer = 1f;
		}
		if (firstLight)
		{
			meshRenderer.material.SetColor("_EmissionColor", emissionColor);
			lightArmed.color = emissionColor;
			firstLight = false;
		}
		else if (!firstLightDone)
		{
			meshRenderer.material.SetColor("_EmissionColor", Color.green);
			lightArmed.color = Color.green;
			firstLightDone = true;
		}
		if (lightArmed.intensity > 0f && beepTimer > 0f)
		{
			float t = 1f - beepTimer;
			lightArmed.intensity = Mathf.Lerp(lightArmed.intensity, 0f, t);
			Color value = Color.Lerp(meshRenderer.material.GetColor("_EmissionColor"), Color.black, t);
			meshRenderer.material.SetColor("_EmissionColor", value);
			beepTimer -= Time.deltaTime * 0.1f;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && itemToggle.toggleState)
		{
			StateSet(States.Arming);
		}
	}

	private void StateArming()
	{
		if (stateStart)
		{
			stateStart = false;
			beepTimer = 1f;
			lightArmed.color = emissionColor;
			Color color = new Color(1f, 0.5f, 0f);
			soundArmingBeep.Play(base.transform.position);
			ColorSet(color);
		}
		beepTimer -= Time.deltaTime * 4f;
		if (beepTimer <= 0f)
		{
			soundArmingBeep.Play(base.transform.position);
			Color color2 = new Color(1f, 0.5f, 0f);
			ColorSet(color2);
			beepTimer = 1f;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!physGrabObject.grabbed)
			{
				stateTimer += Time.deltaTime;
			}
			else
			{
				stateTimer += Time.deltaTime * 0.25f;
			}
			if (physGrabObject.grabbed || physGrabObject.rb.velocity.magnitude > 1f)
			{
				stateTimer = 0f;
			}
			if (stateTimer >= armingTime)
			{
				StateSet(States.Armed);
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && !itemToggle.toggleState)
		{
			StateSet(States.Disarming);
		}
	}

	private void StateArmed()
	{
		if (stateStart)
		{
			soundArmedBeep.Play(base.transform.position);
			ColorSet(emissionColor);
			lightArmed.intensity = initialLightIntensity * 8f;
			stateStart = false;
			secondArmedTimer = 2f;
		}
		lightArmed.intensity = Mathf.Lerp(lightArmed.intensity, initialLightIntensity, Time.deltaTime * 4f);
		if (secondArmedTimer > 0f)
		{
			secondArmedTimer -= Time.deltaTime;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && secondArmedTimer <= 0f && triggeredByForces && physGrabObject.rb.velocity.magnitude > 0.5f)
		{
			StateSet(States.Triggering);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && !itemToggle.toggleState)
		{
			StateSet(States.Disarming);
		}
	}

	private void ColorSet(Color _color)
	{
		lightArmed.intensity = initialLightIntensity;
		lightArmed.color = _color;
		meshRenderer.material.SetColor("_EmissionColor", _color);
	}

	private void StateDisarming()
	{
		if (stateStart)
		{
			stateStart = false;
			beepTimer = 1f;
			soundDisarmingBeep.Play(base.transform.position);
			ColorSet(emissionColor);
			beepTimer = 1f;
		}
		beepTimer -= Time.deltaTime * 4f;
		if (beepTimer <= 0f)
		{
			soundDisarmingBeep.Play(base.transform.position);
			ColorSet(Color.green);
			beepTimer = 1f;
		}
		stateTimer += Time.deltaTime;
		if (stateTimer > 0.1f)
		{
			StateSet(States.Disarmed);
		}
	}

	private void StateTriggering()
	{
		if (stateStart)
		{
			stateStart = false;
			beepTimer = 1f;
		}
		beepTimer -= Time.deltaTime * 4f;
		if (beepTimer < 0f)
		{
			soundTriggereringBeep.Play(base.transform.position);
			ColorSet(emissionColor);
			beepTimer = 1f;
		}
		stateTimer += Time.deltaTime;
		if (stateTimer > triggeringTime)
		{
			StateSet(States.Triggered);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && !itemToggle.toggleState)
		{
			StateSet(States.Disarming);
		}
	}

	private void StateTriggered()
	{
		if (stateStart)
		{
			stateStart = false;
			beepTimer = 1f;
			if (!destroyAfterTimer)
			{
				DestroyMine();
			}
			Color color = new Color(0.5f, 0.9f, 1f);
			ColorSet(color);
		}
		stateTimer += Time.deltaTime;
		if (destroyAfterTimer && stateTimer > destroyTimer)
		{
			DestroyMine();
		}
	}

	public void DestroyMine()
	{
		if (!SemiFunc.RunIsShop())
		{
			if (!mineDestroyed)
			{
				StatsManager.instance.ItemRemove(itemAttributes.instanceName);
				impactDetector.DestroyObject();
				mineDestroyed = true;
			}
		}
		else
		{
			ResetMine();
			physGrabObject.Teleport(startPosition, startRotation);
		}
	}

	private void ResetMine()
	{
		hasBeenGrabbed = false;
		StateSet(States.Disarmed);
		itemToggle.ToggleItem(toggle: false);
		wasTriggeredByForce = false;
		wasTriggeredByEnemy = false;
		wasTriggeredByRigidBody = false;
		wasTriggeredByPlayer = false;
		triggeredPlayerAvatar = null;
		triggeredPlayerTumble = null;
		triggeredPhysGrabObject = null;
		triggeredTransform = null;
		triggeredPosition = Vector3.zero;
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		stateTimer = 0f;
		Rigidbody component = GetComponent<Rigidbody>();
		if (!component.isKinematic)
		{
			component.velocity = Vector3.zero;
			component.angularVelocity = Vector3.zero;
		}
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

	private void AnimateLight()
	{
		if (lightArmed.intensity > 0f && beepTimer > 0f)
		{
			float t = 1f - beepTimer;
			lightArmed.intensity = Mathf.Lerp(lightArmed.intensity, 0f, t);
			Color value = Color.Lerp(meshRenderer.material.GetColor("_EmissionColor"), Color.black, t);
			meshRenderer.material.SetColor("_EmissionColor", value);
		}
	}

	private void Update()
	{
		TriggerRotation();
		TriggerLineVisuals();
		TriggerScaleFixer();
		AnimateLight();
		if (physGrabObject.grabbedLocal && !SemiFunc.RunIsShop())
		{
			PhysGrabber.instance.OverrideGrabDistance(1f);
		}
		if (physGrabObject.grabbed)
		{
			hasBeenGrabbed = true;
		}
		if (itemEquippable.isEquipped && SemiFunc.IsMasterClientOrSingleplayer() && hasBeenGrabbed)
		{
			StateSet(States.Disarmed);
		}
		if (!SemiFunc.RunIsShop())
		{
			if (SemiFunc.IsMasterClientOrSingleplayer() && wasGrabbed && !physGrabObject.grabbed)
			{
				Rigidbody component = GetComponent<Rigidbody>();
				if (!component.isKinematic)
				{
					component.velocity *= 0.15f;
				}
			}
			wasGrabbed = physGrabObject.grabbed;
		}
		switch (state)
		{
		case States.Disarmed:
			StateDisarmed();
			break;
		case States.Arming:
			StateArming();
			break;
		case States.Armed:
			StateArmed();
			break;
		case States.Disarming:
			StateDisarming();
			break;
		case States.Triggering:
			StateTriggering();
			break;
		case States.Triggered:
			StateTriggered();
			break;
		}
	}

	[PunRPC]
	public void TriggeredRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			onTriggered.Invoke();
		}
	}

	private void StateSet(States newState)
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || newState == state)
		{
			return;
		}
		if (newState == States.Triggered)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("TriggeredRPC", RpcTarget.All);
			}
			else
			{
				TriggeredRPC();
			}
		}
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("StateSetRPC", RpcTarget.All, (int)newState);
		}
		else
		{
			StateSetRPC((int)newState);
		}
	}

	[PunRPC]
	public void StateSetRPC(int newState, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			state = (States)newState;
			stateStart = true;
			stateTimer = 0f;
			beepTimer = 0f;
		}
	}

	private void TriggerScaleFixer()
	{
		if (state != States.Armed)
		{
			return;
		}
		bool flag = false;
		if (SemiFunc.FPSImpulse30())
		{
			if (Vector3.Distance(prevPos, base.transform.position) > 0.01f)
			{
				flag = true;
				prevPos = base.transform.position;
			}
			if (Quaternion.Angle(prevRot, base.transform.rotation) > 0.01f)
			{
				flag = true;
				prevRot = base.transform.rotation;
			}
		}
		if ((!flag && SemiFunc.FPSImpulse1()) || (flag && SemiFunc.FPSImpulse30()))
		{
			if (Physics.Raycast(triggerTransform.position, triggerTransform.forward, out var hitInfo, 1f, LayerMask.GetMask("Default")))
			{
				targetLineLength = hitInfo.distance * 0.8f;
			}
			else
			{
				targetLineLength = 1f;
			}
		}
		triggerTransform.localScale = Mathf.Lerp(triggerTransform.localScale.z, targetLineLength, Time.deltaTime * 8f) * Vector3.one;
	}

	private void TriggerRotation()
	{
		upsideDown = true;
		if (Vector3.Dot(base.transform.up, Vector3.up) < 0f)
		{
			upsideDown = false;
		}
		if (upsideDown)
		{
			triggerTargetRotation = Quaternion.Euler(-90f, 0f, 0f);
		}
		else
		{
			triggerTargetRotation = Quaternion.Euler(90f, 0f, 0f);
		}
		triggerTransform.localRotation = SemiFunc.SpringQuaternionGet(triggerSpringQuaternion, triggerTargetRotation);
	}

	private void TriggerLineVisuals()
	{
		if (state == States.Armed)
		{
			triggerLine.material.SetTextureOffset("_MainTex", new Vector2((0f - Time.time) * 2f, 0f));
			if (!triggerLine.enabled)
			{
				triggerLine.enabled = true;
				lineParticles.Play();
			}
			triggerLine.widthMultiplier = Mathf.Lerp(triggerLine.widthMultiplier, 1f, Time.deltaTime * 4f);
		}
		else if (triggerLine.enabled)
		{
			triggerLine.widthMultiplier = Mathf.Lerp(triggerLine.widthMultiplier, 0f, Time.deltaTime * 8f);
			if (triggerLine.widthMultiplier < 0.01f)
			{
				triggerLine.enabled = false;
				lineParticles.Stop();
			}
		}
	}

	public void SetTriggered()
	{
		if (state == States.Armed)
		{
			StateSet(States.Triggering);
		}
	}
}
