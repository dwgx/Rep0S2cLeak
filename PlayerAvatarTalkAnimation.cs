using UnityEngine;

public class PlayerAvatarTalkAnimation : MonoBehaviour
{
	public AudioSource audioSource;

	public PlayerAvatar playerAvatar;

	public GameObject objectToRotate;

	private PlayerAvatarVisuals playerAvatarVisuals;

	[Space]
	public float threshold = 0.01f;

	public float rotationMaxAngle = 45f;

	public float amountMultiplier = 1f;

	private bool audioSourceFetched;

	private void Start()
	{
		playerAvatarVisuals = GetComponent<PlayerAvatarVisuals>();
	}

	private void Update()
	{
		if (playerAvatarVisuals.isMenuAvatar && !playerAvatar)
		{
			playerAvatar = PlayerAvatar.instance;
		}
		if (!GameManager.Multiplayer() || !playerAvatar.voiceChatFetched)
		{
			return;
		}
		if (!audioSourceFetched)
		{
			audioSource = playerAvatar.voiceChat.audioSource;
			audioSourceFetched = true;
		}
		if ((bool)audioSource)
		{
			float x = 0f;
			if (playerAvatar.voiceChat.clipLoudness > 0.005f && playerAvatar.voiceChat.overrideNoTalkAnimationTimer <= 0f)
			{
				x = Mathf.Lerp(0f, 0f - rotationMaxAngle, playerAvatar.voiceChat.clipLoudness * 4f);
			}
			objectToRotate.transform.localRotation = Quaternion.Slerp(objectToRotate.transform.localRotation, Quaternion.Euler(x, 0f, 0f), 100f * Time.deltaTime);
		}
	}
}
