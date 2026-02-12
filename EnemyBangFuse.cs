using UnityEngine;

public class EnemyBangFuse : MonoBehaviour
{
	internal EnemyBang controller;

	internal bool setup;

	private bool active;

	private float delayTimer;

	public GameObject tipObject;

	public Transform glowTransform;

	private float glowLerp = 1f;

	private float glowSpeed;

	private Vector3 glowTargetOld;

	private Vector3 glowTargetNew;

	[Space]
	public Transform particleParent;

	public ParticleSystem particleFire;

	public ParticleSystem particleSpark;

	private float sparkTimer;

	[Space]
	public AnimationCurve glowCurve;

	public AnimationCurve stiffCurve;

	public AnimationCurve shrinkCurve;

	[Space]
	public SpringQuaternion botSpring;

	public Transform botTransformSource;

	public Transform botTransformTarget;

	[Space]
	public SpringQuaternion topSpring;

	public Transform topTransformSource;

	public Transform topTransformTarget;

	private void Awake()
	{
		tipObject.SetActive(value: false);
		delayTimer = Random.Range(0.25f, 0.6f);
	}

	private void Update()
	{
		if (!setup)
		{
			return;
		}
		if (controller.fuseActive)
		{
			if (!active)
			{
				if (delayTimer <= 0f)
				{
					tipObject.SetActive(value: true);
					particleFire.Play();
					SparkPlay();
					active = true;
				}
				else
				{
					delayTimer -= Time.deltaTime;
				}
			}
		}
		else if (active)
		{
			tipObject.SetActive(value: false);
			particleFire.Stop();
			active = false;
			delayTimer = Random.Range(0.25f, 0.6f);
		}
		float t = stiffCurve.Evaluate(controller.fuseLerp);
		float t2 = shrinkCurve.Evaluate(controller.fuseLerp);
		Vector3 direction = botTransformTarget.position - (botTransformTarget.position + Vector3.up);
		direction = SemiFunc.ClampDirection(direction, base.transform.forward, Mathf.Lerp(30f, 0f, t));
		botTransformTarget.rotation = Quaternion.RotateTowards(botTransformTarget.rotation, Quaternion.LookRotation(direction), 800f * Time.deltaTime);
		Vector3 direction2 = topTransformTarget.position - (topTransformTarget.position + Vector3.up);
		direction2 = SemiFunc.ClampDirection(direction2, botTransformTarget.forward, Mathf.Lerp(90f, 0f, t));
		topTransformTarget.rotation = Quaternion.RotateTowards(topTransformTarget.rotation, Quaternion.LookRotation(direction2), 800f * Time.deltaTime);
		botTransformSource.rotation = SemiFunc.SpringQuaternionGet(botSpring, botTransformTarget.rotation);
		topTransformSource.rotation = SemiFunc.SpringQuaternionGet(topSpring, topTransformTarget.rotation);
		botSpring.damping = Mathf.Lerp(0.3f, 0.8f, t);
		botSpring.speed = Mathf.Lerp(10f, 15f, t);
		botSpring.maxAngle = Mathf.Lerp(90f, 5f, t);
		topSpring.damping = Mathf.Lerp(0.3f, 0.8f, t);
		topSpring.speed = Mathf.Lerp(10f, 15f, t);
		topSpring.maxAngle = Mathf.Lerp(90f, 5f, t);
		botTransformSource.localPosition = Vector3.Lerp(Vector3.zero, -Vector3.forward * 0.1f, t2);
		if (active)
		{
			controller.anim.FuseLoop();
			particleParent.position = tipObject.transform.position;
			glowLerp += glowSpeed * Time.deltaTime;
			glowLerp = Mathf.Clamp01(glowLerp);
			if (glowLerp >= 1f)
			{
				glowTargetOld = glowTransform.localScale;
				glowTargetNew = Vector3.one * Random.Range(0.75f, 1.25f);
				glowSpeed = Random.Range(5f, 30f);
				glowLerp = 0f;
			}
			glowTransform.localScale = Vector3.Lerp(glowTargetOld, glowTargetNew, glowCurve.Evaluate(glowLerp));
			if (controller.fuseLerp >= controller.explosionTellFuseThreshold)
			{
				if (sparkTimer <= 0f)
				{
					sparkTimer = 0.2f;
					SparkPlay();
				}
				else
				{
					sparkTimer -= Time.deltaTime;
				}
			}
			else
			{
				sparkTimer = 0f;
			}
		}
		else
		{
			glowTargetOld = Vector3.one * 5f;
			glowTargetNew = Vector3.one;
			glowSpeed = 5f;
			glowLerp = 0f;
		}
	}

	public void SparkPlay()
	{
		particleSpark.Play();
		controller.anim.soundFuseIgnite.Play(tipObject.transform.position);
	}

	private void OnDisable()
	{
		particleFire.Stop();
	}
}
