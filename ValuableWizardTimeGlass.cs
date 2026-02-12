using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class ValuableWizardTimeGlass : MonoBehaviour
{
	public enum States
	{
		Idle,
		Active
	}

	private PhysGrabObject physGrabObject;

	private PhotonView photonView;

	internal States currentState;

	private bool stateStart;

	public Transform particleSystemTransform;

	public ParticleSystem particleSystemSwirl;

	public ParticleSystem particleSystemGlitter;

	public MeshRenderer timeGlassMaterial;

	[FormerlySerializedAs("light")]
	public Light timeGlassLight;

	public Sound soundTimeGlassLoop;

	private float soundPitchLerp;

	private int particleFocus;

	private void StateActive()
	{
		if (stateStart)
		{
			particleSystemGlitter.Play();
			particleSystemSwirl.Play();
			stateStart = false;
			timeGlassLight.gameObject.SetActive(value: true);
		}
		if (!timeGlassLight.gameObject.activeSelf)
		{
			timeGlassLight.gameObject.SetActive(value: true);
		}
		if (particleSystemTransform.gameObject.activeSelf)
		{
			List<PhysGrabber> playerGrabbing = physGrabObject.playerGrabbing;
			if (playerGrabbing.Count > particleFocus)
			{
				PhysGrabber physGrabber = playerGrabbing[particleFocus];
				if ((bool)physGrabber)
				{
					Transform headLookAtTransform = physGrabber.playerAvatar.playerAvatarVisuals.headLookAtTransform;
					if ((bool)headLookAtTransform)
					{
						particleSystemTransform.LookAt(headLookAtTransform);
					}
					particleFocus++;
				}
				else
				{
					particleFocus = 0;
				}
			}
			else
			{
				particleFocus = 0;
			}
		}
		soundPitchLerp = Mathf.Lerp(soundPitchLerp, 1f, Time.deltaTime * 2f);
		timeGlassLight.intensity = Mathf.Lerp(timeGlassLight.intensity, 4f, Time.deltaTime * 2f);
		Color color = new Color(0.5f, 0f, 1f);
		timeGlassMaterial.material.SetColor("_EmissionColor", color * timeGlassLight.intensity);
		foreach (PhysGrabber item in physGrabObject.playerGrabbing)
		{
			if ((bool)item && !item.isLocal)
			{
				item.playerAvatar.voiceChat.OverridePitch(0.65f, 1f, 2f);
			}
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			physGrabObject.OverrideDrag(20f);
			physGrabObject.OverrideAngularDrag(40f);
			if (!physGrabObject.grabbed)
			{
				SetState(States.Idle);
			}
		}
		if (physGrabObject.grabbedLocal)
		{
			PlayerAvatar instance = PlayerAvatar.instance;
			if ((bool)instance.voiceChat)
			{
				instance.voiceChat.OverridePitch(0.65f, 1f, 2f);
			}
			instance.OverridePupilSize(3f, 4, 1f, 1f, 5f, 0.5f);
			PlayerController.instance.OverrideSpeed(0.5f);
			PlayerController.instance.OverrideLookSpeed(0.5f, 2f, 1f);
			PlayerController.instance.OverrideAnimationSpeed(0.2f, 1f, 2f);
			PlayerController.instance.OverrideTimeScale(0.1f);
			physGrabObject.OverrideTorqueStrength(0.6f);
			CameraZoom.Instance.OverrideZoomSet(50f, 0.1f, 0.5f, 1f, base.gameObject, 0);
			PostProcessing.Instance.SaturationOverride(50f, 0.1f, 0.5f, 0.1f, base.gameObject);
		}
	}

	private void StateIdle()
	{
		if (stateStart)
		{
			particleSystemGlitter.Stop();
			particleSystemSwirl.Stop();
			stateStart = false;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && physGrabObject.grabbed)
		{
			SetState(States.Active);
		}
		timeGlassLight.intensity = Mathf.Lerp(timeGlassLight.intensity, 0f, Time.deltaTime * 10f);
		soundPitchLerp = Mathf.Lerp(soundPitchLerp, 0f, Time.deltaTime * 10f);
		Color color = new Color(0.5f, 0f, 1f);
		timeGlassMaterial.material.SetColor("_EmissionColor", color * timeGlassLight.intensity);
		if (timeGlassLight.intensity < 0.01f)
		{
			timeGlassLight.gameObject.SetActive(value: false);
		}
	}

	[PunRPC]
	public void SetStateRPC(States state)
	{
		currentState = state;
		stateStart = true;
	}

	private void SetState(States state)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				SetStateRPC(state);
				return;
			}
			photonView.RPC("SetStateRPC", RpcTarget.All, state);
		}
	}

	private void Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
		photonView = GetComponent<PhotonView>();
	}

	private void Update()
	{
		float pitchMultiplier = Mathf.Lerp(2f, 0.5f, soundPitchLerp);
		soundTimeGlassLoop.PlayLoop(currentState == States.Active, 0.8f, 0.8f, pitchMultiplier);
		switch (currentState)
		{
		case States.Active:
			StateActive();
			break;
		case States.Idle:
			StateIdle();
			break;
		}
	}
}
