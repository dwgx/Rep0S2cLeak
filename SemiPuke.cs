using System.Collections.Generic;
using UnityEngine;

public class SemiPuke : MonoBehaviour
{
	public enum State
	{
		None,
		PukeStart,
		Puke,
		PukeEnd
	}

	public State state;

	public Transform BaseParticlesTransform;

	private float pukeActiveTimer;

	private bool stateStart;

	private bool baseParticlesPlaying;

	public Light pukeLight;

	public List<ParticleSystem> pukeParticles = new List<ParticleSystem>();

	public ParticleSystem pukeEnd = new ParticleSystem();

	private List<ParticleSystem> baseParticles = new List<ParticleSystem>();

	public Sound soundPukeStart;

	public Sound soundPukeEnd;

	public Sound soundPukeLoop;

	private void StateNone()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		PlayBaseParticles(_play: false);
		if (pukeActiveTimer > 0f)
		{
			StateSet(State.PukeStart);
		}
	}

	private void StatePukeStart()
	{
		if (stateStart)
		{
			soundPukeStart.Play(base.transform.position);
			pukeLight.enabled = true;
			pukeLight.intensity = 0f;
			PlayAllParticles(_play: true);
			stateStart = false;
		}
		PlayBaseParticles(_play: true);
		StateSet(State.Puke);
	}

	private void StatePuke()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		PlayBaseParticles(_play: true);
		pukeLight.intensity = Mathf.Lerp(pukeLight.intensity, 1f, Time.deltaTime * 20f);
		pukeLight.intensity += Mathf.Sin(Time.time * 20f) * 0.05f;
		if (pukeActiveTimer <= 0f)
		{
			StateSet(State.PukeEnd);
		}
	}

	private void StatePukeEnd()
	{
		if (stateStart)
		{
			stateStart = false;
			soundPukeEnd.Play(base.transform.position);
			pukeLight.enabled = false;
			PlayAllParticles(_play: false);
			pukeEnd.Play();
		}
		PlayBaseParticles(_play: false);
		pukeLight.intensity = Mathf.Lerp(pukeLight.intensity, 0f, Time.deltaTime * 40f);
		if (pukeLight.intensity < 0.01f)
		{
			pukeLight.intensity = 0f;
			StateSet(State.None);
		}
	}

	private void PlayAllParticles(bool _play)
	{
		foreach (ParticleSystem pukeParticle in pukeParticles)
		{
			if (_play)
			{
				pukeParticle.Play();
			}
			else
			{
				pukeParticle.Stop();
			}
		}
	}

	private void StateMachine()
	{
		bool playing = pukeActiveTimer > 0f;
		soundPukeLoop.PlayLoop(playing, 2f, 2f);
		if (pukeActiveTimer > 0f)
		{
			pukeActiveTimer -= Time.deltaTime;
		}
		switch (state)
		{
		case State.None:
			StateNone();
			break;
		case State.PukeStart:
			StatePukeStart();
			break;
		case State.Puke:
			StatePuke();
			break;
		case State.PukeEnd:
			StatePukeEnd();
			break;
		}
	}

	private void Start()
	{
		baseParticles = new List<ParticleSystem>(BaseParticlesTransform.GetComponentsInChildren<ParticleSystem>());
	}

	private void Update()
	{
		StateMachine();
	}

	private void PlayBaseParticles(bool _play)
	{
		if (baseParticlesPlaying == _play)
		{
			return;
		}
		foreach (ParticleSystem baseParticle in baseParticles)
		{
			if (_play)
			{
				baseParticle.Play();
			}
			else
			{
				baseParticle.Stop();
			}
		}
		baseParticlesPlaying = _play;
	}

	private void StateSet(State _state)
	{
		if (state != _state)
		{
			state = _state;
			stateStart = true;
		}
	}

	public void PukeActive(Vector3 _position, Quaternion _direction)
	{
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		base.transform.position = _position;
		base.transform.rotation = _direction;
		pukeActiveTimer = 0.2f;
	}
}
