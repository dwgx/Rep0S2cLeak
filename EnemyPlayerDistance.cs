using System.Collections;
using UnityEngine;

public class EnemyPlayerDistance : MonoBehaviour
{
	private Enemy Enemy;

	public Transform CheckTransform;

	private bool LogicActive;

	internal float PlayerDistanceLocal = 1000f;

	internal float PlayerDistanceClosest = 1000f;

	private void Start()
	{
		Enemy = GetComponent<Enemy>();
		LogicActive = true;
		StartCoroutine(Logic());
	}

	private void OnDisable()
	{
		LogicActive = false;
		StopAllCoroutines();
	}

	private void OnEnable()
	{
		if (!LogicActive)
		{
			LogicActive = true;
			StartCoroutine(Logic());
		}
	}

	private IEnumerator Logic()
	{
		while (true)
		{
			PlayerDistanceLocal = 999f;
			PlayerDistanceClosest = 999f;
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				float num = Vector3.Distance(CheckTransform.position, player.PlayerVisionTarget.VisionTransform.position);
				if (player.isLocal)
				{
					PlayerDistanceLocal = num;
				}
				if (!player.isDisabled && num < PlayerDistanceClosest)
				{
					PlayerDistanceClosest = num;
				}
			}
			yield return new WaitForSeconds(0.25f);
		}
	}
}
