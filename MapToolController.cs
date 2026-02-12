using Photon.Pun;
using UnityEngine;

public class MapToolController : MonoBehaviour
{
	public static MapToolController instance;

	internal bool Active;

	private bool ActivePrev;

	internal PhotonView photonView;

	public PlayerAvatar PlayerAvatar;

	[Space]
	public Transform FollowTransform;

	public Transform FollowTransformClient;

	[Space]
	public Transform ControllerTransform;

	public Transform VisualTransform;

	public Transform PlayerLookTarget;

	public Transform HideTransform;

	[Space]
	private float DisplayJointAngleDiff;

	private float DisplayJointAnglePreviousX;

	[Space]
	public float MoveMultiplier = 0.5f;

	public float FadeAmount = 0.5f;

	public float BobMultiplier = 0.1f;

	[Space]
	public Sound SoundStart;

	public Sound SoundStop;

	public Sound SoundLoop;

	[Space]
	public MeshRenderer DisplayMesh;

	public Material DisplayMaterial;

	public Material DisplayMaterialClient;

	[Space]
	public AnimationCurve IntroCurve;

	public float IntroSpeed;

	public AnimationCurve OutroCurve;

	public float OutroSpeed;

	internal float HideLerp;

	private float HideScale;

	[Space]
	public Transform displaySpringTransform;

	public Transform displaySpringTransformTarget;

	public SpringQuaternion displaySpring;

	[Space]
	public Transform mainSpringTransform;

	public Transform mainSpringTransformTarget;

	public SpringQuaternion mainSpring;

	private bool mapToggled;

	private void Start()
	{
		VisualTransform.gameObject.SetActive(value: false);
		photonView = GetComponent<PhotonView>();
		if (!GameManager.Multiplayer() || photonView.IsMine)
		{
			DisplayMesh.material = DisplayMaterial;
			base.transform.parent.parent = FollowTransform;
			base.transform.parent.localPosition = Vector3.zero;
			base.transform.parent.localRotation = Quaternion.identity;
			return;
		}
		DisplayMesh.material = DisplayMaterialClient;
		Transform[] componentsInChildren = VisualTransform.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer("Triggers");
		}
		SoundStart.SpatialBlend = 1f;
		SoundStart.Volume *= 0.3f;
		SoundStart.VolumeRandom *= 0.3f;
		SoundStart.OffscreenFalloff = 0.6f;
		SoundStart.OffscreenVolume = 0.6f;
		SoundStop.SpatialBlend = 1f;
		SoundStop.Volume *= 0.3f;
		SoundStop.VolumeRandom *= 0.3f;
		SoundStop.OffscreenFalloff = 0.6f;
		SoundStop.OffscreenVolume = 0.6f;
		SoundLoop.SpatialBlend = 1f;
		SoundLoop.Volume *= 0.3f;
		SoundLoop.VolumeRandom *= 0.3f;
		SoundLoop.OffscreenFalloff = 0.6f;
		SoundLoop.OffscreenVolume = 0.6f;
	}

	private void Update()
	{
		if (!GameManager.Multiplayer() || photonView.IsMine)
		{
			if (!PlayerAvatar.isDisabled && !PlayerAvatar.isTumbling && !CameraAim.Instance.AimTargetActive && !SemiFunc.MenuLevel() && SemiFunc.NoTextInputsActive())
			{
				if (InputManager.instance.InputToggleGet(InputKey.Map))
				{
					if (SemiFunc.InputDown(InputKey.Map))
					{
						mapToggled = !mapToggled;
					}
				}
				else
				{
					mapToggled = false;
				}
				if ((SemiFunc.InputHold(InputKey.Map) || mapToggled || Map.Instance.debugActive) && !PlayerController.instance.sprinting)
				{
					if (HideLerp >= 1f)
					{
						Active = true;
					}
				}
				else
				{
					mapToggled = false;
					if (HideLerp <= 0f)
					{
						Active = false;
					}
				}
			}
			else
			{
				Active = false;
			}
			if (Active)
			{
				StatsUI.instance.Show();
				ItemInfoUI.instance.Hide();
				ItemInfoExtraUI.instance.Hide();
				if (MissionUI.instance.Text.text != "")
				{
					MissionUI.instance.Show();
				}
			}
		}
		if (Active != ActivePrev)
		{
			ActivePrev = Active;
			if (GameManager.Multiplayer() && photonView.IsMine)
			{
				photonView.RPC("SetActiveRPC", RpcTarget.Others, Active);
			}
			if (Active)
			{
				if (!GameManager.Multiplayer() || photonView.IsMine)
				{
					GameDirector.instance.CameraShake.Shake(2f, 0.1f);
					Map.Instance.ActiveSet(active: true);
				}
				VisualTransform.gameObject.SetActive(value: true);
				SoundStart.Play(base.transform.position);
			}
			else
			{
				if (!GameManager.Multiplayer() || photonView.IsMine)
				{
					GameDirector.instance.CameraShake.Shake(2f, 0.1f);
					Map.Instance.ActiveSet(active: false);
				}
				SoundStop.Play(base.transform.position);
			}
		}
		float x = 90f;
		if (GameManager.Multiplayer() && !photonView.IsMine)
		{
			x = 0f;
		}
		float num = 1f;
		if (GameManager.Multiplayer() && !photonView.IsMine)
		{
			num = 2f;
		}
		if (Active)
		{
			if (HideLerp > 0f)
			{
				HideLerp -= Time.deltaTime * IntroSpeed * num;
			}
			HideScale = Mathf.LerpUnclamped(1f, 0f, IntroCurve.Evaluate(HideLerp));
			HideTransform.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(x, 0f, 0f), IntroCurve.Evaluate(HideLerp));
		}
		else
		{
			if (HideLerp < 1f)
			{
				HideLerp += Time.deltaTime * OutroSpeed * num;
				if (HideLerp > 1f)
				{
					VisualTransform.gameObject.SetActive(value: false);
				}
			}
			HideScale = Mathf.LerpUnclamped(1f, 0f, OutroCurve.Evaluate(HideLerp));
		}
		if ((!GameManager.Multiplayer() || photonView.IsMine) && Active)
		{
			SemiFunc.UIHideWorldSpace();
			PlayerController.instance.MoveMult(MoveMultiplier, 0.1f);
			CameraTopFade.Instance.Set(FadeAmount, 0.1f);
			CameraBob.Instance.SetMultiplier(BobMultiplier, 0.1f);
			CameraZoom.Instance.OverrideZoomSet(50f, 0.05f, 2f, 2f, base.gameObject, 100);
			CameraNoise.Instance.Override(0.025f, 0.25f);
			Aim.instance.SetState(Aim.State.Hidden);
		}
		Vector3 vector = Vector3.one;
		if (GameManager.Multiplayer() && !photonView.IsMine)
		{
			base.transform.parent.position = FollowTransformClient.position;
			base.transform.parent.rotation = FollowTransformClient.rotation;
			vector = FollowTransformClient.localScale;
			mainSpringTransform.rotation = SemiFunc.SpringQuaternionGet(mainSpring, mainSpringTransformTarget.rotation);
		}
		base.transform.parent.localScale = Vector3.Lerp(base.transform.parent.localScale, vector * HideScale, Time.deltaTime * 20f);
		displaySpringTransform.rotation = SemiFunc.SpringQuaternionGet(displaySpring, displaySpringTransformTarget.rotation);
		if (Active)
		{
			DisplayJointAngleDiff = (DisplayJointAnglePreviousX - displaySpringTransform.localRotation.x) * 50f;
			DisplayJointAngleDiff = Mathf.Clamp(DisplayJointAngleDiff, -0.1f, 0.1f);
			DisplayJointAnglePreviousX = displaySpringTransform.localRotation.x;
			SoundLoop.LoopPitch = Mathf.Lerp(SoundLoop.LoopPitch, 1f - DisplayJointAngleDiff, Time.deltaTime * 10f);
		}
		SoundLoop.PlayLoop(Active, 5f, 5f);
	}

	[PunRPC]
	public void SetActiveRPC(bool active)
	{
		Active = active;
	}
}
