using UnityEngine;

public class ItemGunLaser : MonoBehaviour
{
	public SemiLaser semiLaser;

	public Transform muzzleTransform;

	private float shootImpulseTimer;

	private bool shooting;

	public ParticleSystem muzzleFlashParticle;

	private ItemGun itemGun;

	public Sound soundInitialCrack;

	public Sound soundBuildup;

	public Sound soundReload;

	public Sound soundReload2;

	private float buildupImpulseTimer;

	public ParticleSystem particleOverHeat;

	public AnimationCurve buildupImpulseCurve;

	public AnimationCurve heatLatchCurve;

	public AnimationCurve backLatchCurve;

	public Transform transformGunEnergyPlop;

	public Transform transformOverHeatLatch;

	public Transform transformBackPlingPlong;

	private Material materialGunEnergyPlop;

	public ParticleSystem particleBuildUp;

	public ParticleSystem particleInitialCrack;

	private bool soundReload2Played;

	private PhysGrabObject physGrabObject;

	private void Start()
	{
		itemGun = GetComponent<ItemGun>();
		materialGunEnergyPlop = transformGunEnergyPlop.GetComponent<Renderer>().material;
		materialGunEnergyPlop.SetColor("_EmissionColor", Color.black);
		physGrabObject = GetComponent<PhysGrabObject>();
	}

	private void FixedUpdate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && itemGun.stateCurrent == ItemGun.State.Shooting)
		{
			Vector3 force = -muzzleTransform.forward * 10f;
			physGrabObject.rb.AddForce(force, ForceMode.Force);
			Vector3 torque = -muzzleTransform.right * 0.6f;
			physGrabObject.rb.AddTorque(torque, ForceMode.Force);
		}
	}

	private void Update()
	{
		if (shootImpulseTimer > 0f)
		{
			if (!shooting)
			{
				muzzleFlashParticle.Play(withChildren: true);
				shooting = true;
			}
			shootImpulseTimer -= Time.deltaTime;
		}
		else if (shooting)
		{
			muzzleFlashParticle.Stop(withChildren: true);
			shooting = false;
		}
		if (buildupImpulseTimer > 0f)
		{
			buildupImpulseTimer -= Time.deltaTime;
		}
		bool playing = buildupImpulseTimer > 0f;
		float pitchMultiplier = Mathf.Lerp(3f, 1f, 1f - itemGun.stateTimer / itemGun.stateTimeMax);
		soundBuildup.PlayLoop(playing, 1f, 1f, pitchMultiplier);
	}

	public void OnStateShootStart()
	{
		soundInitialCrack.Play(base.transform.position);
		particleInitialCrack.Play(withChildren: true);
		GameDirector.instance.CameraImpact.ShakeDistance(7f, 6f, 12f, base.transform.position, 0.1f);
	}

	public void OnStateShootUpdate()
	{
		LaserShooting();
	}

	public void OnStateReloadStart()
	{
		soundReload.Play(base.transform.position);
		particleBuildUp.Stop(withChildren: true);
		particleOverHeat.Play(withChildren: true);
		soundReload.Play(base.transform.position);
		soundReload2Played = false;
		transformGunEnergyPlop.localPosition = new Vector3(0f, 0f, -0.6f);
	}

	public void OnStateReloadUpdate()
	{
		float num = -0.6f;
		float time = itemGun.stateTimer / (itemGun.stateTimeMax / 2f);
		float num2 = buildupImpulseCurve.Evaluate(time);
		if (num2 > 1f)
		{
			num2 = 1f;
		}
		transformGunEnergyPlop.localPosition = new Vector3(0f, 0f, Mathf.Lerp(num, 0f, num2));
		Color value = Color.Lerp(Color.yellow, Color.black, num2);
		materialGunEnergyPlop.SetColor("_EmissionColor", value);
		time = itemGun.stateTimer / itemGun.stateTimeMax;
		num2 = backLatchCurve.Evaluate(time);
		float num3 = 130f * num2;
		transformBackPlingPlong.localRotation = Quaternion.Euler(0f - num3, 0f, 0f);
		time = itemGun.stateTimer / itemGun.stateTimeMax;
		num2 = heatLatchCurve.Evaluate(time);
		float num4 = 84f * num2;
		transformOverHeatLatch.localRotation = Quaternion.Euler(0f - num4, 0f, 0f);
		if (!soundReload2Played && itemGun.stateTimer > itemGun.stateTimeMax * 0.8f)
		{
			soundReload2.Play(base.transform.position);
			soundReload2Played = true;
		}
	}

	public void OnStateIdleStart()
	{
		particleBuildUp.Stop(withChildren: true);
		transformGunEnergyPlop.localPosition = new Vector3(0f, 0f, 0f);
		materialGunEnergyPlop.SetColor("_EmissionColor", Color.black);
		transformOverHeatLatch.localRotation = Quaternion.Euler(0f, 0f, 0f);
		transformBackPlingPlong.localRotation = Quaternion.Euler(0f, 0f, 0f);
	}

	public void OnStateBuildStart()
	{
		particleBuildUp.Play(withChildren: true);
	}

	public void OnStateBuildUpdate()
	{
		buildupImpulseTimer = 0.1f;
		float num = -0.6f;
		float time = itemGun.stateTimer / itemGun.stateTimeMax;
		float num2 = buildupImpulseCurve.Evaluate(time);
		if (num2 > 1f)
		{
			num2 = 1f;
		}
		transformGunEnergyPlop.localPosition = new Vector3(0f, 0f, Mathf.Lerp(0f, num, num2));
		Color value = Color.Lerp(Color.black, Color.yellow, num2);
		materialGunEnergyPlop.SetColor("_EmissionColor", value);
	}

	private void LaserShooting()
	{
		float gunRange = itemGun.gunRange;
		Vector3 endPosition = muzzleTransform.position + muzzleTransform.forward * gunRange;
		bool isHitting = false;
		if (Physics.Raycast(muzzleTransform.position, muzzleTransform.forward, out var hitInfo, gunRange, SemiFunc.LayerMaskGetVisionObstruct()))
		{
			endPosition = hitInfo.point;
			isHitting = true;
		}
		semiLaser.LaserActive(muzzleTransform.position, endPosition, isHitting);
		shootImpulseTimer = 0.1f;
	}
}
