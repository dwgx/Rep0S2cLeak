using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuKeybind : MonoBehaviour
{
	public enum KeyType
	{
		InputKey,
		MovementKey
	}

	public KeyType keyType;

	public InputKey inputKey;

	public MovementDirection movementDirection;

	private MenuBigButton menuBigButton;

	private MenuPage parentPage;

	private MenuSettingElement settingElement;

	private float actionValue;

	private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

	private void Start()
	{
		menuBigButton = GetComponent<MenuBigButton>();
		parentPage = GetComponentInParent<MenuPage>();
		settingElement = GetComponent<MenuSettingElement>();
		StartCoroutine(LateStart());
	}

	private IEnumerator LateStart()
	{
		yield return null;
		UpdateBindingDisplay();
	}

	private void UpdateBindingDisplay()
	{
		string text = InputManager.instance.InputDisplayGet(inputKey, keyType, movementDirection);
		menuBigButton.buttonName = text;
		menuBigButton.menuButton.buttonText.text = text;
		if ((bool)MenuPageLobby.instance)
		{
			MenuPageLobby.instance.UpdateChatPrompt();
		}
	}

	public void OnClick()
	{
		if (parentPage.SettingElementActiveCheckFree(settingElement.settingElementID))
		{
			menuBigButton.state = MenuBigButton.State.Edit;
			parentPage.SettingElementActiveSet(settingElement.settingElementID);
			StartRebinding();
		}
	}

	private void StartRebinding()
	{
		if (keyType == KeyType.InputKey)
		{
			InputAction action = InputManager.instance.GetAction(inputKey);
			if (action != null)
			{
				int bindingIndex = 0;
				action.Disable();
				rebindingOperation = action.PerformInteractiveRebinding(bindingIndex).WithExpectedControlType("Axis").WithCancelingThrough("")
					.OnComplete(delegate
					{
						action.Enable();
						rebindingOperation.Dispose();
						menuBigButton.state = MenuBigButton.State.Main;
						UpdateBindingDisplay();
						MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm, parentPage, 0.2f);
					})
					.Start();
			}
		}
		else
		{
			if (keyType != KeyType.MovementKey)
			{
				return;
			}
			InputAction action2 = InputManager.instance.GetMovementAction();
			int movementBindingIndex = InputManager.instance.GetMovementBindingIndex(movementDirection);
			if (action2 != null && movementBindingIndex >= 0)
			{
				action2.Disable();
				rebindingOperation = action2.PerformInteractiveRebinding(movementBindingIndex).WithCancelingThrough("").OnComplete(delegate
				{
					action2.Enable();
					rebindingOperation.Dispose();
					menuBigButton.state = MenuBigButton.State.Main;
					UpdateBindingDisplay();
					MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm, parentPage, 0.2f);
				})
					.Start();
			}
		}
	}
}
