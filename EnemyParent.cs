using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class EnemyParent : MonoBehaviourPunCallbacks, IPunObservable
{
	public enum Difficulty
	{
		Difficulty1,
		Difficulty2,
		Difficulty3
	}

	public string enemyName = "Dinosaur";

	internal bool SetupDone;

	internal bool Spawned = true;

	internal Enemy Enemy;

	[Space]
	public Difficulty difficulty;

	[Space]
	public float actionMultiplier = 1f;

	public float overchargeMultiplier = 1f;

	[Space]
	public GameObject EnableObject;

	[Space]
	public float SpawnedTimeMin;

	public float SpawnedTimeMax;

	[Space]
	public float DespawnedTimeMin;

	public float DespawnedTimeMax;

	[Space]
	public float SpawnedTimer;

	public float DespawnedTimer;

	private float spawnedTimerPauseTimer;

	private float valuableSpawnTimer;

	internal bool playerClose;

	internal bool playerVeryClose;

	internal bool forceLeave;

	internal List<RoomVolume> currentRooms = new List<RoomVolume>();

	internal LevelPoint firstSpawnPoint;

	internal bool firstSpawnPointUsed;

	private void Awake()
	{
		base.transform.parent = LevelGenerator.Instance.EnemyParent.transform;
		Enemy = GetComponentInChildren<Enemy>();
		if (EnemyDirector.instance.debugEnemy != null)
		{
			if (EnemyDirector.instance.debugEnemyEnableTime > 0f)
			{
				SpawnedTimeMax = EnemyDirector.instance.debugEnemyEnableTime;
				SpawnedTimeMin = SpawnedTimeMax;
			}
			if (EnemyDirector.instance.debugEnemyDisableTime > 0f)
			{
				DespawnedTimeMax = EnemyDirector.instance.debugEnemyDisableTime;
				DespawnedTimeMin = DespawnedTimeMax;
			}
		}
		StartCoroutine(Setup());
	}

	private void Update()
	{
		if (SemiFunc.FPSImpulse1())
		{
			GetRoomVolume();
		}
	}

	private IEnumerator Setup()
	{
		while (!SetupDone)
		{
			yield return new WaitForSeconds(0.1f);
		}
		LevelGenerator.Instance.EnemiesSpawned++;
		EnemyDirector.instance.enemiesSpawned.Add(this);
		if (LevelGenerator.Instance.EnemiesSpawned >= LevelGenerator.Instance.EnemiesSpawnTarget)
		{
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
				{
					item?.Enemy.PlayerAdded(player.photonView.ViewID);
				}
			}
			if (GameManager.Multiplayer() && !LevelGenerator.Instance.EnemyReadyPlayerList.Contains(PhotonNetwork.LocalPlayer))
			{
				LevelGenerator.Instance.PhotonView.RPC("EnemyReadyRPC", RpcTarget.All);
			}
		}
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			if (Enemy.HasRigidbody)
			{
				float y = Enemy.Rigidbody.transform.localPosition.y - Enemy.transform.localPosition.y;
				Vector3 vector = new Vector3(0f, y, 0f);
				Vector3 position = Enemy.transform.position + vector;
				Enemy.Rigidbody.rb.position = position;
				Enemy.Rigidbody.rb.rotation = Enemy.Rigidbody.followTarget.rotation;
				Enemy.Rigidbody.physGrabObject.Teleport(position, Enemy.Rigidbody.followTarget.rotation);
				Enemy.Rigidbody.physGrabObject.spawned = true;
				Enemy.Rigidbody.rb.isKinematic = false;
			}
			StartCoroutine(Logic());
			StartCoroutine(PlayerCloseLogic());
		}
	}

	private IEnumerator Logic()
	{
		Despawn();
		DespawnedTimer = Random.Range(2f, 5f);
		while (true)
		{
			if (Spawned)
			{
				if (SpawnedTimer <= 0f)
				{
					if (!playerClose || EnemyDirector.instance.debugDespawnClose)
					{
						Enemy.CurrentState = EnemyState.Despawn;
					}
				}
				else if (spawnedTimerPauseTimer > 0f)
				{
					spawnedTimerPauseTimer -= Time.deltaTime;
				}
				else if (!playerClose || EnemyDirector.instance.debugDespawnClose)
				{
					SpawnedTimer -= Time.deltaTime;
				}
			}
			else if (DespawnedTimer <= 0f)
			{
				Spawn();
			}
			else
			{
				DespawnedTimer -= Time.deltaTime;
			}
			yield return null;
		}
	}

	private IEnumerator PlayerCloseLogic()
	{
		while (true)
		{
			bool flag = false;
			float num = 20f;
			bool flag2 = false;
			float num2 = 6f;
			if (Spawned)
			{
				foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
				{
					if (!item.isDisabled)
					{
						Vector3 vector = new Vector3(item.transform.position.x, 0f, item.transform.position.z);
						Vector3 vector2 = new Vector3(Enemy.transform.position.x, 0f, Enemy.transform.position.z);
						float num3 = Vector3.Distance(vector, vector2);
						if (num3 <= num2)
						{
							flag2 = true;
							flag = true;
							break;
						}
						if (num3 <= num)
						{
							flag = true;
						}
					}
				}
				if (flag)
				{
					EnemyDirector.instance.spawnIdlePauseTimer = 0f;
				}
				foreach (PlayerDeathSpot playerDeathSpot in GameDirector.instance.PlayerDeathSpots)
				{
					Vector3 vector3 = new Vector3(playerDeathSpot.transform.position.x, 0f, playerDeathSpot.transform.position.z);
					Vector3 vector4 = new Vector3(Enemy.transform.position.x, 0f, Enemy.transform.position.z);
					float num4 = Vector3.Distance(vector3, vector4);
					if (num4 <= num2)
					{
						flag2 = true;
						flag = true;
						break;
					}
					if (num4 <= num)
					{
						flag = true;
					}
				}
			}
			playerClose = flag;
			playerVeryClose = flag2;
			if (flag)
			{
				valuableSpawnTimer = 10f;
			}
			else if (valuableSpawnTimer > 0f)
			{
				valuableSpawnTimer -= 1f;
			}
			yield return new WaitForSeconds(1f);
		}
	}

	public void DisableDecrease(float _time)
	{
		DespawnedTimer -= _time;
	}

	public void SpawnedTimerSet(float _time)
	{
		if (Spawned)
		{
			SpawnedTimer = _time;
			if (_time == 0f && SemiFunc.IsMasterClientOrSingleplayer())
			{
				Enemy.CurrentState = EnemyState.Despawn;
			}
		}
	}

	public void DespawnedTimerSet(float _time, bool _min = false)
	{
		if (!Spawned)
		{
			if (!_min)
			{
				DespawnedTimer = _time;
			}
			else
			{
				DespawnedTimer = Mathf.Min(DespawnedTimer, _time);
			}
		}
	}

	public void SpawnedTimerReset()
	{
		if (Spawned)
		{
			SpawnedTimer = Random.Range(SpawnedTimeMin, SpawnedTimeMax);
			if (Enemy.CurrentState == EnemyState.Despawn)
			{
				Enemy.CurrentState = EnemyState.Roaming;
			}
		}
	}

	public void SpawnedTimerPause(float _time)
	{
		if (_time == 0f)
		{
			spawnedTimerPauseTimer = 0f;
		}
		else
		{
			spawnedTimerPauseTimer = Mathf.Max(spawnedTimerPauseTimer, _time);
		}
	}

	public void GetRoomVolume()
	{
		currentRooms.Clear();
		Collider[] array = Physics.OverlapBox(Enemy.CenterTransform.position, Vector3.one / 2f, base.transform.rotation, LayerMask.GetMask("RoomVolume"));
		foreach (Collider collider in array)
		{
			RoomVolume roomVolume = collider.transform.GetComponent<RoomVolume>();
			if (!roomVolume)
			{
				roomVolume = collider.transform.GetComponentInParent<RoomVolume>();
			}
			if (!currentRooms.Contains(roomVolume))
			{
				currentRooms.Add(roomVolume);
			}
		}
	}

	private void Spawn()
	{
		SpawnedTimer = Random.Range(SpawnedTimeMin, SpawnedTimeMax);
		Enemy.CurrentState = EnemyState.Spawn;
		if (GameManager.Multiplayer())
		{
			base.photonView.RPC("SpawnRPC", RpcTarget.All);
		}
		else
		{
			SpawnRPC();
		}
	}

	[PunRPC]
	private void SpawnRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			if (Enemy.HasHealth)
			{
				Enemy.Health.OnSpawn();
			}
			if (Enemy.HasStateStunned)
			{
				Enemy.StateStunned.Spawn();
			}
			if (Enemy.HasJump)
			{
				Enemy.Jump.StuckReset();
			}
			Enemy.StuckCount = 0;
			Spawned = true;
			EnableObject.SetActive(value: true);
			Enemy.StateSpawn.OnSpawn.Invoke();
			Enemy.Spawn();
			if (!EnemyDirector.instance.debugNoSpawnedPause)
			{
				SpawnedTimerPause(Random.Range(3f, 4f) * 60f);
			}
			forceLeave = false;
		}
	}

	public void Despawn()
	{
		if (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		Enemy.CurrentState = EnemyState.Despawn;
		DespawnedTimer = Random.Range(DespawnedTimeMin, DespawnedTimeMax) * EnemyDirector.instance.despawnedDecreaseMultiplier * EnemyDirector.instance.despawnedTimeMultiplier;
		DespawnedTimer = Mathf.Max(DespawnedTimer, 1f);
		if (Enemy.HasRigidbody)
		{
			Enemy.Rigidbody.grabbed = false;
			Enemy.Rigidbody.grabStrengthTimer = 0f;
			Enemy.Rigidbody.GrabRelease();
		}
		if (GameManager.Multiplayer())
		{
			base.photonView.RPC("DespawnRPC", RpcTarget.All);
		}
		else
		{
			DespawnRPC();
		}
		if (!Enemy.HasHealth || Enemy.Health.healthCurrent > 0)
		{
			return;
		}
		if (Enemy.Health.spawnValuable && valuableSpawnTimer > 0f && Enemy.Health.spawnValuableCurrent < Enemy.Health.spawnValuableMax)
		{
			GameObject gameObject = AssetManager.instance.enemyValuableSmall;
			if (difficulty == Difficulty.Difficulty2)
			{
				gameObject = AssetManager.instance.enemyValuableMedium;
			}
			else if (difficulty == Difficulty.Difficulty3)
			{
				gameObject = AssetManager.instance.enemyValuableBig;
			}
			Transform transform = Enemy.CustomValuableSpawnTransform;
			if (!transform)
			{
				transform = Enemy.CenterTransform;
			}
			if (!SemiFunc.IsMultiplayer())
			{
				Object.Instantiate(gameObject, transform.position, Quaternion.identity);
			}
			else
			{
				PhotonNetwork.InstantiateRoomObject("Valuables/" + gameObject.name, transform.position, Quaternion.identity, 0);
			}
			Enemy.Health.spawnValuableCurrent++;
		}
		DespawnedTimer *= 3f;
	}

	[PunRPC]
	private void DespawnRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			Spawned = false;
			EnableObject.SetActive(value: false);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(SetupDone);
			}
			else
			{
				SetupDone = (bool)stream.ReceiveNext();
			}
		}
	}
}
