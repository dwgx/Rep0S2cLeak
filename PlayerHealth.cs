using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
	public enum Effect
	{
		Upgrade
	}

	public enum EyeOverrideState
	{
		None,
		Red,
		Green,
		Love,
		CeilingEye,
		Inverted
	}

	public bool isMenuAvatar;

	private PlayerExpression playerExpression;

	private PlayerAvatar playerAvatar;

	internal PhotonView photonView;

	internal bool hurtFreeze;

	private float hurtFreezeTimer;

	private bool healthSet;

	internal int health = 100;

	private int healthPrevious;

	internal int maxHealth = 100;

	internal bool godMode;

	public Transform meshParent;

	public Light eyeLight;

	private List<MeshRenderer> renderers;

	private List<Material> sharedMaterials = new List<Material>();

	internal List<Material> instancedMaterials = new List<Material>();

	private int materialHurtAmount;

	private int materialHurtColor;

	internal Material bodyMaterial;

	internal Material eyeMaterial;

	internal Material pupilMaterial;

	private Material healthMaterial;

	private int healthMaterialAmount;

	public Gradient healthMaterialColor;

	private bool materialEffect;

	private Color materialEffectColor;

	private AnimationCurve materialEffectCurve;

	private float materialEffectLerp;

	public Sound hurtOther;

	public Sound healOther;

	public Sound upgradeOther;

	private float overrideEyeMaterialLerp;

	private float overrideEyeMaterialLerpPrevious;

	private float overrideEyeMaterialTimer;

	private Color overrideEyeMaterialColor;

	private Color overridePupilMaterialColor;

	private Color overrideEyeLightColor;

	private float overrideEyeLightIntensity;

	private bool overrideEyeActive;

	private bool overrideEyeActivePrevious;

	private int overrideEyePriority = -999;

	private EyeOverrideState overrideEyeState;

	private EyeOverrideState overrideEyeStatePrevious;

	private float invincibleTimer;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
		if (!isMenuAvatar)
		{
			playerAvatar = GetComponent<PlayerAvatar>();
		}
		else
		{
			playerAvatar = PlayerAvatar.instance;
		}
		if (!isMenuAvatar && !SemiFunc.RunIsLobbyMenu() && (!GameManager.Multiplayer() || photonView.IsMine))
		{
			StartCoroutine(Fetch());
		}
		materialEffectCurve = AssetManager.instance.animationCurveImpact;
		renderers = new List<MeshRenderer>();
		renderers.AddRange(meshParent.GetComponentsInChildren<MeshRenderer>(includeInactive: true));
		foreach (MeshRenderer renderer in renderers)
		{
			Material material = null;
			foreach (Material sharedMaterial in sharedMaterials)
			{
				if (renderer.sharedMaterial.name == sharedMaterial.name)
				{
					material = sharedMaterial;
					renderer.sharedMaterial = instancedMaterials[sharedMaterials.IndexOf(sharedMaterial)];
				}
			}
			if (!material)
			{
				string text = renderer.sharedMaterial.name;
				material = renderer.sharedMaterial;
				sharedMaterials.Add(material);
				instancedMaterials.Add(renderer.material);
				if (text == "Player Avatar - Body")
				{
					bodyMaterial = renderer.sharedMaterial;
				}
				if (text == "Player Avatar - Health")
				{
					healthMaterial = renderer.sharedMaterial;
				}
				if (text == "Player Avatar - Eye")
				{
					eyeMaterial = renderer.sharedMaterial;
				}
				if (text == "Player Avatar - Pupil")
				{
					pupilMaterial = renderer.sharedMaterial;
				}
			}
		}
		materialHurtColor = Shader.PropertyToID("_ColorOverlay");
		materialHurtAmount = Shader.PropertyToID("_ColorOverlayAmount");
		healthMaterialAmount = Shader.PropertyToID("_OffsetX");
	}

	private void Start()
	{
		if ((bool)DebugCommandHandler.instance && DebugCommandHandler.instance.godMode)
		{
			if (SemiFunc.IsMainMenu())
			{
				DebugCommandHandler.instance.godMode = false;
			}
			else
			{
				godMode = true;
			}
		}
	}

	private IEnumerator Fetch()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		int num = StatsManager.instance.GetPlayerHealth(SemiFunc.PlayerGetSteamID(playerAvatar));
		if (num <= 0)
		{
			num = 1;
		}
		health = num;
		maxHealth = 100 + StatsManager.instance.GetPlayerMaxHealth(SemiFunc.PlayerGetSteamID(playerAvatar));
		health = Mathf.Clamp(health, 0, maxHealth);
		if (SemiFunc.RunIsArena())
		{
			health = maxHealth;
		}
		StatsManager.instance.SetPlayerHealth(SemiFunc.PlayerGetSteamID(playerAvatar), health, setInShop: false);
		if (GameManager.Multiplayer())
		{
			photonView.RPC("UpdateHealthRPC", RpcTarget.Others, health, maxHealth, true);
		}
		healthSet = true;
	}

	private void Update()
	{
		if (playerAvatar.isLocal)
		{
			if (overrideEyeMaterialTimer > 0f)
			{
				overrideEyeMaterialTimer -= Time.deltaTime;
				if (!overrideEyeActive)
				{
					overrideEyeActive = true;
				}
			}
			else if (overrideEyeActive)
			{
				overrideEyeActive = false;
				overrideEyePriority = -999;
			}
			if (SemiFunc.IsMultiplayer() && (overrideEyeActive != overrideEyeActivePrevious || overrideEyeState != overrideEyeStatePrevious))
			{
				overrideEyeActivePrevious = overrideEyeActive;
				overrideEyeStatePrevious = overrideEyeState;
				photonView.RPC("EyeMaterialOverrideRPC", RpcTarget.Others, overrideEyeState, overrideEyeActive);
			}
		}
		if (overrideEyeActive)
		{
			overrideEyeMaterialLerp += 3f * Time.deltaTime;
		}
		else
		{
			overrideEyeMaterialLerp -= 3f * Time.deltaTime;
		}
		overrideEyeMaterialLerp = Mathf.Clamp01(overrideEyeMaterialLerp);
		if (overrideEyeMaterialLerp != overrideEyeMaterialLerpPrevious)
		{
			overrideEyeMaterialLerpPrevious = overrideEyeMaterialLerp;
			float num = AssetManager.instance.animationCurveEaseInOut.Evaluate(overrideEyeMaterialLerp);
			eyeMaterial.SetFloat(materialHurtAmount, num);
			eyeMaterial.SetColor(materialHurtColor, overrideEyeMaterialColor);
			pupilMaterial.SetFloat(materialHurtAmount, num);
			pupilMaterial.SetColor(materialHurtColor, overridePupilMaterialColor);
			if (overrideEyeMaterialLerp <= 0f)
			{
				eyeLight.gameObject.SetActive(value: false);
			}
			else if (!eyeLight.gameObject.activeSelf)
			{
				eyeLight.gameObject.SetActive(value: true);
			}
			eyeLight.color = overrideEyeLightColor;
			eyeLight.intensity = overrideEyeLightIntensity * num;
		}
		if (materialEffect)
		{
			materialEffectLerp += 2.5f * Time.deltaTime;
			materialEffectLerp = Mathf.Clamp01(materialEffectLerp);
			if (playerAvatar.deadSet && !playerAvatar.isDisabled)
			{
				materialEffectLerp = Mathf.Clamp(materialEffectLerp, 0f, 0.1f);
			}
			foreach (Material instancedMaterial in instancedMaterials)
			{
				if (instancedMaterial != eyeMaterial && instancedMaterial != pupilMaterial)
				{
					instancedMaterial.SetFloat(materialHurtAmount, materialEffectCurve.Evaluate(materialEffectLerp));
				}
			}
			if (hurtFreeze && materialEffectLerp > 0.2f)
			{
				hurtFreeze = false;
			}
			if (materialEffectLerp >= 1f)
			{
				materialEffect = false;
				foreach (Material instancedMaterial2 in instancedMaterials)
				{
					if (instancedMaterial2 != eyeMaterial && instancedMaterial2 != pupilMaterial)
					{
						instancedMaterial2.SetFloat(materialHurtAmount, 0f);
					}
				}
			}
			hurtFreezeTimer = 0f;
			if (!overrideEyeActive)
			{
				eyeMaterial.SetFloat(materialHurtAmount, materialEffectCurve.Evaluate(materialEffectLerp));
				eyeMaterial.SetColor(materialHurtColor, Color.white);
				pupilMaterial.SetFloat(materialHurtAmount, materialEffectCurve.Evaluate(materialEffectLerp));
				pupilMaterial.SetColor(materialHurtColor, Color.black);
			}
		}
		else if (hurtFreeze)
		{
			hurtFreezeTimer -= Time.deltaTime;
			if (hurtFreezeTimer <= 0f)
			{
				hurtFreeze = false;
			}
		}
		if (isMenuAvatar)
		{
			health = playerAvatar.playerHealth.health;
		}
		if ((isMenuAvatar || (GameManager.Multiplayer() && !playerAvatar.isLocal)) && healthPrevious != health)
		{
			float num2 = (float)health / (float)maxHealth;
			float num3 = Mathf.Lerp(0.98f, 0f, num2);
			if (num3 <= 0f)
			{
				num3 = -0.5f;
			}
			healthMaterial.SetFloat(healthMaterialAmount, num3);
			int nameID = Shader.PropertyToID("_AlbedoColor");
			int nameID2 = Shader.PropertyToID("_EmissionColor");
			Color value = healthMaterialColor.Evaluate(num2);
			healthMaterial.SetColor(nameID, value);
			value.a = healthMaterial.GetColor(nameID2).a;
			healthMaterial.SetColor(nameID2, value);
			healthPrevious = health;
		}
		if (invincibleTimer > 0f)
		{
			invincibleTimer -= Time.deltaTime;
		}
	}

	public void HurtFreezeOverride(float _time)
	{
		hurtFreeze = true;
		hurtFreezeTimer = _time;
	}

	public void Death()
	{
		health = 0;
		StatsManager.instance.SetPlayerHealth(SemiFunc.PlayerGetSteamID(playerAvatar), health, setInShop: false);
		if (GameManager.Multiplayer())
		{
			photonView.RPC("UpdateHealthRPC", RpcTarget.Others, health, maxHealth, true);
		}
	}

	public void MaterialEffectOverride(Effect _effect)
	{
		if (!GameManager.Multiplayer())
		{
			MaterialEffectOverrideRPC((int)_effect);
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("MaterialEffectOverrideRPC", RpcTarget.All, (int)_effect);
		}
	}

	[PunRPC]
	public void MaterialEffectOverrideRPC(int _effect, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		materialEffect = true;
		materialEffectLerp = 0f;
		Color value = Color.white;
		if (_effect == 0)
		{
			value = new Color(1f, 0.94f, 0f);
			if (!playerAvatar.isLocal)
			{
				upgradeOther.Play(base.transform.position);
			}
		}
		foreach (Material instancedMaterial in instancedMaterials)
		{
			if (instancedMaterial != eyeMaterial && instancedMaterial != pupilMaterial)
			{
				instancedMaterial.SetColor(materialHurtColor, value);
			}
		}
	}

	public void Hurt(int damage, bool savingGrace, int enemyIndex = -1)
	{
		if (invincibleTimer > 0f || damage <= 0 || (GameManager.Multiplayer() && !photonView.IsMine) || playerAvatar.deadSet || godMode || GameDirector.instance.currentState != GameDirector.gameState.Main)
		{
			return;
		}
		if (savingGrace && damage <= 25 && health > 5 && health <= 20)
		{
			health -= damage;
			if (health <= 0)
			{
				health = Random.Range(1, 5);
			}
		}
		else
		{
			health -= damage;
		}
		if (health <= 0)
		{
			playerAvatar.PlayerDeath(enemyIndex);
			health = 0;
			return;
		}
		if ((float)damage >= 25f)
		{
			CameraGlitch.Instance.PlayLongHurt();
		}
		else
		{
			CameraGlitch.Instance.PlayShortHurt();
		}
		StatsManager.instance.SetPlayerHealth(SemiFunc.PlayerGetSteamID(playerAvatar), health, setInShop: false);
		if (GameManager.Multiplayer())
		{
			photonView.RPC("UpdateHealthRPC", RpcTarget.Others, health, maxHealth, true);
		}
	}

	public void HurtOther(int damage, Vector3 hurtPosition, bool savingGrace, int enemyIndex = -1)
	{
		if (!GameManager.Multiplayer())
		{
			Hurt(damage, savingGrace, enemyIndex);
			return;
		}
		photonView.RPC("HurtOtherRPC", RpcTarget.All, damage, hurtPosition, savingGrace, enemyIndex);
	}

	[PunRPC]
	public void HurtOtherRPC(int damage, Vector3 hurtPosition, bool savingGrace, int enemyIndex, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterAndOwnerOnlyRPC(_info, photonView) && photonView.IsMine && (hurtPosition == Vector3.zero || Vector3.Distance(playerAvatar.transform.position, hurtPosition) < 2f))
		{
			Hurt(damage, savingGrace, enemyIndex);
		}
	}

	public void Heal(int healAmount, bool effect = true)
	{
		if (healAmount <= 0 || health == maxHealth || (GameManager.Multiplayer() && !photonView.IsMine) || playerAvatar.isDisabled)
		{
			return;
		}
		if (effect && health != 0)
		{
			if ((float)healAmount >= 25f)
			{
				CameraGlitch.Instance.PlayLongHeal();
			}
			else
			{
				CameraGlitch.Instance.PlayShortHeal();
			}
		}
		health += healAmount;
		health = Mathf.Clamp(health, 0, maxHealth);
		StatsManager.instance.SetPlayerHealth(SemiFunc.PlayerGetSteamID(playerAvatar), health, setInShop: false);
		if (GameManager.Multiplayer())
		{
			photonView.RPC("UpdateHealthRPC", RpcTarget.Others, health, maxHealth, effect);
		}
	}

	public void HealOther(int healAmount, bool effect)
	{
		if (!GameManager.Multiplayer() || photonView.IsMine)
		{
			Heal(healAmount, effect);
			return;
		}
		photonView.RPC("HealOtherRPC", RpcTarget.All, healAmount, effect);
	}

	[PunRPC]
	public void HealOtherRPC(int healAmount, bool effect)
	{
		if (photonView.IsMine)
		{
			Heal(healAmount, effect);
		}
	}

	[PunRPC]
	public void UpdateHealthRPC(int healthNew, int healthMax, bool effect, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterAndOwnerOnlyRPC(_info, photonView))
		{
			return;
		}
		maxHealth = healthMax;
		if (!healthSet)
		{
			health = healthNew;
			healthSet = true;
		}
		else
		{
			if (effect)
			{
				materialEffect = true;
				if (!playerAvatar.deadSet)
				{
					materialEffectLerp = 0f;
				}
				if (healthNew < health || healthNew == 0)
				{
					hurtOther.Play(base.transform.position);
					hurtFreeze = true;
					foreach (Material instancedMaterial in instancedMaterials)
					{
						if (instancedMaterial != eyeMaterial && instancedMaterial != pupilMaterial)
						{
							instancedMaterial.SetColor(materialHurtColor, Color.red);
						}
					}
				}
				else
				{
					if (health != 0)
					{
						healOther.Play(base.transform.position);
					}
					SetMaterialGreen();
				}
			}
			health = healthNew;
		}
		StatsManager.instance.SetPlayerHealth(SemiFunc.PlayerGetSteamID(playerAvatar), health, setInShop: false);
	}

	public void SetMaterialGreen()
	{
		foreach (Material instancedMaterial in instancedMaterials)
		{
			if (instancedMaterial != eyeMaterial && instancedMaterial != pupilMaterial)
			{
				instancedMaterial.SetColor(materialHurtColor, new Color(0f, 1f, 0.25f));
			}
		}
	}

	private void EyeMaterialSetup()
	{
		if (overrideEyeMaterialLerp >= 1f)
		{
			overrideEyeMaterialLerp = 0.8f;
		}
		if (overrideEyeState == EyeOverrideState.Red)
		{
			overrideEyeMaterialColor = Color.red;
			overridePupilMaterialColor = Color.white;
			overrideEyeLightColor = Color.red;
			overrideEyeLightIntensity = 5f;
		}
		else if (overrideEyeState == EyeOverrideState.Green)
		{
			overrideEyeMaterialColor = Color.green;
			overridePupilMaterialColor = Color.white;
			overrideEyeLightColor = Color.green;
			overrideEyeLightIntensity = 5f;
		}
		else if (overrideEyeState == EyeOverrideState.Love)
		{
			overrideEyeMaterialColor = new Color(1f, 0f, 0.5f);
			overridePupilMaterialColor = new Color(0.2f, 0f, 0.2f);
			overrideEyeLightColor = new Color(0.4f, 0f, 0f);
			overrideEyeLightIntensity = 1f;
		}
		else if (overrideEyeState == EyeOverrideState.CeilingEye)
		{
			overrideEyeMaterialColor = new Color(1f, 0.4f, 0f);
			overridePupilMaterialColor = new Color(1f, 1f, 0f);
			overrideEyeLightColor = new Color(1f, 0.4f, 0f);
			overrideEyeLightIntensity = 1f;
		}
		else if (overrideEyeState == EyeOverrideState.Inverted)
		{
			overrideEyeMaterialColor = new Color(0f, 0f, 0f);
			overridePupilMaterialColor = new Color(1f, 1f, 1f);
			overrideEyeLightColor = new Color(0f, 0f, 0f);
			overrideEyeLightIntensity = 1f;
		}
	}

	public void EyeMaterialOverride(EyeOverrideState _state, float _time, int _priority)
	{
		if (_priority >= overrideEyePriority)
		{
			overrideEyePriority = _priority;
			if (overrideEyeState != _state)
			{
				overrideEyeState = _state;
				EyeMaterialSetup();
			}
			overrideEyeMaterialTimer = _time;
		}
	}

	[PunRPC]
	public void EyeMaterialOverrideRPC(EyeOverrideState _state, bool _active)
	{
		overrideEyeActive = _active;
		if (overrideEyeState != _state)
		{
			overrideEyeState = _state;
			EyeMaterialSetup();
		}
	}

	public void InvincibleSet(float _time)
	{
		invincibleTimer = _time;
	}
}
