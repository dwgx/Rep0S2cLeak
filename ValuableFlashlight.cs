using System.Collections;
using UnityEngine;

public class ValuableFlashlight : MonoBehaviour
{
	public Sound clickOn;

	public Sound clickOff;

	public Sound flickerOn;

	public Sound flickerOff;

	public GameObject bigLight;

	public CollisionFree colliderCollision;

	private MeshRenderer meshRenderer;

	private bool grabbedPrev;

	private bool skipAimLogic = true;

	private PhysGrabObject physGrabObject;

	private Coroutine flickerRoutine;

	private void Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
		meshRenderer = GetComponentInChildren<MeshRenderer>();
		bigLight.SetActive(value: false);
		meshRenderer.material.DisableKeyword("_EMISSION");
	}

	private void Update()
	{
		if (physGrabObject.grabbed)
		{
			if (!grabbedPrev)
			{
				clickOn.Play(physGrabObject.centerPoint);
				if (flickerRoutine != null)
				{
					StopCoroutine(flickerRoutine);
				}
				flickerRoutine = StartCoroutine(Flicker(_turnOn: true));
				grabbedPrev = true;
			}
		}
		else if (grabbedPrev)
		{
			clickOff.Play(physGrabObject.centerPoint);
			if (flickerRoutine != null)
			{
				StopCoroutine(flickerRoutine);
			}
			flickerRoutine = StartCoroutine(Flicker(_turnOn: false));
			skipAimLogic = true;
			grabbedPrev = false;
		}
	}

	public void ImpactFlicker()
	{
		if (!SemiFunc.Photosensitivity() && !physGrabObject.impactDetector.inCart)
		{
			if (flickerRoutine != null)
			{
				StopCoroutine(flickerRoutine);
			}
			flickerRoutine = StartCoroutine(Flicker(_turnOn: true, !physGrabObject.grabbed));
		}
	}

	public void aimLogic()
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || (skipAimLogic && colliderCollision.colliding))
		{
			return;
		}
		if (!colliderCollision.colliding)
		{
			skipAimLogic = false;
		}
		float num = -0.2f;
		float x = -5f;
		if (physGrabObject.playerGrabbing.Count <= 0)
		{
			return;
		}
		Quaternion turnX = Quaternion.Euler(x, 0f, 0f);
		Quaternion turnY = Quaternion.Euler(0f, 0f, 0f);
		Quaternion turnZ = Quaternion.Euler(0f, 0f, base.transform.rotation.eulerAngles.z);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = true;
		foreach (PhysGrabber item in physGrabObject.playerGrabbing)
		{
			if (flag4)
			{
				if (item.playerAvatar.isCrouching)
				{
					flag2 = true;
				}
				if (item.playerAvatar.isCrawling)
				{
					flag3 = true;
				}
				flag4 = false;
			}
			if (item.isRotating)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			physGrabObject.TurnXYZ(turnX, turnY, turnZ);
		}
		float num2 = num;
		if (flag2)
		{
			num2 += 0.5f;
		}
		if (flag3)
		{
			num2 -= 0.5f;
		}
		physGrabObject.OverrideGrabVerticalPosition(num2);
		if (!flag && physGrabObject.grabbed)
		{
			physGrabObject.OverrideTorqueStrength(12f);
			physGrabObject.OverrideAngularDrag(20f);
		}
		if (flag)
		{
			physGrabObject.OverrideTorqueStrength(2f);
			physGrabObject.OverrideAngularDrag(20f);
		}
	}

	private IEnumerator Flicker(bool _turnOn, bool _dropped = false)
	{
		for (int i = 0; i < Random.Range(1, 4); i++)
		{
			bool flag = _turnOn && !SemiFunc.Photosensitivity() && i % 2 == 0;
			bigLight.SetActive(flag);
			if (flag)
			{
				meshRenderer.material.EnableKeyword("_EMISSION");
				flickerOn.Play(physGrabObject.centerPoint);
			}
			else
			{
				meshRenderer.material.DisableKeyword("_EMISSION");
				flickerOff.Play(physGrabObject.centerPoint);
			}
			yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
		}
		bigLight.SetActive(_turnOn && !_dropped);
		if (_turnOn && !_dropped)
		{
			meshRenderer.material.EnableKeyword("_EMISSION");
			flickerOn.Play(physGrabObject.centerPoint);
		}
		else
		{
			meshRenderer.material.DisableKeyword("_EMISSION");
			flickerOff.Play(physGrabObject.centerPoint);
		}
	}
}
