using UnityEngine;

public class VacuumCleaner : MonoBehaviour
{
	public bool DebugNoSuck;

	[Space]
	public float SuckingTime;

	private float SuckingTimer;

	private bool Sucking;

	[Space]
	public ToolActiveOffset SuckingOffset;

	public VacuumCleanerBag VacuumCleanerBag;

	public ParticleSystem ParticleSystem;

	public Transform FollowTransform;

	public Transform ParentTransform;

	[Space]
	public AnimNoise SuckNoise;

	public float SuckNoiseAmount;

	[Space]
	public Sound IntroSound;

	public Sound OutroSound;

	public Sound LoopSound;

	public Sound LoopSuckSound;

	private float LoopSuckSoundTimer;

	public Sound LoopStartSound;

	public Sound LoopStopSound;

	private bool OutroAudioPlay = true;

	private void Start()
	{
		IntroSound.Play(FollowTransform.position);
		GameDirector.instance.CameraShake.Shake(3f, 0.25f);
		SuckingTimer = 1f;
	}

	private void Update()
	{
		if (ToolController.instance.Interact)
		{
			Interaction activeInteraction = ToolController.instance.ActiveInteraction;
			if ((bool)activeInteraction)
			{
				VacuumSpotInteraction component = activeInteraction.GetComponent<VacuumSpotInteraction>();
				if ((bool)component && !DebugNoSuck)
				{
					component.VacuumSpot.cleanInput = true;
					LoopSuckSoundTimer = 0.1f;
					if (component.VacuumSpot.Amount > 0.2f)
					{
						SuckingTimer = Mathf.Max(SuckingTimer, SuckingTime);
					}
					else if (!component.VacuumSpot.CleanDone)
					{
						component.VacuumSpot.CleanDone = true;
					}
				}
			}
		}
		if (LoopSuckSoundTimer > 0f)
		{
			LoopSuckSoundTimer -= Time.deltaTime;
			LoopSuckSound.PlayLoop(playing: true, 5f, 5f);
		}
		else
		{
			LoopSuckSound.PlayLoop(playing: false, 5f, 5f);
		}
		if (SuckingTimer > 0f)
		{
			SuckingTimer -= Time.deltaTime;
			if (!Sucking)
			{
				GameDirector.instance.CameraShake.Shake(3f, 0.25f);
				Sucking = true;
				VacuumCleanerBag.Active = true;
				SuckingOffset.Active = true;
				if (OutroAudioPlay)
				{
					ParticleSystem.Play();
				}
				LoopStartSound.Play(FollowTransform.position);
			}
			GameDirector.instance.CameraShake.Shake(1f, 0.25f);
			SuckNoise.noiseStrengthDefault = Mathf.Lerp(SuckNoise.noiseStrengthDefault, SuckNoiseAmount, 5f * Time.deltaTime);
		}
		else
		{
			if (Sucking)
			{
				GameDirector.instance.CameraShake.Shake(3f, 0.25f);
				LoopStopSound.Play(FollowTransform.position);
				VacuumCleanerBag.Active = false;
				SuckingOffset.Active = false;
				if (OutroAudioPlay)
				{
					ParticleSystem.Stop();
				}
				Sucking = false;
			}
			SuckNoise.noiseStrengthDefault = Mathf.Lerp(SuckNoise.noiseStrengthDefault, 0f, 5f * Time.deltaTime);
		}
		LoopSound.PlayLoop(Sucking, 0.5f, 0.5f);
		if (OutroAudioPlay && !ToolController.instance.ToolHide.Active)
		{
			if (ParticleSystem != null && ParticleSystem.isPlaying)
			{
				ParticleSystem.gameObject.transform.parent = null;
				ParticleSystem.MainModule main = ParticleSystem.main;
				main.stopAction = ParticleSystemStopAction.Destroy;
				ParticleSystem.Stop();
				ParticleSystem = null;
			}
			OutroSound.Play(FollowTransform.position);
			OutroAudioPlay = false;
			GameDirector.instance.CameraShake.Shake(3f, 0.25f);
		}
		FollowTransform.position = ToolController.instance.ToolFollow.transform.position;
		FollowTransform.rotation = ToolController.instance.ToolFollow.transform.rotation;
		FollowTransform.localScale = ToolController.instance.ToolHide.transform.localScale;
		ParentTransform.transform.position = ToolController.instance.ToolTargetParent.transform.position;
		ParentTransform.transform.rotation = ToolController.instance.ToolTargetParent.transform.rotation;
	}
}
