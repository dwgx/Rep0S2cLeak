using System;
using System.Collections.Generic;
using UnityEngine;

public class Materials : MonoBehaviour
{
	public enum Type
	{
		None,
		Wood,
		Rug,
		Tile,
		Stone,
		Catwalk,
		Snow,
		Metal,
		Wetmetal,
		Gravel,
		Grass,
		Water,
		Vent,
		Beam,
		Garbage,
		Tarp,
		Rubble,
		Brokentiles
	}

	public enum SoundType
	{
		Light,
		Medium,
		Heavy
	}

	public enum HostType
	{
		LocalPlayer,
		OtherPlayer,
		Enemy
	}

	[Serializable]
	public class MaterialTrigger
	{
		internal MaterialPreset LastMaterial;

		internal Type LastMaterialType;

		internal MaterialSlidingLoop SlidingLoopObject;

		internal int LastMaterialResetCount;

		internal bool OverrideLayerMask;

		internal LayerMask LayerMask;
	}

	public static Materials Instance;

	public LayerMask LayerMask;

	[Space]
	public Transform ParticleParent;

	public List<GameObject> Particles = new List<GameObject>();

	public List<MaterialPreset> MaterialList;

	private MaterialPreset LastMaterial;

	private void Awake()
	{
		Instance = this;
	}

	public void Impulse(Vector3 origin, Vector3 direction, SoundType soundType, bool footstep, bool footstepParticles, MaterialTrigger materialTrigger, HostType hostType)
	{
		Vector3 material = GetMaterial(origin, materialTrigger);
		if (!LastMaterial)
		{
			return;
		}
		float volumeMultiplier = 1f;
		float falloffMultiplier = 1f;
		float offscreenVolumeMultiplier = 1f;
		float offscreenFalloffMultiplier = 1f;
		switch (hostType)
		{
		case HostType.OtherPlayer:
			volumeMultiplier = 0.5f;
			break;
		case HostType.Enemy:
			volumeMultiplier = 0.5f;
			falloffMultiplier = 0.5f;
			offscreenVolumeMultiplier = 0.25f;
			offscreenFalloffMultiplier = 0.25f;
			break;
		}
		switch (soundType)
		{
		case SoundType.Light:
			if (footstep)
			{
				if (LastMaterial.RareFootstepLightMax > 0)
				{
					LastMaterial.RareFootstepLightCurrent -= 1f;
					if (LastMaterial.RareFootstepLightCurrent <= 0f)
					{
						LastMaterial.RareFootstepLight.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
						LastMaterial.RareFootstepLightCurrent = UnityEngine.Random.Range(LastMaterial.RareFootstepLightMin, LastMaterial.RareFootstepLightMax);
					}
				}
				if (LastMaterial.FootstepLight.Sounds.Length == 0)
				{
					Debug.LogError("Material - No light footstep sounds assigned to: " + LastMaterial.Name);
				}
				LastMaterial.FootstepLight.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
				break;
			}
			if (LastMaterial.RareImpactLightMax > 0)
			{
				LastMaterial.RareImpactLightCurrent -= 1f;
				if (LastMaterial.RareImpactLightCurrent <= 0f)
				{
					LastMaterial.RareImpactLight.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
					LastMaterial.RareImpactLightCurrent = UnityEngine.Random.Range(LastMaterial.RareImpactLightMin, LastMaterial.RareImpactLightMax);
				}
			}
			if (LastMaterial.ImpactLight.Sounds.Length == 0)
			{
				Debug.LogError("Material - No light impact sounds assigned to: " + LastMaterial.Name);
			}
			LastMaterial.ImpactLight.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
			break;
		case SoundType.Medium:
			if (footstep)
			{
				if (LastMaterial.RareFootstepMediumMax > 0)
				{
					LastMaterial.RareFootstepMediumCurrent -= 1f;
					if (LastMaterial.RareFootstepMediumCurrent <= 0f)
					{
						LastMaterial.RareFootstepMedium.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
						LastMaterial.RareFootstepMediumCurrent = UnityEngine.Random.Range(LastMaterial.RareFootstepMediumMin, LastMaterial.RareFootstepMediumMax);
					}
				}
				if (LastMaterial.FootstepMedium.Sounds.Length == 0)
				{
					Debug.LogError("Material - No medium footstep sounds assigned to: " + LastMaterial.Name);
				}
				LastMaterial.FootstepMedium.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
				break;
			}
			if (LastMaterial.RareImpactMediumMax > 0)
			{
				LastMaterial.RareImpactMediumCurrent -= 1f;
				if (LastMaterial.RareImpactMediumCurrent <= 0f)
				{
					LastMaterial.RareImpactMedium.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
					LastMaterial.RareImpactMediumCurrent = UnityEngine.Random.Range(LastMaterial.RareImpactMediumMin, LastMaterial.RareImpactMediumMax);
				}
			}
			if (LastMaterial.ImpactMedium.Sounds.Length == 0)
			{
				Debug.LogError("Material - No medium impact sounds assigned to: " + LastMaterial.Name);
			}
			LastMaterial.ImpactMedium.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
			break;
		case SoundType.Heavy:
			if (footstep)
			{
				if (LastMaterial.RareFootstepHeavyMax > 0)
				{
					LastMaterial.RareFootstepHeavyCurrent -= 1f;
					if (LastMaterial.RareFootstepHeavyCurrent <= 0f)
					{
						LastMaterial.RareFootstepHeavy.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
						LastMaterial.RareFootstepHeavyCurrent = UnityEngine.Random.Range(LastMaterial.RareFootstepHeavyMin, LastMaterial.RareFootstepHeavyMax);
					}
				}
				if (LastMaterial.FootstepHeavy.Sounds.Length == 0)
				{
					Debug.LogError("Material - No heavy footstep sounds assigned to: " + LastMaterial.Name);
				}
				LastMaterial.FootstepHeavy.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
				break;
			}
			if (LastMaterial.RareImpactHeavyMax > 0)
			{
				LastMaterial.RareImpactHeavyCurrent -= 1f;
				if (LastMaterial.RareImpactHeavyCurrent <= 0f)
				{
					LastMaterial.RareImpactHeavy.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
					LastMaterial.RareImpactHeavyCurrent = UnityEngine.Random.Range(LastMaterial.RareImpactHeavyMin, LastMaterial.RareImpactHeavyMax);
				}
			}
			if (LastMaterial.ImpactHeavy.Sounds.Length == 0)
			{
				Debug.LogError("Material - No heavy impact sounds assigned to: " + LastMaterial.Name);
			}
			LastMaterial.ImpactHeavy.Play(material, volumeMultiplier, falloffMultiplier, offscreenVolumeMultiplier, offscreenFalloffMultiplier);
			break;
		}
		if (Particles.Count < 25 && ((bool)LastMaterial.FootstepPrefab || (bool)LastMaterial.ImpactPrefab) && Vector3.Distance(material, AssetManager.instance.mainCamera.transform.position) < 15f)
		{
			if (footstepParticles)
			{
				UnityEngine.Object.Instantiate(LastMaterial.FootstepPrefab, material, Quaternion.LookRotation(-direction), ParticleParent);
			}
			else
			{
				UnityEngine.Object.Instantiate(LastMaterial.ImpactPrefab, material, Quaternion.LookRotation(-direction), ParticleParent);
			}
		}
	}

	public void Slide(Vector3 origin, MaterialTrigger materialTrigger, float spatialBlend, bool isPlayer)
	{
		float volumeMultiplier = 1f;
		if (!isPlayer)
		{
			volumeMultiplier = 0.5f;
		}
		Vector3 material = GetMaterial(origin, materialTrigger);
		if ((bool)LastMaterial)
		{
			if (LastMaterial.SlideOneShot.Sounds.Length == 0)
			{
				Debug.LogError("Material - No slide sound assigned to: " + LastMaterial.Name);
			}
			LastMaterial.SlideOneShot.SpatialBlend = spatialBlend;
			LastMaterial.SlideOneShot.Play(material, volumeMultiplier);
		}
	}

	public void SlideLoop(Vector3 origin, MaterialTrigger materialTrigger, float spatialBlend, float pitchMultiplier)
	{
		Vector3 position = origin;
		bool flag = materialTrigger.SlidingLoopObject != null;
		if (!flag || materialTrigger.SlidingLoopObject.getMaterialTimer <= 0f)
		{
			position = GetMaterial(origin, materialTrigger);
			if (flag)
			{
				materialTrigger.SlidingLoopObject.getMaterialTimer = 0.25f;
			}
		}
		if (materialTrigger.LastMaterial != null && materialTrigger.LastMaterial.SlideLoop != null)
		{
			bool flag2 = false;
			if (!flag)
			{
				flag2 = true;
			}
			else if (materialTrigger.SlidingLoopObject.material != materialTrigger.LastMaterial)
			{
				materialTrigger.SlidingLoopObject = null;
				flag2 = true;
			}
			if (flag2)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(AudioManager.instance.AudioMaterialSlidingLoop, position, Quaternion.identity, AudioManager.instance.SoundsParent);
				materialTrigger.SlidingLoopObject = gameObject.GetComponent<MaterialSlidingLoop>();
				materialTrigger.SlidingLoopObject.material = materialTrigger.LastMaterial;
				Sound.CopySound(materialTrigger.SlidingLoopObject.material.SlideLoop, materialTrigger.SlidingLoopObject.sound);
				materialTrigger.SlidingLoopObject.sound.Source = materialTrigger.SlidingLoopObject.source;
			}
			materialTrigger.SlidingLoopObject.activeTimer = 0.1f;
			materialTrigger.SlidingLoopObject.transform.position = position;
			materialTrigger.SlidingLoopObject.pitchMultiplier = pitchMultiplier;
		}
	}

	private Vector3 GetMaterial(Vector3 origin, MaterialTrigger materialTrigger)
	{
		origin = new Vector3(origin.x, origin.y + 0.1f, origin.z);
		LayerMask layerMask = LayerMask;
		if (materialTrigger.OverrideLayerMask)
		{
			layerMask = materialTrigger.LayerMask;
		}
		Type _type = materialTrigger.LastMaterialType;
		bool flag = false;
		if (Physics.Raycast(origin, Vector3.down, out var hitInfo, 1f, layerMask, QueryTriggerInteraction.Collide))
		{
			MaterialSurface component = hitInfo.collider.gameObject.GetComponent<MaterialSurface>();
			if ((bool)component)
			{
				_type = component.Type;
				origin = hitInfo.point;
				flag = true;
			}
		}
		if (!flag)
		{
			materialTrigger.LastMaterialResetCount++;
			if (materialTrigger.LastMaterialResetCount >= 2)
			{
				_type = Type.Wood;
				materialTrigger.LastMaterialResetCount = 0;
			}
		}
		LastMaterial = MaterialList.Find((MaterialPreset x) => x.Type == _type);
		materialTrigger.LastMaterialType = _type;
		materialTrigger.LastMaterial = LastMaterial;
		return origin;
	}
}
