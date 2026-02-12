using System.Collections;
using UnityEngine;

public class Interaction : MonoBehaviour
{
	public enum InteractionType
	{
		None,
		VacuumCleaner,
		Duster,
		Sledgehammer,
		DirtFinder,
		Picker
	}

	public InteractionType Type;

	public Sprite Sprite;

	private void Start()
	{
		StartCoroutine(Add());
	}

	private IEnumerator Add()
	{
		while (!CleanDirector.instance.RemoveExcessSpots)
		{
			yield return new WaitForSeconds(0.1f);
		}
	}
}
