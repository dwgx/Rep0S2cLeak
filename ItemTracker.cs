using Photon.Pun;
using TMPro;
using UnityEngine;

public class ItemTracker : MonoBehaviour
{
	public enum TrackerType
	{
		Valuable,
		Extraction
	}

	public TrackerType trackerType;

	private float timer;

	private Transform currentTarget;

	private PhysGrabObject currentTargetPhysGrabObject;

	private Rigidbody rb;

	public Transform nozzleTransform;

	private PhysGrabObject physGrabObject;

	public MeshRenderer meshRenderer;

	public AnimationCurve animationCurve;

	private float blipTimer;

	public Sound soundBleep;

	public Sound digitSwap;

	public Sound soundTargetFound;

	public Sound soundTargetLost;

	private ItemToggle itemToggle;

	private ItemBattery itemBattery;

	private PhotonView photonView;

	private bool currentToggleState;

	public Light nozzleLight;

	public MeshRenderer display;

	public TextMeshPro displayText;

	private int prevDigit;

	private float changeDigitTimer;

	private float displayOverrideTimer;

	public Light displayLight;

	private bool hasTarget;

	public Color colorBleep;

	public Color colorBleepOff;

	public Color colorTargetFound;

	public Color colorScreenNeutral;

	private Vector3 targetPosition;

	private float batteryOutTimer;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		physGrabObject = GetComponent<PhysGrabObject>();
		itemToggle = GetComponent<ItemToggle>();
		itemBattery = GetComponent<ItemBattery>();
		photonView = GetComponent<PhotonView>();
		meshRenderer.material.SetColor("_EmissionColor", Color.black);
		nozzleLight.enabled = false;
		nozzleLight.intensity = 0f;
	}

	private void ValuableTarget()
	{
		if (trackerType != TrackerType.Valuable)
		{
			return;
		}
		Vector3 position = nozzleTransform.position;
		hasTarget = false;
		float radius = 15f;
		if (!currentTarget)
		{
			radius = 30f;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius);
		float num = float.MaxValue;
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ValuableObject componentInParent = array2[i].gameObject.GetComponentInParent<ValuableObject>();
			if ((bool)componentInParent && !componentInParent.discovered)
			{
				PhysGrabObject component = componentInParent.GetComponent<PhysGrabObject>();
				PhysGrabObjectImpactDetector component2 = componentInParent.GetComponent<PhysGrabObjectImpactDetector>();
				float num2 = Vector3.Distance(position, component.midPoint);
				if (num2 < num && !component.grabbed && !component2.inCart)
				{
					num = num2;
					currentTarget = component.transform;
					currentTargetPhysGrabObject = component;
					hasTarget = true;
				}
			}
		}
		if (hasTarget)
		{
			SetTarget(currentTargetPhysGrabObject.photonView.ViewID);
		}
	}

	private void ExtractionTarget()
	{
		if (trackerType == TrackerType.Extraction)
		{
			hasTarget = false;
			ExtractionPoint extractionPoint = SemiFunc.ExtractionPointGetNearestNotActivated(nozzleTransform.position);
			if ((bool)extractionPoint)
			{
				currentTarget = extractionPoint.transform;
				hasTarget = true;
			}
			if (hasTarget)
			{
				SetTarget(currentTarget.GetComponent<PhotonView>().ViewID);
			}
		}
	}

	private void FindATarget()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && !(itemBattery.batteryLife <= 0f))
		{
			timer += Time.deltaTime;
			if (timer > 2f)
			{
				ValuableTarget();
				ExtractionTarget();
				timer = 0f;
			}
		}
	}

	private void AnimateEmissionToBlack()
	{
		if (!itemToggle.toggleState)
		{
			Color color = meshRenderer.material.GetColor("_EmissionColor");
			if (color != Color.black)
			{
				meshRenderer.material.SetColor("_EmissionColor", Color.Lerp(color, Color.black, Time.deltaTime * 20f));
			}
			if (nozzleLight.intensity > 0f)
			{
				nozzleLight.intensity = Mathf.Lerp(nozzleLight.intensity, 0f, Time.deltaTime * 10f);
			}
			else
			{
				nozzleLight.enabled = false;
			}
		}
	}

	private void PhysGrabOverrides()
	{
		if (physGrabObject.grabbed && physGrabObject.grabbedLocal)
		{
			PhysGrabber.instance.OverrideGrabDistance(0.8f);
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (physGrabObject.grabbed)
		{
			Quaternion turnX = Quaternion.Euler(0f, 0f, 0f);
			Quaternion turnY = Quaternion.Euler(0f, 0f, 0f);
			Quaternion identity = Quaternion.identity;
			physGrabObject.TurnXYZ(turnX, turnY, identity);
			physGrabObject.OverrideTorqueStrengthX(2f);
			if ((bool)currentTarget && itemToggle.toggleState)
			{
				physGrabObject.OverrideTorqueStrengthY(0.1f);
			}
			physGrabObject.OverrideGrabVerticalPosition(-0.2f);
		}
		else if (itemToggle.toggleState)
		{
			itemToggle.ToggleItem(toggle: false);
		}
	}

	private void DisplayLogic()
	{
		if (display.gameObject.activeSelf)
		{
			Vector2 textureOffset = display.material.GetTextureOffset("_MainTex");
			textureOffset.y += Time.deltaTime * 2f;
			display.material.SetTextureOffset("_MainTex", textureOffset);
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				itemBattery.batteryLife -= Time.deltaTime * 0.5f;
			}
		}
		else if (displayText.text != "--")
		{
			displayText.text = "--";
		}
		if (displayOverrideTimer >= 0f)
		{
			displayOverrideTimer -= Time.deltaTime;
			if (displayOverrideTimer <= 0f)
			{
				displayText.text = "--";
				Color color = colorScreenNeutral;
				color.a = 0.2f;
				displayText.color = colorScreenNeutral;
				display.material.color = color;
				displayLight.color = colorScreenNeutral;
			}
		}
		if (trackerType == TrackerType.Valuable && (bool)currentTargetPhysGrabObject)
		{
			targetPosition = currentTargetPhysGrabObject.midPoint;
		}
		if (trackerType == TrackerType.Extraction && (bool)currentTarget)
		{
			targetPosition = currentTarget.position;
		}
		if (changeDigitTimer <= 0f && displayOverrideTimer <= 0f)
		{
			if (hasTarget && display.gameObject.activeSelf)
			{
				int num = Mathf.RoundToInt(Vector3.Distance(nozzleTransform.position, targetPosition));
				if (num != prevDigit)
				{
					changeDigitTimer = 1f;
					digitSwap.Play(display.transform.position);
					prevDigit = num;
				}
				displayText.text = num.ToString();
			}
			else
			{
				displayText.text = "--";
			}
		}
		else
		{
			changeDigitTimer -= Time.deltaTime;
		}
		if (!SemiFunc.FPSImpulse15())
		{
			return;
		}
		if (itemToggle.toggleState)
		{
			if (!display.gameObject.activeSelf)
			{
				display.gameObject.SetActive(value: true);
			}
		}
		else if (display.gameObject.activeSelf)
		{
			display.gameObject.SetActive(value: false);
		}
	}

	private void TargetLogic()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (!itemToggle.toggleState)
		{
			hasTarget = false;
			currentTarget = null;
			displayOverrideTimer = 0f;
			return;
		}
		if (trackerType == TrackerType.Valuable)
		{
			if ((bool)currentTarget && currentTarget.GetComponent<ValuableObject>().discovered && hasTarget)
			{
				CurrentTargetUpdate(_found: true);
				currentTarget = null;
				hasTarget = false;
			}
			if (!currentTarget && hasTarget && physGrabObject.grabbed)
			{
				CurrentTargetUpdate(_found: false);
				hasTarget = false;
			}
		}
		if (trackerType == TrackerType.Extraction)
		{
			if ((bool)currentTarget)
			{
				hasTarget = true;
			}
			else
			{
				hasTarget = false;
			}
		}
	}

	private void Update()
	{
		PhysGrabOverrides();
		if (itemBattery.batteryLifeInt == 0 && itemToggle.toggleState)
		{
			if (!display.gameObject.activeSelf && itemToggle.toggleState)
			{
				display.gameObject.SetActive(value: true);
				batteryOutTimer = 0f;
			}
			if (batteryOutTimer == 0f)
			{
				soundTargetLost.Play(display.transform.position);
			}
			if (batteryOutTimer > 2f && itemToggle.toggleState)
			{
				itemToggle.ToggleItem(toggle: false);
				display.gameObject.SetActive(value: false);
				batteryOutTimer = 0f;
			}
			else
			{
				DisplayColorOverride("X", Color.red, 2f);
				batteryOutTimer += Time.deltaTime;
			}
			return;
		}
		batteryOutTimer = 0f;
		DisplayLogic();
		TargetLogic();
		if (!(displayOverrideTimer > 0f))
		{
			AnimateEmissionToBlack();
			if (itemToggle.toggleState)
			{
				FindATarget();
				Blinking();
			}
		}
	}

	private void Blinking()
	{
		Color color = meshRenderer.material.GetColor("_EmissionColor");
		Color color2 = colorBleepOff;
		if (color != color2)
		{
			Color color3 = Color.Lerp(color, color2, Time.deltaTime * 4f);
			meshRenderer.material.SetColor("_EmissionColor", color3);
			nozzleLight.color = color3;
		}
		if (nozzleLight.intensity < 1f)
		{
			if (!nozzleLight.enabled)
			{
				nozzleLight.enabled = true;
			}
			nozzleLight.intensity = Mathf.Lerp(nozzleLight.intensity, 2f, Time.deltaTime * 10f);
		}
		if (hasTarget)
		{
			Vector3 position = nozzleTransform.position;
			float num = 1.5f;
			float num2 = 0.2f;
			blipTimer += Time.deltaTime;
			float num3 = 5f;
			float num4 = 0f;
			float time = (Mathf.Clamp(Vector3.Distance(position, targetPosition), num4, num3) - num4) / (num3 - num4);
			float num5 = animationCurve.Evaluate(time);
			float num6 = Mathf.Lerp(num2, num, num5);
			if (blipTimer > num6)
			{
				blipTimer = 0f;
				soundBleep.Pitch = Mathf.Lerp(1f, 2f, 1f - num5);
				soundBleep.Play(nozzleTransform.position);
				meshRenderer.material.SetColor("_EmissionColor", colorBleep);
				nozzleLight.color = colorBleep;
				nozzleLight.enabled = true;
			}
		}
	}

	private void FixedUpdate()
	{
		if (!(itemBattery.batteryLife <= 0f))
		{
			if (!itemToggle.toggleState)
			{
				currentTarget = null;
				hasTarget = false;
			}
			else if (!(displayOverrideTimer > 0f) && SemiFunc.IsMasterClientOrSingleplayer() && hasTarget && physGrabObject.grabbed)
			{
				SemiFunc.PhysLookAtPositionWithForce(rb, base.transform, targetPosition, 10f);
				rb.AddForceAtPosition(base.transform.forward * 1f, nozzleTransform.position, ForceMode.Force);
			}
		}
	}

	private void SetTarget(int photonViewID)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("SetTargetRPC", RpcTarget.All, photonViewID);
		}
	}

	[PunRPC]
	private void SetTargetRPC(int targetViewID)
	{
		PhysGrabObject component = PhotonView.Find(targetViewID).GetComponent<PhysGrabObject>();
		Transform transform = PhotonView.Find(targetViewID).transform;
		currentTarget = transform;
		if ((bool)component)
		{
			currentTargetPhysGrabObject = component;
		}
		hasTarget = true;
	}

	private void DisplayColorOverride(string _text, Color _color, float _time)
	{
		displayText.text = _text;
		displayText.color = _color;
		displayOverrideTimer = _time;
		displayLight.color = _color;
		_color.a = 0.2f;
		display.material.color = _color;
	}

	private void CurrentTargetUpdate(bool _found)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("CurrentTargetUpdateRPC", RpcTarget.All, _found);
		}
		else
		{
			CurrentTargetUpdateRPC(_found);
		}
	}

	[PunRPC]
	public void CurrentTargetUpdateRPC(bool _found)
	{
		if (_found)
		{
			soundTargetFound.Play(display.transform.position);
			DisplayColorOverride("FOUND", colorTargetFound, 2f);
		}
		else
		{
			soundTargetLost.Play(display.transform.position);
			DisplayColorOverride("NOT FOUND", Color.red, 2f);
		}
		currentTarget = null;
		currentTargetPhysGrabObject = null;
		hasTarget = false;
	}
}
