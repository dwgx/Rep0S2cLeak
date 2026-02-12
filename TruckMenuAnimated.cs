using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TruckMenuAnimated : MonoBehaviour
{
	public Transform antennaMiddleTransform;

	public Transform antennaMiddleTarget;

	public SpringQuaternion antennaMiddleSpring;

	[Space(20f)]
	public Transform frontPanelTransform;

	public Transform frontPanelTarget;

	public SpringQuaternion frontPanelSpring;

	[Space(20f)]
	public Transform windowRightTransform;

	public Transform windowRightTarget;

	public SpringQuaternion windowRightSpring;

	[Space(20f)]
	public Transform windowLeftTransform;

	public Transform windowLeftTarget;

	public SpringQuaternion windowLeftSpring;

	[Space(20f)]
	public Transform dishTransform;

	public Transform dishTarget;

	public SpringQuaternion dishSpring;

	[Space(20f)]
	public Transform antennaBackTransform;

	public Transform antennaBackTarget;

	public SpringQuaternion antennaBackSpring;

	private Animator animator;

	private PhotonView photonView;

	private float breakerCooldown;

	private float breakerCooldownMin = 8f;

	private float breakerCooldownMax = 16f;

	private List<string> breakerTriggers = new List<string>();

	private int breakerTriggerIndex;

	public ParticleSystem particleSkeletonBitsFirst;

	public ParticleSystem particleSkeletonSmokeFirst;

	public ParticleSystem particleSkeletonBitsLast;

	public ParticleSystem particleSkeletonSmokeLast;

	public Sound soundLoop;

	[Space]
	public Sound soundSwerve;

	public Sound soundSpeedUp;

	public Sound soundSlowDown;

	[Space]
	public Sound soundBodyRustleLong01;

	public Sound soundBodyRustleLong02;

	public Sound soundBodyRustleLong03;

	[Space]
	public Sound soundBodyRustleShort01;

	public Sound soundBodyRustleShort02;

	public Sound soundBodyRustleShort03;

	[Space]
	public Sound soundSkeletonHit;

	public Sound soundSkeletonHitSkull;

	[Space]
	public Sound soundSwerveFast01;

	public Sound soundSwerveFast02;

	[Space]
	public Sound soundFirePass;

	public Sound soundFirePassSwerve01;

	public Sound soundFirePassSwerve02;

	private void Start()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
		photonView = GetComponent<PhotonView>();
		breakerCooldown = Random.Range(breakerCooldownMin, breakerCooldownMax);
		breakerTriggers.Add("SpeedUp");
		breakerTriggers.Add("SlowDown");
		breakerTriggers.Add("Swerve");
		breakerTriggers.Add("SkeletonHit");
		breakerTriggers.Add("TruckPass");
		breakerTriggers.Shuffle();
	}

	private void Update()
	{
		antennaMiddleTransform.rotation = SemiFunc.SpringQuaternionGet(antennaMiddleSpring, antennaMiddleTarget.rotation);
		frontPanelTransform.rotation = SemiFunc.SpringQuaternionGet(frontPanelSpring, frontPanelTarget.rotation);
		frontPanelTransform.localEulerAngles = new Vector3(0f, frontPanelTransform.localEulerAngles.y, 0f);
		windowRightTransform.rotation = SemiFunc.SpringQuaternionGet(windowRightSpring, windowRightTarget.rotation);
		windowRightTransform.localEulerAngles = new Vector3(windowRightTransform.localEulerAngles.x, 0f, 0f);
		windowLeftTransform.rotation = SemiFunc.SpringQuaternionGet(windowLeftSpring, windowLeftTarget.rotation);
		windowLeftTransform.localEulerAngles = new Vector3(windowLeftTransform.localEulerAngles.x, 0f, 0f);
		dishTransform.rotation = SemiFunc.SpringQuaternionGet(dishSpring, dishTarget.rotation);
		antennaBackTransform.rotation = SemiFunc.SpringQuaternionGet(antennaBackSpring, antennaBackTarget.rotation);
		if (SemiFunc.IsMasterClientOrSingleplayer() && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
		{
			breakerCooldown -= Time.deltaTime;
			if (breakerCooldown <= 0f)
			{
				BreakerTrigger();
			}
		}
		soundLoop.PlayLoop(playing: true, 2f, 2f);
	}

	private void BreakerTrigger()
	{
		breakerCooldown = Random.Range(breakerCooldownMin, breakerCooldownMax);
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("BreakerTriggerRPC", RpcTarget.All, breakerTriggers[breakerTriggerIndex]);
		}
		else
		{
			BreakerTriggerRPC(breakerTriggers[breakerTriggerIndex]);
		}
		breakerTriggerIndex++;
		if (breakerTriggerIndex >= breakerTriggers.Count)
		{
			string text = breakerTriggers[breakerTriggers.Count - 1];
			breakerTriggers.Shuffle();
			while (breakerTriggers[0] == text)
			{
				breakerTriggers.Shuffle();
			}
			breakerTriggerIndex = 0;
		}
	}

	[PunRPC]
	private void BreakerTriggerRPC(string _triggerName, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info) && (bool)animator)
		{
			animator.SetTrigger(_triggerName);
		}
	}

	public void SkeletonHitFirstImpulse()
	{
		soundSkeletonHit.Play(soundLoop.Source.transform.position);
		particleSkeletonBitsFirst.Play();
		particleSkeletonSmokeFirst.Play();
		GameDirector.instance.CameraShake.Shake(1f, 0.3f);
		GameDirector.instance.CameraImpact.Shake(0.5f, 0.1f);
	}

	public void SkeletonHitLastImpulse()
	{
		soundSkeletonHitSkull.Play(soundLoop.Source.transform.position);
		particleSkeletonBitsLast.Play();
		particleSkeletonSmokeLast.Play();
	}

	public void PlaySwerve()
	{
		soundSwerve.Play(soundLoop.Source.transform.position);
	}

	public void PlaySpeedUp()
	{
		soundSpeedUp.Play(soundLoop.Source.transform.position);
	}

	public void PlaySlowDown()
	{
		soundSlowDown.Play(soundLoop.Source.transform.position);
	}

	public void PlayBodyRustleLong01()
	{
		soundBodyRustleLong01.Play(soundLoop.Source.transform.position);
	}

	public void PlayBodyRustleLong02()
	{
		soundBodyRustleLong02.Play(soundLoop.Source.transform.position);
	}

	public void PlayBodyRustleLong03()
	{
		soundBodyRustleLong03.Play(soundLoop.Source.transform.position);
	}

	public void PlayBodyRustleShort01()
	{
		soundBodyRustleShort01.Play(soundLoop.Source.transform.position);
	}

	public void PlayBodyRustleShort02()
	{
		soundBodyRustleShort02.Play(soundLoop.Source.transform.position);
	}

	public void PlayBodyRustleShort03()
	{
		soundBodyRustleShort03.Play(soundLoop.Source.transform.position);
	}

	public void PlaySwerveFast01()
	{
		soundSwerveFast01.Play(soundLoop.Source.transform.position);
	}

	public void PlaySwerveFast02()
	{
		soundSwerveFast02.Play(soundLoop.Source.transform.position);
	}

	public void PlayFirePass()
	{
		soundFirePass.Play(soundLoop.Source.transform.position);
	}

	public void PlayFirePassSwerve01()
	{
		soundFirePassSwerve01.Play(soundLoop.Source.transform.position);
	}

	public void PlayFirePassSwerve02()
	{
		soundFirePassSwerve02.Play(soundLoop.Source.transform.position);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
		Gizmos.matrix = antennaMiddleTarget.localToWorldMatrix;
		Gizmos.DrawLine(Vector3.zero, Vector3.up * 6f);
		Gizmos.matrix = frontPanelTarget.localToWorldMatrix;
		Gizmos.DrawLine(Vector3.zero, Vector3.right * 1.5f);
		Gizmos.matrix = windowRightTarget.localToWorldMatrix;
		Gizmos.DrawLine(Vector3.zero, Vector3.up * -2.5f);
		Gizmos.matrix = windowLeftTarget.localToWorldMatrix;
		Gizmos.DrawLine(Vector3.zero, Vector3.up * -2.5f);
		Gizmos.matrix = dishTarget.localToWorldMatrix;
		Gizmos.DrawLine(Vector3.zero, Vector3.up * 4f);
		Gizmos.matrix = antennaBackTarget.localToWorldMatrix;
		Gizmos.DrawLine(Vector3.zero, Vector3.up * 8f);
	}
}
