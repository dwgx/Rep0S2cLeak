using System.Collections.Generic;
using UnityEngine;

public class TruckHealer : MonoBehaviour
{
	public enum State
	{
		Closed,
		Opening,
		Open,
		Closing
	}

	public Transform hatch1Transform;

	public Transform hatch2Transform;

	public Light healerLight;

	public AnimationCurve hatchCurve;

	public Transform healSphere;

	public Transform healSpherePulseParent;

	public ParticleSystem swirlParticles;

	private MeshRenderer healSphereRenderer;

	private float hatchAnimEval;

	private float zRotationHatch1Open;

	private float zRotationHatch2Open;

	private float lightIntensityOriginal;

	private float healSphereSizeOriginal;

	private bool hatchClosedEffect;

	private bool hatchOpenedEffect;

	public ParticleSystem partSmokeCeiling;

	public ParticleSystem PartSmokeCeilingPoof;

	public Transform healParticles;

	private List<ParticleSystem> healParticlesList = new List<ParticleSystem>();

	public GameObject healerBeamPrefab;

	public Transform healerBeamOrigin;

	public Sound soundOpen;

	public Sound soundClose;

	public Sound soundSlam;

	public Sound soundLoop;

	private bool allHealingDone;

	public static TruckHealer instance;

	internal State currentState;

	private bool stateStart = true;

	private void Start()
	{
		healSphereRenderer = healSphere.GetComponent<MeshRenderer>();
		zRotationHatch1Open = hatch1Transform.localEulerAngles.z;
		zRotationHatch2Open = hatch2Transform.localEulerAngles.z;
		hatch1Transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		hatch2Transform.localEulerAngles = new Vector3(0f, 180f, 0f);
		lightIntensityOriginal = healerLight.intensity;
		healerLight.intensity = 0f;
		healerLight.enabled = false;
		healSphereSizeOriginal = healSphere.localScale.x;
		healSphere.localScale = new Vector3(0f, 0f, 0f);
		healSphere.gameObject.SetActive(value: false);
		healParticlesList.AddRange(healParticles.GetComponentsInChildren<ParticleSystem>());
		instance = this;
	}

	private void StateClosed()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		else if (!allHealingDone && SemiFunc.RunIsLevel() && RoundDirector.instance.extractionPointsCompleted == RoundDirector.instance.extractionPoints)
		{
			StateUpdate(State.Opening);
		}
	}

	private void PlayHealParticles()
	{
		foreach (ParticleSystem healParticles in healParticlesList)
		{
			healParticles.Play();
		}
	}

	private void StateOpening()
	{
		if (stateStart)
		{
			stateStart = false;
			hatchAnimEval = 0f;
			healerLight.enabled = true;
			swirlParticles.Play();
			hatchOpenedEffect = false;
			healSphere.gameObject.SetActive(value: true);
			soundOpen.Play(healerBeamOrigin.position);
		}
		if (hatchAnimEval < 1f)
		{
			hatchAnimEval += Time.deltaTime * 2f;
			if (hatchAnimEval > 0.8f && !hatchOpenedEffect)
			{
				hatchOpenedEffect = true;
				SemiFunc.CameraShakeImpactDistance(base.transform.position, 4f, 0.1f, 6f, 15f);
				partSmokeCeiling.Play();
				PartSmokeCeilingPoof.Play();
				soundSlam.Play(healerBeamOrigin.position);
			}
			if (healerLight.intensity < lightIntensityOriginal - 0.01f)
			{
				healerLight.intensity = Mathf.Lerp(healerLight.intensity, lightIntensityOriginal, hatchCurve.Evaluate(hatchAnimEval));
			}
			if (healSphere.localScale.x < healSphereSizeOriginal - 0.01f)
			{
				healSphere.localScale = new Vector3(Mathf.Lerp(0f, healSphereSizeOriginal, hatchCurve.Evaluate(hatchAnimEval)), Mathf.Lerp(0f, healSphereSizeOriginal, hatchCurve.Evaluate(hatchAnimEval)), Mathf.Lerp(0f, healSphereSizeOriginal, hatchCurve.Evaluate(hatchAnimEval)));
			}
			else
			{
				healSphere.localScale = new Vector3(healSphereSizeOriginal, healSphereSizeOriginal, healSphereSizeOriginal);
			}
			hatch1Transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, zRotationHatch1Open, hatchCurve.Evaluate(hatchAnimEval)));
			hatch2Transform.localEulerAngles = new Vector3(0f, 180f, Mathf.Lerp(0f, zRotationHatch2Open, hatchCurve.Evaluate(hatchAnimEval)));
		}
		else
		{
			StateUpdate(State.Open);
		}
	}

	private void StateOpen()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		if (!SemiFunc.FPSImpulse5())
		{
			return;
		}
		List<PlayerAvatar> list = SemiFunc.PlayerGetAll();
		int count = list.Count;
		int num = 0;
		foreach (PlayerAvatar item in list)
		{
			if (item.finalHeal)
			{
				num++;
			}
		}
		if (num >= count)
		{
			allHealingDone = true;
			StateUpdate(State.Closing);
		}
	}

	private void StateClosing()
	{
		if (stateStart)
		{
			stateStart = false;
			swirlParticles.Stop();
			hatchClosedEffect = false;
			hatchAnimEval = 0f;
			soundClose.Play(healerBeamOrigin.position);
		}
		if (hatchAnimEval < 1f)
		{
			hatchAnimEval += Time.deltaTime * 2f;
			if (healerLight.intensity > 0.01f)
			{
				healerLight.intensity = Mathf.Lerp(healerLight.intensity, 0f, hatchCurve.Evaluate(hatchAnimEval));
			}
			else
			{
				healerLight.enabled = false;
			}
			if (hatchAnimEval > 0.8f && !hatchClosedEffect)
			{
				hatchClosedEffect = true;
				SemiFunc.CameraShakeImpactDistance(base.transform.position, 4f, 0.1f, 6f, 15f);
				partSmokeCeiling.Play();
				PartSmokeCeilingPoof.Play();
				soundSlam.Play(healerBeamOrigin.position);
			}
			if (healSphere.localScale.x > 0.01f)
			{
				healSphere.localScale = new Vector3(Mathf.Lerp(healSphereSizeOriginal, 0f, hatchCurve.Evaluate(hatchAnimEval)), Mathf.Lerp(healSphereSizeOriginal, 0f, hatchCurve.Evaluate(hatchAnimEval)), Mathf.Lerp(healSphereSizeOriginal, 0f, hatchCurve.Evaluate(hatchAnimEval)));
			}
			else
			{
				healSphere.localScale = new Vector3(0f, 0f, 0f);
				healSphere.gameObject.SetActive(value: false);
			}
			hatch1Transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(zRotationHatch1Open, 0f, hatchCurve.Evaluate(hatchAnimEval)));
			hatch2Transform.localEulerAngles = new Vector3(0f, 180f, Mathf.Lerp(zRotationHatch2Open, 0f, hatchCurve.Evaluate(hatchAnimEval)));
		}
		else
		{
			StateUpdate(State.Closed);
		}
	}

	private void StateMachine()
	{
		switch (currentState)
		{
		case State.Closed:
			StateClosed();
			break;
		case State.Opening:
			StateOpening();
			break;
		case State.Open:
			StateOpen();
			break;
		case State.Closing:
			StateClosing();
			break;
		}
	}

	private void Update()
	{
		bool playing = currentState == State.Open;
		soundLoop.PlayLoop(playing, 2f, 2f);
		StateMachine();
		ScrollSphereTexture();
	}

	private void StateUpdate(State _newState)
	{
		if (currentState != _newState)
		{
			currentState = _newState;
			stateStart = true;
		}
	}

	private void ScrollSphereTexture()
	{
		if (healSphereRenderer.gameObject.activeSelf)
		{
			healSphereRenderer.material.mainTextureOffset = new Vector2(healSphereRenderer.material.mainTextureOffset.x, healSphereRenderer.material.mainTextureOffset.y + Time.deltaTime * 0.1f);
			healSpherePulseParent.localScale = new Vector3(1f + Mathf.Sin(Time.time * 5f) * 0.1f, 1f + Mathf.Sin(Time.time * 5f) * 0.1f, 1f + Mathf.Sin(Time.time * 5f) * 0.1f);
			healSpherePulseParent.localEulerAngles = new Vector3(0f, Time.time * 200f, 0f);
		}
	}

	public void Heal(PlayerAvatar _playerAvatar)
	{
		if (currentState == State.Open)
		{
			TruckHealerLine component = Object.Instantiate(healerBeamPrefab, healerBeamOrigin.position, Quaternion.identity).GetComponent<TruckHealerLine>();
			if (!_playerAvatar.isLocal)
			{
				component.lineTarget = _playerAvatar.playerAvatarVisuals.attachPointTopHeadMiddle;
			}
			else
			{
				component.lineTarget = _playerAvatar.localCamera.transform;
			}
			PlayHealParticles();
		}
	}
}
