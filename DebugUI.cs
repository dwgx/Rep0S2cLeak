using UnityEngine;

public class DebugUI : MonoBehaviour
{
	public static DebugUI instance;

	public GameObject enableParent;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		if (!Application.isEditor)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
		if (SemiFunc.DebugDev() && Input.GetKeyDown(KeyCode.F1))
		{
			enableParent.SetActive(!enableParent.activeSelf);
		}
	}
}
