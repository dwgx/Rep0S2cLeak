using UnityEngine;

public class EnemyRobePersistent : MonoBehaviour
{
	public EnemyRobe enemyRobe;

	public ParticleSystem particleConstant;

	private void Update()
	{
		if (enemyRobe.isActiveAndEnabled && enemyRobe.currentState != EnemyRobe.State.Spawn)
		{
			if (!particleConstant.isPlaying)
			{
				particleConstant.Play();
			}
		}
		else if (particleConstant.isPlaying)
		{
			particleConstant.Stop();
		}
	}
}
