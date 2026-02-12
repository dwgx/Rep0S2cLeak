using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderTextureMain : MonoBehaviour
{
	public static RenderTextureMain instance;

	private List<Camera> cameras = new List<Camera>();

	public RenderTexture renderTexture;

	[Space]
	public float textureWidthSmall;

	public float textureHeightSmall;

	[Space]
	public float textureWidthMedium;

	public float textureHeightMedium;

	[Space]
	public float textureWidthLarge;

	public float textureHeightLarge;

	internal float textureWidthOriginal;

	internal float textureHeightOriginal;

	internal float textureWidth;

	internal float textureHeight;

	internal float textureResetTimer;

	internal float sizeResetTimer;

	[Space]
	public AnimationCurve shakeCurve;

	private float shakeTimer;

	private bool shakeActive;

	private float shakeX;

	private float shakeXOld;

	private float shakeXNew;

	private float shakeXLerp = 1f;

	private float shakeY;

	private float shakeYOld;

	private float shakeYNew;

	private float shakeYLerp = 1f;

	private Vector3 originalSize;

	public RawImage overlayRawImage;

	private float overlayDisableTimer;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		textureWidthOriginal = textureWidthSmall;
		textureHeightOriginal = textureHeightSmall;
		originalSize = base.transform.localScale;
		Camera[] componentsInChildren = Camera.main.GetComponentsInChildren<Camera>();
		foreach (Camera item in componentsInChildren)
		{
			cameras.Add(item);
		}
		ResetResolution();
	}

	private void Update()
	{
		if (shakeActive)
		{
			if (shakeXLerp >= 1f)
			{
				shakeXLerp = 0f;
				shakeXOld = shakeXNew;
				shakeXNew = Random.Range(-5f, 5f);
			}
			else
			{
				shakeXLerp += Time.deltaTime * 100f;
				shakeX = Mathf.Lerp(shakeXOld, shakeXNew, shakeCurve.Evaluate(shakeXLerp));
			}
			if (shakeYLerp >= 1f)
			{
				shakeYLerp = 0f;
				shakeYOld = shakeYNew;
				shakeYNew = Random.Range(-5f, 5f);
			}
			else
			{
				shakeYLerp += Time.deltaTime * 100f;
				shakeY = Mathf.Lerp(shakeYOld, shakeYNew, shakeCurve.Evaluate(shakeYLerp));
			}
			base.transform.localPosition = new Vector3(shakeX, shakeY, 0f);
			shakeTimer -= Time.deltaTime;
			if (shakeTimer <= 0f)
			{
				base.transform.localPosition = new Vector3(0f, 0f, 0f);
				shakeActive = false;
			}
		}
		if (sizeResetTimer > 0f)
		{
			sizeResetTimer -= Time.deltaTime;
		}
		else if (base.transform.localScale != originalSize)
		{
			base.transform.localScale = originalSize;
		}
		if (textureResetTimer > 0f)
		{
			textureResetTimer -= Time.deltaTime;
		}
		else if (renderTexture.width != (int)textureWidthOriginal || renderTexture.height != (int)textureHeightOriginal)
		{
			ResetResolution();
		}
		if (overlayDisableTimer > 0f)
		{
			overlayDisableTimer -= Time.deltaTime;
			if (overlayDisableTimer <= 0f)
			{
				overlayRawImage.enabled = true;
			}
		}
	}

	public void Shake(float _time)
	{
		shakeActive = true;
		shakeTimer = _time;
	}

	public void ChangeSize(float _width, float _height, float _time)
	{
		base.transform.localScale = new Vector3(_width, _height, 1f);
		sizeResetTimer = _time;
	}

	public void ChangeResolution(float _width, float _height, float _time)
	{
		textureWidth = _width;
		textureHeight = _height;
		SetRenderTexture();
		textureResetTimer = _time;
	}

	public void ResetResolution()
	{
		textureWidth = textureWidthOriginal;
		textureHeight = textureHeightOriginal;
		SetRenderTexture();
	}

	private void SetRenderTexture()
	{
		renderTexture.Release();
		renderTexture.width = (int)textureWidth;
		renderTexture.height = (int)textureHeight;
		renderTexture.Create();
		if (cameras.Count > 0)
		{
			cameras[0].targetTexture = renderTexture;
		}
		foreach (Camera camera in cameras)
		{
			camera.enabled = false;
			camera.enabled = true;
		}
	}

	private void OnApplicationQuit()
	{
		textureWidth = textureWidthSmall;
		textureHeight = textureHeightSmall;
		SetRenderTexture();
	}

	public void OverlayDisable()
	{
		overlayRawImage.enabled = false;
		overlayDisableTimer = 0.5f;
	}
}
