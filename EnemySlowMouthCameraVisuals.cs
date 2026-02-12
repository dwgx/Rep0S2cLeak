using System.Collections.Generic;
using UnityEngine;

public class EnemySlowMouthCameraVisuals : MonoBehaviour
{
	public enum State
	{
		Intro,
		Idle,
		Puke,
		Outro
	}

	internal EnemySlowMouth enemySlowMouth;

	internal State state;

	internal State statePrev;

	public SemiPuke semiPuke;

	public AnimationCurve curveIntroOutro;

	public Transform pukeCapsuleTransform;

	public MeshRenderer pukeCapsuleRenderer;

	private float curveEval;

	private PlayerAvatar playerAvatar;

	public Transform topJawTransform;

	public Transform botJawTransform;

	public GameObject puke;

	private Quaternion topJawStartRotation;

	private Quaternion botJawStartRotation;

	private Quaternion topJawTargetRotation;

	private Quaternion botJawTargetRotation;

	private Vector3 jawStartPosition;

	private Vector3 jawTargetPosition;

	private SpringQuaternion topJawSpring;

	private SpringQuaternion botJawSpring;

	private SpringVector3 jawPositionSpring;

	private bool stateStart = true;

	private float stateTimer;

	private float openAngleTarget = 125f;

	private float pukeTimer;

	internal PlayerAvatar playerTarget;

	public List<ParticleSystem> pukeParticles;

	private void Start()
	{
		jawPositionSpring = new SpringVector3();
		jawPositionSpring.damping = 0.5f;
		jawPositionSpring.speed = 20f;
		topJawSpring = new SpringQuaternion();
		topJawSpring.damping = 0.5f;
		topJawSpring.speed = 20f;
		botJawSpring = new SpringQuaternion();
		botJawSpring.damping = 0.5f;
		botJawSpring.speed = 20f;
		playerAvatar = PlayerAvatar.instance;
		topJawStartRotation = topJawTransform.localRotation;
		botJawStartRotation = botJawTransform.localRotation;
		topJawTargetRotation = topJawStartRotation;
		botJawTargetRotation = topJawStartRotation;
		jawStartPosition = base.transform.localPosition;
	}

	private void StateIntro()
	{
		if (stateStart)
		{
			stateTimer = 1f;
			float num = 125f;
			topJawTransform.localRotation = Quaternion.Euler(num, 0f, 0f);
			botJawTransform.localRotation = Quaternion.Euler(0f - num, 0f, 0f);
			topJawSpring.lastRotation = topJawTransform.localRotation;
			botJawSpring.lastRotation = botJawTransform.localRotation;
			jawTargetPosition = jawStartPosition;
			base.transform.localPosition = jawTargetPosition + new Vector3(0f, -1f, -1f);
			jawPositionSpring.lastPosition = base.transform.localPosition;
			stateStart = false;
		}
		float t = curveIntroOutro.Evaluate(1f - stateTimer);
		openAngleTarget = Mathf.LerpUnclamped(125f, 0f, t);
		topJawTargetRotation = topJawStartRotation * Quaternion.Euler(openAngleTarget, 0f, 0f);
		botJawTargetRotation = botJawStartRotation * Quaternion.Euler(0f - openAngleTarget, 0f, 0f);
		if (stateTimer < 0f)
		{
			StateSet(State.Idle);
		}
	}

	private void StateIdle()
	{
		if (stateStart)
		{
			botJawTargetRotation = botJawStartRotation;
			topJawTargetRotation = topJawStartRotation;
			jawTargetPosition = jawStartPosition;
			stateStart = false;
			if (statePrev == State.Puke)
			{
				GameDirector.instance.CameraShake.ShakeDistance(8f, 3f, 8f, base.transform.position, 0.1f);
				GameDirector.instance.CameraImpact.ShakeDistance(12f, 3f, 8f, base.transform.position, 0.1f);
			}
		}
		botJawTargetRotation = botJawStartRotation * Quaternion.Euler(-2f * Mathf.Sin(Time.time * 2f), 0f, 0f);
		topJawTargetRotation = topJawStartRotation * Quaternion.Euler(2f * Mathf.Sin(Time.time * 2f), 0f, 0f);
		if (SemiFunc.IsMultiplayer() && (bool)playerAvatar && (bool)playerAvatar.voiceChat)
		{
			botJawSpring.speed = 20f;
			topJawSpring.speed = 20f;
			float value = playerAvatar.voiceChat.clipLoudness * 100f;
			value = Mathf.Clamp(value, 0f, 10f);
			topJawTargetRotation *= Quaternion.Euler(topJawStartRotation.x + value, topJawStartRotation.y, topJawStartRotation.z);
			botJawTargetRotation *= Quaternion.Euler(botJawStartRotation.x - value, botJawStartRotation.y, botJawStartRotation.z);
		}
		if (SemiFunc.FPSImpulse15())
		{
			topJawSpring.springVelocity = Random.insideUnitSphere * 0.2f;
			botJawSpring.springVelocity = Random.insideUnitSphere * 0.2f;
		}
	}

	private void StatePuke()
	{
		if (stateStart)
		{
			stateStart = false;
			Vector3 localScale = pukeCapsuleTransform.localScale;
			localScale.x = 0f;
			GameDirector.instance.CameraShake.ShakeDistance(8f, 3f, 8f, base.transform.position, 0.1f);
			GameDirector.instance.CameraImpact.ShakeDistance(12f, 3f, 8f, base.transform.position, 0.1f);
			pukeCapsuleTransform.localScale = localScale;
		}
		GameDirector.instance.CameraShake.ShakeDistance(4f, 3f, 8f, base.transform.position, 0.1f);
		semiPuke.PukeActive(semiPuke.transform.position, playerTarget.localCamera.transform.rotation);
		pukeTimer = 0.2f;
		botJawTargetRotation = botJawStartRotation * Quaternion.Euler(-25f, 0f, 0f);
		topJawTargetRotation = topJawStartRotation * Quaternion.Euler(25f, 0f, 0f);
		if (SemiFunc.FPSImpulse15())
		{
			topJawSpring.springVelocity = Random.insideUnitSphere * 5f;
			botJawSpring.springVelocity = Random.insideUnitSphere * 5f;
		}
	}

	private void StateOutro()
	{
		if (stateStart)
		{
			stateStart = false;
			stateTimer = 1f;
		}
		float t = curveIntroOutro.Evaluate(1f - stateTimer);
		openAngleTarget = Mathf.LerpUnclamped(0f, 125f, t);
		topJawTargetRotation = topJawStartRotation * Quaternion.Euler(openAngleTarget, 0f, 0f);
		botJawTargetRotation = botJawStartRotation * Quaternion.Euler(0f - openAngleTarget, 0f, 0f);
		jawTargetPosition = Vector3.LerpUnclamped(jawStartPosition, jawStartPosition + new Vector3(0f, 0.2f, -0.4f), t);
		if (stateTimer < 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void StateSynchingWithParentEnemy()
	{
		bool flag = enemySlowMouth.currentState == EnemySlowMouth.State.Puke;
		if (enemySlowMouth.currentState == EnemySlowMouth.State.Attached || enemySlowMouth.currentState == EnemySlowMouth.State.Puke || enemySlowMouth.currentState == EnemySlowMouth.State.Detach)
		{
			if (flag)
			{
				StateSet(State.Puke);
			}
			else if (state != State.Intro)
			{
				StateSet(State.Idle);
			}
		}
		else
		{
			StateSet(State.Outro);
		}
	}

	private void StateMachine()
	{
		StateSynchingWithParentEnemy();
		if (stateTimer > 0f)
		{
			stateTimer -= Time.deltaTime;
		}
		switch (state)
		{
		case State.Intro:
			StateIntro();
			break;
		case State.Idle:
			StateIdle();
			break;
		case State.Puke:
			StatePuke();
			break;
		case State.Outro:
			StateOutro();
			break;
		}
	}

	public void StateSet(State newState)
	{
		if (state != newState)
		{
			statePrev = state;
			state = newState;
			stateStart = true;
			stateTimer = 0f;
		}
	}

	private void PukeParticles(bool _play)
	{
		foreach (ParticleSystem pukeParticle in pukeParticles)
		{
			if (_play)
			{
				if (!pukeParticle.isPlaying)
				{
					pukeParticle.Play();
				}
			}
			else if (pukeParticle.isPlaying)
			{
				pukeParticle.Stop();
			}
		}
	}

	private void Update()
	{
		StateMachine();
		if (pukeTimer > 0f)
		{
			PukeParticles(_play: true);
			pukeTimer -= Time.deltaTime;
			pukeCapsuleTransform.localScale = Vector3.Lerp(pukeCapsuleTransform.localScale, Vector3.one, Time.deltaTime * 5f);
			pukeCapsuleTransform.localScale += Vector3.one * Mathf.Sin(Time.time * 30f) * 0.01f;
			pukeCapsuleTransform.localScale += Vector3.one * Mathf.Sin(Time.time * 60f) * 0.01f;
			Vector2 textureOffset = pukeCapsuleRenderer.material.GetTextureOffset("_MainTex");
			textureOffset.x -= Time.deltaTime * 1.5f;
			pukeCapsuleRenderer.material.SetTextureOffset("_MainTex", textureOffset);
			if (!pukeCapsuleTransform.gameObject.activeSelf)
			{
				pukeCapsuleTransform.localScale = Vector3.zero;
				pukeCapsuleTransform.gameObject.SetActive(value: true);
			}
		}
		else
		{
			PukeParticles(_play: false);
			pukeCapsuleTransform.localScale = Vector3.Lerp(pukeCapsuleTransform.localScale, Vector3.zero, Time.deltaTime * 5f);
			if (pukeCapsuleTransform.localScale.x < 0.01f && pukeCapsuleTransform.gameObject.activeSelf)
			{
				pukeCapsuleTransform.localScale = Vector3.zero;
				pukeCapsuleTransform.gameObject.SetActive(value: false);
			}
		}
		base.transform.localPosition = SemiFunc.SpringVector3Get(jawPositionSpring, jawTargetPosition);
		topJawTransform.localRotation = SemiFunc.SpringQuaternionGet(topJawSpring, topJawTargetRotation);
		botJawTransform.localRotation = SemiFunc.SpringQuaternionGet(botJawSpring, botJawTargetRotation);
	}
}
