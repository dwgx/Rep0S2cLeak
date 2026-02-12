using System;
using System.Collections.Generic;
using UnityEngine;

public class ValuableTeethBot : MonoBehaviour
{
	[Serializable]
	public class springLimb
	{
		public SpringQuaternion spring;

		public Transform transform;

		public Transform target;
	}

	public Transform talkTopTransform;

	public Transform talkBotTransform;

	[Space]
	public Sound loopSound;

	[Space]
	public List<springLimb> springLimbs = new List<springLimb>();

	private PhysGrabObject physGrabObject;

	private void Awake()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
	}

	private void Update()
	{
		foreach (springLimb springLimb in springLimbs)
		{
			springLimb.transform.rotation = SemiFunc.SpringQuaternionGet(springLimb.spring, springLimb.target.rotation);
		}
		float num = 0f;
		foreach (PhysGrabber item in physGrabObject.playerGrabbing)
		{
			if (item.playerAvatar.voiceChatFetched)
			{
				item.playerAvatar.voiceChat.OverridePosition(physGrabObject.centerPoint, 0.2f);
				item.playerAvatar.voiceChat.OverridePitch(1.25f, 0.1f, 0.1f, 0.2f, 0.05f, 100f);
				item.playerAvatar.voiceChat.OverrideNoTalkAnimation(0.2f);
				item.playerAvatar.voiceChat.OverrideHearSelf(0.2f, 0.2f);
				if (item.playerAvatar.voiceChat.clipLoudness > 0.005f)
				{
					num += item.playerAvatar.voiceChat.clipLoudness;
				}
			}
		}
		float num2 = Mathf.Lerp(0f, -90f, num * 4f);
		talkTopTransform.localRotation = Quaternion.Slerp(talkTopTransform.localRotation, Quaternion.Euler(num2, 0f, 0f), 50f * Time.deltaTime);
		talkBotTransform.localRotation = Quaternion.Slerp(talkBotTransform.localRotation, Quaternion.Euler((0f - num2) * 0.2f, 0f, 0f), 50f * Time.deltaTime);
		float num3 = 0.25f + Mathf.Abs(springLimbs[1].spring.springVelocity.x * 0.1f);
		num3 = Mathf.Min(num3, 1.5f);
		if (springLimbs[1].spring.springVelocity.magnitude > 1f)
		{
			loopSound.PlayLoop(playing: true, 5f, 1f, num3);
		}
		else
		{
			loopSound.PlayLoop(playing: false, 5f, 0.2f, num3);
		}
	}
}
