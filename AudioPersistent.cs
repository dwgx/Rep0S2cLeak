using UnityEngine;

public class AudioPersistent : MonoBehaviour
{
	private AudioSource audioSource;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		base.transform.parent = null;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
		if (!audioSource.isPlaying)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
