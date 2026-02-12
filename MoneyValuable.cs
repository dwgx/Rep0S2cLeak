using UnityEngine;

public class MoneyValuable : MonoBehaviour
{
	public ParticleSystem moneyBurst;

	public void MoneyBurst()
	{
		moneyBurst.Play();
	}
}
