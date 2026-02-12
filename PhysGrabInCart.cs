using System.Collections.Generic;
using UnityEngine;

public class PhysGrabInCart : MonoBehaviour
{
	public class CartObject
	{
		public PhysGrabObject physGrabObject;

		public float timer;
	}

	public PhysGrabCart cart;

	internal List<CartObject> inCartObjects = new List<CartObject>();

	private void Update()
	{
		for (int i = 0; i < inCartObjects.Count; i++)
		{
			CartObject cartObject = inCartObjects[i];
			cartObject.timer -= Time.deltaTime;
			if (cartObject.timer <= 0f || !cartObject.physGrabObject)
			{
				inCartObjects.RemoveAt(i);
				i--;
			}
		}
	}

	public void Add(PhysGrabObject _physGrabObject)
	{
		foreach (CartObject inCartObject in inCartObjects)
		{
			if (inCartObject.physGrabObject == _physGrabObject)
			{
				inCartObject.timer = 1f;
				return;
			}
		}
		CartObject cartObject = new CartObject();
		cartObject.physGrabObject = _physGrabObject;
		cartObject.timer = 1f;
		inCartObjects.Add(cartObject);
	}
}
