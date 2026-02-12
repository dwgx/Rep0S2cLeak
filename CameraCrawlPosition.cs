using UnityEngine;

public class CameraCrawlPosition : MonoBehaviour
{
	public CameraCrouchPosition CrouchPosition;

	[Space]
	public float Position;

	public float PositionSpeed;

	public AnimationCurve AnimationCurve;

	private float Lerp;

	[Space]
	public Sound IntroSound;

	[Space]
	public Sound OutroSound;

	[HideInInspector]
	public bool Active;

	private bool ActivePrev;

	private PlayerController Player;

	private void Start()
	{
		Player = PlayerController.instance;
	}

	private void Update()
	{
		if (Player.Crawling || Player.Sliding)
		{
			Active = true;
		}
		else
		{
			Active = false;
		}
		if (Active != ActivePrev)
		{
			if (Active)
			{
				PlayerController.instance.playerAvatarScript.CrouchToCrawl();
			}
			else
			{
				PlayerController.instance.playerAvatarScript.CrawlToCrouch();
			}
			GameDirector.instance.CameraShake.Shake(1f, 0.1f);
			ActivePrev = Active;
		}
		float num = PositionSpeed;
		if (Player.Sliding)
		{
			num *= 2f;
		}
		if (Active)
		{
			Lerp += Time.deltaTime * num;
		}
		else
		{
			Lerp -= Time.deltaTime * num;
		}
		Lerp = Mathf.Clamp01(Lerp);
		base.transform.localPosition = new Vector3(0f, AnimationCurve.Evaluate(Lerp) * Position, 0f);
	}
}
