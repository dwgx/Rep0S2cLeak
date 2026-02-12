using UnityEngine;

public class TutorialTruckTrigger : MonoBehaviour
{
	private float lockLookTimer;

	public Transform lookTarget;

	private float messageDelay = 1.5f;

	private bool messageSent;

	private bool triggered;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			triggered = true;
		}
	}

	private void Update()
	{
		if (triggered && GetComponent<Collider>().enabled)
		{
			lockLookTimer = 0.5f;
			GetComponent<Collider>().enabled = false;
			CameraGlitch.Instance.PlayLong();
		}
		if (lockLookTimer > 0f)
		{
			lockLookTimer -= Time.deltaTime;
			CameraAim.Instance.AimTargetSet(lookTarget.position + Vector3.down, 0.1f, 5f, base.gameObject, 90);
		}
		if (!triggered)
		{
			return;
		}
		if (messageDelay > 0f)
		{
			messageDelay -= Time.deltaTime;
		}
		else if (!messageSent)
		{
			TruckScreenText component = lookTarget.GetComponent<TruckScreenText>();
			if (!component.isTyping && component.delayTimer <= 0f)
			{
				component.GotoPage(1);
			}
			messageSent = true;
		}
	}
}
