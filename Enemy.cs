using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class Enemy : MonoBehaviourPunCallbacks, IPunObservable
{
	internal PhotonView PhotonView;

	internal EnemyParent EnemyParent;

	internal bool MasterClient;

	public EnemyType Type = EnemyType.Medium;

	[Space]
	public EnemyState CurrentState;

	private EnemyState PreviousState;

	private int CurrentStateIndex;

	[Space]
	public Transform CenterTransform;

	public Transform KillLookAtTransform;

	public Transform CustomValuableSpawnTransform;

	internal LayerMask VisionMask;

	private Vector3 PositionTarget;

	private float PositionDistance;

	internal int StuckCount;

	internal EnemyVision Vision;

	internal bool HasVision;

	internal EnemyPlayerDistance PlayerDistance;

	internal bool HasPlayerDistance;

	internal EnemyOnScreen OnScreen;

	internal bool HasOnScreen;

	internal EnemyPlayerRoom PlayerRoom;

	internal bool HasPlayerRoom;

	internal EnemyRigidbody Rigidbody;

	internal bool HasRigidbody;

	internal EnemyNavMeshAgent NavMeshAgent;

	internal bool HasNavMeshAgent;

	internal EnemyAttackStuckPhysObject AttackStuckPhysObject;

	internal bool HasAttackPhysObject;

	internal EnemyStateInvestigate StateInvestigate;

	internal bool HasStateInvestigate;

	internal EnemyStateChaseBegin StateChaseBegin;

	internal bool HasStateChaseBegin;

	internal EnemyStateChase StateChase;

	internal bool HasStateChase;

	internal EnemyStateLookUnder StateLookUnder;

	internal bool HasStateLookUnder;

	internal EnemyStateDespawn StateDespawn;

	internal bool HasStateDespawn;

	internal EnemyStateSpawn StateSpawn;

	internal bool HasStateSpawn;

	private bool Stunned;

	internal EnemyStateStunned StateStunned;

	internal bool HasStateStunned;

	internal EnemyGrounded Grounded;

	internal bool HasGrounded;

	internal EnemyJump Jump;

	internal bool HasJump;

	internal EnemyHealth Health;

	internal bool HasHealth;

	internal PlayerAvatar TargetPlayerAvatar;

	internal int TargetPlayerViewID;

	protected internal float TeleportedTimer;

	protected internal Vector3 TeleportPosition;

	private EnemyType initialType;

	private float overrideTypeTimer;

	[HideInInspector]
	public float FreezeTimer;

	private float ChaseTimer;

	internal float DisableChaseTimer;

	private PhotonTransformView photonTransformView;

	[Space]
	public bool SightingStinger;

	public bool EnemyNearMusic;

	internal Vector3 moveDirection;

	private void Awake()
	{
		photonTransformView = base.transform.parent.GetComponentInChildren<PhotonTransformView>();
		EnemyParent = GetComponentInParent<EnemyParent>();
		PhotonView = GetComponent<PhotonView>();
		Vision = GetComponent<EnemyVision>();
		if ((bool)Vision)
		{
			HasVision = true;
		}
		VisionMask = (int)SemiFunc.LayerMaskGetVisionObstruct() + LayerMask.GetMask("HideTriggers");
		PlayerDistance = GetComponent<EnemyPlayerDistance>();
		if ((bool)PlayerDistance)
		{
			HasPlayerDistance = true;
		}
		OnScreen = GetComponent<EnemyOnScreen>();
		if ((bool)OnScreen)
		{
			HasOnScreen = true;
		}
		PlayerRoom = GetComponent<EnemyPlayerRoom>();
		if ((bool)PlayerRoom)
		{
			HasPlayerRoom = true;
		}
		NavMeshAgent = GetComponent<EnemyNavMeshAgent>();
		if ((bool)NavMeshAgent)
		{
			HasNavMeshAgent = true;
		}
		AttackStuckPhysObject = GetComponent<EnemyAttackStuckPhysObject>();
		if ((bool)AttackStuckPhysObject)
		{
			HasAttackPhysObject = true;
		}
		StateInvestigate = GetComponent<EnemyStateInvestigate>();
		if ((bool)StateInvestigate)
		{
			HasStateInvestigate = true;
		}
		StateChaseBegin = GetComponent<EnemyStateChaseBegin>();
		if ((bool)StateChaseBegin)
		{
			HasStateChaseBegin = true;
		}
		StateChase = GetComponent<EnemyStateChase>();
		if ((bool)StateChase)
		{
			HasStateChase = true;
		}
		StateLookUnder = GetComponent<EnemyStateLookUnder>();
		if ((bool)StateLookUnder)
		{
			HasStateLookUnder = true;
		}
		StateDespawn = GetComponent<EnemyStateDespawn>();
		if ((bool)StateDespawn)
		{
			HasStateDespawn = true;
		}
		StateSpawn = GetComponent<EnemyStateSpawn>();
		if ((bool)StateSpawn)
		{
			HasStateSpawn = true;
		}
		StateStunned = GetComponent<EnemyStateStunned>();
		if ((bool)StateStunned)
		{
			HasStateStunned = true;
		}
		Health = GetComponent<EnemyHealth>();
		if ((bool)Health)
		{
			HasHealth = true;
		}
		if (!CenterTransform)
		{
			Debug.LogError("Center Transform not set in " + base.gameObject.name, base.gameObject);
		}
		initialType = Type;
	}

	private void Start()
	{
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			MasterClient = true;
		}
		else
		{
			MasterClient = false;
		}
	}

	private void Update()
	{
		if (SemiFunc.IsMultiplayer() && !MasterClient)
		{
			float num = 1f / (float)PhotonNetwork.SerializationRate;
			float num2 = PositionDistance / num;
			moveDirection = (PositionTarget - base.transform.position).normalized;
			base.transform.position = Vector3.MoveTowards(base.transform.position, PositionTarget, num2 * Time.deltaTime);
		}
		if (MasterClient)
		{
			Stunned = false;
			if (HasStateStunned && StateStunned.stunTimer > 0f)
			{
				Stunned = true;
			}
		}
		if (overrideTypeTimer > 0f)
		{
			overrideTypeTimer -= Time.deltaTime;
			if (overrideTypeTimer <= 0f)
			{
				Type = initialType;
			}
		}
		if (FreezeTimer > 0f)
		{
			FreezeTimer -= Time.deltaTime;
		}
		if (TeleportedTimer > 0f)
		{
			StuckCount = 0;
			TeleportedTimer -= Time.deltaTime;
		}
		if (ChaseTimer > 0f)
		{
			ChaseTimer -= Time.deltaTime;
		}
		if (DisableChaseTimer > 0f)
		{
			DisableChaseTimer -= Time.deltaTime;
		}
	}

	public void Spawn()
	{
		Stunned = false;
		FreezeTimer = 0f;
	}

	public bool IsStunned()
	{
		return Stunned;
	}

	public void DisableChase(float time)
	{
		DisableChaseTimer = time;
	}

	public void SetChaseTimer()
	{
		ChaseTimer = 0.1f;
	}

	public bool CheckChase()
	{
		return ChaseTimer > 0f;
	}

	public void SetChaseTarget(PlayerAvatar playerAvatar)
	{
		if (!EnemyDirector.instance.debugNoVision && !(DisableChaseTimer > 0f) && HasVision && !playerAvatar.isDisabled && !(Vision.DisableTimer > 0f))
		{
			Vision.VisionTrigger(playerAvatar.photonView.ViewID, playerAvatar, culled: false, playerNear: false);
			if (HasStateChase && (!CheckChase() || CurrentState == EnemyState.ChaseSlow))
			{
				CurrentState = EnemyState.ChaseBegin;
				TargetPlayerViewID = playerAvatar.photonView.ViewID;
				TargetPlayerAvatar = playerAvatar;
			}
		}
	}

	public LevelPoint TeleportToPoint(float minDistance, float maxDistance)
	{
		LevelPoint levelPoint = null;
		if (!EnemyParent.firstSpawnPointUsed)
		{
			levelPoint = EnemyParent.firstSpawnPoint;
		}
		else
		{
			if (RoundDirector.instance.allExtractionPointsCompleted)
			{
				levelPoint = SemiFunc.LevelPointGetPlayerDistance(base.transform.position, minDistance, maxDistance, _startRoomOnly: true);
			}
			if (!levelPoint)
			{
				levelPoint = SemiFunc.LevelPointGetPlayerDistance(base.transform.position, minDistance, maxDistance);
			}
		}
		if ((bool)levelPoint)
		{
			TeleportPosition = new Vector3(levelPoint.transform.position.x, levelPoint.transform.position.y, levelPoint.transform.position.z);
			EnemyTeleported(TeleportPosition);
		}
		return levelPoint;
	}

	public LevelPoint GetLevelPointAhead(Vector3 currentTargetPosition)
	{
		LevelPoint result = null;
		Vector3 normalized = (currentTargetPosition - base.transform.position).normalized;
		LevelPoint levelPoint = null;
		float num = 1000f;
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			if ((bool)levelPathPoint)
			{
				float num2 = Vector3.Distance(levelPathPoint.transform.position, currentTargetPosition);
				if (num2 < num)
				{
					num = num2;
					levelPoint = levelPathPoint;
				}
			}
		}
		if (!levelPoint)
		{
			return null;
		}
		float num3 = -1f;
		foreach (LevelPoint connectedPoint in levelPoint.ConnectedPoints)
		{
			if ((bool)connectedPoint)
			{
				Vector3 normalized2 = (connectedPoint.transform.position - levelPoint.transform.position).normalized;
				float num4 = Vector3.Dot(normalized, normalized2);
				if (num4 > num3)
				{
					num3 = num4;
					result = connectedPoint;
				}
			}
		}
		return result;
	}

	public void Freeze(float time)
	{
		if (GameManager.instance.gameMode == 0)
		{
			FreezeRPC(time);
			return;
		}
		base.photonView.RPC("FreezeRPC", RpcTarget.All, time);
	}

	[PunRPC]
	public void FreezeRPC(float time, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			FreezeTimer = time;
		}
	}

	public void PlayerAdded(int photonID)
	{
		if (HasVision)
		{
			Vision.PlayerAdded(photonID);
		}
		if (HasOnScreen)
		{
			OnScreen.PlayerAdded(photonID);
		}
	}

	public void PlayerRemoved(int photonID)
	{
		if (StateChaseBegin != null && StateChaseBegin.TargetPlayer != null && StateChaseBegin.TargetPlayer.photonView.ViewID == photonID)
		{
			StateChaseBegin.TargetPlayer = null;
			CurrentState = EnemyState.Roaming;
		}
		if (TargetPlayerAvatar != null && TargetPlayerAvatar.photonView.ViewID == photonID)
		{
			TargetPlayerAvatar = PlayerController.instance.playerAvatarScript;
			TargetPlayerViewID = TargetPlayerAvatar.photonView.ViewID;
		}
		if (HasVision)
		{
			Vision.PlayerRemoved(photonID);
		}
		if (HasOnScreen)
		{
			OnScreen.PlayerRemoved(photonID);
		}
	}

	public void EnemyTeleported(Vector3 teleportPosition)
	{
		base.transform.position = teleportPosition;
		if (HasNavMeshAgent)
		{
			NavMeshAgent.Warp(teleportPosition);
		}
		if (HasRigidbody)
		{
			Rigidbody.Teleport();
		}
		if (GameManager.instance.gameMode == 0)
		{
			EnemyTeleportedRPC(teleportPosition);
			return;
		}
		base.photonView.RPC("EnemyTeleportedRPC", RpcTarget.All, teleportPosition);
	}

	[PunRPC]
	private void EnemyTeleportedRPC(Vector3 teleportPosition, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			PositionDistance = 0f;
			PositionTarget = teleportPosition;
			TeleportPosition = teleportPosition;
			base.transform.position = teleportPosition;
			TeleportedTimer = 1f;
		}
	}

	public void OverrideType(EnemyType _type, float _time)
	{
		Type = _type;
		overrideTypeTimer = _time;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!SemiFunc.MasterOnlyRPC(info))
		{
			return;
		}
		if (stream.IsWriting)
		{
			stream.SendNext(base.transform.position);
			stream.SendNext(CurrentState);
			stream.SendNext(TargetPlayerViewID);
			stream.SendNext(Stunned);
			return;
		}
		PositionTarget = (Vector3)stream.ReceiveNext();
		PositionDistance = Vector3.Distance(base.transform.position, PositionTarget);
		CurrentState = (EnemyState)stream.ReceiveNext();
		TargetPlayerViewID = (int)stream.ReceiveNext();
		Stunned = (bool)stream.ReceiveNext();
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (!player.isDisabled && player.photonView.ViewID == TargetPlayerViewID)
			{
				TargetPlayerAvatar = player;
				break;
			}
		}
	}
}
