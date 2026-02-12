using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPlayerHead : MonoBehaviour
{
	internal PlayerAvatar playerAvatar;

	public Transform headRight;

	public Transform headLeft;

	private RectTransform eyesTransform;

	private MenuPlayerListed playerListed;

	private int listSpotPrev = -1;

	private int listSpot;

	private bool left;

	private bool right = true;

	private List<RawImage> allRawImagesInChildren = new List<RawImage>();

	private Vector3 eyesStartPosOriginal;

	private Vector3 eyesStartPos;

	private bool isTalkingPrev;

	internal bool isTalking;

	public RectTransform focusPoint;

	public RectTransform myFocusPoint;

	private RectTransform playerListedTransform;

	private RectTransform rectTransform;

	internal RectTransform headTransform;

	internal float startedTalkingAtTime;

	public bool isWinnerHead;

	[Space]
	public RectTransform muteIconTransform;

	public Image muteIcon;

	public AnimationCurve muteIntroCurve;

	public AnimationCurve muteOutroCurve;

	private float muteAnimationLerp;

	private bool muteShow;

	private Vector3 muteScalePrevious;

	private Color playerColor;

	private void Start()
	{
		playerListed = GetComponentInParent<MenuPlayerListed>();
		allRawImagesInChildren.AddRange(GetComponentsInChildren<RawImage>());
		playerListedTransform = playerListed.GetComponent<RectTransform>();
		MenuManager.instance.PlayerHeadAdd(this);
		rectTransform = GetComponent<RectTransform>();
		startedTalkingAtTime = Time.time;
		muteIconTransform.localScale = Vector3.zero;
		muteScalePrevious = muteIconTransform.localScale;
	}

	public void SetColor(Color color)
	{
		playerColor = color;
		foreach (RawImage allRawImagesInChild in allRawImagesInChildren)
		{
			if (allRawImagesInChild.name != "Arena Crown" && allRawImagesInChild.name != "Steam Icon")
			{
				allRawImagesInChild.color = playerColor;
			}
		}
		muteIcon.color = Color.Lerp(playerColor, Color.white, 0.5f);
	}

	private void HeadRight()
	{
		headRight.gameObject.SetActive(value: true);
		headLeft.gameObject.SetActive(value: false);
		eyesTransform = headRight.Find("Eyes").GetComponent<RectTransform>();
		left = false;
		right = true;
		headTransform = headRight.GetComponent<RectTransform>();
	}

	private void HeadLeft()
	{
		headRight.gameObject.SetActive(value: false);
		headLeft.gameObject.SetActive(value: true);
		eyesTransform = headLeft.Find("Eyes").GetComponent<RectTransform>();
		left = true;
		right = false;
		headTransform = headLeft.GetComponent<RectTransform>();
	}

	private void Update()
	{
		if (SemiFunc.IsMultiplayer())
		{
			if ((bool)playerAvatar)
			{
				isTalkingPrev = isTalking;
				if (playerAvatar.voiceChatFetched)
				{
					isTalking = playerAvatar.voiceChat.isTalking;
				}
			}
			else
			{
				playerAvatar = playerListed.playerAvatar;
			}
		}
		if (MenuManager.instance.currentMenuPageIndex == MenuPageIndex.Lobby)
		{
			myFocusPoint.localPosition = MenuCursor.instance.transform.localPosition - rectTransform.parent.parent.localPosition;
			myFocusPoint.localPosition = new Vector3(myFocusPoint.localPosition.x + 18f, myFocusPoint.localPosition.y + 15f, 0f);
		}
		if (playerAvatar.voiceChatFetched && muteShow != playerAvatar.voiceChat.toggleMute)
		{
			muteShow = playerAvatar.voiceChat.toggleMute;
			muteAnimationLerp = 0f;
			muteScalePrevious = muteIconTransform.localScale;
		}
		if (muteShow)
		{
			if (muteAnimationLerp < 1f)
			{
				muteAnimationLerp += Time.deltaTime * 5f;
				muteIconTransform.localScale = Vector3.LerpUnclamped(muteScalePrevious, Vector3.one, muteIntroCurve.Evaluate(muteAnimationLerp));
			}
		}
		else if (muteAnimationLerp < 1f)
		{
			muteAnimationLerp += Time.deltaTime * 10f;
			muteIconTransform.localScale = Vector3.LerpUnclamped(muteScalePrevious, Vector3.zero, muteOutroCurve.Evaluate(muteAnimationLerp));
		}
		if ((bool)playerAvatar.playerDeathHead && playerAvatar.playerDeathHead.spectated)
		{
			Color color = Color.Lerp(playerColor, Color.black, 0.9f);
			foreach (RawImage allRawImagesInChild in allRawImagesInChildren)
			{
				allRawImagesInChild.color = Color.Lerp(allRawImagesInChild.color, color, 10f * Time.deltaTime);
			}
		}
		else
		{
			foreach (RawImage allRawImagesInChild2 in allRawImagesInChildren)
			{
				if (!(allRawImagesInChild2.name == "Steam Icon"))
				{
					if (allRawImagesInChild2.name != "Arena Crown")
					{
						allRawImagesInChild2.color = Color.Lerp(allRawImagesInChild2.color, playerColor, 10f * Time.deltaTime);
					}
					else
					{
						allRawImagesInChild2.color = Color.Lerp(allRawImagesInChild2.color, Color.white, 10f * Time.deltaTime);
					}
				}
			}
		}
		if (!isWinnerHead && (bool)headTransform)
		{
			focusPoint.localPosition = playerListedTransform.localPosition + rectTransform.localPosition + headTransform.localPosition * rectTransform.localScale.x;
			float length = 12.5f;
			if (left)
			{
				length = -12.5f;
			}
			focusPoint.localPosition += new Vector3(LengthDirX(length, headTransform.localEulerAngles.z), LengthDirY(length, headTransform.localEulerAngles.z), 0f);
			length = 6f;
			focusPoint.localPosition += new Vector3(LengthDirX(length, headTransform.localEulerAngles.z + 90f), LengthDirY(length, headTransform.localEulerAngles.z + 90f), 0f);
		}
		if (isTalking != isTalkingPrev)
		{
			if (isTalking)
			{
				startedTalkingAtTime = Time.time;
			}
			isTalkingPrev = isTalking;
		}
		if (!isWinnerHead)
		{
			listSpot = playerListed.listSpot;
			if (listSpot != listSpotPrev)
			{
				if (listSpot % 2 == 0)
				{
					HeadRight();
				}
				else
				{
					HeadLeft();
				}
				listSpotPrev = listSpot;
			}
		}
		if (!isWinnerHead)
		{
			MenuPlayerHead menuPlayerHead = null;
			float num = 0f;
			foreach (MenuPlayerHead playerHead in MenuManager.instance.playerHeads)
			{
				if (!(playerHead == this) && playerHead.isTalking)
				{
					float num2 = playerHead.startedTalkingAtTime;
					if (num2 > num)
					{
						num = num2;
						menuPlayerHead = playerHead;
					}
				}
			}
			if ((bool)menuPlayerHead)
			{
				float num3 = 10f;
				Vector3 vector = menuPlayerHead.focusPoint.localPosition - focusPoint.localPosition;
				vector.z = 0f;
				Vector3 vector2 = new Vector3(50f, 25f, 0f) + vector.normalized * num3;
				if (left)
				{
					vector2 = new Vector3(-50f, 25f, 0f) + vector.normalized * num3;
				}
				eyesTransform.localPosition = Vector3.Lerp(eyesTransform.localPosition, vector2, Time.deltaTime * 10f);
			}
			else
			{
				float num4 = 10f;
				Vector3 vector3 = myFocusPoint.localPosition - focusPoint.localPosition;
				vector3.z = 0f;
				Vector3 vector4 = new Vector3(50f, 25f, 0f) + vector3.normalized * num4;
				if (left)
				{
					vector4 = new Vector3(-50f, 25f, 0f) + vector3.normalized * num4;
				}
				eyesTransform.localPosition = Vector3.Lerp(eyesTransform.localPosition, vector4, Time.deltaTime * 10f);
			}
		}
		if ((bool)playerAvatar && playerAvatar.voiceChatFetched && (!playerAvatar.playerDeathHead || !playerAvatar.playerDeathHead.spectated))
		{
			if (left)
			{
				float clipLoudness = playerAvatar.voiceChat.clipLoudness;
				headLeft.localEulerAngles = new Vector3(0f, 0f, (0f - clipLoudness) * 200f);
			}
			if (right)
			{
				float clipLoudness2 = playerAvatar.voiceChat.clipLoudness;
				headRight.localEulerAngles = new Vector3(0f, 0f, clipLoudness2 * 200f);
			}
		}
	}

	public void SetPlayer(PlayerAvatar player)
	{
		playerAvatar = player;
		if (allRawImagesInChildren.Count == 0)
		{
			allRawImagesInChildren.AddRange(GetComponentsInChildren<RawImage>(includeInactive: true));
		}
		if ((bool)playerAvatar && (bool)playerAvatar.playerAvatarVisuals)
		{
			SetColor(playerAvatar.playerAvatarVisuals.color);
		}
	}

	private void OnDestroy()
	{
		MenuManager.instance.PlayerHeadRemove(this);
		UnityEngine.Object.Destroy(focusPoint.gameObject);
		UnityEngine.Object.Destroy(myFocusPoint.gameObject);
	}

	public static float LengthDirX(float length, float direction)
	{
		return length * Mathf.Cos(direction * (MathF.PI / 180f));
	}

	public static float LengthDirY(float length, float direction)
	{
		return length * Mathf.Sin(direction * (MathF.PI / 180f));
	}
}
