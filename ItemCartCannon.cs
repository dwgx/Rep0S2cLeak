using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ItemCartCannon : MonoBehaviour
{
	private ItemCartCannonMain cartCannonMain;

	private PhotonView photonView;

	public AnimationCurve animationCurve;

	public GameObject bulletPrefab;

	private PhysGrabObject physGrabObject;

	private ItemBattery itemBattery;

	public Sound soundHit;

	public AnimationCurve animationCurveShoot;

	public AnimationCurve animationCurveShootRecoil;

	public Transform shootAnimationTransform;

	public Transform shootParticlesTransform;

	private Vector3 movingPieceStartPosition;

	private Vector3 movingPieceStartPositionRecoil;

	public Sound buildup1;

	public Sound buildup2;

	public Sound shootSound;

	public Sound shootSoundGlobal;

	private bool animationEvent1;

	private bool animationEvent2;

	public Transform animationEventTransform;

	public Transform shootNozzleRecoilTransform;

	private List<ParticleSystem> particles = new List<ParticleSystem>();

	private List<ParticleSystem> shootParticles = new List<ParticleSystem>();

	private bool doRecoil;

	private float recoilTimer;

	private float recoilTimerMax = 0.4f;

	private ParticleScriptExplosion particleScriptExplosion;

	private int statePrev;

	private int stateCurrent;

	private bool stateStart;

	private void Start()
	{
		movingPieceStartPosition = shootAnimationTransform.localPosition;
		movingPieceStartPositionRecoil = shootNozzleRecoilTransform.localPosition;
		cartCannonMain = GetComponent<ItemCartCannonMain>();
		photonView = GetComponent<PhotonView>();
		bulletPrefab.SetActive(value: false);
		itemBattery = GetComponent<ItemBattery>();
		physGrabObject = GetComponent<PhysGrabObject>();
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		particles = new List<ParticleSystem>(animationEventTransform.GetComponentsInChildren<ParticleSystem>());
		shootParticles = new List<ParticleSystem>(shootParticlesTransform.GetComponentsInChildren<ParticleSystem>());
	}

	private void StateInactive()
	{
		if (stateStart)
		{
			stateStart = false;
		}
	}

	private void StateActive()
	{
		if (stateStart)
		{
			stateStart = false;
		}
	}

	private void StateBuildup()
	{
		if (stateStart)
		{
			buildup1.Play(base.transform.position);
			doRecoil = true;
			animationEvent2 = false;
			animationEvent1 = false;
			stateStart = false;
		}
		if (cartCannonMain.stateTimer > 0.08f && !animationEvent1)
		{
			AnimationEvent1();
		}
		if (cartCannonMain.stateTimer > 0.5f && !animationEvent2)
		{
			AnimationEvent2();
		}
		float num = animationCurveShoot.Evaluate(cartCannonMain.stateTimer / cartCannonMain.stateTimerMax);
		shootAnimationTransform.localPosition = new Vector3(movingPieceStartPosition.x, movingPieceStartPosition.y, movingPieceStartPosition.z - movingPieceStartPosition.z * num);
		if (doRecoil)
		{
			float num2 = animationCurveShootRecoil.Evaluate(recoilTimer / recoilTimerMax);
			shootNozzleRecoilTransform.localPosition = new Vector3(movingPieceStartPositionRecoil.x, movingPieceStartPositionRecoil.y, movingPieceStartPositionRecoil.z - 4f + 4f * num2);
			if (recoilTimer >= recoilTimerMax)
			{
				doRecoil = false;
				recoilTimer = 0f;
				shootNozzleRecoilTransform.localPosition = movingPieceStartPositionRecoil;
			}
			else
			{
				recoilTimer += Time.deltaTime;
			}
		}
	}

	private void StateShooting()
	{
		if (stateStart)
		{
			ShootLogic();
			stateStart = false;
		}
	}

	private void StateGoingBack()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		float num = 1f - animationCurveShoot.Evaluate(cartCannonMain.stateTimer / cartCannonMain.stateTimerMax);
		shootAnimationTransform.localPosition = new Vector3(movingPieceStartPosition.x, movingPieceStartPosition.y, movingPieceStartPosition.z - movingPieceStartPosition.z * num);
	}

	private void ParticlePlay()
	{
		foreach (ParticleSystem particle in particles)
		{
			particle.Play();
		}
	}

	private void StateMachine()
	{
		switch (stateCurrent)
		{
		case 0:
			StateInactive();
			break;
		case 1:
			StateActive();
			break;
		case 2:
			StateBuildup();
			break;
		case 3:
			StateShooting();
			break;
		case 4:
			StateGoingBack();
			break;
		}
	}

	private void Update()
	{
		statePrev = stateCurrent;
		stateCurrent = (int)cartCannonMain.stateCurrent;
		if (stateCurrent != statePrev)
		{
			stateStart = true;
		}
		StateMachine();
	}

	private void ShootLogic()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			Transform muzzle = cartCannonMain.muzzle;
			itemBattery.RemoveFullBar(1);
			physGrabObject.rb.AddForceAtPosition(-muzzle.forward * 20f, muzzle.position, ForceMode.Impulse);
			Vector3 endPosition = muzzle.position + muzzle.forward * 25f;
			bool hit = false;
			bool flag = false;
			Vector3 vector = muzzle.forward;
			float num = 0.1f;
			float num2 = 25f;
			if (num > 0f)
			{
				float angle = Random.Range(0f, num / 2f);
				float angle2 = Random.Range(0f, 360f);
				Vector3 normalized = Vector3.Cross(vector, Random.onUnitSphere).normalized;
				Quaternion quaternion = Quaternion.AngleAxis(angle, normalized);
				vector = (Quaternion.AngleAxis(angle2, vector) * quaternion * vector).normalized;
			}
			if (Physics.Raycast(muzzle.position, vector, out var hitInfo, num2, (int)SemiFunc.LayerMaskGetVisionObstruct() + LayerMask.GetMask("Enemy")))
			{
				endPosition = hitInfo.point;
				hit = true;
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				endPosition = muzzle.position + vector * num2;
				hit = true;
			}
			ShootBullet(endPosition, hit);
		}
	}

	private void ShootBullet(Vector3 _endPosition, bool _hit)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("ShootBulletRPC", RpcTarget.All, _endPosition, _hit);
			}
			else
			{
				ShootBulletRPC(_endPosition, _hit);
			}
		}
	}

	[PunRPC]
	public void ShootBulletRPC(Vector3 _endPosition, bool _hit, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			ShootParticlesPlay();
			Transform muzzle = cartCannonMain.muzzle;
			ItemGunBullet component = Object.Instantiate(bulletPrefab, muzzle.position, muzzle.rotation).GetComponent<ItemGunBullet>();
			component.hitPosition = _endPosition;
			component.bulletHit = _hit;
			component.shootLineWidthCurve = animationCurve;
			soundHit.Play(_endPosition);
			shootSound.Play(muzzle.position);
			component.ActivateAll();
			particleScriptExplosion.PlayExplosionSoundMedium(_endPosition);
		}
	}

	private void AnimationEvent1()
	{
		ParticlePlay();
		animationEvent1 = true;
	}

	private void AnimationEvent2()
	{
		ParticlePlay();
		buildup2.Play(base.transform.position);
		animationEvent2 = true;
	}

	private void ShootParticlesPlay()
	{
		foreach (ParticleSystem shootParticle in shootParticles)
		{
			if (shootParticle.isPlaying)
			{
				shootParticle.Stop();
			}
			shootParticle.Play();
		}
	}
}
