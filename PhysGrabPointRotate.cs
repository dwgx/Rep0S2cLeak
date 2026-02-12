using UnityEngine;

public class PhysGrabPointRotate : MonoBehaviour
{
	internal PhysGrabber physGrabber;

	private Quaternion smoothRotation;

	internal float rotationActiveTimer;

	private float rotationSpeed;

	private float offsetX;

	private AnimationCurve popIn;

	private AnimationCurve popOut;

	private MeshRenderer meshRenderer;

	public Material originalMaterial;

	public Material greenScreenMaterial;

	internal float animationEval;

	public Sound soundRotationStart;

	public Sound soundRotationEnd;

	public Sound soundRotationLoop;

	private void Start()
	{
		popIn = AssetManager.instance.animationCurveWooshIn;
		popOut = AssetManager.instance.animationCurveWooshAway;
		base.transform.localScale = Vector3.zero;
		meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.material = originalMaterial;
	}

	private void OnEnable()
	{
		if (!meshRenderer)
		{
			meshRenderer = GetComponent<MeshRenderer>();
		}
		if ((bool)meshRenderer)
		{
			if (!VideoGreenScreen.instance)
			{
				meshRenderer.material = originalMaterial;
			}
			else
			{
				meshRenderer.material = greenScreenMaterial;
			}
		}
	}

	private void Update()
	{
		if (!physGrabber)
		{
			return;
		}
		if ((bool)physGrabber)
		{
			Vector3 mouseTurningVelocity = physGrabber.mouseTurningVelocity;
			if (physGrabber.isRotating)
			{
				rotationActiveTimer = 0.1f;
			}
			if (rotationActiveTimer > 0f)
			{
				physGrabber.OverrideColorToPurple();
				base.transform.LookAt(physGrabber.playerAvatar.PlayerVisionTarget.VisionTransform.position);
				animationEval += Time.deltaTime * 2f;
				animationEval = Mathf.Clamp(animationEval, 0f, 1f);
				float num = popIn.Evaluate(animationEval);
				base.transform.localScale = Vector3.one * 0.5f * num;
				base.transform.Rotate(0f, 0f, (0f - Mathf.Atan2(mouseTurningVelocity.y, mouseTurningVelocity.x)) * 57.29578f);
				smoothRotation = Quaternion.Slerp(smoothRotation, base.transform.rotation, Time.deltaTime * 10f);
				base.transform.rotation = smoothRotation;
				rotationActiveTimer -= Time.deltaTime;
			}
			else
			{
				animationEval -= Time.deltaTime * 6f;
				animationEval = Mathf.Clamp(animationEval, 0f, 1f);
				float num2 = popOut.Evaluate(1f - animationEval);
				Vector3 vector = Vector3.one * 0.5f;
				base.transform.localScale = vector - vector * num2;
			}
		}
		if (base.transform.localScale.magnitude < 0.01f)
		{
			meshRenderer.enabled = false;
		}
		else
		{
			if (!meshRenderer.enabled)
			{
				soundRotationStart.Play(base.transform.position);
			}
			meshRenderer.enabled = true;
		}
		float num3 = physGrabber.mouseTurningVelocity.magnitude * 0.2f;
		bool playing = meshRenderer.enabled;
		float pitchMultiplier = Mathf.Min(Mathf.Max(num3 / 5f, 1f), 2f);
		soundRotationLoop.PlayLoop(playing, 0.5f, 1f, pitchMultiplier);
		offsetX -= num3 * 0.08f * Time.deltaTime;
		GetComponent<Renderer>().material.mainTextureOffset = new Vector2(offsetX, 0f);
		float num4 = Mathf.Sin(Time.time * 10f) * 0.2f;
		GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, 1f + num4);
		GetComponent<Renderer>().material.mainTextureOffset = new Vector2(offsetX, (0f - num4) * 0.5f);
	}
}
