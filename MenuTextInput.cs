using TMPro;
using UnityEngine;

public class MenuTextInput : MonoBehaviour
{
	public TextMeshProUGUI textMain;

	public TextMeshProUGUI textCursor;

	private SemiUI textUI;

	[Space]
	public bool upperOnly;

	public int maxLength = 60;

	internal string textCurrent = "";

	private string textPrevious = "";

	private float tooLongDenyCooldown;

	private void Start()
	{
		textUI = GetComponentInChildren<SemiUI>();
	}

	private void Update()
	{
		tooLongDenyCooldown -= Time.deltaTime;
		MenuManager.instance.TextInputActive();
		if (textCurrent == "\b")
		{
			textCurrent = "";
		}
		textCurrent += Input.inputString;
		textCurrent = textCurrent.Replace("\n", "");
		if (Input.inputString == "\b")
		{
			textCurrent = textCurrent.Remove(Mathf.Max(textCurrent.Length - 2, 0));
		}
		textCurrent = textCurrent.Replace("\r", "");
		if (textCurrent.Length > maxLength)
		{
			textCurrent = textCurrent.Remove(maxLength);
			if (tooLongDenyCooldown <= 0f)
			{
				tooLongDenyCooldown = 0.25f;
				textUI.SemiUITextFlashColor(Color.red, 0.2f);
				textUI.SemiUISpringShakeX(10f, 10f, 0.3f);
				textUI.SemiUISpringScale(0.05f, 5f, 0.2f);
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Deny, null, 1f, 1f, soundOnly: true);
			}
		}
		InputTextSet();
		if (textPrevious != textCurrent)
		{
			if (textCurrent.Length > textPrevious.Length)
			{
				textUI.SemiUISpringShakeY(1f, 5f, 0.2f);
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 2f, 0.2f, soundOnly: true);
			}
			else
			{
				textUI.SemiUITextFlashColor(Color.red, 0.2f);
				textUI.SemiUISpringShakeX(5f, 5f, 0.2f);
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Dud, null, 2f, 1f, soundOnly: true);
			}
		}
		textPrevious = textCurrent;
	}

	private void InputTextSet()
	{
		if (upperOnly)
		{
			textCurrent = textCurrent.ToUpper();
		}
		textMain.text = textCurrent;
		float num = textMain.renderedWidth;
		if (num < 0f)
		{
			num = 0f;
		}
		Vector3 position = textMain.transform.position + new Vector3(num + 1f, 0f, 0f);
		textCursor.transform.position = position;
		if (Mathf.Sin(Time.time * 8f) > 0f)
		{
			textCursor.text = "|";
		}
		else
		{
			textCursor.text = "";
		}
	}
}
