using UnityEngine;

public class EnemyTumblerAnim : MonoBehaviour
{
	public Enemy enemy;

	public EnemyTumbler enemyTumbler;

	internal Animator animator;

	public Materials.MaterialTrigger material;

	private bool tumble;

	private bool stunned;

	private bool stunImpulse;

	internal bool spawnImpulse;

	private bool jumpImpulse;

	private float jumpedTimer;

	[Header("One Shots")]
	public Sound sfxJump;

	public Sound sfxLand;

	public Sound sfxNotice;

	public Sound sfxCleaverSwing;

	public Sound sfxCharge;

	public Sound sfxHurt;

	public Sound sfxMoveShort;

	public Sound sfxMoveLong;

	public Sound sfxDeath;

	public Sound sfxHurtColliderImpactAny;

	[Header("Loops")]
	public Sound sfxStunnedLoop;

	public Sound sfxTumbleLoopLocal;

	public Sound sfxTumbleLoopGlobal;

	public bool showGizmos = true;

	public float gizmoMinDistance = 3f;

	public float gizmoMaxDistance = 8f;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.keepAnimatorStateOnDisable = true;
	}

	private void Update()
	{
		if (enemy.Jump.jumping)
		{
			animator.SetBool("jumping", value: true);
			if (jumpImpulse)
			{
				jumpedTimer = 0f;
				animator.SetTrigger("Jump");
				animator.SetBool("falling", value: false);
				jumpImpulse = false;
			}
		}
		else
		{
			animator.SetBool("jumping", value: false);
			jumpImpulse = true;
		}
		jumpedTimer += Time.deltaTime;
		if (jumpedTimer > 0.5f)
		{
			if (enemy.Rigidbody.physGrabObject.rbVelocity.y < -0.1f)
			{
				animator.SetBool("falling", value: true);
			}
			else
			{
				animator.SetBool("falling", value: false);
			}
		}
		if (enemyTumbler.currentState == EnemyTumbler.State.Tell)
		{
			animator.SetBool("tell", value: true);
		}
		else
		{
			animator.SetBool("tell", value: false);
		}
		if (enemyTumbler.currentState == EnemyTumbler.State.Tumble)
		{
			animator.SetBool("tumble", value: true);
			tumble = true;
		}
		else
		{
			animator.SetBool("tumble", value: false);
			tumble = false;
		}
		sfxTumbleLoopLocal.PlayLoop(tumble, 5f, 5f);
		sfxTumbleLoopGlobal.PlayLoop(tumble, 5f, 5f);
		if (enemyTumbler.currentState == EnemyTumbler.State.Stunned)
		{
			if (stunImpulse)
			{
				animator.SetTrigger("Stun");
				stunImpulse = false;
			}
			animator.SetBool("stunned", value: true);
			stunned = true;
		}
		else
		{
			animator.SetBool("stunned", value: false);
			stunImpulse = true;
			stunned = false;
		}
		sfxStunnedLoop.PlayLoop(stunned, 5f, 5f);
		if (enemyTumbler.currentState == EnemyTumbler.State.Despawn)
		{
			animator.SetBool("despawning", value: true);
		}
		else
		{
			animator.SetBool("despawning", value: false);
		}
	}

	public void OnSpawn()
	{
		animator.SetBool("stunned", value: false);
		animator.Play("Spawn", 0, 0f);
	}

	private void OnDrawGizmos()
	{
		if (showGizmos)
		{
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, Quaternion.identity, new Vector3(1f, 0f, 1f));
			Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
			Gizmos.DrawWireSphere(Vector3.zero, gizmoMinDistance);
			Gizmos.color = new Color(0.9f, 0f, 0.1f, 0.5f);
			Gizmos.DrawWireSphere(Vector3.zero, gizmoMaxDistance);
		}
	}

	public void SfxOnHurtColliderImpactAny()
	{
		sfxHurtColliderImpactAny.Play(base.transform.position);
	}

	public void OnTumble()
	{
		GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.5f);
	}

	public void Despawn()
	{
		enemy.EnemyParent.Despawn();
	}

	public void SfxJump()
	{
		sfxJump.Play(base.transform.position);
		GameDirector.instance.CameraImpact.ShakeDistance(1f, 3f, 8f, base.transform.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(1f, 3f, 8f, base.transform.position, 0.5f);
	}

	public void SfxLand()
	{
		sfxLand.Play(base.transform.position);
		GameDirector.instance.CameraImpact.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.1f);
		GameDirector.instance.CameraShake.ShakeDistance(2f, 3f, 8f, base.transform.position, 0.5f);
	}

	public void SfxNotice()
	{
		sfxNotice.Play(base.transform.position);
	}

	public void SfxCleaverSwing()
	{
		sfxCleaverSwing.Play(base.transform.position);
	}

	public void SfxCharge()
	{
		sfxCharge.Play(base.transform.position);
	}

	public void SfxHurt()
	{
		sfxHurt.Play(base.transform.position);
	}

	public void SfxMoveShort()
	{
		sfxMoveShort.Play(base.transform.position);
	}

	public void SfxMoveLong()
	{
		sfxMoveLong.Play(base.transform.position);
	}
}
