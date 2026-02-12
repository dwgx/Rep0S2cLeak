using System;
using System.Collections.Generic;

[Serializable]
public class KeyBindingSaveData
{
	public Dictionary<InputKey, List<string>> bindingOverrides;

	public Dictionary<InputKey, bool> inputToggleStates;

	public Dictionary<InputPercentSetting, int> inputPercentSettings;
}
