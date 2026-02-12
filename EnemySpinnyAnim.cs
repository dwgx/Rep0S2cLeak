using UnityEngine;

public class EnemySpinnyAnim : MonoBehaviour
{
	[Header("Scripts")]
	public Enemy enemy;

	public EnemySpinny enemySpinny;

	[Header("Particles")]
	public ParticleSystem spitDroplets;

	public ParticleSystem spitSplash;

	public ParticleSystem spawnParticles;

	public ParticleSystem moneyParticle;

	public ParticleSystem lockInParticles;

	public ParticleSystem RunSpitParticleSystem;

	public EnemySpinnyParticle healParticle;

	public EnemySpinnyParticle hurtParticle;

	public EnemySpinnyParticle deathParticle;

	public EnemySpinnyParticle fullHealParticle;

	public EnemySpinnyParticle moneyPrizeParticle;

	public EnemySpinnyParticle interruptParticle;

	public ParticleSystem lightningParticle;

	public ParticleSystem healingSmokeParticle;

	public ParticleSystem smallhurtParticle;

	public ParticleSystem fullHealSmokeParticle;

	public ParticleSystem[] deathParticles;

	public GameObject interruptParticlePrefab;

	[Header("Enemy components")]
	public Transform tonguePivot;

	public Transform closedHeadPivot;

	public Transform openHeadPivot;

	public Transform playerBall;

	[Space]
	public Light pointLight;

	[Header("Roulette animation curve")]
	public AnimationCurve progressOverTime = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[Header("Springs")]
	public SpringQuaternion springTopTongue = new SpringQuaternion();

	public SpringQuaternion SpringBaseTongueQuaternion = new SpringQuaternion();

	public SpringQuaternion closedTongueSpring = new SpringQuaternion();

	public SpringQuaternion headSpring = new SpringQuaternion();

	public Transform tongueTopTarget;

	public Transform tongueBaseTarget;

	public Transform tongueBase;

	public Transform tongueTop;

	public Transform closedTongueTarget;

	public Transform closedTongue;

	public Transform headTarget;

	public Transform head1;

	public Transform head2;

	public Transform headANIM;

	[Header("Sounds")]
	public Sound openMouthSound;

	public Sound closeMouthSound;

	public Sound tongueHitSound;

	public Sound noticeSound;

	public Sound jingleGreen;

	public Sound jingleRed;

	public Sound jingleBlackGlobal;

	public Sound jingleBlack;

	public Sound jingleWhite;

	public Sound jingleYellow;

	public Sound idleLoop;

	public Sound runningLoop;

	public Sound tickSound;

	public Sound rouletteLoop;

	public Sound footStep;

	public Sound hurtSound;

	public Sound deathSound;

	public Sound soundMove;

	public Sound soundMoveHead;

	public Sound muffledSongLoop;

	public Sound leaveSongSound;

	public Sound songLoop;

	public Sound interruptionSound;

	public Sound deathJingleSound;

	public Sound jumpSound;

	public Sound landSound;

	[HideInInspector]
	public bool mouthOpened;

	internal Materials.MaterialTrigger material = new Materials.MaterialTrigger();

	[HideInInspector]
	public Animator animator;

	private bool spinning;

	private bool noticed;

	private float startAngleDegrees;

	private float totalDegreesToTravel;

	private float elapsedSeconds;

	private float currentAngleDegrees;

	private float integratedCurveArea;

	private float speedScaleFactor;

	private float moveSoundRigidBodyTimer;

	private float moveSoundHeadTimer;

	private float muffledSongTimer;

	private int lastSliceIndex;

	private Quaternion prevHeadLookRotation;

	private Quaternion prevRigidBodyRotation;

	private int lastBorderAngle = -999;

	private Transform spinnyWheel => enemySpinny.spinnyWheel;

	private Transform headLook => headANIM;

	private Transform[] pieces => enemySpinny.pieces;

	private float spinDurationSeconds => enemySpinny.spinDurationSeconds;

	private Rigidbody rb => enemy.Rigidbody.rb;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
	}

	private void Update()
	{
		TurnSoundLogic();
		AnimateSprings();
		bool playing = enemySpinny.currentState == EnemySpinny.State.Roam || enemySpinny.currentState == EnemySpinny.State.Investigate || enemySpinny.currentState == EnemySpinny.State.WaitForRoulette;
		bool playing2 = enemySpinny.currentState == EnemySpinny.State.GoToPlayer;
		bool playing3 = enemySpinny.currentState == EnemySpinny.State.Roulette && spinDurationSeconds - elapsedSeconds > 3f;
		bool playing4 = enemySpinny.currentState == EnemySpinny.State.GoToPlayer;
		bool playing5 = enemySpinny.currentState == EnemySpinny.State.Leave && muffledSongTimer > 0f;
		bool playing6 = enemySpinny.RouletteGoingOn();
		muffledSongLoop.PlayLoop(playing4, 0.6f, 1f);
		leaveSongSound.PlayLoop(playing5, 1f, 1f);
		songLoop.PlayLoop(playing6, 0.8f, 0.6f);
		idleLoop.PlayLoop(playing, 1f, 0.8f);
		runningLoop.PlayLoop(playing2, 1f, 0.8f);
		rouletteLoop.PlayLoop(playing3, 1f, 0.6f);
		if (enemySpinny.currentState == EnemySpinny.State.PlayerNotice || enemySpinny.currentState == EnemySpinny.State.GoToPlayer || enemySpinny.RouletteGoingOn())
		{
			Quaternion quaternion = Quaternion.LookRotation(enemySpinny.playerTarget.transform.position - closedHeadPivot.position);
			Vector3 eulerAngles = (Quaternion.Inverse(closedHeadPivot.parent.rotation) * quaternion).eulerAngles;
			eulerAngles.x = ((eulerAngles.x > 180f) ? (eulerAngles.x - 360f) : eulerAngles.x);
			eulerAngles.y = ((eulerAngles.y > 180f) ? (eulerAngles.y - 360f) : eulerAngles.y);
			eulerAngles.x = Mathf.Clamp(eulerAngles.x, -30f, 30f);
			eulerAngles.y = Mathf.Clamp(eulerAngles.y, -45f, 45f);
			Quaternion quaternion2 = Quaternion.Euler(eulerAngles);
			Quaternion quaternion3 = closedHeadPivot.parent.rotation * quaternion2;
			closedHeadPivot.rotation = Quaternion.Slerp(closedHeadPivot.rotation, quaternion3, Time.deltaTime * 5f);
			openHeadPivot.rotation = Quaternion.Slerp(openHeadPivot.rotation, quaternion3, Time.deltaTime * 5f);
		}
		else
		{
			closedHeadPivot.localRotation = Quaternion.Slerp(closedHeadPivot.localRotation, Quaternion.identity, Time.deltaTime * 5f);
			openHeadPivot.localRotation = Quaternion.Slerp(openHeadPivot.localRotation, Quaternion.identity, Time.deltaTime * 5f);
		}
		if (enemy.Rigidbody.velocity.magnitude > 0.3f && !enemySpinny.RouletteGoingOn() && enemySpinny.currentState != EnemySpinny.State.Idle && enemySpinny.currentState != EnemySpinny.State.PlayerNotice)
		{
			animator.SetBool("move", value: true);
		}
		else
		{
			animator.SetBool("move", value: false);
		}
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk cycle"))
		{
			if (enemySpinny.currentState == EnemySpinny.State.Roam)
			{
				animator.speed = 1.35f;
			}
			else if (enemySpinny.currentState == EnemySpinny.State.Investigate)
			{
				animator.speed = 1.3f;
			}
			else if (enemySpinny.currentState == EnemySpinny.State.GoToPlayer)
			{
				animator.speed = 2.5f;
			}
			else if (enemySpinny.currentState == EnemySpinny.State.Leave)
			{
				animator.speed = 3.5f;
			}
			else
			{
				animator.speed = 1f;
			}
		}
		else
		{
			animator.speed = 1f;
		}
		if (!enemy.IsStunned() && (enemy.Jump.jumping || enemy.Jump.jumpingDelay))
		{
			animator.SetBool("jump", value: true);
		}
		else
		{
			animator.SetBool("jump", value: false);
		}
		if (enemySpinny.currentState == EnemySpinny.State.PlayerNotice && !noticed)
		{
			animator.SetTrigger("player_notice");
			noticed = true;
		}
		else if (enemySpinny.currentState != EnemySpinny.State.PlayerNotice)
		{
			noticed = false;
		}
		if (enemySpinny.currentState == EnemySpinny.State.GoToPlayer)
		{
			RunSpitParticleSystem.Play();
		}
		else
		{
			RunSpitParticleSystem.Stop();
		}
		if (enemy.IsStunned())
		{
			if (!animator.GetBool("stun"))
			{
				animator.SetTrigger("stun_trigger");
			}
			animator.SetBool("stun", value: true);
		}
		else
		{
			animator.SetBool("stun", value: false);
		}
		if (enemy.CurrentState == EnemyState.Despawn)
		{
			if (!animator.GetBool("despawn"))
			{
				spawnParticles.Play();
			}
			animator.SetBool("despawn", value: true);
		}
		else
		{
			animator.SetBool("despawn", value: false);
		}
		if (enemySpinny.RouletteGoingOn())
		{
			playerBall.position = enemySpinny.playerTarget.tumble.rb.position;
			lightningParticle.transform.position = enemySpinny.playerTarget.tumble.rb.position;
			lockInParticles.transform.position = enemySpinny.playerTarget.tumble.rb.position;
			healingSmokeParticle.transform.position = enemySpinny.playerTarget.tumble.rb.position;
			smallhurtParticle.transform.position = enemySpinny.playerTarget.tumble.rb.position;
			fullHealSmokeParticle.transform.position = enemySpinny.playerTarget.tumble.rb.position;
			if (enemySpinny.currentState == EnemySpinny.State.WaitForRoulette)
			{
				playerBall.gameObject.SetActive(value: true);
				if (!animator.GetBool("open_mouth_bool"))
				{
					TurnOnAllColors();
					if (enemySpinny.playerTarget.isLocal)
					{
						AudioScare.instance.PlaySoft();
					}
				}
				animator.SetBool("close_mouth_bool", value: false);
				animator.SetBool("open_mouth_bool", value: true);
			}
			else if (enemySpinny.currentState == EnemySpinny.State.Roulette)
			{
				if (!spinning)
				{
					PlaySpitParticles();
					BeginningOfRoulette();
					if (!enemySpinny.playerTarget.isLocal)
					{
						lockInParticles.Play(withChildren: true);
					}
					animator.SetBool("open_mouth_bool", value: false);
					spinning = true;
				}
				StepSpinFrame();
				LightUpWheel();
				if (spinDurationSeconds - elapsedSeconds < 3f)
				{
					StopSpitParticles();
				}
				if (spinDurationSeconds - elapsedSeconds < 0.3f)
				{
					LerpTongueBack();
				}
				else
				{
					AnimateTongue();
				}
			}
			else if (enemySpinny.currentState == EnemySpinny.State.RouletteEndPause)
			{
				spinnyWheel.localRotation = Quaternion.Slerp(spinnyWheel.localRotation, Quaternion.Euler(0f, 0f, enemySpinny.targetAngleDegrees), Time.deltaTime * 5f);
				StopSpitParticles();
				LerpTongueBack();
			}
			else if (enemySpinny.currentState == EnemySpinny.State.RouletteEnd)
			{
				if (spinning)
				{
					spinning = false;
					TurnPiecesColor(enemySpinny.GetCurrentColorColor(enemySpinny.targetAngleDegrees));
					TurnLightColor(enemySpinny.GetCurrentColorColor(enemySpinny.targetAngleDegrees));
				}
				LerpTongueBack();
			}
		}
		else if (enemySpinny.currentState == EnemySpinny.State.Spawn || enemySpinny.currentState == EnemySpinny.State.CloseMouth || enemySpinny.currentState == EnemySpinny.State.Stunned)
		{
			playerBall.gameObject.SetActive(value: false);
			lockInParticles.Stop(withChildren: true);
			pointLight.enabled = false;
			if (!animator.GetBool("close_mouth_bool") || enemySpinny.currentState == EnemySpinny.State.CloseMouth)
			{
				muffledSongTimer = 5f;
			}
			animator.SetBool("close_mouth_bool", value: true);
			animator.SetBool("open_mouth_bool", value: false);
			StopSpitParticles();
			spinning = false;
		}
		else if (enemySpinny.currentState == EnemySpinny.State.Leave)
		{
			muffledSongTimer -= Time.deltaTime;
			if (animator.GetBool("close_mouth_bool"))
			{
				animator.SetBool("close_mouth_bool", value: false);
			}
		}
	}

	private void LightUpWheel()
	{
		float rotation = enemySpinny.GetRotation360();
		(int, EnemySpinny.Colors) sliceAndColour = enemySpinny.GetSliceAndColour(rotation);
		if (sliceAndColour.Item1 == lastSliceIndex)
		{
			return;
		}
		(lastSliceIndex, _) = sliceAndColour;
		TurnOnAllColors();
		for (int i = 0; i < pieces.Length; i++)
		{
			MeshRenderer component = pieces[i].GetComponent<MeshRenderer>();
			if (i == sliceAndColour.Item1)
			{
				component.material.EnableKeyword("_EMISSION");
				component.material.SetColor("_EmissionColor", enemySpinny.GetCurrentColorColor());
			}
		}
	}

	private void AnimateSprings()
	{
		tongueBase.rotation = SemiFunc.SpringQuaternionGet(SpringBaseTongueQuaternion, tongueBaseTarget.rotation);
		tongueTop.rotation = SemiFunc.SpringQuaternionGet(springTopTongue, tongueTopTarget.rotation);
		closedTongue.rotation = SemiFunc.SpringQuaternionGet(closedTongueSpring, closedTongueTarget.rotation);
		head1.rotation = SemiFunc.SpringQuaternionGet(headSpring, headTarget.rotation);
		head2.rotation = SemiFunc.SpringQuaternionGet(headSpring, headTarget.rotation);
		headTarget.localRotation = headANIM.localRotation;
	}

	private void AnimateTongue()
	{
		float rotation = enemySpinny.GetRotation360();
		float[] sliceBorders = enemySpinny.SliceBorders;
		for (int i = 0; i < sliceBorders.Length; i++)
		{
			int num = (int)sliceBorders[i];
			float num2 = Mathf.Abs(rotation - (float)num);
			if (num2 <= 20f && rotation > (float)num)
			{
				tonguePivot.localRotation = Quaternion.Euler(0f, 0f, num2);
				if (num != lastBorderAngle)
				{
					lastBorderAngle = num;
					tongueHitSound.Play(base.transform.position);
					tickSound.Play(base.transform.position);
				}
				return;
			}
		}
		LerpTongueBack();
	}

	private void LerpTongueBack()
	{
		tonguePivot.localRotation = Quaternion.identity;
	}

	private void BeginningOfRoulette()
	{
		startAngleDegrees = spinnyWheel.localEulerAngles.z;
		totalDegreesToTravel = (float)enemySpinny.extraSpins * 360f + GetSmallestPositiveDelta(startAngleDegrees, enemySpinny.targetAngleDegrees);
		elapsedSeconds = 0f;
	}

	private void StepSpinFrame()
	{
		elapsedSeconds += Time.deltaTime;
		float num = elapsedSeconds / spinDurationSeconds;
		if (num >= 1f)
		{
			spinnyWheel.localRotation = Quaternion.Euler(0f, 0f, enemySpinny.targetAngleDegrees);
			return;
		}
		float num2 = progressOverTime.Evaluate(Mathf.Clamp01(num));
		float num3 = progressOverTime.Evaluate(0f);
		float num4 = progressOverTime.Evaluate(1f);
		float num5 = (num2 - num3) / (num4 - num3);
		float z = Mathf.Repeat(startAngleDegrees + totalDegreesToTravel * num5, 360f);
		spinnyWheel.localRotation = Quaternion.Euler(0f, 0f, z);
	}

	public void HurtSound()
	{
		hurtSound.Play(base.transform.position);
	}

	public void DeathSound()
	{
		deathSound.Play(base.transform.position);
	}

	public void FootStepSound()
	{
		footStep.Play(base.transform.position);
		MaterialSoundFootStep();
	}

	public void MaterialSound()
	{
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.5f, Vector3.down, Materials.SoundType.Light, footstep: false, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void MaterialSoundFootStep()
	{
		Materials.Instance.Impulse(enemy.Rigidbody.physGrabObject.centerPoint + Vector3.down * 0.5f, Vector3.down, Materials.SoundType.Light, footstep: true, footstepParticles: true, material, Materials.HostType.Enemy);
	}

	public void MoveSmall()
	{
		soundMoveHead.Play(base.transform.position);
	}

	public void MoveBig()
	{
		soundMove.Play(base.transform.position);
	}

	public void OpenMouthSound()
	{
		openMouthSound.Play(base.transform.position);
	}

	public void CloseMouthSound()
	{
		closeMouthSound.Play(base.transform.position);
	}

	public void NoticeSound()
	{
		noticeSound.Play(enemy.transform.position);
	}

	public void JumpSound()
	{
		jumpSound.Play(base.transform.position);
	}

	public void LandSound()
	{
		landSound.Play(base.transform.position);
	}

	private void TurnSoundLogic()
	{
		if (SemiFunc.FPSImpulse5())
		{
			if (Quaternion.Angle(prevHeadLookRotation, headLook.rotation) > 30f && moveSoundHeadTimer <= 0f)
			{
				soundMoveHead.Play(headLook.position);
				prevHeadLookRotation = headLook.rotation;
				moveSoundHeadTimer = 0.2f;
			}
			if (Quaternion.Angle(prevRigidBodyRotation, rb.rotation) > 20f && moveSoundRigidBodyTimer <= 0f)
			{
				moveSoundRigidBodyTimer = 0.5f;
				soundMove.Play(base.transform.position);
				prevRigidBodyRotation = rb.rotation;
			}
			prevHeadLookRotation = headLook.rotation;
			prevRigidBodyRotation = rb.rotation;
		}
		if (moveSoundRigidBodyTimer > 0f)
		{
			moveSoundRigidBodyTimer -= Time.deltaTime;
		}
		if (moveSoundHeadTimer > 0f)
		{
			moveSoundHeadTimer -= Time.deltaTime;
		}
	}

	public void SetDespawn()
	{
		enemy.EnemyParent.Despawn();
	}

	public void SetSpawn()
	{
		animator.Play("Enemy Spinny - Spawn", 0, 0f);
		spawnParticles.Play();
	}

	public void SetMouthOpenedBoolToTrue()
	{
		mouthOpened = true;
	}

	public void SetMouthOpenedBoolToFalse()
	{
		mouthOpened = false;
	}

	private void PlaySpitParticles()
	{
		if (!spitDroplets.isPlaying)
		{
			spitDroplets.Play();
		}
		if (!spitSplash.isPlaying)
		{
			spitSplash.Play();
		}
	}

	private void StopSpitParticles()
	{
		if (spitDroplets.isPlaying)
		{
			spitDroplets.Stop();
		}
		if (spitSplash.isPlaying)
		{
			spitSplash.Stop();
		}
	}

	public void TurnOnAllColorsBright()
	{
		for (int i = 0; i < pieces.Length; i++)
		{
			if (!pieces[i].GetComponent<MeshRenderer>().material.IsKeywordEnabled("_EMISSION"))
			{
				pieces[i].GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
			}
			pieces[i].GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", enemySpinny.colors[i]);
		}
	}

	private void TurnOnAllColors()
	{
		for (int i = 0; i < pieces.Length; i++)
		{
			if (!pieces[i].GetComponent<MeshRenderer>().material.IsKeywordEnabled("_EMISSION"))
			{
				pieces[i].GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
			}
			pieces[i].GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", enemySpinny.colors[i] * 0.2f);
		}
	}

	private void TurnPiecesColor(Color _color)
	{
		Transform[] array = pieces;
		foreach (Transform transform in array)
		{
			if (!transform.GetComponent<MeshRenderer>().material.IsKeywordEnabled("_EMISSION"))
			{
				transform.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
			}
			transform.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", _color);
		}
	}

	private void TurnLightColor(Color _color)
	{
		pointLight.color = _color;
		pointLight.enabled = true;
	}

	private float GetSmallestPositiveDelta(float fromAngle, float toAngle)
	{
		float num = Mathf.DeltaAngle(fromAngle, toAngle);
		if (num < 0f)
		{
			num += 360f;
		}
		return num;
	}

	public void PlayDeathParticles()
	{
		ParticleSystem[] array = deathParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: true);
		}
	}
}
