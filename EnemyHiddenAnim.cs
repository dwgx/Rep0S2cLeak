using UnityEngine;

public class EnemyHiddenAnim : MonoBehaviour
{
	public enum BreathingState
	{
		None,
		Slow,
		Medium,
		Fast,
		FastNoSound
	}

	public enum FootstepState
	{
		None,
		Standing,
		TwoStep,
		Moving,
		Sprinting,
		TimedSteps
	}

	private BreathingState breathingState;

	private FootstepState footstepState;

	[Space]
	public Enemy enemy;

	public EnemyHidden enemyHidden;

	internal Materials.MaterialTrigger material = new Materials.MaterialTrigger();

	[Space]
	public ParticleSystem particleBreath;

	public ParticleSystem particleBreathFast;

	public ParticleSystem particleBreathConstant;

	private bool breathingCurrent;

	private float breathingTimer;

	[Space]
	public Transform transformFoot;

	public ParticleSystem particleFootstepShapeRight;

	public ParticleSystem particleFootstepShapeLeft;

	public ParticleSystem particleFootstepSmoke;

	private Vector3 footstepPositionPrevious;

	private Vector3 footstepPositionPreviousRight;

	private Vector3 footstepPositionPreviousLeft;

	private int footstepCurrent = 1;

	private float movingTimer;

	private float stopStepTimer;

	private float timedStepsTimer;

	private bool jumpStartImpulse = true;

	private bool jumpStopImpulse;

	[Space]
	public Sound soundBreatheIn;

	public Sound soundBreatheOut;

	[Space]
	public Sound soundBreatheInFast;

	public Sound soundBreatheOutFast;

	[Space]
	public Sound soundFootstep;

	public Sound soundFootstepSprint;

	[Space]
	public Sound soundStunStart;

	private bool soundStunStartImpulse;

	public Sound soundStunLoop;

	public Sound soundStunStop;

	private bool soundStunStopImpulse;

	private float soundStunPauseTimer;

	[Space]
	public Sound soundJump;

	private bool soundJumpImpulse;

	public Sound soundLand;

	private bool soundLandImpulse;

	[Space]
	public Sound soundPlayerPickup;

	private bool soundPlayerPickupImpulse;

	public Sound soundPlayerRelease;

	private bool soundPlayerReleaseImpulse;

	public Sound soundPlayerMove;

	public Sound soundPlayerMoveStop;

	private bool soundPlayerMoveImpulse;

	[Space]
	public Sound soundHurt;

	public Sound soundDeath;

	private void Update()
	{
		BreathingLogic();
		FootstepLogic();
		if (enemyHidden.currentState == EnemyHidden.State.Stun)
		{
			if (soundStunStartImpulse)
			{
				StopBreathing();
				soundStunStart.Play(particleBreath.transform.position);
				soundStunStartImpulse = false;
			}
			if (soundStunPauseTimer > 0f)
			{
				if (soundStunStopImpulse)
				{
					soundStunStop.Play(particleBreath.transform.position);
					soundStunStopImpulse = false;
				}
				soundStunLoop.PlayLoop(playing: false, 2f, 5f);
				particleBreathConstant.Stop();
			}
			else
			{
				soundStunLoop.PlayLoop(playing: true, 2f, 10f);
				particleBreathConstant.Play();
				soundStunStopImpulse = true;
			}
		}
		else
		{
			if (soundStunStopImpulse)
			{
				soundStunStop.Play(particleBreath.transform.position);
				soundStunStopImpulse = false;
			}
			soundStunLoop.PlayLoop(playing: false, 2f, 5f);
			particleBreathConstant.Stop();
			soundStunStartImpulse = true;
		}
		if (soundStunPauseTimer > 0f)
		{
			soundStunPauseTimer -= Time.deltaTime;
		}
		if (enemy.Jump.jumping)
		{
			if (soundJumpImpulse)
			{
				particleBreath.Play();
				soundJump.Play(particleBreath.transform.position);
				StopBreathing();
				soundJumpImpulse = false;
			}
			soundLandImpulse = true;
		}
		else
		{
			if (soundLandImpulse)
			{
				particleBreathFast.Play();
				soundLand.Play(particleBreath.transform.position);
				StopBreathing();
				soundLandImpulse = false;
			}
			soundJumpImpulse = true;
		}
		if (enemyHidden.currentState == EnemyHidden.State.PlayerPickup)
		{
			if (soundPlayerPickupImpulse)
			{
				StopBreathing();
				particleBreath.Play();
				soundPlayerPickup.Play(particleBreath.transform.position);
				soundPlayerPickupImpulse = false;
			}
		}
		else
		{
			soundPlayerPickupImpulse = true;
		}
		if (enemyHidden.currentState == EnemyHidden.State.PlayerReleaseWait)
		{
			if (soundPlayerReleaseImpulse)
			{
				StopBreathing();
				particleBreath.Play();
				soundPlayerRelease.Play(particleBreath.transform.position);
				soundPlayerReleaseImpulse = false;
			}
		}
		else
		{
			soundPlayerReleaseImpulse = true;
		}
		if (enemyHidden.currentState == EnemyHidden.State.PlayerMove && !enemy.Jump.jumping)
		{
			soundPlayerMove.PlayLoop(playing: true, 2f, 10f);
			soundPlayerMoveImpulse = true;
			return;
		}
		if (soundPlayerMoveImpulse)
		{
			soundPlayerMoveStop.Play(particleBreath.transform.position);
			soundPlayerMoveImpulse = false;
		}
		soundPlayerMove.PlayLoop(playing: false, 2f, 10f);
	}

	private void BreathingLogic()
	{
		if (enemy.Jump.jumping || enemyHidden.currentState == EnemyHidden.State.Stun || enemyHidden.currentState == EnemyHidden.State.PlayerRelease || enemyHidden.currentState == EnemyHidden.State.PlayerReleaseWait || enemyHidden.currentState == EnemyHidden.State.PlayerPickup)
		{
			breathingState = BreathingState.None;
		}
		else if (enemyHidden.currentState == EnemyHidden.State.PlayerMove)
		{
			breathingState = BreathingState.FastNoSound;
		}
		else if (enemyHidden.currentState == EnemyHidden.State.PlayerGoTo || enemyHidden.currentState == EnemyHidden.State.Leave)
		{
			breathingState = BreathingState.Fast;
		}
		else if (enemyHidden.currentState == EnemyHidden.State.Roam || enemyHidden.currentState == EnemyHidden.State.Investigate)
		{
			breathingState = BreathingState.Medium;
		}
		else
		{
			breathingState = BreathingState.Slow;
		}
		if (breathingState == BreathingState.None)
		{
			soundBreatheIn.Stop();
			soundBreatheOut.Stop();
		}
		if (breathingTimer <= 0f)
		{
			if (breathingCurrent)
			{
				breathingCurrent = false;
				if (breathingState != BreathingState.FastNoSound)
				{
					if (breathingState == BreathingState.Fast)
					{
						soundBreatheInFast.Play(particleBreath.transform.position);
					}
					else
					{
						soundBreatheIn.Play(particleBreath.transform.position);
					}
				}
				else
				{
					particleBreathFast.Play();
				}
				breathingTimer = 3f;
			}
			else
			{
				breathingCurrent = true;
				if (breathingState != BreathingState.FastNoSound)
				{
					if (breathingState == BreathingState.Fast)
					{
						soundBreatheOutFast.Play(particleBreath.transform.position);
					}
					else
					{
						soundBreatheOut.Play(particleBreath.transform.position);
					}
					particleBreath.Play();
				}
				else
				{
					particleBreathFast.Play();
				}
				breathingTimer = 4.5f;
			}
		}
		if (breathingState == BreathingState.Slow)
		{
			breathingTimer -= 1f * Time.deltaTime;
		}
		else if (breathingState == BreathingState.Medium)
		{
			breathingTimer -= 2f * Time.deltaTime;
		}
		else
		{
			breathingTimer -= 5f * Time.deltaTime;
		}
	}

	private void FootstepLogic()
	{
		if (movingTimer > 0f)
		{
			movingTimer -= Time.deltaTime;
		}
		if ((enemyHidden.currentState == EnemyHidden.State.Roam || enemyHidden.currentState == EnemyHidden.State.Investigate || enemyHidden.currentState == EnemyHidden.State.PlayerGoTo || enemyHidden.currentState == EnemyHidden.State.PlayerMove || enemyHidden.currentState == EnemyHidden.State.Leave) && enemy.Rigidbody.velocity.magnitude > 0.5f)
		{
			movingTimer = 0.25f;
		}
		if (enemyHidden.currentState == EnemyHidden.State.Stun || enemy.Jump.jumping)
		{
			footstepState = FootstepState.None;
		}
		else if (enemyHidden.currentState == EnemyHidden.State.StunEnd || enemyHidden.currentState == EnemyHidden.State.PlayerNotice)
		{
			footstepState = FootstepState.TimedSteps;
		}
		else if (movingTimer > 0f)
		{
			if (enemyHidden.currentState == EnemyHidden.State.PlayerGoTo || enemyHidden.currentState == EnemyHidden.State.PlayerMove || enemyHidden.currentState == EnemyHidden.State.Leave)
			{
				footstepState = FootstepState.Sprinting;
			}
			else
			{
				footstepState = FootstepState.Moving;
			}
		}
		else if (footstepState == FootstepState.Moving)
		{
			footstepState = FootstepState.TwoStep;
		}
		else if (footstepState != FootstepState.TwoStep)
		{
			footstepState = FootstepState.Standing;
		}
		if (enemy.Jump.jumping)
		{
			if (jumpStartImpulse)
			{
				jumpStopImpulse = true;
				jumpStartImpulse = false;
				FootstepSet();
				FootstepSet();
			}
		}
		else if (jumpStopImpulse)
		{
			jumpStopImpulse = false;
			jumpStartImpulse = true;
			FootstepSet();
			FootstepSet();
		}
		if ((footstepState == FootstepState.Moving || footstepState == FootstepState.Sprinting) && Vector3.Distance(transformFoot.position, footstepPositionPrevious) > 1f)
		{
			FootstepSet();
		}
		if (footstepState == FootstepState.TimedSteps)
		{
			if (timedStepsTimer <= 0f)
			{
				timedStepsTimer = 0.25f;
				FootstepSet();
			}
			else
			{
				timedStepsTimer -= Time.deltaTime;
			}
		}
		else
		{
			timedStepsTimer = 0f;
		}
		if (footstepState == FootstepState.TwoStep)
		{
			if (stopStepTimer == -1f)
			{
				FootstepSet();
				stopStepTimer = 0.25f;
				return;
			}
			stopStepTimer -= Time.deltaTime;
			if (stopStepTimer <= 0f)
			{
				footstepState = FootstepState.Standing;
				FootstepSet();
				stopStepTimer = -1f;
			}
		}
		else
		{
			stopStepTimer = -1f;
		}
	}

	private void FootstepSet()
	{
		Vector3 vector = transformFoot.right * (-0.3f * (float)footstepCurrent);
		Vector3 vector2 = Random.insideUnitSphere * 0.15f;
		vector2.y = 0f;
		if (Physics.Raycast(transformFoot.position + vector + vector2, Vector3.down * 2f, out var hitInfo, 3f, LayerMask.GetMask("Default")))
		{
			ParticleSystem particleSystem = particleFootstepShapeRight;
			Vector3 vector3 = footstepPositionPreviousRight;
			if (footstepCurrent == 1)
			{
				particleSystem = particleFootstepShapeLeft;
				vector3 = footstepPositionPreviousLeft;
			}
			if (Vector3.Distance(vector3, hitInfo.point) > 0.2f)
			{
				particleSystem.transform.position = hitInfo.point + Vector3.up * 0.02f;
				particleSystem.transform.eulerAngles = new Vector3(0f, transformFoot.eulerAngles.y, 0f);
				particleSystem.Play();
				particleFootstepSmoke.transform.position = particleSystem.transform.position;
				particleFootstepSmoke.transform.rotation = particleSystem.transform.rotation;
				particleFootstepSmoke.Play();
				Materials.Instance.Impulse(particleSystem.transform.position + Vector3.up * 0.5f, Vector3.down, Materials.SoundType.Medium, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
				if (footstepState == FootstepState.Sprinting)
				{
					soundFootstepSprint.Play(particleSystem.transform.position);
				}
				else
				{
					soundFootstep.Play(particleSystem.transform.position);
				}
				if (footstepCurrent == 1)
				{
					footstepPositionPreviousLeft = hitInfo.point;
				}
				else
				{
					footstepPositionPreviousRight = hitInfo.point;
				}
				footstepCurrent *= -1;
			}
		}
		footstepPositionPrevious = transformFoot.position;
	}

	public void StopBreathing()
	{
		soundBreatheIn.Stop();
		soundBreatheInFast.Stop();
		soundBreatheOut.Stop();
		soundBreatheOutFast.Stop();
	}

	public void StunPause()
	{
		soundStunPauseTimer = 1f;
	}

	public void Hurt()
	{
		StopBreathing();
		StunPause();
		soundHurt.Play(particleBreath.transform.position);
	}

	public void Death()
	{
		particleBreathConstant.Stop();
		StopBreathing();
		StunPause();
		soundDeath.Play(particleBreath.transform.position);
	}
}
