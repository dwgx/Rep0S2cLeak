using UnityEngine;

public class FlashlightSprint : MonoBehaviour
{
	public float Offset;

	public float Speed;

	public PlayerAvatar PlayerAvatar;

	private void Update()
	{
		if (PlayerAvatar.isLocal)
		{
			if (PlayerController.instance.CanSlide)
			{
				base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, new Vector3(0f, 0f, Offset * GameplayManager.instance.cameraAnimation), Speed * Time.deltaTime);
			}
			else
			{
				base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, new Vector3(0f, 0f, 0f), Speed * Time.deltaTime);
			}
		}
	}
}
