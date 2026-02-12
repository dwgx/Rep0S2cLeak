using UnityEngine;

public class ScreenSpiderEffect : MonoBehaviour
{
	public float effectDuration;

	public float soundDuration;

	private float initialSoundDuration;

	private bool finalGlitchPlayed;

	public Sound spiderLoop;

	private bool isActive;

	private float activeTimer;

	internal PlayerAvatar playerAvatar;

	private void Start()
	{
		initialSoundDuration = soundDuration;
		SetActive();
	}

	private void Update()
	{
		if (!playerAvatar || playerAvatar.isDisabled)
		{
			SetInactive();
		}
		if (isActive)
		{
			if (SemiFunc.Arachnophobia())
			{
				spiderLoop.PlayLoop(playing: false, 10f, 20f);
			}
			else
			{
				spiderLoop.PlayLoop(soundDuration > 0f, 10f, 1f);
			}
			soundDuration -= Time.deltaTime;
			if (soundDuration > 0f)
			{
				float num = soundDuration / initialSoundDuration;
				if (Random.Range(0f, 1f) < Time.deltaTime / Random.Range(0.5f, 1f))
				{
					if (Random.Range(0f, 1f) < num)
					{
						CameraGlitch.Instance.PlayShort();
					}
					else
					{
						CameraGlitch.Instance.PlayTiny();
					}
				}
			}
			else if (soundDuration <= 0f && !finalGlitchPlayed)
			{
				CameraGlitch.Instance.PlayShort();
				finalGlitchPlayed = true;
			}
			activeTimer += Time.deltaTime;
			if (activeTimer >= effectDuration)
			{
				SetInactive();
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void SetActive()
	{
		isActive = true;
	}

	public void SetInactive()
	{
		isActive = false;
	}
}
