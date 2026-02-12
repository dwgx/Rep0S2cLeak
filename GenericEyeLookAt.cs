using System;
using UnityEngine;

public class GenericEyeLookAt : MonoBehaviour
{
	[Serializable]
	public class EyeSetup
	{
		public bool hasOffsetTransform;

		public Transform offsetTransform;

		public float clamp = 60f;

		public SpringQuaternionSystem spring;
	}

	public Transform animTransform;

	[Space]
	public bool idleAnimation;

	public float idleTimeMin = 1f;

	public float idleTimeMax = 3f;

	public float idleDistance = 0.5f;

	[Space]
	public bool eyeDarts;

	public float eyeDartTimeMin = 0.1f;

	public float eyeDartTimeMax = 0.75f;

	public float eyeDartDistance = 0.05f;

	[Space]
	public EyeSetup[] eyes;

	private Vector3 startPosition;

	private float eyeIdleTimer;

	private Vector3 eyeIdlePosition;

	private Vector3 eyeDartPosition;

	private float eyeDartTimer;

	private Vector3 targetPosition;

	private float targetTime;

	private void Awake()
	{
		startPosition = base.transform.localPosition;
	}

	private void Update()
	{
		if (eyeDarts)
		{
			if (eyeDartTimer <= 0f)
			{
				if (UnityEngine.Random.Range(0, 4) == 0)
				{
					eyeDartTimer = UnityEngine.Random.Range(eyeDartTimeMin, eyeDartTimeMax);
					eyeDartPosition = new Vector3(UnityEngine.Random.Range(0f - eyeDartDistance, eyeDartDistance), UnityEngine.Random.Range(0f - eyeDartDistance, eyeDartDistance), 0f);
				}
			}
			else
			{
				eyeDartTimer -= Time.deltaTime;
			}
		}
		if (idleAnimation)
		{
			if (eyeIdleTimer <= 0f)
			{
				eyeIdleTimer = UnityEngine.Random.Range(idleTimeMin, idleTimeMax);
				if (UnityEngine.Random.Range(0, 2) == 0)
				{
					eyeIdlePosition = Vector3.zero;
				}
				else
				{
					eyeIdlePosition = new Vector3(UnityEngine.Random.Range(0f - idleDistance, idleDistance), UnityEngine.Random.Range(0f - idleDistance, idleDistance), 0f);
				}
			}
			else
			{
				eyeIdleTimer -= Time.deltaTime;
			}
		}
		if (targetTime > 0f)
		{
			targetTime -= Time.deltaTime;
			base.transform.position = targetPosition + eyeDartPosition;
		}
		else
		{
			base.transform.localPosition = startPosition + eyeIdlePosition + eyeDartPosition;
			base.transform.localRotation = Quaternion.identity;
		}
		EyeSetup[] array = eyes;
		foreach (EyeSetup eyeSetup in array)
		{
			Transform offsetTransform = animTransform;
			if (eyeSetup.hasOffsetTransform)
			{
				offsetTransform = eyeSetup.offsetTransform;
			}
			eyeSetup.spring.target.LookAt(offsetTransform);
			eyeSetup.spring.target.forward = SemiFunc.ClampDirection(eyeSetup.spring.target.forward, base.transform.forward, eyeSetup.clamp);
			eyeSetup.spring.UpdateLocalSpace();
		}
	}

	public void SetTarget(Vector3 _position, float _time = 0.5f)
	{
		targetPosition = _position;
		targetTime = _time;
	}

	public void SetTargetPlayer(PlayerAvatar _playerAvatar, float _time = 0.5f)
	{
		if (_playerAvatar.isLocal)
		{
			targetPosition = _playerAvatar.localCamera.transform.position;
		}
		else
		{
			targetPosition = _playerAvatar.PlayerVisionTarget.VisionTransform.position;
		}
		targetTime = _time;
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = animTransform.localToWorldMatrix;
		float num = 0.075f;
		Gizmos.color = new Color(0.99f, 0.99f, 1f, 0.75f);
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one * num);
		Gizmos.color = new Color(1f, 0f, 0.17f, 0.4f);
		Gizmos.DrawCube(Vector3.zero, Vector3.one * num);
		EyeSetup[] array = eyes;
		foreach (EyeSetup eyeSetup in array)
		{
			Transform offsetTransform = animTransform;
			if (eyeSetup.hasOffsetTransform && (bool)eyeSetup.offsetTransform)
			{
				offsetTransform = eyeSetup.offsetTransform;
				Gizmos.matrix = offsetTransform.localToWorldMatrix;
				num = 0.03f;
				Gizmos.color = new Color(0.99f, 0.99f, 1f, 0.3f);
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one * num);
				Gizmos.color = new Color(1f, 0f, 0.17f, 0.2f);
				Gizmos.DrawCube(Vector3.zero, Vector3.one * num);
			}
			if ((bool)eyeSetup.spring.transform)
			{
				Gizmos.matrix = Matrix4x4.identity;
				Gizmos.color = new Color(1f, 1f, 1f, 0.075f);
				Gizmos.DrawLine(eyeSetup.spring.transform.position, offsetTransform.position);
			}
		}
	}
}
