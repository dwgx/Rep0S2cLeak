using System.Collections.Generic;
using UnityEngine;

public class ValuableSpiderPotion : MonoBehaviour
{
	public ParticleSystem destroyParticles;

	public ParticleSystem destroyParticlesArachnophobia;

	[Space]
	public GameObject particlesOnOtherPlayers;

	private PhysGrabObject physgrabObject;

	[Space]
	public PhysAudio physAudio;

	public PhysAudio physAudioArachnophobia;

	private List<PlayerAvatar> allPlayers;

	private int playerCount;

	private float activationRadius = 3f;

	private Vector3 valuablePosition;

	private List<PlayerAvatar> affectedPlayers;

	[Header("Screen Spider Effect")]
	public GameObject screenSpiderEffect;

	private void Awake()
	{
		physgrabObject = GetComponent<PhysGrabObject>();
	}

	public void DestroyParticles()
	{
		if (SemiFunc.Arachnophobia())
		{
			destroyParticlesArachnophobia.gameObject.transform.parent = null;
			destroyParticlesArachnophobia.Play();
		}
		else
		{
			destroyParticles.gameObject.transform.parent = null;
			destroyParticles.Play();
		}
		ActivateScreenSpiderEffect();
	}

	private void ActivateScreenSpiderEffect()
	{
		allPlayers = SemiFunc.PlayerGetList();
		valuablePosition = base.transform.position;
		affectedPlayers = new List<PlayerAvatar>();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			if (!(Vector3.Distance(valuablePosition, allPlayers[i].transform.position) > activationRadius))
			{
				affectedPlayers.Add(allPlayers[i]);
			}
		}
		for (int j = 0; j < affectedPlayers.Count; j++)
		{
			if (affectedPlayers[j].isLocal)
			{
				CameraGlitch.Instance.PlayLong();
				Transform transform = affectedPlayers[j].localCamera.transform;
				GameObject obj = Object.Instantiate(screenSpiderEffect, transform.position, Quaternion.identity, transform);
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localRotation = Quaternion.identity;
				obj.GetComponent<ScreenSpiderEffect>().playerAvatar = affectedPlayers[j];
			}
		}
		for (int k = 0; k < allPlayers.Count; k++)
		{
			if (allPlayers[k].isLocal)
			{
				SpawnParticlesOnAffectedPlayers(allPlayers[k]);
			}
		}
	}

	private void SpawnParticlesOnAffectedPlayers(PlayerAvatar localPlayer)
	{
		for (int i = 0; i < affectedPlayers.Count; i++)
		{
			if (affectedPlayers[i] != localPlayer)
			{
				Object.Instantiate(particlesOnOtherPlayers, affectedPlayers[i].transform.position, Quaternion.identity).GetComponent<SpiderSoundLoop>().playerAvatar = affectedPlayers[i];
			}
		}
	}

	public void ArachnophobiaActive()
	{
		physgrabObject.impactDetector.impactAudio = physAudioArachnophobia;
	}

	public void ArachnophobiaInactive()
	{
		physgrabObject.impactDetector.impactAudio = physAudio;
	}
}
