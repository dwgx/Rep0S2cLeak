using UnityEngine;

public class EnemyHeadUp : MonoBehaviour
{
	public Enemy enemy;

	private float startPosition;

	private void Start()
	{
		startPosition = base.transform.localPosition.y;
	}

	private void Update()
	{
		if (!enemy.NavMeshAgent.IsDisabled() && enemy.CurrentState == EnemyState.Chase && enemy.StateChase.VisionTimer > 0f && !enemy.TargetPlayerAvatar.isDisabled && enemy.TargetPlayerAvatar.PlayerVisionTarget.VisionTransform.position.y > startPosition)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, new Vector3(base.transform.position.x, enemy.TargetPlayerAvatar.PlayerVisionTarget.VisionTransform.position.y, base.transform.position.z), 1f * Time.deltaTime);
			return;
		}
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, new Vector3(0f, startPosition, 0f), 5f * Time.deltaTime);
		if (enemy.CurrentState == EnemyState.Despawn || enemy.NavMeshAgent.IsDisabled())
		{
			base.transform.localPosition = new Vector3(0f, startPosition, 0f);
		}
	}
}
