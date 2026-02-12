using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioLowPassFilter))]
public class AudioLowPassLogic : MonoBehaviour
{
	public bool LowPass;

	[Space]
	public bool ForceStart;

	public bool AlwaysActive;

	[Space]
	public bool HasCustomVolume;

	[Range(0f, 1f)]
	public float CustomVolume = 0.5f;

	private float VolumeMultiplier = 0.5f;

	[Space]
	public bool HasCustomFalloff;

	[Range(0f, 1f)]
	public float CustomFalloff = 0.8f;

	private float FalloffMultiplier = 0.8f;

	internal bool Fetch = true;

	private bool First = true;

	private bool LogicActive;

	internal float Falloff;

	private float LowPassMin;

	private float LowPassMax;

	private AudioLowPassFilter AudioLowpassFilter;

	private AudioSource AudioSource;

	private LayerMask LayerMask;

	private LayerMask LayerMaskOverlap;

	internal bool volumeFetched;

	internal float Volume;

	internal float SpatialBlend;

	internal List<Collider> LowPassIgnoreColliders = new List<Collider>();

	private Transform audioListener;

	private void Start()
	{
		if (ForceStart)
		{
			Setup();
		}
	}

	public void Setup()
	{
		if (Fetch)
		{
			audioListener = AudioListenerFollow.instance.transform;
			AudioLowpassFilter = GetComponent<AudioLowPassFilter>();
			AudioSource = GetComponent<AudioSource>();
			LowPassMin = AudioManager.instance.lowpassValueMin;
			LowPassMax = AudioManager.instance.lowpassValueMax;
			if (HasCustomFalloff)
			{
				FalloffMultiplier = CustomFalloff;
			}
			Falloff = AudioSource.maxDistance;
			SpatialBlend = AudioSource.spatialBlend;
			LayerMask = LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectHinge");
			LayerMaskOverlap = LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectHinge", "LowPassTrigger");
			if (!volumeFetched)
			{
				if (HasCustomVolume)
				{
					VolumeMultiplier = CustomVolume;
				}
				volumeFetched = true;
				Volume = AudioSource.volume;
			}
			Fetch = false;
		}
		CheckStart();
	}

	private void Update()
	{
		if (!LogicActive)
		{
			return;
		}
		if (LowPass)
		{
			if (AudioLowpassFilter.cutoffFrequency != LowPassMin || AudioSource.maxDistance != Falloff * FalloffMultiplier || Mathf.Abs(AudioSource.volume - Volume * VolumeMultiplier) > 0.001f)
			{
				AudioLowpassFilter.cutoffFrequency -= (LowPassMax - LowPassMin) * 10f * Time.deltaTime;
				AudioLowpassFilter.cutoffFrequency = Mathf.Clamp(AudioLowpassFilter.cutoffFrequency, LowPassMin, LowPassMax);
				float t = (AudioLowpassFilter.cutoffFrequency - LowPassMin) / (LowPassMax - LowPassMin);
				AudioSource.maxDistance = Mathf.Lerp(Falloff * FalloffMultiplier, Falloff, t);
				AudioSource.volume = Mathf.Lerp(Volume * VolumeMultiplier, Volume, t);
				if (AudioSource.spatialBlend != 0f)
				{
					AudioSource.spatialBlend = Mathf.Lerp(1f, SpatialBlend, t);
				}
			}
		}
		else if (AudioLowpassFilter.cutoffFrequency != LowPassMax || AudioSource.maxDistance != Falloff || Mathf.Abs(AudioSource.volume - Volume) > 0.001f)
		{
			AudioLowpassFilter.cutoffFrequency += (LowPassMax - LowPassMin) * 1f * Time.deltaTime;
			AudioLowpassFilter.cutoffFrequency = Mathf.Clamp(AudioLowpassFilter.cutoffFrequency, LowPassMin, LowPassMax);
			float t2 = (AudioLowpassFilter.cutoffFrequency - LowPassMin) / (LowPassMax - LowPassMin);
			AudioSource.maxDistance = Mathf.Lerp(Falloff * FalloffMultiplier, Falloff, t2);
			AudioSource.volume = Mathf.Lerp(Volume * VolumeMultiplier, Volume, t2);
			if (AudioSource.spatialBlend != 0f)
			{
				AudioSource.spatialBlend = Mathf.Lerp(1f, SpatialBlend, t2);
			}
		}
		First = false;
	}

	private void OnEnable()
	{
		if (!Fetch)
		{
			CheckStart();
		}
	}

	private void OnDisable()
	{
		LogicActive = false;
		StopAllCoroutines();
	}

	private void CheckStart()
	{
		if (!LogicActive)
		{
			First = true;
			if (base.gameObject.activeInHierarchy)
			{
				StartCoroutine(Check());
			}
		}
	}

	private IEnumerator Check()
	{
		LogicActive = true;
		while ((bool)AudioSource && (AlwaysActive || !AudioSource.loop || AudioSource.isPlaying || First) && !Fetch)
		{
			if (!audioListener)
			{
				if (!AudioListenerFollow.instance)
				{
					yield return null;
					continue;
				}
				audioListener = AudioListenerFollow.instance.transform;
			}
			CheckLogic();
			yield return new WaitForSeconds(0.25f);
		}
		LogicActive = false;
		First = true;
	}

	private void CheckLogic()
	{
		LowPass = true;
		bool flag = SpectateCamera.instance;
		if (!audioListener || !AudioSource || AudioSource.spatialBlend <= 0f || (flag && SpectateCamera.instance.CheckState(SpectateCamera.State.Death)))
		{
			LowPass = false;
		}
		else
		{
			Vector3 direction = audioListener.position - base.transform.position;
			if (direction.magnitude < 20f)
			{
				LowPass = false;
				LowPassTrigger lowPassTrigger = null;
				Collider[] array = Physics.OverlapSphere(base.transform.position, 0.1f, LayerMaskOverlap, QueryTriggerInteraction.Collide);
				List<Collider> list = new List<Collider>();
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					if (collider.transform.CompareTag("LowPassTrigger"))
					{
						lowPassTrigger = collider.GetComponent<LowPassTrigger>();
						if (!lowPassTrigger)
						{
							lowPassTrigger = collider.GetComponentInParent<LowPassTrigger>();
						}
						if ((bool)lowPassTrigger)
						{
							break;
						}
					}
					if (collider.transform.CompareTag("Wall"))
					{
						list.Add(collider);
					}
				}
				if ((bool)AudioListenerFollow.instance.lowPassTrigger)
				{
					if (AudioListenerFollow.instance.lowPassTrigger != lowPassTrigger)
					{
						LowPass = true;
					}
					else
					{
						LowPass = false;
					}
				}
				else if ((bool)lowPassTrigger)
				{
					LowPass = true;
				}
				else
				{
					RaycastHit[] array3 = Physics.RaycastAll(base.transform.position, direction, direction.magnitude, LayerMask, QueryTriggerInteraction.Collide);
					for (int i = 0; i < array3.Length; i++)
					{
						RaycastHit raycastHit = array3[i];
						if (!raycastHit.collider.transform.CompareTag("Wall"))
						{
							continue;
						}
						bool flag2 = true;
						foreach (Collider item in list)
						{
							if (item.transform == raycastHit.collider.transform)
							{
								flag2 = false;
								break;
							}
						}
						if (!flag2)
						{
							continue;
						}
						bool flag3 = false;
						if (LowPassIgnoreColliders.Count > 0)
						{
							foreach (Collider lowPassIgnoreCollider in LowPassIgnoreColliders)
							{
								if ((bool)lowPassIgnoreCollider && lowPassIgnoreCollider.transform == raycastHit.collider.transform)
								{
									flag3 = true;
									break;
								}
							}
						}
						if (!flag3)
						{
							LowPass = true;
							break;
						}
					}
				}
			}
		}
		if (!First)
		{
			return;
		}
		if ((bool)AudioSource)
		{
			if (LowPass)
			{
				AudioLowpassFilter.cutoffFrequency = LowPassMin;
				AudioSource.maxDistance = Falloff * FalloffMultiplier;
				AudioSource.volume = Volume * VolumeMultiplier;
			}
			else
			{
				AudioLowpassFilter.cutoffFrequency = LowPassMax;
				AudioSource.maxDistance = Falloff;
				AudioSource.volume = Volume;
			}
		}
		First = false;
	}
}
