using Photon.Pun;
using UnityEngine;

public class ValuableCocktail : Trap
{
	public Transform liquid;

	public Transform SemiPukeTransform;

	public Transform eye;

	public ParticleSystem eyeParticle;

	public Sound soundSpill;

	private bool isSpilled;

	private bool spill;

	private SemiPuke semiPuke;

	protected override void Start()
	{
		base.Start();
		semiPuke = GetComponentInChildren<SemiPuke>();
	}

	protected override void Update()
	{
		if (!isSpilled)
		{
			if (spill)
			{
				SpillEffects();
			}
			if (SemiFunc.IsMasterClientOrSingleplayer() && (Vector3.Dot(base.transform.up, Vector3.up) < 0.4f || (physGrabObject.rbVelocity.magnitude > 7f && !physGrabObject.impactDetector.inCart)) && !isSpilled)
			{
				SpillCocktail();
			}
		}
	}

	private void SpillEffects()
	{
		Quaternion direction = Quaternion.Lerp(Quaternion.LookRotation(base.transform.up), Quaternion.LookRotation(Vector3.down), 0.75f);
		semiPuke.PukeActive(SemiPukeTransform.position, direction);
		liquid.gameObject.SetActive(value: false);
		eye.gameObject.SetActive(value: false);
		eyeParticle.Play();
		soundSpill.Play(physGrabObject.centerPoint);
		physGrabObject.impactDetector.BreakLight(physGrabObject.centerPoint, _forceBreak: true);
		isSpilled = true;
	}

	private void SpillCocktail()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("SpillCocktailRPC", RpcTarget.All);
			}
			else
			{
				SpillCocktailRPC();
			}
		}
	}

	[PunRPC]
	private void SpillCocktailRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			spill = true;
		}
	}
}
