using UnityEngine;

public class StartRoom : MonoBehaviour
{
	private void Start()
	{
		base.transform.parent = LevelGenerator.Instance.LevelParent.transform;
	}
}
