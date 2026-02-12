using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MenuTwoOptions : MonoBehaviour
{
	public string option1Text = "ON";

	public string option2Text = "OFF";

	public RectTransform optionsBox;

	public RectTransform optionsBoxBehind;

	public Vector3 targetPosition;

	public Vector3 targetScale;

	public DataDirector.Setting setting;

	public bool customEvents = true;

	public bool settingSet;

	public bool customFetch = true;

	public UnityEvent onOption1;

	public UnityEvent onOption2;

	public UnityEvent fetchSetting;

	public TextMeshProUGUI option1TextMesh;

	public TextMeshProUGUI option2TextMesh;

	public bool startSettingFetch = true;

	private bool fetchComplete;

	public string settingName;

	private void Start()
	{
		if ((bool)option1TextMesh)
		{
			option1TextMesh.text = option1Text;
		}
		if ((bool)option1TextMesh)
		{
			option2TextMesh.text = option2Text;
		}
		StartFetch();
	}

	private void StartFetch()
	{
		if (customEvents && customFetch)
		{
			fetchSetting.Invoke();
		}
		else
		{
			bool flag = DataDirector.instance.SettingValueFetch(setting) == 1;
			startSettingFetch = flag;
		}
		if (startSettingFetch)
		{
			OnOption1();
		}
		else
		{
			OnOption2();
		}
		fetchComplete = true;
	}

	private void OnValidate()
	{
		if (!SemiFunc.OnValidateCheck())
		{
			if ((bool)option1TextMesh)
			{
				option1TextMesh.text = option1Text;
			}
			if ((bool)option1TextMesh)
			{
				option2TextMesh.text = option2Text;
			}
			TextMeshProUGUI componentInChildren = GetComponentInChildren<TextMeshProUGUI>();
			if ((bool)componentInChildren)
			{
				componentInChildren.text = settingName;
			}
			base.gameObject.name = "Bool Setting - " + settingName;
		}
	}

	private void OnEnable()
	{
		StartFetch();
	}

	private void Update()
	{
		if ((bool)optionsBox)
		{
			optionsBox.localPosition = Vector3.Lerp(optionsBox.localPosition, targetPosition, 20f * Time.deltaTime);
			optionsBox.localScale = Vector3.Lerp(optionsBox.localScale, targetScale / 10f, 20f * Time.deltaTime);
			optionsBoxBehind.localPosition = Vector3.Lerp(optionsBoxBehind.localPosition, targetPosition, 20f * Time.deltaTime);
			optionsBoxBehind.localScale = Vector3.Lerp(optionsBoxBehind.localScale, new Vector3(targetScale.x + 4f, targetScale.y + 2f, 1f) / 10f, 20f * Time.deltaTime);
		}
	}

	public void SetTarget(Vector3 pos, Vector3 scale)
	{
		targetPosition = pos;
		targetScale = scale;
	}

	public void OnOption1()
	{
		SetTarget(new Vector3(37.8f, 12.3f, 0f), new Vector3(73f, 22f, 1f));
		if (!fetchComplete)
		{
			return;
		}
		if (customEvents)
		{
			if (settingSet)
			{
				DataDirector.instance.SettingValueSet(setting, 1);
			}
			onOption1.Invoke();
		}
		else
		{
			DataDirector.instance.SettingValueSet(setting, 1);
		}
	}

	public void OnOption2()
	{
		SetTarget(new Vector3(112.644f, 12.3f, 0f), new Vector3(74f, 22f, 1f));
		if (!fetchComplete)
		{
			return;
		}
		if (customEvents)
		{
			if (settingSet)
			{
				DataDirector.instance.SettingValueSet(setting, 0);
			}
			onOption2.Invoke();
		}
		else
		{
			DataDirector.instance.SettingValueSet(setting, 0);
		}
	}
}
