using Photon.Pun;
using UnityEngine;

public class ItemMeleeInflatableHammer : MonoBehaviour
{
	private ParticleScriptExplosion particleScriptExplosion;

	private Transform explosionPosition;

	private PhotonView photonView;

	private void Start()
	{
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
		explosionPosition = base.transform.Find("Explosion Position");
		photonView = GetComponent<PhotonView>();
	}

	public void OnHit()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && Random.Range(0, 19) == 0)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("ExplosionRPC", RpcTarget.All);
			}
			else
			{
				ExplosionRPC();
			}
		}
	}

	[PunRPC]
	public void ExplosionRPC()
	{
		ParticlePrefabExplosion particlePrefabExplosion = particleScriptExplosion.Spawn(explosionPosition.position, 0.5f, 0, 250);
		particlePrefabExplosion.SkipHurtColliderSetup = true;
		particlePrefabExplosion.HurtCollider.playerDamage = 0;
		particlePrefabExplosion.HurtCollider.enemyDamage = 250;
		particlePrefabExplosion.HurtCollider.physImpact = HurtCollider.BreakImpact.Heavy;
		particlePrefabExplosion.HurtCollider.physHingeDestroy = true;
		particlePrefabExplosion.HurtCollider.playerTumbleForce = 30f;
		particlePrefabExplosion.HurtCollider.playerTumbleTorque = 50f;
	}
}
