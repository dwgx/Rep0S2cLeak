using UnityEngine;

public class SledgehammerSwing : MonoBehaviour
{
	[Header("Debug")]
	public bool DebugSwing;

	public bool DebugMeshActive;

	public Mesh DebugMesh;

	[Space]
	[Header("Other")]
	public SledgehammerController Controller;

	[Space]
	[Header("Swinging")]
	public Transform MeshTransform;

	[Space]
	[Header("State")]
	public bool Swinging;

	public bool CanHit;

	private bool SwingingOutro;

	[Space]
	[Header("Swinging")]
	public AnimationCurve SwingCurve;

	public float SwingSpeed = 1f;

	public Vector3 SwingRotation;

	public Vector3 SwingPosition;

	private bool SwingSound = true;

	[Space]
	public AnimationCurve OutroCurve;

	public float OutroSpeed = 1f;

	private float LerpAmount;

	private float LerpResult;

	private float DisableTimer;

	public void Swing()
	{
		if (!Swinging)
		{
			Swinging = true;
			SwingSound = true;
		}
	}

	public void HitOutro()
	{
		MeshTransform.gameObject.SetActive(value: false);
		DisableTimer = 0.1f;
		SwingingOutro = true;
		LerpAmount = 0.5f;
	}

	private void Update()
	{
		CanHit = false;
		if (Swinging && DisableTimer <= 0f)
		{
			if (!SwingingOutro)
			{
				if (LerpAmount == 0f)
				{
					PlayerController.instance.MoveForce(PlayerController.instance.transform.forward, -5f, 0.25f);
					GameDirector.instance.CameraShake.Shake(5f, 0f);
					Controller.SoundMoveShort.Play(base.transform.position);
				}
				if (SwingSound && (double)LerpAmount >= 0.2)
				{
					Controller.SoundSwing.Play(base.transform.position);
					SwingSound = false;
				}
				if ((double)LerpAmount >= 0.25 && (double)LerpAmount <= 0.3)
				{
					PlayerController.instance.MoveForce(PlayerController.instance.transform.forward, 20f, 0.01f);
				}
				if ((double)LerpAmount >= 0.2 && (double)LerpAmount <= 0.5)
				{
					CanHit = true;
				}
				LerpAmount += SwingSpeed * Time.deltaTime;
				if (LerpAmount >= 1f)
				{
					SwingingOutro = true;
					LerpAmount = 0f;
				}
			}
			else
			{
				if (LerpAmount == 0f)
				{
					GameDirector.instance.CameraShake.Shake(2f, 0.5f);
					Controller.SoundMoveLong.Play(base.transform.position);
				}
				if ((double)LerpAmount >= 0.25 && (double)LerpAmount <= 0.3)
				{
					PlayerController.instance.MoveForce(PlayerController.instance.transform.forward, -5f, 0.25f);
				}
				LerpAmount += OutroSpeed * Time.deltaTime;
				if (LerpAmount >= 1f)
				{
					if (!DebugSwing)
					{
						Swinging = false;
					}
					SwingingOutro = false;
					LerpAmount = 0f;
				}
			}
		}
		if (!SwingingOutro)
		{
			LerpResult = SwingCurve.Evaluate(LerpAmount);
		}
		else
		{
			LerpResult = OutroCurve.Evaluate(LerpAmount);
		}
		base.transform.localRotation = Quaternion.LerpUnclamped(Quaternion.identity, Quaternion.Euler(SwingRotation.x, SwingRotation.y, SwingRotation.z), LerpResult);
		base.transform.localPosition = Vector3.LerpUnclamped(Vector3.zero, SwingPosition, LerpResult);
		if (DisableTimer > 0f)
		{
			DisableTimer -= Time.deltaTime;
			if (DisableTimer <= 0f)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (DebugMeshActive)
		{
			Gizmos.color = new Color(0.75f, 0f, 0f, 0.75f);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawMesh(DebugMesh, 0, Vector3.zero + SwingPosition, Quaternion.Euler(SwingRotation.x, SwingRotation.y, SwingRotation.z), Vector3.one);
		}
	}
}
