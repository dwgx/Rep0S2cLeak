using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ItemCartLaser : MonoBehaviour
{
	public SemiLaser semiLaser;

	public Transform muzzleTransform;

	private ItemCartCannonMain cartCannonMain;

	private PhotonView photonView;

	private PhysGrabObject physGrabObject;

	private ItemBattery itemBattery;

	public Sound soundHit;

	public AnimationCurve animationCurveBuildup;

	public AnimationCurve animationCurveHatchType1;

	public AnimationCurve animationCurveHatchType2;

	public AnimationCurve animationCurveWarningLight;

	private Vector3 movingPieceStartPosition;

	public Sound soundBuildupStart;

	public Sound soundBuildupLoop;

	public Sound soundGoingBack;

	private PhysGrabObjectImpactDetector physGrabObjectImpactDetector;

	private bool animationEvent1;

	private bool animationEvent2;

	public Transform animationEventTransform;

	public Transform shootParticlesTransform;

	private List<ParticleSystem> particles = new List<ParticleSystem>();

	private List<ParticleSystem> shootParticles = new List<ParticleSystem>();

	private ParticleScriptExplosion particleScriptExplosion;

	private int statePrev;

	internal int stateCurrent;

	private bool stateStart;

	public HurtCollider laserHurtCollider;

	public Transform hatch1Right;

	public Transform hatch1Left;

	public Transform hatch2Right;

	public Transform hatch2Left;

	public MeshRenderer warningLight1MeshRenderer;

	public MeshRenderer warningLight2MeshRenderer;

	public Light warningLight1Light;

	public Light warningLight2Light;

	public ParticleSystem hatchLeft;

	public ParticleSystem hatchRight;

	private void Start()
	{
		cartCannonMain = GetComponent<ItemCartCannonMain>();
		photonView = GetComponent<PhotonView>();
		itemBattery = GetComponent<ItemBattery>();
		physGrabObject = GetComponent<PhysGrabObject>();
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		physGrabObjectImpactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		shootParticles = new List<ParticleSystem>(shootParticlesTransform.GetComponentsInChildren<ParticleSystem>());
	}

	private void StateInactive()
	{
		if (stateStart)
		{
			ResetHatches();
			stateStart = false;
		}
	}

	private void StateActive()
	{
		if (stateStart)
		{
			ResetHatches();
			stateStart = false;
		}
	}

	private void StateBuildup()
	{
		if (stateStart)
		{
			ResetHatches();
			soundBuildupStart.Play(base.transform.position);
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
	}

	private void StateShooting()
	{
		if (stateStart)
		{
			ResetHatches();
			ShootParticles(_play: true);
			stateStart = false;
		}
		Vector3 endPosition = muzzleTransform.position + muzzleTransform.forward * 15f;
		bool isHitting = false;
		if (Physics.Raycast(muzzleTransform.position, muzzleTransform.forward, out var hitInfo, 15f, SemiFunc.LayerMaskGetVisionObstruct()))
		{
			endPosition = hitInfo.point;
			isHitting = true;
		}
		semiLaser.LaserActive(muzzleTransform.position, endPosition, isHitting);
	}

	private void StateGoingBack()
	{
		if (stateStart)
		{
			ResetHatches();
			itemBattery.RemoveFullBar(1);
			soundGoingBack.Play(base.transform.position);
			stateStart = false;
			HatchWarningLightsTurnOn();
		}
		float t = animationCurveHatchType1.Evaluate(cartCannonMain.stateTimer / cartCannonMain.stateTimerMax);
		float t2 = animationCurveHatchType2.Evaluate(cartCannonMain.stateTimer / cartCannonMain.stateTimerMax);
		float t3 = animationCurveWarningLight.Evaluate(cartCannonMain.stateTimer / cartCannonMain.stateTimerMax);
		hatch1Left.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 25f, t));
		hatch1Right.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -25f, t));
		hatch2Left.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -120f, t2));
		hatch2Right.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 120f, t2));
		warningLight1Light.intensity = Mathf.Lerp(0f, 4f, t3);
		warningLight2Light.intensity = Mathf.Lerp(0f, 4f, t3);
		warningLight1MeshRenderer.material.SetColor("_EmissionColor", Color.Lerp(new Color(0f, 0f, 0f), new Color(1f, 0f, 0f), t3));
		warningLight2MeshRenderer.material.SetColor("_EmissionColor", Color.Lerp(new Color(0f, 0f, 0f), new Color(1f, 0f, 0f), t3));
		if (cartCannonMain.stateTimer > 0.2f && !hatchLeft.isPlaying)
		{
			hatchLeft.Play();
			hatchRight.Play();
		}
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
		bool playing = stateCurrent == 2;
		float pitchMultiplier = Mathf.Lerp(0.8f, 1.2f, cartCannonMain.stateTimer / cartCannonMain.stateTimerMax);
		soundBuildupLoop.PlayLoop(playing, 1f, 1f, pitchMultiplier);
		StateMachine();
	}

	private void ResetHatches()
	{
		hatch1Left.localRotation = Quaternion.Euler(0f, 0f, 0f);
		hatch1Right.localRotation = Quaternion.Euler(0f, 0f, 0f);
		hatch2Left.localRotation = Quaternion.Euler(0f, 0f, 0f);
		hatch2Right.localRotation = Quaternion.Euler(0f, 0f, 0f);
		HatchWarningLightsTurnOff();
		ShootParticles(_play: false);
	}

	private void HatchWarningLightsTurnOn()
	{
		warningLight1Light.enabled = true;
		warningLight2Light.enabled = true;
		warningLight1MeshRenderer.material.SetColor("_EmissionColor", new Color(1f, 0f, 0f));
		warningLight2MeshRenderer.material.SetColor("_EmissionColor", new Color(1f, 0f, 0f));
	}

	private void HatchWarningLightsTurnOff()
	{
		warningLight1Light.enabled = false;
		warningLight2Light.enabled = false;
		warningLight1MeshRenderer.material.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
		warningLight2MeshRenderer.material.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
	}

	private void AnimationEvent1()
	{
		ParticlePlay();
		animationEvent1 = true;
	}

	private void AnimationEvent2()
	{
		ParticlePlay();
		animationEvent2 = true;
	}

	private void ShootParticles(bool _play)
	{
		foreach (ParticleSystem shootParticle in shootParticles)
		{
			if (_play)
			{
				shootParticle.Play();
			}
			else
			{
				shootParticle.Stop();
			}
		}
	}
}
