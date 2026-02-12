using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class MonkeyBox : Trap
{
	public Sound monkeySound;

	public List<AudioClip> monkeySoundsClips;

	private bool previousGrabState;

	private float voiceCoolDown = 1.5f;

	private float soundTime;

	protected override void Start()
	{
		base.Start();
		soundTime = Time.time;
	}

	protected override void Update()
	{
		if (SemiFunc.IsMasterClientOrSingleplayer() && physGrabObject.grabbed != previousGrabState)
		{
			previousGrabState = physGrabObject.grabbed;
			if (physGrabObject.grabbed && Time.time - soundTime > voiceCoolDown)
			{
				PlayLine(Random.Range(0, monkeySoundsClips.Count - 1));
				soundTime = Time.time;
			}
		}
	}

	private void PlayLine(int index)
	{
		EnemyDirector.instance.SetInvestigate(base.transform.position, 15f);
		if (SemiFunc.IsMultiplayer())
		{
			photonView.RPC("PlayLineRPC", RpcTarget.All, index);
		}
		else
		{
			PlayLineRPC(index);
		}
	}

	[PunRPC]
	private void PlayLineRPC(int index, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			AudioClip audioClip = monkeySoundsClips[index];
			AudioClip[] sounds = new AudioClip[1] { audioClip };
			monkeySound.Sounds = sounds;
			monkeySound.Play(physGrabObject.centerPoint);
		}
	}
}
