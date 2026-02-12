using TMPro;
using UnityEngine;

public class DebugLevelsCompleted : MonoBehaviour
{
	public TextMeshProUGUI Text;

	private void Update()
	{
		Text.text = "Levels Completed: " + RunManager.instance.levelsCompleted;
	}
}
