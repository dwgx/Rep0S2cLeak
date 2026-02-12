using UnityEngine;

public class AudioAttack : MonoBehaviour
{
	private void Start()
	{
		AudioSource component = GetComponent<AudioSource>();
		if (Vector3.Distance(base.transform.position, PlayerController.instance.transform.position) < component.maxDistance)
		{
			LevelMusic.instance.Interrupt(10f);
		}
		EnemyDirector.instance.spawnIdlePauseTimer = 0f;
	}
}
