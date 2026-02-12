using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Arena : MonoBehaviour
{
	public enum States
	{
		Idle,
		Level1,
		Level2,
		Level3,
		Falling,
		Starting,
		PlatformWarning,
		PlatformRemove,
		GameOver
	}

	private PhotonView photonView;

	private List<ArenaPlatform> platforms;

	public static Arena instance;

	[Space(10f)]
	[Header("Items")]
	public List<Item> itemsUsables;

	public List<Item> itemsMelee;

	public List<Item> itemsGuns;

	public List<Item> itemsCarts;

	public List<Item> itemsDronesAndOrbs;

	public List<Item> itemsHealth;

	private List<Item> itemsMid;

	[Space(10f)]
	public Transform floorDoorTransform;

	public GameObject hurtCollider;

	public GameObject startStuff;

	public Transform pipeTransform;

	public GameObject safeBars;

	public AnimationCurve floorDoorCurve;

	public Transform itemVolumes;

	public MeshRenderer crownSphere;

	public MeshRenderer crownMesh;

	public Transform crownTransform;

	public GameObject crownPlatform;

	public Transform crownMechanic;

	public Transform crownMechanicLineTransform;

	public GameObject crownCageDestroyParticles;

	public StaticGrabObject crownGrab;

	public GameObject crownExplosion;

	public Transform itemsMidSpawner;

	private Vector3 crownTransformPosition;

	private bool stateStart;

	private float stateTimer;

	private Vector3 floorDoorStartPos;

	private Vector3 floorDoorEndPos;

	private float floorDoorAnimationProgress;

	private float startPosCrownMechanicLineTransform;

	private float midSpawnerTimer;

	private List<ArenaPedistalScreen> pedistalScreens;

	internal PlayerAvatar winnerPlayer;

	internal States currentState;

	internal States nextLevel = States.Level1;

	private int level;

	private bool warningSound;

	private AudioClip soundWarning;

	private bool finalPlatform;

	private bool crownCageDestroyed;

	private int numberOfPlayers;

	private bool musicToggle;

	private bool musicTogglePrev;

	public AudioSource musicSource;

	private int playersAlive = 6;

	private int playersAlivePrev = 6;

	public GameObject pipeLights;

	public Light arenaMainLight;

	public Sound soundArenaStart;

	public Sound soundArenaWarning;

	public Sound soundArenaRemove;

	public Sound soundArenaPlayerEliminated;

	public Sound soundArenaHatchOpen;

	public Sound soundArenaMusic;

	public Sound soundArenaMusicWinJingle;

	public Sound soundArenaMusicLoseJingle;

	public Sound soundCrownCageDestroy;

	public Sound soundAllPlayersDead;

	private void Awake()
	{
		instance = this;
		ArenaInit();
		SessionManager.instance.ResetCrown();
	}

	private void ArenaInit()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.IsMultiplayer())
		{
			ArenaInitMultiplayer();
		}
	}

	private void ArenaInitMultiplayer()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < itemVolumes.childCount; i++)
		{
			list.Add(i);
		}
		for (int j = 0; j < itemVolumes.childCount; j++)
		{
			int index = Random.Range(0, list.Count);
			itemVolumes.GetChild(j).SetSiblingIndex(list[index]);
			list.RemoveAt(index);
		}
		itemsMelee.Shuffle();
		itemsGuns.Shuffle();
		itemsCarts.Shuffle();
		itemsDronesAndOrbs.Shuffle();
		itemsHealth.Shuffle();
		itemsUsables.Shuffle();
		itemsMid = new List<Item>();
		itemsMid.AddRange(itemsMelee);
		itemsMid.AddRange(itemsGuns);
		ItemManager.instance.ResetAllItems();
		for (int k = 0; k < 1; k++)
		{
			ItemManager.instance.purchasedItems.Add(itemsUsables[k]);
		}
		for (int l = 0; l < 5; l++)
		{
			ItemManager.instance.purchasedItems.Add(itemsMelee[l]);
		}
		for (int m = 0; m < 3; m++)
		{
			if (Random.Range(0, 100) < 30)
			{
				ItemManager.instance.purchasedItems.Add(itemsGuns[m]);
			}
		}
		for (int n = 0; n < 1; n++)
		{
			if (Random.Range(0, 100) < 30)
			{
				ItemManager.instance.purchasedItems.Add(itemsCarts[n]);
			}
		}
		for (int num = 0; num < 3; num++)
		{
			if (Random.Range(0, 100) < 30)
			{
				ItemManager.instance.purchasedItems.Add(itemsDronesAndOrbs[num]);
			}
		}
		for (int num2 = 0; num2 < 3; num2++)
		{
			if (Random.Range(0, 100) < 30)
			{
				ItemManager.instance.purchasedItems.Add(itemsHealth[num2]);
			}
		}
		ItemManager.instance.GetAllItemVolumesInScene();
		PunManager.instance.TruckPopulateItemVolumes();
	}

	private void Start()
	{
		if ((bool)crownTransform)
		{
			crownTransformPosition = crownTransform.position;
		}
		else
		{
			crownTransformPosition = Vector3.zero;
		}
		numberOfPlayers = SemiFunc.PlayerGetAll().Count;
		photonView = GetComponent<PhotonView>();
		platforms = new List<ArenaPlatform>();
		platforms.AddRange(GetComponentsInChildren<ArenaPlatform>());
		floorDoorStartPos = floorDoorTransform.localPosition;
		floorDoorEndPos = new Vector3(floorDoorStartPos.x, floorDoorStartPos.y, 8.25f);
		startPosCrownMechanicLineTransform = crownMechanicLineTransform.localPosition.y;
		playersAlive = SemiFunc.PlayerGetAll().Count;
		playersAlivePrev = playersAlive;
		pedistalScreens = new List<ArenaPedistalScreen>();
		pedistalScreens.AddRange(GetComponentsInChildren<ArenaPedistalScreen>());
		foreach (ArenaPedistalScreen pedistalScreen in pedistalScreens)
		{
			pedistalScreen.SwitchNumber(playersAlive);
		}
	}

	private void StateIdle()
	{
		if (stateStart)
		{
			stateStart = false;
		}
	}

	private void StateFalling()
	{
		if (stateStart)
		{
			pipeLights.SetActive(value: true);
			stateStart = false;
			hurtCollider.SetActive(value: true);
			stateTimer = 5f;
			soundArenaHatchOpen.Play(floorDoorTransform.position);
			CameraGlitch.Instance.PlayLong();
			GameDirector.instance.CameraShake.Shake(4f, 0.25f);
			GameDirector.instance.CameraImpact.Shake(4f, 0.1f);
			musicToggle = true;
			musicSource.time = 0f;
			if (numberOfPlayers > 1 && SemiFunc.IsMultiplayer())
			{
				foreach (PlayerAvatar item in SemiFunc.PlayerGetAll())
				{
					item.playerHealth.InvincibleSet(5f);
				}
			}
			if (numberOfPlayers < 2 || !SemiFunc.IsMultiplayer())
			{
				foreach (ArenaPlatform platform in platforms)
				{
					platform.StateSet(ArenaPlatform.States.GoDown);
				}
			}
		}
		if (numberOfPlayers > 1 && SemiFunc.IsMultiplayer())
		{
			ArenaMessageUI.instance.ArenaText("LAST LOSER STANDING");
		}
		else
		{
			ArenaMessageUI.instance.ArenaText("GAME OVER");
		}
		if (SemiFunc.FPSImpulse5())
		{
			foreach (PlayerAvatar item2 in SemiFunc.PlayerGetAll())
			{
				item2.FallDamageResetSet(1f);
			}
		}
		floorDoorAnimationProgress += Time.deltaTime * 2f;
		if (floorDoorAnimationProgress >= 1f)
		{
			floorDoorAnimationProgress = 1f;
		}
		floorDoorTransform.localPosition = Vector3.Lerp(floorDoorStartPos, floorDoorEndPos, floorDoorCurve.Evaluate(floorDoorAnimationProgress));
		if (stateTimer <= 0f)
		{
			if (numberOfPlayers > 1 && SemiFunc.IsMultiplayer())
			{
				StateSet(States.Starting);
			}
			else
			{
				StateSet(States.GameOver);
			}
		}
	}

	private void StateStarting()
	{
		if (stateStart)
		{
			pipeLights.SetActive(value: false);
			stateStart = false;
			hurtCollider.SetActive(value: false);
			safeBars.SetActive(value: false);
			stateTimer = 2f;
		}
		if (stateTimer <= 0f)
		{
			StateSet(States.Level1);
		}
	}

	private void StatePlatformWarning()
	{
		if (!SemiFunc.IsMultiplayer())
		{
			return;
		}
		if (stateStart)
		{
			level++;
			nextLevel = (States)(level + 1);
			stateStart = false;
			int index = level - 1;
			platforms[index].StateSet(ArenaPlatform.States.Warning);
			platforms[index].PulsateLights();
			soundArenaWarning.Play(base.transform.position);
			GameDirector.instance.CameraShake.Shake(4f, 0.25f);
			GameDirector.instance.CameraImpact.Shake(4f, 0.1f);
			stateTimer = 3f;
		}
		if (stateTimer % 1f < 0.1f)
		{
			if (!warningSound)
			{
				warningSound = true;
				if (stateTimer > 1f)
				{
					soundArenaWarning.Play(base.transform.position);
					GameDirector.instance.CameraShake.Shake(2f, 0.25f);
					GameDirector.instance.CameraImpact.Shake(2f, 0.1f);
				}
				int index2 = level - 1;
				platforms[index2].PulsateLights();
			}
		}
		else
		{
			warningSound = false;
		}
		if (stateTimer <= 0f)
		{
			StateSet(States.PlatformRemove);
		}
	}

	private void StatePlatformRemove()
	{
		if (stateStart)
		{
			int index = level - 1;
			platforms[index].StateSet(ArenaPlatform.States.GoDown);
			stateStart = false;
			soundArenaRemove.Play(base.transform.position);
			GameDirector.instance.CameraShake.Shake(8f, 0.5f);
			GameDirector.instance.CameraImpact.Shake(8f, 0.1f);
			stateTimer = 3f;
		}
		if (stateTimer <= 0f && !finalPlatform)
		{
			StateSet(nextLevel);
		}
	}

	private void StateLevel1()
	{
		if (stateStart)
		{
			stateStart = false;
			stateTimer = 30f;
		}
		if (stateTimer <= 0f)
		{
			NextLevel();
		}
	}

	private void StateLevel2()
	{
		if (stateStart)
		{
			stateStart = false;
			stateTimer = 30f;
		}
		if (stateTimer <= 0f)
		{
			NextLevel();
		}
	}

	private void StateLevel3()
	{
		if (stateStart)
		{
			stateStart = false;
			stateTimer = 30f;
		}
		if (stateTimer <= 0f)
		{
			finalPlatform = true;
			NextLevel();
		}
	}

	private void StateGameOver()
	{
		if (stateStart)
		{
			musicToggle = false;
			stateStart = false;
			if (numberOfPlayers > 1 && SemiFunc.IsMultiplayer())
			{
				stateTimer = 10f;
			}
			else
			{
				stateTimer = 3f;
			}
			if (!winnerPlayer)
			{
				soundArenaMusicLoseJingle.Play(base.transform.position);
			}
		}
		if ((bool)winnerPlayer)
		{
			ArenaMessageWinUI.instance.ArenaText("KING OF THE LOSERS!", _kingCrowned: true);
			if ((bool)crownMesh)
			{
				crownMesh.enabled = false;
			}
		}
		else if (numberOfPlayers > 1 && SemiFunc.IsMultiplayer())
		{
			ArenaMessageWinUI.instance.ArenaText("EVERYONE'S A LOSER!");
		}
		if (stateTimer <= 0f && SemiFunc.IsMasterClientOrSingleplayer())
		{
			RunManager.instance.ChangeLevel(_completedLevel: false, _levelFailed: true);
		}
	}

	private void GameOver()
	{
		if (stateStart)
		{
			stateStart = false;
		}
	}

	private void NextLevel()
	{
		StateSet(States.PlatformWarning);
	}

	private void StateMachine()
	{
		switch (currentState)
		{
		case States.Idle:
			StateIdle();
			break;
		case States.Falling:
			StateFalling();
			break;
		case States.Starting:
			StateStarting();
			break;
		case States.PlatformWarning:
			StatePlatformWarning();
			break;
		case States.PlatformRemove:
			StatePlatformRemove();
			break;
		case States.Level1:
			StateLevel1();
			break;
		case States.Level2:
			StateLevel2();
			break;
		case States.Level3:
			StateLevel3();
			break;
		case States.GameOver:
			StateGameOver();
			break;
		}
	}

	private void Update()
	{
		StateMachine();
		if (stateTimer > 0f)
		{
			stateTimer -= Time.deltaTime;
		}
		SemiFunc.UIHideCurrency();
		SemiFunc.UIHideInventory();
		SemiFunc.UIHideHaul();
		SemiFunc.UIHideGoal();
		MusicLogic();
		if (numberOfPlayers > 1 && SemiFunc.IsMultiplayer())
		{
			CrownVisuals();
			CrownLogic();
			MainLightAnimation();
			SpawnMidWeapons();
		}
	}

	private void MusicLogic()
	{
		soundArenaMusic.PlayLoop(musicToggle, 20f, 2f);
		if (!musicTogglePrev && musicToggle)
		{
			soundArenaMusic.Source.time = 0f;
			musicTogglePrev = musicToggle;
		}
	}

	private void SpawnMidWeapons()
	{
		if (SemiFunc.IsMultiplayer() && SemiFunc.IsMasterClient() && playersAlive > 1 && level >= 2 && currentState != States.GameOver)
		{
			if (midSpawnerTimer > 5f)
			{
				PhotonNetwork.Instantiate(itemsMid[Random.Range(0, itemsMid.Count)].prefab.ResourcePath, itemsMidSpawner.position, itemsMidSpawner.rotation, 0);
				midSpawnerTimer = 0f;
			}
			else
			{
				midSpawnerTimer += Time.deltaTime;
			}
		}
	}

	private void MainLightAnimation()
	{
		if (arenaMainLight.enabled)
		{
			if (arenaMainLight.intensity > 0.05f)
			{
				arenaMainLight.intensity = Mathf.Lerp(arenaMainLight.intensity, 0f, Time.deltaTime * 2f);
			}
			else
			{
				arenaMainLight.enabled = false;
			}
		}
	}

	private void CrownLogic()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || currentState == States.GameOver)
		{
			return;
		}
		List<PlayerAvatar> list = SemiFunc.PlayerGetAll();
		int count = list.Count;
		playersAlive = count;
		foreach (PlayerAvatar item in list)
		{
			if (item.isDisabled)
			{
				playersAlive--;
			}
		}
		if (playersAlivePrev != playersAlive)
		{
			if ((playersAlive > 1 || playersAlive == 0) && SemiFunc.IsMultiplayer())
			{
				photonView.RPC("PlayerKilledRPC", RpcTarget.All, playersAlive);
			}
			playersAlivePrev = playersAlive;
		}
		if (playersAlive <= 0)
		{
			StateSet(States.GameOver);
		}
		else if (SemiFunc.FPSImpulse15() && !crownCageDestroyed && playersAlive < 2)
		{
			DestroyCrownCage();
			crownCageDestroyed = true;
		}
	}

	private void AllPlayersKilled()
	{
		soundAllPlayersDead.Play(base.transform.position);
		arenaMainLight.color = new Color(0f, 1f, 0f);
		arenaMainLight.enabled = true;
		arenaMainLight.intensity = 10f;
		foreach (ArenaPedistalScreen pedistalScreen in pedistalScreens)
		{
			pedistalScreen.SwitchNumber(1, finalPlayer: true);
		}
	}

	[PunRPC]
	private void PlayerKilledRPC(int _playersAlive, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		soundArenaPlayerEliminated.Play(base.transform.position);
		arenaMainLight.color = new Color(0.8f, 0.3f, 0f);
		arenaMainLight.enabled = true;
		arenaMainLight.intensity = 8f;
		playersAlive = _playersAlive;
		foreach (ArenaPedistalScreen pedistalScreen in pedistalScreens)
		{
			pedistalScreen.SwitchNumber(playersAlive);
		}
	}

	private void CrownVisuals()
	{
		if (currentState != States.GameOver && (bool)crownTransform)
		{
			crownTransform.Rotate(Vector3.up, Time.deltaTime * 50f);
			crownTransform.localRotation = Quaternion.Euler(crownTransform.localRotation.x, crownTransform.localRotation.y + Time.time * 50f, 20f * Mathf.Sin(Time.time * 2f));
			if (!crownCageDestroyed)
			{
				crownSphere.material.mainTextureOffset = new Vector2(0f, Time.time * 0.1f);
				crownMechanic.Rotate(Vector3.up, Time.deltaTime * 500f);
				float num = 0.05f;
				float num2 = 2f;
				crownMechanicLineTransform.localPosition = new Vector3(crownMechanicLineTransform.localPosition.x, startPosCrownMechanicLineTransform - num / 2f + num * Mathf.Sin(Time.time * num2), crownMechanicLineTransform.localPosition.z);
			}
		}
	}

	public void StateSet(States state)
	{
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (!SemiFunc.IsMultiplayer())
			{
				StateSetRPC(state);
				return;
			}
			photonView.RPC("StateSetRPC", RpcTarget.All, state);
		}
	}

	[PunRPC]
	public void StateSetRPC(States state, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			currentState = state;
			stateStart = true;
		}
	}

	private void DestroyCrownCage()
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("DestroyCrownCageRPC", RpcTarget.All);
		}
		else
		{
			DestroyCrownCageRPC();
		}
	}

	public void CrownGrab()
	{
		if (SemiFunc.IsMasterClient())
		{
			int viewID = crownGrab.playerGrabbing[0].photonView.ViewID;
			photonView.RPC("CrownGrabRPC", RpcTarget.All, viewID);
		}
	}

	[PunRPC]
	public void CrownGrabRPC(int photonViewID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info) && !winnerPlayer)
		{
			PhysGrabber component = PhotonView.Find(photonViewID).GetComponent<PhysGrabber>();
			winnerPlayer = component.playerAvatar;
			SessionManager.instance.crownedPlayerSteamID = winnerPlayer.steamID;
			ArenaMessageWinUI.instance.kingObject.GetComponent<MenuPlayerListed>().ForcePlayer(winnerPlayer);
			crownExplosion.SetActive(value: true);
			soundArenaMusicWinJingle.Play(base.transform.position);
		}
	}

	[PunRPC]
	public void DestroyCrownCageRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info) && !crownCageDestroyed)
		{
			musicToggle = false;
			AllPlayersKilled();
			soundCrownCageDestroy.Play(crownTransformPosition);
			GameDirector.instance.CameraShake.Shake(10f, 0.5f);
			GameDirector.instance.CameraImpact.Shake(10f, 0.1f);
			if ((bool)crownCageDestroyParticles)
			{
				crownCageDestroyParticles.SetActive(value: true);
			}
			if ((bool)crownSphere)
			{
				Object.Destroy(crownSphere.gameObject);
			}
			if ((bool)crownMechanic)
			{
				Object.Destroy(crownMechanic.gameObject);
			}
			crownCageDestroyed = true;
		}
	}

	public void OpenHatch()
	{
		StateSet(States.Falling);
	}
}
