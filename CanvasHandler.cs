using System.Collections;
using Photon.Pun;
using UnityEngine;

public class CanvasHandler : MonoBehaviour
{
	public enum State
	{
		Dirty,
		Cleaning,
		Clean
	}

	public bool DebugNoClean;

	private PhotonView photonView;

	[Header("Connected Objects")]
	public GameObject Dirt1;

	public GameObject Dirt2;

	public GameObject DirtHang;

	public GameObject Painting;

	public ParticleSystem DustParticles;

	public CleanEffect cleanEffect;

	public GameObject InteractionArea;

	public LightInteractableFadeRemove InteractableLight;

	[Space]
	[Header("Sounds")]
	public Sound PaintingSwingLoop;

	public Sound PaintingSwingEnd;

	[Space]
	[Header("Painting Swing Settings")]
	public float wiggleSpeed = 20f;

	public float wiggleAmount = 5f;

	public float dampening = 0.95f;

	private bool isWiggling;

	private float currentWiggleAmount;

	[Space]
	[Header("Cleaning Settings")]
	private float cleanStateTimer;

	public bool isCleaning;

	public bool isCleaningPrevious;

	[HideInInspector]
	public float isCleaningTimer;

	private bool cleanInputPrevious;

	public bool cleanInput;

	[HideInInspector]
	public bool CleanDone;

	private bool multiplayerCleaning;

	public State currentState;

	public State previousState;

	[HideInInspector]
	public float fadeMultiplier = 1f;

	public float cleaningSpeed = 0.1f;

	private MeshRenderer dirt1Renderer;

	private MeshRenderer dirt2Renderer;

	private MeshRenderer dirtHangRenderer;

	private void Start()
	{
		dirt1Renderer = Dirt1.GetComponent<MeshRenderer>();
		dirt2Renderer = Dirt2.GetComponent<MeshRenderer>();
		dirtHangRenderer = DirtHang.GetComponent<MeshRenderer>();
		photonView = GetComponent<PhotonView>();
	}

	[PunRPC]
	private void StartCleaningRPC()
	{
		multiplayerCleaning = true;
	}

	[PunRPC]
	private void StopCleaningRPC()
	{
		multiplayerCleaning = false;
	}

	[PunRPC]
	private void CleaningDoneRPC()
	{
		currentState = State.Clean;
		cleanEffect.Clean();
		InteractableLight.StartFading();
		InteractionArea.SetActive(value: false);
		SetMaterialAlpha(dirt1Renderer, 0f);
		SetMaterialAlpha(dirt2Renderer, 0f);
		SetMaterialAlpha(dirtHangRenderer, 0f);
	}

	private void Update()
	{
		if (isCleaningTimer > 0f)
		{
			isCleaning = true;
			isCleaningTimer -= 1f * Time.deltaTime;
		}
		else
		{
			isCleaning = false;
		}
		if (currentState == State.Clean)
		{
			isCleaning = false;
		}
		if ((double)fadeMultiplier < 0.5 && fadeMultiplier != 0f)
		{
			isCleaning = true;
		}
		if (isCleaning)
		{
			cleanStateTimer = 0f;
			if (currentState == State.Dirty || currentState == State.Cleaning)
			{
				if (currentState != State.Cleaning)
				{
					StartWiggle();
					currentState = State.Cleaning;
				}
			}
			else
			{
				isCleaning = false;
			}
		}
		if (currentState == State.Cleaning)
		{
			if (!DebugNoClean)
			{
				if ((double)fadeMultiplier > 0.5)
				{
					fadeMultiplier -= cleaningSpeed * Time.deltaTime;
				}
				else
				{
					fadeMultiplier -= cleaningSpeed * 2f * Time.deltaTime;
				}
			}
			SetMaterialAlpha(dirt1Renderer, fadeMultiplier);
			SetMaterialAlpha(dirt2Renderer, fadeMultiplier);
			SetMaterialAlpha(dirtHangRenderer, fadeMultiplier);
			cleanStateTimer += 1f * Time.deltaTime;
			if ((double)cleanStateTimer > 0.2)
			{
				PaintingSwingEnd.Play(base.transform.position);
				currentState = State.Dirty;
			}
			if (fadeMultiplier < 0f)
			{
				PaintingSwingEnd.Play(base.transform.position);
				currentState = State.Clean;
				cleanEffect.Clean();
				InteractableLight.StartFading();
				fadeMultiplier = 0f;
				InteractionArea.SetActive(value: false);
			}
		}
		if (currentState != State.Cleaning)
		{
			StopWiggle();
		}
		if (GameManager.instance.gameMode == 1)
		{
			if (cleanInput && cleanInput != cleanInputPrevious)
			{
				photonView.RPC("StartCleaningRPC", RpcTarget.All);
			}
			if (!cleanInput && cleanInput != cleanInputPrevious)
			{
				photonView.RPC("StopCleaningRPC", RpcTarget.All);
			}
			if (currentState == State.Clean && currentState != previousState)
			{
				photonView.RPC("CleaningDoneRPC", RpcTarget.AllBuffered);
				previousState = currentState;
			}
			cleanInputPrevious = cleanInput;
			cleanInput = multiplayerCleaning;
		}
		PaintingSwingLoop.PlayLoop(isCleaning, 1f, 2f);
		isCleaning = false;
		if (cleanInput)
		{
			isCleaningTimer = 0.1f;
			cleanInput = false;
		}
	}

	private void SetMaterialAlpha(MeshRenderer renderer, float alpha)
	{
		Color color = renderer.material.color;
		color.a = Mathf.Clamp(alpha, 0f, 1f);
		renderer.material.color = color;
	}

	private void StartWiggle()
	{
		isWiggling = true;
		DustParticles.Play();
		currentWiggleAmount = wiggleAmount;
		StopAllCoroutines();
		StartCoroutine(WiggleCoroutine());
	}

	private void StopWiggle()
	{
		DustParticles.Stop();
		isWiggling = false;
	}

	private IEnumerator WiggleCoroutine()
	{
		float time = 0f;
		float currentZRotation = Painting.transform.localRotation.eulerAngles.z;
		if (currentZRotation > 180f)
		{
			currentZRotation -= 360f;
		}
		float phaseOffset = Mathf.Asin(currentZRotation / wiggleAmount) - wiggleSpeed * time;
		float lerpFactor = 0.15f;
		while (isWiggling || Mathf.Abs(currentWiggleAmount) > 0.1f)
		{
			float num = Mathf.Sin(time * wiggleSpeed + phaseOffset) * currentWiggleAmount;
			float num2 = Mathf.Lerp(currentZRotation, num, lerpFactor);
			Painting.transform.localRotation = Quaternion.Euler(0f, 0f, num2);
			currentZRotation = num2;
			if (!isWiggling)
			{
				currentWiggleAmount *= dampening;
			}
			time += Time.deltaTime;
			yield return null;
		}
	}
}
