using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class ItemToggle : MonoBehaviour
{
	[HideInInspector]
	public bool toggleState;

	public bool playSound;

	private bool fetchSound;

	internal bool toggleStatePrevious;

	private PhotonView photonView;

	private PhysGrabObject physGrabObject;

	private ItemEquippable itemEquippable;

	private Sound soundOn;

	private Sound soundOff;

	internal int playerTogglePhotonID;

	internal bool toggleImpulse;

	private float toggleImpulseTimer;

	internal bool disabled;

	public bool autoTurnOffWhenEquipped = true;

	public UnityEvent onToggle;

	private void Start()
	{
		photonView = GetComponent<PhotonView>();
		physGrabObject = GetComponent<PhysGrabObject>();
		itemEquippable = GetComponent<ItemEquippable>();
	}

	private void Update()
	{
		if (autoTurnOffWhenEquipped && (bool)itemEquippable && itemEquippable.isEquipped && toggleState)
		{
			ToggleItem(toggle: false);
		}
		if (playSound && !fetchSound)
		{
			soundOn = AssetManager.instance.soundDeviceTurnOn;
			soundOff = AssetManager.instance.soundDeviceTurnOff;
			fetchSound = true;
		}
		if (physGrabObject.heldByLocalPlayer && !disabled && PlayerController.instance.InputDisableTimer <= 0f && SemiFunc.InputDown(InputKey.Interact))
		{
			TutorialDirector.instance.playerUsedToggle = true;
			bool toggle = !toggleState;
			int player = SemiFunc.PhotonViewIDPlayerAvatarLocal();
			ToggleItem(toggle, player);
		}
		if (toggleImpulseTimer > 0f)
		{
			toggleImpulse = true;
			toggleImpulseTimer -= Time.deltaTime;
		}
		else
		{
			toggleImpulse = false;
		}
	}

	private void ToggleItemLogic(bool toggle, int player = -1)
	{
		toggleStatePrevious = toggleState;
		toggleState = toggle;
		playerTogglePhotonID = player;
		onToggle.Invoke();
		if (playSound)
		{
			if (toggleState)
			{
				soundOn.Play(base.transform.position);
			}
			else
			{
				soundOff.Play(base.transform.position);
			}
		}
		toggleImpulseTimer = 0.2f;
	}

	public void ToggleItem(bool toggle, int player = -1)
	{
		if (GameManager.Multiplayer())
		{
			photonView.RPC("ToggleItemRPC", RpcTarget.All, toggle, player);
		}
		else
		{
			ToggleItemLogic(toggle, player);
		}
	}

	[PunRPC]
	private void ToggleItemRPC(bool toggle, int player = -1)
	{
		ToggleItemLogic(toggle, player);
	}

	public void ToggleDisable(bool _disable)
	{
		if (GameManager.Multiplayer())
		{
			photonView.RPC("ToggleDisableRPC", RpcTarget.All, _disable);
		}
		else
		{
			ToggleDisableRPC(_disable);
		}
	}

	[PunRPC]
	private void ToggleDisableRPC(bool _disable)
	{
		disabled = _disable;
	}
}
