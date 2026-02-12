using UnityEngine;

public class TruckSafetySpawnPoint : MonoBehaviour
{
	public static TruckSafetySpawnPoint instance;

	private void Awake()
	{
		instance = this;
	}
}
