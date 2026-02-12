using UnityEngine;

public class EnemyHeadLoop : MonoBehaviour
{
	public Enemy Enemy;

	public AudioSource AudioSource;

	[Space]
	public float VolumeMax;

	public float FadeInSpeed;

	public float FadeOutSpeed;

	private bool Active;

	private void Update()
	{
		if (Enemy.PlayerRoom.SameLocal || Enemy.OnScreen.OnScreenLocal)
		{
			if (!Active)
			{
				AudioSource.Play();
				Active = true;
			}
			if (AudioSource.volume < VolumeMax)
			{
				AudioSource.volume += FadeInSpeed * Time.deltaTime;
				AudioSource.volume = Mathf.Min(AudioSource.volume, VolumeMax);
			}
		}
		else if (Active && AudioSource.volume > 0f)
		{
			AudioSource.volume -= FadeOutSpeed * Time.deltaTime;
			if (AudioSource.volume <= 0f)
			{
				AudioSource.Stop();
				Active = false;
			}
		}
	}
}
