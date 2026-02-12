using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
	public static LoadingUI instance;

	public Image fadeImage;

	public Image fadeBehindImage;

	[Space]
	public TextMeshProUGUI levelNumberText;

	public TextMeshProUGUI levelNameText;

	[Space]
	public Image loadingGraphic01;

	public Image loadingGraphic02;

	public Image loadingGraphic03;

	private Animator animator;

	internal bool levelAnimationStarted;

	internal bool levelAnimationCompleted;

	internal bool debugDisableLevelAnimation;

	[Space]
	public Sound soundTurn;

	public Sound soundRevUp;

	public Sound soundCrash;

	public Sound soundtextLevel;

	public Sound soundtextName;

	[Space]
	public RectTransform stuckTransform;

	public TextMeshProUGUI stuckText;

	public Vector2 stuckPositionStart;

	public Vector2 stuckPositionEnd;

	public AnimationCurve stuckCurve;

	public AnimationCurve stuckCurveOutro;

	private float stuckLerp;

	internal bool stuckActive;

	private void Awake()
	{
		instance = this;
		fadeBehindImage.gameObject.SetActive(value: false);
		animator = GetComponent<Animator>();
		animator.enabled = false;
		animator.keepAnimatorStateOnDisable = true;
		if ((bool)RunManager.instance)
		{
			fadeImage.color = RunManager.instance.loadingFadeColor;
			animator.Play("Idle", 0, RunManager.instance.loadingAnimationTime);
		}
	}

	private void Start()
	{
		stuckText.text = InputManager.instance.InputDisplayReplaceTags("<color=white>press</color> [menu] <color=white>to exit to main menu</color>");
		stuckTransform.anchoredPosition = stuckPositionStart;
	}

	private void Update()
	{
		if (stuckActive)
		{
			stuckLerp += 2f * Time.deltaTime;
			stuckLerp = Mathf.Clamp01(stuckLerp);
			stuckTransform.anchoredPosition = Vector2.LerpUnclamped(stuckPositionStart, stuckPositionEnd, stuckCurve.Evaluate(stuckLerp));
		}
		else
		{
			stuckLerp -= 2f * Time.deltaTime;
			stuckLerp = Mathf.Clamp01(stuckLerp);
			stuckTransform.anchoredPosition = Vector2.LerpUnclamped(stuckPositionStart, stuckPositionEnd, stuckCurveOutro.Evaluate(stuckLerp));
		}
	}

	private void LateUpdate()
	{
		if (RunManager.instance.skipLoadingUI)
		{
			return;
		}
		float num = Time.deltaTime;
		if (!levelAnimationStarted)
		{
			num = Mathf.Min(num, 0.01f);
		}
		animator.Update(num);
		if (GameDirector.instance.currentState == GameDirector.gameState.Load || GameDirector.instance.currentState == GameDirector.gameState.End || GameDirector.instance.currentState == GameDirector.gameState.EndWait || (GameDirector.instance.currentState == GameDirector.gameState.Start && !levelAnimationCompleted))
		{
			fadeImage.color = Color.Lerp(fadeImage.color, new Color(0f, 0f, 0f, 0f), 5f * num);
		}
		else
		{
			fadeImage.color = Color.Lerp(fadeImage.color, Color.black, 20f * num);
		}
		RunManager.instance.loadingFadeColor = fadeImage.color;
		RunManager.instance.loadingAnimationTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		if (GameDirector.instance.PlayerList.Count <= 0)
		{
			return;
		}
		bool flag = true;
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (!player.levelAnimationCompleted)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			levelAnimationCompleted = true;
		}
	}

	public void StopLoading()
	{
		RunManager.instance.loadingFadeColor = Color.black;
		RunManager.instance.skipLoadingUI = false;
		base.gameObject.SetActive(value: false);
	}

	public void StartLoading()
	{
		levelAnimationStarted = false;
		base.gameObject.SetActive(value: true);
		fadeImage.color = RunManager.instance.loadingFadeColor;
		animator.Play("Idle", 0, RunManager.instance.loadingAnimationTime);
	}

	public void LevelAnimationStart()
	{
		levelAnimationStarted = true;
		if (!RunManager.instance.skipLoadingUI && !debugDisableLevelAnimation && (SemiFunc.RunIsLevel() || SemiFunc.RunIsShop() || SemiFunc.RunIsArena()))
		{
			loadingGraphic01.sprite = LevelGenerator.Instance.Level.LoadingGraphic01;
			if (!LevelGenerator.Instance.Level.LoadingGraphic01)
			{
				loadingGraphic01.color = Color.clear;
			}
			loadingGraphic02.sprite = LevelGenerator.Instance.Level.LoadingGraphic02;
			if (!LevelGenerator.Instance.Level.LoadingGraphic02)
			{
				loadingGraphic02.color = Color.clear;
			}
			loadingGraphic03.sprite = LevelGenerator.Instance.Level.LoadingGraphic03;
			if (!LevelGenerator.Instance.Level.LoadingGraphic03)
			{
				loadingGraphic03.color = Color.clear;
			}
			if (SemiFunc.RunIsShop())
			{
				levelNumberText.text = "SHOP";
			}
			else if (SemiFunc.RunIsArena())
			{
				levelNumberText.text = "GAME OVER";
				levelNumberText.color = Color.red;
			}
			else
			{
				levelNumberText.text = "LEVEL " + (RunManager.instance.levelsCompleted + 1);
			}
			levelNameText.text = LevelGenerator.Instance.Level.NarrativeName.ToUpper();
			animator.SetTrigger("Level");
		}
		else
		{
			levelAnimationCompleted = true;
		}
	}

	public void LevelAnimationComplete()
	{
		PlayerController.instance.playerAvatarScript.LoadingLevelAnimationCompleted();
	}

	public void PlayTurn()
	{
		soundTurn.Play(base.transform.position);
	}

	public void PlayRevUp()
	{
		soundRevUp.Play(base.transform.position);
	}

	public void PlayCrash()
	{
		soundCrash.Play(base.transform.position);
	}

	public void PlayTextLevel()
	{
		soundtextLevel.Play(base.transform.position);
	}

	public void PlayTextName()
	{
		soundtextName.Play(base.transform.position);
	}
}
