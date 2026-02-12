using System;
using System.Reflection;
using UnityEngine;

public class EnemyDebugGUI : MonoBehaviour
{
	public bool showDebugUI;

	[Header("Enemy Script Reference")]
	public MonoBehaviour enemyScript;

	[Header("State Property Names")]
	public string currentStatePropertyName = "currentState";

	public string stateTimePropertyName = "stateTimer";

	public string stateTimeMaxPropertyName = "stateTimerMax";

	private PropertyInfo currentStateProperty;

	private PropertyInfo stateTimeProperty;

	private PropertyInfo stateTimeMaxProperty;

	private FieldInfo currentStateField;

	private FieldInfo stateTimeField;

	private FieldInfo stateTimeMaxField;

	private string currentState = "No State";

	private string previousState = "No State";

	private string stateTimeText = "";

	private void Start()
	{
		base.enabled = false;
	}

	private void InitializeStateProperties()
	{
		if (!enemyScript)
		{
			enemyScript = GetComponent<MonoBehaviour>();
		}
		if ((bool)enemyScript)
		{
			Type type = enemyScript.GetType();
			currentStateField = type.GetField(currentStatePropertyName, BindingFlags.Instance | BindingFlags.Public);
			if (currentStateField == null)
			{
				currentStateField = type.GetField(currentStatePropertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			}
			currentStateProperty = type.GetProperty(currentStatePropertyName, BindingFlags.Instance | BindingFlags.Public);
			if (currentStateProperty == null)
			{
				currentStateProperty = type.GetProperty(currentStatePropertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			}
			stateTimeField = type.GetField(stateTimePropertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			stateTimeProperty = type.GetProperty(stateTimePropertyName, BindingFlags.Instance | BindingFlags.Public);
			if (stateTimeProperty == null)
			{
				stateTimeProperty = type.GetProperty(stateTimePropertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			}
			stateTimeMaxField = type.GetField(stateTimeMaxPropertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			stateTimeMaxProperty = type.GetProperty(stateTimeMaxPropertyName, BindingFlags.Instance | BindingFlags.Public);
			if (stateTimeMaxProperty == null)
			{
				stateTimeMaxProperty = type.GetProperty(stateTimeMaxPropertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			}
			if (currentStateProperty == null && currentStateField == null)
			{
				Debug.LogWarning("EnemyDebugGUI: Could not find property or field '" + currentStatePropertyName + "' on " + type.Name);
			}
		}
	}

	private void Update()
	{
		if (!showDebugUI || !enemyScript)
		{
			return;
		}
		try
		{
			if (enemyScript is EnemyOogly { currentState: var state })
			{
				string text = currentState;
				currentState = state.ToString();
				if (currentState != text)
				{
					previousState = text;
				}
			}
			else if (currentStateField != null)
			{
				object value = currentStateField.GetValue(enemyScript);
				string text2 = currentState;
				currentState = value?.ToString() ?? "Null";
				if (currentState != text2)
				{
					previousState = text2;
				}
			}
			else if (currentStateProperty != null)
			{
				object value2 = currentStateProperty.GetValue(enemyScript);
				string text3 = currentState;
				currentState = value2?.ToString() ?? "Null";
				if (currentState != text3)
				{
					previousState = text3;
				}
			}
			float? num = null;
			float? num2 = null;
			if (stateTimeField != null)
			{
				if (stateTimeField.GetValue(enemyScript) is float value3)
				{
					num = value3;
				}
			}
			else if (stateTimeProperty != null && stateTimeProperty.GetValue(enemyScript) is float value4)
			{
				num = value4;
			}
			if (stateTimeMaxField != null)
			{
				if (stateTimeMaxField.GetValue(enemyScript) is float value5)
				{
					num2 = value5;
				}
			}
			else if (stateTimeMaxProperty != null && stateTimeMaxProperty.GetValue(enemyScript) is float value6)
			{
				num2 = value6;
			}
			if (num.HasValue && num2.HasValue)
			{
				stateTimeText = $"{num.Value:F1} / {num2.Value:F1}s";
			}
			else if (num.HasValue)
			{
				stateTimeText = $"{num.Value:F1}s";
			}
			else
			{
				stateTimeText = "";
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("EnemyDebugGUI Error: " + ex.Message);
		}
	}
}
