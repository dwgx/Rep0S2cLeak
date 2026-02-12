using UnityEngine;

public class SpiderSoundLoop : MonoBehaviour
{
	public Sound spiderLoop;

	public float effectDuration;

	public float soundDuration;

	internal PlayerAvatar playerAvatar;

	private float activeTimer;

	private void Update()
	{
		if (!playerAvatar || playerAvatar.isDisabled)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		base.transform.position = playerAvatar.PlayerVisionTarget.VisionTransform.position + new Vector3(0f, -0.2f, 0f);
		if (SemiFunc.Arachnophobia())
		{
			spiderLoop.PlayLoop(playing: false, 10f, 20f);
		}
		else
		{
			spiderLoop.PlayLoop(soundDuration > 0f, 10f, 1f);
		}
		soundDuration -= Time.deltaTime;
		activeTimer += Time.deltaTime;
		if (activeTimer >= effectDuration)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
