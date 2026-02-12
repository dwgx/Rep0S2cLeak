using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ItemRubberDuck : MonoBehaviour
{
	public Sound soundQuack;

	public Sound soundSqueak;

	public Sound soundDuckLoop;

	public Sound soundDuckExplosion;

	public Sound soundDuckExplosionGlobal;

	private Rigidbody rb;

	private PhotonView photonView;

	private ParticleScriptExplosion particleScriptExplosion;

	private PhysGrabObject physGrabObject;

	public HurtCollider hurtCollider;

	public Transform hurtTransform;

	private float hurtColliderTime;

	private Vector3 prevPosition;

	private bool playDuckLoop;

	private List<TrailRenderer> trails = new List<TrailRenderer>();

	private ItemBattery itemBattery;

	private float trailTimer;

	public GameObject brokenObject;

	public GameObject notBrokenObject;

	private ItemEquippable itemEquippable;

	private float lilQuacksTimer;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		physGrabObject = GetComponent<PhysGrabObject>();
		hurtCollider = GetComponentInChildren<HurtCollider>();
		hurtCollider.gameObject.SetActive(value: false);
		photonView = GetComponent<PhotonView>();
		itemBattery = GetComponent<ItemBattery>();
		itemEquippable = GetComponent<ItemEquippable>();
		TrailRenderer[] componentsInChildren = GetComponentsInChildren<TrailRenderer>();
		foreach (TrailRenderer item in componentsInChildren)
		{
			trails.Add(item);
		}
	}

	private void Update()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && (SemiFunc.RunIsLevel() || SemiFunc.RunIsArena()) && rb.velocity.magnitude < 0.1f)
		{
			if (lilQuacksTimer > 0f)
			{
				lilQuacksTimer -= Time.deltaTime;
				return;
			}
			lilQuacksTimer = Random.Range(1f, 3f);
			LilQuackJump();
		}
	}

	private void FixedUpdate()
	{
		if (itemEquippable.isEquipped || itemEquippable.wasEquippedTimer > 0f)
		{
			prevPosition = rb.position;
			return;
		}
		if (itemBattery.batteryLifeInt == 0)
		{
			if (!brokenObject.activeSelf)
			{
				brokenObject.SetActive(value: true);
				notBrokenObject.SetActive(value: false);
			}
		}
		else if (brokenObject.activeSelf)
		{
			brokenObject.SetActive(value: false);
			notBrokenObject.SetActive(value: true);
		}
		Vector3 vector = (rb.position - prevPosition) / Time.fixedDeltaTime;
		Vector3 normalized = (rb.position - prevPosition).normalized;
		prevPosition = rb.position;
		if (!physGrabObject.grabbed && itemBattery.batteryLife > 0f)
		{
			if (vector.magnitude > 5f)
			{
				playDuckLoop = true;
				trailTimer = 0.2f;
			}
			else
			{
				playDuckLoop = false;
			}
		}
		else
		{
			playDuckLoop = false;
		}
		if (trailTimer > 0f)
		{
			playDuckLoop = true;
			trailTimer -= Time.fixedDeltaTime;
			foreach (TrailRenderer trail in trails)
			{
				trail.emitting = true;
			}
		}
		else
		{
			playDuckLoop = false;
			foreach (TrailRenderer trail2 in trails)
			{
				trail2.emitting = false;
			}
		}
		soundDuckLoop.PlayLoop(playDuckLoop, 2f, 1f);
		if (hurtColliderTime > 0f)
		{
			hurtTransform.forward = normalized;
			if (!hurtCollider.gameObject.activeSelf)
			{
				hurtCollider.gameObject.SetActive(value: true);
				float num = vector.magnitude * 2f;
				if (num > 50f)
				{
					num = 50f;
				}
				hurtCollider.physHitForce = num;
				hurtCollider.physHitTorque = num;
				hurtCollider.enemyHitForce = num;
				hurtCollider.enemyHitTorque = num;
				hurtCollider.playerTumbleForce = num;
				hurtCollider.playerTumbleTorque = num;
			}
			hurtColliderTime -= Time.fixedDeltaTime;
		}
		else if (hurtCollider.gameObject.activeSelf)
		{
			hurtCollider.gameObject.SetActive(value: false);
		}
	}

	private void LilQuackJump()
	{
		if (!(itemBattery.batteryLife <= 0f) && !physGrabObject.grabbed)
		{
			rb.AddForce(Vector3.up * 0.5f, ForceMode.Impulse);
			rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
			rb.AddForce(Random.insideUnitCircle * 0.2f, ForceMode.Impulse);
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("LilQuackJumpRPC", RpcTarget.All);
			}
			else
			{
				LilQuackJumpRPC();
			}
		}
	}

	[PunRPC]
	public void LilQuackJumpRPC()
	{
		if ((bool)physGrabObject)
		{
			soundQuack.Play(physGrabObject.centerPoint);
		}
	}

	public void Squeak()
	{
		if (!(itemBattery.batteryLife <= 0f))
		{
			soundSqueak.Play(base.transform.position);
		}
	}

	public void Quack()
	{
		if (itemBattery.batteryLife <= 0f || physGrabObject.grabbed)
		{
			return;
		}
		soundQuack.Play(base.transform.position);
		hurtColliderTime = 0.2f;
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		itemBattery.batteryLife -= 2.5f;
		if (Random.Range(0, 10) == 0)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("QuackRPC", RpcTarget.All);
			}
			else
			{
				QuackRPC();
			}
		}
		if (rb.velocity.magnitude < 20f)
		{
			rb.velocity *= 5f;
			rb.AddTorque(Random.insideUnitSphere * 40f);
		}
	}

	[PunRPC]
	public void QuackRPC()
	{
		soundDuckExplosionGlobal.Play(base.transform.position);
		soundDuckExplosion.Play(base.transform.position);
		ParticlePrefabExplosion particlePrefabExplosion = particleScriptExplosion.Spawn(base.transform.position, 0.85f, 0, 250, 1f, onlyParticleEffect: false, disableSound: true);
		particlePrefabExplosion.SkipHurtColliderSetup = true;
		particlePrefabExplosion.HurtCollider.playerDamage = 0;
		particlePrefabExplosion.HurtCollider.enemyDamage = 250;
		particlePrefabExplosion.HurtCollider.physImpact = HurtCollider.BreakImpact.Heavy;
		particlePrefabExplosion.HurtCollider.physHingeDestroy = true;
		particlePrefabExplosion.HurtCollider.playerTumbleForce = 30f;
		particlePrefabExplosion.HurtCollider.playerTumbleTorque = 50f;
	}
}
