using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ChargingStation : MonoBehaviour
{
	public static ChargingStation instance;

	private int maxCrystals = 10;

	private int energyPerCrystal = 10;

	private PhotonView photonView;

	private Transform chargeBar;

	internal int chargeTotal = 100;

	private float chargeFloat = 1f;

	private float chargeScale = 1f;

	private float chargeScaleTarget = 1f;

	internal int chargeInt;

	private int chargeSegments = 40;

	private int chargeSegmentCurrent;

	private float chargeRate = 0.05f;

	public AnimationCurve chargeCurve;

	private float chargeCurveTime;

	private Transform chargeArea;

	private float chargeAreaCheckTimer;

	private List<ItemBattery> itemsCharging = new List<ItemBattery>();

	private Transform lockedTransform;

	public Light barLight;

	public GameObject meshObject;

	private Material chargingStationEmissionMaterial;

	private bool isCharging;

	private Light light1;

	private Light light2;

	public Sound soundStart;

	public Sound soundStop;

	public Sound soundLoop;

	public Transform crystalCylinder;

	public List<Transform> crystals = new List<Transform>();

	public ParticleSystem lightParticle;

	public ParticleSystem fireflyParticles;

	public ParticleSystem bitsParticles;

	public Sound soundPowerCrystalBreak;

	private float crystalCooldown;

	public Item item;

	public GameObject subtleLight;

	private int lastSentSegment = -1;

	private byte lastSentCrystals = byte.MaxValue;

	private float chargeTimer;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		chargeRate = 1f / (float)energyPerCrystal / (float)chargeTotal;
		foreach (Transform item in crystalCylinder)
		{
			crystals.Add(item);
		}
		chargingStationEmissionMaterial = meshObject.GetComponent<Renderer>().material;
		chargeBar = base.transform.Find("Charge");
		photonView = GetComponent<PhotonView>();
		chargeArea = base.transform.Find("Charge Area");
		lockedTransform = base.transform.Find("Locked");
		light1 = base.transform.Find("Light1").GetComponent<Light>();
		light2 = base.transform.Find("Light2").GetComponent<Light>();
		chargeTimer = 999f;
		if (!SemiFunc.RunIsShop())
		{
			if ((bool)lockedTransform)
			{
				Object.Destroy(lockedTransform.gameObject);
			}
		}
		else
		{
			if ((bool)subtleLight)
			{
				Object.Destroy(subtleLight);
			}
			if ((bool)chargeArea)
			{
				Object.Destroy(chargeArea.gameObject);
			}
			if ((bool)chargeBar)
			{
				Object.Destroy(chargeBar.gameObject);
			}
			Object.Destroy(light1.gameObject);
			Object.Destroy(light2.gameObject);
		}
		chargeInt = SemiFunc.StatGetItemsPurchased("Item Power Crystal");
		int num = maxCrystals + 1;
		int num2 = Mathf.RoundToInt((float)(chargeInt * energyPerCrystal / maxCrystals) * (float)energyPerCrystal);
		chargeTotal = StatsManager.instance.runStats["chargingStationChargeTotal"];
		if (chargeTotal > num2)
		{
			chargeTotal = num2;
		}
		StatsManager.instance.runStats["chargingStationChargeTotal"] = chargeTotal;
		int num3 = Mathf.Clamp(Mathf.CeilToInt((float)chargeTotal / (float)energyPerCrystal), 0, maxCrystals);
		while (crystals.Count > num3 && num > 0 && crystals.Count != 0)
		{
			if ((bool)crystals[0])
			{
				Object.Destroy(crystals[0].gameObject);
			}
			crystals.RemoveAt(0);
			num--;
		}
		chargeFloat = (float)chargeTotal / 100f;
		chargeSegmentCurrent = Mathf.RoundToInt(chargeFloat * (float)chargeSegments);
		if (chargeInt <= 0)
		{
			OutOfCrystalsShutdown();
		}
		chargeScale = chargeFloat;
		chargeScaleTarget = (float)chargeSegmentCurrent / (float)chargeSegments;
		if ((bool)chargeBar)
		{
			chargeBar.localScale = new Vector3(chargeScale, 1f, 1f);
		}
		num = maxCrystals + 1;
		while (crystals.Count > chargeInt && num > 0 && crystals.Count != 0)
		{
			if ((bool)crystals[0])
			{
				Object.Destroy(crystals[0].gameObject);
			}
			crystals.RemoveAt(0);
			num--;
		}
		if (crystals.Count == 0)
		{
			OutOfCrystalsShutdown();
		}
		if (RunManager.instance.levelsCompleted < 1)
		{
			Object.Destroy(base.gameObject);
		}
		StartCoroutine(MissionText());
	}

	private void OutOfCrystalsShutdown()
	{
		chargingStationEmissionMaterial.SetColor("_EmissionColor", Color.black);
		if ((bool)light1)
		{
			light1.enabled = false;
		}
		if ((bool)light2)
		{
			light2.enabled = false;
		}
		if ((bool)subtleLight)
		{
			Color color = new Color(0.1f, 0.1f, 0.2f);
			subtleLight.GetComponent<Light>().color = color;
		}
	}

	public IEnumerator MissionText()
	{
		while (LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(2f);
		if (SemiFunc.RunIsLobby())
		{
			SemiFunc.UIFocusText("Enjoy the ride, recharge stuff and GEAR UP!", Color.white, AssetManager.instance.colorYellow);
		}
	}

	private void StopCharge()
	{
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("StopChargeRPC", RpcTarget.All);
		}
		else
		{
			StopChargeRPC();
		}
	}

	[PunRPC]
	public void StopChargeRPC()
	{
		soundStop.Play(base.transform.position);
		isCharging = false;
	}

	private void StartCharge()
	{
		if (SemiFunc.IsMasterClient())
		{
			photonView.RPC("StartChargeRPC", RpcTarget.All);
		}
		else
		{
			StartChargeRPC();
		}
	}

	[PunRPC]
	public void StartChargeRPC()
	{
		soundStart.Play(base.transform.position);
		isCharging = true;
	}

	private void CrystalsItShouldHave()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			int num = Mathf.Clamp(Mathf.CeilToInt((float)chargeTotal / (float)energyPerCrystal), 0, maxCrystals);
			if (crystals.Count > num)
			{
				DestroyCrystal();
			}
		}
	}

	private void ChargeAreaCheck()
	{
		if (SemiFunc.RunIsShop() || RunManager.instance.levelIsShop)
		{
			return;
		}
		if (chargeFloat <= 0f)
		{
			if (isCharging)
			{
				StopCharge();
			}
			return;
		}
		chargeAreaCheckTimer += Time.deltaTime;
		if (chargeAreaCheckTimer > 0.5f)
		{
			Collider[] array = Physics.OverlapBox(chargeArea.position, chargeArea.localScale / 2f, chargeArea.localRotation, SemiFunc.LayerMaskGetPhysGrabObject());
			itemsCharging.Clear();
			Collider[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				ItemBattery componentInParent = array2[i].GetComponentInParent<ItemBattery>();
				if ((bool)componentInParent && componentInParent.batteryLifeInt < componentInParent.batteryBars && !itemsCharging.Contains(componentInParent))
				{
					itemsCharging.Add(componentInParent);
				}
			}
			chargeAreaCheckTimer = 0f;
		}
		bool flag = false;
		float num = 0.05f;
		if (isCharging)
		{
			chargeTimer += Time.deltaTime;
		}
		foreach (ItemBattery item in itemsCharging)
		{
			if (item.batteryLifeInt < item.batteryBars)
			{
				item.ChargeBattery(base.gameObject, 10f);
				if (chargeTimer >= num)
				{
					chargeFloat = Mathf.Max(0f, chargeFloat - chargeRate);
					chargeTotal = Mathf.Clamp((int)(chargeFloat * 100f), 0, 100);
				}
				flag = true;
				if (!isCharging)
				{
					StartCharge();
				}
			}
		}
		if (chargeTimer > num)
		{
			chargeTimer = 0f;
		}
		if (flag)
		{
			StatsManager.instance.runStats["chargingStationCharge"] = chargeInt;
			StatsManager.instance.runStats["chargingStationChargeTotal"] = chargeTotal;
		}
		if (isCharging)
		{
			CrystalsItShouldHave();
		}
		int num2 = Mathf.CeilToInt(chargeFloat * (float)chargeSegments);
		if (SemiFunc.IsMasterClient() && num2 != lastSentSegment)
		{
			lastSentSegment = num2;
			photonView.RPC("ChargingStationSegmentChangedRPC", RpcTarget.AllBuffered, (byte)num2);
		}
		if (!flag && isCharging)
		{
			StopCharge();
		}
	}

	[PunRPC]
	private void ChargingStationSegmentChangedRPC(byte segment)
	{
		chargeSegmentCurrent = segment;
		chargeFloat = (float)(int)segment / (float)chargeSegments;
		chargeTotal = Mathf.RoundToInt(chargeFloat * 100f);
		lastSentSegment = segment;
	}

	private void ChargingEffects()
	{
		if (isCharging)
		{
			TutorialDirector.instance.playerUsedChargingStation = true;
			crystalCylinder.localRotation = Quaternion.Euler(90f, 0f, Mathf.PingPong(Time.time * 150f, 5f) - 2.5f);
			int num = 0;
			foreach (Transform crystal in crystals)
			{
				if ((bool)crystal)
				{
					num++;
					float value = 0.1f + Mathf.PingPong((Time.time + (float)num) * 5f, 1f);
					Color value2 = Color.yellow * Mathf.LinearToGammaSpace(value);
					crystal.GetComponent<Renderer>().material.SetColor("_EmissionColor", value2);
				}
			}
			crystalCooldown = 0f;
			return;
		}
		crystalCylinder.localRotation = Quaternion.Euler(90f, 0f, 0f);
		foreach (Transform crystal2 in crystals)
		{
			if ((bool)crystal2)
			{
				crystalCooldown += Time.deltaTime * 0.5f;
				float num2 = chargeCurve.Evaluate(crystalCooldown);
				float value3 = Mathf.Lerp(1f, 0.1f, num2);
				Color value4 = Color.yellow * Mathf.LinearToGammaSpace(value3);
				crystal2.GetComponent<Renderer>().material.SetColor("_EmissionColor", value4);
				crystalCylinder.localRotation = Quaternion.Euler(90f, 0f, (Mathf.PingPong(Time.time * 250f, 10f) - 5f) * (1f - num2));
			}
		}
	}

	private void Update()
	{
		if (SemiFunc.RunIsShop() || RunManager.instance.levelIsShop)
		{
			return;
		}
		if (chargeTotal <= 0)
		{
			CrystalsItShouldHave();
		}
		chargeScaleTarget = (float)chargeSegmentCurrent / (float)chargeSegments;
		barLight.intensity = Mathf.Min(2.5f, chargeScaleTarget * 2.5f);
		soundLoop.PlayLoop(isCharging, 2f, 2f);
		AnimateChargeBar();
		ChargingEffects();
		if (isCharging && crystals.Count > 0)
		{
			float num = 0.5f + Mathf.PingPong(Time.time * 5f, 0.5f);
			Color value = Color.yellow * Mathf.LinearToGammaSpace(num);
			chargingStationEmissionMaterial.SetColor("_EmissionColor", value);
			if ((bool)light1 && (bool)light2)
			{
				light1.enabled = true;
				light2.enabled = true;
				light1.intensity = num;
				light2.intensity = num;
			}
		}
		else if ((bool)light1 && (bool)light2)
		{
			chargingStationEmissionMaterial.SetColor("_EmissionColor", Color.black);
			light1.enabled = false;
			light2.enabled = false;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && !RunManager.instance.restarting)
		{
			ChargeAreaCheck();
			chargeSegmentCurrent = Mathf.RoundToInt(chargeFloat * (float)chargeSegments);
		}
	}

	private void AnimateChargeBar()
	{
		if ((bool)chargeBar && !Mathf.Approximately(chargeBar.localScale.x, chargeScaleTarget))
		{
			chargeCurveTime += Time.deltaTime;
			chargeScale = Mathf.Lerp(chargeScale, chargeScaleTarget, chargeCurve.Evaluate(chargeCurveTime));
			chargeBar.localScale = new Vector3(chargeScale, 1f, 1f);
		}
	}

	private void DestroyCrystal()
	{
		if (crystals.Count >= 1 && SemiFunc.IsMasterClientOrSingleplayer())
		{
			lastSentCrystals = (byte)crystals.Count;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("ChargingStationCrystalBrokenRPC", RpcTarget.AllBuffered);
			}
			else
			{
				ChargingStationCrystalBrokenRPC();
			}
		}
	}

	[PunRPC]
	private void ChargingStationCrystalBrokenRPC()
	{
		chargeInt = Mathf.Max(chargeInt - 1, 0);
		StatsManager.instance.runStats["chargingStationCharge"] = chargeInt;
		StatsManager.instance.SetItemPurchase(item, StatsManager.instance.GetItemPurchased(item) - 1);
		if (crystals.Count != 0)
		{
			Transform transform = crystals[0];
			Vector3 position = Vector3.zero;
			if ((bool)transform)
			{
				position = transform.position + transform.up * 0.1f;
				lightParticle.transform.position = position;
				fireflyParticles.transform.position = position;
				bitsParticles.transform.position = position;
			}
			lightParticle.Play();
			fireflyParticles.Play();
			bitsParticles.Play();
			crystals.RemoveAt(0);
			if ((bool)transform)
			{
				soundPowerCrystalBreak.Play(position);
				Object.Destroy(transform.gameObject);
			}
			lastSentCrystals = (byte)crystals.Count;
			if (crystals.Count == 0)
			{
				OutOfCrystalsShutdown();
			}
		}
	}
}
