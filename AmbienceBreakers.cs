using System.Collections;
using Photon.Pun;
using UnityEngine;

public class AmbienceBreakers : MonoBehaviour
{
	public static AmbienceBreakers instance;

	private PhotonView photonView;

	private bool isLocal;

	[Space]
	public float minDistance = 8f;

	public float maxDistance = 15f;

	[Space]
	public float cooldownMin = 20f;

	public float cooldownMax = 120f;

	private float cooldownTimer;

	[Space]
	public Sound sound;

	private LevelAmbience presetOverride;

	private int breakerOverride = -1;

	private float updateRate = 0.5f;

	private void Awake()
	{
		instance = this;
		photonView = GetComponent<PhotonView>();
	}

	private void Start()
	{
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
		{
			isLocal = true;
		}
	}

	public void Setup()
	{
		StopAllCoroutines();
		StartCoroutine(Logic());
	}

	private IEnumerator Logic()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		cooldownTimer = Random.Range(cooldownMin, cooldownMax);
		if (!isLocal)
		{
			yield break;
		}
		while (true)
		{
			if (cooldownTimer > 0f)
			{
				cooldownTimer -= updateRate;
			}
			else
			{
				cooldownTimer = Random.Range(cooldownMin, cooldownMax);
				Vector2 normalized = Random.insideUnitCircle.normalized;
				float num = Random.Range(minDistance, maxDistance);
				PlayerAvatar playerAvatar = GameDirector.instance.PlayerList[Random.Range(0, GameDirector.instance.PlayerList.Count)];
				Vector3 vector = playerAvatar.transform.position + new Vector3(normalized.x, 0f, normalized.y) * num;
				LevelAmbience levelAmbience = LevelGenerator.Instance.Level.AmbiencePresets[Random.Range(0, LevelGenerator.Instance.Level.AmbiencePresets.Count)];
				if (playerAvatar.RoomVolumeCheck.CurrentRooms.Count > 0 && (bool)playerAvatar.RoomVolumeCheck.CurrentRooms[0].RoomAmbienceOverride)
				{
					levelAmbience = playerAvatar.RoomVolumeCheck.CurrentRooms[0].RoomAmbienceOverride;
				}
				if ((bool)presetOverride)
				{
					levelAmbience = presetOverride;
				}
				presetOverride = null;
				if (levelAmbience.breakers.Count > 0)
				{
					int num2 = Random.Range(0, levelAmbience.breakers.Count);
					if (breakerOverride != -1)
					{
						num2 = breakerOverride;
					}
					breakerOverride = -1;
					if (!GameManager.Multiplayer())
					{
						PlaySoundRPC(vector, levelAmbience.name, num2);
					}
					else
					{
						photonView.RPC("PlaySoundRPC", RpcTarget.All, vector, levelAmbience.name, num2);
					}
				}
			}
			yield return new WaitForSeconds(updateRate);
		}
	}

	public void LiveTest(LevelAmbience _presetOverride, LevelAmbienceBreaker _breakerOverride)
	{
		foreach (LevelAmbience levelAmbience in AudioManager.instance.levelAmbiences)
		{
			if (!(levelAmbience == _presetOverride))
			{
				continue;
			}
			presetOverride = levelAmbience;
			foreach (LevelAmbienceBreaker breaker in levelAmbience.breakers)
			{
				if (breaker == _breakerOverride)
				{
					breakerOverride = levelAmbience.breakers.IndexOf(breaker);
				}
			}
		}
		cooldownTimer = 0f;
	}

	[PunRPC]
	public void PlaySoundRPC(Vector3 _position, string _presetName, int _breaker)
	{
		foreach (LevelAmbience levelAmbience in AudioManager.instance.levelAmbiences)
		{
			if (levelAmbience.name == _presetName)
			{
				sound.Volume = levelAmbience.breakers[_breaker].volume;
				sound.Sounds[0] = levelAmbience.breakers[_breaker].sound;
				sound.Play(_position);
				return;
			}
		}
		Debug.LogError("Ambience Breaker sound not found: " + _presetName + " - " + _breaker);
	}
}
