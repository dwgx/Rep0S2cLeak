using System;
using UnityEngine;

[Serializable]
public class SpringQuaternionSystem
{
	public SpringQuaternion spring;

	public Transform target;

	public Transform transform;

	public void UpdateWorldSpace()
	{
		transform.rotation = SemiFunc.SpringQuaternionGet(spring, target.rotation);
	}

	public void UpdateLocalSpace()
	{
		transform.localRotation = SemiFunc.SpringQuaternionGet(spring, target.localRotation);
	}
}
