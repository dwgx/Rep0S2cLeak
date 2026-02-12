using System.Collections.Generic;
using UnityEngine;

public class EnemyHeartHuggerGasChecker : MonoBehaviour
{
	internal EnemyHeartHugger enemyHeartHugger;

	internal Vector3 velocity;

	internal float lifeTimeCurrent;

	internal float lifeTimeMax;

	public AnimationCurve scaleOverLifeTime;

	private float perlinSetting1;

	private float perlinSetting2;

	internal float scaleTarget;

	private List<PlayerAvatar> playersColliding = new List<PlayerAvatar>();

	private float checkTimer;

	private Vector3 prevCheckPos;

	public GameObject gasGuider;

	private void Start()
	{
		prevCheckPos = base.transform.position;
		perlinSetting1 = Random.Range(0.2f, 3f);
		perlinSetting2 = Random.Range(0.5f, 4f);
	}

	private void Update()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || !enemyHeartHugger || !enemyHeartHugger.enemy.EnemyParent.Spawned || enemyHeartHugger.enemy.Health.dead)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		float num = Mathf.PerlinNoise(Time.time * perlinSetting1, perlinSetting2);
		base.transform.position += velocity * Time.deltaTime * (1f + num * 0.5f);
		if (lifeTimeCurrent < lifeTimeMax)
		{
			float time = lifeTimeCurrent / lifeTimeMax;
			float num2 = 1f;
			if (velocity.magnitude <= 0f)
			{
				num2 = 5f;
			}
			lifeTimeCurrent += Time.deltaTime * num2;
			base.transform.localScale = new Vector3(scaleTarget, scaleTarget, scaleTarget) * scaleOverLifeTime.Evaluate(time);
			foreach (PlayerAvatar item in playersColliding)
			{
				if ((bool)item)
				{
					enemyHeartHugger.PlayerInGas(item);
				}
			}
			if (checkTimer <= 0f)
			{
				checkTimer = 1f;
				playersColliding.Clear();
				float num3 = base.transform.localScale.z * 0.5f;
				Vector3 normalized = (base.transform.position - prevCheckPos).normalized;
				float maxDistance = Vector3.Distance(prevCheckPos, base.transform.position);
				if (Physics.SphereCast(prevCheckPos, num3 * 0.75f, normalized, out var hitInfo, maxDistance, LayerMask.GetMask("Default")))
				{
					velocity = Vector3.zero;
					base.transform.position = hitInfo.point + normalized * num3;
				}
				RaycastHit[] array = Physics.SphereCastAll(prevCheckPos, num3, normalized, maxDistance, LayerMask.GetMask("Player", "PhysGrabObject"));
				for (int i = 0; i < array.Length; i++)
				{
					RaycastHit raycastHit = array[i];
					PlayerController componentInParent = raycastHit.collider.GetComponentInParent<PlayerController>();
					PlayerAvatar playerAvatar;
					if ((bool)componentInParent)
					{
						playerAvatar = componentInParent.playerAvatarScript;
					}
					else
					{
						playerAvatar = raycastHit.collider.GetComponentInParent<PlayerAvatar>();
						if (!playerAvatar)
						{
							PlayerTumble componentInParent2 = raycastHit.collider.GetComponentInParent<PlayerTumble>();
							if ((bool)componentInParent2)
							{
								playerAvatar = componentInParent2.playerAvatar;
							}
						}
					}
					if ((bool)playerAvatar && !enemyHeartHugger.PlayerIsOnCooldown(playerAvatar) && !enemyHeartHugger.PlayerInGasCheck(playerAvatar))
					{
						Vector3 direction = playerAvatar.PlayerVisionTarget.VisionTransform.position - enemyHeartHugger.enemy.Vision.VisionTransform.position;
						if (!Physics.Raycast(enemyHeartHugger.enemy.Vision.VisionTransform.position, direction, direction.magnitude, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
						{
							playerAvatar.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
							playersColliding.Add(playerAvatar);
							enemyHeartHugger.PlayerInGas(playerAvatar);
							playerAvatar.UpgradeTumbleWingsVisualsActive(_visualsActive: true, _pink: true);
							GameObject obj = Object.Instantiate(gasGuider, base.transform.position, Quaternion.identity);
							EnemyHeartHuggerGasGuider component = obj.GetComponent<EnemyHeartHuggerGasGuider>();
							component.playerTumble = playerAvatar.tumble;
							component.targetTransform = playerAvatar.transform;
							component.enemyHeartHugger = enemyHeartHugger;
							component.headTransform = enemyHeartHugger.headCenterTransform;
							component.startPosition = raycastHit.point;
							component.physGrabObject = playerAvatar.tumble.physGrabObject;
							component.player = playerAvatar;
							obj.SetActive(value: true);
						}
					}
				}
				prevCheckPos = base.transform.position;
			}
			else
			{
				checkTimer -= Time.deltaTime;
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
