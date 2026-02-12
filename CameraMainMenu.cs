using UnityEngine;

public class CameraMainMenu : CameraNoPlayerTarget
{
	public AnimationCurve introCurve;

	private float introLerp;

	protected override void Awake()
	{
		base.Awake();
		CameraNoise.Instance.AnimNoise.noiseStrengthDefault = 0.3f;
		CameraNoise.Instance.AnimNoise.noiseSpeedDefault = 4f;
	}

	protected override void Update()
	{
		base.Update();
		if (GameDirector.instance.currentState == GameDirector.gameState.Main && introLerp < 1f)
		{
			introLerp += 0.25f * Time.deltaTime;
			base.transform.localEulerAngles = new Vector3(Mathf.LerpUnclamped(0f, -45f, introCurve.Evaluate(introLerp)), 0f, 0f);
		}
	}
}
