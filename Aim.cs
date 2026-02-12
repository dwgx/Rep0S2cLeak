using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Aim : MonoBehaviour
{
	public enum State
	{
		Default,
		Grabbable,
		Grab,
		Rotate,
		Hidden,
		Climbable,
		Climb
	}

	[Serializable]
	public class AimState
	{
		public State State;

		public Sprite Sprite;

		public Color Color;
	}

	public static Aim instance;

	[Space]
	public AnimationCurve curveIntro;

	public AnimationCurve curveOutro;

	private float animLerp;

	[Space]
	public List<AimState> aimStates;

	private AimState defaultState;

	private Image image;

	private float stateTimer;

	private State currentState;

	private State previousState;

	private Sprite currentSprite;

	private Color currentColor;

	private void Awake()
	{
		instance = this;
		image = GetComponent<Image>();
		defaultState = aimStates[0];
	}

	private void Update()
	{
		if (stateTimer > 0f)
		{
			stateTimer -= 1f * Time.deltaTime;
		}
		else if (currentState != State.Default)
		{
			animLerp = 0f;
			currentState = State.Default;
			currentSprite = defaultState.Sprite;
			currentColor = defaultState.Color;
		}
		if (currentState == previousState)
		{
			if (animLerp < 1f)
			{
				animLerp += 10f * Time.deltaTime;
				base.transform.localScale = Vector3.one * curveOutro.Evaluate(animLerp);
			}
		}
		else
		{
			animLerp += 15f * Time.deltaTime;
			base.transform.localScale = Vector3.one * curveIntro.Evaluate(animLerp);
			if (animLerp >= 1f)
			{
				image.sprite = currentSprite;
				image.color = currentColor;
				previousState = currentState;
				animLerp = 0f;
			}
		}
		if (previousState == currentState)
		{
			if (currentState == State.Rotate)
			{
				base.transform.localRotation = Quaternion.Euler(0f, 0f, base.transform.localRotation.eulerAngles.z - 100f * Time.deltaTime);
			}
			else
			{
				base.transform.localRotation = Quaternion.identity;
			}
		}
	}

	public void SetState(State _state)
	{
		if (_state == currentState)
		{
			stateTimer = 0.25f;
		}
		else
		{
			if (currentState == State.Hidden)
			{
				return;
			}
			foreach (AimState aimState in aimStates)
			{
				if (aimState.State == _state)
				{
					currentState = aimState.State;
					currentSprite = aimState.Sprite;
					currentColor = aimState.Color;
					animLerp = 0f;
					stateTimer = 0.2f;
					break;
				}
			}
		}
	}
}
