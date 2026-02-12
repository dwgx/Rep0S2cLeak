using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuSliderMicrophone : MonoBehaviour
{
	private MenuSlider menuSlider;

	public UnityEvent micEvent;

	public Image micLevelBar;

	private AudioClip microphoneClip;

	private bool microphoneClipEnabled;

	private string currentDeviceName;

	private int currentDeviceCount;

	private const int sampleDataLength = 128;

	private float[] micData;

	private float micLevel;

	private float micGain = 10f;

	private void Awake()
	{
		menuSlider = GetComponent<MenuSlider>();
		micData = new float[128];
		SetOptions();
	}

	private void Update()
	{
		if (currentDeviceCount != SessionManager.instance.micDeviceList.Count)
		{
			SetOptions();
			return;
		}
		if (menuSlider.bigSettingText.textMeshPro.text != "device name" && SessionManager.instance.micDeviceCurrent != menuSlider.bigSettingText.textMeshPro.text)
		{
			SessionManager.instance.micDeviceCurrent = menuSlider.bigSettingText.textMeshPro.text;
			DataDirector.instance.micDevice = SessionManager.instance.micDeviceCurrent;
			DataDirector.instance.SaveSettings();
		}
		if (!PlayerVoiceChat.instance)
		{
			if (SessionManager.instance.micDeviceCurrent != currentDeviceName)
			{
				Microphone.End(currentDeviceName);
				currentDeviceName = SessionManager.instance.micDeviceCurrent;
				microphoneClipEnabled = false;
			}
			if (currentDeviceName != "NONE")
			{
				if (!microphoneClipEnabled)
				{
					bool flag = false;
					string[] devices = Microphone.devices;
					for (int i = 0; i < devices.Length; i++)
					{
						if (devices[i] == currentDeviceName)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						microphoneClipEnabled = true;
						microphoneClip = Microphone.Start(currentDeviceName, loop: true, 1, 44100);
					}
				}
				if (microphoneClipEnabled)
				{
					int num = Microphone.GetPosition(currentDeviceName) - 128 + 1;
					if (num < 0)
					{
						return;
					}
					microphoneClip.GetData(micData, num);
					float num2 = 0f;
					for (int j = 0; j < micData.Length; j++)
					{
						num2 += micData[j] * micData[j];
					}
					micLevel = Mathf.Sqrt(num2 / (float)micData.Length) * micGain;
					micLevel = Mathf.Clamp01(micLevel);
				}
			}
			if (!microphoneClipEnabled)
			{
				micLevel = 0f;
			}
		}
		else
		{
			micLevel = PlayerVoiceChat.instance.clipLoudnessNoTTS * 5f;
		}
		micLevelBar.GetComponent<RectTransform>().localScale = new Vector3(Mathf.Lerp(micLevelBar.GetComponent<RectTransform>().localScale.x, micLevel, Time.deltaTime * 10f), 0.2f, 1f);
	}

	private void SetOptions()
	{
		menuSlider.customOptions.Clear();
		foreach (string micDevice in SessionManager.instance.micDeviceList)
		{
			menuSlider.CustomOptionAdd(micDevice, micEvent);
		}
		menuSlider.CustomOptionAdd("NONE", micEvent);
		foreach (MenuSlider.CustomOption customOption in menuSlider.customOptions)
		{
			customOption.customValueInt = menuSlider.customOptions.IndexOf(customOption);
		}
		currentDeviceCount = SessionManager.instance.micDeviceList.Count;
	}
}
