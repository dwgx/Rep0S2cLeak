using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ArachnophobiaSwitch : MonoBehaviour
{
	private bool active;

	public List<GameObject> activeObjects;

	public List<GameObject> inactiveObjects;

	[Space]
	public UnityEvent onToggleActive;

	public UnityEvent onToggleInactive;

	private void Start()
	{
		Set();
	}

	private void Update()
	{
		if (SemiFunc.FPSImpulse5() && active != SemiFunc.Arachnophobia())
		{
			Set();
		}
	}

	private void Set()
	{
		active = SemiFunc.Arachnophobia();
		if (active)
		{
			foreach (GameObject activeObject in activeObjects)
			{
				if ((bool)activeObject)
				{
					activeObject.SetActive(value: true);
				}
			}
			foreach (GameObject inactiveObject in inactiveObjects)
			{
				if ((bool)inactiveObject)
				{
					inactiveObject.SetActive(value: false);
				}
			}
			onToggleActive?.Invoke();
			return;
		}
		foreach (GameObject activeObject2 in activeObjects)
		{
			if ((bool)activeObject2)
			{
				activeObject2.SetActive(value: false);
			}
		}
		foreach (GameObject inactiveObject2 in inactiveObjects)
		{
			if ((bool)inactiveObject2)
			{
				inactiveObject2.SetActive(value: true);
			}
		}
		onToggleInactive?.Invoke();
	}
}
