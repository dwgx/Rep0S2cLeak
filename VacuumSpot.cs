using Photon.Pun;
using UnityEngine;

public class VacuumSpot : MonoBehaviour
{
	public GameObject VacuumSpotVisual;

	[HideInInspector]
	public float Amount = 1f;

	public float DecreaseSpeed;

	[HideInInspector]
	public bool CleanDone;

	[Space]
	public Transform PileMesh;

	public MeshRenderer PileRenderer;

	private float PileRendererAlpha;

	[Space]
	public Transform DecalMesh;

	public MeshRenderer DecalRenderer;

	private float DecalRendererAlpha;

	[Space]
	public AnimationCurve ScaleCurve;

	public AnimationCurve AlphaCurve;

	public Light Light;

	private float LightIntensity;

	[HideInInspector]
	public bool Decreasing;

	[HideInInspector]
	public float DecreaseTimer;

	public GameObject CleanEffect;

	private bool multiplayerCleaning;

	private PhotonView photonView;

	private bool cleanInputPrevious;

	public bool cleanInput;

	private bool syncDestroy;

	private void Start()
	{
		LightIntensity = Light.intensity;
		photonView = GetComponent<PhotonView>();
		PileRendererAlpha = PileRenderer.material.color.a;
		DecalRendererAlpha = DecalRenderer.material.color.a;
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

	private void Update()
	{
		if (DecreaseTimer > 0f)
		{
			Amount -= DecreaseSpeed * Time.deltaTime;
			if (Amount > 0.2f)
			{
				DecreaseTimer -= 1f * Time.deltaTime;
			}
			float num = Mathf.Lerp(0f, 1f, AlphaCurve.Evaluate(Amount));
			Color color = PileRenderer.material.color;
			color.a = PileRendererAlpha * num;
			PileRenderer.material.color = color;
			Color color2 = DecalRenderer.material.color;
			color2.a = DecalRendererAlpha * num;
			DecalRenderer.material.color = color2;
			float num2 = Mathf.Lerp(0f, 1f, ScaleCurve.Evaluate(Amount));
			PileMesh.localScale = new Vector3(1f - (1f - num2) * 0.4f, 0.5f + num2 * 0.5f, 1f - (1f - num2) * 0.4f);
			DecalMesh.localScale = new Vector3(1f - (1f - num2) * 0.2f, 1f, 1f - (1f - num2) * 0.2f);
			Light.intensity = LightIntensity * num2;
			if (Amount <= 0f)
			{
				CleanEffect.SetActive(value: true);
				CleanEffect.GetComponent<CleanEffect>().Clean();
				CleanEffect.transform.parent = null;
				if (GameManager.instance.gameMode == 1)
				{
					if (PhotonNetwork.IsMasterClient && !syncDestroy)
					{
						PhotonNetwork.Destroy(base.gameObject);
						syncDestroy = true;
					}
				}
				else
				{
					Object.Destroy(base.gameObject);
				}
			}
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
			cleanInputPrevious = cleanInput;
			cleanInput = multiplayerCleaning;
		}
		if (cleanInput)
		{
			DecreaseTimer = 0.1f;
			cleanInput = false;
		}
	}
}
