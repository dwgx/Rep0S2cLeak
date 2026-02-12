using System;
using UnityEngine;

public class EnemyThinManAnim : MonoBehaviour
{
	internal Animator animator;

	public EnemyThinMan controller;

	public Enemy enemy;

	public GameObject mesh;

	public Light backLight;

	public GameObject tentaclesParent;

	public GameObject extendedTentacles;

	public GameObject extendedPouch;

	public float tentacleSpeed;

	public AnimationCurve tentacleCurve;

	private float[] randomOffsets;

	private bool tentacleBackActive;

	public GameObject tentacleR1;

	public GameObject tentacleR2;

	public GameObject tentacleR3;

	public GameObject tentacleL1;

	public GameObject tentacleL2;

	public GameObject tentacleL3;

	internal bool rattleImpulse;

	public ParticleSystem particleSmoke;

	public ParticleSystem particleSmokeCalmFill;

	public ParticleSystem particleImpact;

	public ParticleSystem particleDirectionalBits;

	[Space]
	public Sound teleportIn;

	public Sound teleportOut;

	[Space]
	public Sound notice;

	[Space]
	public Sound growLoop;

	public Sound zoom;

	public Sound attack;

	public Sound screamLocal;

	public Sound screamGlobal;

	[Space]
	public Sound hurtSound;

	public Sound deathSound;

	private bool despawnImpulse;

	private bool stunImpulse;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
		randomOffsets = new float[6];
		for (int i = 0; i < randomOffsets.Length; i++)
		{
			randomOffsets[i] = UnityEngine.Random.Range(0f, MathF.PI * 2f);
		}
	}

	private void Update()
	{
		growLoop.PlayLoop(tentaclesParent.activeSelf, 5f, 5f);
		if (controller.tentacleLerp > 0f)
		{
			backLight.intensity = tentacleCurve.Evaluate(controller.tentacleLerp) * 4f;
		}
		else
		{
			backLight.intensity = 0f;
		}
		if (controller.tentacleLerp > 0f)
		{
			if (!tentaclesParent.activeSelf)
			{
				tentaclesParent.SetActive(value: true);
			}
			tentaclesParent.transform.localScale = new Vector3(tentacleCurve.Evaluate(controller.tentacleLerp), tentacleCurve.Evaluate(controller.tentacleLerp), tentacleCurve.Evaluate(controller.tentacleLerp));
		}
		else if (tentaclesParent.activeSelf)
		{
			tentaclesParent.SetActive(value: false);
		}
		if (controller.tentacleLerp > 0f)
		{
			float num = controller.tentacleLerp * 20f;
			float num2 = Mathf.Lerp(10f, 1f, controller.tentacleLerp);
			tentacleR1.transform.localRotation = Quaternion.Euler(Mathf.Sin(num2 + randomOffsets[0]) * num, 0f, 0f);
			tentacleR2.transform.localRotation = Quaternion.Euler(Mathf.Sin(num2 + randomOffsets[1]) * num, 0f, 0f);
			tentacleR3.transform.localRotation = Quaternion.Euler(Mathf.Sin(num2 + randomOffsets[2]) * num, 0f, 0f);
			tentacleL1.transform.localRotation = Quaternion.Euler(Mathf.Sin(num2 + randomOffsets[3]) * num, 0f, 0f);
			tentacleL2.transform.localRotation = Quaternion.Euler(Mathf.Sin(num2 + randomOffsets[4]) * num, 0f, 0f);
			tentacleL3.transform.localRotation = Quaternion.Euler(Mathf.Sin(num2 + randomOffsets[5]) * num, 0f, 0f);
		}
		if (controller.currentState == EnemyThinMan.State.TentacleExtend || controller.currentState == EnemyThinMan.State.Damage)
		{
			if (!extendedTentacles.activeSelf)
			{
				tentaclesParent.SetActive(value: false);
				extendedPouch.SetActive(value: true);
				particleSmoke.Play();
				attack.Play(base.transform.position);
				extendedTentacles.SetActive(value: true);
			}
			if ((bool)controller.playerTarget)
			{
				GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, controller.playerTarget.transform.position, 0.3f);
				GameDirector.instance.CameraImpact.ShakeDistance(10f, 3f, 8f, controller.playerTarget.transform.position, 0.1f);
				float z = Vector3.Distance(controller.playerTarget.transform.position, extendedTentacles.transform.position);
				extendedTentacles.transform.localScale = new Vector3(1f, 1f, z);
				extendedTentacles.transform.LookAt(controller.playerTarget.PlayerVisionTarget.VisionTransform.position);
			}
		}
		else if (extendedTentacles.activeSelf)
		{
			Vector3 localScale = extendedTentacles.transform.localScale;
			localScale.z = Mathf.Lerp(localScale.z, 0f, 10f * Time.deltaTime);
			extendedTentacles.transform.localScale = localScale;
			if (extendedTentacles.transform.localScale.z <= 0.1f)
			{
				extendedTentacles.SetActive(value: false);
				extendedPouch.SetActive(value: false);
			}
		}
		if (rattleImpulse)
		{
			if (enemy.Health.healthCurrent > 0)
			{
				int num3 = UnityEngine.Random.Range(1, 3);
				animator.SetTrigger("Rattle" + num3);
			}
			rattleImpulse = false;
		}
		if (enemy.CurrentState == EnemyState.Despawn && enemy.Health.healthCurrent > 0)
		{
			animator.SetBool("Despawn", value: true);
			if (despawnImpulse)
			{
				animator.SetTrigger("DespawnTrigger");
				despawnImpulse = false;
			}
		}
		else
		{
			animator.SetBool("Despawn", value: false);
			despawnImpulse = true;
		}
		if (enemy.IsStunned() && enemy.CurrentState != EnemyState.Despawn && enemy.Health.healthCurrent > 0)
		{
			animator.SetBool("Stun", value: true);
			if (stunImpulse)
			{
				animator.SetTrigger("StunTrigger");
				stunImpulse = false;
			}
		}
		else
		{
			animator.SetBool("Stun", value: false);
			stunImpulse = true;
		}
	}

	public void NoticeSet()
	{
		if (enemy.Health.healthCurrent < 0)
		{
			return;
		}
		if (controller.playerTarget.isLocal)
		{
			float num = 30f;
			if (Vector3.Distance(controller.playerTarget.transform.position, enemy.transform.position) > 5f)
			{
				num = 20f;
			}
			CameraGlitch.Instance.PlayShort();
			CameraAim.Instance.AimTargetSet(controller.head.transform.position, 0.75f, 2f, controller.gameObject, 90);
			CameraZoom.Instance.OverrideZoomSet(num, 0.75f, 3f, 1f, controller.gameObject, 50);
			zoom.Play(base.transform.position);
		}
		animator.SetTrigger("Scream");
	}

	public void OnDeath()
	{
		animator.SetTrigger("Death");
	}

	public void Scream()
	{
		screamLocal.Play(base.transform.position);
		screamGlobal.Play(base.transform.position);
	}

	public void DeathEffect()
	{
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		particleImpact.Play();
		Quaternion rotation = Quaternion.LookRotation(-enemy.Health.hurtDirection.normalized);
		particleDirectionalBits.transform.rotation = rotation;
		particleDirectionalBits.Play();
		deathSound.Play(base.transform.position);
		enemy.EnemyParent.Despawn();
	}

	public void SetDespawn()
	{
		enemy.EnemyParent.Despawn();
	}

	public void DespawnSmoke()
	{
		controller.SmokeEffect(controller.rb.position);
		teleportOut.Play(base.transform.position);
	}
}
