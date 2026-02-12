using Photon.Pun;
using UnityEngine;

public class ItemDroneTorque : MonoBehaviour
{
	private ItemDrone itemDrone;

	private PhysGrabObject myPhysGrabObject;

	private ItemEquippable itemEquippable;

	private ItemToggle itemToggle;

	private ItemBattery itemBattery;

	private ItemAttributes itemAttributes;

	private float tumbleEnemyTimer;

	private bool tumbledPlayer;

	private void Start()
	{
		itemEquippable = GetComponent<ItemEquippable>();
		itemDrone = GetComponent<ItemDrone>();
		myPhysGrabObject = GetComponent<PhysGrabObject>();
		itemToggle = GetComponent<ItemToggle>();
		itemBattery = GetComponent<ItemBattery>();
		itemAttributes = GetComponent<ItemAttributes>();
	}

	private void RollTowards(Vector3 direction, Rigidbody targetRb)
	{
		Vector3 vector = Vector3.Cross(Vector3.up, direction).normalized * 6f;
		float num = Mathf.Clamp(3f / targetRb.mass, 1f, 10f);
		vector *= num;
		targetRb.angularVelocity = vector / targetRb.mass;
	}

	private void BatteryDrain(float amount)
	{
		itemBattery.batteryLife -= amount * Time.fixedDeltaTime;
	}

	private void FixedUpdate()
	{
		if (!itemDrone.itemActivated)
		{
			tumbledPlayer = false;
			tumbleEnemyTimer = 0f;
		}
		if (itemEquippable.isEquipped || (GameManager.instance.gameMode == 1 && !PhotonNetwork.IsMasterClient) || !itemDrone.itemActivated)
		{
			return;
		}
		if (!itemDrone.droneOwner || ((bool)itemDrone.droneOwner && itemDrone.droneOwner.isDisabled))
		{
			itemToggle.ToggleItem(toggle: false);
			return;
		}
		myPhysGrabObject.OverrideZeroGravity();
		myPhysGrabObject.OverrideDrag(1f);
		myPhysGrabObject.OverrideAngularDrag(10f);
		if (!itemDrone.magnetActive)
		{
			return;
		}
		if ((bool)itemDrone.playerAvatarTarget && !tumbledPlayer)
		{
			if (!itemDrone.playerAvatarTarget.tumble.isTumbling)
			{
				itemDrone.playerAvatarTarget.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
			}
			tumbledPlayer = true;
		}
		if ((bool)itemDrone.playerTumbleTarget)
		{
			Vector3 forward = itemDrone.playerTumbleTarget.playerAvatar.localCamera.transform.forward;
			if (SemiFunc.OnGroundCheck(itemDrone.magnetTargetRigidbody.position, 1.5f, itemDrone.magnetTargetPhysGrabObject))
			{
				Vector3 vector = SemiFunc.PhysFollowDirection(itemDrone.magnetTargetRigidbody.transform, forward, itemDrone.magnetTargetRigidbody, 20f);
				itemDrone.magnetTargetRigidbody.AddTorque(vector * 2.5f / itemDrone.magnetTargetRigidbody.mass, ForceMode.Force);
				Vector3 vector2 = SemiFunc.PhysFollowPosition(itemDrone.magnetTargetRigidbody.position, itemDrone.magnetTargetRigidbody.position + forward * 2.5f, itemDrone.magnetTargetRigidbody.velocity, 25f);
				itemDrone.magnetTargetRigidbody.AddForce(vector2 * 2f / itemDrone.magnetTargetRigidbody.mass, ForceMode.Force);
				BatteryDrain(2f);
				if ((bool)itemDrone.magnetTargetPhysGrabObject)
				{
					itemDrone.magnetTargetPhysGrabObject.OverrideMaterial(SemiFunc.PhysicMaterialSticky());
				}
			}
			itemDrone.playerTumbleTarget.OverrideDisableLookAtCamera(0.5f);
		}
		if (!itemDrone.magnetTargetPhysGrabObject || (bool)itemDrone.playerAvatarTarget || (bool)itemDrone.playerTumbleTarget)
		{
			return;
		}
		Rigidbody magnetTargetRigidbody = itemDrone.magnetTargetRigidbody;
		Transform transform = itemDrone.droneOwner.transform;
		if ((bool)transform)
		{
			float num = Vector3.Distance(new Vector3(magnetTargetRigidbody.position.x, 0f, magnetTargetRigidbody.position.z), new Vector3(transform.position.x, 0f, transform.position.z));
			Vector3 vector3 = (transform.position - magnetTargetRigidbody.position).normalized;
			if (itemDrone.magnetTargetPhysGrabObject.isEnemy)
			{
				EnemyParent componentInParent = itemDrone.magnetTargetPhysGrabObject.GetComponentInParent<EnemyParent>();
				if ((bool)componentInParent)
				{
					SemiFunc.ItemAffectEnemyBatteryDrain(componentInParent, itemBattery, tumbleEnemyTimer, Time.fixedDeltaTime);
					tumbleEnemyTimer += Time.fixedDeltaTime;
				}
				vector3 = -vector3;
			}
			float num2 = 2f;
			float num3 = Mathf.Clamp(magnetTargetRigidbody.mass / 1f, 0.2f, 1f);
			float num4 = num3 * 2f;
			if (num < num4)
			{
				num2 = Mathf.Clamp(num - num3, 0f, num4) / num4;
			}
			Vector3 vector4 = SemiFunc.PhysFollowDirection(itemDrone.magnetTargetRigidbody.transform, vector3, itemDrone.magnetTargetRigidbody, 10f) * num2;
			itemDrone.magnetTargetRigidbody.AddTorque(vector4 * 5f / itemDrone.magnetTargetRigidbody.mass, ForceMode.Force);
			Vector3 vector5 = SemiFunc.PhysFollowPosition(itemDrone.magnetTargetRigidbody.position, itemDrone.magnetTargetRigidbody.position + vector3, itemDrone.magnetTargetRigidbody.velocity, 10f) * num2;
			itemDrone.magnetTargetRigidbody.AddForce(vector5 * 2f / itemDrone.magnetTargetRigidbody.mass, ForceMode.Force);
			itemDrone.magnetTargetPhysGrabObject.OverrideFragility(0.65f);
		}
		else
		{
			Vector3 vector6 = -base.transform.forward.normalized;
			Vector3 vector7 = SemiFunc.PhysFollowDirection(itemDrone.magnetTargetRigidbody.transform, vector6, itemDrone.magnetTargetRigidbody, 10f);
			itemDrone.magnetTargetRigidbody.AddTorque(vector7 * 2f / itemDrone.magnetTargetRigidbody.mass, ForceMode.Force);
			Vector3 vector8 = SemiFunc.PhysFollowPosition(itemDrone.magnetTargetRigidbody.position, itemDrone.magnetTargetRigidbody.position + vector6, itemDrone.magnetTargetRigidbody.velocity, 10f);
			itemDrone.magnetTargetRigidbody.AddForce(vector8 * 1f / itemDrone.magnetTargetRigidbody.mass, ForceMode.Force);
		}
	}
}
