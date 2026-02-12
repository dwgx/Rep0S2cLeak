using UnityEngine;

public class RoachSmashCleanEffect : MonoBehaviour
{
	public CleanEffect cleanEffect;

	public float CleanEffectDelay;

	private float CleanEffectTimer;

	private bool CleanEffectDone;

	private void Start()
	{
	}

	private void Update()
	{
		if (!CleanEffectDone)
		{
			if (CleanEffectTimer > CleanEffectDelay)
			{
				cleanEffect.Clean();
				CleanEffectDone = true;
			}
			else
			{
				CleanEffectTimer += Time.deltaTime;
			}
		}
	}
}
