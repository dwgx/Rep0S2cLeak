using System.Collections;
using Photon.Pun;
using UnityEngine;

public class EnemyTickHealAura : MonoBehaviour
{
	private PhotonView photonView;

	public int healthPool;

	private LayerMask LayerMask;

	private Collider Collider;

	private SphereCollider SphereCollider;

	private float timeUntilDestroy = 4f;

	private float healDelayTimer;

	private float healDelay = 0.4f;

	private float totalDuration = 3f;

	private bool durationBonusApplied;

	private bool disperse;

	private bool disperseImpulse = true;

	public ParticleSystem[] particles;

	public ParticleSystem healImpactParticle;

	public ParticleSystem healImpactSmokeParticles;

	public Sound auraLoop;

	private void Update()
	{
		auraLoop.PlayLoop(!disperse, 5f, 0.5f);
		if (disperse)
		{
			Disperse();
		}
		else
		{
			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				return;
			}
			if (!durationBonusApplied && healthPool > 100)
			{
				totalDuration += (healthPool - 100) / 20;
				durationBonusApplied = true;
			}
			if (healthPool <= 0)
			{
				totalDuration = 0f;
			}
			if (healDelayTimer > 0f)
			{
				healDelayTimer -= Time.deltaTime;
			}
			if (totalDuration <= 0f)
			{
				if (GameManager.Multiplayer())
				{
					photonView.RPC("DisperseRPC", RpcTarget.All);
				}
				else
				{
					DisperseRPC();
				}
			}
			else
			{
				totalDuration -= Time.deltaTime;
			}
		}
	}

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
		SphereCollider = GetComponent<SphereCollider>();
		Collider = SphereCollider;
		LayerMask = (int)SemiFunc.LayerMaskGetPhysGrabObject() + LayerMask.GetMask("Player");
		if (particles.Length != 0)
		{
			ParticleSystem[] array = particles;
			foreach (ParticleSystem particleSystem in array)
			{
				if (!particleSystem.isPlaying)
				{
					particleSystem.Play();
				}
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			StartCoroutine(ColliderCheck());
		}
	}

	private IEnumerator ColliderCheck()
	{
		yield return null;
		while (!LevelGenerator.Instance || !LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		while (true)
		{
			Vector3 center = Collider.bounds.center;
			float radius = base.transform.lossyScale.x * SphereCollider.radius;
			Collider[] array = Physics.OverlapSphere(center, radius, LayerMask, QueryTriggerInteraction.Collide);
			if (array.Length != 0)
			{
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					if (collider.gameObject.CompareTag("Player"))
					{
						PlayerAvatar playerAvatar = collider.gameObject.GetComponentInParent<PlayerAvatar>();
						if (!playerAvatar)
						{
							PlayerController componentInParent = collider.gameObject.GetComponentInParent<PlayerController>();
							if ((bool)componentInParent)
							{
								playerAvatar = componentInParent.playerAvatarScript;
							}
						}
						if ((bool)playerAvatar)
						{
							PlayerHeal(playerAvatar);
						}
					}
					if (collider.gameObject.CompareTag("Phys Grab Object") && (bool)collider.gameObject.GetComponentInParent<PhysGrabObject>())
					{
						bool flag = false;
						PlayerTumble componentInParent2 = collider.gameObject.GetComponentInParent<PlayerTumble>();
						if ((bool)componentInParent2)
						{
							flag = true;
						}
						if (flag)
						{
							PlayerHeal(componentInParent2.playerAvatar);
						}
					}
				}
			}
			yield return new WaitForSeconds(0.05f);
		}
	}

	private void PlayerHeal(PlayerAvatar _player)
	{
		int num = _player.playerHealth.maxHealth - _player.playerHealth.health;
		if (healDelayTimer <= 0f && healthPool > 0 && num > 0)
		{
			if (num < 10)
			{
				_player.playerHealth.HealOther(num, effect: true);
				HealthEffects();
				healthPool -= num;
				healDelayTimer = healDelay;
			}
			else if (healthPool > 10)
			{
				_player.playerHealth.HealOther(10, effect: true);
				HealthEffects();
				healthPool -= 10;
				healDelayTimer = healDelay;
			}
			else
			{
				_player.playerHealth.HealOther(healthPool, effect: true);
				HealthEffects();
				healthPool = 0;
				healDelayTimer = healDelay;
			}
		}
	}

	private void Disperse()
	{
		if (disperseImpulse)
		{
			if (particles.Length != 0)
			{
				ParticleSystem[] array = particles;
				foreach (ParticleSystem particleSystem in array)
				{
					if ((bool)particleSystem)
					{
						if (particleSystem.isPlaying)
						{
							particleSystem.Stop();
						}
						particleSystem.gameObject.transform.parent = null;
					}
				}
			}
			if ((bool)healImpactParticle)
			{
				healImpactParticle.gameObject.transform.parent = null;
			}
			if ((bool)healImpactSmokeParticles)
			{
				healImpactSmokeParticles.gameObject.transform.parent = null;
			}
			disperseImpulse = false;
			StopAllCoroutines();
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		timeUntilDestroy -= Time.deltaTime;
		if (timeUntilDestroy <= 0f)
		{
			if (!SemiFunc.IsMultiplayer())
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				PhotonNetwork.Destroy(base.gameObject);
			}
		}
	}

	private void HealthEffects()
	{
		if (GameManager.Multiplayer())
		{
			photonView.RPC("HealEffectsRPC", RpcTarget.All);
		}
		else
		{
			HealEffectsRPC();
		}
	}

	[PunRPC]
	private void DisperseRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			disperse = true;
		}
	}

	[PunRPC]
	private void HealEffectsRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			if ((bool)healImpactParticle)
			{
				healImpactParticle.Play();
			}
			if ((bool)healImpactSmokeParticles)
			{
				healImpactSmokeParticles.Play();
			}
		}
	}
}
