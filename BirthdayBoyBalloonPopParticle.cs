using UnityEngine;

public class BirthdayBoyBalloonPopParticle : MonoBehaviour
{
	public ParticleSystem ps;

	public ParticleSystem ps2;

	public void PlayParticle()
	{
		GetComponent<ParticleSystem>().Play(withChildren: true);
	}

	public void ChangeParticleColor(Color _color)
	{
		ParticleSystem.MainModule main = ps.main;
		main.startColor = _color;
		ps.GetComponent<ParticleSystemRenderer>().material.SetColor("_BaseColor", _color);
		ParticleSystem.MainModule main2 = ps2.main;
		main2.startColor = _color;
		ps2.GetComponent<ParticleSystemRenderer>().material.SetColor("_BaseColor", _color);
	}
}
