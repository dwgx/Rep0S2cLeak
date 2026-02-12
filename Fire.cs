using UnityEngine;

public class Fire : MonoBehaviour
{
	public PropLight propLight;

	[Space]
	public Sound soundHit;

	private void Update()
	{
		if (propLight.turnedOff)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void OnHit()
	{
		soundHit.Play(base.transform.position);
	}
}
