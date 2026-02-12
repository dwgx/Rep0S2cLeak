using UnityEngine;

public class MenuButtonEsc : MonoBehaviour
{
	private Transform parentTransform;

	private void Start()
	{
		parentTransform = GetComponentInParent<MenuPage>().transform;
	}

	private void Update()
	{
	}
}
