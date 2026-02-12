using System.Collections.Generic;
using UnityEngine;

public class EnemySlowMouthAttaching : MonoBehaviour
{
	internal EnemySlowMouth enemySlowMouth;

	public Transform tentaclesTransform;

	public List<Transform> eyeTransforms = new List<Transform>();

	public Transform topJaw;

	public Transform bottomJaw;

	public Transform particleTransform;

	private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

	private Quaternion startRotationTopJaw;

	private Quaternion startRotationBottomJaw;

	internal Transform targetTransform;

	public PlayerAvatar targetPlayerAvatar;

	private SpringQuaternion springQuaternion;

	private bool isActive;

	private Vector3 startPosition;

	private SpringFloat springFloatScale;

	private float targetScale = 1f;

	public GameObject topJawPrefab;

	public GameObject bottomJawPrefab;

	public GameObject localPlayerJaw;

	[Space(20f)]
	public Sound soundAttachVO;

	public Sound soundAttach;

	private void Start()
	{
		springQuaternion = new SpringQuaternion();
		springQuaternion.damping = 0.5f;
		springQuaternion.speed = 20f;
		springFloatScale = new SpringFloat();
		springFloatScale.damping = 0.5f;
		springFloatScale.speed = 20f;
		startRotationTopJaw = topJaw.localRotation;
		startRotationBottomJaw = bottomJaw.localRotation;
		startPosition = base.transform.position;
		particleSystems = new List<ParticleSystem>(particleTransform.GetComponentsInChildren<ParticleSystem>());
		GoTime();
	}

	public void GoTime()
	{
		PlayParticles(finalPlay: false);
		if (targetPlayerAvatar.isLocal)
		{
			CameraGlitch.Instance.PlayLong();
		}
		SetTarget(targetPlayerAvatar);
		springFloatScale.springVelocity = 50f;
		isActive = true;
		targetScale = 1f;
		GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, base.transform.position, 0.1f);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.1f);
		soundAttachVO.Play(base.transform.position);
	}

	private void PlayParticles(bool finalPlay)
	{
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			particleSystem.Play();
			if (finalPlay)
			{
				particleSystem.transform.parent = null;
				Object.Destroy(particleSystem.gameObject, 4f);
			}
		}
	}

	private void SpawnPlayerJaw()
	{
		if (!targetPlayerAvatar.isLocal)
		{
			GameObject obj = Object.Instantiate(topJawPrefab, base.transform.position, base.transform.rotation);
			GameObject gameObject = Object.Instantiate(bottomJawPrefab, base.transform.position, base.transform.rotation);
			EnemySlowMouthPlayerAvatarAttached component = obj.GetComponent<EnemySlowMouthPlayerAvatarAttached>();
			component.jawBot = gameObject.transform;
			Transform attachPointJawTop = targetPlayerAvatar.playerAvatarVisuals.attachPointJawTop;
			Transform attachPointJawBottom = targetPlayerAvatar.playerAvatarVisuals.attachPointJawBottom;
			obj.transform.parent = attachPointJawTop;
			component.playerTarget = targetPlayerAvatar;
			component.enemySlowMouth = enemySlowMouth;
			component.semiPuke = gameObject.GetComponentInChildren<SemiPuke>();
			obj.transform.localPosition = Vector3.zero;
			obj.transform.rotation = Quaternion.identity;
			obj.transform.localRotation = Quaternion.identity;
			gameObject.transform.parent = attachPointJawBottom;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.rotation = Quaternion.identity;
			gameObject.transform.localRotation = Quaternion.identity;
		}
		else
		{
			Transform parent = targetPlayerAvatar.localCamera.transform;
			GameObject obj2 = Object.Instantiate(localPlayerJaw, base.transform.position, Quaternion.identity, parent);
			obj2.transform.localPosition = Vector3.zero;
			obj2.transform.localRotation = Quaternion.identity;
			EnemySlowMouthCameraVisuals component2 = obj2.GetComponent<EnemySlowMouthCameraVisuals>();
			component2.enemySlowMouth = enemySlowMouth;
			component2.playerTarget = targetPlayerAvatar;
		}
	}

	private void Update()
	{
		bool flag = !targetTransform || !targetPlayerAvatar;
		if (!isActive)
		{
			return;
		}
		if (flag)
		{
			Detach();
			return;
		}
		Quaternion targetRotation = Quaternion.LookRotation(targetTransform.position - base.transform.position);
		base.transform.rotation = SemiFunc.SpringQuaternionGet(springQuaternion, targetRotation);
		float num = SemiFunc.SpringFloatGet(springFloatScale, targetScale);
		base.transform.localScale = Vector3.one * num;
		float num2 = Vector3.Distance(base.transform.position, targetTransform.position);
		float num3 = num2 * 2f;
		if (num3 < 4f)
		{
			num3 = 4f;
		}
		if (num3 > 10f)
		{
			num3 = 10f;
		}
		base.transform.position = Vector3.MoveTowards(base.transform.position, targetTransform.position, Time.deltaTime * num3);
		if (num2 < 1f)
		{
			targetScale = 2.5f;
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (enemySlowMouth.currentState != EnemySlowMouth.State.Attack)
			{
				if (!targetPlayerAvatar.isDisabled && !enemySlowMouth.IsPossessed())
				{
					AttachToPlayer();
				}
				else
				{
					Detach();
				}
			}
			if (targetPlayerAvatar.isLocal)
			{
				GameDirector.instance.CameraShake.Shake(4f, 0.1f);
			}
		}
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (!targetPlayerAvatar.isDisabled && !enemySlowMouth.IsPossessed())
		{
			if (num2 < 0.1f)
			{
				AttachToPlayer();
				enemySlowMouth.UpdateState(EnemySlowMouth.State.Attached);
				isActive = false;
			}
		}
		else
		{
			Detach();
		}
		if (targetPlayerAvatar.isLocal)
		{
			CameraAim.Instance.AimTargetSet(base.transform.position, 0.2f, 20f, base.gameObject, 100);
			CameraZoom.Instance.OverrideZoomSet(30f, 0.1f, 8f, 1f, base.gameObject, 50);
		}
		tentaclesTransform.localScale = new Vector3(1f + Mathf.Sin(Time.time * 40f) * 0.2f, 1f + Mathf.Sin(Time.time * 60f) * 0.1f, 1f);
		tentaclesTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 20f) * 10f);
		topJaw.localRotation = startRotationTopJaw * Quaternion.Euler(Mathf.Sin(Time.time * 60f) * 3f, 0f, 0f);
		bottomJaw.localRotation = startRotationBottomJaw * Quaternion.Euler(Mathf.Sin(Time.time * 60f) * 10f, 0f, 0f);
		foreach (Transform eyeTransform in eyeTransforms)
		{
			eyeTransform.localScale = new Vector3(1.5f + Mathf.Sin(Time.time * 40f) * 0.5f, 1.5f + Mathf.Sin(Time.time * 60f) * 0.5f, 1.5f);
		}
	}

	private void AttachToPlayer()
	{
		if (!targetPlayerAvatar.isDisabled && !targetPlayerAvatar.GetComponentInChildren<EnemySlowMouthPlayerAvatarAttached>())
		{
			SpawnPlayerJaw();
			Despawn();
		}
	}

	private void Detach()
	{
		soundAttach.Play(base.transform.position);
		PlayParticles(finalPlay: true);
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			enemySlowMouth.detachPosition = base.transform.position;
			enemySlowMouth.detachRotation = base.transform.rotation;
			enemySlowMouth.UpdateState(EnemySlowMouth.State.Detach);
		}
		Object.Destroy(base.gameObject);
	}

	private void Despawn()
	{
		soundAttach.Play(base.transform.position);
		PlayParticles(finalPlay: true);
		if (targetPlayerAvatar.isLocal)
		{
			GameDirector.instance.CameraImpact.Shake(8f, 0.1f);
			GameDirector.instance.CameraShake.Shake(5f, 0.1f);
			CameraGlitch.Instance.PlayLong();
		}
		else
		{
			GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, base.transform.position, 0.1f);
			GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.1f);
		}
		Object.Destroy(base.gameObject);
	}

	public void SetTarget(PlayerAvatar _playerAvatar)
	{
		targetTransform = SemiFunc.PlayerGetFaceEyeTransform(_playerAvatar);
		targetPlayerAvatar = _playerAvatar;
	}
}
