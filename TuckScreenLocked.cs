using TMPro;
using UnityEngine;

public class TuckScreenLocked : MonoBehaviour
{
	public TextMeshProUGUI text;

	public MeshRenderer scanLines;

	public GameObject enableScreenLock;

	internal bool isLocked;

	private string lockedText = "";

	[SerializeField]
	private float cycleInterval = 0.5f;

	private float cycleTimer;

	private int textIndex = -1;

	private string[] textPhases = new string[4] { "<color=#2B0050>.</color><color=#2B0050>.</color><color=#2B0050>.</color>", "<color=#FF0000>.</color><color=#2B0050>.</color><color=#2B0050>.</color>", "<color=#FF0000>.</color><color=#FF0000>.</color><color=#2B0050>.</color>", "<color=#FF0000>.</color><color=#FF0000>.</color><color=#FF0000>.</color>" };

	private void Update()
	{
		new Color(1f, 0.2f, 0f);
		isLocked = true;
		if (isLocked)
		{
			cycleTimer += Time.deltaTime;
			if (cycleTimer >= cycleInterval)
			{
				cycleTimer = 0f;
				textIndex++;
				if (textIndex >= textPhases.Length)
				{
					textIndex = 0;
				}
			}
			if (textIndex == -1)
			{
				text.text = lockedText + textPhases[0];
			}
			else
			{
				text.text = lockedText + textPhases[textIndex];
			}
		}
		else
		{
			text.text = lockedText;
		}
	}

	public void LockChatToggle(bool _lock, string _lockedText = "", Color _lightColor = default(Color), Color _darkColor = default(Color))
	{
		isLocked = _lock;
		this.text.color = _lightColor;
		string text = ColorUtility.ToHtmlStringRGB(_darkColor);
		string text2 = ColorUtility.ToHtmlStringRGB(_lightColor);
		textPhases = new string[4]
		{
			"<color=#" + text + ">.</color><color=#" + text + ">.</color><color=#" + text + ">.</color>",
			"<color=#" + text2 + ">.</color><color=#" + text + ">.</color><color=#" + text + ">.</color>",
			"<color=#" + text2 + ">.</color><color=#" + text2 + ">.</color><color=#" + text + ">.</color>",
			"<color=#" + text2 + ">.</color><color=#" + text2 + ">.</color><color=#" + text2 + ">.</color>"
		};
		if (isLocked)
		{
			lockedText = _lockedText;
			scanLines.material.color = _lightColor;
			enableScreenLock.SetActive(value: true);
			cycleTimer = 0f;
			textIndex = -1;
			this.text.text = lockedText;
		}
		else
		{
			lockedText = "";
			enableScreenLock.SetActive(value: false);
			this.text.text = "";
		}
	}
}
