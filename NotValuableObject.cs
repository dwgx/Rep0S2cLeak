using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NotValuableObject : MonoBehaviour
{
	public PhysAttribute physAttributePreset;

	public PhysAudio audioPreset;

	public Durability durabilityPreset;

	public Gradient particleColors;

	[Range(0.5f, 3f)]
	public float audioPresetPitch = 1f;

	private NavMeshObstacle navMeshObstacle;

	private PhysGrabObject physGrabObject;

	private Rigidbody rb;

	[Space]
	public bool hasHealth;

	private int healthCurrent;

	public int healthMax;

	public int healthLossOnBreakLight;

	public int healthLossOnBreakMedium;

	public int healthLossOnBreakHeavy;

	private void Start()
	{
		physGrabObject = GetComponent<PhysGrabObject>();
		navMeshObstacle = GetComponent<NavMeshObstacle>();
		if ((bool)navMeshObstacle)
		{
			Debug.LogError(base.gameObject.name + " has a NavMeshObstacle component. Please remove it.", base.gameObject);
		}
		StartCoroutine(EnableRigidbody());
		rb = GetComponent<Rigidbody>();
		if ((bool)rb)
		{
			rb.mass = physAttributePreset.mass;
		}
		if ((bool)physGrabObject)
		{
			physGrabObject.massOriginal = physAttributePreset.mass;
		}
		if (hasHealth)
		{
			healthCurrent = healthMax;
		}
	}

	public void Impact(PhysGrabObjectImpactDetector.ImpactState _impactState)
	{
		switch (_impactState)
		{
		case PhysGrabObjectImpactDetector.ImpactState.Light:
			healthCurrent -= healthLossOnBreakLight;
			break;
		case PhysGrabObjectImpactDetector.ImpactState.Medium:
			healthCurrent -= healthLossOnBreakMedium;
			break;
		case PhysGrabObjectImpactDetector.ImpactState.Heavy:
			healthCurrent -= healthLossOnBreakHeavy;
			break;
		}
		if (healthCurrent <= 0)
		{
			physGrabObject.impactDetector.DestroyObject();
		}
	}

	private IEnumerator EnableRigidbody()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		PhysGrabObject component = GetComponent<PhysGrabObject>();
		if (!component)
		{
			yield return new WaitForSeconds(0.5f);
			yield return new WaitForFixedUpdate();
		}
		else
		{
			component.spawned = true;
		}
	}
}
