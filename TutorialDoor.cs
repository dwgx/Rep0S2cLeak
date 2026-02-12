using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TutorialDoor : MonoBehaviour
{
	private enum DoorState
	{
		Closed,
		Success,
		Unlock,
		Opening,
		Open
	}

	public int tutorialPage;

	public AnimationCurve animationCurve;

	public AnimationCurve animationCurveDoor;

	private float doorEndYPos;

	private float animationProgress;

	private bool animationDone;

	private int prevState;

	private int currentState;

	private float stateTimer;

	private bool stateStart;

	public Transform latchTransform;

	public Transform screenTransform;

	private bool animationImpactDone;

	public GameObject emojiScreenGlitch;

	public TextMeshPro doorText;

	private string prevEmoji;

	private string currentEmoji;

	private float emojiScreenGlitchTimer;

	private float emojiDelay;

	private bool thirtyFPSUpdate;

	public Sound soundEmojiGlitch;

	private float thirtyFPSUpdateTimer;

	public List<Transform> fillBars = new List<Transform>();

	private float fillBarProgress;

	private float fillBarProgressPrev = -1f;

	private bool moveDone;

	public Transform animationTransform;

	[FormerlySerializedAs("light")]
	public Light doorLight;

	public Transform latchLamp1;

	public Transform latchLamp2;

	public GameObject grossupTransform;

	public ParticleSystem particlesCeiling;

	public Transform particlesOpen;

	public ParticleSystem particlesUnlock;

	public ParticleSystem particlesLatch1;

	public ParticleSystem particlesLatch2;

	public ParticleSystem particlesDoorSmoke;

	public ParticleSystem particlesBleep1;

	public ParticleSystem particlesBleep2;

	public ParticleSystem lightParticle;

	public ParticleSystem lightParticle2;

	public Sound soundGoUp;

	public Sound soundGoDown;

	public Sound soundSuccess;

	public Sound soundUnlock;

	public Sound soundUnlockEnd;

	public Sound soundLatches;

	public Sound soundLatchesEnd;

	public Sound soundDoorOpen;

	public Sound soundDoorMove;

	public Sound soundSlamCeiling;

	private void Start()
	{
		doorEndYPos = 7.42f;
		doorLight.intensity = 0f;
	}

	private void ThirtyFPS()
	{
		if (thirtyFPSUpdateTimer > 0f)
		{
			thirtyFPSUpdateTimer -= Time.deltaTime;
			thirtyFPSUpdateTimer = Mathf.Max(0f, thirtyFPSUpdateTimer);
		}
		else
		{
			thirtyFPSUpdate = true;
			thirtyFPSUpdateTimer = 1f / 30f;
		}
	}

	private void Update()
	{
		StateMachine();
		if (fillBarProgress != fillBarProgressPrev)
		{
			if (fillBarProgress > fillBarProgressPrev)
			{
				soundGoUp.Pitch = 1f + fillBarProgress / 11f;
				soundGoUp.Play(animationTransform.position);
				particlesBleep1.Play();
				particlesBleep2.Play();
			}
			else
			{
				soundGoDown.Pitch = 1f + fillBarProgress / 11f;
				soundGoDown.Play(animationTransform.position);
			}
			fillBarProgressPrev = fillBarProgress;
		}
	}

	private void StateMachine()
	{
		ThirtyFPS();
		switch (currentState)
		{
		case 0:
			StateClosed();
			break;
		case 1:
			StateSuccess();
			break;
		case 2:
			StateUnlock();
			break;
		case 3:
			StateOpening();
			break;
		case 4:
			StateOpen();
			break;
		}
		EmojiScreenGlitchLogic();
		thirtyFPSUpdate = false;
		stateTimer += Time.deltaTime;
		if (stateTimer > 1000000f)
		{
			stateTimer = 0f;
		}
	}

	private void StateSet(int _state)
	{
		prevState = currentState;
		stateTimer = 0f;
		currentState = _state;
		stateStart = true;
		animationDone = false;
		animationProgress = 0f;
		animationImpactDone = false;
	}

	private void EffectEmoji()
	{
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, animationTransform.position, 0.1f);
		soundSuccess.Play(animationTransform.position);
		lightParticle.transform.localPosition = new Vector3(-0.82f, 3.3f, 0f);
		lightParticle.Play();
		doorLight.color = new Color(1f, 0.5f, 0f, 1f);
		doorLight.range = 10f;
		doorLight.intensity = 4f;
	}

	private void EffectScreenRotateStart()
	{
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, animationTransform.position, 0.1f);
		soundUnlock.Play(animationTransform.position);
	}

	private void EffectScreenRotateEnd()
	{
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, animationTransform.position, 0.1f);
		soundUnlockEnd.Play(animationTransform.position);
		particlesUnlock.Play();
		lightParticle.transform.localPosition = new Vector3(-0.82f, 3.3f, 0f);
		lightParticle.Play();
	}

	private void EffectLatchStart()
	{
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, animationTransform.position, 0.1f);
		soundLatches.Play(animationTransform.position);
		lightParticle.transform.localPosition = new Vector3(-0.82f, 3.3f, 4.57f);
		lightParticle.Play();
		lightParticle2.transform.localPosition = new Vector3(-0.82f, 3.3f, -4.57f);
		lightParticle2.Play();
		latchLamp1.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 1f, 0f, 1f));
		latchLamp2.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 1f, 0f, 1f));
	}

	private void EffectLatchEnd()
	{
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		soundLatchesEnd.Play(base.transform.position);
		particlesLatch1.Play();
		particlesLatch2.Play();
		lightParticle.transform.localPosition = new Vector3(-0.82f, 3.3f, 3.3f);
		lightParticle.Play();
		lightParticle2.transform.localPosition = new Vector3(-0.82f, 3.3f, -3.3f);
		lightParticle2.Play();
	}

	private void EffectDoorOpenStart()
	{
		GameDirector.instance.CameraShake.ShakeDistance(8f, 3f, 8f, animationTransform.position, 0.1f);
		soundDoorOpen.Play(animationTransform.position);
		particlesDoorSmoke.Play();
	}

	private void EffectDoorMove()
	{
		soundDoorMove.Play(animationTransform.position);
		particlesOpen.gameObject.SetActive(value: true);
	}

	private void EffectDoorOpenEnd()
	{
		GameDirector.instance.CameraShake.ShakeDistance(8f, 3f, 8f, animationTransform.position, 0.1f);
		soundSlamCeiling.Play(animationTransform.position);
		particlesCeiling.Play();
		Object.Destroy(doorLight);
	}

	private void StateClosed()
	{
		if (TutorialDirector.instance.currentPage > tutorialPage)
		{
			StateSet(1);
		}
		float num = TutorialUI.instance.progressBarCurrent * 130f;
		fillBarProgress = Mathf.FloorToInt(num / 11f);
		for (int i = 0; i < fillBars.Count; i++)
		{
			fillBars[i].localScale = new Vector3(1f, 1f, Mathf.Clamp01(fillBarProgress / 11f));
		}
	}

	private void EmojiSet(string emoji)
	{
		doorText.text = "<size=100>|</size>" + emoji + "<size=100>|</size>";
	}

	private void EmojiScreenGlitch(Color color)
	{
		if (emojiScreenGlitchTimer <= 0f)
		{
			soundEmojiGlitch.Play(doorText.transform.position);
		}
		emojiScreenGlitchTimer = 0.2f;
		emojiScreenGlitch.SetActive(value: true);
		doorText.enabled = false;
		emojiScreenGlitch.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color);
	}

	private void EmojiScreenGlitchLogic()
	{
		if (emojiDelay > 0f)
		{
			return;
		}
		currentEmoji = doorText.text;
		if (prevEmoji != currentEmoji)
		{
			prevEmoji = currentEmoji;
			EmojiScreenGlitch(Color.yellow);
		}
		if (!(emojiScreenGlitchTimer <= 0f))
		{
			Vector2 textureOffset = emojiScreenGlitch.GetComponent<MeshRenderer>().material.GetTextureOffset("_MainTex");
			textureOffset.y += Time.deltaTime * 15f;
			emojiScreenGlitch.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", textureOffset);
			emojiScreenGlitchTimer -= Time.deltaTime;
			if (thirtyFPSUpdate)
			{
				float num = Random.Range(0.1f, 1f);
				emojiScreenGlitch.GetComponent<MeshRenderer>().material.SetTextureScale("_MainTex", new Vector2(num, num));
			}
			if (emojiScreenGlitchTimer <= 0f)
			{
				emojiScreenGlitch.SetActive(value: false);
				doorText.enabled = true;
			}
		}
	}

	private void StateSuccess()
	{
		if (stateStart)
		{
			EmojiSet("<sprite name=creepycrying>");
			EffectEmoji();
			grossupTransform.SetActive(value: true);
			stateStart = false;
			for (int i = 0; i < fillBars.Count; i++)
			{
				fillBars[i].localScale = new Vector3(1f, 1f, 1f);
			}
		}
		if (!animationDone)
		{
			if (animationProgress == 0f)
			{
				EffectScreenRotateStart();
			}
			animationProgress += 2f * Time.deltaTime;
			float t = animationCurve.Evaluate(animationProgress);
			screenTransform.localRotation = Quaternion.Euler(Mathf.LerpUnclamped(0f, 45f, t), 0f, 0f);
			if (animationProgress >= 0.53f && !animationImpactDone)
			{
				EffectScreenRotateEnd();
				animationImpactDone = true;
			}
			if (animationProgress >= 1f)
			{
				animationDone = true;
			}
		}
		if (stateTimer > 1f)
		{
			StateSet(2);
		}
	}

	private void StateUnlock()
	{
		if (stateStart)
		{
			EffectLatchStart();
			stateStart = false;
		}
		animationProgress += 1.5f * Time.deltaTime;
		float t = animationCurve.Evaluate(animationProgress);
		if (animationProgress > 0.53f && !animationImpactDone)
		{
			animationImpactDone = true;
			EffectLatchEnd();
		}
		latchTransform.localScale = new Vector3(latchTransform.localScale.x, latchTransform.localScale.y, Mathf.LerpUnclamped(1f, 0.8f, t));
		if (animationProgress >= 1f)
		{
			animationDone = true;
			StateSet(3);
		}
	}

	private void StateOpening()
	{
		if (stateStart)
		{
			EffectDoorOpenStart();
			stateStart = false;
		}
		if (animationProgress > 0.53f && !moveDone)
		{
			moveDone = true;
			EffectDoorMove();
		}
		animationProgress += Time.deltaTime;
		float t = animationCurveDoor.Evaluate(animationProgress);
		animationTransform.position = new Vector3(animationTransform.position.x, Mathf.LerpUnclamped(0f, doorEndYPos, t), animationTransform.position.z);
		if (animationProgress > 0.53f && !animationImpactDone)
		{
			animationImpactDone = true;
			EffectDoorOpenEnd();
		}
		if (animationProgress >= 1f)
		{
			animationDone = true;
			StateSet(4);
		}
	}

	private void StateOpen()
	{
	}
}
