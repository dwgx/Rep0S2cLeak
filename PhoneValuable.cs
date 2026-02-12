using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PhoneValuable : MonoBehaviour
{
	public float ringDistance = 7.5f;

	public GameObject phoneLight;

	public Sound ringTone;

	public Sound vibrationSound;

	private PhotonView photonView;

	public GameObject phoneMesh;

	private Quaternion initialRotation;

	protected Rigidbody rb;

	private List<PlayerAvatar> players;

	private bool playersNearby;

	private MeshRenderer meshRenderer;

	private bool phoneRinging;

	private bool vibrate;

	private bool visualsReset = true;

	private float joltTimer;

	private float joltInterval = 0.3f;

	public float ringIntervalTimer;

	private float ringIntervalMin = 10f;

	private float ringIntervalMax = 100f;

	public float ringTimer;

	private float ringTimerMin = 5f;

	private float ringTimerMax = 20f;

	private float maxVelocity = 0.5f;

	private bool reset;

	private float vibrateCycleTimer;

	private float vibrateCycleDuration = 2f;

	private float investigateTimer;

	private float investigateInterval = 2f;

	protected void Start()
	{
		rb = GetComponent<Rigidbody>();
		meshRenderer = GetComponentInChildren<MeshRenderer>();
		meshRenderer.material.DisableKeyword("_EMISSION");
		initialRotation = phoneMesh.transform.localRotation;
		photonView = GetComponent<PhotonView>();
		joltTimer = joltInterval;
		ringIntervalTimer = UnityEngine.Random.Range(ringIntervalMin, ringIntervalMax);
		ringTimer = UnityEngine.Random.Range(ringTimerMin, ringTimerMax);
	}

	protected void Update()
	{
		RingVisuals();
		ringTone.PlayLoop(phoneRinging, 10f, 10f);
		vibrationSound.PlayLoop(vibrate, 10f, 10f);
		CheckForNearbyPlayers();
	}

	private void CheckForNearbyPlayers()
	{
		if (!SemiFunc.FPSImpulse5())
		{
			return;
		}
		players = SemiFunc.PlayerGetList();
		foreach (PlayerAvatar player in players)
		{
			if (Vector3.Distance(player.transform.position, base.transform.position) < ringDistance)
			{
				playersNearby = true;
				reset = true;
				break;
			}
			playersNearby = false;
		}
	}

	private void FixedUpdate()
	{
		if (playersNearby || reset)
		{
			PhoneRing();
		}
	}

	private void RingVisuals()
	{
		if (!phoneRinging)
		{
			if (!visualsReset)
			{
				meshRenderer.material.DisableKeyword("_EMISSION");
				phoneLight.SetActive(value: false);
				vibrate = false;
				vibrateCycleTimer = 0f;
				initialRotation = phoneMesh.transform.localRotation;
				visualsReset = true;
			}
			return;
		}
		visualsReset = false;
		meshRenderer.material.EnableKeyword("_EMISSION");
		if (!phoneLight.activeSelf)
		{
			phoneLight.SetActive(value: true);
		}
		vibrateCycleTimer -= Time.deltaTime;
		if (vibrateCycleTimer < 0f)
		{
			vibrateCycleTimer = vibrateCycleDuration;
		}
		vibrate = vibrateCycleTimer >= vibrateCycleDuration / 2f;
		if (vibrate)
		{
			float num = 80f;
			float x = 2f * Mathf.Sin(Time.time * num);
			float z = 2f * Mathf.Sin(Time.time * num + MathF.PI / 2f);
			phoneMesh.transform.localRotation = initialRotation * Quaternion.Euler(x, 0f, z);
			if (!SemiFunc.IsMasterClientOrSingleplayer())
			{
				return;
			}
			joltTimer -= Time.deltaTime;
			if (joltTimer <= 0f)
			{
				if (rb.velocity.magnitude < maxVelocity)
				{
					rb.AddForce(Vector3.up * 0.05f, ForceMode.Impulse);
					Vector3 torque = UnityEngine.Random.insideUnitSphere.normalized * 0.1f;
					rb.AddTorque(torque, ForceMode.Impulse);
				}
				joltTimer = joltInterval;
			}
		}
		else
		{
			phoneMesh.transform.localRotation = initialRotation;
		}
	}

	private void PhoneRing()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer())
		{
			return;
		}
		if (!phoneRinging && playersNearby)
		{
			ringIntervalTimer -= Time.deltaTime;
		}
		if (ringIntervalTimer <= 0f && ringTimer > 0f)
		{
			if (investigateTimer <= 0f)
			{
				EnemyDirector.instance.SetInvestigate(base.transform.position, 15f);
				investigateTimer = investigateInterval;
			}
			else
			{
				investigateTimer -= Time.deltaTime;
			}
			if (!phoneRinging)
			{
				phoneRinging = true;
				if (SemiFunc.IsMultiplayer())
				{
					photonView.RPC("SetRingRPC", RpcTarget.Others, phoneRinging);
				}
			}
			ringTimer -= Time.deltaTime;
		}
		else
		{
			if (!(ringTimer <= 0f))
			{
				return;
			}
			investigateTimer = 0f;
			ringIntervalTimer = UnityEngine.Random.Range(ringIntervalMin, ringIntervalMax);
			ringTimer = UnityEngine.Random.Range(ringTimerMin, ringTimerMax);
			if (phoneRinging)
			{
				phoneRinging = false;
				if (SemiFunc.IsMultiplayer())
				{
					photonView.RPC("SetRingRPC", RpcTarget.Others, phoneRinging);
				}
			}
			if (!playersNearby)
			{
				reset = false;
			}
		}
	}

	[PunRPC]
	private void SetRingRPC(bool _ringing, PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			phoneRinging = _ringing;
		}
	}
}
