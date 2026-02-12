using UnityEngine;

public class EnemyHeartHuggerGasGuider : MonoBehaviour
{
	internal Vector3 startPosition;

	internal Transform targetTransform;

	internal EnemyHeartHugger enemyHeartHugger;

	internal Transform headTransform;

	internal PlayerTumble playerTumble;

	internal PhysGrabObject physGrabObject;

	private float moveAlongEval;

	private float bringPlayerInTimer;

	internal PlayerAvatar player;

	private int tries;

	private float targetOutOfRangeTimer;

	private void Update()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || !enemyHeartHugger || !enemyHeartHugger.enemy.EnemyParent.Spawned || enemyHeartHugger.enemy.Health.dead)
		{
			enemyHeartHugger.RemovePlayerFromGas(player);
			Object.Destroy(base.gameObject);
			return;
		}
		enemyHeartHugger.PlayerInGas(player);
		float num = 1.5f;
		Vector3 normalized = new Vector3(headTransform.forward.x, 0f, headTransform.forward.z).normalized;
		Vector3 vector = headTransform.position + normalized * num;
		float num2 = Vector3.Distance(base.transform.position, targetTransform.position);
		base.transform.position = Vector3.Lerp(startPosition, vector, moveAlongEval);
		playerTumble.TumbleOverrideTime(1f);
		if (moveAlongEval < 1f)
		{
			moveAlongEval += Time.deltaTime * 0.15f;
		}
		else
		{
			moveAlongEval = 1f;
		}
		if (num2 > 2f)
		{
			targetOutOfRangeTimer += Time.deltaTime;
		}
		else
		{
			targetOutOfRangeTimer = 0f;
		}
		if (targetOutOfRangeTimer > 1.5f)
		{
			startPosition = targetTransform.position;
			moveAlongEval = 0f;
			tries++;
		}
		bringPlayerInTimer += Time.deltaTime;
		if (Vector3.Distance(targetTransform.position, vector) < 0.5f && enemyHeartHugger.currentState == EnemyHeartHugger.State.Lure)
		{
			enemyHeartHugger.StateSet(EnemyHeartHugger.State.ChompGasp);
			bringPlayerInTimer = 0f;
		}
		bool flag = enemyHeartHugger.currentState == EnemyHeartHugger.State.Lure || enemyHeartHugger.currentState == EnemyHeartHugger.State.ChompGasp || enemyHeartHugger.currentState == EnemyHeartHugger.State.Chomp;
		if (bringPlayerInTimer > 8f || tries > 2 || !enemyHeartHugger.isShootingGas || !flag)
		{
			enemyHeartHugger.RemovePlayerFromGas(player);
			Object.Destroy(base.gameObject);
		}
	}

	private void FixedUpdate()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!playerTumble.isTumbling)
			{
				playerTumble.TumbleRequest(_isTumbling: true, _playerInput: false);
				return;
			}
			Rigidbody rb = playerTumble.rb;
			Vector3 normalized = (enemyHeartHugger.headCenterTransform.position - startPosition).normalized;
			Vector3 position = playerTumble.rb.position;
			Vector3 position2 = base.transform.position;
			physGrabObject.OverrideZeroGravity();
			playerTumble.TumbleOverrideTime(2f);
			Vector3 vector = SemiFunc.PhysFollowDirection(rb.transform, normalized, rb, 0.5f);
			rb.AddTorque(vector / rb.mass, ForceMode.Force);
			Vector3 vector2 = SemiFunc.PhysFollowPosition(position, position2, rb.velocity, 5f);
			rb.AddForce(vector2 * 1f, ForceMode.Acceleration);
		}
	}
}
