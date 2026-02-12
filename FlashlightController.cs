using UnityEngine;

public class FlashlightController : MonoBehaviour
{
	internal enum State
	{
		Hidden,
		Intro,
		LightOn,
		Idle,
		LightOff,
		Outro
	}

	public static FlashlightController Instance;

	public Transform FollowTransformLocal;

	public Transform FollowTransformClient;

	internal bool hideFlashlight;

	[Space]
	public PlayerAvatar PlayerAvatar;

	public MeshRenderer mesh;

	public Light spotlight;

	public Behaviour halo;

	public Transform hideTransform;

	public Transform clickTransform;

	public ToolBackAway toolBackAway;

	internal bool active;

	internal State currentState;

	private float stateTimer;

	[HideInInspector]
	public bool LightActive;

	[Header("Hidden")]
	public float hiddenRot;

	public float hiddenY;

	private float hiddenScale;

	[Header("Intro")]
	public AnimationCurve introCurveScale;

	public AnimationCurve introCurveRot;

	public AnimationCurve introCurveY;

	public float introRotSpeed;

	private float introRotLerp;

	public float introYSpeed;

	private float introYLerp;

	[Header("Light")]
	public AnimationCurve lightOnCurve;

	public float lightOnSpeed;

	private float lightOnLerp;

	public AnimationCurve clickCurve;

	public float clickSpeed;

	public float clickStrength;

	private float clickLerp;

	private bool click;

	private float baseIntensity;

	public Sound lightOnAudio;

	public Sound lightOffAudio;

	[Header("Outro")]
	public AnimationCurve outroCurveScale;

	public AnimationCurve outroCurveRot;

	public AnimationCurve outroCurveY;

	public float outroRotSpeed;

	private float outroRotLerp;

	public float outroYSpeed;

	private float outroYLerp;

	[Header("Flicker")]
	public AnimationCurve flickerCurve;

	private float flickerIntensity;

	private float flickerMultiplier = 0.5f;

	private float flickerMultiplierTarget = 0.5f;

	private float flickerLerp;

	private float flickerTimer;

	private void Start()
	{
		if (PlayerAvatar.isLocal)
		{
			Instance = this;
			base.transform.parent = FollowTransformLocal;
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
		}
		else
		{
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = LayerMask.NameToLayer("Triggers");
			}
			toolBackAway.Active = false;
			lightOnAudio.SpatialBlend = 1f;
			lightOnAudio.Volume *= 0.5f;
			lightOffAudio.SpatialBlend = 1f;
			lightOffAudio.Volume *= 0.5f;
		}
		mesh.enabled = false;
		spotlight.enabled = false;
		halo.enabled = false;
		LightActive = false;
	}

	private void Update()
	{
		if (GameDirector.instance.currentState >= GameDirector.gameState.Main && !SemiFunc.RunIsLobby() && !SemiFunc.RunIsShop() && !SemiFunc.MenuLevel() && !hideFlashlight)
		{
			if (PlayerAvatar.isDisabled || PlayerAvatar.isCrouching || PlayerAvatar.isTumbling)
			{
				active = false;
				if ((PlayerAvatar.isTumbling || PlayerAvatar.isSliding) && currentState < State.Idle && currentState != State.Hidden)
				{
					currentState = State.Idle;
				}
			}
			else
			{
				active = true;
			}
		}
		else
		{
			active = false;
		}
		if (PlayerAvatar.isDisabled && currentState != State.Hidden)
		{
			currentState = State.Hidden;
			mesh.enabled = false;
			spotlight.enabled = false;
			halo.enabled = false;
			LightActive = false;
			hiddenScale = 0f;
		}
		if (currentState == State.Hidden)
		{
			Hidden();
		}
		else if (currentState == State.Intro)
		{
			Intro();
		}
		else if (currentState == State.LightOn)
		{
			LightOn();
		}
		else if (currentState == State.Idle)
		{
			Idle();
		}
		else if (currentState == State.LightOff)
		{
			LightOff();
		}
		else if (currentState == State.Outro)
		{
			Outro();
		}
		if (!PlayerAvatar.isLocal)
		{
			base.transform.position = FollowTransformClient.position;
			base.transform.rotation = FollowTransformClient.rotation;
			base.transform.localScale = FollowTransformClient.localScale * hiddenScale;
		}
		else
		{
			base.transform.localScale = Vector3.one * hiddenScale;
		}
		float intensity = baseIntensity;
		if (RoundDirector.instance.allExtractionPointsCompleted)
		{
			flickerMultiplier = Mathf.Lerp(flickerMultiplier, flickerMultiplierTarget, 10f * Time.deltaTime);
			intensity = (baseIntensity + flickerIntensity) * flickerMultiplier;
			if (flickerLerp < 1f)
			{
				flickerLerp += 1.5f * Time.deltaTime;
				flickerIntensity = flickerCurve.Evaluate(flickerLerp) * 0.15f;
			}
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				if (flickerTimer <= 0f)
				{
					flickerTimer = Random.Range(2f, 10f);
					PlayerAvatar.FlashlightFlicker(Random.Range(0.25f, 0.35f));
				}
				else
				{
					flickerTimer -= Time.deltaTime;
				}
			}
		}
		spotlight.intensity = intensity;
		ClickAnim();
	}

	private void Hidden()
	{
		if (active)
		{
			currentState = State.Intro;
			stateTimer = 1f;
		}
	}

	private void Intro()
	{
		mesh.enabled = true;
		float num = Mathf.LerpUnclamped(hiddenRot, 0f, introCurveRot.Evaluate(introRotLerp));
		hideTransform.localRotation = Quaternion.Euler(0f, 0f - num, 0f - num);
		float num2 = introRotSpeed;
		if (!PlayerAvatar.isLocal)
		{
			num2 *= 2f;
		}
		introRotLerp += num2 * Time.deltaTime;
		introRotLerp = Mathf.Clamp01(introRotLerp);
		hiddenScale = Mathf.LerpUnclamped(0f, 1f, introCurveScale.Evaluate(introRotLerp));
		if (PlayerAvatar.isLocal)
		{
			float y = Mathf.LerpUnclamped(hiddenY, 0f, introCurveY.Evaluate(introYLerp));
			hideTransform.localPosition = new Vector3(0f, y, 0f);
			introYLerp += introYSpeed * Time.deltaTime;
			introYLerp = Mathf.Clamp01(introYLerp);
		}
		else
		{
			hideTransform.localPosition = Vector3.zero;
		}
		if (stateTimer <= 0f)
		{
			currentState = State.LightOn;
			stateTimer = 0.5f;
			introRotLerp = 0f;
			introYLerp = 0f;
			click = true;
			lightOnAudio.Play(base.transform.position);
		}
		else
		{
			stateTimer -= Time.deltaTime;
		}
	}

	private void LightOn()
	{
		spotlight.enabled = true;
		halo.enabled = true;
		LightActive = true;
		baseIntensity = Mathf.LerpUnclamped(0f, 1f, lightOnCurve.Evaluate(lightOnLerp));
		lightOnLerp += lightOnSpeed * Time.deltaTime;
		lightOnLerp = Mathf.Clamp01(lightOnLerp);
		if (stateTimer <= 0f)
		{
			currentState = State.Idle;
			lightOnLerp = 0f;
		}
		else
		{
			stateTimer -= Time.deltaTime;
		}
	}

	private void Idle()
	{
		if (!active)
		{
			currentState = State.LightOff;
			stateTimer = 0.25f;
			if (PlayerAvatar.isTumbling || PlayerAvatar.isSliding)
			{
				stateTimer = 0f;
			}
			click = true;
			lightOffAudio.Play(base.transform.position);
		}
	}

	private void LightOff()
	{
		spotlight.enabled = false;
		halo.enabled = false;
		LightActive = false;
		if (stateTimer <= 0f)
		{
			currentState = State.Outro;
			stateTimer = 1f;
			if (PlayerAvatar.isTumbling || PlayerAvatar.isSliding)
			{
				stateTimer = 0.25f;
			}
		}
		else
		{
			stateTimer -= Time.deltaTime;
		}
	}

	private void Outro()
	{
		float num = Mathf.LerpUnclamped(0f, hiddenRot, outroCurveRot.Evaluate(outroRotLerp));
		hideTransform.localRotation = Quaternion.Euler(0f, num, 0f - num);
		float num2 = outroRotSpeed;
		if (PlayerAvatar.isTumbling || PlayerAvatar.isSliding)
		{
			num2 *= 5f;
		}
		else if (!PlayerAvatar.isLocal)
		{
			num2 *= 2f;
		}
		outroRotLerp += num2 * Time.deltaTime;
		outroRotLerp = Mathf.Clamp01(outroRotLerp);
		hiddenScale = Mathf.LerpUnclamped(1f, 0f, outroCurveScale.Evaluate(outroRotLerp));
		if (PlayerAvatar.isLocal)
		{
			float y = Mathf.LerpUnclamped(0f, hiddenY, outroCurveY.Evaluate(outroYLerp));
			hideTransform.localPosition = new Vector3(0f, y, 0f);
			float num3 = outroYSpeed;
			if (PlayerAvatar.isTumbling || PlayerAvatar.isSliding)
			{
				num3 *= 5f;
			}
			outroYLerp += num3 * Time.deltaTime;
			outroYLerp = Mathf.Clamp01(outroYLerp);
		}
		else
		{
			hideTransform.localPosition = Vector3.zero;
		}
		if (stateTimer <= 0f)
		{
			currentState = State.Hidden;
			mesh.enabled = false;
			outroRotLerp = 0f;
			outroYLerp = 0f;
		}
		else
		{
			stateTimer -= Time.deltaTime;
		}
	}

	private void ClickAnim()
	{
		if (click)
		{
			float num = Mathf.LerpUnclamped(0f, clickStrength, clickCurve.Evaluate(clickLerp));
			clickTransform.localRotation = Quaternion.Euler(0f, 0f - num, 0f);
			clickLerp += clickSpeed * Time.deltaTime;
			clickLerp = Mathf.Clamp01(clickLerp);
			if (clickLerp == 1f)
			{
				clickLerp = 0f;
				click = false;
			}
		}
	}

	public void FlickerSet(float _multiplier)
	{
		flickerLerp = 0f;
		flickerMultiplierTarget = _multiplier;
	}
}
