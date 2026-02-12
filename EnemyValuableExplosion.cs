using UnityEngine;

public class EnemyValuableExplosion : MonoBehaviour
{
	public enum Size
	{
		Small,
		Medium,
		Big
	}

	public Size size = Size.Medium;

	public Sound explosionSoundSmall;

	public Sound explosionSoundSmallGlobal;

	public Sound explosionSoundMedium;

	public Sound explosionSoundMediumGlobal;

	public Sound explosionSoundBig;

	public Sound explosionSoundBigGlobal;

	private void Start()
	{
		if (size == Size.Small)
		{
			explosionSoundSmall.Play(base.transform.position);
			explosionSoundSmallGlobal.Play(base.transform.position);
			GameDirector.instance.CameraImpact.ShakeDistance(4f, 6f, 15f, base.transform.position, 0.1f);
			GameDirector.instance.CameraShake.ShakeDistance(4f, 6f, 15f, base.transform.position, 0.1f);
		}
		else if (size == Size.Medium)
		{
			explosionSoundMedium.Play(base.transform.position);
			explosionSoundMediumGlobal.Play(base.transform.position);
			GameDirector.instance.CameraImpact.ShakeDistance(6f, 6f, 15f, base.transform.position, 0.1f);
			GameDirector.instance.CameraShake.ShakeDistance(6f, 6f, 15f, base.transform.position, 0.1f);
		}
		else if (size == Size.Big)
		{
			explosionSoundBig.Play(base.transform.position);
			explosionSoundBigGlobal.Play(base.transform.position);
			GameDirector.instance.CameraImpact.ShakeDistance(8f, 6f, 15f, base.transform.position, 0.1f);
			GameDirector.instance.CameraShake.ShakeDistance(8f, 6f, 15f, base.transform.position, 0.1f);
		}
	}
}
