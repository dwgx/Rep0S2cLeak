using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewindEffect : MonoBehaviour
{
	public static RewindEffect Instance;

	public Timecode Timecode;

	[HideInInspector]
	public List<Timecode.TimeSnapshot> TimeSnapshots;

	[Space]
	public int maxScreenshots = 50;

	public float movementThreshold = 3f;

	public Transform PlayerTransfrom;

	private List<Texture2D> screenshots = new List<Texture2D>();

	public GameObject RewindEffectUI;

	private Vector3 lastScreenshotPosition;

	[HideInInspector]
	public bool PlayRewind;

	public float rewindDuration = 1.5f;

	private bool RewindEnd;

	private bool FirstStep = true;

	public GameObject RewindLines;

	public RawImage RenderTextureMain;

	public Sound RewindStartSound;

	public Sound RewindEndSound;

	public Sound RewindLoopSound;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		RewindEffectUI.SetActive(value: false);
		RewindLines.SetActive(value: false);
	}

	private void Update()
	{
		if (PlayRewind)
		{
			GameDirector.instance.SetDisableInput(0.5f);
			VideoOverlay.Instance.Override(0.1f, 1f, 5f);
		}
		RewindLoopSound.PlayLoop(PlayRewind, 0.9f, 0.9f);
		if (!PlayRewind && !RewindEnd && Vector3.Distance(PlayerTransfrom.position, lastScreenshotPosition) > movementThreshold)
		{
			if (!FirstStep)
			{
				CaptureScreenshot();
				lastScreenshotPosition = PlayerTransfrom.position;
			}
			else
			{
				ClearScreenshots();
				FirstStep = false;
			}
		}
	}

	private void CaptureScreenshot()
	{
		if (screenshots.Count >= maxScreenshots)
		{
			Object.Destroy(screenshots[0]);
			screenshots.RemoveAt(0);
			TimeSnapshots.RemoveAt(0);
		}
		Texture texture = RenderTextureMain.texture;
		RenderTexture renderTexture = new RenderTexture(128, 72, 24);
		renderTexture.Create();
		Graphics.Blit(texture, renderTexture);
		RenderTexture.active = renderTexture;
		Texture2D texture2D = new Texture2D(128, 72, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, 128f, 72f), 0, 0);
		texture2D.Apply();
		RenderTexture.active = null;
		screenshots.Add(texture2D);
		TimeSnapshots.Add(Timecode.GetSnapshot());
		Object.Destroy(renderTexture);
	}

	public void PlayRewindEffect()
	{
		if (screenshots.Count >= 10)
		{
			RewindStartSound.Play(base.transform.position);
			PlayRewind = true;
			RewindLines.SetActive(value: true);
			RewindEffectUI.SetActive(value: true);
			StartCoroutine(RewindCoroutine());
		}
		else
		{
			RewindEnding();
		}
	}

	private IEnumerator RewindCoroutine()
	{
		Image rewindImage = RewindEffectUI.GetComponent<Image>();
		rewindImage.enabled = true;
		float displayTimePerScreenshot = rewindDuration / (float)screenshots.Count;
		for (int i = screenshots.Count - 1; i >= 0; i--)
		{
			Timecode.SetTime(TimeSnapshots[i]);
			Texture2D screenshot = screenshots[i];
			Sprite sprite = (rewindImage.sprite = Sprite.Create(screenshot, new Rect(0f, 0f, screenshot.width, screenshot.height), new Vector2(0.5f, 0.5f)));
			yield return new WaitForSeconds(displayTimePerScreenshot);
			Object.Destroy(sprite);
			Object.Destroy(screenshot);
		}
		RewindEnding();
	}

	public void ClearScreenshots()
	{
		foreach (Texture2D screenshot in screenshots)
		{
			Object.Destroy(screenshot);
		}
		screenshots.Clear();
		lastScreenshotPosition = PlayerTransfrom.position;
	}

	public void RewindEnding()
	{
		Timecode.SetToStartSnapshot();
		RewindEndSound.Play(base.transform.position);
		PlayRewind = false;
		ClearScreenshots();
		RewindLines.SetActive(value: false);
		lastScreenshotPosition = PlayerTransfrom.position;
		FirstStep = true;
	}
}
