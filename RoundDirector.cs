using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RoundDirector : MonoBehaviour
{
	private PhotonView photonView;

	public AnimationCurve haulGoalCurve1;

	public AnimationCurve haulGoalCurve2;

	internal bool debugLowHaul;

	internal bool debugInfiniteBattery;

	internal int currentHaul;

	internal int totalHaul;

	internal int currentHaulMax;

	internal int haulGoal;

	internal int haulGoalMax;

	internal int deadPlayers;

	internal int extractionPoints;

	internal int extractionPointSurplus;

	internal int extractionPointsCompleted;

	internal int extractionHaulGoal;

	internal bool extractionPointActive;

	internal List<GameObject> extractionPointList = new List<GameObject>();

	internal ExtractionPoint extractionPointCurrent;

	internal bool extractionPointsFetched;

	[HideInInspector]
	public bool extractionPointDeductionDone;

	internal bool allExtractionPointsCompleted;

	public static RoundDirector instance;

	internal List<PhysGrabObject> physGrabObjects;

	public List<GameObject> dollarHaulList = new List<GameObject>();

	[Space]
	public Sound lightsTurnOffSound;

	private void Awake()
	{
		instance = this;
		physGrabObjects = new List<PhysGrabObject>();
	}

	private IEnumerator StartRound()
	{
		while (!LevelGenerator.Instance.Generated && haulGoalMax == 0)
		{
			yield return new WaitForSeconds(0.5f);
		}
		yield return new WaitForSeconds(0.1f);
		if (GameManager.instance.gameMode == 0 || PhotonNetwork.IsMasterClient)
		{
			float num = (float)haulGoalMax * 0.7f;
			num = ((!(SemiFunc.RunGetDifficultyMultiplier2() > 0f)) ? (num * haulGoalCurve1.Evaluate(SemiFunc.RunGetDifficultyMultiplier1())) : (num * haulGoalCurve2.Evaluate(SemiFunc.RunGetDifficultyMultiplier2())));
			if (debugLowHaul || SemiFunc.RunIsTutorial() || SemiFunc.RunIsRecording())
			{
				num = 1000 * extractionPoints;
			}
			StartRoundLogic((int)num);
			if (GameManager.instance.gameMode == 1)
			{
				photonView.RPC("StartRoundRPC", RpcTarget.All, haulGoal);
			}
		}
		extractionPointsFetched = true;
	}

	private void StartRoundLogic(int value)
	{
		currentHaul = 0;
		currentHaulMax = 0;
		deadPlayers = 0;
		haulGoal = value;
	}

	[PunRPC]
	private void StartRoundRPC(int value, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			StartRoundLogic(value);
		}
	}

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		photonView.TransferOwnership(PhotonNetwork.MasterClient);
		extractionPointsFetched = false;
		StartCoroutine(StartRound());
	}

	public void PhysGrabObjectAdd(PhysGrabObject _physGrabObject)
	{
		if (!physGrabObjects.Contains(_physGrabObject))
		{
			physGrabObjects.Add(_physGrabObject);
		}
	}

	public void PhysGrabObjectRemove(PhysGrabObject _physGrabObject)
	{
		if (physGrabObjects.Contains(_physGrabObject))
		{
			physGrabObjects.Remove(_physGrabObject);
		}
	}

	private void Update()
	{
		currentHaul = 0;
		currentHaulMax = 0;
		if (dollarHaulList.Count <= 0)
		{
			return;
		}
		foreach (GameObject dollarHaul in dollarHaulList)
		{
			if (!dollarHaul)
			{
				dollarHaulList.Remove(dollarHaul);
				continue;
			}
			currentHaul += (int)dollarHaul.GetComponent<ValuableObject>().dollarValueCurrent;
			currentHaulMax += (int)dollarHaul.GetComponent<ValuableObject>().dollarValueOriginal;
		}
	}

	public void RequestExtractionPointActivation(int photonViewID)
	{
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("RequestExtractionPointActivationRPC", RpcTarget.MasterClient, photonViewID);
		}
		else
		{
			RequestExtractionPointActivationRPC(photonViewID);
		}
	}

	[PunRPC]
	public void RequestExtractionPointActivationRPC(int photonViewID)
	{
		if (!extractionPointActive && SemiFunc.IsMasterClientOrSingleplayer())
		{
			extractionPointActive = true;
			photonView.RPC("ExtractionPointActivateRPC", RpcTarget.All, photonViewID);
		}
	}

	public void ExtractionPointActivate(int photonViewID)
	{
		if (!extractionPointActive && PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("ExtractionPointActivateRPC", RpcTarget.All, photonViewID);
		}
	}

	[PunRPC]
	public void ExtractionPointActivateRPC(int photonViewID, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			extractionPointDeductionDone = false;
			extractionPointActive = true;
			GameObject gameObject = PhotonView.Find(photonViewID).gameObject;
			extractionPointCurrent = gameObject.GetComponent<ExtractionPoint>();
			extractionPointCurrent.ButtonPress();
			ExtractionPointsLock(gameObject);
		}
	}

	public void ExtractionPointsLock(GameObject exceptMe)
	{
		foreach (GameObject extractionPoint in extractionPointList)
		{
			if (extractionPoint != exceptMe)
			{
				extractionPoint.GetComponent<ExtractionPoint>().isLocked = true;
			}
		}
	}

	public void ExtractionCompleted()
	{
		extractionPointsCompleted++;
		if (extractionPointsCompleted < extractionPoints && SemiFunc.RunIsLevel() && TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialMultipleExtractions, 1))
		{
			TutorialDirector.instance.ActivateTip("Multiple Extractions", 2f, _interrupt: false);
		}
	}

	public void ExtractionCompletedAllCheck()
	{
		if (SemiFunc.RunIsShop() || extractionPointsCompleted < extractionPoints - 1)
		{
			return;
		}
		allExtractionPointsCompleted = true;
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("ExtractionCompletedAllRPC", RpcTarget.All);
			}
			else
			{
				ExtractionCompletedAllRPC();
			}
		}
	}

	public void ExtractionPointsUnlock()
	{
		if (SemiFunc.IsMultiplayer())
		{
			if (SemiFunc.IsMasterClient())
			{
				photonView.RPC("ExtractionPointsUnlockRPC", RpcTarget.All);
			}
		}
		else
		{
			ExtractionPointsUnlockRPC();
		}
	}

	[PunRPC]
	public void ExtractionPointsUnlockRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (!SemiFunc.MasterOnlyRPC(_info))
		{
			return;
		}
		foreach (GameObject extractionPoint in extractionPointList)
		{
			extractionPoint.GetComponent<ExtractionPoint>().isLocked = false;
		}
		extractionPointCurrent = null;
	}

	public void HaulCheck()
	{
		currentHaul = 0;
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject dollarHaul in dollarHaulList)
		{
			if (!dollarHaul)
			{
				list.Add(dollarHaul);
				continue;
			}
			ValuableObject component = dollarHaul.GetComponent<ValuableObject>();
			if ((bool)component)
			{
				component.roomVolumeCheck.CheckSet();
				if (!component.roomVolumeCheck.inExtractionPoint)
				{
					list.Add(dollarHaul);
				}
				else
				{
					currentHaul += (int)component.dollarValueCurrent;
				}
			}
		}
		foreach (GameObject item in list)
		{
			dollarHaulList.Remove(item);
		}
	}

	[PunRPC]
	private void ExtractionCompletedAllRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			AudioScare.instance.PlayImpact();
			lightsTurnOffSound.Play(base.transform.position);
			GameDirector.instance.CameraShake.Shake(3f, 1f);
			GameDirector.instance.CameraImpact.Shake(5f, 0.1f);
			if (SemiFunc.RunIsLevel() && TutorialDirector.instance.TutorialSettingCheck(DataDirector.Setting.TutorialFinalExtraction, 1))
			{
				TutorialDirector.instance.ActivateTip("Final Extraction", 0.5f, _interrupt: false);
			}
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (SemiFunc.MasterOnlyRPC(info))
		{
			if (stream.IsWriting)
			{
				stream.SendNext(currentHaul);
				stream.SendNext(haulGoal);
			}
			else
			{
				currentHaul = (int)stream.ReceiveNext();
				haulGoal = (int)stream.ReceiveNext();
			}
		}
	}
}
