using UnityEngine;

public class SledgehammerController : MonoBehaviour
{
	public Transform ControllerTransform;

	public Transform FollowTransform;

	[Space]
	public SledgehammerSwing Swing;

	public SledgehammerHit Hit;

	public SledgehammerTransition Transition;

	private RoachTrigger Roach;

	[Space]
	[Header("Sounds")]
	public Sound SoundMoveLong;

	public Sound SoundMoveShort;

	public Sound SoundSwing;

	public Sound SoundHit;

	public Sound SoundHitOutro;

	private bool OutroAudioPlay = true;

	private LayerMask Mask;

	private Camera MainCamera;

	private bool InteractImpulse;

	private void Start()
	{
		MainCamera = Camera.main;
		Mask = LayerMask.GetMask("Default");
		Hit.gameObject.SetActive(value: false);
		Transition.gameObject.SetActive(value: false);
		SoundMoveLong.Play(base.transform.position);
		GameDirector.instance.CameraShake.Shake(2f, 0.25f);
	}

	private void OnTriggerStay(Collider other)
	{
		if (!ToolController.instance.ToolHide.Active || !Swing.CanHit)
		{
			return;
		}
		RoachTrigger component = other.GetComponent<RoachTrigger>();
		if ((bool)component)
		{
			_ = (component.transform.position - MainCamera.transform.position).magnitude;
			_ = (component.transform.position - MainCamera.transform.position).normalized;
			if (!Physics.Raycast(MainCamera.transform.position, (component.transform.position - MainCamera.transform.position).normalized, out var _, (component.transform.position - MainCamera.transform.position).magnitude, Mask))
			{
				Roach = component;
				Swing.HitOutro();
				Swing.CanHit = false;
				Transition.gameObject.SetActive(value: true);
				Transition.IntroSet();
				Hit.gameObject.SetActive(value: true);
				Hit.Spawn(Roach);
			}
		}
	}

	public void HitDone()
	{
		Transition.gameObject.SetActive(value: true);
		Transition.OutroSet();
		GameDirector.instance.CameraImpact.Shake(3f, 0f);
		GameDirector.instance.CameraShake.Shake(2f, 0.25f);
		SoundHitOutro.Play(base.transform.position);
		Hit.gameObject.SetActive(value: false);
	}

	public void IntroDone()
	{
		GameDirector.instance.CameraImpact.Shake(5f, 0f);
		GameDirector.instance.CameraShake.Shake(2f, 0.25f);
		Hit.gameObject.SetActive(value: true);
		Hit.Hit();
		Roach.RoachOrbit.Squash();
	}

	public void OutroDone()
	{
		Swing.gameObject.SetActive(value: true);
		Swing.MeshTransform.gameObject.SetActive(value: true);
		PlayerController.instance.MoveForce(PlayerController.instance.transform.forward, -5f, 0.25f);
	}

	private void Update()
	{
		if (Hit.gameObject.activeSelf)
		{
			float magnitude = (Hit.transform.position - PlayerController.instance.transform.position).magnitude;
			PlayerController.instance.MoveForce(Hit.transform.position - PlayerController.instance.transform.position, magnitude * 30f, 0.01f);
			PlayerController.instance.InputDisable(0.1f);
		}
		if (ToolController.instance.Interact && !Swing.Swinging)
		{
			InteractImpulse = true;
		}
		if (InteractImpulse && ToolController.instance.ToolHide.Active && ToolController.instance.ToolHide.ActiveLerp >= 1f)
		{
			InteractImpulse = false;
			Swing.Swing();
		}
		if (Swing.Swinging || Hit.gameObject.activeSelf)
		{
			ToolController.instance.ForceActiveTimer = 0.1f;
		}
		ControllerTransform.transform.position = ToolController.instance.ToolTargetParent.transform.position;
		ControllerTransform.transform.rotation = ToolController.instance.ToolTargetParent.transform.rotation;
		FollowTransform.position = ToolController.instance.ToolFollow.transform.position;
		FollowTransform.rotation = ToolController.instance.ToolFollow.transform.rotation;
		FollowTransform.localScale = ToolController.instance.ToolHide.transform.localScale;
		if (OutroAudioPlay && !ToolController.instance.ToolHide.Active)
		{
			SoundMoveLong.Play(FollowTransform.position);
			OutroAudioPlay = false;
			GameDirector.instance.CameraShake.Shake(2f, 0.25f);
		}
	}
}
