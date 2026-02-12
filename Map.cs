using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
	[Serializable]
	public class RoomVolumeOutlineCustom
	{
		public Mesh mesh;

		public Mesh meshOutline;
	}

	public static Map Instance;

	public bool Active;

	public bool ActivePrevious;

	public GameObject ActiveParent;

	public int PlayerLayer;

	[Space]
	public GameObject LayerPrefab;

	public GameObject ModulePrefab;

	public Transform OverLayerParent;

	[Space]
	public List<MapLayer> Layers = new List<MapLayer>();

	public List<MapModule> MapModules = new List<MapModule>();

	[Space]
	public GameObject EnemyObject;

	public GameObject CustomObject;

	public GameObject ValuableObject;

	[Space]
	public GameObject FloorObject1x1;

	public GameObject FloorObject1x1Diagonal;

	public GameObject FloorObject1x1Curve;

	public GameObject FloorObject1x1CurveInverted;

	[Space]
	public GameObject FloorObject1x05;

	public GameObject FloorObject1x05Diagonal;

	public GameObject FloorObject1x05Curve;

	public GameObject FloorObject1x05CurveInverted;

	[Space]
	public GameObject FloorObject1x025;

	public GameObject FloorObject1x025Diagonal;

	[Space]
	public GameObject RoomVolume;

	public GameObject RoomVolumeOutline;

	[Space]
	public GameObject FloorTruck;

	public GameObject WallTruck;

	[Space]
	public GameObject FloorUsed;

	public GameObject WallUsed;

	[Space]
	public GameObject FloorInactive;

	public GameObject WallInactive;

	[Space]
	public GameObject Wall1x1Object;

	public GameObject Wall1x1DiagonalObject;

	public GameObject Wall1x1CurveObject;

	[Space]
	public GameObject Wall1x05Object;

	public GameObject Wall1x05DiagonalObject;

	public GameObject Wall1x05CurveObject;

	[Space]
	public GameObject Wall1x025Object;

	public GameObject Wall1x025DiagonalObject;

	[Space]
	public GameObject Door1x1Object;

	public GameObject Door1x05Object;

	public GameObject Door1x1DiagonalObject;

	public GameObject Door1x05DiagonalObject;

	public GameObject Door1x2Object;

	public GameObject Door1x1WizardObject;

	public GameObject Door1x1ArcticObject;

	public GameObject Door1x1MuseumObject;

	[Space]
	public GameObject DoorBlockedObject;

	public GameObject DoorBlockedWizardObject;

	public GameObject DoorBlockedArcticObject;

	public GameObject DoorDiagonalObject;

	public GameObject StairsObject;

	[Space]
	public float Scale = 0.1f;

	private float LayerHeight = 4f;

	[Space]
	public Transform playerTransformSource;

	public Transform playerTransformTarget;

	[Space]
	public Transform CompletedTransform;

	internal bool debugActive;

	[Space]
	public List<RoomVolumeOutlineCustom> RoomVolumeOutlineCustoms;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		playerTransformSource = PlayerController.instance.transform;
		ActiveSet(active: false);
	}

	private void Update()
	{
		if (Active != ActivePrevious)
		{
			if (!Active)
			{
				foreach (MapLayer layer in Layers)
				{
					layer.transform.position = layer.positionStart;
				}
			}
			ActivePrevious = Active;
		}
		if (!Active)
		{
			return;
		}
		foreach (MapLayer layer2 in Layers)
		{
			if (layer2.layer == PlayerLayer)
			{
				layer2.transform.localPosition = new Vector3(layer2.transform.localPosition.x, 0f, layer2.transform.localPosition.z);
			}
			else if (layer2.layer == PlayerLayer - 1)
			{
				layer2.transform.localPosition = new Vector3(layer2.transform.localPosition.x, GetLayerPosition(2).y, layer2.transform.localPosition.z);
			}
			else if (layer2.layer == PlayerLayer + 1)
			{
				layer2.transform.localPosition = new Vector3(layer2.transform.localPosition.x, GetLayerPosition(3).y, layer2.transform.localPosition.z);
			}
			else
			{
				layer2.transform.localPosition = new Vector3(layer2.transform.localPosition.x, -5f, layer2.transform.localPosition.z);
			}
		}
	}

	public void ActiveSet(bool active)
	{
		Active = active;
		if (ActiveParent != null)
		{
			ActiveParent.SetActive(active);
		}
	}

	public void EnemyPositionSet(Transform transformTarget, Transform transformSource)
	{
	}

	public void AddEnemy(Enemy enemy)
	{
	}

	public void CustomPositionSet(Transform transformTarget, Transform transformSource)
	{
		transformTarget.position = transformSource.transform.position * Scale + OverLayerParent.position;
		transformTarget.localPosition = new Vector3(transformTarget.localPosition.x, 0f, transformTarget.localPosition.z);
		transformTarget.localRotation = Quaternion.Euler(0f, transformSource.rotation.eulerAngles.y, 0f);
	}

	public void AddCustom(MapCustom mapCustom, Sprite sprite, Color color)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(CustomObject, OverLayerParent.transform);
		gameObject.gameObject.name = mapCustom.gameObject.name;
		CustomPositionSet(gameObject.transform, mapCustom.transform);
		MapCustomEntity component = gameObject.GetComponent<MapCustomEntity>();
		component.Parent = mapCustom.transform;
		component.mapCustom = mapCustom;
		component.spriteRenderer.sprite = sprite;
		component.spriteRenderer.color = color;
		component.StartCoroutine(component.Logic());
		mapCustom.mapCustomEntity = component;
	}

	public void AddFloor(DirtFinderMapFloor floor)
	{
		GameObject gameObject = null;
		MapLayer layerParent = GetLayerParent(floor.transform.position.y);
		if (floor.Type == DirtFinderMapFloor.FloorType.Floor_1x1)
		{
			gameObject = UnityEngine.Object.Instantiate(FloorObject1x1, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Floor_1x1_Diagonal)
		{
			gameObject = UnityEngine.Object.Instantiate(FloorObject1x1Diagonal, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Floor_1x05)
		{
			gameObject = UnityEngine.Object.Instantiate(FloorObject1x05, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Floor_1x05_Diagonal)
		{
			gameObject = UnityEngine.Object.Instantiate(FloorObject1x05Diagonal, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Floor_1x05_Curve)
		{
			gameObject = UnityEngine.Object.Instantiate(FloorObject1x05Curve, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Floor_1x05_Curve_Inverted)
		{
			gameObject = UnityEngine.Object.Instantiate(FloorObject1x05CurveInverted, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Floor_1x025)
		{
			gameObject = UnityEngine.Object.Instantiate(FloorObject1x025, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Floor_1x025_Diagonal)
		{
			gameObject = UnityEngine.Object.Instantiate(FloorObject1x025Diagonal, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Truck_Floor)
		{
			gameObject = UnityEngine.Object.Instantiate(FloorTruck, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Truck_Wall)
		{
			gameObject = UnityEngine.Object.Instantiate(WallTruck, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Used_Floor)
		{
			gameObject = UnityEngine.Object.Instantiate(FloorUsed, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Used_Wall)
		{
			gameObject = UnityEngine.Object.Instantiate(WallUsed, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Inactive_Floor)
		{
			gameObject = UnityEngine.Object.Instantiate(FloorInactive, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Inactive_Wall)
		{
			gameObject = UnityEngine.Object.Instantiate(WallInactive, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Floor_1x1_Curve)
		{
			gameObject = UnityEngine.Object.Instantiate(FloorObject1x1Curve, layerParent.transform);
		}
		else if (floor.Type == DirtFinderMapFloor.FloorType.Floor_1x1_Curve_Inverted)
		{
			gameObject = UnityEngine.Object.Instantiate(FloorObject1x1CurveInverted, layerParent.transform);
		}
		gameObject.gameObject.name = floor.gameObject.name;
		gameObject.transform.localScale = floor.transform.localScale;
		gameObject.transform.position = floor.transform.position * Scale + layerParent.transform.position + GetLayerPosition(layerParent.layer);
		gameObject.transform.rotation = floor.transform.rotation;
		MapObjectSetup(floor.gameObject, gameObject);
	}

	public void AddWall(DirtFinderMapWall wall)
	{
		GameObject gameObject = null;
		MapLayer layerParent = GetLayerParent(wall.transform.position.y);
		gameObject = ((wall.Type == DirtFinderMapWall.WallType.Door_1x1) ? UnityEngine.Object.Instantiate(Door1x1Object, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Door_1x2) ? UnityEngine.Object.Instantiate(Door1x2Object, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Door_Blocked) ? UnityEngine.Object.Instantiate(DoorBlockedObject, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Door_Blocked_Wizard) ? UnityEngine.Object.Instantiate(DoorBlockedWizardObject, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Door_Blocked_Arctic) ? UnityEngine.Object.Instantiate(DoorBlockedArcticObject, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Stairs) ? UnityEngine.Object.Instantiate(StairsObject, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Door_1x05) ? UnityEngine.Object.Instantiate(Door1x05Object, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Door_1x1_Diagonal) ? UnityEngine.Object.Instantiate(Door1x1DiagonalObject, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Door_1x05_Diagonal) ? UnityEngine.Object.Instantiate(Door1x05DiagonalObject, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Wall_1x05) ? UnityEngine.Object.Instantiate(Wall1x05Object, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Wall_1x025) ? UnityEngine.Object.Instantiate(Wall1x025Object, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Wall_1x05_Diagonal) ? UnityEngine.Object.Instantiate(Wall1x05DiagonalObject, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Wall_1x025_Diagonal) ? UnityEngine.Object.Instantiate(Wall1x025DiagonalObject, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Wall_1x1_Diagonal) ? UnityEngine.Object.Instantiate(Wall1x1DiagonalObject, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Door_1x1_Wizard) ? UnityEngine.Object.Instantiate(Door1x1WizardObject, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Door_1x1_Arctic) ? UnityEngine.Object.Instantiate(Door1x1ArcticObject, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Wall_1x1_Curve) ? UnityEngine.Object.Instantiate(Wall1x1CurveObject, layerParent.transform) : ((wall.Type == DirtFinderMapWall.WallType.Wall_1x05_Curve) ? UnityEngine.Object.Instantiate(Wall1x05CurveObject, layerParent.transform) : ((wall.Type != DirtFinderMapWall.WallType.Door_1x1_Museum) ? UnityEngine.Object.Instantiate(Wall1x1Object, layerParent.transform) : UnityEngine.Object.Instantiate(Door1x1MuseumObject, layerParent.transform))))))))))))))))))));
		gameObject.gameObject.name = wall.gameObject.name;
		gameObject.transform.position = wall.transform.position * Scale + layerParent.transform.position + GetLayerPosition(layerParent.layer);
		gameObject.transform.rotation = wall.transform.rotation;
		gameObject.transform.localScale = wall.transform.localScale;
		MapObjectSetup(wall.gameObject, gameObject);
	}

	public MapModule AddRoomVolume(GameObject _parent, Vector3 _position, Quaternion _rotation, Vector3 _scale, Module _module, Mesh _mesh = null)
	{
		MapLayer component = OverLayerParent.GetComponent<MapLayer>();
		GameObject gameObject = UnityEngine.Object.Instantiate(RoomVolume, component.transform);
		gameObject.gameObject.name = "Room Volume";
		gameObject.transform.position = _position * Scale + component.transform.position + GetLayerPosition(component.layer);
		gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, 0f, gameObject.transform.localPosition.z);
		gameObject.transform.rotation = _rotation;
		gameObject.transform.localScale = _scale;
		gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, 0.1f, gameObject.transform.localScale.z);
		if ((bool)_mesh)
		{
			gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, 0.025f, gameObject.transform.localScale.z);
			gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 0.01f, gameObject.transform.position.z);
			gameObject.GetComponentInChildren<MeshFilter>().mesh = _mesh;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(RoomVolumeOutline, component.transform);
		gameObject2.transform.position = gameObject.transform.position;
		gameObject2.transform.rotation = gameObject.transform.rotation;
		if (!_mesh)
		{
			gameObject2.transform.localScale = new Vector3(gameObject.transform.localScale.x + 0.25f, gameObject.transform.localScale.y, gameObject.transform.localScale.z + 0.25f);
		}
		else
		{
			gameObject2.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
		}
		if ((bool)_mesh)
		{
			MeshFilter componentInChildren = gameObject2.GetComponentInChildren<MeshFilter>();
			foreach (RoomVolumeOutlineCustom roomVolumeOutlineCustom in RoomVolumeOutlineCustoms)
			{
				if (roomVolumeOutlineCustom.mesh == _mesh)
				{
					componentInChildren.mesh = roomVolumeOutlineCustom.meshOutline;
					break;
				}
			}
		}
		foreach (MapModule mapModule in MapModules)
		{
			if (mapModule.module == _module)
			{
				gameObject.transform.SetParent(mapModule.transform);
				gameObject2.transform.SetParent(mapModule.transform);
				return mapModule;
			}
		}
		GameObject gameObject3 = UnityEngine.Object.Instantiate(ModulePrefab, component.transform);
		MapModule component2 = gameObject3.GetComponent<MapModule>();
		component2.module = _module;
		gameObject3.gameObject.name = _module.gameObject.name;
		gameObject3.transform.position = _module.transform.position * Scale + component.transform.position + GetLayerPosition(component.layer);
		MapModules.Add(component2);
		gameObject.transform.SetParent(gameObject3.transform);
		gameObject2.transform.SetParent(gameObject3.transform);
		return component2;
	}

	public void AddValuable(ValuableObject _valuable)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(ValuableObject, OverLayerParent.transform);
		gameObject.gameObject.name = _valuable.gameObject.name;
		gameObject.transform.position = _valuable.transform.position * Scale + OverLayerParent.position;
		gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, 0f, gameObject.transform.localPosition.z);
		MapValuable component = gameObject.GetComponent<MapValuable>();
		component.target = _valuable;
		if (_valuable.volumeType <= ValuableVolume.Type.Medium)
		{
			component.spriteRenderer.sprite = component.spriteSmall;
		}
		else
		{
			component.spriteRenderer.sprite = component.spriteBig;
		}
	}

	public GameObject AddDoor(DirtFinderMapDoor door, GameObject doorPrefab)
	{
		MapLayer layerParent = GetLayerParent(door.transform.position.y);
		GameObject gameObject = UnityEngine.Object.Instantiate(doorPrefab, layerParent.transform);
		gameObject.gameObject.name = door.gameObject.name;
		door.Target = gameObject.transform;
		DirtFinderMapDoorTarget component = gameObject.GetComponent<DirtFinderMapDoorTarget>();
		component.Target = door.transform;
		component.Layer = layerParent;
		DoorUpdate(component.HingeTransform, door.transform, layerParent);
		return gameObject;
	}

	public void DoorUpdate(Transform transformTarget, Transform transformSource, MapLayer _layer)
	{
		transformTarget.position = transformSource.transform.position * Scale + _layer.transform.position + GetLayerPosition(_layer.layer);
		transformTarget.rotation = transformSource.rotation;
	}

	public MapLayer GetLayerParent(float _positionY)
	{
		int num = Mathf.FloorToInt((_positionY + 0.1f) / LayerHeight);
		foreach (MapLayer layer in Layers)
		{
			if (layer.layer == num)
			{
				return layer;
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(LayerPrefab, base.transform);
		MapLayer component = gameObject.GetComponent<MapLayer>();
		component.layer = num;
		Layers.Add(component);
		gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, LayerHeight * Scale * (float)num, gameObject.transform.localPosition.z);
		gameObject.name = "Layer " + num;
		Layers = Layers.OrderBy((MapLayer x) => x.layer).ToList();
		Layers.Reverse();
		OverLayerParent.SetSiblingIndex(0);
		int num2 = 1;
		foreach (MapLayer layer2 in Layers)
		{
			layer2.transform.SetSiblingIndex(num2);
			num2++;
		}
		return component;
	}

	public Vector3 GetLayerPosition(int _layerIndex)
	{
		return new Vector3(0f, (0f - LayerHeight * Scale) * (float)_layerIndex, 0f);
	}

	private MapObject MapObjectSetup(GameObject _parent, GameObject _object)
	{
		MapObject component = _object.GetComponent<MapObject>();
		if (!component)
		{
			Debug.LogError("Map Object missing component!", _object);
		}
		else
		{
			component.parent = _parent.transform;
			DirtFinderMapFloor component2 = _parent.GetComponent<DirtFinderMapFloor>();
			if ((bool)component2)
			{
				component2.MapObject = component;
			}
		}
		return component;
	}
}
