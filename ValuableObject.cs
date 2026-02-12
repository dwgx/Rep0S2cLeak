using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class ValuableObject : MonoBehaviour
{
	[Space(40f)]
	[Header("Presets")]
	public Durability durabilityPreset;

	public Value valuePreset;

	public PhysAttribute physAttributePreset;

	public PhysAudio audioPreset;

	[Range(0.5f, 3f)]
	public float audioPresetPitch = 1f;

	public Gradient particleColors;

	[Space(70f)]
	public ValuableVolume.Type volumeType;

	public bool debugVolume = true;

	private Mesh meshTiny;

	private Mesh meshSmall;

	private Mesh meshMedium;

	private Mesh meshBig;

	private Mesh meshWide;

	private Mesh meshTall;

	private Mesh meshVeryTall;

	[Space(20f)]
	internal float dollarValueOriginal = 100f;

	internal float dollarValueCurrent = 100f;

	internal bool dollarValueSet;

	internal int dollarValueOverride;

	private float rigidBodyMass;

	private Rigidbody rb;

	private PhotonView photonView;

	private NavMeshObstacle navMeshObstacle;

	private bool inStartRoom;

	private float inStartRoomCheckTimer;

	internal RoomVolumeCheck roomVolumeCheck;

	internal PhysGrabObject physGrabObject;

	internal bool discovered;

	internal bool discoveredReminder;

	private float discoveredReminderTimer;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
	}

	private void Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
		roomVolumeCheck = GetComponent<RoomVolumeCheck>();
		navMeshObstacle = GetComponent<NavMeshObstacle>();
		if ((bool)navMeshObstacle)
		{
			Debug.LogError(base.gameObject.name + " has a NavMeshObstacle component. Please remove it.");
		}
		StartCoroutine(DollarValueSet());
		rigidBodyMass = physAttributePreset.mass;
		rb = GetComponent<Rigidbody>();
		if ((bool)rb)
		{
			rb.mass = rigidBodyMass;
		}
		physGrabObject.massOriginal = rigidBodyMass;
		if (!LevelGenerator.Instance.Generated)
		{
			ValuableDirector.instance.valuableSpawnAmount++;
			ValuableDirector.instance.valuableList.Add(this);
		}
		if (volumeType <= ValuableVolume.Type.Small)
		{
			physGrabObject.clientNonKinematic = true;
		}
	}

	private void AddToDollarHaulList()
	{
		if (GameManager.instance.gameMode == 1)
		{
			photonView.RPC("AddToDollarHaulListRPC", RpcTarget.All);
		}
		else if ((bool)GetComponent<ValuableObject>() && !RoundDirector.instance.dollarHaulList.Contains(base.gameObject))
		{
			RoundDirector.instance.dollarHaulList.Add(base.gameObject);
		}
	}

	[PunRPC]
	public void AddToDollarHaulListRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info) && (bool)GetComponent<ValuableObject>() && !RoundDirector.instance.dollarHaulList.Contains(base.gameObject))
		{
			RoundDirector.instance.dollarHaulList.Add(base.gameObject);
		}
	}

	private void RemoveFromDollarHaulList()
	{
		if (GameManager.instance.gameMode == 1)
		{
			photonView.RPC("RemoveFromDollarHaulListRPC", RpcTarget.All);
		}
		else if ((bool)GetComponent<ValuableObject>() && RoundDirector.instance.dollarHaulList.Contains(base.gameObject))
		{
			RoundDirector.instance.dollarHaulList.Remove(base.gameObject);
		}
	}

	[PunRPC]
	public void RemoveFromDollarHaulListRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info) && (bool)GetComponent<ValuableObject>() && RoundDirector.instance.dollarHaulList.Contains(base.gameObject))
		{
			RoundDirector.instance.dollarHaulList.Remove(base.gameObject);
		}
	}

	private void Update()
	{
		if (GameManager.instance.gameMode == 0 || PhotonNetwork.IsMasterClient)
		{
			if (inStartRoomCheckTimer > 0f)
			{
				inStartRoomCheckTimer -= Time.deltaTime;
			}
			else
			{
				bool flag = false;
				foreach (RoomVolume currentRoom in roomVolumeCheck.CurrentRooms)
				{
					if (currentRoom.Extraction)
					{
						if (!inStartRoom)
						{
							AddToDollarHaulList();
							inStartRoom = true;
						}
						flag = true;
					}
				}
				if (!flag && inStartRoom)
				{
					RemoveFromDollarHaulList();
					inStartRoom = false;
				}
				inStartRoomCheckTimer = 0.5f;
			}
		}
		DiscoverReminderLogic();
	}

	private IEnumerator DollarValueSet()
	{
		yield return new WaitForSeconds(0.05f);
		while (LevelGenerator.Instance.State <= LevelGenerator.LevelState.Valuable)
		{
			yield return new WaitForSeconds(0.05f);
		}
		if (!SemiFunc.IsMultiplayer())
		{
			DollarValueSetLogic();
		}
		else if (SemiFunc.IsMasterClient())
		{
			DollarValueSetLogic();
			photonView.RPC("DollarValueSetRPC", RpcTarget.Others, dollarValueCurrent);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			RoundDirector.instance.haulGoalMax += (int)dollarValueCurrent;
		}
	}

	public void DollarValueSetLogic()
	{
		if (!dollarValueSet)
		{
			if (dollarValueOverride != 0)
			{
				dollarValueOriginal = dollarValueOverride;
				dollarValueCurrent = dollarValueOverride;
			}
			else
			{
				dollarValueOriginal = Mathf.Round(Random.Range(valuePreset.valueMin, valuePreset.valueMax));
				dollarValueOriginal = Mathf.Round(dollarValueOriginal / 100f) * 100f;
				dollarValueCurrent = dollarValueOriginal;
			}
			dollarValueSet = true;
		}
	}

	private void DiscoverReminderLogic()
	{
		if (!discovered || discoveredReminder)
		{
			return;
		}
		if (discoveredReminderTimer > 0f)
		{
			discoveredReminderTimer -= Time.deltaTime;
			return;
		}
		discoveredReminderTimer = Random.Range(2f, 5f);
		if (!physGrabObject.impactDetector.inCart && PlayerController.instance.isActiveAndEnabled)
		{
			if (Vector3.Distance(base.transform.position, PlayerController.instance.transform.position) > 20f)
			{
				discoveredReminder = true;
			}
		}
		else
		{
			discoveredReminder = false;
		}
	}

	public void Discover(ValuableDiscoverGraphic.State _state)
	{
		if (!discovered)
		{
			if (!GameManager.Multiplayer())
			{
				DiscoverRPC();
			}
			else
			{
				photonView.RPC("DiscoverRPC", RpcTarget.All);
			}
		}
		ValuableDiscover.instance.New(physGrabObject, _state);
	}

	[PunRPC]
	private void DiscoverRPC()
	{
		discovered = true;
		Map.Instance.AddValuable(this);
	}

	[PunRPC]
	public void DollarValueSetRPC(float value, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			dollarValueOriginal = value;
			dollarValueCurrent = value;
			dollarValueSet = true;
		}
	}
}
