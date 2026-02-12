using UnityEngine;

[CreateAssetMenu(fileName = "Battery Drain Preset", menuName = "Semi Presets/Battery Drain Preset")]
public class BatteryDrainPresets : ScriptableObject
{
	public float batteryDrainRate = 0.1f;

	public float GetBatteryDrainRate()
	{
		return batteryDrainRate;
	}
}
