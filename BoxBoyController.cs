using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxBoyController : MonoBehaviour
{
	[Serializable]
	public class springLimb
	{
		public SpringQuaternion spring;

		public Transform transform;

		public Transform target;
	}

	public Transform boxBoyEye1;

	public Transform playerEye1;

	[Space(10f)]
	public Transform boxBoyEye2;

	public Transform playerEye2;

	[Space(10f)]
	public Transform boxBoyEyeLidUpper1;

	public Transform playerEyeLidUpper1;

	[Space(10f)]
	public Transform boxBoyEyeLidUpperClose1;

	public Transform playerEyeLidUpperClose1;

	[Space(10f)]
	public Transform boxBoyEyeLidUpper2;

	public Transform playerEyeLidUpper2;

	[Space(10f)]
	public Transform boxBoyEyeLidUpperClose2;

	public Transform playerEyeLidUpperClose2;

	[Space(10f)]
	public Transform boxBoyEyeLidLower1;

	public Transform playerEyeLidLower1;

	[Space(10f)]
	public Transform boxBoyEyeLidLowerClose1;

	public Transform playerEyeLidLowerClose1;

	[Space(10f)]
	public Transform boxBoyEyeLidLower2;

	public Transform playerEyeLidLower2;

	[Space(10f)]
	public Transform boxBoyEyeLidLowerClose2;

	public Transform playerEyeLidLowerClose2;

	[Space(10f)]
	public Transform boxBoyEyeLids1;

	public Transform playerEyeLids1;

	[Space(10f)]
	public Transform boxBoyEyeLids2;

	public Transform playerEyeLids2;

	[Space(10f)]
	public Transform boxBoyHead;

	public Transform playerHead;

	[Space(10f)]
	public Transform boxBoyLeg1;

	public Transform playerLeg1;

	[Space(10f)]
	public Transform boxBoyLeg2;

	public Transform playerLeg2;

	[Space(10f)]
	public Transform playerBody;

	[Space(10f)]
	public Transform boxBoyCodeTilt;

	public Transform playerCodeTilt;

	[Space(10f)]
	public Transform boxBoyCodeLean;

	public Transform playerCodeLean;

	[Space(10f)]
	[Space(20f)]
	public PlayerAvatarVisuals playerAvatarVisuals;

	private PlayerAvatar playerAvatar;

	public Transform playerRig;

	[Space]
	public List<springLimb> springLimbs = new List<springLimb>();

	private void Start()
	{
		playerAvatar = playerAvatarVisuals.GetComponent<PlayerAvatar>();
		StartCoroutine(InitializeComponents());
	}

	private IEnumerator InitializeComponents()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.5f);
		}
		playerAvatar = playerAvatarVisuals.GetComponent<PlayerAvatar>();
		if (playerAvatar.isLocal)
		{
			base.gameObject.SetActive(value: false);
			yield break;
		}
		MeshRenderer[] componentsInChildren = playerRig.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}

	private void Update()
	{
		Vector3 localPosition = playerLeg1.localPosition - (playerLeg1.localScale - Vector3.one) * 0.1f;
		localPosition += playerLeg2.localPosition - (playerLeg2.localScale - Vector3.one) * 0.1f;
		base.transform.localPosition = localPosition;
		boxBoyCodeTilt.rotation = playerCodeTilt.rotation;
		boxBoyCodeLean.rotation = playerCodeLean.rotation;
		boxBoyEye1.rotation = playerEye1.rotation;
		boxBoyEye2.rotation = playerEye2.rotation;
		boxBoyHead.localRotation = Quaternion.Euler(playerHead.localRotation.eulerAngles.x, 0f, 0f);
		boxBoyLeg1.localPosition = playerLeg1.localPosition - (playerLeg1.localScale - Vector3.one) * 0.1f;
		boxBoyLeg2.localPosition = playerLeg2.localPosition - (playerLeg2.localScale - Vector3.one) * 0.1f;
		boxBoyEyeLidUpper1.rotation = playerEyeLidUpper1.rotation;
		boxBoyEyeLidUpper2.rotation = playerEyeLidUpper2.rotation;
		boxBoyEyeLidLower1.rotation = playerEyeLidLower1.rotation;
		boxBoyEyeLidLower2.rotation = playerEyeLidLower2.rotation;
		boxBoyEyeLids1.rotation = playerEyeLids1.rotation;
		boxBoyEyeLids2.rotation = playerEyeLids2.rotation;
		boxBoyEyeLidUpperClose1.rotation = playerEyeLidUpperClose1.rotation;
		boxBoyEyeLidUpperClose2.rotation = playerEyeLidUpperClose2.rotation;
		boxBoyEyeLidLowerClose1.rotation = playerEyeLidLowerClose1.rotation;
		boxBoyEyeLidLowerClose2.rotation = playerEyeLidLowerClose2.rotation;
		bool flag = true;
		foreach (springLimb springLimb in springLimbs)
		{
			if (flag)
			{
				springLimb.spring.speed = 10f;
				springLimb.spring.damping = 0.3f;
				flag = false;
			}
			else
			{
				springLimb.spring.speed = 30f;
				springLimb.spring.damping = 0.2f;
				springLimb.spring.maxAngle = 3f;
			}
			springLimb.transform.rotation = SemiFunc.SpringQuaternionGet(springLimb.spring, springLimb.target.rotation);
		}
	}
}
