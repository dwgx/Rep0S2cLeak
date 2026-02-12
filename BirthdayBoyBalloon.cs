using UnityEngine;

public class BirthdayBoyBalloon : MonoBehaviour
{
	[HideInInspector]
	public bool popped;

	[HideInInspector]
	public PlayerAvatar popper;

	[Header("Movement")]
	public Transform moveTransform;

	public AnimationCurve balloonFloatCurve = new AnimationCurve();

	[Header("Colliders")]
	public Collider collider1;

	public Collider collider2;

	private readonly float balloonAnimateRange = 0.2f;

	private readonly float balloonAnimationSpeed = 0.1f;

	[HideInInspector]
	public float randomSpeed = 1f;

	private float animationEvaluationTime;

	private bool animatingToPlace = true;

	private bool takenToSpawnPoint;

	public bool poppedWhileDespawned;

	public int balloonIndex;

	public int placerIndex;

	public EnemyParent enemyParent;

	public MeshRenderer mr;

	public void ChangeColor(Color _color)
	{
		mr.material.SetColor("_BaseColor", _color);
		mr.material.SetColor("_EmissionColor", _color * 0.28f);
	}

	private void Update()
	{
		if (!animatingToPlace)
		{
			AnimateUpAndDown();
		}
		else if (takenToSpawnPoint)
		{
			if (Vector3.Distance(moveTransform.localPosition, Vector3.zero) < 0.1f)
			{
				AnimateUpAndDown();
			}
			AnimateToPlace();
		}
		if (SemiFunc.IsMasterClientOrSingleplayer() && (bool)enemyParent && popped && enemyParent.DespawnedTimer > 0f)
		{
			poppedWhileDespawned = true;
			enemyParent.DespawnedTimerSet(0f);
		}
	}

	private void OnTriggerEnter(Collider _other)
	{
		if (!SemiFunc.IsMasterClientOrSingleplayer() || popped || _other.name == "Birthday Boy Collider" || _other.name == "Birthday Boy Collider Small" || _other.transform.IsChildOf(base.transform) || _other.gameObject.layer == LayerMask.NameToLayer("CollisionCheck") || (_other.isTrigger && !_other.GetComponent<HurtCollider>()) || (bool)_other.GetComponent<BirthdayBoyBalloon>() || PlayerNearby() == null)
		{
			return;
		}
		popped = true;
		PlayerController componentInParent = _other.GetComponentInParent<PlayerController>();
		PlayerAvatar playerAvatar;
		if ((bool)componentInParent)
		{
			playerAvatar = componentInParent.playerAvatarScript;
		}
		else
		{
			playerAvatar = _other.GetComponentInParent<PlayerAvatar>();
			if (!playerAvatar)
			{
				PlayerTumble componentInParent2 = _other.GetComponentInParent<PlayerTumble>();
				if ((bool)componentInParent2)
				{
					playerAvatar = componentInParent2.playerAvatar;
				}
			}
		}
		if ((bool)playerAvatar)
		{
			popper = playerAvatar;
		}
		PhysGrabObject physGrabObject = _other.GetComponent<PhysGrabObject>();
		if (!physGrabObject)
		{
			physGrabObject = _other.GetComponentInParent<PhysGrabObject>();
		}
		if ((bool)physGrabObject)
		{
			if (physGrabObject.grabbed)
			{
				popper = physGrabObject.playerGrabbing[0].playerAvatar;
			}
			else if ((bool)physGrabObject.lastPlayerGrabbing)
			{
				popper = physGrabObject.lastPlayerGrabbing;
			}
		}
	}

	public void TakeToSpawnPoint(Vector3 pos, Vector3 size)
	{
		moveTransform.position = pos;
		moveTransform.localScale = size;
		takenToSpawnPoint = true;
	}

	private void AnimateUpAndDown()
	{
		Vector3 localPosition = moveTransform.localPosition;
		animationEvaluationTime += Time.deltaTime * balloonAnimationSpeed * randomSpeed;
		if (animationEvaluationTime > balloonFloatCurve.keys[balloonFloatCurve.length - 1].time)
		{
			animationEvaluationTime = 0f;
		}
		float num = balloonFloatCurve.Evaluate(animationEvaluationTime) * balloonAnimateRange - balloonAnimateRange / 2f;
		localPosition.y = Mathf.Lerp(localPosition.y, num, Time.deltaTime * 2f);
		moveTransform.localPosition = localPosition;
	}

	private PlayerAvatar PlayerNearby()
	{
		PlayerAvatar result = null;
		float num = float.MaxValue;
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			if (item.isDisabled)
			{
				PlayerDeathHead playerDeathHead = item.playerDeathHead;
				if (playerDeathHead.spectated)
				{
					float num2 = Vector3.Distance(playerDeathHead.transform.position, base.transform.position);
					if (num2 < num)
					{
						result = item;
						num = num2;
					}
				}
			}
			else
			{
				float num3 = Vector3.Distance(item.transform.position, base.transform.position);
				if (num3 < num)
				{
					result = item;
					num = num3;
				}
			}
		}
		if (num <= 10f)
		{
			return result;
		}
		return null;
	}

	private void AnimateToPlace()
	{
		Vector3 localPosition = moveTransform.localPosition;
		localPosition = Vector3.Lerp(localPosition, Vector3.zero, Time.deltaTime * 2f);
		moveTransform.localPosition = localPosition;
		Vector3 localScale = moveTransform.localScale;
		localScale = Vector3.Lerp(localScale, Vector3.one, Time.deltaTime * 2f);
		moveTransform.localScale = localScale;
		if (animatingToPlace && Vector3.Distance(moveTransform.localPosition, Vector3.zero) < 0.1f && moveTransform.localScale == Vector3.one)
		{
			collider1.enabled = true;
			collider2.enabled = true;
			animatingToPlace = false;
		}
	}
}
