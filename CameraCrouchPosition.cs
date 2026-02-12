using UnityEngine;

public class CameraCrouchPosition : MonoBehaviour
{
	public static CameraCrouchPosition instance;

	public CameraCrouchRotation CameraCrouchRotation;

	[Space]
	public float Position;

	public float PositionSpeed;

	public AnimationCurve AnimationCurve;

	internal float Lerp;

	[HideInInspector]
	public bool Active;

	internal bool ActivePrev;

	private PlayerController Player;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		Player = PlayerController.instance;
	}

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (Player.Crouching)
		{
			Active = true;
		}
		else
		{
			Active = false;
		}
		if (Active != ActivePrev)
		{
			if (Active)
			{
				PlayerController.instance.playerAvatarScript.StandToCrouch();
			}
			else
			{
				PlayerController.instance.playerAvatarScript.CrouchToStand();
			}
			GameDirector.instance.CameraShake.Shake(2f, 0.1f);
			CameraCrouchRotation.RotationLerp = 0f;
			ActivePrev = Active;
		}
		float num = PositionSpeed * PlayerController.instance.playerAvatarScript.playerAvatarVisuals.animationSpeedMultiplier;
		if (Player.Sliding)
		{
			num *= 2f;
		}
		if (Active)
		{
			Lerp += Time.deltaTime * num;
		}
		else
		{
			Lerp -= Time.deltaTime * num;
		}
		Lerp = Mathf.Clamp01(Lerp);
		base.transform.localPosition = new Vector3(0f, AnimationCurve.Evaluate(Lerp) * Position, 0f);
	}
}
