using UnityEngine;

public class EnemyHeadHair : MonoBehaviour
{
	public Transform Target;

	public bool DebugShow;

	[Space]
	public float PositionSpeed;

	public float RotationSpeed;

	private Vector3 Scale;

	private void Start()
	{
		Scale = base.transform.localScale;
	}

	private void Update()
	{
		if (PositionSpeed == 0f)
		{
			base.transform.position = Target.position;
		}
		else
		{
			base.transform.position = Vector3.Lerp(base.transform.position, Target.position, PositionSpeed * Time.deltaTime);
		}
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Target.rotation, RotationSpeed * Time.deltaTime);
		base.transform.localScale = new Vector3(Scale.x * Target.lossyScale.x, Scale.y * Target.lossyScale.y, Scale.z * Target.lossyScale.z);
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
