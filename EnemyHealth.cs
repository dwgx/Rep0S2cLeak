using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
	private PhotonView photonView;

	private Enemy enemy;

	private float damageResistance;

	private float overrideDamageResistanceTimer;

	public int health = 100;

	internal int healthCurrent;

	private bool deadImpulse;

	internal bool dead;

	private float deadImpulseTimer;

	public float deathFreezeTime = 0.1f;

	public bool impactHurt;

	public int impactLightDamage;

	public int impactMediumDamage;

	public int impactHeavyDamage;

	public bool objectHurt;

	public float objectHurtMultiplier = 1f;

	public bool objectHurtStun = true;

	internal float objectHurtStunTime = 2f;

	public Transform meshParent;

	internal List<MeshRenderer> renderers;

	private List<Material> sharedMaterials = new List<Material>();

	internal List<Material> instancedMaterials = new List<Material>();

	public bool spawnValuable = true;

	public int spawnValuableMax = 3;

	internal int spawnValuableCurrent;

	private Color hurtColor = Color.red;

	private Color healColor = new Color(0f, 1f, 0.25f);

	internal Vector3 hurtDirection;

	private bool hurtEffect;

	internal AnimationCurve hurtCurve;

	internal float hurtLerp;

	public UnityEvent onHurt;

	private bool onHurtImpulse;

	public UnityEvent onDeathStart;

	public UnityEvent onDeath;

	public UnityEvent onObjectHurt;

	internal PlayerAvatar onObjectHurtPlayer;

	internal int materialHurtColor;

	internal int materialHurtAmount;

	internal float nonStunHurtTimer;

	internal float objectHurtDisableTimer;

	internal float deathPitCooldown;

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		photonView = GetComponent<PhotonView>();
		healthCurrent = health;
		hurtCurve = AssetManager.instance.animationCurveImpact;
		renderers = new List<MeshRenderer>();
		if ((bool)meshParent)
		{
			renderers.AddRange(meshParent.GetComponentsInChildren<MeshRenderer>(includeInactive: true));
		}
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
				material = renderer.sharedMaterial;
				sharedMaterials.Add(material);
				instancedMaterials.Add(renderer.material);
			}
		}
		materialHurtColor = Shader.PropertyToID("_ColorOverlay");
		materialHurtAmount = Shader.PropertyToID("_ColorOverlayAmount");
		foreach (Material instancedMaterial in instancedMaterials)
		{
			instancedMaterial.SetColor(materialHurtColor, hurtColor);
		}
	}

	private void Update()
	{
		if (overrideDamageResistanceTimer <= 0f)
		{
			damageResistance = 0f;
		}
		if (overrideDamageResistanceTimer > 0f)
		{
			overrideDamageResistanceTimer -= Time.deltaTime;
		}
		if (hurtEffect)
		{
			hurtLerp += 2.5f * Time.deltaTime;
			hurtLerp = Mathf.Clamp01(hurtLerp);
			foreach (Material instancedMaterial in instancedMaterials)
			{
				instancedMaterial.SetFloat(materialHurtAmount, hurtCurve.Evaluate(hurtLerp));
			}
			if (hurtLerp > 1f)
			{
				hurtEffect = false;
				foreach (Material instancedMaterial2 in instancedMaterials)
				{
					instancedMaterial2.SetFloat(materialHurtAmount, 0f);
				}
			}
		}
		if ((!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient) && deadImpulse)
		{
			deadImpulseTimer -= Time.deltaTime;
			if (deadImpulseTimer <= 0f)
			{
				if (!GameManager.Multiplayer())
				{
					DeathImpulseRPC();
				}
				else
				{
					photonView.RPC("DeathImpulseRPC", RpcTarget.All);
				}
			}
		}
		if (nonStunHurtTimer > 0f)
		{
			nonStunHurtTimer -= Time.deltaTime;
		}
		if (objectHurtDisableTimer > 0f)
		{
			objectHurtDisableTimer -= Time.deltaTime;
		}
		if (deathPitCooldown > 0f)
		{
			deathPitCooldown -= Time.deltaTime;
		}
		if (onHurtImpulse)
		{
			onHurt.Invoke();
			onHurtImpulse = false;
		}
	}

	public void OnSpawn()
	{
		if (hurtEffect)
		{
			hurtLerp = 1f;
			hurtEffect = false;
			foreach (Material instancedMaterial in instancedMaterials)
			{
				instancedMaterial.SetFloat(materialHurtAmount, 0f);
			}
		}
		healthCurrent = health;
		dead = false;
	}

	public void LightImpact()
	{
		if (impactHurt && (enemy.IsStunned() || !(nonStunHurtTimer <= 0f)) && impactLightDamage > 0)
		{
			Hurt(impactLightDamage, -enemy.Rigidbody.impactDetector.previousPreviousVelocityRaw.normalized);
		}
	}

	public void MediumImpact()
	{
		if (impactHurt && (enemy.IsStunned() || !(nonStunHurtTimer <= 0f)) && impactMediumDamage > 0)
		{
			Hurt(impactMediumDamage, -enemy.Rigidbody.impactDetector.previousPreviousVelocityRaw.normalized);
		}
	}

	public void HeavyImpact()
	{
		if (impactHurt && (enemy.IsStunned() || !(nonStunHurtTimer <= 0f)) && impactHeavyDamage > 0)
		{
			Hurt(impactHeavyDamage, -enemy.Rigidbody.impactDetector.previousPreviousVelocityRaw.normalized);
		}
	}

	public void Hurt(int _damage, Vector3 _hurtDirection)
	{
		if (!dead && !deadImpulse)
		{
			healthCurrent -= (int)((float)_damage * (1f - Mathf.Min(damageResistance, 1f)));
			if (healthCurrent <= 0)
			{
				healthCurrent = 0;
				Death(_hurtDirection);
			}
			else if (!GameManager.Multiplayer())
			{
				HurtRPC(_damage, _hurtDirection);
			}
			else
			{
				photonView.RPC("HurtRPC", RpcTarget.All, _damage, _hurtDirection);
			}
		}
	}

	[PunRPC]
	public void HurtRPC(int _damage, Vector3 _hurtDirection)
	{
		hurtDirection = _hurtDirection;
		if (hurtDirection == Vector3.zero)
		{
			hurtDirection = Random.insideUnitSphere;
		}
		MaterialColorFlash(hurtColor);
		onHurtImpulse = true;
	}

	public void Heal(int _amount)
	{
		if (dead || deadImpulse || _amount <= 0)
		{
			return;
		}
		int num = healthCurrent;
		healthCurrent += _amount;
		if (healthCurrent != num)
		{
			if (!GameManager.Multiplayer())
			{
				HealRPC();
			}
			else
			{
				photonView.RPC("HealRPC", RpcTarget.All);
			}
		}
	}

	[PunRPC]
	public void HealRPC()
	{
		MaterialColorFlash(healColor);
	}

	private void Death(Vector3 _deathDirection)
	{
		if (!GameManager.Multiplayer())
		{
			DeathRPC(_deathDirection);
			return;
		}
		photonView.RPC("DeathRPC", RpcTarget.All, _deathDirection);
	}

	[PunRPC]
	public void DeathRPC(Vector3 _deathDirection, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			hurtDirection = _deathDirection;
			deadImpulseTimer = deathFreezeTime;
			enemy.Freeze(deathFreezeTime);
			onDeathStart.Invoke();
			deadImpulse = true;
			MaterialColorFlash(hurtColor);
		}
	}

	[PunRPC]
	public void DeathImpulseRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			deadImpulse = false;
			dead = true;
			if (hurtDirection == Vector3.zero)
			{
				hurtDirection = Random.insideUnitSphere;
			}
			onDeath.Invoke();
		}
	}

	public void ObjectHurtDisable(float _time)
	{
		objectHurtDisableTimer = _time;
	}

	public void NonStunHurtOverride(float _time)
	{
		nonStunHurtTimer = _time;
	}

	public void DeathPitCooldown()
	{
		deathPitCooldown = 1f;
	}

	public void OverrideDamageResistance(float _resistance, float _time)
	{
		damageResistance = _resistance;
		overrideDamageResistanceTimer = _time;
	}

	private void MaterialColorFlash(Color color)
	{
		hurtEffect = true;
		hurtLerp = 0f;
		foreach (Material instancedMaterial in instancedMaterials)
		{
			instancedMaterial.SetColor(materialHurtColor, color);
			instancedMaterial.SetFloat(materialHurtAmount, 0f);
		}
	}
}
