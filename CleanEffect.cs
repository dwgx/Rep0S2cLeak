using UnityEngine;

public class CleanEffect : MonoBehaviour
{
	[Space]
	[Header("Sounds")]
	public Sound CleanSound;

	public ParticleSystem GleamParticles;

	private float destroyTimer;

	public bool destroyNow;

	public void Update()
	{
		if (destroyNow)
		{
			destroyTimer += Time.deltaTime;
			if (destroyTimer > 1f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public void Clean()
	{
		destroyNow = true;
	}
}
