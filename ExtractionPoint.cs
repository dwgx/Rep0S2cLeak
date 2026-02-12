using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class ExtractionPoint : MonoBehaviour
{
	public enum State
	{
		None,
		Idle,
		Active,
		Success,
		Warning,
		Cancel,
		Extracting,
		Complete,
		Surplus,
		TaxReturn
	}

	public GameObject extractionArea;

	public Sound soundButton;

	public Sound soundActivate1;

	public Sound soundActivate2;

	public Sound soundActivate3;

	public Sound soundAlarm;

	public Sound soundAlarmGlobal;

	public Sound soundAlarmFinal;

	public Sound soundCancel;

	public Sound soundEmojiGlitch;

	public Sound soundGreenLights;

	public Sound soundLightsOn;

	public Sound soundHaulIncrease;

	public Sound soundHaulDecrease;

	public Sound soundWarningLightsLoop;

	public Sound soundSuccess;

	public Sound soundSuckEnd;

	public Sound soundSuckLoop;

	public Sound soundTubeBuildup;

	public Sound soundTubeSlam;

	public Sound soundTubeSlamGlobal;

	public Sound soundTubeRaise;

	public Sound soundTubeRaiseGlobal;

	public Sound soundTubeRetract;

	public Sound soundTubeHitCeiling;

	public Sound soundTubeHitCeilingGlobal;

	public Sound jingleLocal;

	public Sound jingleGlobal;

	public Sound surplusStateStart;

	public Sound surplusStateIncreaseLoop;

	public Sound surplusStateDoneLevel1;

	public Sound surplusStateDoneLevel2;

	public Sound surplusStateDoneLevel3;

	public Sound surplusStateDoneLevel4;

	public Sound surplusDeductionStart;

	public Sound surplusDeductionLoop;

	public Sound surplusDeductionEnd;

	public Sound completeJingleLocal;

	public Sound completeJingleGlobal;

	public Sound soundPing;

	public Transform soundPingTransform;

	public Sound surplusLightOutroSound;

	public Transform safetySpawn;

	public Transform haulBar;

	private float haulBarTargetScale;

	private bool stateStart = true;

	public TextMeshPro emojiScreen;

	public TextMeshPro haulGoalScreen;

	public TextMeshPro tubeScreenText;

	public Light tubeScreenLight;

	public Light spotlight1;

	public Light spotlight2;

	public Light emojiLight;

	private float tubeScreenChangeTimer;

	private string tubeScreenTextString = "";

	private Color tubeScreenTextColor = Color.white;

	public GameObject grossUp;

	[Space]
	public Light buttonLight;

	public MeshRenderer button;

	public Material buttonOff;

	public StaticGrabObject buttonGrabObject;

	public Material buttonDenyMaterial;

	private bool buttonActive;

	private float buttonDenyCooldown;

	public Transform buttonDenyTransform;

	public AnimationCurve buttonDenyCurve;

	private bool buttonDenyActive;

	private float buttonDenyLerp;

	[Space]
	public Material spotlightOff;

	public Material spotlightOn;

	public Transform extractionTube;

	public Transform spotlightHead1;

	public Transform spotlightHead2;

	private Color spotLightColor;

	private float stateTimer;

	private bool stateEnd;

	public AnimationCurve tubeSlamDown;

	public AnimationCurve buttonPressAnimationCurve;

	private float tubeSlamDownEval;

	private Vector3 tubeStartPosition;

	private bool thirtyFPSUpdate;

	private float thirtyFPSUpdateTimer;

	public Transform platform;

	public Transform ramp;

	private Vector3 rampStartPosition;

	[Space]
	public GameObject hurtColliders;

	public GameObject hurtColliderMain;

	private float hurtColliderMainTimer;

	[Space]
	public GameObject tubeHitParticles;

	private bool tubeHit;

	public ParticleSystem suckParticles;

	public ParticleSystem upParticles;

	public ParticleSystem ceilingParticles;

	private PhotonView photonView;

	public GameObject roomVolume;

	public GameObject emojiScreenGlitch;

	private int amountOfValuables;

	private float suckUpVariableTimer;

	private float suckUpTimeLeft;

	private float haulUpdateEffectTimer;

	private int haulPrevious;

	private int haulCurrent;

	private bool deductedFromHaul;

	private Color originalHaulColor;

	private bool resetHaulText;

	private bool settingState;

	private float spotlight1Delay;

	private float spotlight2Delay;

	private float emojiDelay;

	private float successDelay;

	[Space]
	public Transform surplusSpawnTransform;

	public Light surplusLight;

	public AnimationCurve surplusLightOutro;

	private bool surplusLightActive;

	private float surplusLightIntensity;

	private float surplusLightRange;

	private float surplusLightTimer = 5f;

	private float surplusLightLerp;

	private int haulSurplus;

	private int haulSurplusAnimated;

	private bool haulSurplusAnimatedDone;

	private int surplusLevel;

	private bool surplusIntroText;

	[HideInInspector]
	public int haulGoal;

	private bool cancelExtraction;

	private Vector3 tubeCancelPosition;

	private bool cancelTube;

	private Quaternion spotlight1StartRotation;

	private Quaternion spotlight2StartRotation;

	private Quaternion spotlight1CancelRotation;

	private Quaternion spotlight2CancelRotation;

	private bool cancelSpotlights;

	private float cancelSpotlightEval;

	private float spotlightIntensity;

	private float spotLightRange;

	private float emojiLightIntensity;

	private Color originalEmojiLightColor;

	private float emojiScreenGlitchTimer;

	private string prevEmoji;

	private string currentEmoji;

	private float buttonDelay;

	private float buttonPressEval;

	private bool buttonPressed;

	private Vector3 buttonOriginalPosition;

	private bool tubeHitCeiling;

	private bool haulGoalFetched;

	[HideInInspector]
	public bool isLocked;

	private float suckInRampEval;

	private Material buttonOriginalMaterial;

	private bool isShop;

	private bool taxReturn;

	private bool inStartRoom;

	[Space]
	public Transform shopStation;

	public Transform shopButton;

	private float shopButtonAnimationEval;

	private bool shopButtonAnimation;

	private Vector3 shopButtonOriginalPosition;

	private float initialStateTime;

	private float textBlinkTime;

	private Color textBlinkColor = Color.white;

	private Color textBlinkColorOriginal = Color.white;

	private int extractionHaul;

	private int runCurrencyBefore;

	private State stateSetTo;

	private float soundPingTimer;

	[Space]
	public DirtFinderMapFloor[] mapActive;

	public DirtFinderMapFloor[] mapUsed;

	public DirtFinderMapFloor[] mapInactive;

	internal State currentState = State.Idle;

	private bool isThief;

	private bool isCompletedRightAway;

	private bool extractionSurplusCompleted;

	private void Start()
	{
		shopButtonOriginalPosition = shopButton.localPosition;
		buttonOriginalMaterial = button.material;
		spotlight1.enabled = false;
		spotlight2.enabled = false;
		emojiLight.enabled = false;
		emojiScreen.enabled = false;
		haulGoalScreen.enabled = false;
		spotLightColor = spotlight1.color;
		tubeStartPosition = extractionTube.localPosition;
		rampStartPosition = ramp.localPosition;
		photonView = GetComponent<PhotonView>();
		originalHaulColor = haulGoalScreen.color;
		StateSet(State.Idle);
		extractionTube.localPosition = new Vector3(tubeStartPosition.x, 0f, tubeStartPosition.z);
		spotlight1StartRotation = spotlightHead1.rotation;
		spotlight2StartRotation = spotlightHead2.rotation;
		spotlightIntensity = spotlight1.intensity;
		spotLightRange = spotlight1.range;
		emojiLightIntensity = emojiLight.intensity;
		originalEmojiLightColor = emojiLight.color;
		prevEmoji = "Jannek farts on the moon!";
		buttonOriginalPosition = button.transform.localPosition;
		RoundDirector.instance.extractionPoints++;
		RoundDirector.instance.extractionPointList.Add(base.gameObject);
		surplusLightIntensity = surplusLight.intensity;
		surplusLightRange = surplusLight.range;
		surplusLight.intensity = 0f;
		surplusLight.range = 0f;
		if ((bool)GetComponentInParent<StartRoom>())
		{
			inStartRoom = true;
		}
		platform.gameObject.SetActive(value: false);
		StartCoroutine(MapHideOnStart());
		isShop = SemiFunc.RunIsShop();
		if (!isShop)
		{
			Object.Destroy(shopStation.gameObject);
			return;
		}
		ShopManager.instance.isThief = false;
		ShopManager.instance.extractionPoint = base.transform;
		RoundDirector.instance.extractionPointSurplus = 0;
	}

	private IEnumerator MapHideOnStart()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(1f);
		DirtFinderMapFloor[] array = mapActive;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].MapObject.Hide();
		}
		array = mapUsed;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].MapObject.Hide();
		}
	}

	public void ActivateTheFirstExtractionPointAutomaticallyWhenAPlayerLeaveTruck()
	{
		OnClick();
	}

	public void OnClick()
	{
		if (isLocked || !StateIs(State.Idle))
		{
			return;
		}
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsNotMasterClient())
			{
				RoundDirector.instance.RequestExtractionPointActivation(photonView.ViewID);
			}
			if (SemiFunc.IsMasterClient())
			{
				RoundDirector.instance.ExtractionPointActivate(photonView.ViewID);
			}
		}
		else
		{
			ButtonPress();
			RoundDirector.instance.extractionPointActive = true;
			RoundDirector.instance.extractionPointCurrent = this;
			RoundDirector.instance.ExtractionPointsLock(base.gameObject);
		}
	}

	public void ButtonPress()
	{
		if (StateIs(State.Idle))
		{
			StateSet(State.Active);
		}
	}

	private void SetLightsEmissionColor(Color color)
	{
		Material[] materials = spotlightHead1.GetComponentInChildren<MeshRenderer>().materials;
		for (int i = 0; i < materials.Length; i++)
		{
			materials[i].SetColor("_EmissionColor", color);
		}
		materials = spotlightHead2.GetComponentInChildren<MeshRenderer>().materials;
		for (int i = 0; i < materials.Length; i++)
		{
			materials[i].SetColor("_EmissionColor", color);
		}
	}

	private void TextBlink(Color textColorOriginal, Color textColor, float time)
	{
		textBlinkTime = time;
		textBlinkColor = textColor;
		textBlinkColorOriginal = textColorOriginal;
	}

	private void TextBlinkLogic()
	{
		if (textBlinkTime > 0f)
		{
			textBlinkTime -= Time.deltaTime;
			if (textBlinkTime <= 0f)
			{
				haulGoalScreen.color = textBlinkColorOriginal;
			}
			else
			{
				haulGoalScreen.color = textBlinkColor;
			}
		}
	}

	public void OnShopClick()
	{
		if (StateIs(State.Active) && !(tubeSlamDownEval < 1f))
		{
			if (haulGoal - haulCurrent >= 0 && haulGoal - haulCurrent != haulGoal)
			{
				StateSet(State.Success);
			}
			else
			{
				StateSet(State.Cancel);
			}
		}
	}

	private void ShopButtonAnimation()
	{
		if (shopButtonAnimation)
		{
			shopButtonAnimationEval += Time.deltaTime * 2f;
			shopButtonAnimationEval = Mathf.Clamp01(shopButtonAnimationEval);
			float num = buttonPressAnimationCurve.Evaluate(shopButtonAnimationEval);
			Color color = new Color(1f, 0.5f, 0f, 1f);
			shopButton.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.Lerp(color, Color.white, num));
			num = Mathf.Clamp(num, 0.5f, 1f);
			shopButton.localScale = new Vector3(1f, num, 1f);
			if (shopButtonAnimationEval >= 1f)
			{
				shopButtonAnimation = false;
				shopButtonAnimationEval = 0f;
			}
		}
	}

	public void HitCeiling()
	{
		ceilingParticles.Play();
		soundTubeHitCeiling.Play(extractionTube.position);
		soundTubeHitCeilingGlobal.Play(extractionTube.position);
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, extractionTube.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, extractionTube.position, 0.1f);
	}

	private void EmojiScreenGlitch(Color color)
	{
		if (emojiScreenGlitchTimer <= 0f)
		{
			soundEmojiGlitch.Play(emojiScreen.transform.position);
		}
		emojiScreenGlitchTimer = 0.2f;
		emojiScreenGlitch.SetActive(value: true);
		emojiScreen.enabled = false;
		emojiScreenGlitch.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color);
	}

	private void HaulGoalSet(int value)
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("HaulGoalSetRPC", RpcTarget.All, value);
			}
		}
		else
		{
			HaulGoalSetRPC(value);
		}
	}

	[PunRPC]
	public void HaulGoalSetRPC(int value, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			haulGoal = value;
			RoundDirector.instance.extractionHaulGoal = value;
			haulGoalFetched = true;
		}
	}

	private void ResetLights()
	{
		spotlight1.range = spotLightRange;
		spotlight2.range = spotLightRange;
		spotlight1.intensity = spotlightIntensity;
		spotlight2.intensity = spotlightIntensity;
		emojiLight.intensity = emojiLightIntensity;
	}

	private void EmojiScreenGlitchLogic()
	{
		if (emojiDelay > 0f)
		{
			return;
		}
		currentEmoji = emojiScreen.text;
		if (prevEmoji != currentEmoji)
		{
			prevEmoji = currentEmoji;
			EmojiScreenGlitch(Color.yellow);
		}
		if (!(emojiScreenGlitchTimer <= 0f))
		{
			Vector2 textureOffset = emojiScreenGlitch.GetComponent<MeshRenderer>().material.GetTextureOffset("_MainTex");
			textureOffset.y += Time.deltaTime * 15f;
			emojiScreenGlitch.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", textureOffset);
			emojiScreenGlitchTimer -= Time.deltaTime;
			if (thirtyFPSUpdate)
			{
				float num = Random.Range(0.1f, 1f);
				emojiScreenGlitch.GetComponent<MeshRenderer>().material.SetTextureScale("_MainTex", new Vector2(num, num));
			}
			if (emojiScreenGlitchTimer <= 0f)
			{
				emojiScreenGlitch.SetActive(value: false);
				emojiScreen.enabled = true;
			}
		}
	}

	private void HaulInternalStatsUpdate()
	{
		haulPrevious = haulCurrent;
		haulCurrent = RoundDirector.instance.currentHaul + RoundDirector.instance.extractionPointSurplus;
		if (isShop)
		{
			haulCurrent = SemiFunc.ShopGetTotalCost() * 1000;
			haulGoal = SemiFunc.StatGetRunCurrency() * 1000;
		}
	}

	private void HaulChecker()
	{
		HaulInternalStatsUpdate();
		if (haulPrevious != haulCurrent)
		{
			haulUpdateEffectTimer = 0.3f;
			if (haulCurrent > haulPrevious)
			{
				deductedFromHaul = false;
				soundHaulIncrease.Play(emojiScreen.transform.position);
			}
			else
			{
				deductedFromHaul = true;
				soundHaulDecrease.Play(emojiScreen.transform.position);
			}
			haulPrevious = haulCurrent;
		}
		if (haulUpdateEffectTimer > 0f)
		{
			haulUpdateEffectTimer -= Time.deltaTime;
			haulUpdateEffectTimer = Mathf.Max(0f, haulUpdateEffectTimer);
			Color color = Color.white;
			if (deductedFromHaul)
			{
				color = Color.red;
			}
			if (isShop)
			{
				color = Color.red;
				if (deductedFromHaul)
				{
					color = Color.white;
				}
			}
			haulGoalScreen.color = color;
			if (thirtyFPSUpdate)
			{
				haulGoalScreen.text = GlitchyText();
			}
			resetHaulText = false;
		}
		else if (!resetHaulText)
		{
			haulGoalScreen.color = originalHaulColor;
			SetHaulText();
			resetHaulText = true;
		}
		if (isShop)
		{
			return;
		}
		if (haulGoal - haulCurrent <= 0 && haulGoalFetched)
		{
			successDelay -= Time.deltaTime;
			if (successDelay <= 0f)
			{
				StateSet(State.Success);
			}
		}
		else
		{
			successDelay = 1.5f;
		}
	}

	private void UpdateEmojiText()
	{
		if (haulCurrent == 0)
		{
			SetEmojiScreen("<sprite name=:'(>");
			if (isShop)
			{
				SetEmojiScreen("<sprite name=shoppingcart>");
				if (isThief)
				{
					SetEmojiScreen("<sprite name=thief>");
				}
			}
			return;
		}
		float num = (float)haulCurrent / (float)haulGoal;
		string[] array = new string[6] { "<sprite name=:'(>", "<sprite name=:(>", "<sprite name=mellow>", "<sprite name=:)>", "<sprite name=:D>", "<sprite name=cryinglaughing>" };
		if (isShop)
		{
			num = 0f;
			array = new string[1] { "<sprite name=shoppingcart>" };
			if (isThief)
			{
				array = new string[1] { "<sprite name=thief>" };
			}
		}
		int value = Mathf.FloorToInt(num * (float)(array.Length - 1));
		value = Mathf.Clamp(value, 0, array.Length - 1);
		SetEmojiScreen(array[value]);
	}

	private void ThirtyFPS()
	{
		if (thirtyFPSUpdateTimer > 0f)
		{
			thirtyFPSUpdateTimer -= Time.deltaTime;
			thirtyFPSUpdateTimer = Mathf.Max(0f, thirtyFPSUpdateTimer);
		}
		else
		{
			thirtyFPSUpdate = true;
			thirtyFPSUpdateTimer = 1f / 30f;
		}
	}

	private void Update()
	{
		ShopButtonAnimation();
		bool playing = tubeHit && tubeSlamDownEval > 0.8f && StateIs(State.Extracting);
		soundSuckLoop.PlayLoop(playing, 2f, 2f);
		bool playing2 = StateIs(State.Warning);
		soundWarningLightsLoop.PlayLoop(playing2, 2f, 2f);
		bool playing3 = StateIs(State.Surplus) && !haulSurplusAnimatedDone && !surplusIntroText;
		surplusStateIncreaseLoop.PlayLoop(playing3, 2f, 2f);
		ThirtyFPS();
		HaulBarAnimateScale();
		StateCancel();
		StateIdle();
		StateActive();
		StateSuccess();
		StateSurplus();
		StateWarning();
		StateExtracting();
		StateComplete();
		StateTaxReturn();
		EmojiScreenGlitchLogic();
		TextBlinkLogic();
		SurplusLightLogic();
		TubeScreenTextChangeLogic();
		stateEnd = false;
		thirtyFPSUpdate = false;
		if (stateTimer > 0f)
		{
			if (initialStateTime == 0f)
			{
				initialStateTime = stateTimer;
			}
			stateTimer -= Time.deltaTime;
			stateTimer = Mathf.Max(0f, stateTimer);
		}
		else if (!stateEnd && stateTimer != -123f)
		{
			stateEnd = true;
			stateTimer = -123f;
			initialStateTime = 0f;
		}
		if (stateSetTo != State.None)
		{
			currentState = stateSetTo;
			stateStart = true;
			settingState = false;
			stateEnd = false;
			stateSetTo = State.None;
		}
		if (isLocked && buttonGrabObject.enabled && buttonGrabObject.playerGrabbing.Count > 0 && buttonDenyCooldown <= 0f)
		{
			foreach (PhysGrabber item in buttonGrabObject.playerGrabbing.ToList())
			{
				if (!SemiFunc.IsMultiplayer())
				{
					item.ReleaseObject(buttonGrabObject.photonView.ViewID);
					continue;
				}
				item.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, 1.5f, buttonGrabObject.photonView.ViewID);
			}
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("ButtonDenyRPC", RpcTarget.All);
			}
			else
			{
				ButtonDenyRPC();
			}
		}
		if (buttonDenyCooldown > 0f)
		{
			buttonDenyCooldown -= Time.deltaTime;
			if (buttonDenyCooldown <= 0f)
			{
				buttonLight.enabled = false;
				button.material = buttonOff;
				buttonGrabObject.enabled = true;
			}
		}
		if (buttonDenyActive)
		{
			buttonDenyLerp += 0.75f * Time.deltaTime;
			buttonDenyLerp = Mathf.Clamp01(buttonDenyLerp);
			buttonDenyTransform.localPosition = new Vector3(0f, 0f, -0.06f * buttonDenyCurve.Evaluate(buttonDenyLerp));
			if (buttonDenyLerp >= 1f)
			{
				buttonDenyActive = false;
				buttonDenyLerp = 0f;
			}
		}
	}

	private void StateIdle()
	{
		if (!StateIs(State.Idle))
		{
			return;
		}
		if (stateStart)
		{
			stateStart = false;
		}
		if (isLocked)
		{
			if (tubeScreenTextString != "LOCKED")
			{
				TubeScreenTextChange("LOCKED", Color.red);
			}
			ButtonToggle(_active: false);
			return;
		}
		Color color = new Color(1f, 0.5f, 0f);
		if (tubeScreenTextString != "READY")
		{
			TubeScreenTextChange("READY", color);
		}
		if (soundPingTimer <= 0f && !SemiFunc.RunIsTutorial() && !SemiFunc.RunIsRecording())
		{
			soundPing.Play(soundPingTransform.position);
			soundPingTimer = 4f;
		}
		else
		{
			soundPingTimer -= Time.deltaTime;
		}
		ButtonToggle(_active: true);
	}

	private void StateActive()
	{
		if (!StateIs(State.Active))
		{
			return;
		}
		if (stateStart)
		{
			extractionArea.SetActive(value: false);
			taxReturn = false;
			if (tubeScreenTextString != "ACTIVE")
			{
				TubeScreenTextChange("ACTIVE", Color.green);
			}
			DirtFinderMapFloor[] array = mapInactive;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].MapObject.Hide();
			}
			array = mapActive;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].MapObject.Show();
			}
			platform.gameObject.SetActive(value: true);
			emojiLight.enabled = true;
			ButtonToggle(_active: false);
			emojiScreen.enabled = true;
			haulGoalScreen.enabled = true;
			spotlight1.enabled = false;
			spotlight2.enabled = false;
			emojiLight.enabled = false;
			emojiScreen.enabled = false;
			roomVolume.SetActive(value: true);
			tubeSlamDownEval = 0f;
			emojiLight.color = originalEmojiLightColor;
			emojiDelay = 2f;
			ResetLightIntensity();
			spotlight1Delay = 1f;
			spotlight2Delay = 1.5f;
			successDelay = 0f;
			tubeHitCeiling = false;
			ResetLights();
			if (cancelExtraction)
			{
				buttonPressed = false;
				tubeSlamDownEval = 1f;
				cancelExtraction = false;
				spotlight1Delay = 0f;
				spotlight2Delay = 0f;
				emojiDelay = 0f;
			}
			else
			{
				if (!isShop)
				{
					if (!inStartRoom)
					{
						SemiFunc.EnemyInvestigate(base.transform.position, 20f);
					}
					jingleLocal.Play(base.transform.position);
					jingleGlobal.Play(base.transform.position);
				}
				buttonDelay = 0.5f;
				buttonPressed = true;
				GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, button.transform.position, 0.1f);
				GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, button.transform.position, 0.1f);
				soundButton.Play(button.transform.position);
				int value = RoundDirector.instance.haulGoal / RoundDirector.instance.extractionPoints;
				if (isShop)
				{
					value = SemiFunc.StatGetRunCurrency() * 1000;
				}
				HaulGoalSet(value);
			}
			haulGoalScreen.color = originalHaulColor;
			currentEmoji = emojiScreen.text;
			stateStart = false;
			HaulInternalStatsUpdate();
			SetHaulText();
			UpdateEmojiText();
		}
		if (!tubeHitCeiling && buttonPressed && tubeSlamDownEval > 0.8f)
		{
			tubeHitCeiling = true;
			HitCeiling();
		}
		if (buttonPressed)
		{
			if (buttonPressEval < 1f)
			{
				buttonPressEval += 8f * Time.deltaTime;
				buttonPressEval = Mathf.Min(1f, buttonPressEval);
				float num = buttonPressAnimationCurve.Evaluate(buttonPressEval);
				button.transform.localPosition = new Vector3(buttonOriginalPosition.x, buttonOriginalPosition.y, buttonOriginalPosition.z - 0.1f * num);
			}
			if (emojiDelay > 0f && !isShop)
			{
				SemiFunc.UIBigMessage("EXTRACTION POINT ACTIVATED", "{!}", 25f, Color.white, Color.white);
				SemiFunc.UIFocusText("Fill the extraction point with valuables", Color.white, AssetManager.instance.colorYellow);
			}
		}
		if (buttonDelay >= 0f)
		{
			buttonDelay -= Time.deltaTime;
		}
		if (spotlight1Delay <= 0f)
		{
			if (!spotlight1.enabled)
			{
				if (buttonPressed)
				{
					soundActivate1.Play(spotlight1.transform.position);
					GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, spotlight1.transform.position, 0.1f);
					GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, spotlight1.transform.position, 0.1f);
				}
				spotlight1.enabled = true;
				spotlight1.color = spotLightColor;
			}
		}
		else if (buttonDelay <= 0f)
		{
			spotlight1Delay -= Time.deltaTime;
		}
		if (spotlight2Delay <= 0f)
		{
			if (!spotlight2.enabled)
			{
				if (buttonPressed)
				{
					soundActivate2.Play(spotlight2.transform.position);
					GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, spotlight2.transform.position, 0.1f);
					GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, spotlight2.transform.position, 0.1f);
				}
				spotlight2.enabled = true;
				spotlight2.color = spotLightColor;
			}
		}
		else if (buttonDelay <= 0f)
		{
			spotlight2Delay -= Time.deltaTime;
		}
		if (emojiDelay <= 0f)
		{
			if (!emojiLight.enabled)
			{
				if (buttonPressed)
				{
					soundActivate3.Play(emojiScreen.transform.position);
					GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, emojiScreen.transform.position, 0.1f);
					GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, emojiScreen.transform.position, 0.1f);
				}
				emojiLight.enabled = true;
				emojiScreen.enabled = true;
				SetHaulText();
			}
		}
		else
		{
			if (buttonDelay <= 0f)
			{
				emojiDelay -= Time.deltaTime;
			}
			if (thirtyFPSUpdate)
			{
				haulGoalScreen.text = GlitchyText();
			}
		}
		if (tubeSlamDownEval < 1f && buttonDelay <= 0f)
		{
			if (tubeSlamDownEval == 0f)
			{
				tubeHitParticles.SetActive(value: true);
				upParticles.Play();
				soundTubeRaise.Play(base.transform.position);
				soundTubeRaiseGlobal.Play(base.transform.position);
				GameDirector.instance.CameraImpact.ShakeDistance(8f, 3f, 8f, extractionTube.position, 0.1f);
				GameDirector.instance.CameraShake.ShakeDistance(8f, 3f, 8f, extractionTube.position, 0.1f);
			}
			tubeSlamDownEval += 2f * Time.deltaTime;
			tubeSlamDownEval = Mathf.Min(1f, tubeSlamDownEval);
			float num2 = tubeSlamDown.Evaluate(tubeSlamDownEval);
			extractionTube.localPosition = new Vector3(tubeStartPosition.x, tubeStartPosition.y * num2, tubeStartPosition.z);
		}
		if (!isCompletedRightAway)
		{
			HaulChecker();
		}
	}

	private void HaulBarAnimateScale()
	{
		float value = Mathf.Lerp(haulBar.localScale.x, haulBarTargetScale, Time.deltaTime * 10f);
		value = Mathf.Clamp01(value);
		if (float.IsNaN(value))
		{
			value = 0f;
		}
		haulBar.localScale = new Vector3(value, 1f, 1f);
		haulBar.localScale = new Vector3(Mathf.Min(1f, haulBar.localScale.x), 1f, 1f);
	}

	private void SetHaulText()
	{
		if (!isShop)
		{
			string text = "<color=#bd4300>$</color>";
			haulGoalScreen.text = text + SemiFunc.DollarGetString(Mathf.Max(0, haulCurrent));
			haulBarTargetScale = (float)haulCurrent / (float)haulGoal;
		}
		else
		{
			string text2 = "<color=#bd4300>$</color>";
			haulGoalScreen.text = text2 + SemiFunc.DollarGetString(haulGoal - haulCurrent);
			if (haulGoal - haulCurrent < 0)
			{
				haulGoalScreen.text = SemiFunc.DollarGetString(haulGoal - haulCurrent);
				haulGoalScreen.color = Color.red;
			}
		}
		UpdateEmojiText();
	}

	private void SetEmojiScreen(string emoji, bool creepyFace = false)
	{
		if (creepyFace)
		{
			grossUp.SetActive(value: true);
		}
		else
		{
			grossUp.SetActive(value: false);
		}
		emojiScreen.text = "<size=100>|</size>" + emoji + "<size=100>|</size>";
		emojiScreen.color = new Color(1f, 1f, 1f, 0f);
	}

	private void ShopButtonPushVisualsStart()
	{
		if (isShop && !shopButtonAnimation)
		{
			shopButton.localScale = new Vector3(1f, 0.1f, 1f);
			soundButton.Play(shopButton.position);
			shopButtonAnimationEval = 0f;
			shopButtonAnimation = true;
		}
	}

	private void StateSuccess()
	{
		if (!StateIs(State.Success))
		{
			return;
		}
		if (stateStart)
		{
			ShopButtonPushVisualsStart();
			ResetLights();
			ResetLightIntensity();
			extractionArea.SetActive(value: false);
			SetSpotlightColor(Color.green);
			tubeSlamDownEval = 0f;
			tubeHitParticles.SetActive(value: false);
			haulGoalScreen.text = "$" + SemiFunc.DollarGetString(Mathf.Max(0, RoundDirector.instance.haulGoal - RoundDirector.instance.currentHaul));
			stateStart = false;
			SetEmojiScreen("<sprite name=check>");
			emojiLight.color = Color.green;
			EmojiScreenGlitch(Color.green);
			emojiDelay = 0f;
			soundSuccess.Play(base.transform.position);
			soundGreenLights.Play(base.transform.position);
			haulGoalScreen.text = "!!!!!!!!!";
			haulGoalScreen.color = Color.green;
			stateTimer = 2f;
			if (isShop)
			{
				stateTimer = 1f;
			}
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, haulGoalScreen.transform.position, 0.1f);
			GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, haulGoalScreen.transform.position, 0.1f);
		}
		if (!isCompletedRightAway)
		{
			CancelExtraction();
		}
		if (!stateEnd)
		{
			return;
		}
		if (isShop)
		{
			StateSet(State.Warning);
			return;
		}
		haulSurplus = Mathf.Abs(RoundDirector.instance.currentHaul - haulGoal);
		if (haulSurplus > 0)
		{
			StateSet(State.Surplus);
		}
		else
		{
			StateSet(State.Warning);
		}
	}

	private void StateSurplus()
	{
		if (!StateIs(State.Surplus))
		{
			return;
		}
		if (stateStart)
		{
			if (RoundDirector.instance.extractionPointsCompleted != RoundDirector.instance.extractionPoints - 1)
			{
				taxReturn = true;
			}
			ResetLights();
			ResetLightIntensity();
			extractionArea.SetActive(value: false);
			SetSpotlightColor(Color.green);
			SetEmojiScreen("<sprite name=surplus>");
			emojiLight.color = Color.green;
			EmojiScreenGlitch(Color.green);
			emojiDelay = 0f;
			tubeSlamDownEval = 0f;
			tubeHitParticles.SetActive(value: false);
			stateStart = false;
			surplusStateStart.Play(base.transform.position);
			soundGreenLights.Play(base.transform.position);
			haulGoalScreen.text = "TAX RETURN";
			haulGoalScreen.color = Color.green;
			haulSurplusAnimated = 0;
			haulSurplusAnimatedDone = false;
			surplusIntroText = true;
			cancelExtraction = true;
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, haulGoalScreen.transform.position, 0.1f);
			GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, haulGoalScreen.transform.position, 0.1f);
			if (haulSurplus < 1000)
			{
				stateTimer = 4f;
				surplusLevel = 1;
			}
			else if (haulSurplus >= 1000 && haulSurplus < 5000)
			{
				stateTimer = 5f;
				surplusLevel = 2;
			}
			else if (haulSurplus >= 5000 && haulSurplus < 10000)
			{
				stateTimer = 6f;
				surplusLevel = 3;
			}
			else if (haulSurplus >= 10000 && haulSurplus < 20000)
			{
				stateTimer = 7f;
				surplusLevel = 4;
			}
			else if (haulSurplus >= 20000 && haulSurplus < 50000)
			{
				stateTimer = 8f;
				surplusLevel = 4;
			}
			else if (haulSurplus >= 50000)
			{
				stateTimer = 9f;
				surplusLevel = 4;
			}
		}
		if (!isCompletedRightAway)
		{
			CancelExtraction();
		}
		if (!haulSurplusAnimatedDone)
		{
			haulSurplus = Mathf.Abs(RoundDirector.instance.currentHaul - haulGoal);
		}
		float num = 1.5f;
		if (stateTimer < initialStateTime - num)
		{
			surplusIntroText = false;
			if (haulSurplusAnimated < haulSurplus && initialStateTime != 0f)
			{
				float num2 = 2f;
				float value = (initialStateTime - num - stateTimer) / (initialStateTime - num - num2);
				value = Mathf.Clamp01(value);
				surplusStateIncreaseLoop.LoopPitch = 1f + value;
				haulSurplusAnimated = (int)((float)haulSurplus * value);
				haulGoalScreen.text = "+$" + SemiFunc.DollarGetString(haulSurplusAnimated);
			}
			else if (!haulSurplusAnimatedDone && initialStateTime != 0f)
			{
				haulSurplus = Mathf.Abs(haulCurrent - haulGoal);
				SetEmojiScreen("<sprite name=moneyeyes>");
				GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, haulGoalScreen.transform.position, 0.1f);
				GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, haulGoalScreen.transform.position, 0.1f);
				haulSurplusAnimatedDone = true;
				TextBlink(Color.green, Color.white, 0.5f);
				if (surplusLevel == 1)
				{
					surplusStateDoneLevel1.Play(base.transform.position);
				}
				if (surplusLevel == 2)
				{
					surplusStateDoneLevel1.Play(base.transform.position);
				}
				if (surplusLevel == 3)
				{
					surplusStateDoneLevel1.Play(base.transform.position);
				}
				if (surplusLevel == 4)
				{
					surplusStateDoneLevel1.Play(base.transform.position);
				}
			}
		}
		if (stateEnd && !isCompletedRightAway)
		{
			StateSet(State.Warning);
		}
	}

	private void SpawnTaxReturn()
	{
		if (RoundDirector.instance.extractionPointSurplus > 0)
		{
			if (SemiFunc.IsMasterClientOrSingleplayer())
			{
				GameObject gameObject = AssetManager.instance.surplusValuableSmall;
				if (RoundDirector.instance.extractionPointSurplus > 10000)
				{
					gameObject = AssetManager.instance.surplusValuableBig;
				}
				else if (RoundDirector.instance.extractionPointSurplus > 5000)
				{
					gameObject = AssetManager.instance.surplusValuableMedium;
				}
				GameObject gameObject2 = null;
				gameObject2 = (SemiFunc.IsMultiplayer() ? PhotonNetwork.InstantiateRoomObject("Valuables/" + gameObject.name, surplusSpawnTransform.position, Quaternion.identity, 0) : Object.Instantiate(gameObject, surplusSpawnTransform.position, Quaternion.identity));
				gameObject2.GetComponent<ValuableObject>().dollarValueOverride = RoundDirector.instance.extractionPointSurplus;
				gameObject2.GetComponent<PhysGrabObject>().spawnTorque = Random.insideUnitSphere * 0.05f;
			}
			surplusLightActive = true;
			surplusLight.intensity = surplusLightIntensity;
			surplusLight.range = surplusLightRange;
		}
		RoundDirector.instance.extractionPointSurplus = 0;
	}

	private bool CancelExtraction()
	{
		haulCurrent = RoundDirector.instance.currentHaul + RoundDirector.instance.extractionPointSurplus;
		if (isShop)
		{
			haulCurrent = SemiFunc.ShopGetTotalCost() * 1000;
		}
		if (!isShop)
		{
			if (haulGoal - haulCurrent > 0)
			{
				StateSet(State.Cancel);
				return true;
			}
		}
		else if (haulGoal - haulCurrent < 0 || haulGoal - haulCurrent == haulGoal)
		{
			StateSet(State.Cancel);
			return true;
		}
		return false;
	}

	private void ResetLightIntensity()
	{
		emojiLight.intensity = emojiLightIntensity;
		spotlight1.intensity = spotlightIntensity;
		spotlight2.intensity = spotlightIntensity;
	}

	private void StateWarning()
	{
		if (!StateIs(State.Warning))
		{
			return;
		}
		if (stateStart)
		{
			ResetLights();
			SetSpotlightColor(Color.red);
			ResetLightIntensity();
			spotlight1.intensity = spotlightIntensity * 2f;
			spotlight2.intensity = spotlightIntensity * 2f;
			spotlight1.range *= 2f;
			spotlight2.range *= 2f;
			SetEmojiScreen("<sprite name=!>");
			emojiLight.color = Color.red;
			EmojiScreenGlitch(Color.red);
			stateTimer = 3f;
			stateStart = false;
			haulGoalScreen.text = "3";
			haulGoalScreen.color = Color.red;
			soundAlarm.Play(emojiScreen.transform.position);
			soundAlarmGlobal.Play(emojiScreen.transform.position);
			SemiFunc.EnemyInvestigate(base.transform.position, 20f);
			extractionArea.SetActive(value: true);
			Material material = extractionArea.GetComponent<MeshRenderer>().material;
			if (SemiFunc.Photosensitivity())
			{
				material.SetFloat("_SpinSpeed", 0f);
			}
			else
			{
				material.SetFloat("_SpinSpeed", 2f);
			}
			HaulInternalStatsUpdate();
			SetHaulText();
		}
		if (stateTimer > 0f)
		{
			string text = Mathf.CeilToInt(stateTimer).ToString();
			if (haulGoalScreen.text != text)
			{
				soundAlarm.Play(emojiScreen.transform.position);
				soundAlarmGlobal.Play(emojiScreen.transform.position);
				haulGoalScreen.text = text;
			}
		}
		else if (thirtyFPSUpdate)
		{
			haulGoalScreen.text = GlitchyText();
		}
		if (!SemiFunc.Photosensitivity())
		{
			spotlightHead1.Rotate(0f, 500f * Time.deltaTime, 0f);
			spotlightHead2.Rotate(0f, -500f * Time.deltaTime, 0f);
		}
		if (!isCompletedRightAway)
		{
			CancelExtraction();
		}
		if (stateEnd)
		{
			StateSet(State.Extracting);
		}
	}

	private void SetSpotlightColor(Color color)
	{
		spotlight1.color = color;
		spotlight2.color = color;
		SetLightsEmissionColor(color);
		spotlight1.intensity = spotlightIntensity;
		spotlight2.intensity = spotlightIntensity;
	}

	private void StateCancel()
	{
		if (!StateIs(State.Cancel))
		{
			return;
		}
		if (stateStart)
		{
			ShopButtonPushVisualsStart();
			taxReturn = false;
			extractionArea.SetActive(value: false);
			SetEmojiScreen("<sprite name=X>");
			emojiLight.color = Color.red;
			emojiLight.intensity = emojiLightIntensity;
			haulGoalScreen.color = Color.red;
			ResetLights();
			spotlight1CancelRotation = spotlightHead1.rotation;
			spotlight2CancelRotation = spotlightHead2.rotation;
			if (spotlight1CancelRotation != spotlight1StartRotation || spotlight2CancelRotation != spotlight2StartRotation)
			{
				cancelSpotlights = true;
			}
			cancelSpotlightEval = 0f;
			ResetLightIntensity();
			SetSpotlightColor(Color.red);
			stateTimer = 1f;
			stateStart = false;
			suckParticles.Stop();
			soundCancel.Play(base.transform.position);
			cancelExtraction = true;
			GameDirector.instance.CameraImpact.ShakeDistance(8f, 3f, 8f, emojiScreen.transform.position, 0.1f);
			GameDirector.instance.CameraShake.ShakeDistance(8f, 3f, 8f, emojiScreen.transform.position, 0.1f);
			if (cancelTube)
			{
				tubeCancelPosition = extractionTube.localPosition;
				tubeSlamDownEval = 0f;
				stateTimer = 2f;
				soundTubeRetract.Play(base.transform.position);
				tubeHitCeiling = false;
			}
		}
		if (cancelTube && !tubeHitCeiling && tubeSlamDownEval > 0.8f)
		{
			tubeHitCeiling = true;
			HitCeiling();
		}
		if (thirtyFPSUpdate)
		{
			haulGoalScreen.text = GlitchyText();
		}
		if (cancelTube)
		{
			if (tubeSlamDownEval < 1f)
			{
				tubeSlamDownEval += 4f * Time.deltaTime;
				tubeSlamDownEval = Mathf.Min(1f, tubeSlamDownEval);
				float num = tubeSlamDown.Evaluate(tubeSlamDownEval);
				extractionTube.localPosition = new Vector3(tubeStartPosition.x, tubeStartPosition.y * num, tubeStartPosition.z);
			}
			else
			{
				cancelTube = false;
			}
		}
		if (cancelSpotlights)
		{
			if (cancelSpotlightEval < 1f)
			{
				cancelSpotlightEval += 4f * Time.deltaTime;
				cancelSpotlightEval = Mathf.Min(1f, cancelSpotlightEval);
				float t = tubeSlamDown.Evaluate(cancelSpotlightEval);
				spotlightHead1.rotation = Quaternion.Lerp(spotlight1CancelRotation, spotlight1StartRotation, t);
				spotlightHead2.rotation = Quaternion.Lerp(spotlight2CancelRotation, spotlight2StartRotation, t);
			}
			else
			{
				cancelSpotlights = false;
			}
		}
		if (stateEnd)
		{
			StateSet(State.Active);
		}
	}

	private void StateExtracting()
	{
		if (!StateIs(State.Extracting))
		{
			extractionSurplusCompleted = false;
			return;
		}
		if (stateStart)
		{
			Color color = new Color(1f, 0.5f, 0f);
			TubeScreenTextChange("EXTRACTING", color);
			cancelTube = true;
			extractionArea.SetActive(value: false);
			ResetLights();
			stateTimer = 5f;
			tubeHit = false;
			stateStart = false;
			cancelExtraction = false;
			soundTubeBuildup.Play(base.transform.position);
			soundAlarmFinal.Play(emojiScreen.transform.position);
			tubeSlamDownEval = 0f;
			if (isShop)
			{
				hurtColliderMainTimer = 0.25f;
			}
			else
			{
				hurtColliderMainTimer = 1f;
			}
			CurrencyUI.instance.FetchCurrency();
			if (isShop)
			{
				stateTimer = 2f;
			}
			HaulInternalStatsUpdate();
			SetHaulText();
			SetEmojiScreen("<sprite name=creepycrying>", creepyFace: true);
		}
		if (tubeSlamDownEval > 0f && tubeSlamDownEval < 0.8f)
		{
			hurtColliderMain.SetActive(value: false);
			hurtColliders.SetActive(value: true);
		}
		else
		{
			hurtColliders.SetActive(value: false);
		}
		if (tubeSlamDownEval > 0.8f && !tubeHit)
		{
			tubeHitParticles.SetActive(value: true);
			GameDirector.instance.CameraImpact.ShakeDistance(10f, 3f, 8f, extractionTube.position, 0.1f);
			GameDirector.instance.CameraShake.ShakeDistance(10f, 3f, 8f, extractionTube.position, 0.1f);
			suckParticles.Play();
			tubeHit = true;
			amountOfValuables = RoundDirector.instance.dollarHaulList.Count;
			suckUpTimeLeft = stateTimer;
			suckUpVariableTimer = suckUpTimeLeft / (float)amountOfValuables;
			soundTubeSlam.Play(base.transform.position);
			soundTubeSlamGlobal.Play(base.transform.position);
			if (!isShop)
			{
				if (!isCompletedRightAway)
				{
					RoundDirector.instance.HaulCheck();
				}
			}
			else
			{
				ShopManager.instance.ShopCheck();
			}
			extractionHaul = haulGoal;
			if (!isCompletedRightAway && CancelExtraction())
			{
				return;
			}
		}
		if (cancelExtraction)
		{
			return;
		}
		if (tubeHit)
		{
			if (!isShop)
			{
				if (!isCompletedRightAway)
				{
					SemiFunc.UIBigMessage("EXTRACTION POINT COMPLETED", "{check}", 25f, Color.white, Color.white);
				}
				else
				{
					SemiFunc.UIBigMessage("EXTRACTION POINT SKIPPED", "{:O}", 25f, Color.white, Color.white);
				}
				haulSurplus = Mathf.Abs(haulCurrent - haulGoal);
				if (RoundDirector.instance.extractionPointsCompleted == RoundDirector.instance.extractionPoints - 1)
				{
					extractionHaul = RoundDirector.instance.extractionHaulGoal + RoundDirector.instance.extractionPointSurplus;
				}
				if (!extractionSurplusCompleted)
				{
					ExtractionPointSurplus();
					RoundDirector.instance.ExtractionCompletedAllCheck();
					extractionSurplusCompleted = true;
				}
				HaulUI.instance.Hide();
				ShopCostUI.instance.Show();
				CurrencyUI.instance.Show();
				float num = stateTimer / suckUpTimeLeft;
				ShopCostUI.instance.animatedValue = Mathf.CeilToInt(Mathf.Lerp(0f, Mathf.Ceil(extractionHaul / 1000), 1f - num));
			}
			if (suckUpVariableTimer <= 0f)
			{
				if (!isShop)
				{
					DestroyTheFirstPhysObjectsInHaulList();
				}
				else
				{
					DestroyTheFirstPhysObjectsInShopList();
				}
				suckUpVariableTimer = suckUpTimeLeft / (float)amountOfValuables;
			}
			else
			{
				suckUpVariableTimer -= Time.deltaTime;
			}
			if (hurtColliderMainTimer > 0f)
			{
				hurtColliderMainTimer -= Time.deltaTime;
				if (hurtColliderMainTimer <= 0f)
				{
					hurtColliderMain.SetActive(value: true);
				}
			}
			GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 8f, extractionTube.position, 0.5f);
			GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, extractionTube.position, 0.5f);
		}
		if (tubeSlamDownEval < 1f)
		{
			if (!SemiFunc.Photosensitivity())
			{
				spotlightHead1.Rotate(0f, 500f * Time.deltaTime, 0f);
				spotlightHead2.Rotate(0f, -500f * Time.deltaTime, 0f);
			}
			if (thirtyFPSUpdate)
			{
				haulGoalScreen.text = GlitchyText();
			}
			tubeSlamDownEval += 2f * Time.deltaTime;
			tubeSlamDownEval = Mathf.Min(1f, tubeSlamDownEval);
			float num2 = tubeSlamDown.Evaluate(tubeSlamDownEval);
			extractionTube.localPosition = new Vector3(tubeStartPosition.x, tubeStartPosition.y * (1f - num2), tubeStartPosition.z);
			spotlight1.intensity = 6f * (1f - num2);
			spotlight2.intensity = 6f * (1f - num2);
			emojiLight.intensity = 5f * (1f - num2);
		}
		if (tubeHit && !isShop)
		{
			extractionTube.localPosition = new Vector3(0f, 0f + 0.025f * Mathf.Sin(Time.time * 60f), 0f);
			suckInRampEval += 2f * Time.deltaTime;
			suckInRampEval = Mathf.Min(1f, suckInRampEval);
			float num3 = tubeSlamDown.Evaluate(suckInRampEval);
			ramp.localPosition = new Vector3(rampStartPosition.x, rampStartPosition.y - 0.05f * num3, rampStartPosition.z * (1f - num3));
		}
		if ((double)stateTimer <= 0.3)
		{
			upParticles.Play();
		}
		if (stateEnd)
		{
			hurtColliderMain.SetActive(value: false);
			if (!taxReturn)
			{
				StateSet(State.Complete);
			}
			else
			{
				StateSet(State.TaxReturn);
			}
			tubeHitParticles.SetActive(value: false);
			if (!isShop)
			{
				DestroyAllPhysObjectsInHaulList();
				roomVolume.SetActive(value: false);
			}
			else
			{
				DestroyAllPhysObjectsInShoppingList();
			}
		}
	}

	private string GlitchyText()
	{
		string text = "";
		for (int i = 0; i < 9; i++)
		{
			bool flag = false;
			if (Random.Range(0, 4) == 0 && i <= 5)
			{
				text += "TAX";
				i += 2;
				flag = true;
			}
			if (Random.Range(0, 3) == 0 && !flag)
			{
				text += "$";
				flag = true;
			}
			if (!flag)
			{
				text += Random.Range(0, 10);
			}
		}
		return text;
	}

	private void ThiefPunishment()
	{
		if (SemiFunc.ShopGetTotalCost() <= 0)
		{
			return;
		}
		isThief = true;
		ShopManager.instance.isThief = true;
		SetEmojiScreen("<sprite name=thief>");
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (TruckScreenText.instance.playerChatBoxState == TruckScreenText.PlayerChatBoxState.Idle)
			{
				TruckScreenText.instance.GotoPage(3);
			}
			if (SemiFunc.IsMultiplayer())
			{
				for (int i = 0; i < 5; i++)
				{
					PhotonNetwork.InstantiateRoomObject("Items/Item Grenade Explosive", base.transform.position + base.transform.up * 0.2f + base.transform.up * (0.2f * (float)i), Quaternion.identity, 0);
				}
			}
			else
			{
				for (int j = 0; j < 5; j++)
				{
					Object.Instantiate(Resources.Load("Items/Item Grenade Explosive"), base.transform.position + base.transform.up * 0.2f + base.transform.up * (0.2f * (float)j), Quaternion.identity);
				}
			}
		}
		SemiFunc.UIFocusText("Avoid the grenades!", Color.red, Color.red);
	}

	private void StateTaxReturn()
	{
		if (StateIs(State.TaxReturn))
		{
			if (stateStart)
			{
				cancelTube = false;
				tubeHitCeiling = false;
				tubeSlamDownEval = 0f;
				extractionArea.SetActive(value: false);
				tubeHitParticles.SetActive(value: true);
				soundSuckEnd.Play(base.transform.position);
				suckParticles.Stop();
				spotlightHead1.rotation = spotlight1StartRotation;
				spotlightHead2.rotation = spotlight2StartRotation;
				stateStart = false;
				runCurrencyBefore = SemiFunc.StatGetRunCurrency();
				extractionHaul = Mathf.CeilToInt(extractionHaul / 1000);
				SemiFunc.StatSetRunCurrency(runCurrencyBefore + extractionHaul);
				SemiFunc.StatSetRunTotalHaul(SemiFunc.StatGetRunTotalHaul() + extractionHaul);
				CurrencyUI.instance.FetchCurrency();
				RoundDirector.instance.ExtractionCompleted();
				platform.gameObject.SetActive(value: false);
				completeJingleGlobal.Play(base.transform.position);
				completeJingleLocal.Play(base.transform.position);
				GameDirector.instance.CameraImpact.ShakeDistance(8f, 3f, 8f, base.transform.position, 0.1f);
				GameDirector.instance.CameraShake.ShakeDistance(8f, 3f, 8f, base.transform.position, 0.1f);
				GoalUI.instance.SemiUISpringShakeY(10f, 2f, 0.2f);
				stateTimer = 3f;
			}
			CurrencyUI.instance.Show();
			HaulUI.instance.Hide();
			if (!tubeHitCeiling && tubeSlamDownEval > 0.8f)
			{
				tubeHitCeiling = true;
				HitCeiling();
				TubeScreenTextChange("TAX RETURN", Color.green);
				SpawnTaxReturn();
			}
			if (tubeSlamDownEval < 1f)
			{
				tubeSlamDownEval += 2f * Time.deltaTime;
				tubeSlamDownEval = Mathf.Min(1f, tubeSlamDownEval);
				float num = tubeSlamDown.Evaluate(1f - tubeSlamDownEval);
				extractionTube.localPosition = new Vector3(tubeStartPosition.x, 2.2f * (1f - num), tubeStartPosition.z);
			}
			if (stateEnd)
			{
				StateSet(State.Complete);
			}
		}
	}

	private void StateComplete()
	{
		if (!StateIs(State.Complete))
		{
			return;
		}
		if (stateStart)
		{
			TubeScreenTextChange("COMPLETED", Color.green);
			extractionArea.SetActive(value: false);
			cancelTube = false;
			if (!shopStation)
			{
				DirtFinderMapFloor[] array = mapActive;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].MapObject.Hide();
				}
				array = mapUsed;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].MapObject.Show();
				}
			}
			suckParticles.Stop();
			tubeSlamDownEval = 0f;
			stateStart = false;
			if (!taxReturn)
			{
				tubeHitParticles.SetActive(value: true);
				soundSuckEnd.Play(base.transform.position);
			}
			soundTubeRaise.Play(base.transform.position);
			soundTubeRaiseGlobal.Play(base.transform.position);
			tubeHitCeiling = false;
			spotlightHead1.rotation = spotlight1StartRotation;
			spotlightHead2.rotation = spotlight2StartRotation;
			if (!isShop)
			{
				RoundDirector.instance.extractionPointActive = false;
				RoundDirector.instance.ExtractionPointsUnlock();
				if (!taxReturn)
				{
					HaulInternalStatsUpdate();
					extractionHaul = haulGoal + haulSurplus;
					int num = SemiFunc.StatGetRunTotalHaul();
					runCurrencyBefore = SemiFunc.StatGetRunCurrency();
					extractionHaul = Mathf.CeilToInt(extractionHaul / 1000);
					SemiFunc.StatSetRunCurrency(runCurrencyBefore + extractionHaul);
					SemiFunc.StatSetRunTotalHaul(num + extractionHaul);
					CurrencyUI.instance.FetchCurrency();
					RoundDirector.instance.ExtractionCompleted();
					platform.gameObject.SetActive(value: false);
					completeJingleGlobal.Play(base.transform.position);
					completeJingleLocal.Play(base.transform.position);
				}
				stateTimer = 3f;
				if (RoundDirector.instance.extractionPointsCompleted < RoundDirector.instance.extractionPoints)
				{
					SemiFunc.UIFocusText("Find the next extraction point", Color.white, AssetManager.instance.colorYellow);
				}
				else
				{
					SemiFunc.UIFocusText("Get back to the truck!", Color.white, AssetManager.instance.colorYellow);
				}
			}
			else
			{
				cancelExtraction = true;
				isThief = false;
				ShopManager.instance.isThief = false;
				stateTimer = 1f;
				ThiefPunishment();
				HaulInternalStatsUpdate();
				SetHaulText();
			}
			GameDirector.instance.CameraImpact.ShakeDistance(8f, 3f, 8f, base.transform.position, 0.1f);
			GameDirector.instance.CameraShake.ShakeDistance(8f, 3f, 8f, base.transform.position, 0.1f);
		}
		if (!isShop && stateTimer > 0f)
		{
			CurrencyUI.instance.Show();
		}
		if (!tubeHitCeiling && tubeSlamDownEval > 0.8f)
		{
			tubeHitCeiling = true;
			HitCeiling();
		}
		if (tubeSlamDownEval < 1f)
		{
			tubeSlamDownEval += 2f * Time.deltaTime;
			tubeSlamDownEval = Mathf.Min(1f, tubeSlamDownEval);
			float num2 = tubeSlamDown.Evaluate(1f - tubeSlamDownEval);
			if (taxReturn)
			{
				extractionTube.localPosition = new Vector3(tubeStartPosition.x, Mathf.LerpUnclamped(tubeStartPosition.y, 2.2f, num2), tubeStartPosition.z);
			}
			else
			{
				extractionTube.localPosition = new Vector3(tubeStartPosition.x, tubeStartPosition.y * (1f - num2), tubeStartPosition.z);
			}
		}
		if (stateEnd && isShop)
		{
			cancelExtraction = true;
			StateSet(State.Active);
		}
	}

	private void ExtractionPointSurplus()
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient())
			{
				int num = CalculateSurplus();
				photonView.RPC("ExtractionPointSurplusRPC", RpcTarget.All, num);
			}
		}
		else
		{
			int surplus = CalculateSurplus();
			ExtractionPointSurplusRPC(surplus);
		}
	}

	private int CalculateSurplus()
	{
		int num = haulCurrent - haulGoal;
		return Mathf.Max(0, num);
	}

	private void SurplusLightLogic()
	{
		if (!surplusLightActive)
		{
			return;
		}
		if (surplusLightTimer > 0f)
		{
			surplusLightTimer -= Time.deltaTime;
			if (surplusLightTimer <= 0f)
			{
				surplusLightOutroSound.Play(base.transform.position);
			}
			return;
		}
		surplusLightLerp += 2f * Time.deltaTime;
		surplusLight.intensity = Mathf.Lerp(0f, surplusLightIntensity, surplusLightOutro.Evaluate(surplusLightLerp));
		if (surplusLightLerp >= 1f)
		{
			surplusLight.intensity = 0f;
			surplusLight.range = 0f;
			surplusLightActive = false;
		}
	}

	[PunRPC]
	public void ExtractionPointSurplusRPC(int surplus, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			RoundDirector.instance.extractionPointSurplus = surplus;
		}
	}

	private void StateSet(State newState)
	{
		if (settingState)
		{
			return;
		}
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient() && stateSetTo == State.None)
			{
				settingState = true;
				photonView.RPC("StateSetRPC", RpcTarget.All, newState);
			}
		}
		else if (stateSetTo == State.None)
		{
			settingState = true;
			StateSetRPC(newState);
		}
	}

	[PunRPC]
	public void StateSetRPC(State state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			stateSetTo = state;
			stateTimer = 0f;
			stateEnd = true;
		}
	}

	private bool StateIs(State state)
	{
		return currentState == state;
	}

	private void DestroyTheFirstPhysObjectsInHaulList()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && RoundDirector.instance.dollarHaulList.Count != 0 && (bool)RoundDirector.instance.dollarHaulList[0] && (bool)RoundDirector.instance.dollarHaulList[0].GetComponent<PhysGrabObject>())
		{
			RoundDirector.instance.totalHaul += (int)RoundDirector.instance.dollarHaulList[0].GetComponent<ValuableObject>().dollarValueCurrent;
			RoundDirector.instance.dollarHaulList[0].GetComponent<PhysGrabObject>().DestroyPhysGrabObject();
			RoundDirector.instance.dollarHaulList.RemoveAt(0);
		}
	}

	private void DestroyTheFirstPhysObjectsInShopList()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || ShopManager.instance.shoppingList.Count == 0)
		{
			return;
		}
		ItemAttributes itemAttributes = ShopManager.instance.shoppingList[0];
		if ((bool)itemAttributes && (bool)itemAttributes.GetComponent<PhysGrabObject>() && SemiFunc.StatGetRunCurrency() - itemAttributes.value >= 0)
		{
			SemiFunc.StatSetRunCurrency(SemiFunc.StatGetRunCurrency() - itemAttributes.value);
			StatsManager.instance.ItemPurchase(itemAttributes.item.name);
			if (itemAttributes.item.itemType == SemiFunc.itemType.item_upgrade)
			{
				StatsManager.instance.AddItemsUpgradesPurchased(itemAttributes.item.name);
			}
			if (itemAttributes.item.itemType == SemiFunc.itemType.power_crystal)
			{
				StatsManager.instance.runStats["chargingStationChargeTotal"] += 17;
				if (StatsManager.instance.runStats["chargingStationChargeTotal"] > 100)
				{
					StatsManager.instance.runStats["chargingStationChargeTotal"] = 100;
				}
				Debug.Log("Charging station charge total: " + StatsManager.instance.runStats["chargingStationChargeTotal"]);
			}
			itemAttributes.GetComponent<PhysGrabObject>().DestroyPhysGrabObject();
			ShopManager.instance.shoppingList.RemoveAt(0);
		}
		SemiFunc.ShopUpdateCost();
	}

	private void DestroyAllPhysObjectsInShoppingList()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			player.playerDeathHead.Revive();
		}
		List<ItemAttributes> list = new List<ItemAttributes>();
		foreach (ItemAttributes shopping in ShopManager.instance.shoppingList)
		{
			if (!shopping || !shopping.GetComponent<PhysGrabObject>() || SemiFunc.StatGetRunCurrency() - shopping.value < 0)
			{
				continue;
			}
			SemiFunc.StatSetRunCurrency(SemiFunc.StatGetRunCurrency() - shopping.GetComponent<ItemAttributes>().value);
			StatsManager.instance.ItemPurchase(shopping.item.name);
			if (shopping.item.itemType == SemiFunc.itemType.item_upgrade)
			{
				StatsManager.instance.AddItemsUpgradesPurchased(shopping.item.name);
			}
			if (shopping.item.itemType == SemiFunc.itemType.power_crystal)
			{
				StatsManager.instance.runStats["chargingStationChargeTotal"] += 17;
				if (StatsManager.instance.runStats["chargingStationChargeTotal"] > 100)
				{
					StatsManager.instance.runStats["chargingStationChargeTotal"] = 100;
				}
			}
			shopping.GetComponent<PhysGrabObject>().DestroyPhysGrabObject();
			list.Add(shopping);
		}
		foreach (ItemAttributes item in list)
		{
			ShopManager.instance.shoppingList.Remove(item);
		}
		SemiFunc.ShopUpdateCost();
	}

	private void DestroyAllPhysObjectsInHaulList()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			player.playerDeathHead.Revive();
		}
		foreach (GameObject dollarHaul in RoundDirector.instance.dollarHaulList)
		{
			if ((bool)dollarHaul && (bool)dollarHaul.GetComponent<PhysGrabObject>())
			{
				RoundDirector.instance.totalHaul += (int)dollarHaul.GetComponent<ValuableObject>().dollarValueCurrent;
				dollarHaul.GetComponent<PhysGrabObject>().DestroyPhysGrabObject();
			}
		}
	}

	private void TubeScreenTextChange(string text, Color color)
	{
		if (tubeScreenTextString != text)
		{
			soundActivate2.Play(base.transform.position);
		}
		tubeScreenTextString = text;
		tubeScreenChangeTimer = 0.2f;
		tubeScreenText.color = Color.white;
		tubeScreenLight.color = Color.white;
		tubeScreenTextColor = color;
	}

	private void TubeScreenTextChangeLogic()
	{
		if (tubeScreenChangeTimer > 0f)
		{
			tubeScreenChangeTimer -= Time.deltaTime;
			if (thirtyFPSUpdate)
			{
				tubeScreenText.text = GlitchyText();
			}
			if (tubeScreenChangeTimer <= 0f)
			{
				tubeScreenText.text = tubeScreenTextString;
				tubeScreenText.color = tubeScreenTextColor;
				tubeScreenLight.color = tubeScreenTextColor;
			}
		}
	}

	private void ButtonToggle(bool _active)
	{
		if (buttonActive != _active)
		{
			if (buttonDenyCooldown > 0f)
			{
				buttonDenyCooldown = 0f;
			}
			if (_active)
			{
				button.material = buttonOriginalMaterial;
				buttonGrabObject.enabled = true;
				buttonLight.enabled = true;
			}
			else
			{
				button.material = buttonOff;
				buttonGrabObject.enabled = true;
				buttonLight.enabled = false;
			}
			if (currentState != State.Idle)
			{
				buttonGrabObject.enabled = false;
			}
			buttonActive = _active;
		}
	}

	[PunRPC]
	private void ButtonDenyRPC()
	{
		buttonDenyActive = true;
		buttonDenyLerp = 0f;
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, emojiScreen.transform.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, emojiScreen.transform.position, 0.1f);
		soundCancel.Play(base.transform.position);
		buttonLight.enabled = true;
		button.material = buttonDenyMaterial;
		buttonGrabObject.enabled = false;
		buttonDenyCooldown = 1f;
		PlayerAvatar playerAvatarScript = PlayerController.instance.playerAvatarScript;
		if (!playerAvatarScript.isDisabled && Vector3.Distance(playerAvatarScript.transform.position, base.transform.position) < 10f && TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialOnlyOneExtraction, 1))
		{
			TutorialDirector.instance.ActivateTip("Only One Extraction", 2f, _interrupt: false);
		}
	}
}
