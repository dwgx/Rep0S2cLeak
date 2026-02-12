using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyDirector : MonoBehaviour
{
	public enum ExtractionsDoneState
	{
		StartRoom,
		PlayerRoom
	}

	internal ExtractionsDoneState extractionsDoneState;

	private float extractionDoneStateTimer;

	private bool extractionDoneStateImpulse = true;

	public static EnemyDirector instance;

	internal bool debugNoVision;

	internal EnemySetup[] debugEnemy;

	internal float debugEnemyEnableTime;

	internal float debugEnemyDisableTime;

	internal bool debugEasyGrab;

	internal bool debugSpawnClose;

	internal bool debugDespawnClose;

	internal bool debugInvestigate;

	internal bool debugShortActionTimer;

	internal bool debugNoSpawnedPause;

	internal bool debugNoSpawnIdlePause;

	internal bool debugNoGrabMaxTime;

	public List<EnemySetup> enemiesDifficulty1;

	public List<EnemySetup> enemiesDifficulty2;

	public List<EnemySetup> enemiesDifficulty3;

	[Space]
	public AnimationCurve spawnIdlePauseCurve;

	[Space]
	public AnimationCurve amountCurve1_1;

	public AnimationCurve amountCurve1_2;

	private int amountCurve1Value;

	[Space]
	public AnimationCurve amountCurve2_1;

	public AnimationCurve amountCurve2_2;

	private int amountCurve2Value;

	[Space]
	public AnimationCurve amountCurve3_1;

	public AnimationCurve amountCurve3_2;

	private int amountCurve3Value;

	internal int totalAmount;

	private List<EnemySetup> enemyList = new List<EnemySetup>();

	private List<EnemySetup> enemyListCurrent = new List<EnemySetup>();

	private int enemyListIndex;

	[Space]
	public AnimationCurve despawnTimeCurve_1;

	public AnimationCurve despawnTimeCurve_2;

	internal float despawnedTimeMultiplier = 1f;

	public float despawnedDecreaseMinutes;

	public float despawnedDecreasePercent;

	internal float despawnedDecreaseMultiplier = 1f;

	private float despawnedDecreaseTimer;

	private float investigatePointTimer;

	private float investigatePointTime = 3f;

	internal float enemyActionAmount;

	internal float spawnIdlePauseTimer;

	[Space]
	public List<EnemyParent> enemiesSpawned;

	internal List<EnemyValuable> enemyValuables = new List<EnemyValuable>();

	internal List<LevelPoint> enemyFirstSpawnPoints = new List<LevelPoint>();

	private void Awake()
	{
		instance = this;
		despawnedDecreaseTimer = 60f * despawnedDecreaseMinutes;
	}

	private void Start()
	{
		spawnIdlePauseTimer = 60f * Random.Range(2f, 3f) * spawnIdlePauseCurve.Evaluate(SemiFunc.RunGetDifficultyMultiplier1());
		if (Random.Range(0, 100) < 20)
		{
			spawnIdlePauseTimer *= Random.Range(0.1f, 0.25f);
		}
		spawnIdlePauseTimer = Mathf.Max(spawnIdlePauseTimer, 5f);
		if ((bool)DebugCommandHandler.instance && DebugCommandHandler.instance.enemyNoVision)
		{
			if (SemiFunc.IsMainMenu())
			{
				DebugCommandHandler.instance.enemyNoVision = false;
			}
			else
			{
				debugNoVision = true;
			}
		}
	}

	private void Update()
	{
		if (LevelGenerator.Instance.Generated && spawnIdlePauseTimer > 0f)
		{
			bool flag = true;
			foreach (EnemyParent item in enemiesSpawned)
			{
				if ((bool)item && !item.firstSpawnPointUsed)
				{
					flag = false;
				}
			}
			if (flag)
			{
				spawnIdlePauseTimer -= Time.deltaTime;
			}
			if (debugNoSpawnIdlePause)
			{
				spawnIdlePauseTimer = 0f;
			}
		}
		despawnedDecreaseTimer -= Time.deltaTime;
		if (despawnedDecreaseTimer <= 0f)
		{
			despawnedDecreaseMultiplier -= despawnedDecreasePercent;
			if (despawnedDecreaseMultiplier < 0f)
			{
				despawnedDecreaseMultiplier = 0f;
			}
			despawnedDecreaseTimer = 60f * despawnedDecreaseMinutes;
		}
		if (RoundDirector.instance.allExtractionPointsCompleted)
		{
			foreach (EnemyParent item2 in enemiesSpawned)
			{
				if ((bool)item2 && item2.DespawnedTimer > 30f)
				{
					item2.DespawnedTimerSet(0f);
				}
			}
			if (investigatePointTimer <= 0f)
			{
				if (extractionsDoneState == ExtractionsDoneState.StartRoom)
				{
					enemyActionAmount = 0f;
					despawnedDecreaseMultiplier = 0f;
					if (extractionDoneStateImpulse)
					{
						extractionDoneStateTimer = 10f;
						extractionDoneStateImpulse = false;
						foreach (EnemyParent item3 in enemiesSpawned)
						{
							if ((bool)item3 && item3.Spawned && !item3.playerClose)
							{
								item3.SpawnedTimerPause(0f);
								item3.SpawnedTimerSet(0f);
							}
						}
					}
					investigatePointTimer = investigatePointTime;
					List<LevelPoint> list = SemiFunc.LevelPointsGetInStartRoom();
					if (list.Count > 0)
					{
						SemiFunc.EnemyInvestigate(list[Random.Range(0, list.Count)].transform.position, 100f, pathfindOnly: true);
					}
					extractionDoneStateTimer -= investigatePointTime;
					if (extractionDoneStateTimer <= 0f)
					{
						extractionsDoneState = ExtractionsDoneState.PlayerRoom;
					}
				}
				else
				{
					List<LevelPoint> list2 = SemiFunc.LevelPointsGetInPlayerRooms();
					if (list2.Count > 0)
					{
						SemiFunc.EnemyInvestigate(list2[Random.Range(0, list2.Count)].transform.position, 100f, pathfindOnly: true);
					}
					investigatePointTimer = investigatePointTime;
					investigatePointTime = Mathf.Min(investigatePointTime + 2f, 30f);
				}
			}
			else
			{
				investigatePointTimer -= Time.deltaTime;
			}
		}
		float num = 0f;
		foreach (EnemyParent item4 in enemiesSpawned)
		{
			if (!item4 || !item4.Spawned || !item4.playerClose || item4.forceLeave)
			{
				continue;
			}
			bool flag2 = false;
			foreach (PlayerAvatar item5 in SemiFunc.PlayerGetList())
			{
				foreach (RoomVolume currentRoom in item5.RoomVolumeCheck.CurrentRooms)
				{
					foreach (RoomVolume currentRoom2 in item4.currentRooms)
					{
						if (currentRoom == currentRoom2)
						{
							flag2 = true;
							break;
						}
					}
				}
			}
			if (flag2)
			{
				float num2 = 0f;
				num2 = ((item4.difficulty == EnemyParent.Difficulty.Difficulty3) ? (num2 + 2f) : ((item4.difficulty != EnemyParent.Difficulty.Difficulty2) ? (num2 + 0.5f) : (num2 + 1f)));
				num += num2 * item4.actionMultiplier;
			}
		}
		if (num > 0f)
		{
			enemyActionAmount += num * Time.deltaTime;
		}
		else
		{
			enemyActionAmount -= 0.1f * Time.deltaTime;
			enemyActionAmount = Mathf.Max(0f, enemyActionAmount);
		}
		float num3 = 120f;
		if (debugShortActionTimer)
		{
			num3 = 5f;
		}
		if (!(enemyActionAmount > num3))
		{
			return;
		}
		enemyActionAmount = 0f;
		LevelPoint levelPoint = SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f);
		if ((bool)levelPoint)
		{
			SetInvestigate(levelPoint.transform.position, float.MaxValue, pathfindOnly: true);
		}
		if (RoundDirector.instance.allExtractionPointsCompleted && extractionsDoneState == ExtractionsDoneState.PlayerRoom)
		{
			investigatePointTimer = 60f;
		}
		foreach (EnemyParent item6 in enemiesSpawned)
		{
			if ((bool)item6 && item6.Spawned)
			{
				item6.forceLeave = true;
			}
		}
	}

	public void AmountSetup()
	{
		if (SemiFunc.RunGetDifficultyMultiplier2() > 0f)
		{
			amountCurve3Value = (int)amountCurve3_2.Evaluate(SemiFunc.RunGetDifficultyMultiplier2());
			amountCurve2Value = (int)amountCurve2_2.Evaluate(SemiFunc.RunGetDifficultyMultiplier2());
			amountCurve1Value = (int)amountCurve1_2.Evaluate(SemiFunc.RunGetDifficultyMultiplier2());
		}
		else
		{
			amountCurve3Value = (int)amountCurve3_1.Evaluate(SemiFunc.RunGetDifficultyMultiplier1());
			amountCurve2Value = (int)amountCurve2_1.Evaluate(SemiFunc.RunGetDifficultyMultiplier1());
			amountCurve1Value = (int)amountCurve1_1.Evaluate(SemiFunc.RunGetDifficultyMultiplier1());
		}
		enemyListCurrent.Clear();
		for (int i = 0; i < amountCurve3Value; i++)
		{
			PickEnemies(enemiesDifficulty3);
		}
		for (int j = 0; j < amountCurve2Value; j++)
		{
			PickEnemies(enemiesDifficulty2);
		}
		for (int k = 0; k < amountCurve1Value; k++)
		{
			PickEnemies(enemiesDifficulty1);
		}
		if (SemiFunc.RunGetDifficultyMultiplier3() > 0f)
		{
			despawnedTimeMultiplier = despawnTimeCurve_2.Evaluate(SemiFunc.RunGetDifficultyMultiplier3());
		}
		else if (SemiFunc.RunGetDifficultyMultiplier2() > 0f)
		{
			despawnedTimeMultiplier = despawnTimeCurve_1.Evaluate(SemiFunc.RunGetDifficultyMultiplier2());
		}
		else
		{
			despawnedTimeMultiplier = 1f;
		}
		totalAmount = amountCurve1Value + amountCurve2Value + amountCurve3Value;
	}

	private void PickEnemies(List<EnemySetup> _enemiesList)
	{
		int num = DataDirector.instance.SettingValueFetch(DataDirector.Setting.RunsPlayed);
		_enemiesList.Shuffle();
		EnemySetup item = null;
		float num2 = -1f;
		foreach (EnemySetup _enemies in _enemiesList)
		{
			if ((_enemies.levelsCompletedCondition && (RunManager.instance.levelsCompleted < _enemies.levelsCompletedMin || (_enemies.levelsCompletedMax != 0 && RunManager.instance.levelsCompleted > _enemies.levelsCompletedMax))) || num < _enemies.runsPlayed)
			{
				continue;
			}
			int num3 = 0;
			foreach (EnemySetup item2 in RunManager.instance.enemiesSpawned)
			{
				if (item2 == _enemies)
				{
					num3++;
				}
			}
			int num4 = 0;
			foreach (EnemySetup enemy in enemyList)
			{
				if (enemy == _enemies)
				{
					num4++;
				}
			}
			float num5 = 100f;
			if ((bool)_enemies.rarityPreset)
			{
				num5 = _enemies.rarityPreset.chance;
			}
			float maxInclusive = Mathf.Max(1f, num5 - 30f * (float)num3 - 10f * (float)num4);
			float num6 = Random.Range(0f, maxInclusive);
			if (num6 > num2)
			{
				item = _enemies;
				num2 = num6;
			}
		}
		enemyListCurrent.Add(item);
		enemyList.Add(item);
	}

	public EnemySetup GetEnemy()
	{
		EnemySetup enemySetup = enemyList[enemyListIndex];
		enemyListIndex++;
		int num = 0;
		foreach (EnemySetup item in RunManager.instance.enemiesSpawned)
		{
			if (item == enemySetup)
			{
				num++;
			}
		}
		int num2 = 2;
		while (num < 4 && num2 > 0)
		{
			RunManager.instance.enemiesSpawned.Add(enemySetup);
			num++;
			num2--;
		}
		return enemySetup;
	}

	public void FirstSpawnPointAdd(EnemyParent _enemyParent)
	{
		List<LevelPoint> list = (from x in SemiFunc.LevelPointsGetAll()
			where !x.Truck
			select x).ToList();
		float num = 0f;
		LevelPoint levelPoint = null;
		foreach (LevelPoint item in list)
		{
			float num2 = Vector3.Distance(item.transform.position, LevelGenerator.Instance.LevelPathTruck.transform.position);
			foreach (LevelPoint enemyFirstSpawnPoint in enemyFirstSpawnPoints)
			{
				if (enemyFirstSpawnPoint == item)
				{
					num2 = 0f;
					break;
				}
			}
			if (num2 > num)
			{
				num = num2;
				levelPoint = item;
			}
		}
		if ((bool)levelPoint)
		{
			_enemyParent.firstSpawnPoint = levelPoint;
			enemyFirstSpawnPoints.Add(levelPoint);
		}
		if (enemyFirstSpawnPoints.Count >= list.Count)
		{
			enemyFirstSpawnPoints.Clear();
		}
	}

	public void DebugResult()
	{
	}

	public void SetInvestigate(Vector3 position, float radius, bool pathfindOnly = false)
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (debugInvestigate)
		{
			Object.Instantiate(AssetManager.instance.debugEnemyInvestigate, position, Quaternion.identity).GetComponent<DebugEnemyInvestigate>().radius = radius;
		}
		foreach (EnemyParent item in enemiesSpawned)
		{
			if (!item)
			{
				continue;
			}
			if (!item.Spawned)
			{
				if (radius >= 15f)
				{
					item.DisableDecrease(5f);
				}
			}
			else if (item.Enemy.HasStateInvestigate && Vector3.Distance(position, item.Enemy.transform.position) / item.Enemy.StateInvestigate.rangeMultiplier < radius)
			{
				item.Enemy.StateInvestigate.Set(position, pathfindOnly);
			}
		}
	}

	public void AddEnemyValuable(EnemyValuable _newValuable)
	{
		List<EnemyValuable> list = new List<EnemyValuable>();
		foreach (EnemyValuable enemyValuable2 in enemyValuables)
		{
			if (!enemyValuable2)
			{
				list.Add(enemyValuable2);
			}
		}
		foreach (EnemyValuable item in list)
		{
			enemyValuables.Remove(item);
		}
		enemyValuables.Add(_newValuable);
		if (enemyValuables.Count > 10)
		{
			EnemyValuable enemyValuable = enemyValuables[0];
			enemyValuables.RemoveAt(0);
			enemyValuable.Destroy();
		}
	}
}
