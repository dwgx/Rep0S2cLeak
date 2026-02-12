using UnityEngine;
using UnityEngine.UI;

public class TumbleUI : MonoBehaviour
{
	public static TumbleUI instance;

	private CanvasGroup canvasGroup;

	private bool active;

	private bool activePrevious = true;

	internal bool canExit;

	private bool canExitPrevious;

	private bool animating;

	private float animationLerp;

	public Color canNotExitColor1;

	public Color canNotExitColor2;

	public Color canExitColor1;

	public Color canExitColor2;

	[Space]
	public AnimationCurve introCurve;

	public float introSpeed;

	[Space]
	public AnimationCurve outroCurve;

	public float outroSpeed;

	[Space]
	public float updateTime;

	private float updateTimer;

	[Space]
	public GameObject[] parts1;

	public GameObject[] parts2;

	private Image[] images1;

	private Image[] images2;

	[Space]
	public Sound canExitSound;

	private float hideTimer;

	private float hideAlpha;

	private void Awake()
	{
		instance = this;
		canvasGroup = GetComponent<CanvasGroup>();
	}

	private void Start()
	{
		images1 = new Image[parts1.Length];
		int num = 0;
		GameObject[] array = parts1;
		foreach (GameObject gameObject in array)
		{
			images1[num] = gameObject.GetComponent<Image>();
			num++;
		}
		images2 = new Image[parts2.Length];
		num = 0;
		array = parts2;
		foreach (GameObject gameObject2 in array)
		{
			images2[num] = gameObject2.GetComponent<Image>();
			num++;
		}
	}

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (PlayerController.instance.playerAvatarScript.isTumbling && !PlayerController.instance.playerAvatarScript.isDisabled)
		{
			active = true;
		}
		else
		{
			active = false;
		}
		if (active != activePrevious)
		{
			activePrevious = active;
			animationLerp = 0f;
			updateTimer = 0f;
			animating = true;
		}
		canExit = true;
		if (active && (PlayerController.instance.playerAvatarScript.tumble.tumbleOverride || PlayerController.instance.tumbleInputDisableTimer > 0f))
		{
			canExit = false;
		}
		if (canExit != canExitPrevious)
		{
			canExitPrevious = canExit;
			if (canExit)
			{
				canExitSound.Play(base.transform.position);
				animating = true;
				animationLerp = 0.5f;
				updateTimer = 0f;
				Image[] array = images1;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].color = canExitColor1;
				}
				array = images2;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].color = canExitColor2;
				}
			}
			else
			{
				Image[] array = images1;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].color = canNotExitColor2;
				}
				array = images2;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].color = canNotExitColor1;
				}
			}
		}
		if (animating)
		{
			if (updateTimer <= 0f)
			{
				if (active)
				{
					if (animationLerp == 0f)
					{
						GameObject[] array2 = parts1;
						for (int i = 0; i < array2.Length; i++)
						{
							array2[i].SetActive(value: true);
						}
						array2 = parts2;
						for (int i = 0; i < array2.Length; i++)
						{
							array2[i].SetActive(value: true);
						}
					}
					animationLerp += Time.deltaTime * introSpeed;
					updateTimer = updateTime;
				}
				else
				{
					animationLerp += Time.deltaTime * outroSpeed;
					updateTimer = updateTime;
					if (animationLerp >= 1f)
					{
						GameObject[] array2 = parts1;
						for (int i = 0; i < array2.Length; i++)
						{
							array2[i].SetActive(value: false);
						}
						array2 = parts2;
						for (int i = 0; i < array2.Length; i++)
						{
							array2[i].SetActive(value: false);
						}
					}
				}
			}
			else
			{
				updateTimer -= Time.deltaTime;
			}
		}
		if (animating)
		{
			if (active)
			{
				base.transform.localScale = Vector3.LerpUnclamped(Vector3.one * 1.25f, Vector3.one, introCurve.Evaluate(animationLerp));
			}
			else
			{
				base.transform.localScale = Vector3.LerpUnclamped(Vector3.one, Vector3.one * 1.25f, outroCurve.Evaluate(animationLerp));
			}
			if (animationLerp >= 1f)
			{
				animating = false;
			}
		}
		float num = 1f;
		if (hideTimer > 0f)
		{
			num = 0f;
			hideTimer -= Time.deltaTime;
		}
		hideAlpha = Mathf.Lerp(hideAlpha, num, Time.deltaTime * 20f);
		canvasGroup.alpha = hideAlpha;
	}

	public void Hide()
	{
		hideTimer = 0.1f;
	}
}
