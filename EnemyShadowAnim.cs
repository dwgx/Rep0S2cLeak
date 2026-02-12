using UnityEngine;

public class EnemyShadowAnim : MonoBehaviour
{
	public Enemy enemy;

	public EnemyShadow enemyShadow;

	[Header("Sounds")]
	public Sound backCrackLong;

	public Sound backCrackShort;

	public Sound neckCrackLong;

	public Sound neckCrackShort;

	public Sound clapSound;

	public Sound globalClapSound;

	public Sound clapTell;

	public Sound hurtSound;

	public Sound deathSound;

	public Sound idleLoop;

	public Sound moveShort;

	public Sound moveLong;

	public Sound targeted;

	public Sound notTargeted;

	[Header("Particles")]
	public ParticleSystem slapParticles;

	public ParticleSystem spawnSmokeParticle;

	public ParticleSystem shockwaveParticles;

	public ParticleSystem[] deathParticles;

	[Header("Misc")]
	public GameObject wooshLine1;

	public GameObject wooshLine2;

	private Animator animator;

	private static readonly int animDespawn = Animator.StringToHash("despawn");

	private static readonly int animDespawnTrigger = Animator.StringToHash("despawn_trigger");

	private static readonly int animSpawnTrigger = Animator.StringToHash("spawn_trigger");

	internal bool spawnTriggerImpulse = true;

	private Quaternion prevHeadLookRotation;

	private Quaternion prevRigidBodyRotation;

	private Quaternion prevBackRotation;

	private float moveSoundRigidBodyTimer;

	private float moveBackSoundTimer;

	private float moveSoundHeadTimer;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		idleLoop.PlayLoop(enemyShadow.currentState != EnemyShadow.State.Despawn && enemy.CurrentState != EnemyState.Despawn, 1f, 1f);
		if (enemyShadow.currentState == EnemyShadow.State.Despawn || enemy.CurrentState == EnemyState.Despawn)
		{
			if (!animator.GetBool(animDespawn))
			{
				spawnSmokeParticle.Play();
				animator.SetTrigger(animDespawnTrigger);
			}
			animator.SetBool(animDespawn, value: true);
		}
		else
		{
			animator.SetBool(animDespawn, value: false);
			if (spawnTriggerImpulse)
			{
				spawnTriggerImpulse = false;
				animator.SetTrigger(animSpawnTrigger);
			}
		}
	}

	public void TurnSoundLogic(Transform _headTransform, Rigidbody _rb, Transform _backTransform)
	{
		if (SemiFunc.FPSImpulse5())
		{
			float num = Quaternion.Angle(prevHeadLookRotation, _headTransform.rotation);
			if (num > 30f && moveSoundHeadTimer <= 0f)
			{
				neckCrackShort.Play(_headTransform.position);
				prevHeadLookRotation = _headTransform.rotation;
				moveSoundHeadTimer = 1f;
			}
			num = Quaternion.Angle(prevRigidBodyRotation, _rb.rotation);
			if (num > 20f && moveSoundRigidBodyTimer <= 0f)
			{
				moveSoundRigidBodyTimer = 0.5f;
				moveLong.Play(base.transform.position);
				prevRigidBodyRotation = _rb.rotation;
			}
			num = Quaternion.Angle(prevBackRotation, _backTransform.rotation);
			if (num > 30f && moveBackSoundTimer <= 0f)
			{
				moveBackSoundTimer = 1f;
				backCrackLong.Play(base.transform.position);
				prevBackRotation = _backTransform.rotation;
			}
			else if (num > 15f && moveBackSoundTimer <= 0f)
			{
				moveBackSoundTimer = 1f;
				backCrackShort.Play(base.transform.position);
				prevBackRotation = _backTransform.rotation;
			}
			prevHeadLookRotation = _headTransform.rotation;
			prevRigidBodyRotation = _rb.rotation;
			prevBackRotation = _backTransform.rotation;
		}
		if (moveSoundRigidBodyTimer > 0f)
		{
			moveSoundRigidBodyTimer -= Time.deltaTime;
		}
		if (moveSoundHeadTimer > 0f)
		{
			moveSoundHeadTimer -= Time.deltaTime;
		}
		if (moveBackSoundTimer > 0f)
		{
			moveBackSoundTimer -= Time.deltaTime;
		}
	}

	public void PlayBackCrackSound()
	{
		backCrackLong.Play(base.transform.position);
	}

	public void PlayClapSound()
	{
		clapSound.Play(base.transform.position);
		globalClapSound.Play(Vector3.zero);
	}

	public void PlayHurtSound()
	{
		hurtSound.Play(base.transform.position);
	}

	public void PlayDeathSound()
	{
		deathSound.Play(base.transform.position);
	}

	public void playTargetedSound(Vector3 _pos)
	{
		targeted.Play(_pos);
	}

	public void PlayUntargetedSound(Vector3 _pos)
	{
		notTargeted.Play(_pos);
	}

	public void PlaySlapParticles(Vector3 _pos)
	{
		slapParticles.transform.position = _pos;
		slapParticles.Play(withChildren: true);
		shockwaveParticles.transform.position = _pos;
		shockwaveParticles.Play(withChildren: true);
	}

	public void ClapCameraShake()
	{
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
		GameDirector.instance.CameraImpact.ShakeDistance(10f, 3f, 8f, base.transform.position, 0.1f);
	}

	public void PlayDeathParticles()
	{
		ParticleSystem[] array = deathParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: true);
		}
	}

	public void ToggleWooshLines(bool _state)
	{
		wooshLine1.SetActive(_state);
		wooshLine2.SetActive(_state);
	}

	public void SetDespawn()
	{
		enemy.EnemyParent.Despawn();
	}
}
