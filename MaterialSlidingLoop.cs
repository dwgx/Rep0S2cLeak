using UnityEngine;

public class MaterialSlidingLoop : MonoBehaviour
{
	public AudioSource source;

	public float activeTimer;

	public MaterialPreset material;

	public float pitchMultiplier;

	public float getMaterialTimer;

	private AudioLowPassLogic lowPassLogic;

	public Sound sound;

	private void Start()
	{
		lowPassLogic = GetComponent<AudioLowPassLogic>();
		activeTimer = 1f;
	}

	private void Update()
	{
		if (getMaterialTimer > 0f)
		{
			getMaterialTimer -= Time.deltaTime;
		}
		if (activeTimer > 0f)
		{
			activeTimer -= Time.deltaTime;
			sound.PlayLoop(playing: true, 5f, 5f, pitchMultiplier);
			return;
		}
		lowPassLogic.Volume -= 5f * Time.deltaTime;
		if (lowPassLogic.Volume <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
