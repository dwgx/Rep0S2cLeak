using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
	public static MusicManager Instance;

	public AudioMixerSnapshot MusicMixerOn;

	public AudioMixerSnapshot MusicMixerOff;

	public AudioMixerSnapshot MusicMixerScareOnly;

	[Space(15f)]
	public MusicEnemyNear MusicEnemyNear;

	public MusicEnemySighting MusicEnemySighting;

	public MusicEnemyCatch MusicEnemyCatch;

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		if (MusicEnemySighting.Active)
		{
			MusicEnemyNear.LowerVolume(0.1f, 0.25f);
		}
	}
}
