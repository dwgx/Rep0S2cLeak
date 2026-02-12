using UnityEngine;

public class ValuableForeverCandle : MonoBehaviour
{
	public Sound soundHit;

	public void OnHit()
	{
		soundHit.Play(base.transform.position);
	}
}
