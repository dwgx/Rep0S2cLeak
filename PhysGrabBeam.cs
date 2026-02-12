using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PhysGrabBeam : MonoBehaviour
{
	public PlayerAvatar playerAvatar;

	public Transform PhysGrabPointOrigin;

	public Transform PhysGrabPointOriginClient;

	public Transform PhysGrabPoint;

	public Transform PhysGrabPointPuller;

	public Material greenScreenMaterial;

	private Material originalMaterial;

	[HideInInspector]
	public Vector3 physGrabPointPullerSmoothPosition;

	public float CurveStrength = 1f;

	public int CurveResolution = 20;

	public LineRenderer lineRendererOverCharge;

	[Header("Texture Scrolling")]
	public Vector2 scrollSpeed = new Vector2(5f, 0f);

	[HideInInspector]
	public Vector2 originalScrollSpeed;

	public LineRenderer lineRenderer;

	[HideInInspector]
	public Material lineMaterial;

	private Material lineMaterialOverCharge;

	private Vector3[] curvePointsOverChargeOffsets;

	public Transform overchargeImpact;

	private List<ParticleSystem> overchargeParticles = new List<ParticleSystem>();

	public Sound soundOverchargeImpact;

	private void Start()
	{
		if (!playerAvatar.isLocal)
		{
			PhysGrabPointOrigin = PhysGrabPointOriginClient;
		}
		originalScrollSpeed = scrollSpeed;
		originalMaterial = lineRenderer.material;
		lineMaterial = lineRenderer.material;
		lineMaterialOverCharge = lineRendererOverCharge.material;
		overchargeParticles.AddRange(overchargeImpact.GetComponentsInChildren<ParticleSystem>());
		overchargeImpact.parent = playerAvatar.transform.parent;
		lineRenderer.enabled = false;
	}

	private void LateUpdate()
	{
		if (!lineRenderer.enabled)
		{
			if (lineRendererOverCharge.enabled)
			{
				lineRendererOverCharge.enabled = false;
			}
		}
		else
		{
			DrawCurve();
			ScrollTexture();
		}
	}

	private void PlayAllOverchargeParticles()
	{
		foreach (ParticleSystem overchargeParticle in overchargeParticles)
		{
			overchargeParticle.Play();
		}
	}

	private void OnEnable()
	{
		physGrabPointPullerSmoothPosition = PhysGrabPointPuller.position;
		if ((bool)VideoGreenScreen.instance)
		{
			lineMaterial = greenScreenMaterial;
			GetComponent<LineRenderer>().material = greenScreenMaterial;
		}
	}

	private void OnDisable()
	{
		lineMaterial = originalMaterial;
		if ((bool)lineRenderer)
		{
			lineRenderer.material = originalMaterial;
		}
	}

	public void OverChargeLaunchPlayer()
	{
		if (playerAvatar.isLocal)
		{
			CameraGlitch.Instance.PlayLong();
			PhysGrabber.instance.physGrabBeamOverChargeFloat = 0.5f;
			playerAvatar.physGrabber.ReleaseObject(-1);
		}
		Vector3 vector = -playerAvatar.localCamera.transform.forward * 0.14f;
		vector += Vector3.up * 0.7f;
		overchargeImpact.rotation = Quaternion.LookRotation(vector);
		overchargeImpact.position = PhysGrabPointOrigin.position;
		playerAvatar.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
		playerAvatar.tumble.TumbleForce(vector * 25f);
		playerAvatar.tumble.TumbleTorque(-playerAvatar.transform.forward * 45f);
		playerAvatar.tumble.TumbleOverrideTime(2.2f);
		playerAvatar.tumble.ImpactHurtSet(3f, 20);
		soundOverchargeImpact.Play(overchargeImpact.position);
		PlayAllOverchargeParticles();
		curvePointsOverChargeOffsets = new Vector3[CurveResolution];
	}

	private void DrawCurve()
	{
		if (!PhysGrabPointPuller)
		{
			return;
		}
		bool flag = false;
		float num = (float)(int)playerAvatar.physGrabber.physGrabBeamOverCharge / 2f;
		if (!(num > 0.05f) || !playerAvatar.physGrabber.grabbedPhysGrabObject || !playerAvatar.physGrabber.grabbedPhysGrabObject.isEnemy)
		{
			if (lineRendererOverCharge.enabled)
			{
				lineRendererOverCharge.enabled = false;
			}
		}
		else
		{
			if (!lineRendererOverCharge.enabled)
			{
				lineRendererOverCharge.enabled = true;
			}
			flag = true;
			lineRendererOverCharge.widthMultiplier = 0.5f * (num / 100f);
		}
		Vector3[] array = new Vector3[CurveResolution];
		Vector3[] array2 = new Vector3[CurveResolution];
		Vector3 position = PhysGrabPointPuller.position;
		physGrabPointPullerSmoothPosition = Vector3.Lerp(physGrabPointPullerSmoothPosition, position, Time.deltaTime * 10f);
		Vector3 p = physGrabPointPullerSmoothPosition * CurveStrength;
		if (flag && SemiFunc.FPSImpulse15())
		{
			float num2 = (float)(int)playerAvatar.physGrabber.physGrabBeamOverCharge / 2f;
			num2 /= 100f;
			curvePointsOverChargeOffsets = new Vector3[CurveResolution];
			float num3 = 0.1f + 0.2f * num2;
			for (int i = 0; i < CurveResolution; i++)
			{
				curvePointsOverChargeOffsets[i] = new Vector3(UnityEngine.Random.Range(0f - num3, num3), UnityEngine.Random.Range(0f - num3, num3), UnityEngine.Random.Range(0f - num3, num3));
			}
		}
		for (int j = 0; j < CurveResolution; j++)
		{
			float num4 = (float)j / ((float)CurveResolution - 1f);
			if (playerAvatar.physGrabber.grabState == PhysGrabber.GrabState.Climb)
			{
				array[j] = Vector3.Lerp(PhysGrabPointOrigin.position, PhysGrabPoint.position, num4);
				Vector3 vector = Vector3.up * Mathf.Sin(Time.time * 10f + num4 * 4f) * 0.1f;
				vector *= Mathf.Sin(num4 * MathF.PI);
				array[j] += vector;
				continue;
			}
			array[j] = CalculateBezierPoint(num4, PhysGrabPointOrigin.position, p, PhysGrabPoint.position);
			if (flag && curvePointsOverChargeOffsets != null)
			{
				array2[j] = array[j];
				curvePointsOverChargeOffsets[j] = Vector3.Lerp(curvePointsOverChargeOffsets[j], Vector3.zero, Time.deltaTime * 30f);
				array2[j] += curvePointsOverChargeOffsets[j];
			}
		}
		lineRenderer.positionCount = CurveResolution;
		lineRenderer.SetPositions(array);
		if (flag)
		{
			lineRendererOverCharge.positionCount = CurveResolution;
			lineRendererOverCharge.SetPositions(array2);
		}
	}

	private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		return Mathf.Pow(1f - t, 2f) * p0 + 2f * (1f - t) * t * p1 + Mathf.Pow(t, 2f) * p2;
	}

	private void ScrollTexture()
	{
		if ((bool)lineMaterial)
		{
			if (playerAvatar.physGrabber.colorState == 1)
			{
				lineMaterial.mainTextureScale = new Vector2(-1f, 1f);
			}
			else
			{
				lineMaterial.mainTextureScale = new Vector2(1f, 1f);
			}
			Vector2 mainTextureOffset = Time.time * scrollSpeed;
			lineMaterial.mainTextureOffset = mainTextureOffset;
			if (lineRendererOverCharge.enabled)
			{
				lineMaterialOverCharge.mainTextureOffset = mainTextureOffset;
			}
		}
	}
}
