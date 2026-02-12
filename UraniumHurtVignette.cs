using UnityEngine;

public class UraniumHurtVignette : MonoBehaviour
{
	private HurtCollider hurtCollider;

	private void Start()
	{
		hurtCollider = GetComponent<HurtCollider>();
	}

	public void HurtVignette()
	{
		if (hurtCollider.onImpactPlayerAvatar.isLocal)
		{
			PostProcessing.Instance.VignetteOverride(new Color(0f, 0.6f, 0f), 0.5f, 0.5f, 5f, 2f, 0.33f, base.gameObject);
			CameraZoom.Instance.OverrideZoomSet(80f, 0.33f, 3f, 1f, base.gameObject, 50);
			CameraGlitch.Instance.PlayLong();
		}
	}
}
