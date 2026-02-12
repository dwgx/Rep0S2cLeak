using Photon.Pun;
using UnityEngine;

public class FireExtinguisherValuable : MonoBehaviour
{
	public enum States
	{
		Full,
		Empty
	}

	public SemiZuperFlames semiFlames;

	public Transform triggerMesh;

	private PhysGrabObject physGrabObject;

	public Transform Center;

	private Vector3 triggerMeshInitialEulerAngles;

	private bool triggerStuck;

	private float triggerStuckTimer = 0.2f;

	public Sound soundFlameEmpty;

	public float fuelTimer;

	private bool fuelCountdownActive;

	private PhotonView photonView;

	private ParticleScriptExplosion particleScriptExplosion;

	public ParticleSystem flameEndSquirt;

	public ParticleSystem flameEndSparks;

	internal States currentState;

	private void Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
		photonView = GetComponent<PhotonView>();
		triggerMeshInitialEulerAngles = triggerMesh.localEulerAngles;
		particleScriptExplosion = GetComponent<ParticleScriptExplosion>();
	}

	private void Update()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (fuelCountdownActive)
		{
			fuelTimer -= Time.deltaTime;
			if (fuelTimer <= 0f)
			{
				fuelTimer = 0f;
				ReleaseTrigger();
				fuelCountdownActive = false;
				SetState(States.Empty);
			}
		}
		if (triggerStuck)
		{
			triggerStuckTimer -= Time.deltaTime;
			if (triggerStuckTimer <= 0f)
			{
				triggerStuckTimer = 0f;
				ReleaseTrigger();
				triggerStuck = false;
			}
		}
	}

	private void GrabTriggerLogic()
	{
		SetTriggerMeshPosition(pulled: true);
		if (currentState == States.Empty)
		{
			soundFlameEmpty.Play(semiFlames.transform.position);
			flameEndSquirt.Play();
			flameEndSparks.Play();
		}
		else
		{
			EnemyDirector.instance.SetInvestigate(base.transform.position, 5f);
			semiFlames.FlamesActive(semiFlames.transform.position, semiFlames.transform.rotation);
			fuelCountdownActive = true;
		}
	}

	public void GrabTrigger()
	{
		if (GameManager.instance.gameMode == 0)
		{
			GrabTriggerLogic();
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("GrabTriggerRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void GrabTriggerRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			GrabTriggerLogic();
		}
	}

	private void ReleaseTriggerLogic()
	{
		SetTriggerMeshPosition(pulled: false);
		if (currentState == States.Empty)
		{
			flameEndSparks.Stop();
			return;
		}
		semiFlames.FlamesInactive();
		fuelCountdownActive = false;
	}

	public void ReleaseTrigger()
	{
		if (GameManager.instance.gameMode == 0)
		{
			ReleaseTriggerLogic();
		}
		else if (PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("ReleaseTriggerRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void ReleaseTriggerRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			ReleaseTriggerLogic();
		}
	}

	[PunRPC]
	public void SetStateRPC(States state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = state;
		}
	}

	private void SetState(States state)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				SetStateRPC(state);
				return;
			}
			photonView.RPC("SetStateRPC", RpcTarget.All, state);
		}
	}

	public void SetTriggerMeshPosition(bool pulled)
	{
		if (!triggerStuck)
		{
			if (pulled)
			{
				Vector3 localEulerAngles = new Vector3(triggerMeshInitialEulerAngles.x, triggerMeshInitialEulerAngles.y, triggerMeshInitialEulerAngles.z - 40f);
				triggerMesh.localEulerAngles = localEulerAngles;
			}
			else
			{
				triggerMesh.localEulerAngles = triggerMeshInitialEulerAngles;
			}
		}
	}

	public void TriggerStuck()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && physGrabObject.playerGrabbing.Count <= 0)
		{
			triggerStuckTimer = 0.2f;
			triggerStuck = true;
			GrabTrigger();
		}
	}

	public void Explode()
	{
		particleScriptExplosion.Spawn(Center.position, 0.2f, 10, 20);
	}
}
