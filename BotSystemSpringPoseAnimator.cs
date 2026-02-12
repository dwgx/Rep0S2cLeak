using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class BotSystemSpringPoseAnimator : MonoBehaviour
{
	[Serializable]
	public class BoneSpring
	{
		public string name;

		public Transform transform;

		public Transform mesh;

		[Tooltip("The matching bone in the current pose object")]
		public Transform targetTransform;

		public bool overrideDefaultSpringValues;

		public SpringQuaternion spring;

		[HideInInspector]
		public Quaternion originalLocal;

		[HideInInspector]
		public Quaternion targetLocal;

		[HideInInspector]
		public int curveIndex;

		[HideInInspector]
		public Quaternion idleLocalDelta = Quaternion.identity;

		[SerializeField]
		[HideInInspector]
		public string indexPath;

		[SerializeField]
		[HideInInspector]
		public string nameOrdinalPath;
	}

	[Serializable]
	public class Limb
	{
		public string name;

		public Transform rootTransform;

		public List<BoneSpring> bones = new List<BoneSpring>();

		[Tooltip("Absolute world distance at which the limb becomes fully straight")]
		public float straightenDistance = 0.15f;

		[Tooltip("Absolute world distance at which the limb returns to fully posed (must be less than straightenDistance)")]
		public float fullyPosedDistance = 0.05f;

		public BoneSpring GetRoot()
		{
			if (bones.Count <= 0)
			{
				return null;
			}
			return bones[0];
		}

		public BoneSpring GetTip()
		{
			if (bones.Count <= 0)
			{
				return null;
			}
			return bones[bones.Count - 1];
		}
	}

	[Serializable]
	public class LimbChain
	{
		public Transform root;

		public Transform[] joints;

		public Transform[] meshes;

		public Transform grabPoint;

		[HideInInspector]
		public float[] lenBind;

		[HideInInspector]
		public Vector3[] upRoot;

		[HideInInspector]
		public float[] bindMeshScaleY;

		[HideInInspector]
		public float[] bindMeshScaleZ;

		[HideInInspector]
		public Vector3 bindTipOffset;

		[HideInInspector]
		public bool isInitialized;

		[HideInInspector]
		public Vector3[] lastUp;

		[HideInInspector]
		public Vector3 lastUpRoot;

		[HideInInspector]
		public Vector3 lastFwdRoot;

		[HideInInspector]
		public Quaternion restLimbRootLocalRotation;

		[HideInInspector]
		public Vector3[] bindFwdLocal;

		[HideInInspector]
		public Vector3[] bindUpLocal;

		[HideInInspector]
		public Quaternion[] bindAxisFix;
	}

	public enum CurvePickMode
	{
		Sequential,
		Randoms
	}

	[Tooltip("This is optional, will make the limb alignment follow the Vector3.up of the actor instead of the world.")]
	public Transform mainActorTransform;

	[Space(15f)]
	public Transform modelRoot;

	public List<BoneSpring> bones = new List<BoneSpring>();

	public List<Limb> limbs = new List<Limb>();

	public CurvePickMode curvePickMode;

	public int randomSeed = 1234;

	public float defaultSpringSpeed = 15f;

	public float defaultSpringDamping = 0.5f;

	public BotSystemSpringPoseAnimatorPose mainPose;

	public BotSystemSpringPoseAnimatorPose currentPose;

	public float currentPoseIdleTime;

	public List<BotSystemSpringPoseAnimatorPose> availablePoses = new List<BotSystemSpringPoseAnimatorPose>();

	[Header("Impulse Settings")]
	public AnimationCurve defaultImpulseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public bool autoReturnToMain;

	public float autoReturnTime = 0.5f;

	[Range(0f, 2f)]
	public float overshootCap = 0.75f;

	[Header("Culling")]
	private bool isWithinCullingDistance = true;

	private float cullingDistance = 20f;

	private float cullingOverrideTimer;

	[Header("Limb Stretch Settings")]
	public bool enableLimbStretch = true;

	private float maxStretch = 50f;

	public bool stretchAlongY;

	public bool lockRollOnStretch = true;

	[Tooltip("Scale compensation for rigs that are smaller or larger than 1. Use this if your rig is scaled and limbs aren't reaching correctly.")]
	[Range(0.1f, 10f)]
	public float rigScale = 1f;

	public bool showDebugCrosses;

	public float debugCrossSize = 0.1f;

	private BotSystemSpringPoseAnimatorPose nextPose;

	private BotSystemSpringPoseAnimatorPose previousPose;

	private float impulseTimer;

	private float impulseTime;

	private bool hasImpulse;

	private AnimationCurve impulseTransitionCurve;

	private Dictionary<Transform, int> perBoneCurveIndex = new Dictionary<Transform, int>();

	private Dictionary<Transform, int> previousPoseCurveIndex = new Dictionary<Transform, int>();

	private float poseHoldOverrideTimer;

	private bool isPoseActive;

	private float tempSpringOverrideTimer;

	private Dictionary<BoneSpring, (float originalSpeed, float originalDamping)> tempSpringOriginalStates = new Dictionary<BoneSpring, (float, float)>();

	private HashSet<BoneSpring> bonesWithTempOverride = new HashSet<BoneSpring>();

	private Dictionary<string, Transform> _poseBonesByPath = new Dictionary<string, Transform>();

	private BotSystemSpringPoseAnimatorPose _lastLinkedPose;

	private Dictionary<Limb, LimbChain> limbChains = new Dictionary<Limb, LimbChain>();

	private Dictionary<Limb, Vector3> limbStretchTargets = new Dictionary<Limb, Vector3>();

	private Dictionary<Transform, BoneSpring> boneLookup = new Dictionary<Transform, BoneSpring>();

	private const float EPS = 0.001f;

	private const float EPS_SQR = 1.0000001E-06f;

	private const float PARALLEL = 0.9995f;

	private Transform RootSentinel
	{
		get
		{
			if (!modelRoot)
			{
				return base.transform;
			}
			return modelRoot;
		}
	}

	private void Awake()
	{
		DiscoverAndEnsurePoses();
		InitBones();
		BakeIdsHere(modelRoot ? modelRoot : base.transform, regenerateExisting: true);
		BuildLimbChains();
		if (currentPose == null && mainPose != null)
		{
			currentPose = mainPose;
		}
		if (!Application.isEditor)
		{
			showDebugCrosses = false;
		}
	}

	private void Start()
	{
		if (currentPose != null)
		{
			_lastLinkedPose = null;
			LinkBonesToPose(currentPose);
			ReassignCurvesForPose(currentPose);
			currentPoseIdleTime = 0f;
			ApplyPoseInstant(currentPose, 1f, 0f, freezeIdle: false);
			isPoseActive = true;
		}
	}

	private void Update()
	{
		currentPoseIdleTime += Time.deltaTime;
		if (SemiFunc.FPSImpulse15())
		{
			if ((bool)PlayerController.instance)
			{
				float num = Vector3.Distance(base.transform.position, SemiFunc.PlayerGetObservedPosition());
				isWithinCullingDistance = num <= cullingDistance;
			}
			else
			{
				isWithinCullingDistance = true;
			}
		}
		if (poseHoldOverrideTimer > 0f)
		{
			float num2 = poseHoldOverrideTimer;
			poseHoldOverrideTimer -= Time.deltaTime;
			if (num2 > 0f && poseHoldOverrideTimer <= 0f)
			{
				UpdateIdle();
				if (autoReturnToMain && mainPose != null && currentPose != mainPose)
				{
					ImpulseSetPose(mainPose, autoReturnTime);
				}
			}
		}
		if (!(tempSpringOverrideTimer > 0f))
		{
			return;
		}
		float num3 = tempSpringOverrideTimer;
		tempSpringOverrideTimer -= Time.deltaTime;
		if (!(num3 > 0f) || !(tempSpringOverrideTimer <= 0f))
		{
			return;
		}
		foreach (KeyValuePair<BoneSpring, (float, float)> tempSpringOriginalState in tempSpringOriginalStates)
		{
			BoneSpring key = tempSpringOriginalState.Key;
			(float, float) value = tempSpringOriginalState.Value;
			if (key?.spring != null)
			{
				key.spring.speed = value.Item1;
				key.spring.damping = value.Item2;
			}
		}
		tempSpringOriginalStates.Clear();
		bonesWithTempOverride.Clear();
		tempSpringOverrideTimer = 0f;
	}

	private void LateUpdate()
	{
		if (cullingOverrideTimer > 0f)
		{
			cullingOverrideTimer -= Time.deltaTime;
		}
		else if (!isWithinCullingDistance)
		{
			return;
		}
		if (!modelRoot)
		{
			modelRoot = base.transform;
		}
		if (!currentPose)
		{
			currentPose = mainPose;
		}
		if (bones.Count == 0)
		{
			InitBones();
		}
		UpdateIdle();
		if (!enableLimbStretch)
		{
			UpdateSprings();
			return;
		}
		Dictionary<Limb, Vector3> dictionary = new Dictionary<Limb, Vector3>(limbStretchTargets);
		limbStretchTargets.Clear();
		HashSet<Transform> hashSet = new HashSet<Transform>();
		HashSet<Transform> hashSet2 = new HashSet<Transform>();
		foreach (KeyValuePair<Limb, Vector3> item in dictionary)
		{
			Limb key = item.Key;
			foreach (BoneSpring bone in key.bones)
			{
				if ((bool)bone?.transform)
				{
					hashSet.Add(bone.transform);
				}
			}
			BoneSpring tip = key.GetTip();
			if ((bool)tip?.transform)
			{
				hashSet2.Add(tip.transform);
			}
		}
		float num = (currentPose ? currentPose.springSpeedMultiplier : 1f);
		float num2 = (currentPose ? currentPose.springDampingMultiplier : 1f);
		float speed = Mathf.Max(1f, defaultSpringSpeed * num);
		float damping = Mathf.Max(0f, defaultSpringDamping * num2);
		foreach (BoneSpring bone2 in bones)
		{
			if ((bool)bone2.transform && !hashSet.Contains(bone2.transform))
			{
				if (!bone2.overrideDefaultSpringValues && !bonesWithTempOverride.Contains(bone2))
				{
					bone2.spring.speed = speed;
					bone2.spring.damping = damping;
				}
				Quaternion targetRotation = (bone2.transform.parent ? (bone2.transform.parent.rotation * bone2.targetLocal) : bone2.targetLocal);
				bone2.transform.rotation = SemiFunc.SpringQuaternionGet(bone2.spring, targetRotation);
			}
		}
		List<Limb> limbSolveOrder = GetLimbSolveOrder();
		for (int i = 0; i < limbSolveOrder.Count; i++)
		{
			Limb limb = limbSolveOrder[i];
			if (dictionary.ContainsKey(limb))
			{
				limbStretchTargets[limb] = dictionary[limb];
				SolveLimbIdleAndStretch(limb);
			}
			else
			{
				RestoreLimbIdlePose(limb);
			}
		}
	}

	private void DiscoverAndEnsurePoses()
	{
		if (availablePoses == null)
		{
			availablePoses = new List<BotSystemSpringPoseAnimatorPose>();
		}
		availablePoses.RemoveAll((BotSystemSpringPoseAnimatorPose p) => p == null);
		BotSystemSpringPoseAnimatorPose[] componentsInChildren = GetComponentsInChildren<BotSystemSpringPoseAnimatorPose>(includeInactive: true);
		foreach (BotSystemSpringPoseAnimatorPose botSystemSpringPoseAnimatorPose in componentsInChildren)
		{
			if ((bool)botSystemSpringPoseAnimatorPose && !availablePoses.Contains(botSystemSpringPoseAnimatorPose))
			{
				availablePoses.Add(botSystemSpringPoseAnimatorPose);
			}
		}
		if (mainPose == null && availablePoses.Count > 0)
		{
			BotSystemSpringPoseAnimatorPose botSystemSpringPoseAnimatorPose2 = availablePoses.Find((BotSystemSpringPoseAnimatorPose p) => (bool)p && !string.IsNullOrEmpty(p.poseName) && (p.poseName.EndsWith("Pose0", StringComparison.OrdinalIgnoreCase) || p.poseName.EndsWith("_Pose0", StringComparison.OrdinalIgnoreCase)));
			mainPose = (botSystemSpringPoseAnimatorPose2 ? botSystemSpringPoseAnimatorPose2 : availablePoses[0]);
		}
		if (currentPose == null)
		{
			currentPose = mainPose;
		}
	}

	private void EnsureMainPoseSet()
	{
		if (mainPose == null && availablePoses.Count > 0)
		{
			mainPose = availablePoses[0];
		}
	}

	private void InitBones()
	{
		if (!modelRoot)
		{
			modelRoot = base.transform;
		}
		if (modelRoot == null)
		{
			return;
		}
		Dictionary<Transform, BoneSpring> dictionary = new Dictionary<Transform, BoneSpring>();
		foreach (BoneSpring bone in bones)
		{
			if (bone.transform != null)
			{
				dictionary[bone.transform] = bone;
			}
		}
		bones.Clear();
		Transform[] componentsInChildren = modelRoot.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (!(transform == modelRoot) && transform.name.Contains("Main") && !IsTransformPartOfExistingPose(transform))
			{
				BoneSpring boneSpring = new BoneSpring();
				boneSpring.name = transform.name;
				boneSpring.transform = transform;
				if (dictionary.ContainsKey(transform) && dictionary[transform].overrideDefaultSpringValues)
				{
					boneSpring.overrideDefaultSpringValues = true;
					boneSpring.spring = dictionary[transform].spring;
				}
				else
				{
					boneSpring.spring = new SpringQuaternion();
				}
				boneSpring.originalLocal = transform.localRotation;
				boneSpring.targetLocal = boneSpring.originalLocal;
				bones.Add(boneSpring);
			}
		}
		boneLookup.Clear();
		foreach (BoneSpring bone2 in bones)
		{
			if ((bool)bone2?.transform)
			{
				boneLookup[bone2.transform] = bone2;
			}
		}
		perBoneCurveIndex.Clear();
		ReassignCurvesForPose(currentPose);
	}

	private string GetFullPath(Transform t)
	{
		string text = t.name;
		Transform parent = t.parent;
		while (parent != null && parent != modelRoot)
		{
			text = parent.name + "/" + text;
			parent = parent.parent;
		}
		return text;
	}

	private string GetPathFromPoseRoot(Transform t, Transform poseRoot)
	{
		string text = t.name;
		Transform parent = t.parent;
		while (parent != null && parent != poseRoot)
		{
			text = parent.name + "/" + text;
			parent = parent.parent;
		}
		return text;
	}

	private bool IsLimbRootName(string n)
	{
		if (!string.IsNullOrEmpty(n))
		{
			return n.StartsWith("Limb Root");
		}
		return false;
	}

	private bool IsTransparentWrapper(Transform t)
	{
		if (!t)
		{
			return false;
		}
		if (!IsLimbRootName(t.name))
		{
			return t.name.Contains("IdleOffset");
		}
		return true;
	}

	private Transform GetVisibleParent(Transform t, Transform stop)
	{
		if (!t || t == stop)
		{
			return null;
		}
		Transform parent = t.parent;
		while (parent != null && parent != stop && IsTransparentWrapper(parent))
		{
			parent = parent.parent;
		}
		return parent;
	}

	private Transform GetVisibleChildOnPath(Transform visibleParent, Transform descendant)
	{
		if (!visibleParent || !descendant)
		{
			return null;
		}
		Transform transform = descendant;
		while ((bool)transform && transform.parent != visibleParent)
		{
			transform = transform.parent;
		}
		if (!transform)
		{
			return null;
		}
		while ((bool)transform && IsTransparentWrapper(transform))
		{
			Transform transform2 = descendant;
			while ((bool)transform2 && transform2.parent != transform)
			{
				transform2 = transform2.parent;
			}
			transform = transform2;
		}
		return transform;
	}

	private IEnumerable<Transform> FirstNonTransparentDescendants(Transform wrapper)
	{
		for (int i = 0; i < wrapper.childCount; i++)
		{
			Transform child = wrapper.GetChild(i);
			if (IsTransparentWrapper(child))
			{
				foreach (Transform item in FirstNonTransparentDescendants(child))
				{
					yield return item;
				}
			}
			else
			{
				yield return child;
			}
		}
	}

	private List<Transform> GetVisibleChildren(Transform parent)
	{
		List<Transform> list = new List<Transform>();
		if (!parent)
		{
			return list;
		}
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (child.name.Contains("IdleOffset"))
			{
				continue;
			}
			if (IsLimbRootName(child.name))
			{
				foreach (Transform item in FirstNonTransparentDescendants(child))
				{
					list.Add(item);
				}
			}
			else
			{
				list.Add(child);
			}
		}
		return list;
	}

	private int[] GetVisibleIndexPath(Transform t, Transform root)
	{
		List<int> list = new List<int>();
		Transform transform = t;
		while (transform != null && transform != root)
		{
			Transform visibleParent = GetVisibleParent(transform, root);
			if (!visibleParent)
			{
				break;
			}
			List<Transform> visibleChildren = GetVisibleChildren(visibleParent);
			Transform visibleChildOnPath = GetVisibleChildOnPath(visibleParent, transform);
			int num = visibleChildren.IndexOf(visibleChildOnPath);
			if (num < 0)
			{
				break;
			}
			list.Add(num);
			transform = visibleParent;
		}
		list.Reverse();
		return list.ToArray();
	}

	private Transform FollowVisibleIndexPath(int[] path, Transform root)
	{
		if ((bool)root && (path == null || path.Length == 0))
		{
			return root;
		}
		Transform transform = root;
		for (int i = 0; i < path.Length; i++)
		{
			List<Transform> visibleChildren = GetVisibleChildren(transform);
			int num = path[i];
			if (num < 0 || num >= visibleChildren.Count)
			{
				return null;
			}
			transform = visibleChildren[num];
		}
		return transform;
	}

	private void ReassignCurvesForPose(BotSystemSpringPoseAnimatorPose pose)
	{
		perBoneCurveIndex.Clear();
		if (pose == null)
		{
			return;
		}
		if (curvePickMode == CurvePickMode.Sequential)
		{
			int num = 0;
			{
				foreach (BoneSpring bone in bones)
				{
					int curveIndexForBone = pose.GetCurveIndexForBone(num);
					perBoneCurveIndex[bone.transform] = curveIndexForBone;
					num++;
				}
				return;
			}
		}
		System.Random rng = new System.Random(randomSeed);
		foreach (BoneSpring bone2 in bones)
		{
			int randomCurveIndex = pose.GetRandomCurveIndex(rng);
			perBoneCurveIndex[bone2.transform] = randomCurveIndex;
		}
	}

	private void UpdateImpulse()
	{
		if (hasImpulse)
		{
			impulseTimer += Time.deltaTime;
			float num = ((impulseTime <= 0.0001f) ? 1f : Mathf.Clamp01(impulseTimer / impulseTime));
			float value = ((impulseTransitionCurve != null) ? impulseTransitionCurve.Evaluate(num) : num);
			value = Mathf.Clamp(value, 0f - overshootCap, 1f + overshootCap);
			BotSystemSpringPoseAnimatorPose botSystemSpringPoseAnimatorPose = (nextPose ? nextPose : currentPose);
			if ((bool)botSystemSpringPoseAnimatorPose)
			{
				ApplyPoseInstant(botSystemSpringPoseAnimatorPose, value, currentPoseIdleTime, freezeIdle: false);
			}
			if (impulseTimer >= impulseTime)
			{
				currentPose = (nextPose ? nextPose : currentPose);
				currentPoseIdleTime = 0f;
				hasImpulse = false;
				impulseTransitionCurve = null;
				previousPose = null;
				previousPoseCurveIndex.Clear();
				ReassignCurvesForPose(currentPose);
			}
		}
	}

	private void UpdateIdle()
	{
		if (!isPoseActive)
		{
			return;
		}
		if (currentPose == null)
		{
			if (!(mainPose != null))
			{
				Debug.LogWarning("UpdateIdle: currentPose and mainPose are both null on " + base.gameObject.name);
				return;
			}
			currentPose = mainPose;
		}
		SetTargetsFromPose(currentPose, includeIdle: true, currentPoseIdleTime);
	}

	private Quaternion GetAnimatedPoseLocal(BotSystemSpringPoseAnimatorPose pose, BoneSpring mainBone, float idleTime, bool freezeIdle)
	{
		if (pose == null || mainBone?.transform == null)
		{
			return mainBone?.originalLocal ?? Quaternion.identity;
		}
		BotSystemSpringPoseAnimatorPose.BoneOffset boneDataForMain = pose.GetBoneDataForMain(mainBone.transform);
		if (boneDataForMain == null || boneDataForMain.bone == null)
		{
			return mainBone.originalLocal;
		}
		Quaternion localRotation = boneDataForMain.bone.localRotation;
		int curveIndex = 0;
		if (perBoneCurveIndex.TryGetValue(mainBone.transform, out var value))
		{
			curveIndex = value;
		}
		float curveEval = pose.GetCurveEval(curveIndex, freezeIdle ? 1f : idleTime, boneDataForMain.curveOffset);
		if ((bool)boneDataForMain.idleOffsetTransform)
		{
			Quaternion quaternion = Quaternion.Slerp(Quaternion.identity, boneDataForMain.idleOffsetTransform.localRotation, boneDataForMain.amount * curveEval);
			if (!boneDataForMain.additive)
			{
				return quaternion;
			}
			return localRotation * quaternion;
		}
		Quaternion quaternion2 = Quaternion.Euler(boneDataForMain.offsetEuler * (boneDataForMain.amount * curveEval));
		if (!boneDataForMain.additive)
		{
			return quaternion2;
		}
		return localRotation * quaternion2;
	}

	public void SetTargetsFromPose(BotSystemSpringPoseAnimatorPose pose, bool includeIdle, float idleTime, float amountMul = 1f)
	{
		if (pose == null)
		{
			return;
		}
		LinkBonesToPose(pose);
		foreach (BoneSpring bone in bones)
		{
			if (bone?.transform == null)
			{
				continue;
			}
			if (!bone.targetTransform)
			{
				bone.idleLocalDelta = Quaternion.identity;
				bone.targetLocal = bone.originalLocal;
				continue;
			}
			Quaternion rotation = (bone.targetTransform.parent ? bone.targetTransform.parent.rotation : Quaternion.identity);
			Quaternion rotation2 = bone.targetTransform.rotation;
			Quaternion quaternion = Quaternion.Inverse(rotation) * rotation2;
			if (!includeIdle)
			{
				bone.idleLocalDelta = Quaternion.identity;
				bone.targetLocal = quaternion;
				continue;
			}
			BotSystemSpringPoseAnimatorPose.BoneOffset boneDataForMain = pose.GetBoneDataForMain(bone.transform);
			if (boneDataForMain == null)
			{
				bone.targetLocal = quaternion;
				continue;
			}
			int value;
			int curveIndex = (perBoneCurveIndex.TryGetValue(bone.transform, out value) ? value : 0);
			float curveEval = pose.GetCurveEval(curveIndex, idleTime, boneDataForMain.curveOffset);
			float num = boneDataForMain.amount * amountMul;
			Quaternion quaternion2;
			if ((bool)boneDataForMain.idleOffsetTransform)
			{
				Quaternion localRotation = boneDataForMain.idleOffsetTransform.localRotation;
				quaternion2 = Quaternion.Slerp(Quaternion.identity, localRotation, num * curveEval);
			}
			else
			{
				quaternion2 = Quaternion.Euler(boneDataForMain.offsetEuler * (num * curveEval));
			}
			bone.idleLocalDelta = quaternion2;
			bone.targetLocal = (boneDataForMain.additive ? (quaternion * quaternion2) : quaternion2);
		}
	}

	private void ApplyPoseInstant(BotSystemSpringPoseAnimatorPose pose, float poseLerp, float idleTime, bool freezeIdle)
	{
		if (pose == null)
		{
			return;
		}
		LinkBonesToPose(pose);
		foreach (BoneSpring bone in bones)
		{
			if (!bone.transform)
			{
				continue;
			}
			if (hasImpulse && previousPose != null && nextPose != null)
			{
				Quaternion quaternion = (bone.transform.parent ? bone.transform.parent.rotation : Quaternion.identity);
				BotSystemSpringPoseAnimatorPose.BoneOffset boneDataForMain = previousPose.GetBoneDataForMain(bone.transform);
				Quaternion quaternion2 = boneDataForMain?.targetWorldRotation ?? Quaternion.identity;
				bool num = boneDataForMain != null && Mathf.Abs(quaternion2.x * quaternion2.x + quaternion2.y * quaternion2.y + quaternion2.z * quaternion2.z + quaternion2.w * quaternion2.w - 1f) < 0.01f;
				Quaternion quaternion3 = (modelRoot ? modelRoot.rotation : Quaternion.identity);
				Quaternion quaternion4 = ((boneDataForMain != null) ? (quaternion3 * boneDataForMain.targetWorldRotation) : (quaternion * bone.originalLocal));
				Quaternion quaternion5 = (num ? (Quaternion.Inverse(quaternion) * quaternion4) : bone.originalLocal);
				int curveIndex = 0;
				if (previousPoseCurveIndex.TryGetValue(bone.transform, out var value))
				{
					curveIndex = value;
				}
				float curveEval = previousPose.GetCurveEval(curveIndex, freezeIdle ? 1f : idleTime, boneDataForMain?.curveOffset ?? 0f);
				Quaternion quaternion6 = quaternion5;
				if (boneDataForMain != null)
				{
					float amount = boneDataForMain.amount;
					if ((bool)boneDataForMain.idleOffsetTransform)
					{
						Quaternion localRotation = boneDataForMain.idleOffsetTransform.localRotation;
						Quaternion quaternion7 = Quaternion.Slerp(Quaternion.identity, localRotation, amount * curveEval);
						quaternion6 = (boneDataForMain.additive ? (quaternion5 * quaternion7) : quaternion7);
					}
					else
					{
						Quaternion quaternion8 = Quaternion.Euler(boneDataForMain.offsetEuler * (amount * curveEval));
						quaternion6 = (boneDataForMain.additive ? (quaternion5 * quaternion8) : quaternion8);
					}
				}
				BotSystemSpringPoseAnimatorPose.BoneOffset boneDataForMain2 = nextPose.GetBoneDataForMain(bone.transform);
				Quaternion quaternion9 = boneDataForMain2?.targetWorldRotation ?? Quaternion.identity;
				bool num2 = boneDataForMain2 != null && Mathf.Abs(quaternion9.x * quaternion9.x + quaternion9.y * quaternion9.y + quaternion9.z * quaternion9.z + quaternion9.w * quaternion9.w - 1f) < 0.01f;
				Quaternion quaternion10 = (modelRoot ? modelRoot.rotation : Quaternion.identity);
				Quaternion quaternion11 = ((boneDataForMain2 != null) ? (quaternion10 * boneDataForMain2.targetWorldRotation) : (quaternion * bone.originalLocal));
				Quaternion quaternion12 = (num2 ? (Quaternion.Inverse(quaternion) * quaternion11) : bone.originalLocal);
				int curveIndex2 = 0;
				if (perBoneCurveIndex.TryGetValue(bone.transform, out var value2))
				{
					curveIndex2 = value2;
				}
				float curveEval2 = nextPose.GetCurveEval(curveIndex2, freezeIdle ? 1f : idleTime, boneDataForMain2?.curveOffset ?? 0f);
				Quaternion quaternion13 = quaternion12;
				if (boneDataForMain2 != null)
				{
					float amount2 = boneDataForMain2.amount;
					if ((bool)boneDataForMain2.idleOffsetTransform)
					{
						Quaternion localRotation2 = boneDataForMain2.idleOffsetTransform.localRotation;
						Quaternion quaternion14 = Quaternion.Slerp(Quaternion.identity, localRotation2, amount2 * curveEval2);
						quaternion13 = (boneDataForMain2.additive ? (quaternion12 * quaternion14) : quaternion14);
					}
					else
					{
						Quaternion quaternion15 = Quaternion.Euler(boneDataForMain2.offsetEuler * (amount2 * curveEval2));
						quaternion13 = (boneDataForMain2.additive ? (quaternion12 * quaternion15) : quaternion15);
					}
				}
				bone.targetLocal = Quaternion.SlerpUnclamped(quaternion6, quaternion13, poseLerp);
				continue;
			}
			if (!bone.targetTransform)
			{
				bone.idleLocalDelta = Quaternion.identity;
				bone.targetLocal = Quaternion.SlerpUnclamped(bone.targetLocal, bone.originalLocal, poseLerp);
				continue;
			}
			Quaternion rotation = (bone.targetTransform.parent ? bone.targetTransform.parent.rotation : Quaternion.identity);
			Quaternion rotation2 = bone.targetTransform.rotation;
			Quaternion quaternion16 = Quaternion.Inverse(rotation) * rotation2;
			BotSystemSpringPoseAnimatorPose.BoneOffset boneDataForMain3 = pose.GetBoneDataForMain(bone.transform);
			if (boneDataForMain3 == null)
			{
				bone.targetLocal = Quaternion.SlerpUnclamped(bone.originalLocal, quaternion16, poseLerp);
				continue;
			}
			int num3 = 0;
			num3 = (perBoneCurveIndex.TryGetValue(bone.transform, out var value3) ? value3 : 0);
			float curveEval3 = pose.GetCurveEval(num3, freezeIdle ? 1f : idleTime, boneDataForMain3.curveOffset);
			float amount3 = boneDataForMain3.amount;
			if ((bool)boneDataForMain3.idleOffsetTransform)
			{
				Quaternion localRotation3 = boneDataForMain3.idleOffsetTransform.localRotation;
				Quaternion quaternion17 = (bone.idleLocalDelta = Quaternion.Slerp(Quaternion.identity, localRotation3, amount3 * curveEval3));
				Quaternion quaternion18 = (boneDataForMain3.additive ? (quaternion16 * quaternion17) : quaternion17);
				bone.targetLocal = Quaternion.SlerpUnclamped(quaternion16, quaternion18, poseLerp);
			}
			else
			{
				Quaternion quaternion19 = (bone.idleLocalDelta = Quaternion.Euler(boneDataForMain3.offsetEuler * (amount3 * curveEval3)));
				Quaternion quaternion20 = (boneDataForMain3.additive ? (quaternion16 * quaternion19) : quaternion19);
				bone.targetLocal = Quaternion.SlerpUnclamped(quaternion16, quaternion20, poseLerp);
			}
		}
	}

	private void UpdateSprings()
	{
		float num = 1f;
		float num2 = 1f;
		if (currentPose != null)
		{
			num = currentPose.springSpeedMultiplier;
			num2 = currentPose.springDampingMultiplier;
		}
		float num3 = defaultSpringSpeed * num;
		float num4 = defaultSpringDamping * num2;
		if (num3 <= 0f)
		{
			Debug.LogWarning($"BotSystemSpringPoseAnimator: Invalid spring speed ({num3}) on {base.gameObject.name}");
			num3 = 1f;
		}
		if (num4 < 0f)
		{
			Debug.LogWarning($"BotSystemSpringPoseAnimator: Invalid spring damping ({num4}) on {base.gameObject.name}");
			num4 = 0f;
		}
		List<BoneSpring> list = new List<BoneSpring>(bones);
		list.Sort(delegate(BoneSpring boneSpring, BoneSpring boneSpring2)
		{
			if (!(boneSpring?.transform) || !(boneSpring2?.transform))
			{
				return 0;
			}
			int depth = GetDepth(boneSpring.transform);
			int depth2 = GetDepth(boneSpring2.transform);
			return depth.CompareTo(depth2);
		});
		foreach (BoneSpring item in list)
		{
			if ((bool)item.transform)
			{
				if (!item.overrideDefaultSpringValues && !bonesWithTempOverride.Contains(item))
				{
					item.spring.speed = num3;
					item.spring.damping = num4;
				}
				Quaternion targetRotation = (item.transform.parent ? (item.transform.parent.rotation * item.targetLocal) : item.targetLocal);
				item.transform.rotation = SemiFunc.SpringQuaternionGet(item.spring, targetRotation);
			}
		}
	}

	private int GetDepth(Transform t)
	{
		int num = 0;
		Transform transform = t;
		Transform transform2 = (modelRoot ? modelRoot : base.transform);
		while (transform != null && transform != transform2)
		{
			num++;
			transform = transform.parent;
		}
		return num;
	}

	private List<Limb> GetLimbSolveOrder()
	{
		List<Limb> list = new List<Limb>(limbs);
		list.RemoveAll((Limb l) => l == null || !l.rootTransform);
		list.Sort((Limb limb, Limb limb2) => GetDepth(limb.rootTransform).CompareTo(GetDepth(limb2.rootTransform)));
		return list;
	}

	private bool IsTransformPartOfExistingPose(Transform t)
	{
		if (!t)
		{
			return false;
		}
		Transform transform = t;
		while (transform != null)
		{
			if (transform.GetComponent<BotSystemSpringPoseAnimatorPose>() != null)
			{
				return true;
			}
			if (transform == modelRoot)
			{
				break;
			}
			transform = transform.parent;
		}
		return false;
	}

	private void BakeIdsHere(Transform root, bool regenerateExisting)
	{
		if ((bool)root)
		{
			AssignRecursive(root, "", root.name + "[0]", regenerateExisting);
		}
	}

	private void AssignRecursive(Transform t, string idxPath, string nameOrdPath, bool regen)
	{
		if (boneLookup.TryGetValue(t, out var value) && (regen || string.IsNullOrEmpty(value.indexPath) || string.IsNullOrEmpty(value.nameOrdinalPath)))
		{
			value.indexPath = idxPath;
			value.nameOrdinalPath = nameOrdPath;
		}
		int childCount = t.childCount;
		Dictionary<string, int> dictionary = new Dictionary<string, int>(childCount);
		for (int i = 0; i < childCount; i++)
		{
			Transform child = t.GetChild(i);
			dictionary.TryGetValue(child.name, out var value2);
			dictionary[child.name] = value2 + 1;
			string idxPath2 = (string.IsNullOrEmpty(idxPath) ? i.ToString() : $"{idxPath}/{i}");
			string nameOrdPath2 = $"{nameOrdPath}/{child.name}[{value2}]";
			AssignRecursive(child, idxPath2, nameOrdPath2, regen);
		}
	}

	private void BuildMainMaps(out Dictionary<string, Transform> byIndex, out Dictionary<string, Transform> byNameOrd)
	{
		byIndex = new Dictionary<string, Transform>(1024);
		byNameOrd = new Dictionary<string, Transform>(1024);
		Transform transform = (modelRoot ? modelRoot : base.transform);
		WalkBuildMaps(transform, "", transform.name + "[0]", byIndex, byNameOrd);
	}

	private void WalkBuildMaps(Transform t, string idxPath, string nameOrdPath, Dictionary<string, Transform> byIndex, Dictionary<string, Transform> byNameOrd)
	{
		if (boneLookup.TryGetValue(t, out var value) && !string.IsNullOrEmpty(value.indexPath))
		{
			byIndex[value.indexPath] = t;
			if (!string.IsNullOrEmpty(value.nameOrdinalPath))
			{
				byNameOrd[value.nameOrdinalPath] = t;
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
			dictionary.TryGetValue(child.name, out var value2);
			dictionary[child.name] = value2 + 1;
			string idxPath2 = (string.IsNullOrEmpty(idxPath) ? i.ToString() : $"{idxPath}/{i}");
			string nameOrdPath2 = $"{nameOrdPath}/{child.name}[{value2}]";
			WalkBuildMaps(child, idxPath2, nameOrdPath2, byIndex, byNameOrd);
		}
	}

	private void LinkBonesToPose(BotSystemSpringPoseAnimatorPose pose)
	{
		if (pose == null || _lastLinkedPose == pose)
		{
			return;
		}
		_lastLinkedPose = pose;
		BuildMainMaps(out var _, out var _);
		pose.BuildPoseMaps(out var byIndex2, out var byNameOrd2);
		foreach (BoneSpring bone in bones)
		{
			if (bone?.transform == null)
			{
				continue;
			}
			Transform value = null;
			if (!string.IsNullOrEmpty(bone.indexPath))
			{
				byIndex2.TryGetValue(bone.indexPath, out value);
			}
			if (value == null && !string.IsNullOrEmpty(bone.nameOrdinalPath))
			{
				byNameOrd2.TryGetValue(bone.nameOrdinalPath, out value);
			}
			if (value == null && (string.IsNullOrEmpty(bone.indexPath) || string.IsNullOrEmpty(bone.nameOrdinalPath)))
			{
				string text = "";
				string text2 = (modelRoot ? modelRoot.name : base.transform.name) + "[0]";
				List<Transform> list = new List<Transform>();
				Transform parent = bone.transform;
				Transform transform = (modelRoot ? modelRoot : base.transform);
				while ((bool)parent && parent != transform)
				{
					list.Add(parent);
					parent = parent.parent;
				}
				list.Reverse();
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
				if (!byIndex2.TryGetValue(text, out value))
				{
					byNameOrd2.TryGetValue(text2, out value);
				}
				if (value != null)
				{
					bone.indexPath = text;
					bone.nameOrdinalPath = text2;
				}
			}
			bone.targetTransform = value;
			if (!(value != null))
			{
				continue;
			}
			BotSystemSpringPoseAnimatorPose.BoneOffset boneData = pose.GetBoneData(value);
			if (boneData != null)
			{
				boneData.linkedMainRigBone = bone.transform;
			}
			if (string.IsNullOrEmpty(bone.indexPath) || string.IsNullOrEmpty(bone.nameOrdinalPath))
			{
				pose.GetIdsForPoseBone(value, out var idx, out var nameOrd);
				if (!string.IsNullOrEmpty(idx) && string.IsNullOrEmpty(bone.indexPath))
				{
					bone.indexPath = idx;
				}
				if (!string.IsNullOrEmpty(nameOrd) && string.IsNullOrEmpty(bone.nameOrdinalPath))
				{
					bone.nameOrdinalPath = nameOrd;
				}
			}
		}
	}

	private string NormalizeBoneName(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		return Regex.Replace(Regex.Replace(Regex.Replace(s.Trim(), "\\s+", " "), "\\d+", "").Replace("_", " ").Replace("-", " "), "\\s+", " ").Trim().ToLowerInvariant();
	}

	private Transform FindMatchingBoneInPose(Transform runtimeBone, Transform poseRoot)
	{
		if (!runtimeBone || !poseRoot)
		{
			return null;
		}
		Transform transform = null;
		if ((bool)runtimeBone.parent && runtimeBone.parent != modelRoot)
		{
			transform = FindMatchingBoneInPose(runtimeBone.parent, poseRoot);
		}
		string text = NormalizeBoneName(runtimeBone.name);
		Transform[] componentsInChildren = poseRoot.GetComponentsInChildren<Transform>(includeInactive: true);
		List<Transform> list = new List<Transform>();
		if (transform != null)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (NormalizeBoneName(child.name) == text)
				{
					list.Add(child);
				}
			}
		}
		if (list.Count == 0)
		{
			Transform[] array = componentsInChildren;
			foreach (Transform transform2 in array)
			{
				if (NormalizeBoneName(transform2.name) == text)
				{
					list.Add(transform2);
				}
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		int siblingIndex = runtimeBone.GetSiblingIndex();
		list.Sort((Transform transform3, Transform transform4) => transform3.GetSiblingIndex().CompareTo(transform4.GetSiblingIndex()));
		if (siblingIndex < list.Count)
		{
			return list[siblingIndex];
		}
		return list[0];
	}

	private List<Transform> GetSiblingsWithSameNormalizedName(Transform t)
	{
		List<Transform> list = new List<Transform>();
		if (!t || !t.parent)
		{
			return list;
		}
		string text = NormalizeBoneName(t.name);
		for (int i = 0; i < t.parent.childCount; i++)
		{
			Transform child = t.parent.GetChild(i);
			if (NormalizeBoneName(child.name) == text)
			{
				list.Add(child);
			}
		}
		return list;
	}

	private int GetBoneMatchScore(Transform runtimeBone, Transform poseBone, Transform runtimeRoot, Transform poseRoot)
	{
		int num = 0;
		List<string> parentChain = GetParentChain(runtimeBone, runtimeRoot);
		List<string> parentChain2 = GetParentChain(poseBone, poseRoot);
		int num2 = Mathf.Min(parentChain.Count, parentChain2.Count);
		for (int i = 1; i <= num2; i++)
		{
			int index = parentChain.Count - i;
			int index2 = parentChain2.Count - i;
			if (!(parentChain[index] == parentChain2[index2]))
			{
				break;
			}
			num += 10;
		}
		if (runtimeBone.GetSiblingIndex() == poseBone.GetSiblingIndex())
		{
			num += 100;
		}
		return num;
	}

	private List<string> GetParentChain(Transform target, Transform root)
	{
		List<string> list = new List<string>();
		Transform parent = target.parent;
		while (parent != null && parent != root)
		{
			list.Insert(0, parent.name);
			parent = parent.parent;
		}
		return list;
	}

	private void RepairPoseChildIndexPathsRuntime()
	{
		if (modelRoot == null)
		{
			return;
		}
		foreach (BotSystemSpringPoseAnimatorPose availablePose in availablePoses)
		{
			if (availablePose == null)
			{
				continue;
			}
			foreach (BotSystemSpringPoseAnimatorPose.BoneOffset boneOffset in availablePose.boneOffsets)
			{
				if ((bool)boneOffset.bone)
				{
					boneOffset.childIndexPath = GetVisibleIndexPath(boneOffset.bone, availablePose.transform);
				}
			}
		}
		_lastLinkedPose = null;
	}

	public void ImpulseSetPose(BotSystemSpringPoseAnimatorPose pose, float time)
	{
		ImpulseSetPose(pose, time, defaultImpulseCurve);
	}

	public void ImpulseSetPose(BotSystemSpringPoseAnimatorPose pose, float time, AnimationCurve transitionCurve)
	{
		if (!(pose == null))
		{
			ClearAllLimbStretches();
			poseHoldOverrideTimer = 0.1f;
			isPoseActive = true;
			if (currentPose != pose)
			{
				currentPose = pose;
				ReassignCurvesForPose(currentPose);
				LinkBonesToPose(currentPose);
			}
			SetTargetsFromPose(currentPose, includeIdle: true, currentPoseIdleTime);
		}
	}

	public BotSystemSpringPoseAnimatorPose GetPoseByName(string poseNameToFind)
	{
		if (string.IsNullOrEmpty(poseNameToFind))
		{
			return null;
		}
		for (int i = 0; i < availablePoses.Count; i++)
		{
			BotSystemSpringPoseAnimatorPose botSystemSpringPoseAnimatorPose = availablePoses[i];
			if ((bool)botSystemSpringPoseAnimatorPose && string.Equals(botSystemSpringPoseAnimatorPose.poseName, poseNameToFind, StringComparison.OrdinalIgnoreCase))
			{
				return botSystemSpringPoseAnimatorPose;
			}
		}
		return null;
	}

	public void SetPoseByName(string poseNameToSet, float transitionTime = 0.5f)
	{
		if (string.IsNullOrEmpty(poseNameToSet))
		{
			Debug.LogWarning("BotSystemSpringPoseAnimator: Cannot set pose with null or empty name on " + base.gameObject.name);
			return;
		}
		BotSystemSpringPoseAnimatorPose poseByName = GetPoseByName(poseNameToSet);
		if ((bool)poseByName)
		{
			ImpulseSetPose(poseByName, transitionTime);
		}
		else
		{
			Debug.LogWarning("BotSystemSpringPoseAnimator: Pose '" + poseNameToSet + "' not found in availablePoses on " + base.gameObject.name);
		}
	}

	public void DeactivatePose()
	{
		isPoseActive = false;
		ResetTargetsToOriginal();
	}

	public void DeactivatePoseAll()
	{
		isPoseActive = false;
		ResetTargetsToOriginal();
	}

	private void ResetTargetsToOriginal()
	{
		foreach (BoneSpring bone in bones)
		{
			if (!(bone?.transform == null))
			{
				bone.targetLocal = bone.originalLocal;
				bone.idleLocalDelta = Quaternion.identity;
			}
		}
	}

	private BoneSpring NearestAncestorInMap(Transform t, Dictionary<Transform, BoneSpring> map)
	{
		Transform transform = (t ? t.parent : null);
		Transform rootSentinel = RootSentinel;
		while (transform != null && transform != rootSentinel)
		{
			if (map.TryGetValue(transform, out var value))
			{
				return value;
			}
			transform = transform.parent;
		}
		return null;
	}

	private int CompareByHierarchy(BoneSpring a, BoneSpring b)
	{
		if (a == null || b == null || !a.transform || !b.transform)
		{
			return 0;
		}
		List<Transform> list = new List<Transform>();
		List<Transform> list2 = new List<Transform>();
		Transform rootSentinel = RootSentinel;
		Transform parent = a.transform;
		while (parent != null && parent != rootSentinel)
		{
			list.Insert(0, parent);
			parent = parent.parent;
		}
		Transform parent2 = b.transform;
		while (parent2 != null && parent2 != rootSentinel)
		{
			list2.Insert(0, parent2);
			parent2 = parent2.parent;
		}
		int num = Mathf.Min(list.Count, list2.Count);
		for (int i = 0; i < num; i++)
		{
			if (list[i] != list2[i])
			{
				return list[i].GetSiblingIndex().CompareTo(list2[i].GetSiblingIndex());
			}
		}
		return list.Count.CompareTo(list2.Count);
	}

	private string StripMain(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			return s.Replace("Main", "").Trim();
		}
		return s;
	}

	public Limb GetLimbByName(string limbName)
	{
		if (string.IsNullOrEmpty(limbName))
		{
			return null;
		}
		foreach (Limb limb in limbs)
		{
			if (string.Equals(limb.name, limbName, StringComparison.OrdinalIgnoreCase))
			{
				return limb;
			}
		}
		return null;
	}

	public Limb GetLimbContainingBone(Transform bone)
	{
		if (!bone)
		{
			return null;
		}
		foreach (Limb limb in limbs)
		{
			foreach (BoneSpring bone2 in limb.bones)
			{
				if (bone2.transform == bone)
				{
					return limb;
				}
			}
		}
		return null;
	}

	private static Vector3 SafeNormalize(Vector3 v, Vector3 fallback)
	{
		float sqrMagnitude = v.sqrMagnitude;
		if (!(sqrMagnitude > 1.0000001E-06f))
		{
			return fallback;
		}
		return v / Mathf.Sqrt(sqrMagnitude);
	}

	private static Vector3 BuildStableUp(Vector3 forward, Vector3 upRef, Vector3 prevUp)
	{
		forward = SafeNormalize(forward, Vector3.forward);
		Vector3 vector = SafeNormalize(upRef, Vector3.up);
		if (Mathf.Abs(Vector3.Dot(forward, vector)) > 0.9995f && prevUp.sqrMagnitude > 1.0000001E-06f)
		{
			Vector3 v = Vector3.ProjectOnPlane(prevUp, forward);
			if (v.sqrMagnitude > 1.0000001E-06f)
			{
				return SafeNormalize(v, vector);
			}
		}
		Vector3 v2 = Vector3.ProjectOnPlane(vector, forward);
		if (v2.sqrMagnitude > 1.0000001E-06f)
		{
			return SafeNormalize(v2, vector);
		}
		v2 = Vector3.ProjectOnPlane((Mathf.Abs(forward.y) < 0.95f) ? Vector3.up : Vector3.right, forward);
		return SafeNormalize(v2, Vector3.up);
	}

	private static Quaternion LookRotationNoRoll(Vector3 forward, Vector3 upRef, Vector3 prevUp, out Vector3 usedUp)
	{
		if (forward.sqrMagnitude < 1.0000001E-06f)
		{
			usedUp = ((prevUp.sqrMagnitude > 1.0000001E-06f) ? prevUp : Vector3.up);
			return Quaternion.LookRotation(Vector3.forward, usedUp);
		}
		forward = SafeNormalize(forward, Vector3.forward);
		usedUp = BuildStableUp(forward, upRef, prevUp);
		return Quaternion.LookRotation(forward, usedUp);
	}

	private static Quaternion RemoveRoll(Quaternion q, Vector3 upRef, Vector3 prevUp, out Vector3 usedUp)
	{
		return LookRotationNoRoll(q * Vector3.forward, upRef, prevUp, out usedUp);
	}

	private static Quaternion FromToRotationSafe(Vector3 _from, Vector3 _to)
	{
		Vector3 vector = SafeNormalize(_from, Vector3.forward);
		Vector3 vector2 = SafeNormalize(_to, vector);
		float num = Vector3.Dot(vector, vector2);
		if (num < -0.9999f)
		{
			Vector3 rhs = ((Mathf.Abs(vector.y) < 0.95f) ? Vector3.up : Vector3.right);
			Vector3 axis = SafeNormalize(Vector3.Cross(vector, rhs), Vector3.up);
			return Quaternion.AngleAxis(180f, axis);
		}
		if (num > 0.9999f)
		{
			return Quaternion.identity;
		}
		return Quaternion.FromToRotation(vector, vector2);
	}

	private static Vector3 TransportUp(Vector3 _prevFwd, Vector3 _newFwd, Vector3 _prevUp)
	{
		Vector3 vector = SafeNormalize(_newFwd, Vector3.forward);
		return SafeNormalize(Vector3.ProjectOnPlane(FromToRotationSafe(SafeNormalize((_prevFwd.sqrMagnitude < 1.0000001E-06f) ? vector : _prevFwd, vector), vector) * ((_prevUp.sqrMagnitude < 1.0000001E-06f) ? Vector3.up : _prevUp), vector), Vector3.up);
	}

	private static Quaternion LookRotationNoRollTransport(Vector3 _forward, Vector3 _upRef, ref Vector3 _prevFwd, ref Vector3 _prevUp, float _maxTwistDegPerSec = 1080f)
	{
		if (_forward.sqrMagnitude < 1.0000001E-06f)
		{
			Vector3 forward = ((_prevFwd.sqrMagnitude > 1.0000001E-06f) ? _prevFwd : Vector3.forward);
			Vector3 upwards = ((_prevUp.sqrMagnitude > 1.0000001E-06f) ? _prevUp : Vector3.up);
			return Quaternion.LookRotation(forward, upwards);
		}
		Vector3 vector = SafeNormalize(_forward, Vector3.forward);
		Vector3 vector2 = TransportUp(_prevFwd, vector, _prevUp);
		Vector3 v = Vector3.ProjectOnPlane(SafeNormalize(_upRef, Vector3.up), vector);
		Vector3 to = ((v.sqrMagnitude > 1.0000001E-06f) ? SafeNormalize(v, Vector3.up) : vector2);
		Vector3 vector3 = vector2;
		float num = _maxTwistDegPerSec * Time.deltaTime;
		Vector3 vector4 = Quaternion.AngleAxis(Mathf.Clamp(Vector3.SignedAngle(vector3, to, vector), 0f - num, num), vector) * vector3;
		_prevFwd = vector;
		_prevUp = vector4;
		return Quaternion.LookRotation(vector, vector4);
	}

	private void CaptureBind(LimbChain c)
	{
		if (c == null || c.root == null || c.joints == null || c.joints.Length < 2)
		{
			return;
		}
		int num = c.joints.Length - 1;
		c.lenBind = new float[num];
		c.upRoot = new Vector3[num];
		c.bindMeshScaleY = new float[num + 1];
		c.bindMeshScaleZ = new float[num + 1];
		for (int i = 0; i < num; i++)
		{
			Transform transform = c.joints[i];
			Transform transform2 = c.joints[i + 1];
			if ((bool)transform && (bool)transform2)
			{
				Vector3 vector = transform2.position - transform.position;
				c.lenBind[i] = vector.magnitude;
				Vector3 up = transform.up;
				c.upRoot[i] = c.root.InverseTransformDirection(up);
				if (c.meshes != null && i < c.meshes.Length && (bool)c.meshes[i])
				{
					c.bindMeshScaleY[i] = c.meshes[i].localScale.y;
					c.bindMeshScaleZ[i] = c.meshes[i].localScale.z;
				}
				else
				{
					c.bindMeshScaleY[i] = 1f;
					c.bindMeshScaleZ[i] = 1f;
				}
			}
		}
		if (c.meshes != null && num < c.meshes.Length && (bool)c.meshes[num])
		{
			c.bindMeshScaleY[num] = c.meshes[num].localScale.y;
			c.bindMeshScaleZ[num] = c.meshes[num].localScale.z;
		}
		else
		{
			c.bindMeshScaleY[num] = 1f;
			c.bindMeshScaleZ[num] = 1f;
		}
		if (c.grabPoint != null && c.joints.Length != 0)
		{
			Transform transform3 = c.joints[c.joints.Length - 1];
			if (transform3 != null)
			{
				c.bindTipOffset = transform3.InverseTransformVector(c.grabPoint.position - transform3.position);
			}
		}
		else
		{
			c.bindTipOffset = Vector3.zero;
		}
		c.lastUp = new Vector3[num + 1];
		for (int j = 0; j <= num; j++)
		{
			Transform transform4 = c.joints[j];
			c.lastUp[j] = (transform4 ? transform4.up : Vector3.up);
		}
		c.lastUpRoot = (c.root ? c.root.up : Vector3.up);
		c.lastFwdRoot = (c.root ? c.root.forward : Vector3.forward);
		Transform parent = c.root.parent;
		if (parent != null && IsLimbRootName(parent.name))
		{
			c.restLimbRootLocalRotation = parent.localRotation;
		}
		else
		{
			c.restLimbRootLocalRotation = Quaternion.identity;
		}
		c.bindFwdLocal = new Vector3[num + 1];
		c.bindUpLocal = new Vector3[num + 1];
		c.bindAxisFix = new Quaternion[num + 1];
		for (int k = 0; k < num; k++)
		{
			Transform transform5 = c.joints[k];
			Transform transform6 = c.joints[k + 1];
			if (!transform5 || !transform6)
			{
				c.bindFwdLocal[k] = Vector3.forward;
				c.bindUpLocal[k] = Vector3.up;
				c.bindAxisFix[k] = Quaternion.identity;
				continue;
			}
			Vector3 vector2 = transform5.InverseTransformDirection(transform6.position - transform5.position);
			vector2 = ((vector2.sqrMagnitude > 1.0000001E-06f) ? vector2.normalized : Vector3.forward);
			Vector3 vector3 = (transform5 ? transform5.InverseTransformDirection(transform5.up) : Vector3.up);
			vector3 = ((vector3.sqrMagnitude > 1.0000001E-06f) ? vector3.normalized : Vector3.up);
			Vector3 rhs = Vector3.Normalize(Vector3.Cross(vector3, vector2));
			vector3 = Vector3.Normalize(Vector3.Cross(vector2, rhs));
			if (vector3.sqrMagnitude < 1.0000001E-06f)
			{
				vector3 = Vector3.up;
			}
			c.bindFwdLocal[k] = vector2;
			c.bindUpLocal[k] = vector3;
			c.bindAxisFix[k] = Quaternion.LookRotation(vector2, vector3);
		}
		Transform transform7 = c.joints[num];
		Vector3 vector4 = ((!transform7 || !(c.bindTipOffset.sqrMagnitude > 1.0000001E-06f)) ? ((num > 0) ? c.bindFwdLocal[num - 1] : Vector3.forward) : c.bindTipOffset.normalized);
		Vector3 vector5 = (transform7 ? transform7.InverseTransformDirection(transform7.up) : Vector3.up);
		vector5 = ((vector5.sqrMagnitude > 1.0000001E-06f) ? vector5.normalized : Vector3.up);
		Vector3 rhs2 = Vector3.Normalize(Vector3.Cross(vector5, vector4));
		vector5 = Vector3.Normalize(Vector3.Cross(vector4, rhs2));
		if (vector5.sqrMagnitude < 1.0000001E-06f)
		{
			vector5 = Vector3.up;
		}
		c.bindFwdLocal[num] = vector4;
		c.bindUpLocal[num] = vector5;
		c.bindAxisFix[num] = Quaternion.LookRotation(vector4, vector5);
		c.isInitialized = true;
	}

	private void SolveTo(LimbChain c, Vector3 target, float straightenDist = 0.15f, float fullyPosedDist = 0.05f)
	{
		if (c == null || !c.isInitialized || c.root == null || c.joints == null)
		{
			return;
		}
		int num = c.joints.Length - 1;
		if (num < 1)
		{
			return;
		}
		if (c.lastUp == null || c.lastUp.Length != num + 1)
		{
			c.lastUp = new Vector3[num + 1];
			for (int i = 0; i <= num; i++)
			{
				c.lastUp[i] = (c.joints[i] ? c.joints[i].up : Vector3.up);
			}
		}
		Transform parent = c.root.parent;
		if (parent != null && IsLimbRootName(parent.name))
		{
			Vector3 forward = target - parent.position;
			Vector3 upRef = (mainActorTransform ? mainActorTransform.up : Vector3.up);
			Quaternion quaternion = LookRotationNoRollTransport(forward, upRef, ref c.lastFwdRoot, ref c.lastUpRoot, 720f);
			Transform parent2 = parent.parent;
			Quaternion localRotation = (parent2 ? (Quaternion.Inverse(parent2.rotation) * quaternion) : quaternion);
			parent.localRotation = localRotation;
			if (boneLookup.TryGetValue(parent, out var value))
			{
				value.spring.lastRotation = quaternion;
				value.spring.springVelocity = Vector3.zero;
			}
		}
		Vector3 position = c.joints[0].position;
		Vector3 vector = target - position;
		float magnitude = vector.magnitude;
		Quaternion[] array = new Quaternion[num + 1];
		for (int j = 0; j <= num; j++)
		{
			if (boneLookup.TryGetValue(c.joints[j], out var value2))
			{
				Quaternion quaternion2 = (c.joints[j].parent ? c.joints[j].parent.rotation : Quaternion.identity);
				array[j] = quaternion2 * value2.targetLocal;
			}
			else
			{
				array[j] = c.joints[j].rotation;
			}
		}
		float[] array2 = new float[num + 1];
		for (int k = 0; k < num; k++)
		{
			array2[k] = c.lenBind[k];
		}
		bool flag = (bool)c.grabPoint && c.grabPoint != c.joints[num] && c.bindTipOffset.sqrMagnitude > 0.001f;
		array2[num] = (flag ? c.bindTipOffset.magnitude : ((num > 0) ? (c.lenBind[num - 1] * 0.5f) : 1f));
		float num2 = 0f;
		for (int l = 0; l <= num; l++)
		{
			num2 += array2[l];
		}
		num2 *= rigScale;
		Vector3 vector2 = ((magnitude > 0.001f) ? vector.normalized : Vector3.forward);
		float t;
		if (magnitude <= fullyPosedDist)
		{
			t = 0f;
		}
		else if (magnitude >= straightenDist)
		{
			t = 1f;
		}
		else
		{
			float num3 = straightenDist - fullyPosedDist;
			if (num3 > 0.001f)
			{
				float t2 = (magnitude - fullyPosedDist) / num3;
				t = Mathf.SmoothStep(0f, 1f, t2);
			}
			else
			{
				t = 1f;
			}
		}
		Quaternion[] array3 = new Quaternion[num + 1];
		Vector3[] array4 = new Vector3[num + 1];
		array4[0] = position;
		Vector3[] array5 = new Vector3[num + 1];
		for (int m = 0; m <= num; m++)
		{
			Vector3 vector3 = target - array4[m];
			Vector3 forward2 = ((vector3.sqrMagnitude > 0.001f) ? vector3.normalized : vector2);
			Vector3 vector4 = array[m] * Vector3.up;
			Vector3 vector5 = (mainActorTransform ? mainActorTransform.up : Vector3.up);
			Vector3 upRef2 = Vector3.Lerp(vector4, vector5, t);
			Vector3 usedUp;
			Quaternion quaternion3 = LookRotationNoRoll(forward2, upRef2, c.lastUp[m], out usedUp);
			array3[m] = Quaternion.Slerp(array[m], quaternion3, t);
			array5[m] = usedUp;
			if (m < num)
			{
				Vector3 vector6 = array3[m] * c.bindFwdLocal[m];
				array4[m + 1] = array4[m] + vector6 * array2[m];
			}
		}
		Vector3 vector7 = array4[num];
		if (flag)
		{
			vector7 += array3[num] * (c.bindTipOffset * rigScale);
		}
		else
		{
			vector7 += array3[num] * Vector3.forward * array2[num] * rigScale;
		}
		float num4 = Vector3.Distance(position, vector7);
		float value3 = ((num4 > 0.001f) ? (magnitude / num4) : 1f);
		value3 = Mathf.Clamp(value3, 0.1f, maxStretch);
		Vector3[] array6 = new Vector3[num + 1];
		array6[0] = position;
		for (int n = 0; n < num; n++)
		{
			Vector3 vector8 = array3[n] * c.bindFwdLocal[n];
			float num5 = array2[n] * value3;
			array6[n + 1] = array6[n] + vector8 * num5;
		}
		for (int num6 = 0; num6 < num; num6++)
		{
			Transform transform = c.joints[num6];
			if ((bool)transform)
			{
				transform.rotation = array3[num6];
				transform.position = array6[num6];
				if (boneLookup.TryGetValue(transform, out var value4))
				{
					value4.spring.lastRotation = array3[num6];
					value4.spring.springVelocity = Vector3.zero;
				}
				if (showDebugCrosses)
				{
					DrawDebugCross(array6[num6], debugCrossSize, Color.green);
				}
				if (c.meshes != null && num6 < c.meshes.Length && (bool)c.meshes[num6])
				{
					Transform transform2 = c.meshes[num6];
					transform2.localScale = (stretchAlongY ? new Vector3(transform2.localScale.x, c.bindMeshScaleY[num6] * value3, transform2.localScale.z) : new Vector3(transform2.localScale.x, transform2.localScale.y, c.bindMeshScaleZ[num6] * value3));
				}
			}
		}
		Transform transform3 = c.joints[num];
		if ((bool)transform3)
		{
			transform3.position = array6[num];
			if (showDebugCrosses)
			{
				DrawDebugCross(array6[num], debugCrossSize, Color.green);
			}
			Vector3 forward3 = target - transform3.position;
			if (forward3.sqrMagnitude > 1.0000001E-06f)
			{
				Vector3 upRef3 = (mainActorTransform ? mainActorTransform.up : Vector3.up);
				Vector3 usedUp2;
				Quaternion quaternion4 = LookRotationNoRoll(forward3, upRef3, c.lastUp[num], out usedUp2);
				Quaternion rotation = Quaternion.LookRotation(c.bindFwdLocal[num], c.bindUpLocal[num]);
				transform3.rotation = quaternion4 * Quaternion.Inverse(rotation);
				c.lastUp[num] = usedUp2;
				if (boneLookup.TryGetValue(transform3, out var value5))
				{
					value5.spring.lastRotation = transform3.rotation;
					value5.spring.springVelocity = Vector3.zero;
				}
			}
			if (c.meshes != null && num < c.meshes.Length && (bool)c.meshes[num])
			{
				Transform transform4 = c.meshes[num];
				float magnitude2 = (((bool)c.grabPoint && c.grabPoint != c.joints[num] && c.bindTipOffset.sqrMagnitude > 0.001f) ? transform3.TransformVector(c.bindTipOffset) : (transform3.forward * ((num > 0) ? (c.lenBind[num - 1] * 0.5f) : 1f))).magnitude;
				float num7 = Vector3.Distance(transform3.position, target);
				float value6 = ((magnitude2 > 0.001f) ? (num7 / magnitude2) : 1f);
				value6 = Mathf.Clamp(value6, 0.1f, maxStretch);
				transform4.localScale = (stretchAlongY ? new Vector3(transform4.localScale.x, c.bindMeshScaleY[num] * value6, transform4.localScale.z) : new Vector3(transform4.localScale.x, transform4.localScale.y, c.bindMeshScaleZ[num] * value6));
			}
		}
		if (flag)
		{
			c.grabPoint.position = target;
			if (showDebugCrosses)
			{
				DrawDebugCross(c.grabPoint.position, debugCrossSize, Color.cyan);
			}
		}
		if (showDebugCrosses)
		{
			DrawDebugCross(target, debugCrossSize * 1.5f, Color.red);
		}
	}

	private void DrawDebugCross(Vector3 position, float size, Color color)
	{
		Debug.DrawLine(position + Vector3.right * size, position - Vector3.right * size, color);
		Debug.DrawLine(position + Vector3.up * size, position - Vector3.up * size, color);
		Debug.DrawLine(position + Vector3.forward * size, position - Vector3.forward * size, color);
	}

	private void CollectMainChain(Transform root, List<Transform> results)
	{
		results.Clear();
		if (!(root == null) && root.name.Contains("Main"))
		{
			DfsAllMainBones(root, results);
		}
	}

	private void DfsAllMainBones(Transform t, List<Transform> results)
	{
		if (t == null || !t.name.Contains("Main"))
		{
			return;
		}
		results.Add(t);
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < t.childCount; i++)
		{
			Transform child = t.GetChild(i);
			if (child.name.Contains("Main"))
			{
				list.Add(child);
			}
		}
		foreach (Transform item in list)
		{
			DfsAllMainBones(item, results);
		}
	}

	private Transform FindGrabPointRecursive(Transform parent)
	{
		if (!parent)
		{
			return null;
		}
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (child.name.Contains("Tip") || child.name.Contains("End") || child.name.Contains("Grab") || child.name.Contains("Target"))
			{
				return child;
			}
		}
		for (int j = 0; j < parent.childCount; j++)
		{
			Transform child2 = parent.GetChild(j);
			Transform transform = FindGrabPointRecursive(child2);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public void BuildLimbChains()
	{
		limbChains.Clear();
		foreach (Limb limb in limbs)
		{
			if (limb == null || limb.rootTransform == null)
			{
				continue;
			}
			Transform parent = limb.rootTransform.parent;
			if (parent != null && (!limb.rootTransform.parent || !IsLimbRootName(limb.rootTransform.parent.name)))
			{
				string n = "Limb Root - " + limb.name;
				Transform transform = parent.Find(n);
				Transform transform2;
				if ((bool)transform)
				{
					transform2 = transform;
				}
				else
				{
					transform2 = new GameObject(n).transform;
					transform2.SetParent(parent, worldPositionStays: false);
					transform2.localPosition = limb.rootTransform.localPosition;
					transform2.localRotation = Quaternion.identity;
					transform2.localScale = Vector3.one;
				}
				limb.rootTransform.SetParent(transform2, worldPositionStays: true);
			}
			limb.bones = new List<BoneSpring>();
			List<Transform> list = new List<Transform>();
			CollectMainChain(limb.rootTransform, list);
			foreach (Transform boneTransform in list)
			{
				BoneSpring boneSpring = bones.Find((BoneSpring boneSpring3) => boneSpring3?.transform == boneTransform);
				if (boneSpring != null)
				{
					limb.bones.Add(boneSpring);
					continue;
				}
				BoneSpring boneSpring2 = new BoneSpring();
				boneSpring2.name = boneTransform.name;
				boneSpring2.transform = boneTransform;
				boneSpring2.spring = new SpringQuaternion();
				boneSpring2.originalLocal = boneTransform.localRotation;
				boneSpring2.targetLocal = boneSpring2.originalLocal;
				limb.bones.Add(boneSpring2);
				bones.Add(boneSpring2);
				boneLookup[boneTransform] = boneSpring2;
			}
			foreach (BoneSpring bone in limb.bones)
			{
				if (bone?.transform == null)
				{
					continue;
				}
				Transform mesh = null;
				for (int num = 0; num < bone.transform.childCount; num++)
				{
					Transform child = bone.transform.GetChild(num);
					if (child.name == "Mesh" || child.name.Contains("Mesh"))
					{
						mesh = child;
						break;
					}
				}
				bone.mesh = mesh;
			}
			if (limb.bones.Count < 2)
			{
				continue;
			}
			LimbChain limbChain = new LimbChain();
			limbChain.root = limb.bones[0].transform;
			List<Transform> list2 = new List<Transform>();
			List<Transform> list3 = new List<Transform>();
			foreach (BoneSpring bone2 in limb.bones)
			{
				if ((bool)bone2?.transform)
				{
					list2.Add(bone2.transform);
					list3.Add(bone2?.mesh);
				}
			}
			limbChain.joints = list2.ToArray();
			limbChain.meshes = list3.ToArray();
			Transform transform3 = limbChain.joints[limbChain.joints.Length - 1];
			Transform grabPoint = transform3;
			if ((bool)transform3)
			{
				Transform transform4 = FindGrabPointRecursive(transform3);
				if (transform4 == null && transform3.childCount > 0 && limbChain.joints.Length >= 2)
				{
					Vector3 normalized = (transform3.position - limbChain.joints[limbChain.joints.Length - 2].position).normalized;
					float num2 = 0f;
					Transform[] componentsInChildren = transform3.GetComponentsInChildren<Transform>();
					foreach (Transform transform5 in componentsInChildren)
					{
						if (!(transform5 == transform3))
						{
							float num4 = Vector3.Dot(transform5.position - transform3.position, normalized);
							if (num4 > num2)
							{
								num2 = num4;
								transform4 = transform5;
							}
						}
					}
				}
				if (transform4 != null)
				{
					grabPoint = transform4;
				}
			}
			limbChain.grabPoint = grabPoint;
			CaptureBind(limbChain);
			limbChains[limb] = limbChain;
		}
	}

	public void SolveLimbTo(Limb limb, Vector3 target)
	{
		if (limb != null && limbChains.TryGetValue(limb, out var value))
		{
			SolveTo(value, target, limb.straightenDistance, limb.fullyPosedDistance);
		}
	}

	public void StretchLimbToPoint(string limbName, Vector3 target)
	{
		Limb limbByName = GetLimbByName(limbName);
		if (limbByName == null)
		{
			Debug.LogWarning("[StretchLimbToPoint] Limb '" + limbName + "' not found. Available limbs: " + string.Join(", ", limbs.ConvertAll((Limb l) => l?.name ?? "null")));
		}
		else if (!limbChains.ContainsKey(limbByName))
		{
			Debug.LogWarning("[StretchLimbToPoint] Limb '" + limbName + "' has no chain built. Call BuildLimbChains first.");
		}
		else
		{
			limbStretchTargets[limbByName] = target;
		}
	}

	public void ClearLimbStretch(string limbName)
	{
		Limb limbByName = GetLimbByName(limbName);
		if (limbByName != null)
		{
			limbStretchTargets.Remove(limbByName);
		}
	}

	public void ClearAllLimbStretches()
	{
		limbStretchTargets.Clear();
	}

	public void SetRandomForceOnAllBones(float minForce, float maxForce)
	{
		foreach (BoneSpring bone in bones)
		{
			if (bone?.spring != null)
			{
				Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
				float num = UnityEngine.Random.Range(minForce, maxForce);
				bone.spring.springVelocity = onUnitSphere * num;
			}
		}
	}

	public void SetTempSpeedAndDampingOnAllBones(float speed, float damping, float duration = 0.1f)
	{
		foreach (BoneSpring bone in bones)
		{
			if (bone?.spring != null)
			{
				if (!tempSpringOriginalStates.ContainsKey(bone))
				{
					tempSpringOriginalStates[bone] = (bone.spring.speed, bone.spring.damping);
				}
				bonesWithTempOverride.Add(bone);
				bone.spring.speed = speed;
				bone.spring.damping = damping;
			}
		}
		tempSpringOverrideTimer = duration;
	}

	private void SolveLimbIdleAndStretch(Limb limb)
	{
		if (limb == null || !limbChains.TryGetValue(limb, out var value) || value == null || !value.isInitialized || value.joints == null)
		{
			return;
		}
		int num = value.joints.Length - 1;
		if (num < 1)
		{
			return;
		}
		if (limbStretchTargets.TryGetValue(limb, out var value2))
		{
			SolveTo(value, value2, limb.straightenDistance, limb.fullyPosedDistance);
			return;
		}
		float num2 = (currentPose ? currentPose.springSpeedMultiplier : 1f);
		float num3 = (currentPose ? currentPose.springDampingMultiplier : 1f);
		float num4 = Mathf.Max(1f, defaultSpringSpeed * num2);
		float damping = Mathf.Max(0f, defaultSpringDamping * num3);
		Transform parent = value.root.parent;
		if (parent != null && IsLimbRootName(parent.name))
		{
			parent.localRotation = Quaternion.Slerp(parent.localRotation, value.restLimbRootLocalRotation, 1f - Mathf.Exp((0f - num4) * Time.deltaTime));
		}
		for (int i = 0; i < num; i++)
		{
			Transform transform = value.joints[i];
			if ((bool)transform && boneLookup.TryGetValue(transform, out var value3))
			{
				if (!value3.overrideDefaultSpringValues && !bonesWithTempOverride.Contains(value3))
				{
					value3.spring.speed = num4;
					value3.spring.damping = damping;
				}
				Quaternion quaternion = (transform.parent ? transform.parent.rotation : Quaternion.identity) * value3.targetLocal;
				if (Quaternion.Dot(transform.rotation, quaternion) > 0.999f)
				{
					transform.rotation = quaternion;
					value3.spring.lastRotation = quaternion;
					value3.spring.springVelocity = Vector3.zero;
				}
				else
				{
					transform.rotation = SemiFunc.SpringQuaternionGet(value3.spring, quaternion);
				}
				if (value.lastUp != null && i < value.lastUp.Length && (bool)value.joints[i])
				{
					value.lastUp[i] = value.joints[i].up;
				}
			}
		}
		if (boneLookup.TryGetValue(value.joints[num], out var value4))
		{
			if (!value4.overrideDefaultSpringValues && !bonesWithTempOverride.Contains(value4))
			{
				value4.spring.speed = num4;
				value4.spring.damping = damping;
			}
			Quaternion quaternion2 = (value.joints[num].parent ? value.joints[num].parent.rotation : Quaternion.identity) * value4.targetLocal;
			if (Quaternion.Dot(value.joints[num].rotation, quaternion2) > 0.999f)
			{
				value.joints[num].rotation = quaternion2;
				value4.spring.lastRotation = quaternion2;
				value4.spring.springVelocity = Vector3.zero;
			}
			else
			{
				value.joints[num].rotation = SemiFunc.SpringQuaternionGet(value4.spring, quaternion2);
			}
			if (value.lastUp != null && num < value.lastUp.Length && (bool)value.joints[num])
			{
				value.lastUp[num] = value.joints[num].up;
			}
		}
		Vector3[] array = new Vector3[num + 1];
		for (int j = 0; j <= num; j++)
		{
			array[j] = value.joints[j].position;
		}
		for (int k = 0; k < num; k++)
		{
			float num5 = value.lenBind[k];
			Vector3 vector = value.joints[k].rotation * value.bindFwdLocal[k];
			array[k + 1] = array[k] + vector * num5;
		}
		for (int l = 0; l <= num; l++)
		{
			value.joints[l].position = array[l];
		}
		for (int m = 0; m <= num; m++)
		{
			if (value.meshes != null && m < value.meshes.Length && (bool)value.meshes[m])
			{
				Transform transform2 = value.meshes[m];
				if (stretchAlongY)
				{
					transform2.localScale = new Vector3(transform2.localScale.x, value.bindMeshScaleY[m], transform2.localScale.z);
				}
				else
				{
					transform2.localScale = new Vector3(transform2.localScale.x, transform2.localScale.y, value.bindMeshScaleZ[m]);
				}
			}
		}
		if ((bool)value.grabPoint && value.grabPoint != value.joints[num])
		{
			value.grabPoint.position = value.joints[num].position + value.joints[num].rotation * value.bindTipOffset;
		}
	}

	private void RestoreLimbIdlePose(Limb limb)
	{
		if (limb != null && limbChains.TryGetValue(limb, out var value) && value != null && value.isInitialized && value.joints != null && value.joints.Length - 1 >= 1)
		{
			SolveLimbIdleAndStretch(limb);
		}
	}

	public void CullingOverride(float time)
	{
		cullingOverrideTimer = time;
	}
}
