using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuPagePassword : MonoBehaviour
{
	internal MenuPage menuPage;

	public TextMeshProUGUI passwordText;

	public TextMeshProUGUI passwordCursor;

	public SemiUI passwordSemiUI;

	public MenuButton confirmButton;

	public RectTransform confirmButtonAnimationTransform;

	public AnimationCurve confirmButtonAnimationCurve;

	private float confirmButtonAnimationLerp = 1f;

	private bool confirmButtonState;

	private string password = "";

	private string passwordPrev = "";

	private float tooLongDenyCooldown;

	private bool showing;

	[Space]
	public Sprite showIconOn;

	public Sprite showIconOff;

	public RawImage showImage;

	[Space]
	public SemiUI showUI;

	public SemiUI copyUI;

	private void Start()
	{
		menuPage = GetComponent<MenuPage>();
	}

	private void Update()
	{
		showUI.hideTimer = 0f;
		copyUI.hideTimer = 0f;
		tooLongDenyCooldown -= Time.deltaTime;
		MenuManager.instance.TextInputActive();
		if (password == "\b")
		{
			password = "";
		}
		password += Input.inputString;
		password = password.Replace("\n", "");
		password = password.Replace(" ", "");
		password = password.ToUpper();
		if (Input.inputString == "\b")
		{
			password = password.Remove(Mathf.Max(password.Length - 2, 0));
		}
		password = password.Replace("\r", "");
		if (password.Length > 10)
		{
			password = passwordPrev;
			if (tooLongDenyCooldown <= 0f)
			{
				tooLongDenyCooldown = 0.25f;
				passwordSemiUI.SemiUITextFlashColor(Color.red, 0.2f);
				passwordSemiUI.SemiUISpringShakeX(10f, 10f, 0.3f);
				passwordSemiUI.SemiUISpringScale(0.05f, 5f, 0.2f);
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Deny, null, 1f, 1f, soundOnly: true);
			}
		}
		PasswordTextSet();
		if (passwordPrev != password)
		{
			if (password.Length > passwordPrev.Length)
			{
				passwordSemiUI.SemiUITextFlashColor(Color.yellow, 0.1f);
				passwordSemiUI.SemiUISpringShakeY(2f, 5f, 0.2f);
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 2f, 0.2f, soundOnly: true);
			}
			else
			{
				passwordSemiUI.SemiUITextFlashColor(Color.red, 0.2f);
				passwordSemiUI.SemiUISpringShakeX(5f, 5f, 0.2f);
				MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Dud, null, 2f, 1f, soundOnly: true);
			}
		}
		passwordPrev = password;
		if (SemiFunc.InputDown(InputKey.Confirm))
		{
			ConfirmButton();
		}
		if (!confirmButtonState)
		{
			if (password.Length > 0)
			{
				confirmButton.buttonTextString = "Confirm";
				confirmButton.buttonText.text = confirmButton.buttonTextString;
				confirmButtonAnimationLerp = 0f;
				confirmButtonState = true;
			}
		}
		else if (password.Length <= 0)
		{
			confirmButton.buttonTextString = "Skip";
			confirmButton.buttonText.text = confirmButton.buttonTextString;
			confirmButtonAnimationLerp = 0f;
			confirmButtonState = false;
		}
		if (confirmButtonAnimationLerp < 1f)
		{
			confirmButtonAnimationLerp += Time.deltaTime * 5f;
			confirmButtonAnimationLerp = Mathf.Clamp01(confirmButtonAnimationLerp);
			confirmButtonAnimationTransform.anchoredPosition = new Vector3(0f, confirmButtonAnimationCurve.Evaluate(confirmButtonAnimationLerp) * 5f, 0f);
		}
	}

	private void PasswordTextSet()
	{
		if (showing)
		{
			passwordText.text = password;
		}
		else
		{
			string text = "";
			for (int i = 0; i < password.Length; i++)
			{
				text += "*";
			}
			passwordText.text = text;
		}
		float num = passwordText.renderedWidth;
		if (num < 0f)
		{
			num = 0f;
		}
		Vector3 position = passwordText.transform.position + new Vector3(num + 1f, 1f, 0f);
		passwordCursor.transform.position = position;
		if (Mathf.Sin(Time.time * 8f) > 0f)
		{
			passwordCursor.text = "|";
		}
		else
		{
			passwordCursor.text = "";
		}
	}

	public void ToggleShowButton()
	{
		passwordSemiUI.SemiUITextFlashColor(Color.white, 0.1f);
		passwordSemiUI.SemiUISpringShakeY(2f, 5f, 0.2f);
		MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm, null, 2f, 0.5f);
		showing = !showing;
		if (showing)
		{
			passwordText.alignment = TextAlignmentOptions.Left;
			showImage.texture = showIconOff.texture;
		}
		else
		{
			passwordText.alignment = TextAlignmentOptions.MidlineLeft;
			showImage.texture = showIconOn.texture;
		}
		showUI.SemiUISpringShakeY(2f, 5f, 0.2f);
		PasswordTextSet();
	}

	public void CopyButton()
	{
		copyUI.SemiUISpringShakeY(2f, 5f, 0.2f);
		MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm, null, 2f, 0.5f);
		GUIUtility.systemCopyBuffer = password;
	}

	public void ConfirmButton()
	{
		MenuManager.instance.PageReactivatePageUnderThisPage(menuPage);
		MenuManager.instance.MenuEffectPopUpClose();
		menuPage.PageStateSet(MenuPage.PageState.Closing);
		DataDirector.instance.networkPassword = password;
	}
}
