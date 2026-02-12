using UnityEngine;

public class TruckLevelNumberScreen : MonoBehaviour
{
	private ArenaPedistalScreen arenaPedistalScreen;

	private void Start()
	{
		arenaPedistalScreen = GetComponent<ArenaPedistalScreen>();
		arenaPedistalScreen.SwitchNumber(RunManager.instance.levelsCompleted + 1);
	}
}
