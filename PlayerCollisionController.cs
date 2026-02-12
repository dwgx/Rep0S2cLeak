using UnityEngine;

public class PlayerCollisionController : MonoBehaviour
{
	public Transform FollowTarget;

	public Vector3 Offset;

	public PlayerCollisionGrounded CollisionGrounded;

	[Space]
	public float GroundedDisableTimer;

	public bool Grounded;

	internal float fallDistance;

	private float fallLastY;

	private float tumbleVelocityTime;

	private float fallLoopPitch;

	private float fallLoopStopTimer;

	private float volume;

	public Sound soundFallLoop;

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (GroundedDisableTimer > 0f)
		{
			GroundedDisableTimer -= Time.deltaTime;
		}
		base.transform.position = FollowTarget.position + Offset;
		base.transform.rotation = FollowTarget.rotation;
		if (PlayerController.instance.playerAvatarScript.fallDamageResetState || SemiFunc.MenuLevel())
		{
			ResetFalling();
		}
		PlayerTumble tumble = PlayerController.instance.playerAvatarScript.tumble;
		if ((bool)tumble && !Grounded && (!tumble.isTumbling || (float)tumble.physGrabObject.playerGrabbing.Count <= 0f))
		{
			if (GameDirector.instance.currentState == GameDirector.gameState.Main && fallLastY - base.transform.position.y > 0f)
			{
				fallDistance += Mathf.Abs(base.transform.position.y - fallLastY);
			}
			fallLastY = base.transform.position.y;
			if (PlayerController.instance.featherTimer > 0f || PlayerController.instance.antiGravityTimer > 0f)
			{
				fallDistance = 0f;
			}
		}
		else
		{
			fallLastY = base.transform.position.y;
			fallDistance = 0f;
		}
		if (LevelGenerator.Instance.Generated)
		{
			PlayerController.instance.playerAvatarScript.isGrounded = Grounded;
		}
		float num = 0f;
		bool flag = false;
		if ((bool)tumble && tumble.isTumbling)
		{
			if (tumble.physGrabObject.rbVelocity.magnitude > 6f)
			{
				tumbleVelocityTime += Time.deltaTime;
				if (tumbleVelocityTime > 0.5f || tumble.physGrabObject.rbVelocity.magnitude > 8f)
				{
					if (tumble.physGrabObject.rbVelocity.magnitude > 15f)
					{
						fallLoopStopTimer = 0f;
					}
					flag = true;
				}
			}
			else
			{
				tumbleVelocityTime = 0f;
			}
			num = Mathf.Clamp(tumble.physGrabObject.rbVelocity.magnitude / 20f, 0.8f, 1.25f);
		}
		fallLoopPitch = Mathf.Lerp(fallLoopPitch, num, 10f * Time.deltaTime);
		if (fallLoopStopTimer > 0f)
		{
			volume = 0f;
			fallLoopStopTimer -= Time.deltaTime;
			soundFallLoop.PlayLoop(playing: false, 2f, 20f, fallLoopPitch);
			return;
		}
		if (!flag)
		{
			volume = 0f;
		}
		else
		{
			volume = Mathf.Lerp(volume, 1f, 0.75f * Time.deltaTime);
		}
		soundFallLoop.PlayLoop(flag, 5f, 5f, fallLoopPitch);
		soundFallLoop.LoopVolume = volume;
	}

	public void StopFallLoop()
	{
		fallLoopStopTimer = 1f;
	}

	public void ResetFalling()
	{
		StopFallLoop();
		fallDistance = 0f;
	}
}
