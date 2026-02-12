using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ToiletFun : MonoBehaviour
{
	internal Transform sphereChecker;

	internal List<PhysGrabObject> physGrabObjects = new List<PhysGrabObject>();

	internal List<PlayerAvatar> playerAvatars = new List<PlayerAvatar>();

	private PlayerController playerController;

	internal float checkTimer;

	private PhotonView photonView;

	private float toiletCharge;

	private float explosionTimer;

	private float hurtColliderTimer;

	private bool toiletActive;

	public GameObject hurtCollider;

	public Transform explosion;

	public Sound soundLoop;

	public Sound soundExplosion;

	public Sound soundFlush;

	public Sound soundSplash;

	private float safetyTimer;

	public ParticleSystem smallParticles;

	public ParticleSystem bigParticles;

	public ParticleSystem splashBigParticles;

	public ParticleSystem splashSmallParticles;

	private float splashTimer;

	private float randomForceTimer;

	public Rigidbody hingeRigidBody;

	private float hingeRattlingTimer;

	private void Start()
	{
		sphereChecker = GetComponentInChildren<SphereCollider>().transform;
		photonView = GetComponent<PhotonView>();
	}

	private void FixedUpdate()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		bool flag = false;
		foreach (PhysGrabObject physGrabObject in physGrabObjects)
		{
			if (!physGrabObject)
			{
				continue;
			}
			if (toiletActive)
			{
				Rigidbody component = physGrabObject.GetComponent<Rigidbody>();
				if ((bool)component)
				{
					component.AddTorque(Vector3.up * toiletCharge, ForceMode.Impulse);
					if (randomForceTimer <= 0f)
					{
						Vector3 vector = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
						component.AddForce(vector * toiletCharge, ForceMode.Impulse);
					}
				}
			}
			float num = Vector3.Distance(physGrabObject.midPoint, sphereChecker.position);
			if ((physGrabObject.impactHappenedTimer > 0f || physGrabObject.impactLightTimer > 0f || physGrabObject.impactHeavyTimer > 0f || physGrabObject.impactMediumTimer > 0f) && num < sphereChecker.localScale.x)
			{
				flag = true;
			}
		}
		foreach (PlayerAvatar playerAvatar in playerAvatars)
		{
			if ((bool)playerAvatar && toiletActive)
			{
				playerAvatar.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
				playerAvatar.tumble.TumbleOverrideTime(10f);
			}
		}
		if ((bool)playerController && toiletActive)
		{
			playerController.playerAvatarScript.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
			playerController.playerAvatarScript.tumble.TumbleOverrideTime(10f);
		}
		if (splashTimer <= 0f && flag)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("SplashRPC", RpcTarget.All);
			}
			else
			{
				Splash();
			}
			splashTimer = 0.5f;
		}
		if (!toiletActive)
		{
			return;
		}
		if (hingeRattlingTimer <= 0f)
		{
			if ((bool)hingeRigidBody && !hingeRigidBody.GetComponent<PhysGrabHinge>().broken)
			{
				hingeRigidBody.AddForce(-Vector3.up * 2f * toiletCharge * 0.5f, ForceMode.Impulse);
			}
			hingeRattlingTimer = 0.1f;
		}
		else
		{
			hingeRattlingTimer -= Time.deltaTime;
		}
	}

	private void Update()
	{
		float num = 1f;
		num = 1f + toiletCharge / 8f * 2f;
		soundLoop.LoopPitch = num;
		soundLoop.PlayLoop(toiletActive, 1f, 1f);
		float num2 = 1f;
		if (toiletActive)
		{
			num2 = 1f + toiletCharge * 20f;
			GameDirector.instance.CameraShake.ShakeDistance(num2 / 30f, 0f, 3f, base.transform.position, 0.5f);
		}
		if ((!SemiFunc.IsMultiplayer() && explosionTimer <= 0f) || (SemiFunc.IsMasterClientOrSingleplayer() && explosionTimer <= 0f))
		{
			checkTimer += Time.deltaTime;
			if (checkTimer > 1f)
			{
				physGrabObjects.Clear();
				playerAvatars.Clear();
				playerController = null;
				Collider[] array = Physics.OverlapSphere(sphereChecker.position, sphereChecker.localScale.x * 0.5f, SemiFunc.LayerMaskGetPlayersAndPhysObjects(), QueryTriggerInteraction.Ignore);
				foreach (Collider collider in array)
				{
					if ((bool)collider)
					{
						PhysGrabObject componentInParent = collider.GetComponentInParent<PhysGrabObject>();
						if ((bool)componentInParent)
						{
							physGrabObjects.Add(componentInParent);
							break;
						}
						if ((bool)collider.GetComponentInParent<PlayerAvatar>())
						{
							playerAvatars.Add(collider.GetComponentInParent<PlayerAvatar>());
							break;
						}
						if ((bool)collider.GetComponentInParent<PlayerController>())
						{
							playerController = collider.GetComponentInParent<PlayerController>();
							break;
						}
					}
				}
				checkTimer = 0f;
			}
		}
		if (splashTimer > 0f)
		{
			splashTimer -= Time.deltaTime;
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
		if (toiletActive)
		{
			if (randomForceTimer > 0f)
			{
				randomForceTimer -= Time.deltaTime;
			}
			else
			{
				randomForceTimer = Random.Range(0.5f, 2f);
			}
			if (!smallParticles.isPlaying)
			{
				smallParticles.Play();
			}
			if (!bigParticles.isPlaying)
			{
				bigParticles.Play();
			}
			toiletCharge += Time.deltaTime * 2f;
			toiletCharge = Mathf.Clamp(toiletCharge, 0f, 8f);
			if (toiletCharge > 7.5f)
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
		}
		else
		{
			toiletCharge -= Time.deltaTime * 20f;
			toiletCharge = Mathf.Clamp(toiletCharge, 0f, 8f);
			smallParticles.Stop();
			bigParticles.Stop();
		}
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
		toiletActive = false;
		safetyTimer = 0f;
	}

	[PunRPC]
	public void ExplosionRPC()
	{
		GameDirector.instance.CameraShake.ShakeDistance(8f, 3f, 8f, base.transform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(20f, 3f, 8f, base.transform.position, 0.1f);
		soundExplosion.Play(base.transform.position);
		explosion.gameObject.SetActive(value: true);
		hurtCollider.gameObject.SetActive(value: true);
		explosionTimer = 3f;
		hurtColliderTimer = 0.5f;
		EndCook();
	}

	private void FlushStart()
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("FlushStartRPC", RpcTarget.All);
		}
		else
		{
			FlushStartRPC();
		}
	}

	[PunRPC]
	public void FlushStartRPC()
	{
		soundFlush.Play(base.transform.position);
		toiletActive = true;
	}

	public void Flush()
	{
		if (!toiletActive && SemiFunc.IsMasterClientOrSingleplayer())
		{
			FlushStart();
		}
	}

	[PunRPC]
	public void SplashRPC()
	{
		Splash();
	}

	private void Splash()
	{
		splashBigParticles.Play();
		splashSmallParticles.Play();
		soundSplash.Play(base.transform.position);
		splashTimer = 1f;
	}
}
