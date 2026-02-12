using System.Collections.Generic;
using UnityEngine;

public class PlayerNameChecker : MonoBehaviour
{
	private float checkTimer;

	private void Update()
	{
		if (GameDirector.instance.currentState != GameDirector.gameState.Main || ((bool)Map.Instance && Map.Instance.Active) || !GameplayManager.instance.playerNames)
		{
			return;
		}
		if (checkTimer <= 0f)
		{
			checkTimer = 0.25f;
			List<PlayerAvatar> list = new List<PlayerAvatar>();
			Camera main = Camera.main;
			RaycastHit[] array = Physics.SphereCastAll(main.transform.position, 0.25f, main.transform.forward, 15f, LayerMask.GetMask("PlayerVisuals"), QueryTriggerInteraction.Collide);
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit raycastHit = array[i];
				PlayerAvatarVisuals componentInParent = raycastHit.collider.GetComponentInParent<PlayerAvatarVisuals>();
				if (!componentInParent)
				{
					continue;
				}
				PlayerAvatar playerAvatar = componentInParent.playerAvatar;
				if (!list.Contains(playerAvatar) && !playerAvatar.isLocal)
				{
					Vector3 direction = main.transform.position - raycastHit.point;
					if (!Physics.Raycast(raycastHit.point, direction, out var _, direction.magnitude, (int)SemiFunc.LayerMaskGetVisionObstruct() - LayerMask.GetMask("Player"), QueryTriggerInteraction.Collide))
					{
						playerAvatar.worldSpaceUIPlayerName.Show();
						list.Add(playerAvatar);
					}
				}
			}
		}
		else
		{
			checkTimer -= Time.deltaTime;
		}
	}
}
