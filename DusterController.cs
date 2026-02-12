using UnityEngine;

public class DusterController : MonoBehaviour
{
	public Transform FollowTransform;

	public Transform ParentTransform;

	[Space]
	public ToolActiveOffset ToolActiveOffset;

	public DusterDusting DusterDusting;

	public ToolBackAway ToolBackAway;

	private bool Dusting;

	private float DustingTimer;

	[Space]
	public Sound MoveSound;

	private bool OutroAudioPlay = true;

	private void Start()
	{
		GameDirector.instance.CameraShake.Shake(2f, 0.25f);
		MoveSound.Play(base.transform.position);
	}

	private void Update()
	{
		if (ToolController.instance.Interact && ToolController.instance.ToolHide.Active && ToolController.instance.ToolHide.ActiveLerp > 0.75f)
		{
			Interaction activeInteraction = ToolController.instance.ActiveInteraction;
			if ((bool)activeInteraction)
			{
				DirtyPainting component = activeInteraction.GetComponent<DirtyPainting>();
				if ((bool)component)
				{
					CanvasHandler canvasHandler = component.CanvasHandler;
					if ((bool)canvasHandler && canvasHandler.currentState != CanvasHandler.State.Clean)
					{
						DustingTimer = 0.5f;
						if (DusterDusting.ActiveAmount >= 0.1f)
						{
							canvasHandler.cleanInput = true;
							if (!canvasHandler.CleanDone && canvasHandler.fadeMultiplier <= 0.5f)
							{
								canvasHandler.CleanDone = true;
							}
						}
					}
				}
			}
		}
		if (DustingTimer > 0f)
		{
			ToolActiveOffset.Active = true;
			ToolBackAway.Active = true;
			Dusting = true;
			DustingTimer -= Time.deltaTime;
			if (DustingTimer <= 0f)
			{
				Dusting = false;
				ToolActiveOffset.Active = false;
				ToolBackAway.Active = false;
			}
		}
		if (Dusting && ToolActiveOffset.Active && ToolActiveOffset.ActiveLerp >= 0.3f)
		{
			DusterDusting.Active = true;
		}
		else
		{
			DusterDusting.Active = false;
		}
		FollowTransform.position = ToolController.instance.ToolFollow.transform.position;
		FollowTransform.rotation = ToolController.instance.ToolFollow.transform.rotation;
		FollowTransform.localScale = ToolController.instance.ToolHide.transform.localScale;
		ParentTransform.transform.position = ToolController.instance.ToolTargetParent.transform.position;
		ParentTransform.transform.rotation = ToolController.instance.ToolTargetParent.transform.rotation;
		if (OutroAudioPlay && !ToolController.instance.ToolHide.Active)
		{
			MoveSound.Play(FollowTransform.position);
			OutroAudioPlay = false;
			GameDirector.instance.CameraShake.Shake(2f, 0.25f);
		}
	}
}
