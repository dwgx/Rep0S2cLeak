using UnityEngine;

public class RecordingDirector : MonoBehaviour
{
	internal bool hideUI;

	public static RecordingDirector instance;

	public Light playerLight;

	private float lightHue;

	private void Start()
	{
		if (instance != null && instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
		if (!PlayerAvatar.instance.isDisabled)
		{
			playerLight.transform.position = PlayerAvatar.instance.PlayerVisionTarget.VisionTransform.position;
		}
		if (Input.GetKey(KeyCode.Keypad4))
		{
			Color.RGBToHSV(playerLight.color, out var H, out var _, out var _);
			H = (H + Time.deltaTime * 0.2f) % 1f;
			playerLight.color = Color.HSVToRGB(H, 1f, 1f);
		}
		if (Input.GetKey(KeyCode.Keypad6))
		{
			Color.RGBToHSV(playerLight.color, out var H2, out var _, out var _);
			H2 = (H2 - Time.deltaTime * 0.2f) % 1f;
			playerLight.color = Color.HSVToRGB(H2, 1f, 1f);
		}
		if (Input.GetKey(KeyCode.Keypad8))
		{
			playerLight.range += Time.deltaTime * 30f;
		}
		if (Input.GetKey(KeyCode.Keypad2))
		{
			playerLight.range -= Time.deltaTime * 30f;
		}
		if (Input.GetKey(KeyCode.Keypad7))
		{
			playerLight.intensity += Time.deltaTime * 2f;
		}
		if (Input.GetKey(KeyCode.Keypad9))
		{
			playerLight.intensity -= Time.deltaTime * 2f;
		}
		if (Input.GetKey(KeyCode.Keypad0))
		{
			playerLight.intensity = 1f;
			playerLight.range = 10f;
			playerLight.color = Color.white;
		}
		if (!MenuPageEsc.instance && SemiFunc.NoTextInputsActive())
		{
			hideUI = true;
		}
		else
		{
			hideUI = false;
		}
		if (hideUI)
		{
			RenderTextureMain.instance.OverlayDisable();
		}
		FlashlightController.Instance.hideFlashlight = true;
		GameplayManager.instance.OverrideCameraAnimation(0f, 0.2f);
		if (SemiFunc.RunIsLobbyMenu() || SemiFunc.MenuLevel())
		{
			GameDirector.instance.CommandRecordingDirectorToggle();
		}
	}
}
