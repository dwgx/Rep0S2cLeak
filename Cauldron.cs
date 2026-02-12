using Photon.Pun;
using UnityEngine;

public class Cauldron : MonoBehaviour
{
	public Transform sphereChecker;

	public ParticleSystem sparkParticles;

	public ParticleSystem windParticles;

	public Light lightGreen;

	public GameObject explosion;

	public GameObject hurtCollider;

	private float checkTimer;

	private bool cauldronActive;

	private float explosionTimer;

	private float hurtColliderTimer;

	private PhotonView photonView;

	public Transform liquid;

	private Renderer liquidRenderer;

	public Sound soundExplosion;

	public Sound soundLoop;

	private float safetyTimer;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		if ((bool)liquid)
		{
			liquidRenderer = liquid.GetComponent<Renderer>();
		}
	}

	private void Update()
	{
		if (!liquid)
		{
			return;
		}
		float loopPitch = 1f;
		if (lightGreen.gameObject.activeSelf)
		{
			loopPitch = 1f + lightGreen.intensity / 8f * 5f;
		}
		soundLoop.LoopPitch = loopPitch;
		soundLoop.PlayLoop(cauldronActive, 1f, 1f);
		float num = 1f;
		if (cauldronActive && lightGreen.gameObject.activeSelf)
		{
			num = 1f + lightGreen.intensity * 20f;
			GameDirector.instance.CameraShake.ShakeDistance(num / 30f, 0f, 3f, liquid.position, 0.5f);
		}
		if ((!SemiFunc.IsMultiplayer() && explosionTimer <= 0f) || (SemiFunc.IsMasterClient() && explosionTimer <= 0f))
		{
			checkTimer += Time.deltaTime;
			if (checkTimer > 1f)
			{
				bool flag = cauldronActive;
				cauldronActive = false;
				Collider[] array = Physics.OverlapSphere(sphereChecker.position, sphereChecker.localScale.x * 0.5f, SemiFunc.LayerMaskGetPlayersAndPhysObjects(), QueryTriggerInteraction.Ignore);
				foreach (Collider collider in array)
				{
					if ((bool)collider)
					{
						if ((bool)collider.GetComponentInParent<PhysGrabObject>())
						{
							cauldronActive = true;
							break;
						}
						if ((bool)collider.GetComponentInParent<PlayerAvatar>())
						{
							cauldronActive = true;
							break;
						}
						if ((bool)collider.GetComponentInParent<PlayerController>())
						{
							cauldronActive = true;
							break;
						}
					}
				}
				if (cauldronActive && !flag)
				{
					CookStart();
				}
				if (!cauldronActive && flag)
				{
					if (SemiFunc.IsMultiplayer())
					{
						photonView.RPC("EndCookRPC", RpcTarget.All);
					}
					else
					{
						EndCookRPC();
					}
				}
				checkTimer = 0f;
			}
		}
		if (explosionTimer > 0f)
		{
			explosionTimer -= Time.deltaTime;
			if (explosionTimer <= 0f && (bool)explosion)
			{
				explosion.gameObject.SetActive(value: false);
			}
		}
		if (hurtColliderTimer > 0f)
		{
			hurtColliderTimer -= Time.deltaTime;
			if (hurtColliderTimer <= 0f)
			{
				if ((bool)hurtCollider)
				{
					hurtCollider.SetActive(value: false);
				}
			}
			else if ((bool)hurtCollider)
			{
				hurtCollider.SetActive(value: true);
			}
		}
		if (cauldronActive)
		{
			if (!sparkParticles.isPlaying)
			{
				sparkParticles.Play();
			}
			if (!windParticles.isPlaying)
			{
				windParticles.Play();
			}
			if (!lightGreen.gameObject.activeSelf)
			{
				lightGreen.gameObject.SetActive(value: true);
				lightGreen.intensity = 0f;
				return;
			}
			lightGreen.intensity += Time.deltaTime * 2f;
			lightGreen.intensity = Mathf.Clamp(lightGreen.intensity, 0f, 8f);
			float num2 = Mathf.Abs(lightGreen.intensity / 8f);
			lightGreen.range = 4f + 1f * Mathf.Sin(Time.time * 10f) * num2;
			if (lightGreen.intensity > 7.5f)
			{
				safetyTimer += Time.deltaTime;
				if (safetyTimer > 3f)
				{
					EndCook();
				}
				if (SemiFunc.IsMasterClientOrSingleplayer())
				{
					Explosion();
				}
			}
			return;
		}
		if (lightGreen.gameObject.activeSelf)
		{
			lightGreen.intensity -= Time.deltaTime * 20f;
			lightGreen.intensity = Mathf.Clamp(lightGreen.intensity, 0f, 8f);
			if (lightGreen.intensity < 0.1f)
			{
				lightGreen.gameObject.SetActive(value: false);
			}
		}
		sparkParticles.Stop();
		windParticles.Stop();
	}

	private void Explosion()
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("ExplosionRPC", RpcTarget.All);
		}
		else
		{
			ExplosionRPC();
		}
	}

	[PunRPC]
	public void EndCookRPC()
	{
		EndCook();
	}

	private void EndCook()
	{
		cauldronActive = false;
		safetyTimer = 0f;
	}

	[PunRPC]
	public void ExplosionRPC()
	{
		GameDirector.instance.CameraShake.ShakeDistance(8f, 3f, 8f, liquid.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(20f, 3f, 8f, liquid.position, 0.1f);
		soundExplosion.Play(liquid.position);
		explosion.gameObject.SetActive(value: true);
		hurtCollider.gameObject.SetActive(value: true);
		explosionTimer = 3f;
		hurtColliderTimer = 0.5f;
		EndCook();
	}

	private void CookStart()
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("CookStartRPC", RpcTarget.All);
		}
		else
		{
			CookStartRPC();
		}
	}

	[PunRPC]
	public void CookStartRPC()
	{
		cauldronActive = true;
	}
}
