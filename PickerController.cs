using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PickerController : MonoBehaviour
{
	public Transform ParentTransform;

	public AnimationCurve PickerStabIntro;

	public AnimationCurve PickerStabOutro;

	public float AnimationSpeedIntro = 1f;

	public float AnimationSpeedOutro = 1f;

	public GameObject AnimatedPicker;

	[Space]
	public GameObject StabPoint;

	public GameObject StabPointChild;

	[Space]
	private LayerMask Mask;

	private Camera MainCamera;

	public GameObject meshObject;

	public MeshRenderer meshRenderer;

	private bool isAnimating;

	private float animationProgress;

	private float ShowTimer = 0.3f;

	private bool stab;

	private GameObject stabObject;

	private List<GameObject> stabObjects = new List<GameObject>();

	private List<float> stabObjectsAngles = new List<float>();

	[Space]
	public Transform PickerPoint;

	public Transform PickerPointEnd;

	public Transform PickerPointAnimate;

	public Transform PickerPointEndAnimate;

	public float StabObjectSpacing = 10f;

	private bool isStabbing;

	private bool introAnimation = true;

	private Quaternion RotationStart;

	private Quaternion RotationEnd;

	private Vector3 PositionStart;

	private Vector3 PositionEnd;

	private Vector3 ScaleStart;

	private Vector3 ScaleEnd;

	[Space]
	public Sound IntroSound;

	public Sound OutroSound;

	public Sound StabSound;

	private bool OutroAudioPlay = true;

	private void Start()
	{
		IntroSound.Play(base.transform.position);
		AnimatedPicker.SetActive(value: false);
		StabPoint.SetActive(value: false);
		MainCamera = Camera.main;
		Mask = LayerMask.GetMask("Default");
		GameDirector.instance.CameraShake.Shake(2f, 0.25f);
	}

	private void Update()
	{
		if (OutroAudioPlay && !ToolController.instance.ToolHide.Active)
		{
			OutroSound.Play(base.transform.position);
			OutroAudioPlay = false;
			GameDirector.instance.CameraShake.Shake(2f, 0.25f);
		}
		if (isAnimating)
		{
			AnimatePicker();
		}
		AlignStabObjects();
		if (ShowTimer > 0f)
		{
			ShowTimer -= Time.deltaTime;
		}
		if (ToolController.instance.Interact)
		{
			isStabbing = true;
		}
		Interaction activeInteraction = ToolController.instance.ActiveInteraction;
		if ((bool)activeInteraction && !isAnimating && ShowTimer <= 0f && ToolController.instance.ToolHide.Active && isStabbing)
		{
			StabPoint.SetActive(value: true);
			PaperPick component = activeInteraction.GetComponent<PaperPick>();
			stabObject = component.PaperInteraction.GameObject();
			StabPoint.transform.position = component.PaperInteraction.PaperTransform.position;
			Vector3 forward = StabPoint.transform.position - base.transform.position;
			StabPoint.transform.rotation = Quaternion.LookRotation(forward);
			GameDirector.instance.CameraShake.Shake(3f, 0.2f);
			StartAnimation();
			isStabbing = false;
		}
		base.transform.position = ToolController.instance.ToolFollow.transform.position;
		base.transform.rotation = ToolController.instance.ToolFollow.transform.rotation;
		base.transform.localScale = ToolController.instance.ToolHide.transform.localScale;
	}

	private void AlignStabObjects()
	{
		for (int i = 0; i < stabObjects.Count; i++)
		{
			Vector3 position = PickerPoint.position;
			Vector3 position2 = PickerPointEnd.position;
			if (isAnimating)
			{
				position = PickerPointAnimate.position;
				position2 = PickerPointEndAnimate.position;
			}
			float num = StabObjectSpacing * (float)i;
			float num2 = Vector3.Distance(position, position2);
			float t = num / num2;
			stabObjects[i].transform.position = Vector3.Lerp(position, position2, t);
			stabObjects[i].transform.LookAt(base.transform.position);
			stabObjects[i].transform.Rotate(90f, 0f, 0f, Space.Self);
			stabObjects[i].transform.Rotate(0f, stabObjectsAngles[i], 0f, Space.Self);
		}
	}

	private void AnimatePicker()
	{
		if (animationProgress < 2f)
		{
			ToolController.instance.ForceActiveTimer = 0.1f;
			int num = 0;
			if (!introAnimation)
			{
				num = 1;
			}
			if (!introAnimation)
			{
				animationProgress += Time.deltaTime * AnimationSpeedOutro;
				float t = PickerStabOutro.Evaluate(animationProgress - (float)num);
				AnimatedPicker.transform.position = Vector3.LerpUnclamped(PositionStart, meshObject.transform.position, t);
				AnimatedPicker.transform.rotation = Quaternion.LerpUnclamped(RotationStart, meshObject.transform.rotation, t);
				AnimatedPicker.transform.localScale = Vector3.LerpUnclamped(ScaleStart, meshObject.transform.localScale, t);
			}
			else
			{
				animationProgress += Time.deltaTime * AnimationSpeedIntro;
				float t2 = PickerStabIntro.Evaluate(animationProgress - (float)num);
				AnimatedPicker.transform.position = Vector3.LerpUnclamped(PositionStart, StabPointChild.transform.position, t2);
				AnimatedPicker.transform.rotation = Quaternion.LerpUnclamped(RotationStart, StabPointChild.transform.rotation, t2);
				AnimatedPicker.transform.localScale = Vector3.LerpUnclamped(ScaleStart, StabPointChild.transform.localScale, t2);
			}
			if (animationProgress > 1f && !stab)
			{
				GameDirector.instance.CameraImpact.Shake(3f, 0f);
				PaperInteraction component = stabObject.GetComponent<PaperInteraction>();
				component.Picked = true;
				component.CleanEffect.Clean();
				component.CleanEffect.transform.parent = null;
				GameObject paperVisual = component.paperVisual;
				paperVisual.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
				paperVisual.layer = LayerMask.NameToLayer("TopLayer");
				StabSound.Play(paperVisual.transform.position);
				paperVisual.transform.parent = ParentTransform;
				stabObjects.Add(paperVisual);
				stabObjectsAngles.Add(Random.Range(0, 360));
				introAnimation = false;
				stab = true;
				AnimationSet(introAnimation);
				GameDirector.instance.CameraShake.Shake(2f, 0.25f);
			}
		}
		else
		{
			EndAnimation();
		}
	}

	private void AnimationSet(bool intro)
	{
		if (intro)
		{
			RotationStart = meshObject.transform.rotation;
			PositionStart = meshObject.transform.position;
			ScaleStart = meshObject.transform.localScale;
		}
		else
		{
			RotationStart = StabPointChild.transform.rotation;
			PositionStart = StabPointChild.transform.position;
			ScaleStart = StabPointChild.transform.localScale;
		}
		AnimatedPicker.transform.rotation = RotationStart;
	}

	public void StartAnimation()
	{
		isAnimating = true;
		animationProgress = 0f;
		StabPoint.SetActive(value: true);
		AnimatedPicker.SetActive(value: true);
		AnimatedPicker.transform.position = base.transform.position;
		AnimatedPicker.transform.rotation = base.transform.rotation;
		AnimatedPicker.transform.localScale = base.transform.localScale;
		meshRenderer.enabled = false;
		AnimationSet(introAnimation);
	}

	private void EndAnimation()
	{
		stab = false;
		introAnimation = true;
		isAnimating = false;
		StabPoint.SetActive(value: false);
		AnimatedPicker.SetActive(value: false);
		meshRenderer.enabled = true;
	}
}
