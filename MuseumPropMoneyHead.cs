using Photon.Pun;
using UnityEngine;

public class MuseumPropMoneyHead : MonoBehaviour
{
	public enum State
	{
		Open,
		Closing,
		Closed,
		Opening,
		OpenCoolingDown,
		OpenEvilEyes,
		OpenDraggingInPlayer
	}

	private bool stateStart;

	private float stateTimer;

	private float stateTimerMax;

	public AnimationCurve animationCurve;

	public AnimationCurve animationCurveLights;

	public AnimationCurve animationCurveEyesCoolDown;

	public Color colorCooldown;

	[Space(20f)]
	public State state;

	[Space(20f)]
	public Transform headTransform;

	public Transform fireEyes;

	[Space(10f)]
	public MeshRenderer eye1;

	public MeshRenderer eye2;

	[Space(10f)]
	public Light eye1Light;

	public Light eye2Light;

	[Space(10f)]
	public ParticleSystem particlesOpen;

	public ParticleSystem eye1Particles;

	public ParticleSystem eye2Particles;

	[Space(10f)]
	public Transform headTopTransform;

	public Transform headBottomTransform;

	public Transform headTopMeshTransform;

	[Space(10f)]
	public Transform forcePoint;

	public Transform forcePointFirst;

	public Transform boxColliderCheckTransform;

	[Space(10f)]
	public GameObject evilEyes;

	public Light spotLight;

	public MeshRenderer grunkaMeshRenderer;

	public GameObject lowPassWalls;

	public SemiLine semiLine;

	public GameObject hurtCollider;

	[Space(10f)]
	public Sound soundRedEyes;

	public Sound soundActivateDragIn;

	public Sound soundClose;

	public Sound soundScorchLoop;

	public Sound soundOpen;

	public Sound soundCoolingDown;

	private Material grunkaMaterial;

	private Material eye1Material;

	private Material eye2Material;

	private float headZRotation;

	private float lightOriginalIntensity;

	private Color originalColor;

	private Color originalLightColor;

	private Transform eye1Transform;

	private Transform eye2Transform;

	private PhotonView photonView;

	private bool dinkDone;

	private PlayerAvatar playerToDragIn;

	private PhysGrabObjectGrabArea grabArea;

	private bool playerInCheckBox;

	private void Start()
	{
		stateStart = true;
		state = State.Open;
		headZRotation = headTransform.localRotation.eulerAngles.z;
		eye1Material = eye1.material;
		eye2Material = eye2.material;
		lightOriginalIntensity = eye1Light.intensity;
		originalColor = eye1Material.GetColor("_Color");
		originalLightColor = eye1Light.color;
		eye1Transform = eye1.transform;
		eye2Transform = eye2.transform;
		photonView = GetComponent<PhotonView>();
		grabArea = GetComponent<PhysGrabObjectGrabArea>();
		evilEyes.SetActive(value: false);
		grunkaMaterial = grunkaMeshRenderer.material;
		hurtCollider.SetActive(value: false);
	}

	private void StateOpen()
	{
		if (stateStart)
		{
			stateStart = false;
			stateTimer = 0f;
			eye1Light.color = originalLightColor;
			eye2Light.color = originalLightColor;
			fireEyes.gameObject.SetActive(value: false);
			evilEyes.SetActive(value: false);
			grunkaMaterial.SetColor("_EmissionColor", Color.black);
			lowPassWalls.SetActive(value: false);
			hurtCollider.SetActive(value: false);
		}
	}

	private void StateClosing()
	{
		if (stateStart)
		{
			stateStart = false;
			stateTimer = 0f;
			stateTimerMax = 0.5f;
			fireEyes.gameObject.SetActive(value: true);
			eye1Light.intensity = 0f;
			eye2Light.intensity = 0f;
			eye1Material.SetColor("_Color", Color.black);
			eye2Material.SetColor("_Color", Color.black);
			GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.1f);
			GameDirector.instance.CameraImpact.ShakeDistance(4f, 3f, 8f, base.transform.position, 0.1f);
			dinkDone = false;
			if (SemiFunc.Photosensitivity())
			{
				fireEyes.gameObject.SetActive(value: false);
			}
			spotLight.enabled = false;
			soundClose.Play(base.transform.position);
		}
		ShakeTopAndBottom(2.5f * Mathf.Lerp(1f, 0f, stateTimer / stateTimerMax));
		if (stateTimer > stateTimerMax * 0.55f && !dinkDone)
		{
			GameDirector.instance.CameraShake.ShakeDistance(6f, 3f, 8f, base.transform.position, 0.1f);
			GameDirector.instance.CameraImpact.ShakeDistance(8f, 3f, 8f, base.transform.position, 0.1f);
			dinkDone = true;
			lowPassWalls.SetActive(value: true);
		}
		stateTimer += Time.deltaTime;
		ScrollEyes();
		float time = stateTimer / stateTimerMax;
		float z = Mathf.LerpUnclamped(headZRotation, 0f, animationCurve.Evaluate(time));
		headTransform.localRotation = Quaternion.Euler(0f, 0f, z);
		if (!SemiFunc.Photosensitivity())
		{
			float intensity = Mathf.LerpUnclamped(0f, lightOriginalIntensity, animationCurveLights.Evaluate(time));
			eye1Light.intensity = intensity;
			eye2Light.intensity = intensity;
		}
		Color value = Color.Lerp(Color.black, originalColor, animationCurveLights.Evaluate(time));
		eye1Material.SetColor("_Color", value);
		eye2Material.SetColor("_Color", value);
		if (stateTimer >= stateTimerMax)
		{
			StateSet(State.Closed);
		}
	}

	private void StateClosed()
	{
		if (stateStart)
		{
			stateStart = false;
			stateTimer = 0f;
			stateTimerMax = 3f;
			evilEyes.SetActive(value: false);
			grunkaMaterial.SetColor("_EmissionColor", Color.black);
			lowPassWalls.SetActive(value: true);
			spotLight.enabled = false;
			hurtCollider.SetActive(value: true);
		}
		ScrollEyes();
		ShakeTopAndBottom(5f);
		stateTimer += Time.deltaTime;
		GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.1f);
		if (stateTimer >= stateTimerMax)
		{
			StateSet(State.Opening);
		}
	}

	private void StateOpening()
	{
		if (stateStart)
		{
			headTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			stateStart = false;
			stateTimer = 0f;
			stateTimerMax = 0.25f;
			eye1Light.intensity = lightOriginalIntensity;
			eye2Light.intensity = lightOriginalIntensity;
			eye1Material.SetColor("_Color", originalColor);
			eye2Material.SetColor("_Color", originalColor);
			GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.1f);
			GameDirector.instance.CameraImpact.ShakeDistance(4f, 3f, 8f, base.transform.position, 0.1f);
			particlesOpen.Play();
			dinkDone = false;
			if (SemiFunc.Photosensitivity())
			{
				fireEyes.gameObject.SetActive(value: false);
				eye1Light.intensity = 0f;
				eye2Light.intensity = 0f;
			}
			evilEyes.SetActive(value: false);
			grunkaMaterial.SetColor("_EmissionColor", Color.black);
			lowPassWalls.SetActive(value: false);
			hurtCollider.SetActive(value: false);
			soundOpen.Play(base.transform.position);
		}
		stateTimer += Time.deltaTime;
		ShakeTopAndBottom(10f * Mathf.Lerp(1f, 0f, stateTimer / stateTimerMax));
		if (stateTimer > stateTimerMax * 0.55f && !dinkDone)
		{
			GameDirector.instance.CameraShake.ShakeDistance(6f, 3f, 8f, base.transform.position, 0.1f);
			GameDirector.instance.CameraImpact.ShakeDistance(8f, 3f, 8f, base.transform.position, 0.1f);
			spotLight.enabled = true;
			spotLight.color = Color.yellow;
			dinkDone = true;
		}
		ScrollEyes();
		float time = stateTimer / stateTimerMax;
		float z = Mathf.LerpUnclamped(0f, headZRotation, animationCurve.Evaluate(time));
		headTransform.localRotation = Quaternion.Euler(0f, 0f, z);
		if (stateTimer >= stateTimerMax)
		{
			StateSet(State.OpenCoolingDown);
		}
	}

	private void StateOpenCoolingDown()
	{
		if (stateStart)
		{
			stateTimer = 0f;
			stateTimerMax = 3f;
			eye1Particles.Stop();
			eye2Particles.Stop();
			stateStart = false;
			ShakeRotationReset();
			if (SemiFunc.Photosensitivity())
			{
				fireEyes.gameObject.SetActive(value: false);
			}
			evilEyes.SetActive(value: false);
			soundCoolingDown.Play(base.transform.position);
		}
		stateTimer += Time.deltaTime;
		ScrollEyes();
		float num = stateTimer / stateTimerMax;
		Color color;
		if (num < 0.5f)
		{
			float time = num / 0.5f;
			color = Color.Lerp(originalColor, colorCooldown * 3f, animationCurveEyesCoolDown.Evaluate(time));
		}
		else
		{
			float time2 = (num - 0.5f) / 0.5f;
			color = Color.Lerp(colorCooldown * 3f, Color.black, animationCurveEyesCoolDown.Evaluate(time2));
		}
		eye1Material.SetColor("_Color", color);
		eye2Material.SetColor("_Color", color);
		eye1Light.color = color;
		eye2Light.color = color;
		eye1Light.intensity = Mathf.Lerp(lightOriginalIntensity, 0f, animationCurveEyesCoolDown.Evaluate(num));
		eye2Light.intensity = Mathf.Lerp(lightOriginalIntensity, 0f, animationCurveEyesCoolDown.Evaluate(num));
		if (stateTimer >= stateTimerMax)
		{
			StateSet(State.Open);
		}
	}

	private void StateOpenEvilEyes()
	{
		if (stateStart)
		{
			stateStart = false;
			stateTimer = 0f;
			stateTimerMax = 2f;
			evilEyes.SetActive(value: true);
			GameDirector.instance.CameraShake.ShakeDistance(6f, 3f, 8f, base.transform.position, 0.1f);
			GameDirector.instance.CameraImpact.ShakeDistance(8f, 3f, 8f, base.transform.position, 0.1f);
			grunkaMaterial.SetColor("_EmissionColor", Color.red);
			spotLight.color = Color.red;
			soundRedEyes.Play(base.transform.position);
		}
		stateTimer += Time.deltaTime;
		if (stateTimer >= stateTimerMax)
		{
			StateSet(State.OpenDraggingInPlayer);
		}
	}

	private void StateOpenDraggingInPlayer()
	{
		if (stateStart)
		{
			stateStart = false;
			stateTimer = 0f;
			stateTimerMax = 2f;
			evilEyes.SetActive(value: true);
			GameDirector.instance.CameraShake.ShakeDistance(6f, 3f, 8f, base.transform.position, 0.1f);
			GameDirector.instance.CameraImpact.ShakeDistance(8f, 3f, 8f, base.transform.position, 0.1f);
			playerInCheckBox = false;
			soundActivateDragIn.Play(base.transform.position);
		}
		if ((bool)playerToDragIn)
		{
			semiLine.LineActive(playerToDragIn.tumble.physGrabObject.rb.transform);
		}
		BoxCheck();
		stateTimer += Time.deltaTime;
		if (stateTimer >= stateTimerMax)
		{
			StateSet(State.Closing);
		}
	}

	private void StateMachine()
	{
		switch (state)
		{
		case State.Open:
			StateOpen();
			break;
		case State.Closing:
			StateClosing();
			break;
		case State.Closed:
			StateClosed();
			break;
		case State.Opening:
			StateOpening();
			break;
		case State.OpenCoolingDown:
			StateOpenCoolingDown();
			break;
		case State.OpenEvilEyes:
			StateOpenEvilEyes();
			break;
		case State.OpenDraggingInPlayer:
			StateOpenDraggingInPlayer();
			break;
		}
		bool playing = state == State.Closed;
		soundScorchLoop.PlayLoop(playing, 2f, 2f);
	}

	private void BoxCheck()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || !SemiFunc.FPSImpulse5())
		{
			return;
		}
		Collider[] array = Physics.OverlapBox(boxColliderCheckTransform.position, boxColliderCheckTransform.localScale / 2f, boxColliderCheckTransform.rotation);
		playerInCheckBox = false;
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if ((bool)array2[i].GetComponentInParent<PhysGrabObject>())
			{
				playerInCheckBox = true;
				break;
			}
		}
	}

	private void DebugWalter()
	{
		if (Input.GetKeyDown(KeyCode.M))
		{
			StateSet((state == State.Open) ? State.Closing : ((state == State.Closing) ? State.Closed : ((state == State.Closed) ? State.Opening : State.Open)));
		}
	}

	private void Update()
	{
		StateMachine();
	}

	private void FixedUpdate()
	{
		DragInPlayer();
	}

	private void ScrollEyes()
	{
		float num = 1f;
		float num2 = Time.time * num;
		eye1Material.mainTextureOffset = new Vector2(0f - num2, num2);
		eye2Material.mainTextureOffset = new Vector2(num2, 0f - num2);
		float num3 = 250f;
		eye1Transform.Rotate(Vector3.forward * num3 * Time.deltaTime);
		eye2Transform.Rotate(Vector3.forward * num3 * Time.deltaTime);
		float num4 = Mathf.Sin(Time.time * num3) * 10f;
		eye1Transform.localRotation = Quaternion.Euler(0f, 0f - num4, num4);
		eye2Transform.localRotation = Quaternion.Euler(0f, num4, 0f - num4);
		float num5 = 50f;
		float num6 = Mathf.Sin(Time.time * num5) * 0.5f + 0.5f;
		eye1Light.intensity += num6;
		eye2Light.intensity += num6;
		if (eye1Light.intensity > lightOriginalIntensity + num6)
		{
			eye1Light.intensity = lightOriginalIntensity + num6;
			eye2Light.intensity = lightOriginalIntensity + num6;
		}
	}

	private void StateSet(State _newState)
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("StateSetRPC", RpcTarget.All, (int)_newState);
			}
		}
		else
		{
			StateSetRPC((int)_newState);
		}
	}

	[PunRPC]
	private void StateSetRPC(int _newState, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			stateStart = true;
			state = (State)_newState;
			stateTimer = 0f;
		}
	}

	private void ShakeRotationReset()
	{
		headTopTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		headBottomTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
	}

	private void DragInPlayer()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || state != State.OpenDraggingInPlayer)
		{
			return;
		}
		Transform transform = forcePointFirst;
		if (playerInCheckBox)
		{
			transform = forcePoint;
		}
		if ((bool)playerToDragIn)
		{
			if (!playerToDragIn.isTumbling)
			{
				playerToDragIn.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
				return;
			}
			Rigidbody rb = playerToDragIn.tumble.rb;
			playerToDragIn.tumble.TumbleOverrideTime(2f);
			Vector3 targetDirection = (transform.position - playerToDragIn.transform.position).normalized * 1f;
			Vector3 position = playerToDragIn.tumble.rb.position;
			Vector3 position2 = transform.position;
			Vector3 vector = SemiFunc.PhysFollowDirection(rb.transform, targetDirection, rb, 10f) * 2f;
			rb.AddTorque(vector / rb.mass, ForceMode.Force);
			Vector3 vector2 = SemiFunc.PhysFollowPosition(position, position2, rb.velocity, 5f);
			rb.AddForce(vector2 * 1f, ForceMode.Impulse);
		}
	}

	public void DragInPlayerStart()
	{
		playerToDragIn = grabArea.GetLatestGrabber();
		if (SemiFunc.IsMasterClientOrSingleplayer() && state == State.Open && (bool)playerToDragIn)
		{
			StateSet(State.OpenEvilEyes);
		}
	}

	private void ShakeTopAndBottom(float _shakeAmount)
	{
		float num = 100f;
		float num2 = Mathf.Sin(Time.time * num) * _shakeAmount;
		headTopTransform.localRotation = Quaternion.Euler(num2, 0f, 0f);
		headBottomTransform.localRotation = Quaternion.Euler(0f - num2, 0f, 0f);
		float num3 = 60f;
		float num4 = Mathf.Sin(Time.time * num3) * _shakeAmount;
		headTopTransform.localRotation *= Quaternion.Euler(0f, num4, 0f);
		headBottomTransform.localRotation *= Quaternion.Euler(0f, 0f - num4, 0f);
		float num5 = 80f;
		float num6 = Mathf.Sin(Time.time * num5) * _shakeAmount;
		headTopTransform.localRotation *= Quaternion.Euler(0f, 0f, num6);
		headBottomTransform.localRotation *= Quaternion.Euler(0f, 0f, 0f - num6);
	}
}
