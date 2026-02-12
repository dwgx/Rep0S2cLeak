using UnityEngine;

public class CleanSpotIdentifier : MonoBehaviour
{
	public Interaction.InteractionType InteractionType;

	private void Start()
	{
		CleanDirector.instance.CleanList.Add(base.gameObject);
	}

	private void Update()
	{
	}
}
