using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightManager : MonoBehaviour
{
	[HideInInspector]
	public Transform lightCullTarget;

	public float checkDistance = 5f;

	public float fadeTimeMin = 1f;

	public float fadeTimeMax = 1f;

	public AnimationCurve fadeCurve;

	public AnimationCurve fadeCullCurve;

	public static LightManager instance;

	internal List<PropLight> propLights = new List<PropLight>();

	private List<PropLightEmission> propEmissions = new List<PropLightEmission>();

	private Vector3 lastCheckPos;

	private float lastYRotation;

	internal int activeLightsAmount;

	internal bool updateInstant;

	internal float updateInstantTimer;

	[Space]
	[Header("Sounds")]
	public Sound lampFlickerSound;

	private bool turnOffLights;

	private bool turningOffLights;

	private bool turningOffEmissions;

	private float logicUpdateTimer;

	private bool setup;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		if (GameDirector.instance.currentState != GameDirector.gameState.Main || !PlayerAvatar.instance)
		{
			return;
		}
		if (!lightCullTarget)
		{
			lightCullTarget = PlayerAvatar.instance.transform;
		}
		LogicUpdate();
		if ((bool)DebugUI.instance && DebugUI.instance.enableParent.activeSelf)
		{
			int num = 0;
			foreach (PropLight propLight in propLights)
			{
				if ((bool)propLight && propLight.gameObject.activeInHierarchy)
				{
					num++;
				}
			}
			activeLightsAmount = num;
		}
		if (updateInstant)
		{
			updateInstantTimer -= Time.deltaTime;
			if (updateInstantTimer <= 0f)
			{
				updateInstant = false;
			}
		}
		if (RoundDirector.instance.allExtractionPointsCompleted && !turnOffLights)
		{
			StopAllCoroutines();
			turningOffLights = true;
			StartCoroutine(TurnOffLights());
			turningOffEmissions = true;
			StartCoroutine(TurnOffEmissions());
			turnOffLights = true;
		}
	}

	private IEnumerator TurnOffLights()
	{
		int _lightsPerFrame = 5;
		int _lightsPerFrameCounter = 0;
		foreach (PropLight item in propLights.ToList())
		{
			if ((bool)item && item.levelLight)
			{
				item.lightComponent.intensity = 0f;
				item.originalIntensity = 0f;
				if (item.hasHalo)
				{
					item.halo.enabled = false;
				}
				item.turnedOff = true;
				_lightsPerFrameCounter++;
				if (_lightsPerFrameCounter >= _lightsPerFrame)
				{
					_lightsPerFrameCounter = 0;
					yield return null;
				}
			}
		}
		turningOffLights = false;
	}

	private IEnumerator TurnOffEmissions()
	{
		int _emissionsPerFrame = 5;
		int _emissionsPerFrameCounter = 0;
		foreach (PropLightEmission item in propEmissions.ToList())
		{
			if ((bool)item && item.levelLight)
			{
				item.material.SetColor("_EmissionColor", Color.black);
				item.originalEmission = Color.black;
				item.turnedOff = true;
				_emissionsPerFrameCounter++;
				if (_emissionsPerFrameCounter >= _emissionsPerFrame)
				{
					_emissionsPerFrameCounter = 0;
					yield return null;
				}
			}
		}
		turningOffEmissions = false;
	}

	private void Setup()
	{
		setup = true;
		if ((bool)lightCullTarget)
		{
			lastCheckPos = lightCullTarget.position;
		}
		GameObject[] array = GameObject.FindGameObjectsWithTag("Prop Lights");
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				PropLight component = gameObject.GetComponent<PropLight>();
				if ((bool)component)
				{
					propLights.Add(component);
				}
				else
				{
					Debug.LogError("PropLight component not found in " + gameObject.name, gameObject);
				}
			}
		}
		array = GameObject.FindGameObjectsWithTag("Prop Emission");
		foreach (GameObject gameObject2 in array)
		{
			if ((bool)gameObject2)
			{
				PropLightEmission component2 = gameObject2.GetComponent<PropLightEmission>();
				if ((bool)component2)
				{
					propEmissions.Add(component2);
				}
				else
				{
					Debug.LogError("PropLightEmission component not found in " + gameObject2.name, gameObject2);
				}
			}
		}
		foreach (PropLight propLight in propLights)
		{
			if (!propLight.turnedOff)
			{
				HandleLightActivation(propLight);
			}
		}
		foreach (PropLightEmission propEmission in propEmissions)
		{
			if (!propEmission.turnedOff)
			{
				HandleEmissionActivation(propEmission);
			}
		}
	}

	private void LogicUpdate()
	{
		if (!LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (!setup)
		{
			Setup();
		}
		if (logicUpdateTimer > 0f)
		{
			logicUpdateTimer -= Time.deltaTime;
		}
		else if ((bool)lightCullTarget)
		{
			bool flag = false;
			if (Mathf.Abs(lightCullTarget.eulerAngles.y - lastYRotation) >= 20f)
			{
				lastYRotation = lightCullTarget.eulerAngles.y;
				flag = true;
			}
			if (!turningOffLights && !turningOffEmissions && (Vector3.Distance(lastCheckPos, lightCullTarget.position) >= checkDistance || flag))
			{
				logicUpdateTimer = 0.5f;
				UpdateLights();
			}
		}
	}

	private void UpdateLights()
	{
		lastCheckPos = lightCullTarget.position;
		List<PropLight> list = new List<PropLight>();
		foreach (PropLight propLight in propLights)
		{
			if ((bool)propLight)
			{
				HandleLightActivation(propLight);
			}
			else
			{
				list.Add(propLight);
			}
		}
		foreach (PropLight item in list)
		{
			propLights.Remove(item);
		}
		List<PropLightEmission> list2 = new List<PropLightEmission>();
		foreach (PropLightEmission propEmission in propEmissions)
		{
			if ((bool)propEmission)
			{
				HandleEmissionActivation(propEmission);
			}
			else
			{
				list2.Add(propEmission);
			}
		}
		foreach (PropLightEmission item2 in list2)
		{
			propEmissions.Remove(item2);
		}
	}

	public void RemoveLight(PropLight PropLight)
	{
		if ((bool)PropLight && propLights.Contains(PropLight))
		{
			propLights.Remove(PropLight);
		}
	}

	private void HandleLightActivation(PropLight propLight)
	{
		if (!lightCullTarget)
		{
			return;
		}
		Vector3 position = propLight.transform.position;
		Vector3 position2 = lightCullTarget.position;
		bool flag = Vector3.Dot(propLight.transform.position - lightCullTarget.position, lightCullTarget.forward) <= -0.25f;
		if ((bool)SpectateCamera.instance)
		{
			flag = false;
			if (SpectateCamera.instance.CheckState(SpectateCamera.State.Death))
			{
				position.y = 0f;
				position2.y = 0f;
			}
		}
		float num = Vector3.Distance(position, position2);
		float num2 = GraphicsManager.instance.lightDistance * propLight.lightRangeMultiplier;
		if (propLight.gameObject.activeInHierarchy && ((num >= num2 && !flag) || (num >= num2 * 0.8f && flag)))
		{
			StartCoroutine(FadeLightIntensity(propLight, 0f, UnityEngine.Random.Range(fadeTimeMin, fadeTimeMax), delegate
			{
				propLight.gameObject.SetActive(value: false);
			}));
		}
		else if (!propLight.gameObject.activeInHierarchy && num < num2)
		{
			propLight.gameObject.SetActive(value: true);
			propLight.lightComponent.intensity = 0f;
			StartCoroutine(FadeLightIntensity(propLight, propLight.originalIntensity, UnityEngine.Random.Range(fadeTimeMin, fadeTimeMax)));
		}
	}

	public void UpdateInstant()
	{
		if (setup)
		{
			updateInstant = true;
			updateInstantTimer = 0.1f;
			UpdateLights();
		}
	}

	private void HandleEmissionActivation(PropLightEmission _propLightEmission)
	{
		if ((bool)lightCullTarget)
		{
			Vector3 position = _propLightEmission.transform.position;
			Vector3 position2 = lightCullTarget.position;
			if ((bool)SpectateCamera.instance && SpectateCamera.instance.CheckState(SpectateCamera.State.Death))
			{
				position.y = 0f;
				position2.y = 0f;
			}
			if (Vector3.Distance(position, position2) >= GraphicsManager.instance.lightDistance)
			{
				StartCoroutine(FadeEmissionIntensity(_propLightEmission, Color.black, UnityEngine.Random.Range(fadeTimeMin, fadeTimeMax)));
			}
			else
			{
				StartCoroutine(FadeEmissionIntensity(_propLightEmission, _propLightEmission.originalEmission, UnityEngine.Random.Range(fadeTimeMin, fadeTimeMax)));
			}
		}
	}

	private IEnumerator FadeLightIntensity(PropLight propLight, float targetIntensity, float duration, Action onComplete = null)
	{
		if (!propLight || !propLight.lightComponent)
		{
			yield break;
		}
		float startTime = Time.time;
		float startIntensity = propLight.lightComponent.intensity;
		while (Time.time - startTime < duration && !updateInstant)
		{
			if (!propLight || !propLight.lightComponent)
			{
				yield break;
			}
			float time = (Time.time - startTime) / duration;
			propLight.lightComponent.intensity = Mathf.Lerp(startIntensity, targetIntensity, fadeCullCurve.Evaluate(time));
			yield return null;
		}
		if ((bool)propLight && (bool)propLight.lightComponent)
		{
			propLight.lightComponent.intensity = targetIntensity;
			if (Mathf.Approximately(targetIntensity, 0f) && propLight.gameObject.CompareTag("Prop Lights"))
			{
				propLight.gameObject.SetActive(value: false);
			}
			onComplete?.Invoke();
		}
	}

	private IEnumerator FadeEmissionIntensity(PropLightEmission _propLightEmission, Color targetColor, float duration)
	{
		if (!_propLightEmission)
		{
			yield break;
		}
		float startTime = Time.time;
		Color startColor = _propLightEmission.material.GetColor("_EmissionColor");
		while (Time.time - startTime < duration && !updateInstant)
		{
			if (!_propLightEmission)
			{
				yield break;
			}
			float time = (Time.time - startTime) / duration;
			_propLightEmission.material.SetColor("_EmissionColor", Color.Lerp(startColor, targetColor, fadeCullCurve.Evaluate(time)));
			yield return null;
		}
		if ((bool)_propLightEmission)
		{
			_propLightEmission.material.SetColor("_EmissionColor", targetColor);
		}
	}
}
