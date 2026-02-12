using System;
using UnityEngine;

public class GameEventObjectSwap : MonoBehaviour
{
	[Serializable]
	public class EventSwapConfig
	{
		public GameManager.GameEvents gameEvent;

		public bool hasObjectsToActivate;

		public GameObject[] objectsToActivate;

		[Header("Original Objects")]
		public bool hasObjectsToDeactivate;

		public GameObject[] objectsToDeactivate;
	}

	[Header("Event Swap Configurations")]
	public EventSwapConfig[] eventSwaps;

	private void Awake()
	{
		EventSwapConfig[] array = eventSwaps;
		foreach (EventSwapConfig eventSwapConfig in array)
		{
			if (eventSwapConfig == null)
			{
				continue;
			}
			if (GameManager.instance.currentGameEvent == eventSwapConfig.gameEvent)
			{
				GameObject[] objectsToDeactivate;
				if (eventSwapConfig.hasObjectsToDeactivate && eventSwapConfig.objectsToDeactivate != null)
				{
					objectsToDeactivate = eventSwapConfig.objectsToDeactivate;
					foreach (GameObject gameObject in objectsToDeactivate)
					{
						if ((bool)gameObject)
						{
							gameObject.SetActive(value: false);
						}
					}
				}
				if (!eventSwapConfig.hasObjectsToActivate || eventSwapConfig.objectsToActivate == null)
				{
					continue;
				}
				objectsToDeactivate = eventSwapConfig.objectsToActivate;
				foreach (GameObject gameObject2 in objectsToDeactivate)
				{
					if ((bool)gameObject2)
					{
						gameObject2.SetActive(value: true);
					}
				}
			}
			else
			{
				if (!eventSwapConfig.hasObjectsToActivate || eventSwapConfig.objectsToActivate == null)
				{
					continue;
				}
				GameObject[] objectsToDeactivate = eventSwapConfig.objectsToActivate;
				foreach (GameObject gameObject3 in objectsToDeactivate)
				{
					if ((bool)gameObject3)
					{
						gameObject3.SetActive(value: false);
					}
				}
			}
		}
	}
}
