using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ItemCartCannonMain : MonoBehaviour
{
	public enum state
	{
		inactive,
		active,
		buildup,
		shooting,
		goingBack
	}

	public float shootBuildUpTime = 0.5f;

	public float shootTime = 0.5f;

	public float goBackFromShootTime = 0.5f;

	public bool singleShot;

	public Color mainOnColor = Color.green;

	public float investigationRange = 35f;

	private ItemBattery battery;

	private PhysGrabObject physGrabObject;

	internal bool impulseShoot;

	internal bool impulseShooting;

	public Transform muzzle;

	private ItemToggle itemToggle;

	private bool prevToggleState;

	private Rigidbody rb;

	private PhysGrabObjectImpactDetector impactDetector;

	internal bool isActive;

	internal Quaternion rotationTargetY;

	public GameObject currentCorrector;

	public GameObject cannonGrabPoint;

	private PhysGrabObjectGrabArea physGrabObjectGrabArea;

	public List<MeshRenderer> grabMeshRenderers = new List<MeshRenderer>();

	private List<Material> grabMaterials = new List<Material>();

	public MeshRenderer cartLogoScreen;

	public Light cartGrabLight;

	private PhotonView photonView;

	public MeshRenderer mainMesh;

	public Light mainLight;

	public Sound soundBootUp;

	public Sound soundShutdown;

	public Sound soundAimLoop;

	public Sound soundQuickTurn;

	private Quaternion prevRotation;

	private float quickTurnSoundCooldown;

	private float smoothPitch;

	internal float stateTimer;

	internal float stateTimerMax = 0.5f;

	internal bool stateStart = true;

	internal state stateCurrent;

	internal state statePrev;

	private bool isFixedUpdate;

	private bool singleShotNextFrame;

	public Sound soundGrabStart;

	public Sound soundGrabEnd;

	private bool handleGrabbed;

	private void Start()
	{
		battery = GetComponent<ItemBattery>();
		physGrabObject = GetComponent<PhysGrabObject>();
		itemToggle = GetComponent<ItemToggle>();
		impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		rb = GetComponent<Rigidbody>();
		physGrabObjectGrabArea = GetComponent<PhysGrabObjectGrabArea>();
		photonView = GetComponent<PhotonView>();
		foreach (MeshRenderer grabMeshRenderer in grabMeshRenderers)
		{
			if (grabMeshRenderer != null)
			{
				Material material = grabMeshRenderer.material;
				if (material != null)
				{
					grabMaterials.Add(material);
				}
			}
		}
	}

	private void Update()
	{
		impulseShoot = false;
		StateMachine();
		MovementSounds();
		if (stateTimer <= stateTimerMax)
		{
			stateTimer += Time.deltaTime;
		}
	}

	private void FixedUpdate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			isFixedUpdate = true;
			StateMachine();
			isFixedUpdate = false;
		}
	}

	private void StateSet(int state)
	{
		if (state == (int)stateCurrent)
		{
			return;
		}
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("StateSetRPC", RpcTarget.All, state);
			}
		}
		else
		{
			StateSetRPC(state);
		}
	}

	[PunRPC]
	private void StateSetRPC(int state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			stateStart = true;
			statePrev = stateCurrent;
			stateCurrent = (state)state;
			stateTimer = 0f;
		}
	}

	private void StateInactive()
	{
		if (stateStart && !isFixedUpdate)
		{
			mainMesh.material.SetColor("_EmissionColor", Color.red);
			mainLight.color = Color.red;
			cartLogoScreen.material.SetColor("_EmissionColor", Color.red);
			soundShutdown.Play(base.transform.position);
			int num = 0;
			foreach (Material grabMaterial in grabMaterials)
			{
				grabMaterial.SetColor("_EmissionColor", Color.red);
				if (num == 1)
				{
					grabMaterial.mainTextureOffset = new Vector2(0f, 0f);
				}
				num++;
			}
			cartGrabLight.color = Color.red;
			stateStart = false;
		}
		if (!isFixedUpdate)
		{
			if (itemToggle.toggleState)
			{
				prevToggleState = false;
				itemToggle.toggleState = false;
				itemToggle.onToggle.Invoke();
			}
			if (SemiFunc.FPSImpulse5())
			{
				bool flag = true;
				if (!impactDetector.inCart)
				{
					flag = false;
				}
				if (!impactDetector.currentCart)
				{
					flag = false;
				}
				if (battery.batteryLifeInt <= 0)
				{
					flag = false;
				}
				if (flag)
				{
					StateSet(1);
				}
			}
		}
		_ = isFixedUpdate;
	}

	private void StateActive()
	{
		if (stateStart && !isFixedUpdate)
		{
			mainMesh.material.SetColor("_EmissionColor", mainOnColor);
			mainLight.color = mainOnColor;
			cartLogoScreen.material.SetColor("_EmissionColor", Color.green);
			stateStart = false;
			if (statePrev == state.inactive)
			{
				soundBootUp.Play(base.transform.position);
			}
		}
		if (!isFixedUpdate)
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				if (!impactDetector.currentCart || !impactDetector.inCart || battery.batteryLifeInt <= 0)
				{
					StateSet(0);
					return;
				}
				if (itemToggle.toggleState != prevToggleState && battery.batteryLifeInt > 0)
				{
					prevToggleState = itemToggle.toggleState;
					StateSet(2);
					itemToggle.toggleImpulse = false;
				}
			}
			CorrectorAndLightLogic();
		}
		if (isFixedUpdate)
		{
			GrabLogic();
		}
	}

	private void StateBuildup()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = shootBuildUpTime;
		}
		if (!isFixedUpdate)
		{
			CorrectorAndLightLogic();
		}
		if (isFixedUpdate)
		{
			GrabLogic();
		}
		if (stateTimer >= stateTimerMax)
		{
			StateSet(3);
		}
	}

	private void StateShooting()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = shootTime;
			singleShotNextFrame = false;
			EnemyDirector.instance.SetInvestigate(base.transform.position, investigationRange);
			impulseShoot = true;
		}
		if (!isFixedUpdate)
		{
			if (singleShot)
			{
				if (singleShotNextFrame)
				{
					StateSet(4);
					singleShotNextFrame = false;
					return;
				}
				singleShotNextFrame = true;
			}
			impulseShoot = true;
			CorrectorAndLightLogic();
			if (!impactDetector.inCart)
			{
				stateTimer = stateTimerMax;
			}
		}
		if (isFixedUpdate)
		{
			GrabLogic();
		}
		if (stateTimer >= stateTimerMax)
		{
			StateSet(4);
		}
	}

	private void StateGoingBack()
	{
		if (stateStart && !isFixedUpdate)
		{
			stateStart = false;
			stateTimerMax = goBackFromShootTime;
		}
		if (!isFixedUpdate)
		{
			CorrectorAndLightLogic();
		}
		if (isFixedUpdate)
		{
			GrabLogic();
		}
		if (stateTimer >= stateTimerMax)
		{
			StateSet(1);
		}
	}

	private void StateMachine()
	{
		switch (stateCurrent)
		{
		case state.inactive:
			StateInactive();
			break;
		case state.active:
			StateActive();
			break;
		case state.buildup:
			StateBuildup();
			break;
		case state.shooting:
			StateShooting();
			break;
		case state.goingBack:
			StateGoingBack();
			break;
		}
	}

	private void GrabLogic()
	{
		if (!currentCorrector || !impactDetector.currentCart)
		{
			return;
		}
		float y = rotationTargetY.eulerAngles.y;
		float y2 = impactDetector.currentCart.transform.rotation.eulerAngles.y;
		float num = Mathf.DeltaAngle(y2, y);
		Quaternion quaternion = Quaternion.Euler(0f, y2 - num, 0f);
		bool flag = false;
		List<PhysGrabber> listOfAllGrabbers = physGrabObjectGrabArea.listOfAllGrabbers;
		bool flag2 = false;
		foreach (PhysGrabber item in physGrabObject.playerGrabbing)
		{
			if (item.isRotating)
			{
				flag2 = true;
				break;
			}
		}
		if (physGrabObject.playerGrabbing.Count > 0 && listOfAllGrabbers.Count > 0)
		{
			foreach (PhysGrabber item2 in listOfAllGrabbers)
			{
				item2.OverrideGrabPoint(cannonGrabPoint.transform);
			}
			if ((bool)currentCorrector)
			{
				Quaternion rotation = physGrabObject.playerGrabbing[0].playerAvatar.localCamera.transform.rotation;
				currentCorrector.transform.rotation = physGrabObject.playerGrabbing[0].playerAvatar.localCamera.transform.rotation;
				currentCorrector.transform.rotation = Quaternion.Euler(0f, currentCorrector.transform.rotation.eulerAngles.y, 0f);
				Vector3 vector = physGrabObject.playerGrabbing[0].playerAvatar.localCamera.transform.position + rotation * Vector3.forward * 0.5f;
				Vector3 position = new Vector3(vector.x, base.transform.position.y, vector.z);
				currentCorrector.transform.position = position;
			}
			flag = true;
			if (!flag2)
			{
				physGrabObject.OverrideTorqueStrength(0f);
			}
			physGrabObject.OverrideGrabStrength(0f);
			physGrabObject.OverrideMass(2f);
			Vector3 force = Vector3.down * 2f;
			rb.AddForce(force, ForceMode.Force);
		}
		if (!flag2)
		{
			quaternion = currentCorrector.transform.rotation;
			float num2 = 0.5f;
			if (flag)
			{
				num2 = 0.8f;
			}
			float num3 = Quaternion.Angle(quaternion, base.transform.rotation);
			num2 *= num3;
			physGrabObject.OverrideAngularDrag(5f);
			num2 = Mathf.Min(num2, 12f);
			Vector3 torque = SemiFunc.PhysFollowRotation(base.transform, quaternion, rb, num2 * 0.1f);
			rb.AddTorque(torque, ForceMode.Impulse);
		}
		Vector3 position2 = base.transform.position;
		Vector3 position3 = currentCorrector.transform.position;
		if (flag)
		{
			Vector3 vector2 = SemiFunc.PhysFollowPosition(position2, position3, rb.velocity, 5f);
			rb.AddForce(vector2 * 0.1f, ForceMode.Impulse);
		}
	}

	private void MovementSounds()
	{
		float num = Quaternion.Angle(prevRotation, base.transform.rotation);
		prevRotation = base.transform.rotation;
		float num2 = num / Time.deltaTime;
		bool flag = num2 > 6f && physGrabObjectGrabArea.listOfAllGrabbers.Count > 0 && stateCurrent != state.inactive;
		float num3 = Mathf.Max(num2 * 0.01f, 0.5f);
		smoothPitch = Mathf.Lerp(smoothPitch, num3, Time.deltaTime * 5f);
		soundAimLoop.PlayLoop(flag, 1f, 0.5f, smoothPitch);
		if (SemiFunc.FPSImpulse5() && num2 > 120f && quickTurnSoundCooldown <= 0f && flag)
		{
			soundQuickTurn.Play(base.transform.position);
			quickTurnSoundCooldown = 1f;
		}
		if (quickTurnSoundCooldown > 0f)
		{
			quickTurnSoundCooldown -= Time.deltaTime;
		}
	}

	private void CorrectorAndLightLogic()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && (bool)impactDetector.currentCart)
		{
			if (!currentCorrector || ((bool)currentCorrector && currentCorrector.transform.parent != impactDetector.currentCart.transform))
			{
				if ((bool)currentCorrector)
				{
					Object.Destroy(currentCorrector);
				}
				GameObject original = new GameObject("GreatCorrector");
				currentCorrector = Object.Instantiate(original, impactDetector.currentCart.transform);
				currentCorrector.transform.localPosition = Vector3.zero;
				currentCorrector.transform.localRotation = Quaternion.identity;
				currentCorrector.transform.localScale = Vector3.one;
				currentCorrector.transform.SetParent(impactDetector.currentCart.transform);
			}
			if (physGrabObject.playerGrabbing.Count > 0)
			{
				rotationTargetY = physGrabObject.playerGrabbing[0].playerAvatar.localCamera.transform.rotation;
			}
		}
		if (!SemiFunc.FPSImpulse15())
		{
			return;
		}
		if (physGrabObjectGrabArea.listOfAllGrabbers.Count > 0)
		{
			if (!handleGrabbed)
			{
				soundGrabStart.Play(base.transform.position);
				handleGrabbed = true;
			}
			int num = 0;
			foreach (Material grabMaterial in grabMaterials)
			{
				grabMaterial.SetColor("_EmissionColor", Color.green);
				if (num == 1)
				{
					grabMaterial.mainTextureOffset = new Vector2(0.5f, 0f);
				}
				num++;
			}
			cartGrabLight.color = Color.green;
			return;
		}
		if (handleGrabbed)
		{
			soundGrabEnd.Play(base.transform.position);
			handleGrabbed = false;
		}
		int num2 = 0;
		foreach (Material grabMaterial2 in grabMaterials)
		{
			grabMaterial2.SetColor("_EmissionColor", Color.red);
			if (num2 == 1)
			{
				grabMaterial2.mainTextureOffset = new Vector2(0f, 0f);
			}
			num2++;
		}
		cartGrabLight.color = Color.red;
	}
}
