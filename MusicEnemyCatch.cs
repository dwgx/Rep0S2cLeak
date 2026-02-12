using UnityEngine;

public class MusicEnemyCatch : MonoBehaviour
{
	public LevelMusic LevelMusic;

	public AudioSource Source;

	public AudioClip[] Sounds;

	public void Play()
	{
		Source.clip = Sounds[Random.Range(0, Sounds.Length)];
		Source.Play();
		LevelMusic.Interrupt(10f);
	}
}
