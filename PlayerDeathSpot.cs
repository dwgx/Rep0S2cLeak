using UnityEngine;

public class PlayerDeathSpot : MonoBehaviour
{
	private float timer = 5f;

	private void Awake()
	{
		GameDirector.instance.PlayerDeathSpots.Add(this);
	}

	private void Update()
	{
		timer -= Time.deltaTime;
		if (timer <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		GameDirector.instance.PlayerDeathSpots.Remove(this);
	}
}
