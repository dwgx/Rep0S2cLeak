using UnityEngine;

public class PlayerCrownSet : MonoBehaviour
{
	public static PlayerCrownSet instance;

	internal bool crownOwnerFetched;

	internal string crownOwnerSteamID;

	private void Start()
	{
		if (!instance)
		{
			instance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
