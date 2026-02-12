using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TutorialDirector : MonoBehaviour
{
	[Serializable]
	public class TutorialPage
	{
		public string pageName = "";

		[Space(10f)]
		public VideoClip video;

		public string text;

		public string focusText;

		[TextArea(3, 10)]
		public string dummyText;
	}

	public static TutorialDirector instance;

	public List<TutorialPage> tutorialPages = new List<TutorialPage>();

	internal int currentPage = -1;

	internal bool tutorialActive;

	private float tutorialCheckActiveTimer;

	private float tutorialActiveTimer;

	internal float tutorialProgress;

	private PhysGrabCart tutorialCart;

	private ExtractionPoint tutorialExtractionPoint;

	internal bool deadPlayer;

	private float arrowDelay;

	internal bool playerSprinted;

	internal bool playerJumped;

	internal bool playerSawHead;

	internal bool playerRevived;

	internal bool playerHealed;

	internal bool playerRotated;

	internal bool playerTumbled;

	internal bool playerCrouched;

	internal bool playerCrawled;

	internal bool playerUsedCart;

	internal bool playerPushedAndPulled;

	internal bool playerUsedToggle;

	internal bool playerHadItemsAndUsedInventory;

	internal bool playerUsedMap;

	internal bool playerUsedChargingStation;

	internal bool playerReviveTipDone;

	internal bool playerChatted;

	internal bool playerUsedExpression;

	internal int numberOfRoundsWithoutChatting;

	internal int numberOfRoundsWithoutCharging;

	internal int numberOfRoundsWithoutMap;

	internal int numberOfRoundsWithoutInventory;

	internal int numberOfRoundsWithoutCart;

	internal int numberOfRoundsWithoutToggle;

	internal List<string> potentialTips = new List<string>();

	internal List<string> shownTips = new List<string>();

	private float showTipTimer;

	private float showTipTime;

	private float delayBeforeTip;

	private bool scaleDownTip;

	private string scheduleTipName;

	private float scheduleTipTimer;

	private float scheduleTipShowTimer;

	private bool scheduleTipScaleDown;

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		currentPage = -1;
	}

	private void FixedUpdate()
	{
		if (!SemiFunc.RunIsArena() && !SemiFunc.RunIsLobbyMenu() && !SemiFunc.MenuLevel())
		{
			if (tutorialActiveTimer > 0f)
			{
				tutorialActiveTimer -= Time.fixedDeltaTime;
				tutorialActive = true;
			}
			else
			{
				tutorialActive = false;
			}
			TipBoolChecks();
		}
	}

	private void Update()
	{
		if (scheduleTipTimer > 0f)
		{
			scheduleTipTimer -= Time.deltaTime;
			if (scheduleTipTimer <= 0f)
			{
				ActivateTip(scheduleTipName, 0f, _interrupt: true, scheduleTipShowTimer, scheduleTipScaleDown);
				scheduleTipTimer = 0f;
			}
		}
		if (SemiFunc.RunIsArena() || SemiFunc.RunIsLobbyMenu() || SemiFunc.MenuLevel())
		{
			TutorialUI.instance.Hide();
			return;
		}
		if (!tutorialActive)
		{
			TutorialUI.instance.Hide();
			tutorialCheckActiveTimer -= Time.deltaTime;
			if (tutorialCheckActiveTimer < 0f)
			{
				tutorialCheckActiveTimer = 0.5f;
				if (SemiFunc.IsCurrentLevel(LevelGenerator.Instance.Level, RunManager.instance.levelTutorial))
				{
					tutorialActive = true;
					TutorialActive();
				}
			}
			TutorialUI.instance.Hide();
			TipsTick();
			return;
		}
		TutorialActive();
		if (tutorialActive)
		{
			if (currentPage == -1)
			{
				NextPage();
			}
			SemiFunc.UIFocusText(tutorialPages[currentPage].focusText, Color.white, AssetManager.instance.colorYellow, 0.2f);
			if (currentPage < 6)
			{
				HealthUI.instance.Hide();
			}
			if (currentPage < 14)
			{
				HaulUI.instance.Hide();
				CurrencyUI.instance.Hide();
				GoalUI.instance.Hide();
			}
			if (currentPage < 4)
			{
				EnergyUI.instance.Hide();
			}
			if (currentPage < 10)
			{
				InventoryUI.instance.Hide();
			}
			if (currentPage == 0)
			{
				TaskMove();
			}
			if (currentPage == 1)
			{
				TaskJump();
			}
			if (currentPage == 2)
			{
				TaskSneak();
			}
			if (currentPage == 3)
			{
				TaskSneakUnder();
			}
			if (currentPage == 4)
			{
				TaskSprint();
			}
			if (currentPage == 5)
			{
				TaskTumble();
			}
			if (currentPage == 6)
			{
				TaskGrab();
			}
			if (currentPage == 7)
			{
				TaskPushAndPull();
			}
			if (currentPage == 8)
			{
				TaskRotate();
			}
			if (currentPage == 9)
			{
				TaskInteract();
			}
			if (currentPage == 10)
			{
				TaskInventoryFill();
			}
			if (currentPage == 11)
			{
				TaskInventoryEmpty();
			}
			if (currentPage == 12)
			{
				TaskMap();
			}
			if (currentPage == 13)
			{
				TaskCartMove();
			}
			if (currentPage == 14)
			{
				TaskCartFill();
			}
			if (currentPage == 15)
			{
				TaskExtractionPoint();
			}
			if (currentPage == 16)
			{
				TaskEnterTuck();
			}
			if (arrowDelay > 0f)
			{
				arrowDelay -= Time.deltaTime;
			}
			if (TutorialUI.instance.progressBarCurrent > 0.98f && tutorialProgress > 0.98f)
			{
				NextPage();
				tutorialProgress = 0f;
				TutorialUI.instance.animationCurveEval = 0f;
				TutorialUI.instance.progressBar.localScale = new Vector3(0f, 1f, 1f);
				TutorialUI.instance.progressBarCurrent = 0f;
			}
		}
	}

	public void SetPageID(string pageName)
	{
		for (int i = 0; i < tutorialPages.Count; i++)
		{
			if (tutorialPages[i].pageName == pageName)
			{
				currentPage = i;
				break;
			}
		}
	}

	public void NextPage()
	{
		currentPage++;
		if (currentPage > tutorialPages.Count - 1)
		{
			currentPage = tutorialPages.Count - 1;
		}
		int num = currentPage;
		string text = tutorialPages[num].text;
		string dummyText = tutorialPages[num].dummyText;
		dummyText = InputManager.instance.InputDisplayReplaceTags(dummyText);
		VideoClip video = tutorialPages[num].video;
		text = InputManager.instance.InputDisplayReplaceTags(text);
		if (num == 0)
		{
			TutorialUI.instance.SetPage(video, text, dummyText, transition: false);
		}
		else
		{
			TutorialUI.instance.SetPage(video, text, dummyText);
		}
		arrowDelay = 4f;
	}

	private void TipsClear()
	{
		potentialTips.Clear();
	}

	public void TipsStore()
	{
		if (!playerJumped && TutorialSettingCheck(DataDirector.Setting.TutorialJumping, 3))
		{
			potentialTips.Add("Jumping");
		}
		if (!playerSprinted && TutorialSettingCheck(DataDirector.Setting.TutorialSprinting, 3))
		{
			potentialTips.Add("Sprinting");
		}
		if (!playerCrouched && TutorialSettingCheck(DataDirector.Setting.TutorialSneaking, 3))
		{
			potentialTips.Add("Sneaking");
		}
		if (!playerCrawled && TutorialSettingCheck(DataDirector.Setting.TutorialHiding, 3))
		{
			potentialTips.Add("Hiding");
		}
		if (!playerTumbled && TutorialSettingCheck(DataDirector.Setting.TutorialTumbling, 3))
		{
			potentialTips.Add("Tumbling");
		}
		if (!playerPushedAndPulled && TutorialSettingCheck(DataDirector.Setting.TutorialPushingAndPulling, 3))
		{
			potentialTips.Add("Pushing and Pulling");
		}
		if (!playerRotated && TutorialSettingCheck(DataDirector.Setting.TutorialRotating, 3))
		{
			potentialTips.Add("Rotating");
		}
		if (SemiFunc.IsMultiplayer())
		{
			if (playerSawHead && !playerRevived && TutorialSettingCheck(DataDirector.Setting.TutorialReviving, 3))
			{
				playerReviveTipDone = true;
				potentialTips.Add("Reviving");
			}
			if (!playerHealed && TutorialSettingCheck(DataDirector.Setting.TutorialHealing, 3))
			{
				bool flag = true;
				bool flag2 = false;
				foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
				{
					if (item.isLocal)
					{
						if (item.playerHealth.health > 50)
						{
							flag2 = true;
						}
					}
					else if (item.playerHealth.health < 50)
					{
						flag = false;
					}
				}
				if (flag2 && !flag)
				{
					potentialTips.Add("Healing");
				}
			}
			if (!playerChatted && numberOfRoundsWithoutChatting > 2 && TutorialSettingCheck(DataDirector.Setting.TutorialChat, 3))
			{
				potentialTips.Add("Chat");
			}
		}
		if (!playerUsedCart && numberOfRoundsWithoutCart > 2 && TutorialSettingCheck(DataDirector.Setting.TutorialCartHandling, 3))
		{
			potentialTips.Add("Cart Handling 2");
		}
		if (!playerUsedToggle && numberOfRoundsWithoutCart > 5 && TutorialSettingCheck(DataDirector.Setting.TutorialItemToggling, 3))
		{
			potentialTips.Add("Item Toggling");
		}
		if (!playerHadItemsAndUsedInventory && numberOfRoundsWithoutInventory > 3 && TutorialSettingCheck(DataDirector.Setting.TutorialInventoryFill, 3))
		{
			potentialTips.Add("Inventory Fill");
		}
		if (!playerUsedMap && numberOfRoundsWithoutMap > 1 && TutorialSettingCheck(DataDirector.Setting.TutorialMap, 3))
		{
			potentialTips.Add("Map");
		}
		if (!playerUsedChargingStation && numberOfRoundsWithoutCharging > 5 && TutorialSettingCheck(DataDirector.Setting.TutorialChargingStation, 3))
		{
			potentialTips.Add("Charging Station");
		}
		if (!playerUsedExpression && TutorialSettingCheck(DataDirector.Setting.TutorialExpressions, 3))
		{
			SemiLogger.LogAxel("expression check");
			potentialTips.Add("Expressions");
		}
	}

	public void TipsShow()
	{
		if (SemiFunc.RunIsArena() || SemiFunc.RunIsLobbyMenu() || SemiFunc.MenuLevel())
		{
			return;
		}
		for (int i = 0; i < potentialTips.Count; i++)
		{
			for (int j = 0; j < shownTips.Count; j++)
			{
				if (potentialTips[i] == shownTips[j])
				{
					potentialTips.RemoveAt(i);
					i--;
					break;
				}
			}
		}
		if (potentialTips.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, potentialTips.Count);
			ActivateTip(potentialTips[index], 4f, _interrupt: true);
		}
		TipsClear();
	}

	public void ActivateTip(string tipName, float _delay, bool _interrupt, float _overrideShowTime = -1f, bool _scaleDown = true)
	{
		if (GameplayManager.instance.tips && (_interrupt || (!(delayBeforeTip > 0f) && !(showTipTimer > 0f))))
		{
			TutorialUI.instance.SemiUISpringShakeY(5f, 5f, 0.3f);
			TutorialSettingSet(tipName);
			shownTips.Add(tipName);
			scaleDownTip = _scaleDown;
			SetPageID(tipName);
			SetTipPageUI();
			delayBeforeTip = _delay;
			float num = 12f;
			if (_overrideShowTime > 0f)
			{
				num = _overrideShowTime;
			}
			showTipTime = num;
			showTipTimer = num;
		}
	}

	public void ScheduleTip(string tipName, float _timer, bool _interrupt, float _overrideShowTime = -1f, bool _scaleDown = true)
	{
		if (GameplayManager.instance.tips && (scheduleTipTimer <= 0f || _interrupt))
		{
			scheduleTipName = tipName;
			scheduleTipTimer = _timer;
			scheduleTipScaleDown = _scaleDown;
			scheduleTipShowTimer = 12f;
			if (_overrideShowTime > 0f)
			{
				scheduleTipShowTimer = _overrideShowTime;
			}
		}
	}

	private void SetTipPageUI()
	{
		if (currentPage != -1)
		{
			int index = currentPage;
			string text = tutorialPages[index].text;
			VideoClip video = tutorialPages[index].video;
			text = InputManager.instance.InputDisplayReplaceTags(text);
			TutorialUI.instance.SetTipPage(video, text, scaleDownTip);
		}
	}

	private void TipBoolChecks()
	{
		if (SemiFunc.RunIsArena() || SemiFunc.RunIsLobbyMenu() || SemiFunc.MenuLevel() || tutorialActive || !LevelGenerator.Instance.Generated)
		{
			return;
		}
		if (PhysGrabber.instance.isRotating)
		{
			playerRotated = true;
		}
		if (Map.Instance.Active)
		{
			playerUsedMap = true;
		}
		if (SemiFunc.FPSImpulse1())
		{
			bool flag = (bool)Inventory.instance && Inventory.instance.inventorySpots != null && Inventory.instance.InventorySpotsOccupied() > 0;
			if (!SemiFunc.RunIsShop() && ItemManager.instance.purchasedItems.Count > 2 && flag)
			{
				playerHadItemsAndUsedInventory = true;
			}
		}
	}

	public void TipCancel()
	{
		showTipTimer = 0f;
		tutorialProgress = 0f;
	}

	private void TipsTick()
	{
		if (delayBeforeTip > 0f)
		{
			delayBeforeTip -= Time.deltaTime;
		}
		else if (showTipTimer > 0f)
		{
			showTipTimer -= Time.deltaTime;
			tutorialProgress = 1f - showTipTimer / showTipTime;
			TutorialUI.instance.Show();
		}
	}

	private void TutorialProgressFill(float amount)
	{
		tutorialProgress += amount * Time.deltaTime;
	}

	public void EndTutorial()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public bool TutorialSettingCheck(DataDirector.Setting _setting, int _max)
	{
		if (DataDirector.instance.SettingValueFetch(_setting) < _max)
		{
			return true;
		}
		return false;
	}

	private void TutorialSettingSet(string _tutorial)
	{
		DataDirector.Setting setting = DataDirector.Setting.TutorialJumping;
		switch (_tutorial)
		{
		case "Jumping":
			setting = DataDirector.Setting.TutorialJumping;
			break;
		case "Sprinting":
			setting = DataDirector.Setting.TutorialSprinting;
			break;
		case "Sneaking":
			setting = DataDirector.Setting.TutorialSneaking;
			break;
		case "Hiding":
			setting = DataDirector.Setting.TutorialHiding;
			break;
		case "Tumbling":
			setting = DataDirector.Setting.TutorialTumbling;
			break;
		case "Pushing and Pulling":
			setting = DataDirector.Setting.TutorialPushingAndPulling;
			break;
		case "Rotating":
			setting = DataDirector.Setting.TutorialRotating;
			break;
		case "Reviving":
			setting = DataDirector.Setting.TutorialReviving;
			break;
		case "Healing":
			setting = DataDirector.Setting.TutorialHealing;
			break;
		case "Cart Handling 2":
			setting = DataDirector.Setting.TutorialCartHandling;
			break;
		case "Item Toggling":
			setting = DataDirector.Setting.TutorialItemToggling;
			break;
		case "Inventory Fill":
			setting = DataDirector.Setting.TutorialInventoryFill;
			break;
		case "Map":
			setting = DataDirector.Setting.TutorialMap;
			break;
		case "Charging Station":
			setting = DataDirector.Setting.TutorialChargingStation;
			break;
		case "Only One Extraction":
			setting = DataDirector.Setting.TutorialOnlyOneExtraction;
			break;
		case "Chat":
			setting = DataDirector.Setting.TutorialChat;
			break;
		case "Final Extraction":
			setting = DataDirector.Setting.TutorialFinalExtraction;
			break;
		case "Multiple Extractions":
			setting = DataDirector.Setting.TutorialMultipleExtractions;
			break;
		case "Shop":
			setting = DataDirector.Setting.TutorialShop;
			break;
		case "Expressions":
			setting = DataDirector.Setting.TutorialExpressions;
			break;
		case "Overcharge1":
			setting = DataDirector.Setting.TutorialOvercharge1;
			break;
		case "HeadSpectate1":
			setting = DataDirector.Setting.TutorialHeadSpectate;
			break;
		case "EnemyElsa":
			setting = DataDirector.Setting.TutorialEnemyElsa;
			break;
		}
		int num = DataDirector.instance.SettingValueFetch(setting);
		DataDirector.instance.SettingValueSet(setting, num + 1);
		DataDirector.instance.SaveSettings();
	}

	public void Reset()
	{
		currentPage = -1;
	}

	public void UpdateRoundEnd()
	{
		if (!playerUsedCart)
		{
			numberOfRoundsWithoutCart++;
		}
		if (!playerUsedMap)
		{
			numberOfRoundsWithoutMap++;
		}
		if (!playerHadItemsAndUsedInventory)
		{
			numberOfRoundsWithoutInventory++;
		}
		if (!playerUsedToggle)
		{
			numberOfRoundsWithoutToggle++;
		}
		if (!playerUsedChargingStation)
		{
			numberOfRoundsWithoutCharging++;
		}
		if (!playerChatted)
		{
			numberOfRoundsWithoutChatting++;
		}
		if (!playerReviveTipDone)
		{
			playerSawHead = false;
			playerRevived = false;
		}
	}

	public void TutorialActive()
	{
		tutorialActiveTimer = 0.2f;
	}

	private void TaskMove()
	{
		Vector3 velocity = PlayerController.instance.rb.velocity;
		velocity.y = 0f;
		if (velocity.magnitude > 0.05f)
		{
			TutorialProgressFill(0.2f);
		}
	}

	private void TaskJump()
	{
		if ((bool)PlayerController.instance && PlayerController.instance.rb.velocity.y > 2f)
		{
			TutorialProgressFill(0.8f);
		}
	}

	private void TaskSneak()
	{
		Vector3 velocity = PlayerController.instance.rb.velocity;
		bool crouching = PlayerController.instance.Crouching;
		velocity.y = 0f;
		if (velocity.magnitude > 0.05f && crouching)
		{
			TutorialProgressFill(0.2f);
		}
	}

	private void TaskSneakUnder()
	{
		if (PlayerController.instance.Crawling)
		{
			TutorialProgressFill(0.2f);
		}
	}

	private void TaskSprint()
	{
		if (arrowDelay <= 0f)
		{
			SemiFunc.UIShowArrow(new Vector3(340f, 90f, 0f), new Vector3(70f, 320f, 0f), 145f);
		}
		bool sprinting = PlayerController.instance.sprinting;
		Vector3 velocity = PlayerController.instance.rb.velocity;
		velocity.y = 0f;
		if (velocity.magnitude > 2f && sprinting)
		{
			TutorialProgressFill(0.3f);
		}
	}

	private void TaskTumble()
	{
		if ((bool)PlayerAvatar.instance.tumble)
		{
			Vector3 velocity = PlayerAvatar.instance.tumble.rb.velocity;
			bool isTumbling = PlayerAvatar.instance.isTumbling;
			if (velocity.magnitude > 1f && isTumbling)
			{
				TutorialProgressFill(0.3f);
			}
			if (isTumbling)
			{
				TutorialProgressFill(0.025f);
			}
		}
	}

	private void TaskGrab()
	{
		if (PhysGrabber.instance.grabbed)
		{
			TutorialProgressFill(0.2f);
		}
	}

	private void TaskPushAndPull()
	{
		if (PhysGrabber.instance.isPushing || PhysGrabber.instance.isPulling)
		{
			TutorialProgressFill(0.6f);
		}
	}

	private void TaskRotate()
	{
		if (PhysGrabber.instance.isRotating)
		{
			TutorialProgressFill(0.2f);
		}
	}

	private void TaskInteract()
	{
		Transform grabbedObjectTransform = PhysGrabber.instance.grabbedObjectTransform;
		ItemToggle itemToggle = null;
		if ((bool)grabbedObjectTransform)
		{
			itemToggle = grabbedObjectTransform.GetComponent<ItemToggle>();
		}
		if ((bool)itemToggle && itemToggle.toggleImpulse)
		{
			TutorialProgressFill(0.8f);
		}
	}

	private void TaskInventoryFill()
	{
		if (arrowDelay <= 0f)
		{
			SemiFunc.UIShowArrow(new Vector3(340f, 340f, 0f), new Vector3(370f, 20f, 0f), 200f);
		}
		int num = 3;
		int num2 = Inventory.instance.InventorySpotsOccupied();
		tutorialProgress = (float)num2 / (float)num;
	}

	private void TaskInventoryEmpty()
	{
		if (arrowDelay <= 0f)
		{
			SemiFunc.UIShowArrow(new Vector3(340f, 340f, 0f), new Vector3(370f, 20f, 0f), 200f);
		}
		int num = 3;
		int num2 = Inventory.instance.InventorySpotsOccupied();
		tutorialProgress = 1f - (float)num2 / (float)num;
	}

	private void TaskMap()
	{
		if (Map.Instance.Active)
		{
			TutorialProgressFill(0.2f);
		}
	}

	private void TaskCartMove()
	{
		Transform grabbedObjectTransform = PhysGrabber.instance.grabbedObjectTransform;
		PhysGrabCart physGrabCart = null;
		if ((bool)grabbedObjectTransform)
		{
			physGrabCart = grabbedObjectTransform.GetComponent<PhysGrabCart>();
		}
		if ((bool)physGrabCart)
		{
			tutorialCart = physGrabCart;
			Vector3 vector = Vector3.zero;
			if ((bool)physGrabCart)
			{
				vector = physGrabCart.rb.velocity;
			}
			vector.y = 0f;
			if ((bool)physGrabCart && vector.magnitude > 0.5f && physGrabCart.cartBeingPulled)
			{
				TutorialProgressFill(0.2f);
			}
		}
	}

	private void TaskCartFill()
	{
		if ((bool)tutorialCart)
		{
			int num = 3;
			int itemsInCartCount = tutorialCart.itemsInCartCount;
			tutorialProgress = (float)itemsInCartCount / (float)num;
			return;
		}
		Transform grabbedObjectTransform = PhysGrabber.instance.grabbedObjectTransform;
		PhysGrabCart physGrabCart = null;
		if ((bool)grabbedObjectTransform)
		{
			physGrabCart = grabbedObjectTransform.GetComponent<PhysGrabCart>();
		}
		if ((bool)physGrabCart)
		{
			tutorialCart = physGrabCart;
		}
	}

	private void TaskExtractionPoint()
	{
		GoalUI.instance.Show();
		if (arrowDelay <= 0f)
		{
			SemiFunc.UIShowArrow(new Vector3(340f, 90f, 0f), new Vector3(610f, 330f, 0f), 45f);
		}
		int currentHaul = RoundDirector.instance.currentHaul;
		int haulGoal = RoundDirector.instance.haulGoal;
		float value = (float)currentHaul / (float)haulGoal;
		value = Mathf.Clamp(value, 0f, 0.95f);
		if ((bool)RoundDirector.instance.extractionPointCurrent)
		{
			tutorialExtractionPoint = RoundDirector.instance.extractionPointCurrent;
		}
		if ((bool)tutorialExtractionPoint)
		{
			if (tutorialExtractionPoint.currentState != ExtractionPoint.State.Extracting && tutorialExtractionPoint.currentState != ExtractionPoint.State.Complete)
			{
				tutorialProgress = value;
			}
			if (tutorialExtractionPoint.currentState == ExtractionPoint.State.Complete && tutorialProgress < 0.95f)
			{
				tutorialProgress = 0.95f;
			}
			if (tutorialExtractionPoint.currentState == ExtractionPoint.State.Complete)
			{
				TutorialProgressFill(0.2f);
			}
		}
	}

	private void TaskEnterTuck()
	{
	}
}
