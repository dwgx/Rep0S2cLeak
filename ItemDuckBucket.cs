using System.Collections;
using UnityEngine;

public class ItemDuckBucket : MonoBehaviour
{
	public SphereCollider sphereCollider;

	private bool active;

	private bool activePrevious;

	private EnemyDuck enemyDuck;

	private EnemyElsa enemyElsa;

	private PlayerAvatar playerAvatar;

	public GameObject lowPassParent;

	private void Start()
	{
		StartCoroutine(DuckFinder());
	}

	private IEnumerator DuckFinder()
	{
		while (true)
		{
			active = false;
			enemyDuck = null;
			enemyElsa = null;
			playerAvatar = null;
			Collider[] array = Physics.OverlapSphere(sphereCollider.transform.position, sphereCollider.radius, LayerMask.GetMask("PhysGrabObject"));
			foreach (Collider collider in array)
			{
				if (collider.gameObject.name == "Duck Collider")
				{
					EnemyRigidbody componentInParent = collider.GetComponentInParent<EnemyRigidbody>();
					if ((bool)componentInParent)
					{
						EnemyDuck component = componentInParent.enemy.GetComponent<EnemyDuck>();
						if ((bool)component)
						{
							enemyDuck = component;
							active = true;
							break;
						}
					}
				}
				if (!(collider.gameObject.name == "Small Elsa - Collider"))
				{
					continue;
				}
				EnemyRigidbody componentInParent2 = collider.GetComponentInParent<EnemyRigidbody>();
				if ((bool)componentInParent2)
				{
					EnemyElsa component2 = componentInParent2.enemy.GetComponent<EnemyElsa>();
					if ((bool)component2)
					{
						enemyElsa = component2;
						active = true;
						break;
					}
				}
			}
			array = Physics.OverlapSphere(sphereCollider.transform.position, sphereCollider.radius, SemiFunc.LayerMaskGetPlayersAndPhysObjects());
			foreach (Collider collider2 in array)
			{
				if (collider2.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
				{
					PlayerController componentInParent3 = collider2.transform.GetComponentInParent<PlayerController>();
					if ((bool)componentInParent3)
					{
						playerAvatar = componentInParent3.playerAvatarScript;
					}
					else
					{
						playerAvatar = collider2.transform.GetComponentInParent<PlayerAvatar>();
					}
					active = true;
					break;
				}
				PlayerTumble componentInParent4 = collider2.transform.GetComponentInParent<PlayerTumble>();
				if ((bool)componentInParent4)
				{
					playerAvatar = componentInParent4.playerAvatar;
					active = true;
					break;
				}
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private void Update()
	{
		if (active)
		{
			if ((bool)enemyDuck)
			{
				enemyDuck.DuckBucketActive();
			}
			else if ((bool)enemyElsa)
			{
				enemyElsa.DuckBucketActive();
			}
			else if ((bool)playerAvatar && playerAvatar.isLocal)
			{
				PlayerController.instance.OverrideJumpCooldown(0.5f);
			}
		}
		if (active != activePrevious)
		{
			activePrevious = active;
			if (active)
			{
				lowPassParent.SetActive(value: true);
			}
			else
			{
				lowPassParent.SetActive(value: false);
			}
		}
	}
}
