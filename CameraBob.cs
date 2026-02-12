using UnityEngine;

public class CameraBob : MonoBehaviour
{
	public static CameraBob Instance;

	public PlayerController playerController;

	public AudioPlay footstepAudio;

	[Header("Bob Up")]
	public AnimationCurve bobUpCurve;

	public float bobUpLerpSpeed;

	public float bobUpLerpStrength;

	private float bobUpLerpStrengthCurrent;

	private float bobUpLerpAmount;

	public float bobUpActiveLerpSpeedIn = 1f;

	public float bobUpActiveLerpSpeedOut = 1f;

	private float bobUpActiveLerp;

	public AnimationCurve bobUpActiveCurve;

	[Header("Bob Side")]
	public AnimationCurve bobSideCurve;

	public float bobSideLerpSpeed;

	public float bobSideLerpStrength;

	private float bobSideLerpStrengthCurrent;

	private float bobSideLerpAmount;

	private bool bobSideRev;

	public float bobSideActiveLerpSpeedIn = 1f;

	public float bobSideActiveLerpSpeedOut = 1f;

	private float bobSideActiveLerp;

	public AnimationCurve bobSideActiveCurve;

	[Header("Other")]
	public float SprintSpeedMultiplier = 1f;

	public float CrouchSpeedMultiplier = 1f;

	public float CrouchAmountMultiplier = 1f;

	private float Multiplier;

	private float MultiplierTarget;

	private float MultiplierTimer;

	internal Vector3 positionResult;

	internal Quaternion rotationResult;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		bobUpLerpStrengthCurrent = bobUpLerpStrength;
		bobSideLerpStrengthCurrent = bobSideLerpStrength;
	}

	public void SetMultiplier(float multiplier, float time)
	{
		MultiplierTarget = multiplier;
		MultiplierTimer = time;
	}

	private void Update()
	{
		float overrideSpeedMultiplier = PlayerController.instance.overrideSpeedMultiplier;
		float num = Time.deltaTime * overrideSpeedMultiplier;
		if (MultiplierTimer > 0f)
		{
			MultiplierTimer -= 1f * num;
		}
		else
		{
			MultiplierTarget = 1f;
		}
		Multiplier = Mathf.Lerp(Multiplier, MultiplierTarget, 5f * num);
		if (GameDirector.instance.currentState == GameDirector.gameState.Main && !PlayerController.instance.playerAvatarScript.isDisabled)
		{
			float num2 = 1f;
			float num3 = 1f;
			if (playerController.sprinting)
			{
				float num4 = SprintSpeedMultiplier + (float)StatsManager.instance.playerUpgradeSpeed[playerController.playerAvatarScript.steamID] * 0.1f;
				num2 = Mathf.Lerp(1f, num4, playerController.SprintSpeedLerp);
			}
			else if (playerController.Crouching)
			{
				num2 = CrouchSpeedMultiplier;
				num3 = CrouchAmountMultiplier;
			}
			bobUpLerpStrengthCurrent = Mathf.Lerp(bobUpLerpStrengthCurrent, bobUpLerpStrength * num3, 5f * num);
			float num5 = Mathf.LerpUnclamped(0f, bobUpLerpStrengthCurrent, bobUpCurve.Evaluate(bobUpLerpAmount));
			bobUpLerpAmount += bobUpLerpSpeed * bobUpActiveLerp * num2 * num;
			if (bobUpLerpAmount > 1f)
			{
				if (playerController.CollisionController.Grounded && !CameraJump.instance.jumpActive)
				{
					if (playerController.sprinting)
					{
						playerController.playerAvatarScript.Footstep(Materials.SoundType.Heavy);
					}
					else if (bobUpActiveLerp > 0.75f && !playerController.Crouching)
					{
						playerController.playerAvatarScript.Footstep(Materials.SoundType.Medium);
					}
					else
					{
						playerController.playerAvatarScript.Footstep(Materials.SoundType.Light);
					}
				}
				bobUpLerpAmount = 0f;
			}
			if (playerController.moving && playerController.CollisionController.Grounded)
			{
				bobUpActiveLerp = Mathf.Clamp01(bobUpActiveLerp + bobUpActiveLerpSpeedIn * num);
			}
			else
			{
				bobUpActiveLerp = Mathf.Clamp01(bobUpActiveLerp - bobUpActiveLerpSpeedOut * num);
			}
			bobSideLerpStrengthCurrent = Mathf.Lerp(bobSideLerpStrengthCurrent, bobSideLerpStrength * num3, 5f * num);
			float num6 = Mathf.LerpUnclamped(0f - bobSideLerpStrengthCurrent, bobSideLerpStrengthCurrent, bobSideCurve.Evaluate(bobSideLerpAmount));
			if (bobSideRev)
			{
				bobSideLerpAmount += bobSideLerpSpeed * bobSideActiveLerp * num2 * num;
				if (bobSideLerpAmount > 1f)
				{
					bobSideRev = false;
				}
			}
			else
			{
				bobSideLerpAmount -= bobSideLerpSpeed * bobSideActiveLerp * num2 * num;
				if (bobSideLerpAmount < 0f)
				{
					bobSideRev = true;
				}
			}
			if (playerController.moving)
			{
				bobSideActiveLerp = Mathf.Clamp01(bobSideActiveLerp + bobSideActiveLerpSpeedIn * num);
			}
			else
			{
				bobSideActiveLerp = Mathf.Clamp01(bobSideActiveLerp - bobSideActiveLerpSpeedOut * num);
			}
			positionResult = new Vector3(0f, Mathf.LerpUnclamped(0f, num5, bobUpActiveCurve.Evaluate(bobUpActiveLerp)) * Multiplier, 0f);
			rotationResult = Quaternion.Euler(0f, Mathf.LerpUnclamped(0f, num6 * 10f, bobSideActiveCurve.Evaluate(bobSideActiveLerp)) * Multiplier, Mathf.LerpUnclamped(0f, num6 * 5f, bobSideActiveCurve.Evaluate(bobSideActiveLerp)) * Multiplier);
		}
		else
		{
			bobSideActiveLerp = 0f;
			bobUpActiveLerp = 0f;
			positionResult = Vector3.Lerp(positionResult, Vector3.zero, 5f * num);
			rotationResult = Quaternion.Slerp(rotationResult, Quaternion.identity, 5f * num);
		}
		base.transform.localPosition = Vector3.Lerp(Vector3.zero, positionResult, GameplayManager.instance.cameraAnimation);
		base.transform.localRotation = Quaternion.Slerp(Quaternion.identity, rotationResult, GameplayManager.instance.cameraAnimation);
	}
}
