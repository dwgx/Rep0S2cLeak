using UnityEngine;

public class ItemDeactivatedUntilLevel : MonoBehaviour
{
	public int levelToActivate;

	private void Start()
	{
		if (SemiFunc.RunGetLevelsCompleted() < levelToActivate)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
