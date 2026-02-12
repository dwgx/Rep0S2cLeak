using System.Collections;
using UnityEngine;

public class ItemUpgradePlayerTumbleWingsLogic : MonoBehaviour
{
	private enum State
	{
		Intro,
		Outro,
		Active,
		Inactive
	}

	private PhysGrabObject physGrabObject;

	private PlayerTumble playerTumble;

	public PlayerAvatar playerAvatar;

	private string steamID = "";

	private bool stateStart;

	public Transform transformWings;

	public Transform transformWingLeft;

	public Transform transformWingRight;

	private bool isLocal;

	public Sound soundWingsLoop;

	private Vector3 posPrev;

	private Vector3 posCurrent;

	private float targetSpeed;

	private float currentSpeed;

	public AudioSource localAudioSource;

	private bool lateStartDone;

	private float pitchSpeed = 1f;

	private bool fetchComplete;

	private float wingsSwitchCooldown;

	public Light lightWings;

	internal float tumbleWingTimer;

	private bool hasBeenGrounded = true;

	internal float tumbleWingPinkTimer;

	private bool isPink;

	private MeshRenderer wing1MeshRenderer;

	private MeshRenderer wing2MeshRenderer;

	private Color originalWingBaseColor;

	private Color originalWingFresnelColor;

	private Color lightOriginalColor;

	private State currentState;

	private void Start()
	{
		lightOriginalColor = lightWings.color;
		wing1MeshRenderer = transformWingLeft.GetComponentInChildren<MeshRenderer>();
		wing2MeshRenderer = transformWingRight.GetComponentInChildren<MeshRenderer>();
		originalWingBaseColor = wing1MeshRenderer.material.GetColor("_BaseColor");
		originalWingFresnelColor = wing1MeshRenderer.material.GetColor("_FresnelColor");
		tumbleWingTimer = 1f;
		currentState = State.Inactive;
		stateStart = true;
		StartCoroutine(LateStart());
	}

	private IEnumerator LateStart()
	{
		while (!LevelGenerator.Instance.Generated)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (!SemiFunc.RunIsLobbyMenu())
		{
			lateStartDone = true;
		}
	}

	private void StateMachine()
	{
		if (fetchComplete)
		{
			switch (currentState)
			{
			case State.Intro:
				StateIntro();
				break;
			case State.Outro:
				StateOutro();
				break;
			case State.Active:
				StateActive();
				break;
			case State.Inactive:
				StateInactive();
				break;
			}
		}
	}

	private void LoopSound()
	{
		if (!(playerAvatar.upgradeTumbleWings <= 0f) && (bool)playerAvatar && (bool)playerTumble && playerAvatar.playerAvatarVisuals.isActiveAndEnabled)
		{
			if (SemiFunc.FPSImpulse30())
			{
				posPrev = posCurrent;
				posCurrent = base.transform.position;
				targetSpeed = Vector3.Distance(posPrev, posCurrent) * 5f;
			}
			currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 2f);
			pitchSpeed = Mathf.Clamp(currentSpeed * 2f, 1f, 4f);
			soundWingsLoop.PlayLoop(playerAvatar.upgradeTumbleWingsVisualsActive, 2f, 2f, pitchSpeed);
		}
	}

	private void Update()
	{
		TumbleWingPinkTimerUpdate();
		if (!lateStartDone)
		{
			return;
		}
		if (fetchComplete && (bool)playerAvatar && playerAvatar.upgradeTumbleWingsVisualsActive && currentState == State.Inactive)
		{
			StateSet(State.Intro);
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && wingsSwitchCooldown > 0f)
		{
			wingsSwitchCooldown -= Time.deltaTime;
		}
		if (fetchComplete && (SemiFunc.IsMasterClientOrSingleplayer() || isLocal) && playerAvatar.upgradeTumbleWingsVisualsActive && tumbleWingTimer > 0f)
		{
			float num = 1f + playerAvatar.upgradeTumbleWings / 4f;
			tumbleWingTimer -= Time.deltaTime / num;
		}
		StateMachine();
		LoopSound();
		if (fetchComplete || !SemiFunc.FPSImpulse1() || (bool)playerTumble || !playerAvatar)
		{
			return;
		}
		playerTumble = playerAvatar.tumble;
		if ((bool)playerTumble)
		{
			steamID = playerAvatar.steamID;
			physGrabObject = playerTumble.physGrabObject;
			if (playerAvatar.isLocal)
			{
				isLocal = true;
			}
			playerAvatar.upgradeTumbleWingsLogic = this;
			fetchComplete = true;
			StateSet(State.Inactive);
			if (isLocal)
			{
				localAudioSource.enabled = true;
				TumbleWingsUI.instance.itemUpgradePlayerTumbleWingsLogic = this;
				soundWingsLoop.Source = localAudioSource;
			}
		}
	}

	public void WingsSetOriginalColors()
	{
		wing1MeshRenderer.material.SetColor("_FresnelColor", originalWingFresnelColor);
		wing2MeshRenderer.material.SetColor("_FresnelColor", originalWingFresnelColor);
		wing1MeshRenderer.material.SetColor("_BaseColor", originalWingBaseColor);
		wing2MeshRenderer.material.SetColor("_BaseColor", originalWingBaseColor);
		lightWings.color = lightOriginalColor;
	}

	public void WingsSetPinkColors()
	{
		wing1MeshRenderer.material.SetColor("_FresnelColor", new Color(1f, 0.1f, 0.4f, 1f));
		wing2MeshRenderer.material.SetColor("_FresnelColor", new Color(1f, 0.1f, 0.4f, 1f));
		wing1MeshRenderer.material.SetColor("_BaseColor", new Color(0.5f, 0.1f, 0.4f, 1f));
		wing2MeshRenderer.material.SetColor("_BaseColor", new Color(0.5f, 0.1f, 0.4f, 1f));
		lightWings.color = new Color(1f, 0.3f, 0.8f, 1f);
	}

	private void TumbleWingPinkTimerUpdate()
	{
		if (tumbleWingPinkTimer <= 0f && isPink)
		{
			isPink = false;
			tumbleWingPinkTimer = 0f;
			if (SemiFunc.IsMasterClientOrSingleplayer() && (bool)playerAvatar && playerAvatar.upgradeTumbleWingsVisualsActive)
			{
				playerAvatar.UpgradeTumbleWingsVisualsActive(_visualsActive: false, _pink: true);
				playerAvatar.upgradeTumbleWingsVisualsActive = false;
			}
		}
		if (tumbleWingPinkTimer > 0f)
		{
			if (!isPink)
			{
				isPink = true;
			}
			playerAvatar.upgradeTumbleWingsVisualsActive = true;
			tumbleWingPinkTimer -= Time.deltaTime;
		}
	}

	private void FixedUpdate()
	{
		if (tumbleWingPinkTimer > 0f || !fetchComplete || !lateStartDone || !SemiFunc.IsMasterClientOrSingleplayer() || !playerAvatar)
		{
			return;
		}
		float upgradeTumbleWings = playerAvatar.upgradeTumbleWings;
		if (!(upgradeTumbleWings > 0f))
		{
			return;
		}
		upgradeTumbleWings += 6f;
		if (playerTumble.isTumbling && (playerTumble.isPlayerInputTriggered || !playerTumble.tumbleOverride) && physGrabObject.playerGrabbing.Count == 0 && !SemiFunc.OnGroundCheck(playerTumble.transform.position, 1f, physGrabObject))
		{
			if (playerAvatar.upgradeTumbleWingsVisualsActive)
			{
				if (upgradeTumbleWings <= 15f && physGrabObject.timerZeroGravity <= 0f)
				{
					physGrabObject.rb.AddForceAtPosition(Vector3.up * (1.9f * upgradeTumbleWings), transformWings.position, ForceMode.Force);
				}
				else
				{
					physGrabObject.OverrideZeroGravity();
				}
				physGrabObject.rb.AddForceAtPosition(playerAvatar.localCamera.transform.forward * (0.02f * upgradeTumbleWings), transformWings.position, ForceMode.Impulse);
			}
			Quaternion targetRotation = Quaternion.LookRotation(playerAvatar.localCamera.transform.forward, Vector3.up);
			Vector3 torque = SemiFunc.PhysFollowRotation(physGrabObject.transform, targetRotation, physGrabObject.rb, 20f);
			physGrabObject.rb.AddTorque(torque, ForceMode.Impulse);
			if (!playerAvatar.upgradeTumbleWingsVisualsActive && wingsSwitchCooldown <= 0f && hasBeenGrounded)
			{
				playerAvatar.UpgradeTumbleWingsVisualsActive();
				playerAvatar.upgradeTumbleWingsVisualsActive = true;
				hasBeenGrounded = false;
				tumbleWingTimer = 1f;
				wingsSwitchCooldown = 2f;
			}
		}
		else
		{
			hasBeenGrounded = true;
			if (wingsSwitchCooldown <= 0f)
			{
				TurnOffWings();
			}
		}
		if (tumbleWingTimer <= 0f)
		{
			TurnOffWings();
		}
	}

	private void TurnOffWings()
	{
		if (playerAvatar.upgradeTumbleWingsVisualsActive)
		{
			playerAvatar.UpgradeTumbleWingsVisualsActive(_visualsActive: false);
			playerAvatar.upgradeTumbleWingsVisualsActive = false;
			wingsSwitchCooldown = 0.5f;
		}
	}

	private void StateIntro()
	{
		if (stateStart)
		{
			transformWings.gameObject.SetActive(value: true);
			transformWings.localScale = Vector3.zero;
			lightWings.intensity = 0f;
			transformWingLeft.localRotation = Quaternion.Euler(0f, 0f, 0f);
			transformWingRight.localRotation = Quaternion.Euler(0f, 0f, 0f);
			stateStart = false;
		}
		transformWings.localScale = Vector3.Lerp(transformWings.localScale, Vector3.one, Time.deltaTime * 10f);
		if (transformWings.localScale.x > 0.98f)
		{
			transformWings.localScale = Vector3.one;
			StateSet(State.Active);
		}
		lightWings.intensity = transformWings.localScale.x * 3f;
		FlapWings();
	}

	private void StateOutro()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		transformWings.localScale = Vector3.Lerp(transformWings.localScale, Vector3.zero, Time.deltaTime * 10f);
		if (transformWings.localScale.x < 0.02f)
		{
			transformWings.localScale = Vector3.zero;
			StateSet(State.Inactive);
		}
		lightWings.intensity = transformWings.localScale.x * 3f;
		FlapWings();
	}

	private void StateActive()
	{
		if (stateStart)
		{
			stateStart = false;
			lightWings.intensity = 2f;
		}
		if (!playerAvatar.upgradeTumbleWingsVisualsActive)
		{
			StateSet(State.Outro);
		}
		else
		{
			FlapWings();
		}
	}

	private void StateInactive()
	{
		if (stateStart)
		{
			transformWings.gameObject.SetActive(value: false);
			stateStart = false;
		}
	}

	private void FlapWings()
	{
		float num = -54f;
		float num2 = 40f;
		float num3 = Mathf.Sin(Time.time * num2) * num;
		transformWingLeft.localRotation = Quaternion.Euler(0f, -34f - num3, 0f);
		transformWingRight.localRotation = Quaternion.Euler(0f, 34f + num3, 0f);
	}

	private void StateSet(State newState)
	{
		if (currentState != newState)
		{
			currentState = newState;
			stateStart = true;
		}
	}
}
