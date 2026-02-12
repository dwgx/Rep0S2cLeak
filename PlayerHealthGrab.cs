using Photon.Pun;
using UnityEngine;

public class PlayerHealthGrab : MonoBehaviour
{
	public Transform followTransform;

	public Transform hideTransform;

	public PlayerAvatar playerAvatar;

	internal StaticGrabObject staticGrabObject;

	private Collider physCollider;

	private bool colliderActive = true;

	private float grabbingTimer;

	[Space]
	public AnimationCurve hideCurve;

	private float hideLerp;

	private void Start()
	{
		physCollider = GetComponent<Collider>();
		staticGrabObject = GetComponent<StaticGrabObject>();
		if (playerAvatar.isLocal)
		{
			staticGrabObject.enabled = false;
		}
	}

	private void Update()
	{
		if (this.playerAvatar.isTumbling || SemiFunc.RunIsShop() || RunManager.instance.levelIsShop || SemiFunc.RunIsArena())
		{
			if (hideLerp < 1f)
			{
				hideLerp += Time.deltaTime * 5f;
				hideLerp = Mathf.Clamp(hideLerp, 0f, 1f);
				hideTransform.localScale = new Vector3(1f, hideCurve.Evaluate(hideLerp), 1f);
				if (hideLerp >= 1f)
				{
					hideTransform.gameObject.SetActive(value: false);
				}
			}
		}
		else if (hideLerp > 0f)
		{
			if (!hideTransform.gameObject.activeSelf)
			{
				hideTransform.gameObject.SetActive(value: true);
			}
			hideLerp -= Time.deltaTime * 2f;
			hideLerp = Mathf.Clamp(hideLerp, 0f, 1f);
			hideTransform.localScale = new Vector3(1f, hideCurve.Evaluate(hideLerp), 1f);
		}
		bool flag = true;
		if (this.playerAvatar.isDisabled || hideLerp > 0f)
		{
			flag = false;
		}
		if (colliderActive != flag)
		{
			colliderActive = flag;
			physCollider.enabled = colliderActive;
		}
		base.transform.position = followTransform.position;
		base.transform.rotation = followTransform.rotation;
		if (!colliderActive || (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient))
		{
			return;
		}
		if (staticGrabObject.playerGrabbing.Count > 0)
		{
			grabbingTimer += Time.deltaTime;
			foreach (PhysGrabber item in staticGrabObject.playerGrabbing)
			{
				if (grabbingTimer >= 1f)
				{
					PlayerAvatar playerAvatar = item.playerAvatar;
					if (this.playerAvatar.playerHealth.health != this.playerAvatar.playerHealth.maxHealth && playerAvatar.playerHealth.health > 10)
					{
						this.playerAvatar.playerHealth.HealOther(10, effect: true);
						playerAvatar.playerHealth.HurtOther(10, Vector3.zero, savingGrace: false);
						playerAvatar.HealedOther();
					}
				}
			}
			if (grabbingTimer >= 1f)
			{
				grabbingTimer = 0f;
			}
		}
		else
		{
			grabbingTimer = 0f;
		}
	}
}
