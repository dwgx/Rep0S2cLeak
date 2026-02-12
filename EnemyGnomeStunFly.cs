using UnityEngine;

public class EnemyGnomeStunFly : MonoBehaviour
{
	public Enemy enemy;

	public EnemyGnome enemyGnome;

	private float soundTimer;

	private float spawnTimer;

	[Space]
	public Sound sound;

	private void Update()
	{
		if (enemyGnome.currentState == EnemyGnome.State.Stun && enemy.IsStunned() && (float)enemy.Rigidbody.physGrabObject.playerGrabbing.Count <= 0f && enemy.Rigidbody.physGrabObject.rbVelocity.magnitude > 2f)
		{
			soundTimer = 0.5f;
		}
		if (!enemy.isActiveAndEnabled)
		{
			spawnTimer = 2f;
			sound.PlayLoop(playing: false, 5f, 50f);
		}
		else if (soundTimer > 0f && spawnTimer <= 0f)
		{
			sound.PlayLoop(playing: true, 5f, 5f);
		}
		else
		{
			sound.PlayLoop(playing: false, 5f, 5f);
		}
		if (spawnTimer > 0f)
		{
			spawnTimer -= Time.deltaTime;
		}
		if (soundTimer > 0f)
		{
			soundTimer -= Time.deltaTime;
		}
	}
}
