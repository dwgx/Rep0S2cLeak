using UnityEngine;

public class ValuableBoombox : Trap
{
	[Header("Sounds")]
	public Sound soundBoomboxStart;

	public Sound soundBoomboxStop;

	public Sound soundBoomboxMusic;

	[Space]
	public Transform speaker1;

	public Transform speaker2;

	[Space]
	public Light light1;

	public Light light2;

	protected override void Start()
	{
		base.Start();
		light1.enabled = false;
		light2.enabled = false;
	}

	protected override void Update()
	{
		base.Update();
		if (!trapTriggered && physGrabObject.grabbed)
		{
			trapStart = true;
		}
		if (physGrabObject.grabbed)
		{
			soundBoomboxMusic.PlayLoop(playing: true, 2f, 2f);
			enemyInvestigate = true;
			float num = 1f + Mathf.Sin(Time.time * 80f) * 0.005f;
			speaker1.localScale = new Vector3(num, num, num);
			speaker2.localScale = new Vector3(num, num, num);
			float h = Mathf.PingPong(Time.time * 0.5f, 1f);
			float num2 = 1f;
			if (SemiFunc.Photosensitivity())
			{
				h = 0.75f;
				num2 = 0.5f;
			}
			Color color = Color.Lerp(speaker1.GetComponent<Renderer>().material.GetColor("_EmissionColor"), Color.HSVToRGB(h, 1f, 1f), Time.deltaTime * 6000f);
			speaker1.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
			speaker2.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
			light1.enabled = true;
			light2.enabled = true;
			light1.color = color;
			light2.color = color;
			light1.intensity = Mathf.Lerp(light1.intensity, 4f, num2 * Time.deltaTime);
			light2.intensity = Mathf.Lerp(light2.intensity, 4f, num2 * Time.deltaTime);
		}
		else
		{
			soundBoomboxMusic.PlayLoop(playing: false, 2f, 2f);
			if (light1.enabled)
			{
				Color value = Color.Lerp(speaker1.GetComponent<Renderer>().material.GetColor("_EmissionColor"), Color.black, Time.deltaTime);
				speaker1.GetComponent<Renderer>().material.SetColor("_EmissionColor", value);
				speaker2.GetComponent<Renderer>().material.SetColor("_EmissionColor", value);
				speaker1.localScale = Vector3.Lerp(speaker1.localScale, Vector3.one, Time.deltaTime);
				speaker2.localScale = Vector3.Lerp(speaker2.localScale, Vector3.one, Time.deltaTime);
				light1.intensity = Mathf.Lerp(light1.intensity, 0f, Time.deltaTime);
				light2.intensity = Mathf.Lerp(light2.intensity, 0f, Time.deltaTime);
				if (light1.intensity < 0.01f)
				{
					speaker1.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
					speaker2.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
					light1.enabled = false;
					light2.enabled = false;
					speaker1.localScale = Vector3.one;
					speaker2.localScale = Vector3.one;
				}
			}
		}
		if (!physGrabObject.grabbed)
		{
			return;
		}
		float amount = Mathf.Sin(Time.time * 15f) * 0.5f;
		float num3 = Mathf.Sin(Time.time * 15f) * 25f;
		foreach (PhysGrabber item in physGrabObject.playerGrabbing)
		{
			if (item.isLocal)
			{
				CameraAim.Instance.AdditiveAimY(amount);
				item.OverrideGrabDistance(1f);
				item.OverrideDisableRotationControls();
				item.playerAvatar.playerExpression.OverrideExpressionSet(4, 100f);
				PlayerExpressionsUI.instance.playerExpression.OverrideExpressionSet(4, 100f);
				PlayerExpressionsUI.instance.playerAvatarVisuals.HeadTiltOverride(num3 * 0.5f);
			}
			else
			{
				item.playerAvatar.playerAvatarVisuals.HeadTiltOverride(num3);
			}
		}
	}
}
