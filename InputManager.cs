using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class InputManager : MonoBehaviour
{
	public static InputManager instance;

	private Dictionary<InputKey, InputAction> inputActions;

	private Dictionary<InputKey, bool> inputToggle;

	internal Dictionary<InputPercentSetting, int> inputPercentSettings;

	private Dictionary<MovementDirection, int> movementBindingIndices;

	private Dictionary<InputKey, List<string>> defaultBindingPaths;

	private Dictionary<InputKey, bool> defaultInputToggleStates;

	private Dictionary<InputPercentSetting, int> defaultInputPercentSettings;

	internal float disableMovementTimer;

	internal float disableAimingTimer;

	internal float mouseSensitivity = 0.1f;

	private Dictionary<string, InputKey> tagDictionary = new Dictionary<string, InputKey>();

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			InitializeInputs();
			StoreDefaultBindings();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.ResetAndDisableAllDevices;
		tagDictionary.Add("[move]", InputKey.Movement);
		tagDictionary.Add("[jump]", InputKey.Jump);
		tagDictionary.Add("[grab]", InputKey.Grab);
		tagDictionary.Add("[grab2]", InputKey.Rotate);
		tagDictionary.Add("[sprint]", InputKey.Sprint);
		tagDictionary.Add("[crouch]", InputKey.Crouch);
		tagDictionary.Add("[map]", InputKey.Map);
		tagDictionary.Add("[inventory1]", InputKey.Inventory1);
		tagDictionary.Add("[inventory2]", InputKey.Inventory2);
		tagDictionary.Add("[inventory3]", InputKey.Inventory3);
		tagDictionary.Add("[tumble]", InputKey.Tumble);
		tagDictionary.Add("[interact]", InputKey.Interact);
		tagDictionary.Add("[push]", InputKey.Push);
		tagDictionary.Add("[pull]", InputKey.Pull);
		tagDictionary.Add("[chat]", InputKey.Chat);
		tagDictionary.Add("[expression1]", InputKey.Expression1);
		tagDictionary.Add("[expression2]", InputKey.Expression2);
		tagDictionary.Add("[expression3]", InputKey.Expression3);
		tagDictionary.Add("[expression4]", InputKey.Expression4);
		tagDictionary.Add("[expression5]", InputKey.Expression5);
		tagDictionary.Add("[expression6]", InputKey.Expression6);
		tagDictionary.Add("[menu]", InputKey.Menu);
		ES3.DeleteFile("DefaultKeyBindings.es3");
		if (!ES3.FileExists(new ES3Settings("DefaultKeyBindings.es3", ES3.Location.File)))
		{
			SaveDefaultKeyBindings();
		}
		if (!ES3.FileExists(new ES3Settings("CurrentKeyBindings.es3", ES3.Location.File)))
		{
			SaveCurrentKeyBindings();
		}
		LoadKeyBindings("CurrentKeyBindings.es3");
	}

	private void FixedUpdate()
	{
		float num = Mathf.Min(Time.fixedDeltaTime, 0.05f);
		if (disableMovementTimer > 0f)
		{
			disableMovementTimer -= num;
		}
		if (disableAimingTimer > 0f)
		{
			disableAimingTimer -= num;
		}
	}

	private void InitializeInputs()
	{
		inputActions = new Dictionary<InputKey, InputAction>();
		movementBindingIndices = new Dictionary<MovementDirection, int>();
		inputToggle = new Dictionary<InputKey, bool>();
		InputAction inputAction = new InputAction("Movement");
		InputActionSetupExtensions.CompositeSyntax compositeSyntax = inputAction.AddCompositeBinding("2DVector");
		compositeSyntax.With("Up", "<Keyboard>/w");
		compositeSyntax.With("Down", "<Keyboard>/s");
		compositeSyntax.With("Left", "<Keyboard>/a");
		compositeSyntax.With("Right", "<Keyboard>/d");
		inputActions[InputKey.Movement] = inputAction;
		ReadOnlyArray<InputBinding> bindings = inputAction.bindings;
		for (int i = 0; i < bindings.Count; i++)
		{
			InputBinding inputBinding = bindings[i];
			if (inputBinding.isPartOfComposite)
			{
				switch (inputBinding.name.ToLower())
				{
				case "up":
					movementBindingIndices[MovementDirection.Up] = i;
					break;
				case "down":
					movementBindingIndices[MovementDirection.Down] = i;
					break;
				case "left":
					movementBindingIndices[MovementDirection.Left] = i;
					break;
				case "right":
					movementBindingIndices[MovementDirection.Right] = i;
					break;
				}
			}
		}
		InputAction inputAction2 = new InputAction("Scroll");
		inputAction2.AddBinding("<Mouse>/scroll/y");
		inputActions[InputKey.Scroll] = inputAction2;
		InputAction value = new InputAction("Jump", InputActionType.Value, "<Keyboard>/space");
		inputActions[InputKey.Jump] = value;
		value = new InputAction("Use", InputActionType.Value, "<Keyboard>/e");
		inputActions[InputKey.Interact] = value;
		value = new InputAction("MouseInput", InputActionType.Value, "<Pointer>/position");
		inputActions[InputKey.MouseInput] = value;
		InputAction inputAction3 = new InputAction("Push");
		inputAction3.AddBinding("<Mouse>/scroll/y");
		inputActions[InputKey.Push] = inputAction3;
		InputAction inputAction4 = new InputAction("Pull");
		inputAction4.AddBinding("<Mouse>/scroll/y");
		inputActions[InputKey.Pull] = inputAction4;
		value = new InputAction("Menu", InputActionType.Value, "<Keyboard>/escape");
		inputActions[InputKey.Menu] = value;
		value = new InputAction("Back", InputActionType.Value, "<Keyboard>/escape");
		inputActions[InputKey.Back] = value;
		value = new InputAction("BackEditor", InputActionType.Value, "<Keyboard>/F1");
		inputActions[InputKey.BackEditor] = value;
		value = new InputAction("Chat", InputActionType.Value, "<Keyboard>/t");
		inputActions[InputKey.Chat] = value;
		value = new InputAction("Map", InputActionType.Value, "<Keyboard>/tab");
		inputActions[InputKey.Map] = value;
		inputToggle.Add(InputKey.Map, value: false);
		value = new InputAction("Confirm", InputActionType.Value, "<Keyboard>/enter");
		inputActions[InputKey.Confirm] = value;
		value = new InputAction("Grab", InputActionType.Value, "<Mouse>/leftButton");
		inputActions[InputKey.Grab] = value;
		inputToggle.Add(InputKey.Grab, value: false);
		value = new InputAction("Rotate", InputActionType.Value, "<Mouse>/rightButton");
		inputActions[InputKey.Rotate] = value;
		value = new InputAction("Crouch", InputActionType.Value, "<Keyboard>/ctrl");
		inputActions[InputKey.Crouch] = value;
		inputToggle.Add(InputKey.Crouch, value: false);
		value = new InputAction("Chat Delete", InputActionType.Value, "<Keyboard>/backspace");
		inputActions[InputKey.ChatDelete] = value;
		value = new InputAction("Tumble", InputActionType.Value, "<Keyboard>/q");
		inputActions[InputKey.Tumble] = value;
		value = new InputAction("Sprint", InputActionType.Value, "<Keyboard>/leftShift");
		inputActions[InputKey.Sprint] = value;
		inputToggle.Add(InputKey.Sprint, value: false);
		value = new InputAction("MouseDelta", InputActionType.Value, "<Pointer>/delta");
		inputActions[InputKey.MouseDelta] = value;
		value = new InputAction("Inventory1", InputActionType.Value, "<Keyboard>/1");
		inputActions[InputKey.Inventory1] = value;
		value = new InputAction("Inventory2", InputActionType.Value, "<Keyboard>/2");
		inputActions[InputKey.Inventory2] = value;
		value = new InputAction("Inventory3", InputActionType.Value, "<Keyboard>/3");
		inputActions[InputKey.Inventory3] = value;
		value = new InputAction("SpectateNext", InputActionType.Value, "<Mouse>/rightButton");
		inputActions[InputKey.SpectateNext] = value;
		value = new InputAction("SpectatePrevious", InputActionType.Value, "<Mouse>/leftButton");
		inputActions[InputKey.SpectatePrevious] = value;
		value = new InputAction("PushToTalk", InputActionType.Value, "<Keyboard>/v");
		inputActions[InputKey.PushToTalk] = value;
		value = new InputAction("ToggleMute", InputActionType.Value, "<Keyboard>/b");
		inputActions[InputKey.ToggleMute] = value;
		inputPercentSettings = new Dictionary<InputPercentSetting, int>();
		inputPercentSettings[InputPercentSetting.MouseSensitivity] = 50;
		value = new InputAction("Expression1", InputActionType.Value, "<Keyboard>/5");
		inputActions[InputKey.Expression1] = value;
		inputToggle.Add(InputKey.Expression1, value: true);
		value = new InputAction("Expression2", InputActionType.Value, "<Keyboard>/6");
		inputActions[InputKey.Expression2] = value;
		value = new InputAction("Expression3", InputActionType.Value, "<Keyboard>/7");
		inputActions[InputKey.Expression3] = value;
		value = new InputAction("Expression4", InputActionType.Value, "<Keyboard>/8");
		inputActions[InputKey.Expression4] = value;
		value = new InputAction("Expression5", InputActionType.Value, "<Keyboard>/9");
		inputActions[InputKey.Expression5] = value;
		value = new InputAction("Expression6", InputActionType.Value, "<Keyboard>/0");
		inputActions[InputKey.Expression6] = value;
		foreach (InputAction value2 in inputActions.Values)
		{
			value2.Enable();
		}
	}

	private void StoreDefaultBindings()
	{
		defaultBindingPaths = new Dictionary<InputKey, List<string>>();
		defaultInputToggleStates = new Dictionary<InputKey, bool>();
		defaultInputPercentSettings = new Dictionary<InputPercentSetting, int>();
		foreach (InputKey key in inputActions.Keys)
		{
			InputAction inputAction = inputActions[key];
			List<string> list = new List<string>();
			foreach (InputBinding binding in inputAction.bindings)
			{
				list.Add(binding.path);
			}
			defaultBindingPaths[key] = list;
		}
		foreach (KeyValuePair<InputKey, bool> item in inputToggle)
		{
			defaultInputToggleStates[item.Key] = item.Value;
		}
		foreach (KeyValuePair<InputPercentSetting, int> inputPercentSetting in inputPercentSettings)
		{
			defaultInputPercentSettings[inputPercentSetting.Key] = inputPercentSetting.Value;
		}
	}

	public void SaveDefaultKeyBindings()
	{
		KeyBindingSaveData keyBindingSaveData = new KeyBindingSaveData();
		keyBindingSaveData.bindingOverrides = defaultBindingPaths;
		keyBindingSaveData.inputToggleStates = defaultInputToggleStates;
		keyBindingSaveData.inputPercentSettings = defaultInputPercentSettings;
		ES3Settings eS3Settings = new ES3Settings(ES3.Location.Cache);
		eS3Settings.path = "DefaultKeyBindings.es3";
		ES3.Save("KeyBindings", keyBindingSaveData, eS3Settings);
		ES3.StoreCachedFile(eS3Settings);
	}

	public void SaveCurrentKeyBindings()
	{
		KeyBindingSaveData keyBindingSaveData = new KeyBindingSaveData();
		keyBindingSaveData.bindingOverrides = new Dictionary<InputKey, List<string>>();
		keyBindingSaveData.inputToggleStates = new Dictionary<InputKey, bool>(inputToggle);
		keyBindingSaveData.inputPercentSettings = new Dictionary<InputPercentSetting, int>(inputPercentSettings);
		foreach (InputKey key in inputActions.Keys)
		{
			InputAction inputAction = inputActions[key];
			List<string> list = new List<string>();
			foreach (InputBinding binding in inputAction.bindings)
			{
				list.Add(string.IsNullOrEmpty(binding.overridePath) ? binding.path : binding.overridePath);
			}
			keyBindingSaveData.bindingOverrides[key] = list;
		}
		ES3Settings eS3Settings = new ES3Settings(ES3.Location.Cache);
		eS3Settings.path = "CurrentKeyBindings.es3";
		ES3.Save("KeyBindings", keyBindingSaveData, eS3Settings);
		ES3.StoreCachedFile(eS3Settings);
	}

	public void LoadKeyBindings(string filename)
	{
		try
		{
			ES3Settings settings = new ES3Settings(filename, ES3.Location.File);
			if (ES3.FileExists(settings))
			{
				KeyBindingSaveData saveData = ES3.Load<KeyBindingSaveData>("KeyBindings", settings);
				ApplyLoadedKeyBindings(saveData);
			}
			else
			{
				Debug.LogWarning("Keybindings file not found: " + filename);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to load keybindings: " + ex.Message);
		}
	}

	private void ApplyLoadedKeyBindings(KeyBindingSaveData saveData)
	{
		foreach (InputKey key in saveData.bindingOverrides.Keys)
		{
			if (!inputActions.TryGetValue(key, out var value))
			{
				continue;
			}
			List<string> list = saveData.bindingOverrides[key];
			value.Disable();
			for (int i = 0; i < list.Count; i++)
			{
				string text = list[i];
				if (!string.IsNullOrEmpty(text) && value.bindings.Count > i)
				{
					value.ApplyBindingOverride(i, text);
				}
			}
			value.Enable();
		}
		if (saveData.inputToggleStates != null)
		{
			foreach (KeyValuePair<InputKey, bool> inputToggleState in saveData.inputToggleStates)
			{
				inputToggle[inputToggleState.Key] = inputToggleState.Value;
			}
		}
		if (saveData.inputPercentSettings == null)
		{
			return;
		}
		foreach (KeyValuePair<InputPercentSetting, int> inputPercentSetting in saveData.inputPercentSettings)
		{
			inputPercentSettings[inputPercentSetting.Key] = inputPercentSetting.Value;
		}
	}

	public void ResetKeyToDefault(InputKey key)
	{
		if (inputActions.TryGetValue(key, out var value))
		{
			value.Disable();
			List<string> list = defaultBindingPaths[key];
			for (int i = 0; i < list.Count; i++)
			{
				value.ApplyBindingOverride(i, list[i]);
			}
			value.Enable();
			if (defaultInputToggleStates.ContainsKey(key))
			{
				inputToggle[key] = defaultInputToggleStates[key];
			}
			if (defaultInputPercentSettings.ContainsKey((InputPercentSetting)key))
			{
				inputPercentSettings[(InputPercentSetting)key] = defaultInputPercentSettings[(InputPercentSetting)key];
			}
		}
		else
		{
			Debug.LogWarning("InputKey not found: " + key);
		}
	}

	public bool KeyDown(InputKey key)
	{
		if ((key == InputKey.Jump || key == InputKey.Crouch || key == InputKey.Tumble || key == InputKey.Inventory1 || key == InputKey.Inventory2 || key == InputKey.Inventory3 || key == InputKey.Interact || key == InputKey.ToggleMute || key == InputKey.Expression1 || key == InputKey.Expression2 || key == InputKey.Expression3 || key == InputKey.Expression4 || key == InputKey.Expression5 || key == InputKey.Expression6) && disableMovementTimer > 0f)
		{
			return false;
		}
		if (inputActions.TryGetValue(key, out var value))
		{
			return key switch
			{
				InputKey.Push => value.ReadValue<Vector2>().y > 0f, 
				InputKey.Pull => value.ReadValue<Vector2>().y < 0f, 
				_ => value.WasPressedThisFrame(), 
			};
		}
		return false;
	}

	public bool KeyUp(InputKey key)
	{
		if ((key == InputKey.Jump || key == InputKey.Crouch || key == InputKey.Tumble) && disableMovementTimer > 0f)
		{
			return false;
		}
		if (inputActions.TryGetValue(key, out var value))
		{
			if (key == InputKey.Push || key == InputKey.Pull)
			{
				return false;
			}
			return value.WasReleasedThisFrame();
		}
		return false;
	}

	public float KeyPullAndPush()
	{
		if (inputActions.TryGetValue(InputKey.Push, out var value))
		{
			if (value.bindings[0].effectivePath.EndsWith("scroll/y"))
			{
				if (value.ReadValue<float>() > 0f)
				{
					return value.ReadValue<float>();
				}
			}
			else if (value.IsPressed())
			{
				return 1f;
			}
		}
		if (inputActions.TryGetValue(InputKey.Pull, out var value2))
		{
			if (value2.bindings[0].effectivePath.EndsWith("scroll/y"))
			{
				if (value2.ReadValue<float>() < 0f)
				{
					return value2.ReadValue<float>();
				}
			}
			else if (value2.IsPressed())
			{
				return -1f;
			}
		}
		return 0f;
	}

	public InputAction GetAction(InputKey key)
	{
		if (inputActions.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public InputAction GetMovementAction()
	{
		if (inputActions.TryGetValue(InputKey.Movement, out var value))
		{
			return value;
		}
		return null;
	}

	public int GetMovementBindingIndex(MovementDirection direction)
	{
		if (movementBindingIndices.TryGetValue(direction, out var value))
		{
			return value;
		}
		return -1;
	}

	public bool KeyHold(InputKey key)
	{
		if ((key == InputKey.Jump || key == InputKey.Crouch || key == InputKey.Tumble || key == InputKey.Expression1 || key == InputKey.Expression2 || key == InputKey.Expression3 || key == InputKey.Expression4 || key == InputKey.Expression5 || key == InputKey.Expression6) && disableMovementTimer > 0f)
		{
			return false;
		}
		if (inputActions.TryGetValue(key, out var value))
		{
			return key switch
			{
				InputKey.Push => value.ReadValue<Vector2>().y > 0f, 
				InputKey.Pull => value.ReadValue<Vector2>().y < 0f, 
				_ => value.IsPressed(), 
			};
		}
		return false;
	}

	public float GetMovementX()
	{
		if (disableMovementTimer > 0f)
		{
			return 0f;
		}
		if (inputActions.TryGetValue(InputKey.Movement, out var value))
		{
			return value.ReadValue<Vector2>().x;
		}
		return 0f;
	}

	public float GetScrollY()
	{
		if (inputActions.TryGetValue(InputKey.Scroll, out var value))
		{
			return value.ReadValue<float>();
		}
		return 0f;
	}

	public float GetMovementY()
	{
		if (disableMovementTimer > 0f)
		{
			return 0f;
		}
		if (inputActions.TryGetValue(InputKey.Movement, out var value))
		{
			return value.ReadValue<Vector2>().y;
		}
		return 0f;
	}

	public Vector2 GetMovement()
	{
		if (disableMovementTimer > 0f)
		{
			return Vector2.zero;
		}
		if (inputActions.TryGetValue(InputKey.Movement, out var value))
		{
			return value.ReadValue<Vector2>();
		}
		return Vector2.zero;
	}

	public float GetMouseX()
	{
		if (disableAimingTimer > 0f)
		{
			return 0f;
		}
		if (inputActions.TryGetValue(InputKey.MouseDelta, out var value))
		{
			return value.ReadValue<Vector2>().x * mouseSensitivity;
		}
		return 0f;
	}

	public float GetMouseY()
	{
		if (disableAimingTimer > 0f)
		{
			return 0f;
		}
		if (inputActions.TryGetValue(InputKey.MouseDelta, out var value))
		{
			return value.ReadValue<Vector2>().y * mouseSensitivity;
		}
		return 0f;
	}

	public Vector2 GetMousePosition()
	{
		if (disableAimingTimer > 0f)
		{
			return Vector2.zero;
		}
		if (inputActions.TryGetValue(InputKey.MouseInput, out var value))
		{
			return value.ReadValue<Vector2>();
		}
		return Vector2.zero;
	}

	public void Rebind(InputKey key, string newBinding)
	{
		if (inputActions.TryGetValue(key, out var value))
		{
			value.ApplyBindingOverride(newBinding);
		}
	}

	public void RebindMovementKey(MovementDirection direction, string newBinding)
	{
		if (inputActions.TryGetValue(InputKey.Movement, out var value))
		{
			if (movementBindingIndices.TryGetValue(direction, out var value2))
			{
				value.ApplyBindingOverride(value2, newBinding);
			}
			else
			{
				Debug.LogWarning($"Binding index for {direction} not found.");
			}
		}
	}

	public void DisableMovement()
	{
		disableMovementTimer = 0.1f;
	}

	public void DisableAiming()
	{
		disableAimingTimer = 0.1f;
	}

	public void InputToggleRebind(InputKey key, bool toggle)
	{
		inputToggle[key] = toggle;
	}

	public bool InputToggleGet(InputKey key)
	{
		return inputToggle[key];
	}

	public string GetKeyString(InputKey key)
	{
		return GetAction(key)?.bindings[0].effectivePath;
	}

	public string GetMovementKeyString(MovementDirection direction)
	{
		InputAction movementAction = GetMovementAction();
		int movementBindingIndex = GetMovementBindingIndex(direction);
		return movementAction?.bindings[movementBindingIndex].effectivePath;
	}

	public string InputDisplayGet(InputKey _inputKey, MenuKeybind.KeyType _keyType, MovementDirection _movementDirection)
	{
		switch (_keyType)
		{
		case MenuKeybind.KeyType.InputKey:
		{
			InputAction action = GetAction(_inputKey);
			if (action != null)
			{
				int bindingIndex = 0;
				return InputDisplayGetString(action, bindingIndex);
			}
			break;
		}
		case MenuKeybind.KeyType.MovementKey:
		{
			InputAction movementAction = GetMovementAction();
			int movementBindingIndex = GetMovementBindingIndex(_movementDirection);
			if (movementAction != null && movementBindingIndex >= 0)
			{
				return InputDisplayGetString(movementAction, movementBindingIndex);
			}
			break;
		}
		}
		return "Unassigned";
	}

	public string InputDisplayGetString(InputAction action, int bindingIndex)
	{
		InputBinding inputBinding = action.bindings[bindingIndex];
		string text = InputControlPath.ToHumanReadableString(inputBinding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
		bool flag = false;
		if (text.EndsWith("Scroll/Y"))
		{
			text = "Mouse Scroll";
			flag = true;
		}
		if (inputBinding.effectivePath.Contains("Mouse") && !flag)
		{
			text = InputDisplayMouseStringGet(inputBinding.effectivePath);
		}
		return text.ToUpper();
	}

	private string InputDisplayMouseStringGet(string path)
	{
		if (path.Contains("leftButton"))
		{
			return "Mouse Left";
		}
		if (path.Contains("rightButton"))
		{
			return "Mouse Right";
		}
		if (path.Contains("middleButton"))
		{
			return "Mouse Middle";
		}
		if (path.Contains("press"))
		{
			return "Mouse Press";
		}
		if (path.Contains("backButton"))
		{
			return "Mouse Back";
		}
		if (path.Contains("forwardButton"))
		{
			return "Mouse Forward";
		}
		if (path.Contains("button"))
		{
			int num = path.IndexOf("button");
			string text = path.Substring(num + "button".Length);
			return "Mouse " + text;
		}
		return "Mouse Button";
	}

	public string InputDisplayReplaceTags(string _text, string _prefix = "<u><b>", string _suffix = "</b></u>")
	{
		foreach (KeyValuePair<string, InputKey> item in tagDictionary)
		{
			string text = "";
			if (item.Value == InputKey.Movement)
			{
				text = InputDisplayGet(item.Value, MenuKeybind.KeyType.MovementKey, MovementDirection.Up);
				text += InputDisplayGet(item.Value, MenuKeybind.KeyType.MovementKey, MovementDirection.Left);
				text += InputDisplayGet(item.Value, MenuKeybind.KeyType.MovementKey, MovementDirection.Down);
				text += InputDisplayGet(item.Value, MenuKeybind.KeyType.MovementKey, MovementDirection.Right);
			}
			else
			{
				text = InputDisplayGet(item.Value, MenuKeybind.KeyType.InputKey, MovementDirection.Up);
			}
			_text = _text.Replace(item.Key, _prefix + text + _suffix);
		}
		return _text;
	}

	public void ResetInput()
	{
		InputSystem.ResetHaptics();
		InputSystem.ResetDevice(Keyboard.current);
		foreach (KeyValuePair<InputKey, InputAction> inputAction in inputActions)
		{
			inputAction.Value.Reset();
		}
	}
}
