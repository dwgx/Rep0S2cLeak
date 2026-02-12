using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameDirector : MonoBehaviour
{
	public enum gameState
	{
		Load,
		Start,
		Main,
		Outro,
		End,
		EndWait,
		Death
	}

	public static GameDirector instance;

	public gameState currentState = gameState.Start;

	public bool LevelCompleted;

	public bool LevelCompletedDone;

	[Header("Debug")]
	public float TimeScale = 1f;

	[Space(15f)]
	public bool RandomSeed;

	public int Seed;

	[Space(15f)]
	[Header("Audio")]
	public AudioMixerSnapshot volumeOff;

	public AudioMixerSnapshot volumeOn;

	public AudioMixerSnapshot volumeCutsceneOnly;

	public Sound SoundIntro;

	public Sound SoundIntroRun;

	public Sound SoundOutro;

	public Sound SoundOutroRun;

	public Sound SoundDeath;

	[Space(15f)]
	[Header("Enemy")]
	public bool LevelEnemyChasing;

	[Space(15f)]
	[Header("Other")]
	public Camera MainCamera;

	[HideInInspector]
	public Transform MainCameraParent;

	public RenderTexture MainRenderTexture;

	[Space(15f)]
	public AnimNoise camNoise;

	private float gameStateTimer;

	private bool gameStateStartImpulse = true;

	[Space(15f)]
	public GameObject cameraPosition;

	public Animator cameraTargetAnimator;

	public CameraShake CameraShake;

	public CameraShake CameraImpact;

	public CameraBob CameraBob;

	[Space(15f)]
	public int InitialCleaningSpots;

	public List<PlayerAvatar> PlayerList = new List<PlayerAvatar>();

	internal List<PlayerDeathSpot> PlayerDeathSpots = new List<PlayerDeathSpot>();

	public bool DisableInput;

	private float DisableInputTimer;

	private float disableEscMenuTimer;

	internal bool outroStart;

	private float deathFreezeTime = 0.2f;

	private float deathFreezeTimer;

	private float timer1FPS;

	private float timer5FPS;

	private float timer15FPS;

	private float timer20FPS;

	private float timer30FPS;

	private float timer60FPS;

	private const float INTERVAL_1FPS = 1f;

	private const float INTERVAL_5FPS = 0.2f;

	private const float INTERVAL_15FPS = 1f / 15f;

	private const float INTERVAL_20FPS = 0.05f;

	private const float INTERVAL_30FPS = 1f / 30f;

	private const float INTERVAL_60FPS = 1f / 60f;

	internal bool fpsImpulse1;

	internal bool fpsImpulse5;

	internal bool fpsImpulse15;

	internal bool fpsImpulse20;

	internal bool fpsImpulse30;

	internal bool fpsImpulse60;

	internal bool greenScreenActive;

	public GameObject greenScreenPrefab;

	private void Awake()
	{
		MainCamera = Camera.main;
		MainCameraParent = MainCamera.transform.parent;
		instance = this;
		currentState = gameState.Load;
	}

	private void Start()
	{
		RunManager.instance.runStarted = true;
		RunManager.instance.allPlayersDead = false;
	}

	private void gameStateLoad()
	{
		if (gameStateStartImpulse)
		{
			cameraPosition.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);
			AudioManager.instance.SetSoundSnapshot(AudioManager.SoundSnapshot.Off, 0f);
			gameStateStartImpulse = false;
		}
	}

	private void gameStateStart()
	{
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (gameStateStartImpulse)
		{
			AudioManager.instance.SetSoundSnapshot(AudioManager.SoundSnapshot.CutsceneOnly, 0.1f);
			gameStateTimer = 0.5f;
			LoadingUI.instance.LevelAnimationStart();
			gameStateStartImpulse = false;
			return;
		}
		if (SemiFunc.RunIsLevel() || SemiFunc.RunIsTutorial() || SemiFunc.RunIsShop() || SemiFunc.RunIsArena())
		{
			if (LoadingUI.instance.levelAnimationCompleted)
			{
				gameStateTimer -= Time.deltaTime;
			}
		}
		else
		{
			gameStateTimer -= Time.deltaTime;
		}
		if (gameStateTimer <= 0f)
		{
			LoadingUI.instance.StopLoading();
			if (SemiFunc.RunIsLobbyMenu())
			{
				AudioManager.instance.SetSoundSnapshot(AudioManager.SoundSnapshot.Spectate, 0.1f);
			}
			else
			{
				AudioManager.instance.SetSoundSnapshot(AudioManager.SoundSnapshot.On, 0.1f);
			}
			MusicManager.Instance.MusicMixerOff.TransitionTo(0f);
			MusicManager.Instance.MusicMixerOn.TransitionTo(0.1f);
			SoundIntro.Play(base.transform.position);
			if (!SemiFunc.MenuLevel())
			{
				SoundIntroRun.Play(base.transform.position);
			}
			MoonUI.instance.Check();
			currentState = gameState.Main;
			gameStateStartImpulse = true;
			gameStateTimer = 0f;
		}
	}

	private void gameStateMain()
	{
	}

	private void gameStateOutro()
	{
		if (gameStateStartImpulse)
		{
			AudioManager.instance.SetSoundSnapshot(AudioManager.SoundSnapshot.CutsceneOnly, 0.25f);
			MusicManager.Instance.MusicMixerScareOnly.TransitionTo(0.25f);
			SoundOutro.Play(base.transform.position);
			if (!SemiFunc.MenuLevel())
			{
				SoundOutroRun.Play(base.transform.position);
			}
			gameStateTimer = 1f;
			gameStateStartImpulse = false;
			HUD.instance.Hide();
			{
				foreach (PlayerAvatar player in PlayerList)
				{
					if ((bool)player.voiceChat)
					{
						player.voiceChat.ToggleMixer(_lobby: true);
					}
				}
				return;
			}
		}
		gameStateTimer -= Time.deltaTime;
		if (gameStateTimer <= 0f)
		{
			currentState = gameState.End;
			gameStateStartImpulse = true;
			gameStateTimer = 0f;
		}
	}

	private void gameStateEnd()
	{
		if (gameStateStartImpulse)
		{
			AudioManager.instance.SetSoundSnapshot(AudioManager.SoundSnapshot.Off, 0.5f);
			PlayerController.instance.playerAvatarScript.SetDisabled();
			LoadingUI.instance.StartLoading();
			gameStateTimer = 0.5f;
			gameStateStartImpulse = false;
		}
		else
		{
			gameStateTimer -= Time.deltaTime;
			if (gameStateTimer <= 0f)
			{
				PlayerController.instance.playerAvatarScript.OutroDone();
				currentState = gameState.EndWait;
			}
		}
	}

	private void gameStateDeath()
	{
		SemiFunc.UIShowSpectate();
		SemiFunc.UIHideHealth();
		SemiFunc.UIHideOvercharge();
		SemiFunc.UIHideEnergy();
		SemiFunc.UIHideInventory();
		SemiFunc.UIHideAim();
		if (gameStateStartImpulse)
		{
			gameStateTimer = 0.5f;
			deathFreezeTimer = deathFreezeTime;
			SoundDeath.Play(base.transform.position);
			HUD.instance.Hide();
			RenderTextureMain.instance.ChangeResolution(RenderTextureMain.instance.textureWidthOriginal * 0.2f, RenderTextureMain.instance.textureHeightOriginal * 0.2f, gameStateTimer);
			gameStateStartImpulse = false;
			return;
		}
		if (deathFreezeTimer > 0f)
		{
			deathFreezeTimer -= Time.deltaTime;
			if (deathFreezeTimer <= 0f)
			{
				AudioManager.instance.SetSoundSnapshot(AudioManager.SoundSnapshot.CutsceneOnly, 0.1f);
				RenderTextureMain.instance.Shake(gameStateTimer);
				RenderTextureMain.instance.ChangeSize(1.25f, 1.25f, gameStateTimer);
				CameraFreeze.Freeze(gameStateTimer + 0.1f);
			}
		}
		gameStateTimer -= Time.deltaTime;
		if (gameStateTimer <= 0f)
		{
			AudioManager.instance.SetSoundSnapshot(AudioManager.SoundSnapshot.Spectate, 0.1f);
			HUD.instance.Show();
			RenderTextureMain.instance.Shake(0f);
			RenderTextureMain.instance.sizeResetTimer = 0f;
			RenderTextureMain.instance.textureResetTimer = 0f;
			CameraFreeze.Freeze(0f);
			PlayerController.instance.playerAvatarScript.SetSpectate();
			currentState = gameState.Main;
		}
	}

	private void Update()
	{
		if (SemiFunc.InputDown(InputKey.Menu) && !SemiFunc.MenuLevel() && !ChatManager.instance.chatActive && disableEscMenuTimer <= 0f)
		{
			if (SemiFunc.InputDown(InputKey.Back) && ChatManager.instance.StateIsActive())
			{
				return;
			}
			if (!MenuManager.instance.currentMenuPage)
			{
				MenuManager.instance.PageOpen(MenuPageIndex.Escape);
			}
			else if (MenuManager.instance.currentMenuPage.menuPageIndex == MenuPageIndex.Escape)
			{
				MenuManager.instance.PageCloseAll();
			}
		}
		disableEscMenuTimer -= Time.deltaTime;
		if (outroStart)
		{
			OutroStart();
		}
		if (currentState == gameState.Load)
		{
			gameStateLoad();
		}
		else if (currentState == gameState.Start)
		{
			gameStateStart();
		}
		else if (currentState == gameState.Main)
		{
			gameStateMain();
		}
		else if (currentState == gameState.Outro)
		{
			gameStateOutro();
		}
		else if (currentState == gameState.End)
		{
			gameStateEnd();
		}
		else if (currentState == gameState.Death)
		{
			gameStateDeath();
		}
		if (DisableInput)
		{
			DisableInputTimer -= Time.deltaTime;
			if (DisableInputTimer <= 0f)
			{
				DisableInput = false;
			}
		}
	}

	public void OutroStart()
	{
		outroStart = true;
		if (currentState == gameState.Main)
		{
			currentState = gameState.Outro;
			if ((bool)FadeOverlay.Instance)
			{
				FadeOverlay.Instance.Image.color = Color.black;
			}
			gameStateStartImpulse = true;
			gameStateTimer = 0f;
		}
	}

	public void DeathStart()
	{
		currentState = gameState.Death;
		gameStateStartImpulse = true;
		gameStateTimer = 0f;
	}

	public void Revive()
	{
		currentState = gameState.Main;
		gameStateStartImpulse = true;
		gameStateTimer = 0f;
	}

	public void CommandSetFPS(int _fps)
	{
		Application.targetFrameRate = _fps;
	}

	public void CommandRecordingDirectorToggle()
	{
		if (RecordingDirector.instance != null)
		{
			Object.Destroy(RecordingDirector.instance.gameObject);
			FlashlightController.Instance.hideFlashlight = false;
		}
		else
		{
			Object.Instantiate(Resources.Load("Recording Director"));
		}
	}

	public void CommandGreenScreenToggle()
	{
		if (greenScreenActive)
		{
			Object.Destroy(VideoGreenScreen.instance.gameObject);
			HurtVignette.instance.gameObject.SetActive(value: true);
			greenScreenActive = false;
		}
		else
		{
			Object.Instantiate(greenScreenPrefab);
			HurtVignette.instance.gameObject.SetActive(value: false);
			greenScreenActive = true;
		}
	}

	public void SetDisableInput(float time)
	{
		DisableInput = true;
		DisableInputTimer = time;
	}

	public void SetDisableEscMenu(float _time)
	{
		disableEscMenuTimer = _time;
	}

	public void SetStart()
	{
		if (currentState < gameState.Outro)
		{
			gameStateStartImpulse = true;
			currentState = gameState.Start;
		}
	}

	private void LateUpdate()
	{
		FPSImpulses();
	}

	private void FPSImpulses()
	{
		float deltaTime = Time.deltaTime;
		fpsImpulse1 = false;
		fpsImpulse5 = false;
		fpsImpulse15 = false;
		fpsImpulse20 = false;
		fpsImpulse30 = false;
		fpsImpulse60 = false;
		timer1FPS += deltaTime;
		timer5FPS += deltaTime;
		timer15FPS += deltaTime;
		timer20FPS += deltaTime;
		timer30FPS += deltaTime;
		timer60FPS += deltaTime;
		while (timer1FPS >= 1f)
		{
			fpsImpulse1 = true;
			timer1FPS -= 1f;
		}
		while (timer5FPS >= 0.2f)
		{
			fpsImpulse5 = true;
			timer5FPS -= 0.2f;
		}
		while (timer15FPS >= 1f / 15f)
		{
			fpsImpulse15 = true;
			timer15FPS -= 1f / 15f;
		}
		while (timer20FPS >= 0.05f)
		{
			fpsImpulse20 = true;
			timer20FPS -= 0.05f;
		}
		while (timer30FPS >= 1f / 30f)
		{
			fpsImpulse30 = true;
			timer30FPS -= 1f / 30f;
		}
		while (timer60FPS >= 1f / 60f)
		{
			fpsImpulse60 = true;
			timer60FPS -= 1f / 60f;
		}
	}
}
