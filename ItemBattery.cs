using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ItemBattery : MonoBehaviour
{
	public int batteryBars = 6;

	public bool isUnchargable;

	public Transform batteryTransform;

	private Camera mainCamera;

	public float upOffset = 0.5f;

	[HideInInspector]
	public bool batteryActive;

	[HideInInspector]
	public float batteryLife = 100f;

	internal int batteryLifeInt = 6;

	private float batteryOutBlinkTimer;

	private PhotonView photonView;

	[HideInInspector]
	public Color batteryColor;

	internal Color batteryColorMedium;

	private float chargeTimer;

	private float chargeRate;

	private List<GameObject> chargerList = new List<GameObject>();

	internal bool isCharging;

	private float chargingBlinkTimer;

	private bool chargingBlink;

	private ItemAttributes itemAttributes;

	private float showTimer;

	private bool showBattery;

	public bool autoDrain = true;

	private ItemEquippable itemEquippable;

	public bool onlyShowWhenItemToggleIsOn;

	public float batteryDrainRate = 1f;

	private float drainRate;

	private float drainTimer;

	internal int currentBars = 6;

	private int batteryLifeCountBars = 6;

	internal int batteryLifeCountBarsPrev = 6;

	private BatteryVisualLogic batteryVisualLogic;

	private bool tutorialCheck;

	private PhysGrabObject physGrabObject;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
		itemAttributes = GetComponent<ItemAttributes>();
		if (!itemAttributes)
		{
			Debug.LogWarning("ItemBattery.cs: No ItemAttributes found on " + base.gameObject.name);
		}
	}

	private void Start()
	{
		batteryVisualLogic = GetComponentInChildren<BatteryVisualLogic>();
		if ((bool)batteryVisualLogic)
		{
			batteryVisualLogic.itemBattery = this;
		}
		itemEquippable = GetComponent<ItemEquippable>();
		mainCamera = Camera.main;
		batteryLifeCountBars = batteryBars;
		batteryLifeCountBarsPrev = batteryBars;
		batteryLifeInt = batteryBars;
		batteryLife = 100f;
		physGrabObject = GetComponentInChildren<PhysGrabObject>();
		if (SemiFunc.RunIsLevel() && TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialChargingStation, 1))
		{
			tutorialCheck = true;
		}
	}

	private IEnumerator BatteryInit()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.2f);
		}
		while (GetComponent<ItemAttributes>().instanceName == null)
		{
			yield return new WaitForSeconds(0.2f);
		}
		if (SemiFunc.RunIsArena())
		{
			StatsManager.instance.SetBatteryLevel(itemAttributes.instanceName, 100);
		}
		batteryLife = StatsManager.instance.GetBatteryLevel(itemAttributes.instanceName);
		if (batteryLife > 0f)
		{
			batteryLifeInt = (int)Mathf.Round(batteryLife / (float)(100 / batteryBars));
			batteryColor = itemAttributes.colorPreset.GetColorLight();
			batteryColorMedium = itemAttributes.colorPreset.GetColorMain();
		}
		else
		{
			batteryLife = 0f;
			batteryLifeInt = 0;
			batteryColor = itemAttributes.colorPreset.GetColorLight();
			batteryColorMedium = itemAttributes.colorPreset.GetColorMain();
		}
		BatteryFullPercentChange(batteryLifeInt);
	}

	public void SetBatteryLife(int _batteryLife)
	{
		if (batteryLife > 0f)
		{
			batteryLife = _batteryLife;
			batteryLifeInt = (int)Mathf.Round(batteryLife / (float)(100 / batteryBars));
			batteryLifeInt = Mathf.Min(batteryLifeInt, batteryBars);
			batteryLife = batteryLifeInt * (100 / batteryBars);
		}
		else
		{
			batteryLife = 0f;
			batteryLifeInt = 0;
		}
		batteryColor = itemAttributes.colorPreset.GetColorLight();
		batteryColorMedium = itemAttributes.colorPreset.GetColorMain();
		BatteryFullPercentChange(batteryLifeInt);
	}

	public void OverrideBatteryShow(float time = 0.1f)
	{
		showTimer = time;
	}

	public void ChargeBattery(GameObject chargerObject, float chargeAmount)
	{
		if (!chargerList.Contains(chargerObject))
		{
			chargerList.Add(chargerObject);
			chargeRate += chargeAmount;
		}
		chargeTimer = 0.1f;
	}

	private void FixedUpdate()
	{
		SemiFunc.IsMasterClientOrSingleplayer();
	}

	private void Update()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if ((!(chargeTimer > 0f) || !(batteryLife < 99f)) && chargeRate != 0f)
			{
				chargeRate = 0f;
				chargeTimer = 0f;
				chargerList.Clear();
				BatteryChargeToggle(toggle: false);
			}
			if (chargeTimer > 0f && batteryLife < 99f)
			{
				batteryLife = Mathf.Clamp(batteryLife + chargeRate * Time.deltaTime, 0f, 100f);
				if (!isCharging)
				{
					BatteryChargeToggle(toggle: true);
				}
				chargeTimer -= Time.deltaTime;
			}
			if ((!(drainTimer > 0f) || !(batteryLife > 0f)) && drainRate != 0f)
			{
				drainRate = 0f;
				drainTimer = 0f;
			}
			if (drainTimer > 0f && batteryLife > 0f)
			{
				batteryLife = Mathf.Clamp(batteryLife - drainRate * Time.deltaTime, 0f, 100f);
				drainTimer -= Time.deltaTime;
			}
		}
		if (SemiFunc.RunIsLobby() && batteryLifeInt < batteryBars)
		{
			OverrideBatteryShow();
		}
		if ((bool)batteryVisualLogic && SemiFunc.RunIsLobby())
		{
			OverrideBatteryShow(0.2f);
			if (batteryVisualLogic.currentBars <= batteryVisualLogic.batteryBars / 2)
			{
				batteryVisualLogic.OverrideChargeNeeded(0.2f);
			}
		}
		if (showTimer <= 0f)
		{
			showBattery = false;
		}
		if (showTimer > 0f)
		{
			showTimer -= Time.deltaTime;
			showBattery = true;
		}
		if (showBattery && !batteryVisualLogic.gameObject.activeSelf)
		{
			batteryVisualLogic.gameObject.SetActive(value: true);
			batteryVisualLogic.BatteryBarsSet();
		}
		if ((itemAttributes.shopItem && SemiFunc.IsMasterClientOrSingleplayer()) || RoundDirector.instance.debugInfiniteBattery)
		{
			batteryLife = 100f;
		}
		if (isCharging)
		{
			batteryVisualLogic.OverrideBatteryCharge(0.2f);
		}
		if (!SemiFunc.RunIsShop() && (bool)PhysGrabber.instance && PhysGrabber.instance.grabbed && PhysGrabber.instance.grabbedPhysGrabObject == physGrabObject)
		{
			if (BatteryUI.instance.batteryVisualLogic.itemBattery != this)
			{
				BatteryUI.instance.batteryVisualLogic.itemBattery = this;
				BatteryUI.instance.batteryVisualLogic.BatteryBarsSet();
			}
			BatteryUI.instance.Show();
		}
		BatteryLookAt();
		BatteryChargingVisuals();
		if (showBattery && (bool)batteryVisualLogic && !batteryVisualLogic.gameObject.activeSelf)
		{
			BatteryUpdateBars(batteryLifeInt);
		}
		if (tutorialCheck && batteryLife <= 0f && SemiFunc.FPSImpulse15() && physGrabObject.playerGrabbing.Count > 0)
		{
			foreach (PhysGrabber item in physGrabObject.playerGrabbing)
			{
				if (item.isLocal && TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialChargingStation, 1))
				{
					TutorialDirector.instance.ActivateTip("Charging Station", 2f, _interrupt: false);
					tutorialCheck = false;
				}
			}
		}
		if (batteryActive)
		{
			if (SemiFunc.IsMasterClientOrSingleplayer() && autoDrain && !itemEquippable.isEquipped)
			{
				batteryLife -= batteryDrainRate * Time.deltaTime;
			}
			if (batteryLifeInt < 1)
			{
				if (batteryLifeInt == 1)
				{
					batteryOutBlinkTimer += Time.deltaTime;
				}
				else
				{
					batteryOutBlinkTimer += 5f * Time.deltaTime;
				}
				batteryVisualLogic.OverrideBatteryOutWarning(0.2f);
			}
			if ((bool)batteryVisualLogic && !batteryVisualLogic.gameObject.activeSelf && !SemiFunc.RunIsShop())
			{
				batteryVisualLogic.gameObject.SetActive(value: true);
				BatteryUpdateBars(batteryLifeInt);
			}
		}
		else if (!showBattery)
		{
			batteryOutBlinkTimer = 0f;
			if ((bool)batteryVisualLogic && batteryVisualLogic.gameObject.activeSelf && !isCharging)
			{
				batteryVisualLogic.BatteryOutro();
				BatteryUpdateBars(batteryLifeInt);
			}
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer() || string.IsNullOrEmpty(itemAttributes.instanceName))
		{
			return;
		}
		batteryLifeCountBars = (int)Mathf.Round(batteryLife / (float)(100 / batteryBars));
		if (batteryLifeCountBars != batteryLifeCountBarsPrev)
		{
			bool charge = false;
			if (batteryLifeCountBarsPrev < batteryLifeCountBars)
			{
				charge = true;
			}
			BatteryFullPercentChange(batteryLifeCountBars, charge);
			batteryLifeCountBarsPrev = batteryLifeCountBars;
		}
	}

	public void RemoveFullBar(int _bars)
	{
		if (!SemiFunc.RunIsShop() && batteryLifeInt > 0)
		{
			batteryLifeInt -= _bars;
			if (batteryLifeInt <= 0)
			{
				batteryLifeInt = 0;
				batteryLife = 0f;
			}
			else
			{
				batteryLife = batteryLifeInt * (100 / batteryBars);
			}
			BatteryFullPercentChange(batteryLifeInt);
		}
	}

	public void BatteryToggle(bool toggle)
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				photonView.RPC("BatteryToggleRPC", RpcTarget.All, toggle);
			}
		}
		else
		{
			BatteryToggleRPC(toggle);
		}
	}

	[PunRPC]
	public void BatteryToggleRPC(bool toggle)
	{
		batteryActive = toggle;
	}

	private void BatteryLookAt()
	{
		if (batteryVisualLogic.cameraTurn && (showBattery || batteryActive || isCharging))
		{
			batteryTransform.LookAt(mainCamera.transform);
			float num = Vector3.Distance(batteryTransform.position, mainCamera.transform.position);
			batteryTransform.localScale = Vector3.one * num * 0.8f;
			if (batteryTransform.localScale.x > 3f)
			{
				batteryTransform.localScale = Vector3.one * 3f;
			}
			batteryTransform.Rotate(0f, 180f, 0f);
			batteryTransform.position = base.transform.position + Vector3.up * upOffset;
		}
	}

	private void BatteryChargingVisuals()
	{
		if (!isCharging)
		{
			return;
		}
		if (!batteryVisualLogic.gameObject.activeSelf)
		{
			batteryVisualLogic.gameObject.SetActive(value: true);
		}
		chargingBlinkTimer += Time.deltaTime;
		if (chargingBlinkTimer > 0.5f)
		{
			chargingBlink = !chargingBlink;
			if (chargingBlink)
			{
				BatteryUpdateBars(batteryLifeInt + 1);
			}
			else
			{
				BatteryUpdateBars(batteryLifeInt);
			}
			chargingBlinkTimer = 0f;
		}
	}

	private void BatteryChargeToggle(bool toggle)
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				photonView.RPC("BatteryChargeStartRPC", RpcTarget.All, toggle);
			}
		}
		else
		{
			BatteryChargeStartRPC(toggle);
		}
	}

	[PunRPC]
	private void BatteryChargeStartRPC(bool toggle)
	{
		isCharging = toggle;
		BatteryUpdateBars(batteryLifeInt);
	}

	private void BatteryUpdateBars(int batteryLevel)
	{
		if ((bool)batteryVisualLogic)
		{
			currentBars = batteryLifeInt;
			batteryVisualLogic.BatteryBarsUpdate();
		}
	}

	private void BatteryFullPercentChangeLogic(int batteryLevel, bool charge)
	{
		if (batteryLifeInt > batteryLevel && batteryLevel == 1 && batteryActive)
		{
			AssetManager.instance.batteryLowWarning.Play(base.transform.position);
		}
		batteryLifeInt = batteryLevel;
		if (batteryLifeInt != 0)
		{
			batteryLife = batteryLifeInt * (100 / batteryBars);
		}
		else
		{
			batteryLife = 0f;
		}
		SemiFunc.StatSetBattery(itemAttributes.instanceName, (int)batteryLife);
		BatteryUpdateBars(batteryLifeInt);
		if (batteryActive || charge)
		{
			if (charge)
			{
				AssetManager.instance.batteryChargeSound.Play(base.transform.position);
			}
			else
			{
				AssetManager.instance.batteryDrainSound.Play(base.transform.position);
			}
		}
	}

	private void OnDisable()
	{
	}

	private void OnEnable()
	{
	}

	private void BatteryFullPercentChange(int batteryLifeInt, bool charge = false)
	{
		if (GameManager.instance.gameMode == 0)
		{
			BatteryFullPercentChangeLogic(batteryLifeInt, charge);
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("BatteryFullPercentChangeRPC", RpcTarget.All, batteryLifeInt, charge);
		}
	}

	[PunRPC]
	private void BatteryFullPercentChangeRPC(int batteryLifeInt, bool charge)
	{
		BatteryFullPercentChangeLogic(batteryLifeInt, charge);
	}

	public void Drain(float amount)
	{
		drainRate = amount;
		drainTimer = 0.1f;
	}
}
