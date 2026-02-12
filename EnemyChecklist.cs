using UnityEngine;

public class EnemyChecklist : MonoBehaviour
{
	private Color colorPositive = Color.green;

	private Color colorDone = Color.yellow;

	private Color colorNegative = new Color(1f, 0.74f, 0.61f);

	[Space]
	public bool hasRigidbody;

	public new bool name;

	[Space(20f)]
	public bool difficulty;

	[Space(20f)]
	public bool type;

	[Space(20f)]
	public bool center;

	[Space(20f)]
	public bool killLookAt;

	[Space(20f)]
	public bool sightingStinger;

	[Space(20f)]
	public bool enemyNearMusic;

	public bool healthMax;

	[Space(20f)]
	public bool healthMeshParent;

	[Space(20f)]
	public bool healthOnHurt;

	[Space(20f)]
	public bool healthOnDeath;

	[Space(20f)]
	public bool healthImpact;

	[Space(20f)]
	public bool healthObject;

	public bool rigidbodyPhysAttribute;

	[Space(20f)]
	public bool rigidbodyAudioPreset;

	[Space(20f)]
	public bool rigidbodyColliders;

	[Space(20f)]
	public bool rigidbodyFollow;

	[Space(20f)]
	public bool rigidbodyCustomGravity;

	[Space(20f)]
	public bool rigidbodyGrab;

	[Space(20f)]
	public bool rigidbodyPositionFollow;

	[Space(20f)]
	public bool rigidbodyRotationFollow;

	private void ResetChecklist()
	{
		difficulty = false;
		type = false;
		center = false;
		killLookAt = false;
		sightingStinger = false;
		enemyNearMusic = false;
		healthMax = false;
		healthMeshParent = false;
		healthOnHurt = false;
		healthOnDeath = false;
		healthImpact = false;
		healthObject = false;
		rigidbodyPhysAttribute = false;
		rigidbodyAudioPreset = false;
		rigidbodyColliders = false;
		rigidbodyFollow = false;
		rigidbodyCustomGravity = false;
		rigidbodyGrab = false;
		rigidbodyPositionFollow = false;
		rigidbodyRotationFollow = false;
	}

	private void SetAllChecklist()
	{
		difficulty = true;
		type = true;
		center = true;
		killLookAt = true;
		sightingStinger = true;
		enemyNearMusic = true;
		healthMax = true;
		healthMeshParent = true;
		healthOnHurt = true;
		healthOnDeath = true;
		healthImpact = true;
		healthObject = true;
		rigidbodyPhysAttribute = true;
		rigidbodyAudioPreset = true;
		rigidbodyColliders = true;
		rigidbodyFollow = true;
		rigidbodyCustomGravity = true;
		rigidbodyGrab = true;
		rigidbodyPositionFollow = true;
		rigidbodyRotationFollow = true;
	}
}
