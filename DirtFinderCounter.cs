using TMPro;
using UnityEngine;

public class DirtFinderCounter : MonoBehaviour
{
	public MapToolController Controller;

	[Space]
	public TextMeshPro NumberText;

	[Space]
	public GameObject HatchObject;

	public GameObject HatchLightObject;

	[Space]
	public float UpdateTime;

	public float UpdateTimeMin;

	public float UpdateTimeDecrease;

	private float UpdateTimer;

	private float UpdateTimerDecrease;

	[Space]
	public float NumberCurveAmount;

	public float NumberCurveSpeed;

	public AnimationCurve NumberCurve;

	private float NumberCurveLerp = 1f;

	[Space]
	public Sound SoundDown;

	public Sound SoundUp;

	private int PitchUpdated;

	private int PlayerAmount;

	[Space]
	public float PitchIncrease = 0.1f;

	public float PitchMax = 3f;

	private bool active;

	private void Start()
	{
		PlayerAmount = GameDirector.instance.PlayerList.Count;
		UpdateNumbers();
		if (GameManager.Multiplayer() && (bool)Controller && (bool)Controller.photonView && !Controller.photonView.IsMine)
		{
			SoundDown.SpatialBlend = 1f;
			SoundUp.SpatialBlend = 1f;
		}
	}

	private void OnEnable()
	{
		if (Controller.PlayerAvatar.upgradeMapPlayerCount > 0)
		{
			HatchObject.SetActive(value: false);
			HatchLightObject.SetActive(value: true);
			active = true;
		}
		else
		{
			HatchObject.SetActive(value: true);
			HatchLightObject.SetActive(value: false);
			active = false;
		}
	}

	private void OnDisable()
	{
		PitchUpdated = 0;
		UpdateTimer = 0.8f;
	}

	private void Update()
	{
		if (!active)
		{
			return;
		}
		if (UpdateTimer <= 0f)
		{
			int num = 0;
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (!player.isDisabled)
				{
					num++;
				}
			}
			if (PlayerAmount != num)
			{
				float pitch = Mathf.Clamp(1f + (float)PitchUpdated * PitchIncrease, 1f, PitchMax);
				PitchUpdated++;
				if (PlayerAmount > num)
				{
					SoundDown.Pitch = pitch;
					SoundDown.Play(base.transform.position);
					PlayerAmount--;
				}
				else
				{
					SoundUp.Pitch = pitch;
					SoundUp.Play(base.transform.position);
					PlayerAmount++;
				}
				UpdateNumbers();
				UpdateTimer = UpdateTime - UpdateTimerDecrease;
				UpdateTimer = Mathf.Clamp(UpdateTimer, UpdateTimeMin, UpdateTime);
				UpdateTimerDecrease += UpdateTimeDecrease;
			}
			else
			{
				UpdateTimerDecrease = 0f;
			}
		}
		else
		{
			UpdateTimer -= Time.deltaTime;
		}
		if (NumberCurveLerp < 1f)
		{
			NumberCurveLerp += Time.deltaTime * NumberCurveSpeed;
			NumberCurveLerp = Mathf.Clamp01(NumberCurveLerp);
		}
		NumberText.transform.localPosition = new Vector3(NumberText.transform.localPosition.x, NumberCurve.Evaluate(NumberCurveLerp) * NumberCurveAmount, NumberText.transform.localPosition.z);
	}

	private void UpdateNumbers()
	{
		string text = NumberText.text;
		NumberText.text = PlayerAmount.ToString();
		if (text != NumberText.text)
		{
			NumberCurveLerp = 0f;
		}
	}
}
