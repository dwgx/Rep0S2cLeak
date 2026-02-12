using System.Collections.Generic;
using UnityEngine;

public class ItemOrb : MonoBehaviour
{
	public enum OrbType
	{
		Constant,
		Pulse
	}

	[HideInInspector]
	public SemiFunc.emojiIcon emojiIcon;

	public Texture orbIcon;

	private Material orbEffect;

	public float orbRadius = 1f;

	private float orbRadiusOriginal = 1f;

	private float orbRadiusMultiplier = 1f;

	private Transform orbTransform;

	private Transform orbInnerTransform;

	private ItemToggle itemToggle;

	[HideInInspector]
	public float batteryDrainRate = 0.1f;

	[HideInInspector]
	public bool itemActive;

	private Transform sphereEffectTransform;

	private float sphereEffectScaleLerp;

	private PhysGrabObject physGrabObject;

	internal List<PhysGrabObject> objectAffected = new List<PhysGrabObject>();

	internal bool localPlayerAffected;

	private Transform sphereCheckTransform;

	private float sphereCheckTimer;

	private List<Transform> spherePieces = new List<Transform>();

	private Transform sphereCore;

	[HideInInspector]
	public ColorPresets colorPresets;

	public BatteryDrainPresets batteryDrainPreset;

	[HideInInspector]
	public Color orbColor;

	private Color orbColorLight;

	[HideInInspector]
	public Color batteryColor;

	private ItemBattery itemBattery;

	private float onNoBatteryTimer;

	private ItemEquippable itemEquippable;

	private ItemAttributes itemAttributes;

	public Sound soundOrbBoot;

	public Sound soundOrbShutdown;

	public Sound soundOrbLoop;

	public OrbType orbType;

	public bool targetValuables = true;

	public bool targetPlayers = true;

	public bool targetEnemies = true;

	public bool targetNonValuables = true;

	private ITargetingCondition customTargetingCondition;

	private void Start()
	{
		customTargetingCondition = GetComponent<ITargetingCondition>();
		itemBattery = GetComponent<ItemBattery>();
		itemEquippable = GetComponent<ItemEquippable>();
		itemAttributes = GetComponent<ItemAttributes>();
		emojiIcon = itemAttributes.emojiIcon;
		colorPresets = itemAttributes.colorPreset;
		orbColor = colorPresets.GetColorMain();
		orbColorLight = colorPresets.GetColorLight();
		batteryColor = orbColorLight;
		itemBattery.batteryColor = batteryColor;
		batteryDrainRate = batteryDrainPreset.batteryDrainRate;
		itemBattery.batteryDrainRate = batteryDrainRate;
		itemEquippable.itemEmoji = emojiIcon.ToString();
		ItemLight component = GetComponent<ItemLight>();
		if ((bool)component)
		{
			component.itemLight.color = orbColor;
		}
		Transform transform = base.transform.Find("Item Orb Mesh/Top/Piece1/Orb Icon");
		if ((bool)transform)
		{
			transform.GetComponent<Renderer>().material.SetTexture("_EmissionMap", orbIcon);
			transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", orbColor);
		}
		Transform transform2 = null;
		foreach (Transform item in base.transform)
		{
			if (item.name == "Item Orb Mesh")
			{
				transform2 = item;
			}
		}
		if (transform2 == null)
		{
			Debug.LogWarning("Item Orb Mesh not found in" + base.gameObject.name);
		}
		foreach (Transform item2 in transform2)
		{
			foreach (Transform item3 in item2)
			{
				if (item3.name.Contains("Piece"))
				{
					spherePieces.Add(item3);
					item3.GetComponent<Renderer>().material.SetColor("_EmissionColor", orbColor);
				}
			}
			if (item2.name.Contains("Core"))
			{
				sphereCore = item2;
				item2.GetComponent<Renderer>().material.SetColor("_EmissionColor", orbColorLight);
			}
		}
		sphereEffectTransform = base.transform.Find("sphere effect");
		Material material = base.transform.Find("sphere effect/AreaEffect/effect").GetComponent<Renderer>().material;
		Material material2 = base.transform.Find("sphere effect/AreaEffect/outline_inside").GetComponent<Renderer>().material;
		Material material3 = base.transform.Find("sphere effect/AreaEffect/outline").GetComponent<Renderer>().material;
		Color color = orbColorLight;
		color = new Color(color.r, color.g, color.b, 0.5f);
		Color value = new Color(orbColor.r, orbColor.g, orbColor.b, 0.1f);
		if ((bool)material)
		{
			material.SetColor("_Color", value);
		}
		if ((bool)material2)
		{
			material2.SetColor("_EdgeColor", color);
		}
		if ((bool)material3)
		{
			material3.SetColor("_EdgeColor", color);
		}
		itemToggle = GetComponent<ItemToggle>();
		physGrabObject = GetComponent<PhysGrabObject>();
		sphereEffectTransform.transform.localScale = new Vector3(0f, 0f, 0f);
		sphereEffectTransform.gameObject.SetActive(value: false);
		orbRadiusOriginal = orbRadius;
		physGrabObject.clientNonKinematic = true;
	}

	private void Update()
	{
		if (!SemiFunc.RunIsLevel() && !SemiFunc.RunIsLobby() && !SemiFunc.RunIsShop() && !SemiFunc.RunIsArena() && !SemiFunc.RunIsTutorial())
		{
			return;
		}
		soundOrbLoop.PlayLoop(itemActive, 0.5f, 0.5f);
		if (!itemActive)
		{
			onNoBatteryTimer = 0f;
		}
		if (orbType == OrbType.Constant)
		{
			OrbConstantLogic();
		}
		if (orbType == OrbType.Pulse)
		{
			OrbPulseLogic();
		}
		bool num = itemActive;
		itemActive = itemToggle.toggleState;
		orbRadius = orbRadiusOriginal * orbRadiusMultiplier;
		if (num != itemActive)
		{
			SphereAnimatePiecesBack();
			itemBattery.batteryActive = itemActive;
			if (itemActive)
			{
				soundOrbBoot.Play(base.transform.position);
			}
			else
			{
				soundOrbShutdown.Play(base.transform.position);
			}
		}
		if (itemActive)
		{
			sphereEffectTransform.gameObject.SetActive(value: true);
			if (itemBattery.batteryLife > 0f)
			{
				OrbAnimateAppear();
			}
			SphereAnimatePieces();
			if (itemBattery.batteryLife <= 0f)
			{
				onNoBatteryTimer += Time.deltaTime;
				if (onNoBatteryTimer >= 1.5f)
				{
					itemToggle.ToggleItem(toggle: false);
					onNoBatteryTimer = 0f;
				}
			}
		}
		else
		{
			OrbAnimateDisappear();
		}
		if (sphereEffectTransform.gameObject.activeSelf)
		{
			sphereEffectTransform.rotation = Quaternion.identity;
		}
	}

	private void SphereAnimatePieces()
	{
		int num = 0;
		foreach (Transform spherePiece in spherePieces)
		{
			float num2 = Mathf.Sin(Time.time * 50f + (float)num) * 0.1f;
			spherePiece.localScale = new Vector3(1f + num2, 1f + num2, 1f + num2);
			num++;
		}
		float num3 = Mathf.Sin(Time.time * 30f) * 0.2f;
		sphereCore.localScale = new Vector3(1f + num3, 1f + num3, 1f + num3);
	}

	private void SphereAnimatePiecesBack()
	{
		foreach (Transform spherePiece in spherePieces)
		{
			spherePiece.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	private void OrbConstantLogic()
	{
		if (itemBattery.batteryLifeInt == 0)
		{
			objectAffected.Clear();
			localPlayerAffected = false;
		}
		else
		{
			if (!itemActive)
			{
				return;
			}
			sphereCheckTimer += Time.deltaTime;
			if (!(sphereCheckTimer > 0.1f))
			{
				return;
			}
			objectAffected.Clear();
			sphereCheckTimer = 0f;
			if (itemBattery.batteryLife <= 0f)
			{
				return;
			}
			if (targetEnemies || targetNonValuables || targetValuables)
			{
				objectAffected = SemiFunc.PhysGrabObjectGetAllWithinRange(orbRadius, base.transform.position);
				if (!targetEnemies || !targetNonValuables || !targetValuables)
				{
					List<PhysGrabObject> list = new List<PhysGrabObject>();
					foreach (PhysGrabObject item in objectAffected)
					{
						bool flag = customTargetingCondition != null && customTargetingCondition.CustomTargetingCondition(item.gameObject);
						if (customTargetingCondition == null)
						{
							flag = true;
						}
						if (targetEnemies && item.isEnemy && flag)
						{
							list.Add(item);
						}
						if (targetNonValuables && item.isNonValuable && flag)
						{
							list.Add(item);
						}
						if (targetValuables && item.isValuable && flag)
						{
							list.Add(item);
						}
					}
					objectAffected.Clear();
					objectAffected = list;
				}
			}
			if (targetPlayers)
			{
				localPlayerAffected = SemiFunc.LocalPlayerOverlapCheck(orbRadius, base.transform.position);
			}
		}
	}

	private void OrbPulseLogic()
	{
	}

	private void OrbAnimateAppear()
	{
		float num = Mathf.Lerp(0f, orbRadius, sphereEffectScaleLerp);
		sphereEffectTransform.localScale = new Vector3(num, num, num);
		if (sphereEffectScaleLerp < 1f)
		{
			sphereEffectScaleLerp += 10f * Time.deltaTime;
		}
		else
		{
			sphereEffectScaleLerp = 1f;
		}
	}

	private void OrbAnimateDisappear()
	{
		if (sphereEffectTransform.gameObject.activeSelf)
		{
			float num = Mathf.Lerp(0f, orbRadius, sphereEffectScaleLerp);
			sphereEffectTransform.localScale = new Vector3(num, num, num);
			if (sphereEffectScaleLerp > 0f)
			{
				sphereEffectScaleLerp -= 10f * Time.deltaTime;
				return;
			}
			sphereEffectScaleLerp = 0f;
			sphereEffectTransform.gameObject.SetActive(value: false);
		}
	}
}
