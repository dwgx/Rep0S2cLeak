using Photon.Pun;
using UnityEngine;

public class MusicBoxTrap : Trap
{
	public Transform colliderLid;

	public Transform colliderDancers;

	private PhysGrabObject physgrabObject;

	private CollisionFree colliderLidCollision;

	private CollisionFree colliderDancersCollision;

	public Transform MusicBoxRattler;

	public Transform MusicBoxDancerSpin;

	public Transform MusicBoxDancer;

	public Transform MusicBoxLid;

	public Transform PedestalTransform;

	[Space]
	public AnimationCurve MusicBoxLidCurve;

	public AnimationCurve MusicBoxLidRattlerCurve;

	[Space]
	[Header("Sounds")]
	public Sound MusicBoxOpenSound;

	public Sound MusicBoxCloseSound;

	public Sound MusicBoxMusic;

	private float MusicBoxLidDuration = 0.5f;

	private float MusicBoxLidProgress;

	private bool MusicBoxOpenAnimationActive;

	private bool MusicBoxCloseAnimationActive;

	private bool MusicBoxPlaying;

	private bool openTheBox;

	private Rigidbody rb;

	protected override void Start()
	{
		base.Start();
		rb = GetComponent<Rigidbody>();
		physgrabObject = GetComponent<PhysGrabObject>();
		MusicBoxDancer.gameObject.SetActive(value: false);
		PedestalTransform.gameObject.SetActive(value: false);
		colliderLidCollision = colliderLid.GetComponent<CollisionFree>();
		colliderDancersCollision = colliderDancers.GetComponent<CollisionFree>();
	}

	protected override void Update()
	{
		base.Update();
		if (!trapTriggered && physgrabObject.grabbed)
		{
			trapStart = true;
		}
		if (trapStart && !MusicBoxOpenAnimationActive && !MusicBoxCloseAnimationActive)
		{
			MusicBoxStart();
		}
		MusicBoxMusic.PlayLoop(MusicBoxPlaying, 2f, 2f);
		if (openTheBox && !colliderDancersCollision.colliding && !colliderLidCollision.colliding)
		{
			if (GameManager.instance.gameMode == 0)
			{
				float musicTime = (MusicBoxMusic.Source.clip ? Random.Range(0f, MusicBoxMusic.Source.clip.length) : 0f);
				OpenTheBox(musicTime);
			}
			else if (PhotonNetwork.IsMasterClient)
			{
				float num = (MusicBoxMusic.Source.clip ? Random.Range(0f, MusicBoxMusic.Source.clip.length) : 0f);
				photonView.RPC("OpenTheBox", RpcTarget.All, num);
			}
			openTheBox = false;
		}
		if (MusicBoxOpenAnimationActive)
		{
			MusicBoxPlaying = true;
			MusicBoxLidProgress += Time.deltaTime;
			float num2 = MusicBoxLidCurve.Evaluate(MusicBoxLidProgress / MusicBoxLidDuration);
			MusicBoxLid.localRotation = Quaternion.Euler(-90f + (0f - num2) * 100f, 0f, 0f);
			float num3 = MusicBoxLidRattlerCurve.Evaluate(MusicBoxLidProgress / MusicBoxLidDuration);
			MusicBoxRattler.localRotation = Quaternion.Euler(0f, 0f, (0f - num3) * 300f);
			PedestalTransform.localScale = new Vector3(1f, Mathf.Lerp(0.15f, 1f, num2), 1f);
			float num4 = Mathf.Lerp(0.5f, 3f, num2);
			MusicBoxDancer.localScale = new Vector3(num4, num4, num4);
			if (MusicBoxLidProgress >= MusicBoxLidDuration)
			{
				MusicBoxOpenAnimationActive = false;
				MusicBoxLidProgress = 0f;
			}
		}
		if (MusicBoxCloseAnimationActive)
		{
			MusicBoxLidProgress += Time.deltaTime;
			float num5 = 1f - MusicBoxLidCurve.Evaluate(MusicBoxLidProgress / MusicBoxLidDuration);
			MusicBoxLid.localRotation = Quaternion.Euler(-90f + (0f - num5) * 100f, 0f, 0f);
			float num6 = MusicBoxLidRattlerCurve.Evaluate(MusicBoxLidProgress / MusicBoxLidDuration);
			MusicBoxRattler.localRotation = Quaternion.Euler(0f, 0f, (0f - num6) * 300f);
			PedestalTransform.localScale = new Vector3(1f, Mathf.Lerp(0.15f, 1f, num5), 1f);
			float num7 = Mathf.Lerp(0.5f, 3f, num5);
			MusicBoxDancer.localScale = new Vector3(num7, num7, num7);
			if (MusicBoxLidProgress >= MusicBoxLidDuration)
			{
				MusicBoxCloseAnimationActive = false;
				MusicBoxLidProgress = 0f;
				MusicBoxPlaying = false;
				MusicBoxDancer.gameObject.SetActive(value: false);
				PedestalTransform.gameObject.SetActive(value: false);
			}
		}
		if (!MusicBoxPlaying)
		{
			return;
		}
		if (SemiFunc.IsMasterClientOrSingleplayer())
		{
			Quaternion turnX = Quaternion.Euler(0f, 180f, 0f);
			Quaternion turnY = Quaternion.Euler(0f, 0f, 0f);
			Quaternion identity = Quaternion.identity;
			bool flag = false;
			foreach (PhysGrabber item in physGrabObject.playerGrabbing)
			{
				if (item.isRotating)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				physGrabObject.TurnXYZ(turnX, turnY, identity);
			}
		}
		if ((bool)PhysGrabber.instance && PhysGrabber.instance.grabbedObject == rb)
		{
			CameraAim.Instance.AimTargetSoftSet(physGrabObject.transform.position + CameraAim.Instance.transform.right * 100f, 0.01f, 1f, 1f, base.gameObject, 100);
			PhysGrabber.instance.OverrideGrabDistance(1f);
		}
		enemyInvestigate = true;
		MusicBoxDancer.Rotate(0f, 0f, 40f * Time.deltaTime);
		MusicBoxDancerSpin.Rotate(0f, 20f * Time.deltaTime, 0f);
		if (!MusicBoxOpenAnimationActive)
		{
			float num8 = Mathf.Sin(Time.time * 50f) * 0.1f + Mathf.Sin(Time.time * 20f) * 0.1f - Mathf.Sin(Time.time * 70f) * 0.1f;
			float num9 = Mathf.Sin(Time.time * 70f) * 0.1f + Mathf.Sin(Time.time * 10f) * 0.1f - Mathf.Sin(Time.time * 50f) * 0.1f;
			MusicBoxRattler.localRotation = Quaternion.Euler((0f - num9) * 5f, 0f, num8 * 5f);
		}
		if (!physgrabObject.grabbed && !MusicBoxOpenAnimationActive)
		{
			MusicBoxStop();
		}
	}

	public void MusicBoxStop()
	{
		MusicBoxPlaying = false;
		MusicBoxCloseAnimationActive = true;
		trapTriggered = false;
		trapStart = false;
		MusicBoxCloseSound.Play(physgrabObject.centerPoint);
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, base.transform.position, 0.1f);
		colliderDancers.GetComponent<BoxCollider>().isTrigger = true;
		colliderDancers.tag = "Untagged";
		colliderDancers.gameObject.layer = 13;
		colliderLid.GetComponent<BoxCollider>().isTrigger = true;
		colliderLid.tag = "Untagged";
		colliderLid.gameObject.layer = 13;
	}

	public void MusicBoxStart()
	{
		if (!trapTriggered)
		{
			trapTriggered = true;
			openTheBox = true;
		}
	}

	[PunRPC]
	private void OpenTheBox(float musicTime)
	{
		GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, base.transform.position, 0.1f);
		MusicBoxOpenAnimationActive = true;
		MusicBoxOpenSound.Play(physgrabObject.centerPoint);
		MusicBoxDancer.gameObject.SetActive(value: true);
		PedestalTransform.gameObject.SetActive(value: true);
		openTheBox = false;
		colliderDancers.GetComponent<BoxCollider>().isTrigger = false;
		colliderDancers.tag = "Phys Grab Object";
		colliderDancers.gameObject.layer = 16;
		colliderLid.GetComponent<BoxCollider>().isTrigger = false;
		colliderLid.tag = "Phys Grab Object";
		colliderLid.gameObject.layer = 16;
		MusicBoxMusic.StartTimeOverride = musicTime;
	}
}
