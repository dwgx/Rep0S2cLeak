using UnityEngine;

public class AudioListenerFollow : MonoBehaviour
{
	public static AudioListenerFollow instance;

	public Transform TargetPositionTransform;

	public Transform TargetRotationTransform;

	internal LowPassTrigger lowPassTrigger;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		TargetPositionTransform = Camera.main.transform;
		TargetRotationTransform = Camera.main.transform;
	}

	private void Update()
	{
		if (!TargetPositionTransform)
		{
			return;
		}
		if ((bool)SpectateCamera.instance && SpectateCamera.instance.CheckState(SpectateCamera.State.Death))
		{
			base.transform.position = TargetPositionTransform.position;
		}
		else
		{
			base.transform.position = TargetPositionTransform.position + TargetPositionTransform.forward * AssetManager.instance.mainCamera.nearClipPlane;
		}
		if (!TargetRotationTransform)
		{
			return;
		}
		base.transform.rotation = TargetRotationTransform.rotation;
		if (!SemiFunc.FPSImpulse15())
		{
			return;
		}
		lowPassTrigger = null;
		Collider[] array = Physics.OverlapSphere(base.transform.position, 0.1f, LayerMask.GetMask("LowPassTrigger"), QueryTriggerInteraction.Collide);
		if (array.Length != 0)
		{
			lowPassTrigger = array[0].GetComponent<LowPassTrigger>();
			if (!lowPassTrigger)
			{
				lowPassTrigger = array[0].GetComponentInParent<LowPassTrigger>();
			}
		}
	}
}
