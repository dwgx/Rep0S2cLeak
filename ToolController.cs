using System;
using System.Collections.Generic;
using UnityEngine;

public class ToolController : MonoBehaviour
{
	[Serializable]
	public class Tool
	{
		public string Name;

		public Interaction.InteractionType InteractionType;

		[Space]
		public GameObject Object;

		public GameObject ObjectParent;

		public GameObject playerAvatarPrefab;

		[Space]
		public Vector3 HidePosition;

		public Vector3 HideRotation;

		public float HideSpeed = 2f;

		[Space]
		public Vector3 OffsetPosition;

		public Vector3 OffsetRotation;

		[Space]
		public Sprite Icon;

		public bool HeadBob = true;

		public float Range = 1f;
	}

	public bool DebugAlwaysInteract;

	[HideInInspector]
	public static ToolController instance;

	[HideInInspector]
	public bool Active;

	private float ActiveTime = 0.25f;

	private float ActiveTimer;

	[HideInInspector]
	public bool Interact;

	private float InteractTimer;

	[Space]
	public float InteractionRange = 4f;

	public float InteractionCheckTime = 0.1f;

	private float InteractionCheckTimer;

	private float RangeCheckTimer;

	private bool RangeCheck = true;

	[HideInInspector]
	public float ForceActiveTimer;

	[HideInInspector]
	public Interaction.InteractionType ActiveInteractionType;

	[HideInInspector]
	public Interaction.InteractionType CurrentInteractionType;

	[HideInInspector]
	public Interaction ActiveInteraction;

	[HideInInspector]
	public Interaction CurrentInteraction;

	[HideInInspector]
	public Vector3 CurrentHidePosition;

	[HideInInspector]
	public Vector3 CurrentHideRotation;

	[HideInInspector]
	public float CurrentHideSpeed;

	[HideInInspector]
	public Sprite CurrentSprite;

	private float CurrentRange;

	private GameObject CurrentObject;

	[HideInInspector]
	public Interaction.InteractionType PreviousInteractionType;

	[Space]
	public ToolFollowPush ToolFollowPush;

	public ToolHide ToolHide;

	public Transform ToolFollow;

	public Transform ToolOffset;

	public ToolFollow ToolHeadbob;

	public Transform ToolTargetParent;

	public Transform FollowTargetTransform;

	private LayerMask Mask;

	private LayerMask VisibilityMask;

	public List<Tool> Tools;

	private Camera MainCamera;

	private bool InteractInput;

	private bool InteractInputDelayed;

	private bool DirtFinderInput;

	private float DisableTimer;

	public PlayerAvatar playerAvatarScript;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		MainCamera = Camera.main;
		Mask = LayerMask.GetMask("Interaction");
		VisibilityMask = LayerMask.GetMask("Default");
	}

	private void Update()
	{
		UpdateInput();
		InteractionCheck();
		UpdateDirtFinder();
		UpdateActive();
		UpdateInteract();
		ToolFollow.position = Vector3.Lerp(ToolFollow.position, FollowTargetTransform.position, 20f * Time.deltaTime);
		ToolFollow.rotation = Quaternion.Lerp(ToolFollow.rotation, FollowTargetTransform.rotation, 20f * Time.deltaTime);
	}

	public void Disable(float time)
	{
		DisableTimer = time;
	}

	private void UpdateInput()
	{
		if (GameDirector.instance.currentState != GameDirector.gameState.Main || PlayerAvatar.instance.isDisabled)
		{
			return;
		}
		if (SemiFunc.InputDown(InputKey.Interact) || InteractInputDelayed)
		{
			if (ActiveInteractionType != Interaction.InteractionType.None && CurrentInteractionType != Interaction.InteractionType.None && CurrentInteractionType != ActiveInteractionType)
			{
				InteractInputDelayed = true;
				InteractInput = false;
			}
			else
			{
				InteractInput = true;
			}
		}
		else
		{
			InteractInput = false;
		}
		if (PlayerController.instance.CanInteract && (Input.GetButton("Dirt Finder") || Input.GetAxis("Dirt Finder") == 1f || GameDirector.instance.LevelCompleted))
		{
			DirtFinderInput = true;
		}
		else
		{
			DirtFinderInput = false;
		}
		if (DisableTimer > 0f)
		{
			DisableTimer -= 1f * Time.deltaTime;
			ActiveTimer = 0f;
			InteractInputDelayed = false;
			InteractInput = false;
			DirtFinderInput = false;
		}
	}

	private void InteractionCheck()
	{
		if (InteractionCheckTimer <= 0f)
		{
			InteractionCheckTimer = InteractionCheckTime;
			CurrentInteractionType = Interaction.InteractionType.None;
			if (!PlayerController.instance.CanInteract)
			{
				return;
			}
			RaycastHit[] array = Physics.BoxCastAll(MainCamera.transform.position, new Vector3(0.01f, 0.01f, 0.01f), MainCamera.transform.forward, MainCamera.transform.rotation, InteractionRange, Mask);
			if (array.Length == 0)
			{
				return;
			}
			RaycastHit hitInfo;
			bool flag = Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, out hitInfo, InteractionRange, VisibilityMask);
			bool flag2 = false;
			Interaction hitPicked = null;
			float num = 360f;
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				if (flag && raycastHit.distance > hitInfo.distance)
				{
					continue;
				}
				Interaction interaction = raycastHit.transform.GetComponent<Interaction>();
				if (interaction == null)
				{
					continue;
				}
				float range = Tools.Find((Tool x) => x.InteractionType == interaction.Type).Range;
				if (raycastHit.distance <= range)
				{
					float num2 = Quaternion.Angle(Quaternion.LookRotation(raycastHit.transform.position - MainCamera.transform.position), MainCamera.transform.rotation);
					if (num2 < num)
					{
						num = num2;
						hitPicked = interaction;
						flag2 = true;
					}
				}
			}
			if (flag2)
			{
				CurrentInteraction = hitPicked;
				CurrentInteractionType = hitPicked.Type;
				if (CurrentInteractionType == ActiveInteractionType)
				{
					ActiveInteraction = CurrentInteraction;
				}
				CurrentSprite = Tools.Find((Tool x) => x.InteractionType == CurrentInteractionType).Icon;
				CurrentRange = Tools.Find((Tool x) => x.InteractionType == hitPicked.Type).Range;
			}
		}
		else
		{
			InteractionCheckTimer -= 1f * Time.deltaTime;
		}
	}

	private void UpdateDirtFinder()
	{
		if (GameDirector.instance.LevelCompletedDone || (DirtFinderInput && ForceActiveTimer <= 0f))
		{
			CurrentInteractionType = Interaction.InteractionType.DirtFinder;
			CurrentSprite = Tools.Find((Tool x) => x.InteractionType == CurrentInteractionType).Icon;
			if (ActiveInteractionType != Interaction.InteractionType.DirtFinder && ActiveInteractionType != Interaction.InteractionType.None)
			{
				DeactivateTool();
			}
			else if (!Active && CurrentObject == null)
			{
				ActivateTool();
			}
			if (ActiveInteractionType == Interaction.InteractionType.DirtFinder)
			{
				ActiveTimer = ActiveTime;
			}
		}
	}

	private void UpdateActive()
	{
		if (ForceActiveTimer > 0f)
		{
			ForceActiveTimer -= 1f * Time.deltaTime;
			ForceActiveTimer = Mathf.Max(ForceActiveTimer, 0f);
		}
		if (GameDirector.instance.currentState == GameDirector.gameState.Main)
		{
			if (ActiveInteractionType != Interaction.InteractionType.DirtFinder && (CurrentInteractionType != Interaction.InteractionType.None || ActiveInteractionType != Interaction.InteractionType.None) && (InteractInput || ForceActiveTimer > 0f))
			{
				ActiveTimer = ActiveTime;
				if (!Active && CurrentObject == null)
				{
					ActivateTool();
				}
			}
			if (Active && ActiveTimer <= 0f)
			{
				DeactivateTool();
			}
			if (Active)
			{
				if (CurrentInteraction == null || ActiveInteractionType == Interaction.InteractionType.DirtFinder)
				{
					ActiveTimer -= 1f * Time.deltaTime;
				}
				else if (!Interact)
				{
					if (RangeCheckTimer <= 0f)
					{
						RangeCheck = false;
						Vector3 direction = CurrentInteraction.transform.position - MainCamera.transform.position;
						RaycastHit[] array = Physics.BoxCastAll(MainCamera.transform.position, new Vector3(0.01f, 0.01f, 0.01f), direction, Quaternion.identity, InteractionRange, Mask);
						if (array.Length != 0)
						{
							RaycastHit[] array2 = array;
							for (int i = 0; i < array2.Length; i++)
							{
								RaycastHit raycastHit = array2[i];
								if (raycastHit.transform.GetComponent<Interaction>() == ActiveInteraction && raycastHit.distance <= CurrentRange)
								{
									RangeCheck = true;
									break;
								}
							}
						}
						RangeCheckTimer = 0.2f;
					}
					else
					{
						RangeCheckTimer -= 1f * Time.deltaTime;
					}
					if (!RangeCheck)
					{
						ActiveTimer -= 1f * Time.deltaTime;
					}
				}
			}
		}
		if (ActiveInteractionType != Interaction.InteractionType.None)
		{
			PlayerController.instance.CrouchDisable(0.5f);
		}
	}

	private void UpdateInteract()
	{
		if (ActiveInteractionType == Interaction.InteractionType.DirtFinder)
		{
			Interact = false;
			return;
		}
		if (CurrentInteractionType == ActiveInteractionType && (InteractInput || DebugAlwaysInteract))
		{
			InteractTimer = 0.25f;
			InteractInputDelayed = false;
		}
		if (InteractTimer > 0f)
		{
			Interact = true;
			InteractTimer -= 1f * Time.deltaTime;
			if (InteractTimer <= 0f)
			{
				Interact = false;
			}
		}
	}

	private void ActivateTool()
	{
		ActiveInteractionType = CurrentInteractionType;
		ActiveInteraction = CurrentInteraction;
		Active = true;
		foreach (Tool tool in Tools)
		{
			if (tool.InteractionType == CurrentInteractionType)
			{
				ToolOffset.transform.localPosition = tool.OffsetPosition;
				ToolOffset.transform.localRotation = Quaternion.Euler(tool.OffsetRotation);
				if (tool.HeadBob)
				{
					ToolHeadbob.Activate();
				}
				else
				{
					ToolHeadbob.Deactivate();
				}
				CurrentHidePosition = tool.HidePosition;
				CurrentHideRotation = tool.HideRotation;
				CurrentHideSpeed = tool.HideSpeed;
				break;
			}
		}
		ToolHide.Show();
	}

	public void ShowTool()
	{
		foreach (Tool tool in Tools)
		{
			if (tool.InteractionType == ActiveInteractionType)
			{
				CurrentObject = UnityEngine.Object.Instantiate(tool.Object, tool.ObjectParent.transform);
				break;
			}
		}
	}

	private void DeactivateTool()
	{
		Active = false;
		ActiveInteractionType = Interaction.InteractionType.None;
		ActiveInteraction = null;
		ToolHide.Hide();
	}

	public void HideTool()
	{
		ToolOffset.transform.localPosition = Vector3.zero;
		ToolOffset.transform.localRotation = Quaternion.identity;
		UnityEngine.Object.Destroy(CurrentObject);
	}
}
