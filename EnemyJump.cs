using System.Collections;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(EnemyGrounded))]
public class EnemyJump : MonoBehaviour
{
	public enum Type
	{
		None,
		Surface,
		Stuck,
		Gap
	}

	public Enemy enemy;

	internal bool jumping;

	internal bool jumpingDelay;

	internal bool landDelay;

	internal float jumpCooldown;

	internal float timeSinceJumped;

	internal Type jumpingType;

	[Space]
	public bool warpAgentOnLand;

	[Space]
	public bool surfaceJump = true;

	public float surfaceJumpForceUp = 5f;

	public float surfaceJumpForceSide = 2f;

	private bool surfaceJumpImpulse;

	private Vector3 surfaceJumpDirection;

	private float surfaceJumpDisableTimer;

	private float surfaceJumpOverrideTimer;

	private float surfaceJumpOverrideUp;

	private float surfaceJumpOverrideSide;

	public float surfaceJumpDelay;

	private float surfaceJumpDelayTimer;

	public float surfaceLandDelay;

	private float surfaceLandDelayTimer;

	[Space]
	public bool stuckJump;

	private float stuckJumpDisableTimer;

	private float cartJumpTimer;

	private float cartJumpCooldown;

	public int stuckJumpCount = 5;

	public float stuckJumpForceUp = 5f;

	public float stuckJumpForceSide = 2f;

	internal bool stuckJumpImpulse;

	private Vector3 stuckJumpImpulseDirection;

	private float stuckJumpOverrideTimer;

	private float stuckJumpOverrideUp;

	private float stuckJumpOverrideSide;

	public float stuckJumpDelay;

	private float stuckJumpDelayTimer;

	public float stuckLandDelay;

	private float stuckLandDelayTimer;

	[Space]
	public bool gapJump;

	public float gapJumpForceUp = 5f;

	public float gapJumpForceForward = 5f;

	internal bool gapJumpImpulse;

	private float gapJumpOverrideTimer;

	private float gapJumpOverrideUp;

	private float gapJumpOverrideForward;

	private Vector3 gapJumpLandPosition;

	public float gapJumpDelay;

	private float gapJumpDelayTimer;

	public float gapLandDelay;

	private float gapLandDelayTimer;

	private bool gapCheckerActive;

	private void Awake()
	{
		enemy.Jump = this;
		enemy.HasJump = true;
		if (gapJump && !gapCheckerActive)
		{
			StartCoroutine(GapChecker());
			gapCheckerActive = true;
		}
	}

	private void Start()
	{
		if (!enemy.HasRigidbody)
		{
			Debug.LogError("EnemyJump: No Rigidbody found on " + enemy.name);
			stuckJump = false;
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		gapCheckerActive = false;
	}

	private void OnEnable()
	{
		if (gapJump && !gapCheckerActive)
		{
			StartCoroutine(GapChecker());
			gapCheckerActive = true;
		}
	}

	public void StuckReset()
	{
		stuckJumpImpulse = false;
	}

	public void SurfaceJumpTrigger(Vector3 _direction)
	{
		if (!jumping)
		{
			surfaceJumpImpulse = true;
			surfaceJumpDirection = _direction;
		}
	}

	public void SurfaceJumpDisable(float _time)
	{
		surfaceJumpImpulse = false;
		surfaceJumpDisableTimer = _time;
	}

	public void StuckTrigger(Vector3 _direction)
	{
		if (!jumping)
		{
			stuckJumpImpulse = true;
			stuckJumpImpulseDirection = _direction;
		}
	}

	public void StuckDisable(float _time)
	{
		stuckJumpDisableTimer = _time;
	}

	private IEnumerator GapChecker()
	{
		gapCheckerActive = true;
		while (true)
		{
			if (enemy.Grounded.grounded && enemy.NavMeshAgent.HasPath() && !enemy.NavMeshAgent.IsStopped() && enemy.Rigidbody.velocity.magnitude >= 0.05f)
			{
				int num = 8;
				float num2 = 0.5f;
				float maxDistance = 2f;
				Vector3 normalized = enemy.Rigidbody.velocity.normalized;
				normalized.y = 0f;
				Vector3 vector = enemy.Rigidbody.physGrabObject.centerPoint + normalized * num2;
				bool flag = false;
				for (int i = 0; i < num; i++)
				{
					if (Physics.Raycast(vector, Vector3.down * 0.25f, maxDistance, SemiFunc.LayerMaskGetVisionObstruct()))
					{
						if (flag && !gapJumpImpulse)
						{
							gapJumpLandPosition = vector + normalized * num2 * 0.5f;
							gapJumpImpulse = true;
						}
					}
					else if (i < 2)
					{
						flag = true;
					}
					vector += normalized * num2;
				}
			}
			yield return new WaitForSeconds(0.2f);
		}
	}

	private void FixedUpdate()
	{
		if (!jumping)
		{
			timeSinceJumped += Time.fixedDeltaTime;
		}
		else
		{
			timeSinceJumped = 0f;
		}
		if (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		bool flag = false;
		Type type = Type.Surface;
		if (enemy.Rigidbody.grabbed || enemy.IsStunned() || enemy.Rigidbody.teleportedTimer > 0f)
		{
			stuckJumpImpulse = false;
			gapJumpImpulse = false;
			return;
		}
		float num = gapJumpForceUp;
		float num2 = gapJumpForceForward;
		if (gapJumpOverrideTimer > 0f)
		{
			num = gapJumpOverrideUp;
			num2 = gapJumpOverrideForward;
			gapJumpOverrideTimer -= Time.fixedDeltaTime;
		}
		if (gapJumpImpulse && !jumping && jumpCooldown <= 0f)
		{
			if (gapJumpDelayTimer > 0f)
			{
				JumpingDelaySet(_jumpingDelay: true, Type.Gap);
				if (warpAgentOnLand)
				{
					enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position, _force: true);
				}
				enemy.NavMeshAgent.Stop(0.1f);
				enemy.Rigidbody.OverrideFollowPosition(0.1f, 0f);
				enemy.Rigidbody.OverrideColliderMaterialStunned(0.1f);
				gapJumpDelayTimer -= Time.fixedDeltaTime;
			}
			else
			{
				enemy.Rigidbody.DisableFollowPosition(0.25f, 10f);
				Vector3 force = (gapJumpLandPosition - enemy.Rigidbody.transform.position).normalized * num2;
				force.y = 0f;
				force += Vector3.up * num;
				enemy.Rigidbody.JumpImpulse();
				enemy.Rigidbody.rb.AddForce(force, ForceMode.Impulse);
				enemy.NavMeshAgent.Warp(gapJumpLandPosition, _force: true);
				gapJumpImpulse = false;
				stuckJumpImpulse = false;
				flag = true;
				type = Type.Gap;
			}
		}
		else if (gapJumpDelayTimer != gapJumpDelay)
		{
			gapJumpDelayTimer = gapJumpDelay;
			if (jumpingDelay && jumpingType == Type.Gap)
			{
				JumpingDelaySet(_jumpingDelay: false, Type.Gap);
			}
		}
		if (stuckJump)
		{
			float num3 = stuckJumpForceUp;
			float num4 = stuckJumpForceSide;
			if (stuckJumpOverrideTimer > 0f)
			{
				num3 = stuckJumpOverrideUp;
				num4 = stuckJumpOverrideSide;
				stuckJumpOverrideTimer -= Time.fixedDeltaTime;
			}
			if (enemy.TeleportedTimer > 0f)
			{
				StuckDisable(0.5f);
			}
			if (stuckJumpDisableTimer > 0f)
			{
				stuckJumpDisableTimer -= Time.fixedDeltaTime;
				stuckJumpImpulse = false;
			}
			else
			{
				if (cartJumpTimer > 0f && enemy.Rigidbody.touchingCartTimer > 0f && cartJumpCooldown <= 0f)
				{
					stuckJumpImpulse = true;
					cartJumpCooldown = 2f;
				}
				if (enemy.StuckCount >= stuckJumpCount)
				{
					stuckJumpImpulse = true;
					enemy.StuckCount = 0;
				}
				if (!flag && stuckJumpImpulse && enemy.Grounded.grounded && !jumping && jumpCooldown <= 0f)
				{
					if (stuckJumpDelayTimer > 0f)
					{
						JumpingDelaySet(_jumpingDelay: true, Type.Stuck);
						enemy.NavMeshAgent.Stop(0.1f);
						enemy.Rigidbody.OverrideFollowPosition(0.1f, 0f);
						enemy.Rigidbody.OverrideColliderMaterialStunned(0.1f);
						stuckJumpDelayTimer -= Time.fixedDeltaTime;
					}
					else
					{
						if (stuckJumpImpulseDirection == Vector3.zero)
						{
							stuckJumpImpulseDirection = enemy.transform.position - enemy.Rigidbody.transform.position;
							if (Random.Range(0, 2) == 0)
							{
								enemy.Rigidbody.DisableFollowPosition(1f, 10f);
								stuckJumpImpulseDirection = Random.insideUnitCircle.normalized;
							}
						}
						Vector3 force2 = stuckJumpImpulseDirection.normalized * num4;
						force2.y = 0f;
						force2 += Vector3.up * num3;
						stuckJumpImpulseDirection = Vector3.zero;
						enemy.Rigidbody.JumpImpulse();
						enemy.Rigidbody.rb.AddForce(force2, ForceMode.Impulse);
						enemy.Rigidbody.DisableFollowPosition(0.25f, 10f);
						stuckJumpImpulse = false;
						flag = true;
						type = Type.Stuck;
					}
				}
				else if (stuckJumpDelayTimer != stuckJumpDelay)
				{
					stuckJumpDelayTimer = stuckJumpDelay;
					if (jumpingDelay && jumpingType == Type.Stuck)
					{
						JumpingDelaySet(_jumpingDelay: false, Type.Stuck);
					}
				}
			}
			if (cartJumpCooldown > 0f)
			{
				cartJumpCooldown -= Time.fixedDeltaTime;
			}
			if (cartJumpTimer > 0f)
			{
				cartJumpTimer -= Time.fixedDeltaTime;
			}
		}
		if (surfaceJump)
		{
			if (!flag && surfaceJumpImpulse && enemy.Grounded.grounded && !jumping && jumpCooldown <= 0f && surfaceJumpDisableTimer <= 0f)
			{
				float num5 = surfaceJumpForceUp;
				float num6 = surfaceJumpForceSide;
				if (surfaceJumpOverrideTimer > 0f)
				{
					num5 = surfaceJumpOverrideUp;
					num6 = surfaceJumpOverrideSide;
				}
				if (surfaceJumpDelayTimer > 0f)
				{
					JumpingDelaySet(_jumpingDelay: true, Type.Surface);
					enemy.NavMeshAgent.Stop(0.1f);
					enemy.Rigidbody.OverrideFollowPosition(0.1f, 0f);
					enemy.Rigidbody.OverrideColliderMaterialStunned(0.1f);
					surfaceJumpDelayTimer -= Time.fixedDeltaTime;
				}
				else
				{
					enemy.Rigidbody.DisableFollowPosition(0.2f, 20f);
					enemy.NavMeshAgent.Stop(0.3f);
					Vector3 vector = surfaceJumpDirection * num6;
					vector.y = 0f;
					enemy.Rigidbody.JumpImpulse();
					enemy.Rigidbody.rb.AddForce(vector + Vector3.up * num5, ForceMode.Impulse);
					surfaceJumpImpulse = false;
					stuckJumpImpulse = false;
					flag = true;
					type = Type.Surface;
				}
			}
			else if (surfaceJumpDelayTimer != surfaceJumpDelay)
			{
				surfaceJumpDelayTimer = surfaceJumpDelay;
				if (jumpingDelay && jumpingType == Type.Surface)
				{
					JumpingDelaySet(_jumpingDelay: false, Type.Surface);
				}
			}
			if (surfaceJumpOverrideTimer > 0f)
			{
				surfaceJumpOverrideTimer -= Time.fixedDeltaTime;
			}
			if (surfaceJumpDisableTimer > 0f)
			{
				surfaceJumpDisableTimer -= Time.fixedDeltaTime;
			}
		}
		if (!jumping)
		{
			if (flag)
			{
				JumpingDelaySet(_jumpingDelay: false, type);
				JumpingSet(_jumping: true, type);
				LandDelaySet(_landDelay: false);
				enemy.Grounded.GroundedDisable(0.1f);
			}
		}
		else if (enemy.Grounded.grounded)
		{
			if (warpAgentOnLand && !enemy.NavMeshAgent.IsDisabled())
			{
				enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position);
			}
			if (jumpingType == Type.Gap && gapLandDelay > 0f)
			{
				LandDelaySet(_landDelay: true);
				gapLandDelayTimer = gapLandDelay;
			}
			if (jumpingType == Type.Surface && surfaceLandDelay > 0f)
			{
				LandDelaySet(_landDelay: true);
				surfaceLandDelayTimer = surfaceLandDelay;
			}
			if (jumpingType == Type.Stuck && stuckLandDelay > 0f)
			{
				LandDelaySet(_landDelay: true);
				stuckLandDelayTimer = stuckLandDelay;
			}
			JumpingDelaySet(_jumpingDelay: false, Type.None);
			JumpingSet(_jumping: false, Type.None);
			jumpCooldown = 0.25f;
		}
		else if (jumpingType == Type.Gap)
		{
			enemy.NavMeshAgent.Warp(gapJumpLandPosition, _force: true);
		}
		if (jumpCooldown > 0f)
		{
			jumpCooldown -= Time.fixedDeltaTime;
			jumpCooldown = Mathf.Max(jumpCooldown, 0f);
			enemy.StuckCount = 0;
			surfaceJumpImpulse = false;
			stuckJumpImpulse = false;
			gapJumpImpulse = false;
		}
		if (gapLandDelayTimer > 0f || surfaceLandDelayTimer > 0f || stuckLandDelayTimer > 0f)
		{
			if (warpAgentOnLand)
			{
				enemy.NavMeshAgent.Warp(enemy.Rigidbody.transform.position, _force: true);
			}
			enemy.NavMeshAgent.Stop(0.1f);
			enemy.Rigidbody.OverrideFollowPosition(0.1f, 0f);
			enemy.Rigidbody.OverrideColliderMaterialStunned(0.1f);
		}
		if (gapLandDelayTimer > 0f)
		{
			gapLandDelayTimer -= Time.fixedDeltaTime;
			if (gapLandDelayTimer <= 0f)
			{
				LandDelaySet(_landDelay: false);
			}
		}
		if (surfaceLandDelayTimer > 0f)
		{
			surfaceLandDelayTimer -= Time.fixedDeltaTime;
			if (surfaceLandDelayTimer <= 0f)
			{
				LandDelaySet(_landDelay: false);
			}
		}
		if (stuckLandDelayTimer > 0f)
		{
			stuckLandDelayTimer -= Time.fixedDeltaTime;
			if (stuckLandDelayTimer <= 0f)
			{
				LandDelaySet(_landDelay: false);
			}
		}
	}

	public void JumpingSet(bool _jumping, Type _type)
	{
		if (_jumping != jumping)
		{
			if (_jumping)
			{
				enemy.Grounded.grounded = false;
			}
			if (GameManager.Multiplayer() && PhotonNetwork.IsMasterClient)
			{
				enemy.Rigidbody.photonView.RPC("JumpingSetRPC", RpcTarget.All, _jumping, _type);
			}
			else
			{
				JumpingSetRPC(_jumping, _type);
			}
		}
	}

	public void JumpingDelaySet(bool _jumpingDelay, Type _type)
	{
		if (jumpingDelay != _jumpingDelay)
		{
			if (SemiFunc.IsMasterClient())
			{
				enemy.Rigidbody.photonView.RPC("JumpingDelaySetRPC", RpcTarget.All, _jumpingDelay, _type);
			}
			else
			{
				JumpingDelaySetRPC(_jumpingDelay, _type);
			}
		}
	}

	public void LandDelaySet(bool _landDelay)
	{
		if (landDelay != _landDelay)
		{
			landDelay = _landDelay;
			if (SemiFunc.IsMasterClient())
			{
				enemy.Rigidbody.photonView.RPC("LandDelaySetRPC", RpcTarget.Others, landDelay);
			}
		}
	}

	public void CartJump(float _time)
	{
		cartJumpTimer = _time;
	}

	public void GapJumpOverride(float _time, float _up, float _forward)
	{
		gapJumpOverrideTimer = _time;
		gapJumpOverrideUp = _up;
		gapJumpOverrideForward = _forward;
	}

	public void StuckJumpOverride(float _time, float _up, float _side)
	{
		stuckJumpOverrideTimer = _time;
		stuckJumpOverrideUp = _up;
		stuckJumpOverrideSide = _side;
	}

	public void SurfaceJumpOverride(float _time, float _up, float _side)
	{
		surfaceJumpOverrideTimer = _time;
		surfaceJumpOverrideUp = _up;
		surfaceJumpOverrideSide = _side;
	}

	[PunRPC]
	private void JumpingSetRPC(bool _jumping, Type _type)
	{
		if (_type != Type.None)
		{
			jumpingType = _type;
		}
		jumping = _jumping;
	}

	[PunRPC]
	private void JumpingDelaySetRPC(bool _jumpingDelay, Type _type)
	{
		if (_type != Type.None)
		{
			jumpingType = _type;
		}
		jumpingDelay = _jumpingDelay;
	}

	[PunRPC]
	private void LandDelaySetRPC(bool _landDelay)
	{
		landDelay = _landDelay;
	}
}
