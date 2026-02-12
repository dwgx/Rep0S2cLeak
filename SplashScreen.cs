using UnityEngine;

public class SplashScreen : MonoBehaviour
{
	private enum State
	{
		Wait,
		Semiwork,
		Warning,
		Done
	}

	public static SplashScreen instance;

	private State state;

	private float stateTimer;

	private bool stateImpulse;

	[Space]
	public AnimationCurve semiworkCurveIntro;

	private float semiworkCurveLerp;

	[Space]
	public AnimationCurve warningCurveIntro;

	private float warningCurveLerp;

	[Space]
	public Sound warningSound;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		StateSet(State.Wait);
	}

	private void Update()
	{
		switch (state)
		{
		case State.Wait:
			StateWait();
			break;
		case State.Semiwork:
			StateSemiwork();
			break;
		case State.Warning:
			StateWarning();
			break;
		case State.Done:
			StateDone();
			break;
		}
		SkipLogic();
		SemiworkAnimation();
		WarningAnimation();
	}

	private void StateWait()
	{
		VideoOverlay.Instance.Override(0.1f, 1f, 20f);
		if (GameDirector.instance.currentState == GameDirector.gameState.Main)
		{
			StateSet(State.Semiwork);
		}
	}

	private void StateSemiwork()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 2f;
			SplashScreenUI.instance.semiworkTransform.gameObject.SetActive(value: true);
		}
		VideoOverlay.Instance.Override(0.1f, 0.02f, 2f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			SplashScreenUI.instance.semiworkTransform.gameObject.SetActive(value: false);
			StateSet(State.Warning);
		}
	}

	private void StateWarning()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			stateTimer = 6f;
			SplashScreenUI.instance.warningTransform.gameObject.SetActive(value: true);
			warningSound.Play(base.transform.position);
		}
		VideoOverlay.Instance.Override(0.1f, 0.02f, 5f);
		stateTimer -= Time.deltaTime;
		if (stateTimer <= 0f)
		{
			StateSet(State.Done);
		}
	}

	private void StateDone()
	{
		if (stateImpulse)
		{
			stateImpulse = false;
			if (DataDirector.instance.SettingValueFetch(DataDirector.Setting.SplashScreenCount) == 0)
			{
				DataDirector.instance.SettingValueSet(DataDirector.Setting.ItemUnequipAutoHold, 0);
			}
			DataDirector.instance.SettingValueSet(DataDirector.Setting.SplashScreenCount, 1);
			SplashScreenUI.instance.gameObject.SetActive(value: false);
			if (SteamManager.instance.lobbyIdToAutoJoin != 0L)
			{
				SteamManager.instance.StartAutoJoiningLobby();
				return;
			}
			RunManager.instance.skipLoadingUI = true;
			GameDirector.instance.OutroStart();
			NetworkManager.instance.leavePhotonRoom = true;
		}
	}

	private void StateSet(State _state)
	{
		if (state != _state)
		{
			state = _state;
			stateTimer = 0f;
			stateImpulse = true;
		}
	}

	private void SemiworkAnimation()
	{
		if (state == State.Semiwork)
		{
			semiworkCurveLerp += Time.deltaTime * 5f;
			semiworkCurveLerp = Mathf.Clamp01(semiworkCurveLerp);
			SplashScreenUI.instance.semiworkTransform.anchoredPosition = Vector2.LerpUnclamped(Vector2.zero, Vector2.up * 10f, semiworkCurveIntro.Evaluate(semiworkCurveLerp));
		}
	}

	private void WarningAnimation()
	{
		if (state == State.Warning)
		{
			warningCurveLerp += Time.deltaTime * 5f;
			warningCurveLerp = Mathf.Clamp01(warningCurveLerp);
			SplashScreenUI.instance.warningTransform.anchoredPosition = Vector2.LerpUnclamped(Vector2.zero, Vector2.up * 5f, warningCurveIntro.Evaluate(warningCurveLerp));
		}
	}

	private void SkipLogic()
	{
		if (state > State.Wait && DataDirector.instance.SettingValueFetch(DataDirector.Setting.SplashScreenCount) != 0 && Input.anyKeyDown)
		{
			StateSet(State.Done);
		}
	}
}
