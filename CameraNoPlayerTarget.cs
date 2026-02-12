using UnityEngine;

public class CameraNoPlayerTarget : MonoBehaviour
{
	public static CameraNoPlayerTarget instance;

	internal Camera cam;

	protected virtual void Awake()
	{
		instance = this;
		cam = GetComponent<Camera>();
		cam.enabled = false;
	}

	protected virtual void Update()
	{
		SemiFunc.UIHideAim();
		SemiFunc.UIHideCurrency();
		SemiFunc.UIHideEnergy();
		SemiFunc.UIHideGoal();
		SemiFunc.UIHideHaul();
		SemiFunc.UIHideHealth();
		SemiFunc.UIHideOvercharge();
		SemiFunc.UIHideInventory();
		SemiFunc.UIHideShopCost();
	}
}
