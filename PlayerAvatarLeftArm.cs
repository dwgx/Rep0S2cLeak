using UnityEngine;

public class PlayerAvatarLeftArm : MonoBehaviour
{
	public PlayerAvatar playerAvatar;

	public Transform leftArmTransform;

	public FlashlightController flashlightController;

	[Space]
	public AnimationCurve poseCurve;

	public float poseSpeed;

	private float poseLerp;

	private Vector3 poseNew;

	private Vector3 poseOld;

	private Vector3 poseCurrent;

	[Space]
	public Vector3 basePose;

	public Vector3 flashlightPose;

	public SpringQuaternion poseSpring;

	private PlayerAvatarVisuals playerAvatarVisuals;

	private float headRotation;

	private void Start()
	{
		playerAvatarVisuals = GetComponent<PlayerAvatarVisuals>();
	}

	private void Update()
	{
		if (!playerAvatarVisuals.isMenuAvatar && !playerAvatar.playerHealth.hurtFreeze)
		{
			if (flashlightController.currentState > FlashlightController.State.Hidden && flashlightController.currentState < FlashlightController.State.Outro && !playerAvatar.playerAvatarVisuals.animInCrawl)
			{
				SetPose(flashlightPose);
				HeadAnimate(_active: true);
				AnimatePose();
			}
			else
			{
				SetPose(basePose);
				HeadAnimate(_active: false);
				AnimatePose();
			}
		}
	}

	private void HeadAnimate(bool _active)
	{
		if (_active)
		{
			float num = playerAvatar.localCamera.transform.eulerAngles.x;
			if (num > 90f)
			{
				num -= 360f;
			}
			headRotation = Mathf.Lerp(headRotation, num * 0.5f, 20f * Time.deltaTime);
		}
		else
		{
			headRotation = Mathf.Lerp(headRotation, 0f, 20f * Time.deltaTime);
		}
	}

	private void AnimatePose()
	{
		if (poseLerp < 1f)
		{
			poseLerp += poseSpeed * Time.deltaTime;
			poseCurrent = Vector3.LerpUnclamped(poseOld, poseNew, poseCurve.Evaluate(poseLerp));
		}
		Quaternion rotation = leftArmTransform.rotation;
		leftArmTransform.localEulerAngles = new Vector3(poseCurrent.x, poseCurrent.y - headRotation, poseCurrent.z);
		Quaternion rotation2 = leftArmTransform.rotation;
		leftArmTransform.rotation = rotation;
		leftArmTransform.rotation = SemiFunc.SpringQuaternionGet(poseSpring, rotation2);
	}

	private void SetPose(Vector3 _poseNew)
	{
		if (poseNew != _poseNew)
		{
			poseOld = poseCurrent;
			poseNew = _poseNew;
			poseLerp = 0f;
		}
	}
}
