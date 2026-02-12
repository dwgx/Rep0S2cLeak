using UnityEngine;

public class PlayerCrawlTrigger : MonoBehaviour
{
	private float timer;

	private void Update()
	{
		if (timer <= 0f)
		{
			timer = 0.2f;
			if (!LevelGenerator.Instance.Generated || !PlayerController.instance || !PlayerController.instance.playerAvatarScript)
			{
				return;
			}
			Collider[] array = Physics.OverlapSphere(PlayerController.instance.playerAvatarScript.PlayerVisionTarget.VisionTransform.position, 0.5f, LayerMask.GetMask("PlayerOnlyCollision"), QueryTriggerInteraction.Collide);
			for (int i = 0; i < array.Length; i++)
			{
				if ((bool)array[i].GetComponent<CrawlTrigger>())
				{
					PlayerCollisionStand.instance.SetBlocked();
				}
			}
		}
		else
		{
			timer -= Time.deltaTime;
		}
	}
}
