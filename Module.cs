using Photon.Pun;
using UnityEngine;

public class Module : MonoBehaviour
{
	public enum Type
	{
		Normal,
		Passage,
		DeadEnd,
		Extraction
	}

	[Space]
	public SemiFunc.User moduleOwner;

	[Space(20f)]
	private PhotonView photonView;

	private Color colorPositive = Color.green;

	private Color colorNegative = new Color(1f, 0.74f, 0.61f);

	public bool wallsInside;

	[Space]
	public bool wallsMap;

	public bool levelPointsEntrance;

	[Space]
	public bool levelPointsWaypoints;

	[Space]
	public bool levelPointsRoomVolume;

	[Space]
	public bool levelPointsNavmesh;

	[Space]
	public bool levelPointsConnected;

	public bool lightsMax;

	[Space]
	public bool lightsPrefab;

	public bool roomVolumeDoors;

	[Space]
	public bool roomVolumeHeight;

	[Space]
	public bool roomVolumeSpace;

	public bool navmeshConnected;

	[Space]
	public bool navmeshPitfalls;

	public bool valuablesAllTypes;

	[Space]
	public bool valuablesMaxed;

	[Space]
	public bool valuablesSwitch;

	[Space]
	public bool valuablesSwitchNavmesh;

	[Space]
	public bool valuablesTest;

	public bool ModulePropSwitchSetup;

	[Space]
	public bool ModulePropSwitchNavmesh;

	internal bool ConnectingTop;

	internal bool ConnectingRight;

	internal bool ConnectingBottom;

	internal bool ConnectingLeft;

	[Space]
	internal bool SetupDone;

	internal bool First;

	[Space]
	internal int GridX;

	internal int GridY;

	public bool Explored;

	internal bool StartRoom;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
		photonView.ObservedComponents.Clear();
	}

	private void Start()
	{
		if ((bool)GetComponent<StartRoom>())
		{
			StartRoom = true;
		}
		else
		{
			base.transform.parent = LevelGenerator.Instance.LevelParent.transform;
		}
	}

	private void ResetChecklist()
	{
		wallsInside = false;
		wallsMap = false;
		levelPointsEntrance = false;
		levelPointsWaypoints = false;
		levelPointsRoomVolume = false;
		levelPointsNavmesh = false;
		levelPointsConnected = false;
		lightsMax = false;
		lightsPrefab = false;
		roomVolumeDoors = false;
		roomVolumeHeight = false;
		roomVolumeSpace = false;
		navmeshConnected = false;
		navmeshPitfalls = false;
		valuablesAllTypes = false;
		valuablesMaxed = false;
		valuablesSwitch = false;
		valuablesSwitchNavmesh = false;
		valuablesTest = false;
		ModulePropSwitchSetup = false;
		ModulePropSwitchNavmesh = false;
	}

	private void SetAllChecklist()
	{
		wallsInside = true;
		wallsMap = true;
		levelPointsEntrance = true;
		levelPointsWaypoints = true;
		levelPointsRoomVolume = true;
		levelPointsNavmesh = true;
		levelPointsConnected = true;
		lightsMax = true;
		lightsPrefab = true;
		roomVolumeDoors = true;
		roomVolumeHeight = true;
		roomVolumeSpace = true;
		navmeshConnected = true;
		navmeshPitfalls = true;
		valuablesAllTypes = true;
		valuablesMaxed = true;
		valuablesSwitch = true;
		valuablesSwitchNavmesh = true;
		valuablesTest = true;
		ModulePropSwitchSetup = true;
		ModulePropSwitchNavmesh = true;
	}

	public void ModuleConnectionSet(bool _top, bool _bottom, bool _right, bool _left, bool _first)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("ModuleConnectionSetRPC", RpcTarget.All, _top, _bottom, _right, _left, _first);
		}
		else
		{
			ModuleConnectionSetRPC(_top, _bottom, _right, _left, _first);
		}
	}

	[PunRPC]
	private void ModuleConnectionSetRPC(bool _top, bool _bottom, bool _right, bool _left, bool _first, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			ConnectingTop = _top;
			ConnectingBottom = _bottom;
			ConnectingRight = _right;
			ConnectingLeft = _left;
			First = _first;
			SetupDone = true;
			ModulePropSwitch[] componentsInChildren = GetComponentsInChildren<ModulePropSwitch>();
			foreach (ModulePropSwitch obj in componentsInChildren)
			{
				obj.Module = this;
				obj.Setup();
			}
			LevelGenerator.Instance.ModulesSpawned++;
			if (!wallsInside || !wallsMap || !levelPointsEntrance || !levelPointsWaypoints || !levelPointsRoomVolume || !levelPointsNavmesh || !levelPointsConnected || !lightsMax || !lightsPrefab || !roomVolumeDoors || !roomVolumeHeight || !roomVolumeSpace || !navmeshConnected || !navmeshPitfalls || !valuablesAllTypes || !valuablesMaxed || !valuablesSwitch || !valuablesSwitchNavmesh || !valuablesTest || !ModulePropSwitchSetup || !ModulePropSwitchNavmesh)
			{
				Debug.LogWarning("Module not checked off: " + base.name, base.gameObject);
			}
		}
	}
}
