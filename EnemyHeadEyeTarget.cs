using UnityEngine;

public class EnemyHeadEyeTarget : MonoBehaviour
{
	public Enemy Enemy;

	public Transform Follow;

	[Space]
	public Vector3 Limit;

	public float Speed;

	public bool DebugShow;

	[Space]
	public bool Idle = true;

	public float IdleOffset;

	[Space]
	public float PupilSizeMultiplier;

	public float PupilSizeSpeed;

	public float PupilMinSize;

	public float PupilMaxSize;

	[HideInInspector]
	public float PupilCurrentSize;

	private Camera Camera;

	private void Start()
	{
		Camera = Camera.main;
	}

	private void Update()
	{
		if (!Enemy.CheckChase())
		{
			Idle = true;
		}
		else
		{
			Idle = false;
		}
		if (Idle || !Enemy.TargetPlayerAvatar)
		{
			base.transform.position = Follow.position + Follow.forward * IdleOffset;
		}
		else
		{
			base.transform.position = Enemy.TargetPlayerAvatar.PlayerVisionTarget.VisionTransform.position;
		}
		base.transform.rotation = Follow.rotation;
		float value = Vector3.Distance(Enemy.transform.position, Camera.transform.position) * PupilSizeMultiplier;
		value = Mathf.Clamp(value, PupilMinSize, PupilMaxSize);
		PupilCurrentSize = Mathf.Lerp(PupilCurrentSize, value, PupilSizeSpeed * Time.deltaTime);
	}

	private void OnValidate()
	{
		if (!SemiFunc.OnValidateCheck())
		{
			MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = DebugShow;
			}
		}
	}
}
