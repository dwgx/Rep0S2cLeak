using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ValuableDirector : MonoBehaviour
{
	public enum ValuableDebug
	{
		Normal,
		All,
		None
	}

	public static ValuableDirector instance;

	private PhotonView PhotonView;

	internal ValuableDebug valuableDebug;

	[HideInInspector]
	public bool setupComplete;

	[HideInInspector]
	public bool valuablesSpawned;

	internal int valuableSpawnPlayerReady;

	internal int valuableSpawnAmount;

	internal int valuableTargetAmount = -1;

	private List<Player> switchSetupPlayerReadyList = new List<Player>();

	internal int switchSetupPlayerReady;

	internal string resourcePath = "Valuables/";

	public AnimationCurve totalMaxValueCurve1;

	public AnimationCurve totalMaxValueCurve2;

	private float totalMaxValue;

	private float totalCurrentValue;

	private int totalMaxAmount = 50;

	[Space(20f)]
	public AnimationCurve tinyMaxAmountCurve1;

	public AnimationCurve tinyMaxAmountCurve2;

	public int tinyChance;

	private int tinyMaxAmount;

	internal string tinyPath = "01 Tiny";

	private List<PrefabRef> tinyValuables = new List<PrefabRef>();

	private List<ValuableVolume> tinyVolumes = new List<ValuableVolume>();

	[Space]
	public AnimationCurve smallMaxAmountCurve1;

	public AnimationCurve smallMaxAmountCurve2;

	public int smallChance;

	private int smallMaxAmount;

	internal string smallPath = "02 Small";

	private List<PrefabRef> smallValuables = new List<PrefabRef>();

	private List<ValuableVolume> smallVolumes = new List<ValuableVolume>();

	[Space]
	public AnimationCurve mediumMaxAmountCurve1;

	public AnimationCurve mediumMaxAmountCurve2;

	public int mediumChance;

	private int mediumMaxAmount;

	internal string mediumPath = "03 Medium";

	private List<PrefabRef> mediumValuables = new List<PrefabRef>();

	private List<ValuableVolume> mediumVolumes = new List<ValuableVolume>();

	[Space]
	public AnimationCurve bigMaxAmountCurve1;

	public AnimationCurve bigMaxAmountCurve2;

	public int bigChance;

	private int bigMaxAmount;

	internal string bigPath = "04 Big";

	private List<PrefabRef> bigValuables = new List<PrefabRef>();

	private List<ValuableVolume> bigVolumes = new List<ValuableVolume>();

	[Space]
	public AnimationCurve wideMaxAmountCurve1;

	public AnimationCurve wideMaxAmountCurve2;

	public int wideChance;

	private int wideMaxAmount;

	internal string widePath = "05 Wide";

	private List<PrefabRef> wideValuables = new List<PrefabRef>();

	private List<ValuableVolume> wideVolumes = new List<ValuableVolume>();

	[Space]
	public AnimationCurve tallMaxAmountCurve1;

	public AnimationCurve tallMaxAmountCurve2;

	public int tallChance;

	private int tallMaxAmount;

	internal string tallPath = "06 Tall";

	private List<PrefabRef> tallValuables = new List<PrefabRef>();

	private List<ValuableVolume> tallVolumes = new List<ValuableVolume>();

	[Space]
	public AnimationCurve veryTallMaxAmountCurve1;

	public AnimationCurve veryTallMaxAmountCurve2;

	public int veryTallChance;

	private int veryTallMaxAmount;

	internal string veryTallPath = "07 Very Tall";

	private List<PrefabRef> veryTallValuables = new List<PrefabRef>();

	private List<ValuableVolume> veryTallVolumes = new List<ValuableVolume>();

	[Space(20f)]
	public List<ValuableObject> valuableList = new List<ValuableObject>();

	private void Awake()
	{
		instance = this;
		PhotonView = GetComponent<PhotonView>();
	}

	private void Start()
	{
		if (GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient)
		{
			StartCoroutine(SetupClient());
		}
	}

	public IEnumerator SetupClient()
	{
		while (valuableTargetAmount == -1)
		{
			yield return new WaitForSeconds(0.1f);
		}
		while (valuableSpawnAmount < valuableTargetAmount)
		{
			yield return new WaitForSeconds(0.1f);
		}
		PhotonView.RPC("PlayerReadyRPC", RpcTarget.All);
	}

	public IEnumerator SetupHost()
	{
		if (SemiFunc.RunGetDifficultyMultiplier2() > 0f && !SemiFunc.RunIsArena())
		{
			float time = SemiFunc.RunGetDifficultyMultiplier2();
			totalMaxValue = Mathf.RoundToInt(totalMaxValueCurve2.Evaluate(time));
			tinyMaxAmount = Mathf.RoundToInt(tinyMaxAmountCurve2.Evaluate(time));
			smallMaxAmount = Mathf.RoundToInt(smallMaxAmountCurve2.Evaluate(time));
			mediumMaxAmount = Mathf.RoundToInt(mediumMaxAmountCurve2.Evaluate(time));
			bigMaxAmount = Mathf.RoundToInt(bigMaxAmountCurve2.Evaluate(time));
			wideMaxAmount = Mathf.RoundToInt(wideMaxAmountCurve2.Evaluate(time));
			tallMaxAmount = Mathf.RoundToInt(tallMaxAmountCurve2.Evaluate(time));
			veryTallMaxAmount = Mathf.RoundToInt(veryTallMaxAmountCurve2.Evaluate(time));
		}
		else
		{
			float time2 = SemiFunc.RunGetDifficultyMultiplier1();
			if (SemiFunc.RunIsArena())
			{
				time2 = 0.75f;
			}
			totalMaxValue = Mathf.RoundToInt(totalMaxValueCurve1.Evaluate(time2));
			tinyMaxAmount = Mathf.RoundToInt(tinyMaxAmountCurve1.Evaluate(time2));
			smallMaxAmount = Mathf.RoundToInt(smallMaxAmountCurve1.Evaluate(time2));
			mediumMaxAmount = Mathf.RoundToInt(mediumMaxAmountCurve1.Evaluate(time2));
			bigMaxAmount = Mathf.RoundToInt(bigMaxAmountCurve1.Evaluate(time2));
			wideMaxAmount = Mathf.RoundToInt(wideMaxAmountCurve1.Evaluate(time2));
			tallMaxAmount = Mathf.RoundToInt(tallMaxAmountCurve1.Evaluate(time2));
			veryTallMaxAmount = Mathf.RoundToInt(veryTallMaxAmountCurve1.Evaluate(time2));
		}
		if (SemiFunc.RunIsArena())
		{
			totalMaxAmount /= 2;
			tinyMaxAmount /= 3;
			smallMaxAmount /= 3;
			mediumMaxAmount /= 3;
			bigMaxAmount /= 3;
			wideMaxAmount /= 2;
			tallMaxAmount /= 2;
			veryTallMaxAmount /= 2;
		}
		foreach (LevelValuables valuablePreset in LevelGenerator.Instance.Level.ValuablePresets)
		{
			tinyValuables.AddRange(valuablePreset.tiny);
			smallValuables.AddRange(valuablePreset.small);
			mediumValuables.AddRange(valuablePreset.medium);
			bigValuables.AddRange(valuablePreset.big);
			wideValuables.AddRange(valuablePreset.wide);
			tallValuables.AddRange(valuablePreset.tall);
			veryTallValuables.AddRange(valuablePreset.veryTall);
		}
		List<ValuableVolume> list = Object.FindObjectsOfType<ValuableVolume>(includeInactive: false).ToList();
		tinyVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.Tiny);
		tinyVolumes.Shuffle();
		smallVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.Small);
		smallVolumes.Shuffle();
		mediumVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.Medium);
		mediumVolumes.Shuffle();
		bigVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.Big);
		bigVolumes.Shuffle();
		wideVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.Wide);
		wideVolumes.Shuffle();
		tallVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.Tall);
		tallVolumes.Shuffle();
		veryTallVolumes = list.FindAll((ValuableVolume x) => x.VolumeType == ValuableVolume.Type.VeryTall);
		veryTallVolumes.Shuffle();
		if (valuableDebug == ValuableDebug.All)
		{
			totalMaxAmount = list.Count;
			totalMaxValue = 99999f;
			tinyMaxAmount = tinyVolumes.Count;
			smallMaxAmount = smallVolumes.Count;
			mediumMaxAmount = mediumVolumes.Count;
			bigMaxAmount = bigVolumes.Count;
			wideMaxAmount = wideVolumes.Count;
			tallMaxAmount = tallVolumes.Count;
			veryTallMaxAmount = veryTallVolumes.Count;
		}
		if (valuableDebug == ValuableDebug.None || LevelGenerator.Instance.Level.ValuablePresets.Count <= 0)
		{
			totalMaxAmount = 0;
			tinyMaxAmount = 0;
			smallMaxAmount = 0;
			mediumMaxAmount = 0;
			bigMaxAmount = 0;
			wideMaxAmount = 0;
			tallMaxAmount = 0;
			veryTallMaxAmount = 0;
		}
		valuableTargetAmount = 0;
		string[] _names = new string[7] { "Tiny", "Small", "Medium", "Big", "Wide", "Tall", "Very Tall" };
		int[] _maxAmount = new int[7] { tinyMaxAmount, smallMaxAmount, mediumMaxAmount, bigMaxAmount, wideMaxAmount, tallMaxAmount, veryTallMaxAmount };
		List<ValuableVolume>[] _volumes = new List<ValuableVolume>[7] { tinyVolumes, smallVolumes, mediumVolumes, bigVolumes, wideVolumes, tallVolumes, veryTallVolumes };
		string[] _path = new string[7] { tinyPath, smallPath, mediumPath, bigPath, widePath, tallPath, veryTallPath };
		int[] _chance = new int[7] { tinyChance, smallChance, mediumChance, bigChance, wideChance, tallChance, veryTallChance };
		List<PrefabRef>[] _valuables = new List<PrefabRef>[7] { tinyValuables, smallValuables, mediumValuables, bigValuables, wideValuables, tallValuables, veryTallValuables };
		int[] _volumeIndex = new int[7];
		for (int _i = 0; _i < totalMaxAmount; _i++)
		{
			float num = -1f;
			int num2 = -1;
			for (int num3 = 0; num3 < _names.Length; num3++)
			{
				if (_volumeIndex[num3] < _maxAmount[num3] && _volumeIndex[num3] < _volumes[num3].Count)
				{
					int num4 = Random.Range(0, _chance[num3]);
					if ((float)num4 > num)
					{
						num = num4;
						num2 = num3;
					}
				}
			}
			if (num2 == -1)
			{
				break;
			}
			ValuableVolume volume = _volumes[num2][_volumeIndex[num2]];
			PrefabRef valuable = _valuables[num2][Random.Range(0, _valuables[num2].Count)];
			Spawn(valuable, volume, _path[num2]);
			_volumeIndex[num2]++;
			yield return null;
		}
		if (GameManager.instance.gameMode == 1)
		{
			PhotonView.RPC("ValuablesTargetSetRPC", RpcTarget.All, valuableTargetAmount);
		}
		valuableSpawnPlayerReady++;
		while (GameManager.instance.gameMode == 1 && valuableSpawnPlayerReady < PhotonNetwork.CurrentRoom.PlayerCount)
		{
			yield return new WaitForSeconds(0.1f);
		}
		VolumesAndSwitchSetup();
		while (GameManager.instance.gameMode == 1 && switchSetupPlayerReady < PhotonNetwork.CurrentRoom.PlayerCount)
		{
			yield return new WaitForSeconds(0.1f);
		}
		setupComplete = true;
	}

	private void Spawn(PrefabRef _valuable, ValuableVolume _volume, string _path)
	{
		GameObject prefab = _valuable.Prefab;
		if (GameManager.instance.gameMode == 0)
		{
			Object.Instantiate(prefab, _volume.transform.position, _volume.transform.rotation);
		}
		else
		{
			PhotonNetwork.InstantiateRoomObject(_valuable.ResourcePath, _volume.transform.position, _volume.transform.rotation, 0);
		}
		ValuableObject component = prefab.GetComponent<ValuableObject>();
		component.DollarValueSetLogic();
		valuableTargetAmount++;
		totalCurrentValue += component.dollarValueCurrent * 0.001f;
		if (totalCurrentValue > totalMaxValue)
		{
			totalMaxAmount = valuableTargetAmount;
		}
	}

	[PunRPC]
	private void ValuablesTargetSetRPC(int _amount, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			valuableTargetAmount = _amount;
		}
	}

	[PunRPC]
	private void PlayerReadyRPC()
	{
		valuableSpawnPlayerReady++;
	}

	public void VolumesAndSwitchSetup()
	{
		if (GameManager.instance.gameMode == 0)
		{
			VolumesAndSwitchSetupRPC();
		}
		else
		{
			PhotonView.RPC("VolumesAndSwitchSetupRPC", RpcTarget.All);
		}
	}

	[PunRPC]
	private void VolumesAndSwitchSetupRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			ValuableVolume[] array = Object.FindObjectsOfType<ValuableVolume>(includeInactive: true);
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Setup();
			}
			ValuablePropSwitch[] array2 = Object.FindObjectsOfType<ValuablePropSwitch>(includeInactive: true);
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Setup();
			}
			if (GameManager.instance.gameMode == 0)
			{
				VolumesAndSwitchReadyRPC();
			}
			else
			{
				PhotonView.RPC("VolumesAndSwitchReadyRPC", RpcTarget.All);
			}
		}
	}

	[PunRPC]
	private void VolumesAndSwitchReadyRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!switchSetupPlayerReadyList.Contains(_info.Sender))
		{
			switchSetupPlayerReadyList.Add(_info.Sender);
			switchSetupPlayerReady++;
		}
	}
}
