using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
	public PhysicMaterial physicMaterialStickyExtreme;

	public PhysicMaterial physicMaterialSlipperyExtreme;

	public PhysicMaterial physicMaterialSlippery;

	public PhysicMaterial physicMaterialDefault;

	public PhysicMaterial physicMaterialPlayerMove;

	public PhysicMaterial physicMaterialPlayerIdle;

	public PhysicMaterial physicMaterialPhysGrabObject;

	public PhysicMaterial physicMaterialSlipperyPlus;

	public AnimationCurve animationCurveImpact;

	public AnimationCurve animationCurveWooshAway;

	public AnimationCurve animationCurveWooshIn;

	public AnimationCurve animationCurveInOut;

	public AnimationCurve animationCurveClickInOut;

	public AnimationCurve animationCurveEaseInOut;

	public Sound soundEquip;

	public Sound soundUnequip;

	public Sound soundDeviceTurnOn;

	public Sound soundDeviceTurnOff;

	public Sound batteryChargeSound;

	public Sound batteryDrainSound;

	public Sound batteryLowBeep;

	public Sound batteryLowWarning;

	public List<Color> playerColors;

	public GameObject enemyValuableSmall;

	public GameObject enemyValuableMedium;

	public GameObject enemyValuableBig;

	public GameObject surplusValuableSmall;

	public GameObject surplusValuableMedium;

	public GameObject surplusValuableBig;

	[Space]
	public Mesh valuableMeshTiny;

	public Mesh valuableMeshSmall;

	public Mesh valuableMeshMedium;

	public Mesh valuableMeshBig;

	public Mesh valuableMeshWide;

	public Mesh valuableMeshTall;

	public Mesh valuableMeshVeryTall;

	public GameObject prefabTeleportEffect;

	public GameObject debugEnemyInvestigate;

	public GameObject debugLevelPointError;

	public GameObject debugCube;

	public GameObject debugSphere;

	public GameObject deathPitSaveEffect;

	public GameObject debugNavMeshError;

	public GameObject physImpactEffect;

	public Sound physImpactEffectSound;

	internal Color colorYellow = new Color(1f, 0.55f, 0f);

	internal Camera mainCamera;

	public static AssetManager instance;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
		mainCamera = Camera.main;
	}

	public void PhysImpactEffect(Vector3 _position)
	{
		physImpactEffectSound.Play(_position);
		Object.Instantiate(physImpactEffect, _position, Quaternion.identity);
	}
}
