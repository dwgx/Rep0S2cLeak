using System.Collections.Generic;
using UnityEngine;

public class EnemySlowMouthPlayerAvatarAttached : MonoBehaviour
{
	public enum State
	{
		Intro,
		Idle,
		Puke,
		Outro
	}

	public Transform jawBot;

	public Transform particles;

	private SpringFloat springFloatScale;

	internal EnemySlowMouth enemySlowMouth;

	internal SemiPuke semiPuke;

	private float scaleTarget = 1f;

	private bool stateStart;

	internal PlayerAvatar playerTarget;

	public List<Transform> eyeTransforms;

	private PlayerVoiceChat playerVoiceChat;

	private float loudnessAdd;

	private SpringFloat loudnessSpring;

	private float loudnessTarget;

	public State state;

	private void Start()
	{
		loudnessSpring = new SpringFloat();
		loudnessSpring.damping = 0.5f;
		loudnessSpring.speed = 20f;
		springFloatScale = new SpringFloat();
		springFloatScale.damping = 0.35f;
		springFloatScale.speed = 10f;
		base.transform.localScale = Vector3.one * 2f;
		springFloatScale.lastPosition = 2f;
		playerVoiceChat = playerTarget.voiceChat;
	}

	private void StateIntro()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		loudnessTarget = 0f;
		scaleTarget = 1f;
	}

	private void StateIdle()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		loudnessTarget = 0f;
		scaleTarget = 1f;
	}

	private void StatePuke()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		float num = Mathf.Sin(Time.time * 40f) * 0.05f;
		loudnessTarget = 0.2f + num;
		scaleTarget = 1f;
		semiPuke.PukeActive(semiPuke.transform.position, playerTarget.localCamera.transform.rotation);
	}

	private void StateOutro()
	{
		if (stateStart)
		{
			particles.gameObject.SetActive(value: true);
			stateStart = false;
		}
		loudnessTarget = 0f;
		scaleTarget = 0f;
		if (base.transform.localScale.x < 0.05f)
		{
			enemySlowMouth.UpdateState(EnemySlowMouth.State.Detach);
			Object.Destroy(jawBot.gameObject);
			Object.Destroy(base.gameObject);
		}
	}

	private void StateMachine()
	{
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

	private void Update()
	{
		if ((bool)playerVoiceChat)
		{
			playerVoiceChat.OverrideClipLoudnessAnimationValue(loudnessAdd);
		}
		else if ((bool)playerTarget)
		{
			playerVoiceChat = playerTarget.voiceChat;
		}
		loudnessAdd = SemiFunc.SpringFloatGet(loudnessSpring, loudnessTarget);
		Quaternion rotation = playerTarget.playerAvatarVisuals.playerEyes.eyeLeft.rotation;
		Quaternion rotation2 = playerTarget.playerAvatarVisuals.playerEyes.eyeRight.rotation;
		eyeTransforms[0].rotation = rotation;
		eyeTransforms[1].rotation = rotation2;
		StateSynchingWithParentEnemy();
		StateMachine();
		base.transform.localScale = Vector3.one * SemiFunc.SpringFloatGet(springFloatScale, scaleTarget);
		jawBot.localScale = Vector3.one * SemiFunc.SpringFloatGet(springFloatScale, scaleTarget);
	}

	private void StateSynchingWithParentEnemy()
	{
		if (!enemySlowMouth)
		{
			StateSet(State.Outro);
			return;
		}
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

	private void StateSet(State _state)
	{
		if (state != _state)
		{
			state = _state;
			stateStart = true;
		}
	}

	private void OnDisable()
	{
		if (playerTarget.isDisabled)
		{
			enemySlowMouth.UpdateState(EnemySlowMouth.State.Detach);
			enemySlowMouth.detachPosition = playerTarget.localCamera.transform.position;
			enemySlowMouth.detachRotation = playerTarget.localCamera.transform.rotation;
			Object.Destroy(jawBot.gameObject);
			Object.Destroy(base.gameObject);
		}
	}
}
