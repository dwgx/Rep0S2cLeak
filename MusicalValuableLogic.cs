using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class MusicalValuableLogic : MonoBehaviour
{
	private PhotonView photonView;

	[FormerlySerializedAs("pianoKeysStart")]
	public Transform musicKeysStart;

	[FormerlySerializedAs("pianoKeysEnd")]
	public Transform musicKeysEnd;

	[Range(0f, 1f)]
	public float volume = 0.25f;

	[Range(0f, 3f)]
	public float lowKeyAmpAmount;

	[FormerlySerializedAs("pitchShift")]
	public bool hasPitchShift;

	public float pitchShiftAmount = 1f;

	public int numberOfOctaves = 6;

	public List<Sound> musicKeys;

	private PhysGrabObject physGrabObject;

	private PhysGrabObjectGrabArea grabArea;

	private int numberOfKeys = 108;

	private Dictionary<AudioSource, PhysGrabber> currentlyPlayedKeys = new Dictionary<AudioSource, PhysGrabber>();

	private bool grabbedByLocalPlayer;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		grabArea = GetComponent<PhysGrabObjectGrabArea>();
		physGrabObject = GetComponent<PhysGrabObject>();
		numberOfKeys = musicKeys.Count * numberOfOctaves;
	}

	private void RemovePhysGrabberFromDictionary(PhysGrabber _physGrabber)
	{
		foreach (KeyValuePair<AudioSource, PhysGrabber> item in currentlyPlayedKeys.ToList())
		{
			AudioSource key = item.Key;
			if (item.Value == _physGrabber)
			{
				key.priority = 50;
				currentlyPlayedKeys.Remove(key);
				break;
			}
		}
	}

	private void UpdateGrabbedByLocalPlayerGrabRelease()
	{
		if (!SemiFunc.IsMultiplayer())
		{
			RemovePhysGrabberFromDictionary(PhysGrabber.instance);
			return;
		}
		photonView.RPC("UpdateGrabbedByThisPhysGrabberGrabReleaseRPC", RpcTarget.All, PhysGrabber.instance.photonView.ViewID);
	}

	[PunRPC]
	public void UpdateGrabbedByThisPhysGrabberGrabReleaseRPC(int physGrabberPhotonViewID)
	{
		PhysGrabber component = PhotonView.Find(physGrabberPhotonViewID).GetComponent<PhysGrabber>();
		RemovePhysGrabberFromDictionary(component);
	}

	private void PitchShiftLogic()
	{
		if (PhysGrabber.instance.grabbed && PhysGrabber.instance.grabbedPhysGrabObject == physGrabObject)
		{
			grabbedByLocalPlayer = true;
		}
		else
		{
			if (grabbedByLocalPlayer)
			{
				UpdateGrabbedByLocalPlayerGrabRelease();
			}
			grabbedByLocalPlayer = false;
		}
		foreach (KeyValuePair<AudioSource, PhysGrabber> item in currentlyPlayedKeys.ToList())
		{
			AudioSource key = item.Key;
			PhysGrabber value = item.Value;
			if (!key || value == null)
			{
				currentlyPlayedKeys.Remove(key);
				continue;
			}
			Vector3 physGrabPointPullerPosition = value.physGrabPointPullerPosition;
			Vector3 position = value.physGrabPoint.position;
			float forceMax = value.forceMax;
			float num = Mathf.Clamp(1f + (Vector3.ClampMagnitude(physGrabPointPullerPosition - position, forceMax) * 10f).magnitude / forceMax, 1f, 1f + pitchShiftAmount);
			key.pitch = Mathf.Lerp(key.pitch, num, Time.deltaTime * 10f);
			key.priority = 20;
		}
	}

	private void Update()
	{
		if (hasPitchShift)
		{
			PitchShiftLogic();
		}
	}

	public void MusicKeyPressed()
	{
		int num = numberOfKeys;
		PlayerAvatar latestGrabber = grabArea.GetLatestGrabber();
		Vector3 position = latestGrabber.physGrabber.physGrabPoint.position;
		Vector3 position2 = musicKeysStart.position;
		Vector3 position3 = musicKeysEnd.position;
		float num2 = Vector3.Dot(rhs: (position3 - position2).normalized, lhs: position - position2);
		float num3 = Vector3.Distance(position2, position3);
		int num4;
		if (num2 <= 0f)
		{
			num4 = 0;
		}
		else if (num2 >= num3)
		{
			num4 = num - 1;
		}
		else
		{
			float num5 = num3 / (float)num;
			num4 = (int)(num2 / num5);
		}
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("MusicKeyPressedRPC", RpcTarget.All, num4, latestGrabber.physGrabber.photonView.ViewID);
		}
		else
		{
			MusicKeyPressedRPC(num4);
		}
	}

	[PunRPC]
	public void MusicKeyPressedRPC(int keyIndex, int grabberID = -1)
	{
		PlayKey(keyIndex, grabberID);
		SemiFunc.EnemyInvestigate(physGrabObject.midPoint, 25f);
	}

	private void PlayKey(int key, int grabberID = -1)
	{
		float num = 0.05f;
		int num2 = 0;
		int num3 = numberOfKeys / musicKeys.Count;
		for (int i = 0; i < numberOfKeys; i++)
		{
			int index = i % musicKeys.Count;
			if (key >= num2 && key < num2 + num3)
			{
				PhysGrabber physGrabber = null;
				physGrabber = ((grabberID == -1) ? PhysGrabber.instance : PhotonView.Find(grabberID).GetComponent<PhysGrabber>());
				int num4 = 0;
				int num5 = numberOfKeys - 1;
				float num6 = Mathf.Clamp(1f - (float)(key - num4) / (float)(num5 - num4), 0f, 1f) * lowKeyAmpAmount;
				musicKeys[index].Volume = volume * (1f + num6);
				musicKeys[index].Pitch = 1f + (float)(key - num2) * num;
				AudioSource audioSource = musicKeys[index].Play(physGrabObject.midPoint);
				audioSource.priority = 20;
				if (hasPitchShift && (bool)physGrabber)
				{
					currentlyPlayedKeys.Add(audioSource, physGrabber);
				}
			}
			num2 += num3;
		}
	}
}
