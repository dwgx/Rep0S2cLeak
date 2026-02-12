using UnityEngine;

public class FlashlightBob : MonoBehaviour
{
	public PlayerAvatar PlayerAvatar;

	private void Update()
	{
		if (PlayerAvatar.isLocal)
		{
			Vector3 positionResult = CameraBob.Instance.positionResult;
			base.transform.localPosition = new Vector3((0f - positionResult.y) * 0.2f, 0f, 0f);
			base.transform.localRotation = Quaternion.Euler(0f, positionResult.y * 30f, 0f);
		}
	}
}
