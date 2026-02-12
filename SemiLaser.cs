using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class SemiLaser : MonoBehaviour
{
	public enum LaserState
	{
		Intro,
		Active,
		Outro
	}

	public float hurtColliderBeamThickness = 1f;

	public float beamThickness = 1f;

	public float beamHitSize = 1f;

	public float wobbleAmount = 1f;

	public GameObject enableLaser;

	public Transform hitTransform;

	public Transform shootTransform;

	public Transform graceTransform;

	public Light laserSpotLight;

	public Transform hurtColliderRotation;

	[FormerlySerializedAs("HitParticlesTransform")]
	public Transform hitParticlesTransform;

	public AudioSource audioSource;

	public AudioSource audioSourceHit;

	public Sound soundLaserStart;

	public Sound soundLaserStartGlobal;

	public Sound soundLaserEnd;

	public Sound soundLaserEndGlobal;

	public Sound soundLaserLoop;

	public Sound soundLaserHitStart;

	public Sound soundLaserHitEnd;

	public Sound soundLaserHitLoop;

	public Sound soundLaserGrace;

	internal LaserState state;

	private List<LineRenderer> lineRenderers = new List<LineRenderer>();

	private List<Light> pointLights = new List<Light>();

	private Vector3 startPosition;

	private Vector3 endPosition;

	private List<MeshRenderer> hitMeshRenderers = new List<MeshRenderer>();

	private bool isHitting;

	private List<ParticleSystem> hitParticles = new List<ParticleSystem>();

	private List<ParticleSystem> shootParticles = new List<ParticleSystem>();

	private List<ParticleSystem> graceParticles = new List<ParticleSystem>();

	internal HurtCollider hurtCollider;

	private Light hitLight;

	private float hitLightOriginalIntensity;

	private bool isActivePrev;

	private bool isActive;

	private float isActiveTimer;

	private float beamThicknessOriginal;

	private float originalHitLightRange;

	private float laserSpotLightOriginalIntensity;

	private bool hitEnd;

	private bool laserEnd;

	private bool laserStart;

	private Transform audioSourceTransform;

	private Transform audioSourceHitTransform;

	private float graceSoundTimer;

	private Vector3 graceSoundPosition;

	private float isHittingTimer;

	private void Start()
	{
		enableLaser.SetActive(value: true);
		lineRenderers = GetComponentsInChildren<LineRenderer>().ToList();
		startPosition = base.transform.position;
		endPosition = base.transform.position + base.transform.forward * 10f;
		pointLights = GetComponentsInChildren<Light>().ToList();
		pointLights.RemoveAll((Light light) => light.type != LightType.Point);
		pointLights.RemoveAll((Light light) => light.shadows != LightShadows.None);
		hitMeshRenderers = hitTransform.GetComponentsInChildren<MeshRenderer>().ToList();
		hitTransform.localScale = Vector3.one * 0.1f;
		hitTransform.gameObject.SetActive(value: false);
		hitParticles = hitParticlesTransform.GetComponentsInChildren<ParticleSystem>().ToList();
		shootParticles = shootTransform.GetComponentsInChildren<ParticleSystem>().ToList();
		hurtCollider = GetComponentInChildren<HurtCollider>();
		hitLight = hitTransform.GetComponentInChildren<Light>();
		hitLightOriginalIntensity = hitLight.intensity;
		graceParticles = graceTransform.GetComponentsInChildren<ParticleSystem>().ToList();
		graceTransform.localScale = Vector3.one * beamThickness;
		shootTransform.localScale = Vector3.one * beamThickness;
		hitTransform.localScale = Vector3.one * beamHitSize * beamThickness;
		enableLaser.SetActive(value: false);
		beamThicknessOriginal = beamThickness;
		originalHitLightRange = hitLight.range;
		audioSourceTransform = audioSource.transform;
		audioSourceHitTransform = audioSourceHit.transform;
	}

	public void LaserActive(Vector3 _startPosition, Vector3 _endPosition, bool _isHitting)
	{
		if (!enableLaser.activeSelf)
		{
			enableLaser.SetActive(value: true);
		}
		startPosition = _startPosition;
		endPosition = _endPosition;
		if (_isHitting)
		{
			isHittingTimer = 0.1f;
		}
		isActiveTimer = 0.1f;
	}

	private void ActiveTimer()
	{
		if (isActiveTimer <= 0f && isActivePrev)
		{
			HitParticles(_play: false);
			ShootParticles(_play: false);
			isActivePrev = isActive;
			isActive = false;
		}
		if (isActiveTimer > 0f)
		{
			if (!isActivePrev)
			{
				ShootParticles(_play: true);
				isActivePrev = isActive;
				isActive = true;
			}
			isActiveTimer -= Time.fixedDeltaTime;
		}
		if (isHittingTimer <= 0f)
		{
			isHitting = false;
		}
		if (isHittingTimer > 0f)
		{
			isHitting = true;
			isHittingTimer -= Time.fixedDeltaTime;
		}
	}

	private void FixedUpdate()
	{
		ActiveTimer();
	}

	private void LaserActiveIntroOutro()
	{
		if (!isActive)
		{
			beamThickness = Mathf.Lerp(beamThickness, 0f, Time.deltaTime * 10f);
			if (hurtCollider.gameObject.activeSelf)
			{
				hurtCollider.gameObject.SetActive(value: false);
			}
			if (beamThickness < beamThicknessOriginal * 0.01f)
			{
				enableLaser.SetActive(value: false);
				beamThickness = 0f;
			}
			if (!laserEnd)
			{
				soundLaserEnd.Play(audioSourceTransform.position);
				soundLaserEndGlobal.Play(audioSourceTransform.position);
				laserEnd = true;
			}
			laserStart = false;
		}
		else
		{
			laserEnd = false;
			if (!laserStart)
			{
				soundLaserStart.Play(audioSourceTransform.position);
				soundLaserStartGlobal.Play(audioSourceTransform.position);
				laserStart = true;
			}
			beamThickness = Mathf.Lerp(beamThickness, beamThicknessOriginal, Time.deltaTime * 10f);
			if (beamThickness > beamThicknessOriginal * 0.95f && !hurtCollider.gameObject.activeSelf)
			{
				hurtCollider.gameObject.SetActive(value: true);
			}
		}
	}

	private void LaserPositioning()
	{
		base.transform.position = startPosition;
		hitTransform.LookAt(startPosition);
		shootTransform.LookAt(endPosition);
		graceTransform.LookAt(endPosition);
		shootTransform.position = startPosition;
		hitTransform.position = endPosition + hitTransform.forward * 0.3f;
		audioSourceHitTransform.position = hitTransform.position;
		hitParticlesTransform.position = hitTransform.position;
		hitParticlesTransform.LookAt(startPosition);
		hurtCollider.transform.localScale = new Vector3(hurtColliderBeamThickness, hurtColliderBeamThickness, Vector3.Distance(startPosition, endPosition));
		hurtCollider.transform.localPosition = new Vector3((0f - hurtCollider.transform.localScale.x) / 2f, (0f - hurtCollider.transform.localScale.y) / 2f, 0f);
		hurtColliderRotation.transform.LookAt(endPosition);
		laserSpotLight.transform.LookAt(endPosition);
		laserSpotLight.range = Vector3.Distance(startPosition, endPosition) * 1.5f;
		laserSpotLight.intensity = laserSpotLightOriginalIntensity * beamThickness;
	}

	private void Update()
	{
		soundLaserLoop.PlayLoop(enableLaser.activeSelf, 2f, 2f);
		soundLaserHitLoop.PlayLoop(isHitting && enableLaser.activeSelf, 2f, 2f);
		LaserEffectIsHitting();
		LaserActiveIntroOutro();
		if (enableLaser.activeSelf)
		{
			LaserPositioning();
			AudioSourcePositioning();
			LaserEffectGrace();
			LaserEffectLine();
		}
	}

	private void LaserEffectGrace()
	{
		if (graceSoundTimer > 0f)
		{
			graceSoundTimer -= Time.deltaTime;
		}
		if (!SemiFunc.FPSImpulse15())
		{
			return;
		}
		RaycastHit[] array = Physics.SphereCastAll(startPosition, hurtColliderBeamThickness, shootTransform.forward, Vector3.Distance(startPosition, endPosition), SemiFunc.LayerMaskGetVisionObstruct());
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			foreach (ParticleSystem graceParticle in graceParticles)
			{
				if (raycastHit.point != Vector3.zero && isActive)
				{
					graceParticle.transform.position = raycastHit.point;
					graceParticle.Emit(3);
					float num = Vector3.Distance(graceSoundPosition, raycastHit.point);
					if (graceSoundTimer <= 0f || num > 1f)
					{
						soundLaserGrace.Play(raycastHit.point);
						graceSoundPosition = raycastHit.point;
						graceSoundTimer = 0.15f;
					}
				}
			}
		}
	}

	private void AudioSourcePositioning()
	{
		Transform obj = AudioListenerFollow.instance.transform;
		Vector3 vector = endPosition - startPosition;
		float value = Vector3.Dot(obj.position - startPosition, vector) / vector.sqrMagnitude;
		value = Mathf.Clamp01(value);
		Vector3 position = startPosition + vector * value;
		audioSourceTransform.position = position;
	}

	private void LaserEffectLine()
	{
		float num = 0.035f * wobbleAmount + (beamThicknessOriginal - beamThickness) * 0.02f;
		int num2 = Mathf.CeilToInt(Vector3.Distance(startPosition, endPosition) * 2f);
		foreach (LineRenderer lineRenderer in lineRenderers)
		{
			Vector3[] array = new Vector3[num2];
			for (int i = 0; i < num2; i++)
			{
				float t = (float)i / (float)num2;
				array[i] = Vector3.Lerp(startPosition, endPosition, t);
				array[i] += Vector3.right * Mathf.Sin(Time.time * 60f + (float)i) * num;
				array[i] += Vector3.up * Mathf.Cos(Time.time * 60f + (float)i) * num;
			}
			lineRenderer.material.mainTextureOffset = new Vector2((0f - Time.time) * 30f, 0f);
			lineRenderer.widthMultiplier = (Mathf.PingPong(Time.time * 60f, 0.4f) + 0.8f) * beamThickness;
			lineRenderer.positionCount = num2;
			lineRenderer.SetPositions(array);
			if (isHitting)
			{
				lineRenderer.endWidth = 0.4f * beamThickness;
			}
			else
			{
				lineRenderer.endWidth = 0f;
			}
		}
		float num3 = 4f;
		float num4 = 4f;
		int count = pointLights.Count;
		float num5 = Vector3.Distance(startPosition, endPosition);
		_ = (endPosition - startPosition).normalized;
		int num6 = Mathf.Min(count, Mathf.CeilToInt(num5 / 2f));
		for (int j = 0; j < count; j++)
		{
			if (j < num6)
			{
				int num7 = num6 - 1;
				if (num7 <= 0)
				{
					num7 = 1;
				}
				float t2 = (float)j / (float)num7;
				Vector3 position = Vector3.Lerp(startPosition, endPosition, t2);
				pointLights[j].transform.position = Vector3.Lerp(pointLights[j].transform.position, position, Time.deltaTime * 20f);
				if (!pointLights[j].enabled)
				{
					pointLights[j].transform.position = position;
					pointLights[j].enabled = true;
					pointLights[j].range = 0f;
				}
				pointLights[j].range = Mathf.Lerp(pointLights[j].range, num4, Time.deltaTime * 10f) * beamThickness;
				pointLights[j].intensity = Mathf.PingPong(Time.time * 20f, 2f) + num3;
			}
			else
			{
				pointLights[j].range = Mathf.Lerp(pointLights[j].range, 0f, Time.deltaTime * 8f);
				if (pointLights[j].range < 0.05f)
				{
					pointLights[j].enabled = false;
				}
			}
		}
	}

	private void HitParticles(bool _play)
	{
		foreach (ParticleSystem hitParticle in hitParticles)
		{
			if (_play)
			{
				hitParticle.Play();
			}
			else
			{
				hitParticle.Stop();
			}
		}
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

	private void LaserEffectIsHitting()
	{
		if (isHitting)
		{
			if (!hitTransform.gameObject.activeSelf)
			{
				HitParticles(_play: true);
				hitTransform.gameObject.SetActive(value: true);
				GameDirector.instance.CameraShake.ShakeDistance(8f, 3f, 8f, hitTransform.position, 0.1f);
				GameDirector.instance.CameraImpact.ShakeDistance(12f, 3f, 8f, hitTransform.position, 0.1f);
				hitTransform.localScale = Vector3.zero;
				hitLight.intensity = 0f;
				soundLaserHitStart.Play(hitTransform.position);
				hitEnd = false;
			}
			GameDirector.instance.CameraShake.ShakeDistance(8f, 0f, 6f, hitTransform.position, 0.1f);
			hitTransform.localScale = Vector3.Lerp(hitTransform.localScale, Vector3.one, Time.deltaTime * 40f) * beamHitSize * beamThickness;
			hitLight.intensity = Mathf.Lerp(hitLight.intensity, hitLightOriginalIntensity, Time.deltaTime * 40f) * beamThickness;
			hitLight.intensity += Mathf.Sin(Time.time * 40f) * 0.5f * beamThickness;
			hitLight.range = originalHitLightRange * beamThickness;
			int num = 0;
			float num2 = 0.15f;
			float num3 = 60f;
			{
				foreach (MeshRenderer hitMeshRenderer in hitMeshRenderers)
				{
					float num4 = 1.5f;
					num4 = ((num != 0) ? 1.55f : 0.85f);
					Vector3 vector = new Vector3(Mathf.Sin(Time.time * num3) * num2 + 1f, Mathf.Cos(Time.time * num3) * num2 + 1f, Mathf.Sin(Time.time * num3) * num2 + 1f);
					hitMeshRenderer.transform.localScale = new Vector3(vector.x * num4, vector.y * num4, vector.z * num4);
					hitMeshRenderer.material.mainTextureOffset = new Vector2(Time.time * 10f, 0f);
					hitMeshRenderer.material.mainTextureScale = new Vector2(Mathf.Sin(Time.time * 20f) * 0.4f + 1f, Mathf.Cos(Time.time * 10f) * 0.4f + 1f);
					num++;
				}
				return;
			}
		}
		if (!hitEnd)
		{
			soundLaserHitEnd.Play(hitTransform.position);
			hitEnd = true;
		}
		hitTransform.localScale = Vector3.Lerp(hitTransform.localScale, Vector3.zero, Time.deltaTime * 40f) * beamHitSize * beamThickness;
		hitLight.intensity = Mathf.Lerp(hitLight.intensity, 0f, Time.deltaTime * 40f) / beamThickness;
		if (hitTransform.localScale.x < 0.01f)
		{
			hitTransform.gameObject.SetActive(value: false);
		}
	}
}
