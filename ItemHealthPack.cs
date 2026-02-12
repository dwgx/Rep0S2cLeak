using Photon.Pun;
using UnityEngine;

public class ItemHealthPack : MonoBehaviour
{
	public int healAmount;

	private ItemToggle itemToggle;

	private ItemEquippable itemEquippable;

	private ItemAttributes itemAttributes;

	private PhotonView photonView;

	private PhysGrabObject physGrabObject;

	[Space]
	public ParticleSystem[] particles;

	public ParticleSystem[] rejectParticles;

	[Space]
	public PropLight propLight;

	public AnimationCurve lightIntensityCurve;

	private float lightIntensityLerp;

	public MeshRenderer mesh;

	private Material material;

	private Color materialEmissionOriginal;

	private int materialPropertyEmission = Shader.PropertyToID("_EmissionColor");

	[Space]
	public Sound soundUse;

	public Sound soundReject;

	private bool used;

	private void Start()
	{
		itemToggle = GetComponent<ItemToggle>();
		itemEquippable = GetComponent<ItemEquippable>();
		itemAttributes = GetComponent<ItemAttributes>();
		photonView = GetComponent<PhotonView>();
		physGrabObject = GetComponent<PhysGrabObject>();
		material = mesh.material;
		materialEmissionOriginal = material.GetColor(materialPropertyEmission);
	}

	private void Update()
	{
		if (SemiFunc.RunIsShop() || RunManager.instance.levelIsShop)
		{
			return;
		}
		LightLogic();
		if (!SemiFunc.IsMasterClientOrSingleplayer() || !itemToggle.toggleState || used)
		{
			return;
		}
		PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(itemToggle.playerTogglePhotonID);
		if (!playerAvatar)
		{
			return;
		}
		if (playerAvatar.playerHealth.health >= playerAvatar.playerHealth.maxHealth)
		{
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("RejectRPC", RpcTarget.All);
			}
			else
			{
				RejectRPC();
			}
			itemToggle.ToggleItem(toggle: false);
			physGrabObject.rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
			physGrabObject.rb.AddTorque(-physGrabObject.transform.right * 0.05f, ForceMode.Impulse);
		}
		else
		{
			playerAvatar.playerHealth.HealOther(healAmount, effect: true);
			StatsManager.instance.ItemRemove(itemAttributes.instanceName);
			physGrabObject.impactDetector.indestructibleBreakEffects = true;
			if (SemiFunc.IsMultiplayer())
			{
				photonView.RPC("UsedRPC", RpcTarget.All);
			}
			else
			{
				UsedRPC();
			}
		}
	}

	private void LightLogic()
	{
		if (used && lightIntensityLerp < 1f)
		{
			lightIntensityLerp += 1f * Time.deltaTime;
			propLight.lightComponent.intensity = lightIntensityCurve.Evaluate(lightIntensityLerp);
			propLight.originalIntensity = propLight.lightComponent.intensity;
			material.SetColor(materialPropertyEmission, Color.Lerp(Color.black, materialEmissionOriginal, lightIntensityCurve.Evaluate(lightIntensityLerp)));
		}
	}

	[PunRPC]
	private void UsedRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, base.transform.position, 0.2f);
			itemToggle.ToggleDisable(_disable: true);
			itemAttributes.DisableUI(_disable: true);
			Object.Destroy(itemEquippable);
			ParticleSystem[] array = particles;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
			soundUse.Play(base.transform.position);
			used = true;
		}
	}

	[PunRPC]
	private void RejectRPC(PhotonMessageInfo _info = default(PhotonMessageInfo))
	{
		if (SemiFunc.MasterOnlyRPC(_info))
		{
			PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(itemToggle.playerTogglePhotonID);
			if (playerAvatar.isLocal)
			{
				playerAvatar.physGrabber.ReleaseObjectRPC(physGrabEnded: false, 1f, photonView.ViewID);
			}
			ParticleSystem[] array = rejectParticles;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, base.transform.position, 0.2f);
			soundReject.Play(base.transform.position);
		}
	}

	public void OnDestroy()
	{
		ParticleSystem[] array = particles;
		foreach (ParticleSystem particleSystem in array)
		{
			if ((bool)particleSystem && particleSystem.isPlaying)
			{
				particleSystem.transform.SetParent(null);
				ParticleSystem.MainModule main = particleSystem.main;
				main.stopAction = ParticleSystemStopAction.Destroy;
			}
		}
		array = rejectParticles;
		foreach (ParticleSystem particleSystem2 in array)
		{
			if ((bool)particleSystem2 && particleSystem2.isPlaying)
			{
				particleSystem2.transform.SetParent(null);
				ParticleSystem.MainModule main2 = particleSystem2.main;
				main2.stopAction = ParticleSystemStopAction.Destroy;
			}
		}
	}
}
