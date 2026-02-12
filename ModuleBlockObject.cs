using UnityEngine;

public class ModuleBlockObject : MonoBehaviour
{
	private void Start()
	{
		if (!base.transform.parent)
		{
			base.transform.parent = LevelGenerator.Instance.LevelParent.transform;
		}
	}
}
