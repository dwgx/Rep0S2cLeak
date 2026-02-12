using System.Collections;
using UnityEngine;

public class ValuableVolume : MonoBehaviour
{
	public enum Type
	{
		Tiny,
		Small,
		Medium,
		Big,
		Wide,
		Tall,
		VeryTall
	}

	public Type VolumeType;

	[HideInInspector]
	public Module Module;

	private Mesh MeshTiny;

	private Mesh MeshSmall;

	private Mesh MeshMedium;

	private Mesh MeshBig;

	private Mesh MeshWide;

	private Mesh MeshTall;

	private Mesh MeshVeryTall;

	private void Start()
	{
		Module = GetComponentInParent<Module>();
	}

	public void Setup()
	{
		ValuablePropSwitch componentInParent = GetComponentInParent<ValuablePropSwitch>();
		if ((bool)componentInParent && base.transform.parent != componentInParent.ValuableParent.transform)
		{
			Debug.LogError("Valuable Volume: Child of ValuablePropSwitch but not valuable parent...", base.gameObject);
		}
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		bool flag = true;
		if (Debug.isDebugBuild)
		{
			StartCoroutine(SafetyCheck());
			flag = false;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, 2f);
		foreach (Collider collider in array)
		{
			if (collider.gameObject.CompareTag("Phys Grab Object"))
			{
				ValuableObject componentInParent2 = collider.transform.GetComponentInParent<ValuableObject>();
				if ((bool)componentInParent2 && componentInParent2.volumeType == VolumeType && Vector3.Distance(componentInParent2.transform.position, base.transform.position) < 0.1f)
				{
					componentInParent2.transform.parent = base.transform.parent;
					break;
				}
			}
		}
		if (flag)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator SafetyCheck()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return null;
		}
		Mesh mesh = null;
		switch (VolumeType)
		{
		case Type.Tiny:
			mesh = AssetManager.instance.valuableMeshTiny;
			break;
		case Type.Small:
			mesh = AssetManager.instance.valuableMeshSmall;
			break;
		case Type.Medium:
			mesh = AssetManager.instance.valuableMeshMedium;
			break;
		case Type.Big:
			mesh = AssetManager.instance.valuableMeshBig;
			break;
		case Type.Wide:
			mesh = AssetManager.instance.valuableMeshWide;
			break;
		case Type.Tall:
			mesh = AssetManager.instance.valuableMeshTall;
			break;
		case Type.VeryTall:
			mesh = AssetManager.instance.valuableMeshVeryTall;
			break;
		}
		Vector3 size = mesh.bounds.size;
		Collider[] array = Physics.OverlapBox(base.transform.position + base.transform.forward * size.z / 2f + base.transform.up * size.y / 2f + Vector3.up * 0.01f, size / 2f, base.transform.rotation, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
		if (array.Length == 0)
		{
			yield break;
		}
		string text = "not found";
		Module componentInParent = GetComponentInParent<Module>();
		if ((bool)componentInParent)
		{
			text = componentInParent.gameObject.name;
		}
		Debug.LogError("Valuable Volume: Overlapping colliders:");
		Debug.LogError("     Volume Name: " + base.gameObject.name, base.gameObject);
		Debug.LogError("     Volume Module: " + text, componentInParent?.gameObject);
		if ((bool)componentInParent)
		{
			Debug.LogError("     Volume Local Position: " + componentInParent.transform.InverseTransformPoint(base.transform.position).ToString());
		}
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			text = "not found";
			componentInParent = collider.gameObject.GetComponentInParent<Module>();
			if ((bool)componentInParent)
			{
				text = componentInParent.gameObject.name;
			}
			Debug.LogError("          Collider: " + collider.gameObject.name, collider.gameObject);
			Debug.LogError("          Collider Module: " + text, componentInParent?.gameObject);
			if ((bool)componentInParent)
			{
				Debug.LogError("          Collider Local Position: " + componentInParent.transform.InverseTransformPoint(collider.transform.position).ToString());
			}
			Debug.LogError(" ");
		}
	}
}
