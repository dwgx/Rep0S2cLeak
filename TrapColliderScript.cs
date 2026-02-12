using UnityEngine;

public class TrapColliderScript : MonoBehaviour
{
	[HideInInspector]
	public bool TrapCollision;

	[HideInInspector]
	public float TrapCollisionForce;

	public PlayerAvatar triggerPlayer;

	private void OnTriggerEnter(Collider other)
	{
		PlayerTrigger component = other.GetComponent<PlayerTrigger>();
		if ((bool)component && !GameDirector.instance.LevelCompleted)
		{
			PlayerAvatar playerAvatar = component.PlayerAvatar;
			if (playerAvatar.isLocal && other.gameObject.CompareTag("Player") && !GameDirector.instance.LevelEnemyChasing && TrapDirector.instance.TrapCooldown <= 0f && !PlayerController.instance.Crouching)
			{
				TrapCollision = true;
				triggerPlayer = playerAvatar;
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(1f, 0.95f, 0f, 0.2f);
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1f, 0.95f, 0f, 0.5f);
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
	}
}
