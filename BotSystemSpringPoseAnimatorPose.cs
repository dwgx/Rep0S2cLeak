using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BotSystemSpringPoseAnimatorPose : MonoBehaviour
{
	[Serializable]
	public class BoneOffset
	{
		public Transform bone;

		public Transform linkedMainRigBone;

		public Quaternion targetWorldRotation;

		public Vector3 offsetEuler;

		public Transform idleOffsetTransform;

		public float amount = 1f;

		public bool additive = true;

		[Range(0f, 1f)]
		public float curveOffset;

		[HideInInspector]
		public int[] childIndexPath;

		[HideInInspector]
		public int[] mainRigBoneChildIndexPath;

		[SerializeField]
		[HideInInspector]
		public string indexPath;

		[SerializeField]
		[HideInInspector]
		public string nameOrdinalPath;
	}

	public string poseName = "Untitled";

	public float springSpeedMultiplier = 1f;

	public float springDampingMultiplier = 1f;

	public BotSystemSpringPoseAnimator sourceAnimator;

	public List<AnimationCurve> idleCurves = new List<AnimationCurve> { AnimationCurve.Linear(0f, 0f, 1f, 1f) };

	public float idleSpeed = 1f;

	public bool loop = true;

	public bool previewIdleAnimation;

	[HideInInspector]
	public Quaternion rootRotationOverride = Quaternion.identity;

	public List<BoneOffset> boneOffsets = new List<BoneOffset>();

	private List<string> allBonesInHierarchy = new List<string>();

	public bool previewWireMesh = true;

	public float previewScale = 1f;

	private List<MeshFilter> meshFilters = new List<MeshFilter>();

	private Dictionary<Transform, Quaternion> originalLocals = new Dictionary<Transform, Quaternion>();

	private Dictionary<Transform, Quaternion> previewRotations = new Dictionary<Transform, Quaternion>();

	private bool initDone;

	public void StartIdlePreview()
	{
		previewIdleAnimation = true;
		CacheOriginals();
		CacheMeshes();
	}

	public void StopIdlePreview()
	{
		previewIdleAnimation = false;
		previewRotations.Clear();
	}

	public void RefreshBoneListButton()
	{
		RefreshBoneList();
	}

	private void BakeIdsHere(bool regenerateExisting)
	{
		Transform transform = base.transform;
		AssignRecursivePose(transform, "", transform.name + "[0]", regenerateExisting);
	}

	private void AssignRecursivePose(Transform t, string idxPath, string nameOrdPath, bool regen)
	{
		BoneOffset boneData = GetBoneData(t);
		if (boneData != null && (regen || string.IsNullOrEmpty(boneData.indexPath) || string.IsNullOrEmpty(boneData.nameOrdinalPath)))
		{
			boneData.indexPath = idxPath;
			boneData.nameOrdinalPath = nameOrdPath;
		}
		int childCount = t.childCount;
		Dictionary<string, int> dictionary = new Dictionary<string, int>(childCount);
		for (int i = 0; i < childCount; i++)
		{
			Transform child = t.GetChild(i);
			dictionary.TryGetValue(child.name, out var value);
			dictionary[child.name] = value + 1;
			string idxPath2 = (string.IsNullOrEmpty(idxPath) ? i.ToString() : $"{idxPath}/{i}");
			string nameOrdPath2 = $"{nameOrdPath}/{child.name}[{value}]";
			AssignRecursivePose(child, idxPath2, nameOrdPath2, regen);
		}
	}

	public void BuildPoseMaps(out Dictionary<string, Transform> byIndex, out Dictionary<string, Transform> byNameOrd)
	{
		byIndex = new Dictionary<string, Transform>(1024);
		byNameOrd = new Dictionary<string, Transform>(1024);
		Transform transform = base.transform;
		WalkBuildPoseMaps(transform, "", transform.name + "[0]", byIndex, byNameOrd);
	}

	private void WalkBuildPoseMaps(Transform t, string idxPath, string nameOrdPath, Dictionary<string, Transform> byIndex, Dictionary<string, Transform> byNameOrd)
	{
		BoneOffset boneData = GetBoneData(t);
		if (boneData != null && !string.IsNullOrEmpty(boneData.indexPath))
		{
			byIndex[boneData.indexPath] = t;
			if (!string.IsNullOrEmpty(boneData.nameOrdinalPath))
			{
				byNameOrd[boneData.nameOrdinalPath] = t;
			}
		}
		else
		{
			byIndex[idxPath] = t;
			byNameOrd[nameOrdPath] = t;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>(t.childCount);
		for (int i = 0; i < t.childCount; i++)
		{
			Transform child = t.GetChild(i);
			dictionary.TryGetValue(child.name, out var value);
			dictionary[child.name] = value + 1;
			string idxPath2 = (string.IsNullOrEmpty(idxPath) ? i.ToString() : $"{idxPath}/{i}");
			string nameOrdPath2 = $"{nameOrdPath}/{child.name}[{value}]";
			WalkBuildPoseMaps(child, idxPath2, nameOrdPath2, byIndex, byNameOrd);
		}
	}

	public void GetIdsForPoseBone(Transform t, out string idx, out string nameOrd)
	{
		BoneOffset boneData = GetBoneData(t);
		if (boneData != null)
		{
			idx = boneData.indexPath;
			nameOrd = boneData.nameOrdinalPath;
			return;
		}
		idx = "";
		nameOrd = base.transform.name + "[0]";
		List<Transform> list = new List<Transform>();
		Transform transform = t;
		while ((bool)transform && transform != base.transform)
		{
			list.Add(transform);
			transform = transform.parent;
		}
		list.Reverse();
		string text = "";
		string text2 = base.transform.name + "[0]";
		foreach (Transform item in list)
		{
			int siblingIndex = item.GetSiblingIndex();
			text = (string.IsNullOrEmpty(text) ? siblingIndex.ToString() : $"{text}/{siblingIndex}");
			int num = 0;
			for (int i = 0; i < siblingIndex; i++)
			{
				if (item.parent.GetChild(i).name == item.name)
				{
					num++;
				}
			}
			text2 = $"{text2}/{item.name}[{num}]";
		}
		idx = text;
		nameOrd = text2;
	}

	private void Awake()
	{
		if (!initDone)
		{
			CacheOriginals();
		}
		CacheMeshes();
		RefreshBoneList();
	}

	private bool IsThisObjectSelected()
	{
		return false;
	}

	private bool IsThisObjectOrChildSelected()
	{
		return false;
	}

	private string GetFullPath(Transform t)
	{
		if (t.parent == null)
		{
			return t.name;
		}
		return GetFullPath(t.parent) + "/" + t.name;
	}

	private void CacheOriginals()
	{
		originalLocals.Clear();
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (!originalLocals.ContainsKey(transform))
			{
				originalLocals[transform] = transform.localRotation;
			}
		}
		initDone = true;
	}

	private void CacheMeshes()
	{
		meshFilters.Clear();
		GetComponentsInChildren(includeInactive: true, meshFilters);
	}

	private void RefreshBoneList()
	{
		if (allBonesInHierarchy == null)
		{
			allBonesInHierarchy = new List<string>();
		}
		allBonesInHierarchy.Clear();
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>(includeInactive: true);
		int num = 0;
		int num2 = 0;
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (!(transform == base.transform))
			{
				num2++;
				string pathFromRoot = GetPathFromRoot(transform);
				string text = $" (sib:{transform.GetSiblingIndex()})";
				if (transform.name.Contains("Main"))
				{
					num++;
					allBonesInHierarchy.Add("[M] " + pathFromRoot + text);
				}
				else
				{
					allBonesInHierarchy.Add("    " + pathFromRoot + text);
				}
			}
		}
		allBonesInHierarchy.Sort();
		allBonesInHierarchy.Insert(0, $"=== {num} Main bones, {num2} total ===");
	}

	private string GetPathFromRoot(Transform t)
	{
		if (t == base.transform)
		{
			return "";
		}
		string text = t.name;
		Transform parent = t.parent;
		while (parent != null && parent != base.transform)
		{
			text = parent.name + "/" + text;
			parent = parent.parent;
		}
		return text;
	}

	public BoneOffset GetBoneData(Transform t)
	{
		if (t == null)
		{
			return null;
		}
		for (int i = 0; i < boneOffsets.Count; i++)
		{
			if (!(boneOffsets[i].bone == null) && boneOffsets[i].bone == t)
			{
				return boneOffsets[i];
			}
		}
		return null;
	}

	public BoneOffset GetBoneDataByName(Transform t)
	{
		if (t == null)
		{
			return null;
		}
		for (int i = 0; i < boneOffsets.Count; i++)
		{
			if (!(boneOffsets[i].bone == null) && boneOffsets[i].bone.name == t.name)
			{
				return boneOffsets[i];
			}
		}
		return null;
	}

	public BoneOffset GetBoneDataForMain(Transform mainBone)
	{
		if (!mainBone)
		{
			return null;
		}
		for (int i = 0; i < boneOffsets.Count; i++)
		{
			if (boneOffsets[i].linkedMainRigBone == mainBone)
			{
				return boneOffsets[i];
			}
		}
		for (int j = 0; j < boneOffsets.Count; j++)
		{
			if ((bool)boneOffsets[j].bone && boneOffsets[j].bone.name == mainBone.name)
			{
				return boneOffsets[j];
			}
		}
		return null;
	}

	public float GetCurveEval(int curveIndex, float time)
	{
		return GetCurveEval(curveIndex, time, 0f);
	}

	public float GetCurveEval(int curveIndex, float time, float offset)
	{
		if (idleCurves == null || idleCurves.Count == 0)
		{
			return 0f;
		}
		if (curveIndex < 0 || curveIndex >= idleCurves.Count)
		{
			curveIndex = 0;
		}
		AnimationCurve animationCurve = idleCurves[curveIndex];
		float num = ((idleSpeed <= 0.0001f) ? 0f : (time * idleSpeed));
		num += offset;
		if (loop)
		{
			num = Mathf.Repeat(num, 1f);
		}
		return animationCurve.Evaluate(num);
	}

	public int GetCurveIndexForBone(int i)
	{
		if (idleCurves == null || idleCurves.Count == 0)
		{
			return 0;
		}
		if (boneOffsets == null || boneOffsets.Count == 0)
		{
			return 0;
		}
		return Mathf.Abs(i) % idleCurves.Count;
	}

	public int GetRandomCurveIndex(System.Random rng)
	{
		if (idleCurves == null || idleCurves.Count == 0)
		{
			return 0;
		}
		return rng.Next(0, idleCurves.Count);
	}

	public Dictionary<Transform, Quaternion> GetOriginalLocals()
	{
		return originalLocals;
	}

	public Dictionary<Transform, Quaternion> GetPreviewRotations()
	{
		return previewRotations;
	}

	public Vector3 GetPreviewPosition(Transform t)
	{
		if (!previewIdleAnimation || previewRotations.Count == 0)
		{
			return t.position;
		}
		return GetPreviewMatrix(t).MultiplyPoint3x4(Vector3.zero);
	}

	private void RestoreOriginalPose()
	{
		foreach (KeyValuePair<Transform, Quaternion> originalLocal in originalLocals)
		{
			Transform key = originalLocal.Key;
			if ((bool)key)
			{
				BoneOffset boneData = GetBoneData(key);
				if (boneData == null)
				{
					key.localRotation = originalLocal.Value;
					continue;
				}
				Quaternion value = originalLocal.Value;
				Quaternion quaternion = Quaternion.Euler(boneData.offsetEuler * boneData.amount);
				key.localRotation = (boneData.additive ? (value * quaternion) : quaternion);
			}
		}
	}

	public Vector3 GetIdleOffsetPosition(Transform t, Dictionary<Transform, Quaternion> idleRotations)
	{
		return Vector3.zero;
	}

	public Matrix4x4 GetPreviewMatrix(Transform t)
	{
		if (!previewIdleAnimation || previewRotations.Count == 0)
		{
			return t.localToWorldMatrix;
		}
		bool flag = false;
		Transform transform = t;
		while (transform != null && transform != base.transform)
		{
			if (previewRotations.ContainsKey(transform))
			{
				flag = true;
				break;
			}
			transform = transform.parent;
		}
		if (!flag)
		{
			return t.localToWorldMatrix;
		}
		Matrix4x4 identity = Matrix4x4.identity;
		List<Transform> list = new List<Transform>();
		transform = t;
		while (transform != null && transform != base.transform.parent)
		{
			list.Add(transform);
			transform = transform.parent;
		}
		list.Reverse();
		identity = ((base.transform.parent != null) ? base.transform.parent.localToWorldMatrix : Matrix4x4.identity);
		foreach (Transform item in list)
		{
			Matrix4x4 matrix4x;
			if (item == base.transform)
			{
				Quaternion q = ((rootRotationOverride != Quaternion.identity) ? rootRotationOverride : item.localRotation);
				matrix4x = Matrix4x4.TRS(item.localPosition, q, item.localScale);
			}
			else if (previewRotations.ContainsKey(item))
			{
				Quaternion q2 = previewRotations[item];
				matrix4x = Matrix4x4.TRS(item.localPosition, q2, item.localScale);
			}
			else
			{
				matrix4x = Matrix4x4.TRS(item.localPosition, item.localRotation, item.localScale);
			}
			identity *= matrix4x;
		}
		return identity;
	}

	public Matrix4x4 GetIdleOffsetMatrix(Transform t, Dictionary<Transform, Quaternion> idleRotations)
	{
		return Matrix4x4.identity;
	}

	public Dictionary<Transform, Quaternion> GetStaticIdleOffsetRotations()
	{
		Dictionary<Transform, Quaternion> dictionary = new Dictionary<Transform, Quaternion>();
		foreach (KeyValuePair<Transform, Quaternion> originalLocal in originalLocals)
		{
			Transform key = originalLocal.Key;
			if (!key)
			{
				continue;
			}
			BoneOffset boneData = GetBoneData(key);
			if (boneData == null || !boneData.idleOffsetTransform)
			{
				dictionary[key] = originalLocal.Value;
				continue;
			}
			Quaternion value = originalLocal.Value;
			if ((bool)boneData.idleOffsetTransform)
			{
				Quaternion localRotation = boneData.idleOffsetTransform.localRotation;
				dictionary[key] = (boneData.additive ? (value * localRotation) : (value * localRotation));
			}
			else
			{
				dictionary[key] = originalLocal.Value;
			}
		}
		return dictionary;
	}
}
