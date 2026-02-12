using UnityEngine;

public class ItemCartLaserBuildupMeter : MonoBehaviour
{
	private enum State
	{
		Inactive,
		Buildup,
		Shooting,
		GoingBack
	}

	private ItemCartLaser itemCartLaser;

	private Light lightMeter;

	private MeshRenderer meshRenderer;

	private ItemCartCannonMain itemCartCannonMain;

	private AnimationCurve animationCurveBuildup;

	private State statePrevState = State.GoingBack;

	private State stateCurrent;

	private bool stateStart = true;

	private int cartLaserStatePrev;

	private int cartLaserStateCurrent;

	private void Start()
	{
		itemCartLaser = GetComponentInParent<ItemCartLaser>();
		itemCartCannonMain = GetComponentInParent<ItemCartCannonMain>();
		lightMeter = GetComponentInChildren<Light>();
		meshRenderer = GetComponentInChildren<MeshRenderer>();
		animationCurveBuildup = itemCartLaser.animationCurveBuildup;
	}

	private void Update()
	{
		cartLaserStatePrev = itemCartLaser.stateCurrent;
		if (cartLaserStateCurrent != itemCartLaser.stateCurrent)
		{
			cartLaserStateCurrent = itemCartLaser.stateCurrent;
			UpdateMeterState();
		}
		StateMachine();
	}

	private void StateMachine()
	{
		switch (stateCurrent)
		{
		case State.Inactive:
			StateInactive();
			break;
		case State.Buildup:
			StateBuildup();
			break;
		case State.Shooting:
			StateShooting();
			break;
		case State.GoingBack:
			StateGoingBack();
			break;
		}
	}

	private void LoopTexture()
	{
		meshRenderer.material.SetTextureOffset("_MainTex", new Vector2(0f, (0f - Time.time) * 20f));
		float y = 2f + 2f * base.transform.localScale.z;
		meshRenderer.material.SetTextureScale("_MainTex", new Vector2(1f, y));
	}

	private void LightIntensity()
	{
		float intensity = base.transform.localScale.z * 4f;
		lightMeter.intensity = intensity;
	}

	private void StateInactive()
	{
		if (stateStart)
		{
			stateStart = false;
			ToggleMeterEffect(_toggle: false);
		}
	}

	private void StateBuildup()
	{
		if (stateStart)
		{
			stateStart = false;
			ToggleMeterEffect(_toggle: true);
		}
		float z = animationCurveBuildup.Evaluate(itemCartCannonMain.stateTimer / itemCartCannonMain.stateTimerMax);
		base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, z);
		LoopTexture();
		LightIntensity();
	}

	private void StateShooting()
	{
		if (stateStart)
		{
			stateStart = false;
			ToggleMeterEffect(_toggle: true);
			base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, 1f);
		}
		lightMeter.intensity = 2f + Mathf.Sin(Time.time * 10f) * 2f;
		LoopTexture();
	}

	private void StateGoingBack()
	{
		if (stateStart)
		{
			stateStart = false;
			ToggleMeterEffect(_toggle: true);
		}
		float num = animationCurveBuildup.Evaluate(itemCartCannonMain.stateTimer / itemCartCannonMain.stateTimerMax);
		base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, 1f - num);
		LightIntensity();
		LoopTexture();
		if (num > 0.95f && lightMeter.enabled)
		{
			ToggleMeterEffect(_toggle: false);
		}
	}

	private void ToggleMeterEffect(bool _toggle)
	{
		meshRenderer.enabled = _toggle;
		lightMeter.enabled = _toggle;
	}

	private void UpdateMeterState()
	{
		if (cartLaserStateCurrent == 0 || cartLaserStateCurrent == 1)
		{
			StateSet(State.Inactive);
		}
		if (cartLaserStateCurrent == 2)
		{
			StateSet(State.Buildup);
		}
		if (cartLaserStateCurrent == 3)
		{
			StateSet(State.Shooting);
		}
		if (cartLaserStateCurrent == 4)
		{
			StateSet(State.GoingBack);
		}
	}

	private void StateSet(State _state)
	{
		if (stateCurrent != statePrevState && stateCurrent != _state)
		{
			statePrevState = stateCurrent;
			stateCurrent = _state;
			stateStart = true;
		}
	}
}
