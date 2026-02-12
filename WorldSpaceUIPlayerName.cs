using TMPro;
using UnityEngine;

public class WorldSpaceUIPlayerName : WorldSpaceUIChild
{
	public TextMeshProUGUI text;

	internal PlayerAvatar playerAvatar;

	private Vector3 followTarget;

	private float followTargetY;

	private float showTimer;

	private float showTimeTotal;

	private float showTimeTotalResetTimer;

	private void OnDisable()
	{
		text.color = new Color(1f, 1f, 1f, 0f);
	}

	protected override void Update()
	{
		base.Update();
		if (!playerAvatar)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (showTimeTotalResetTimer > 0f)
		{
			showTimeTotalResetTimer -= Time.deltaTime;
			if (showTimeTotalResetTimer <= 0f)
			{
				showTimeTotal = 0f;
			}
		}
		if ((bool)SpectateCamera.instance || playerAvatar.isDisabled || showTimer <= 0f)
		{
			text.color = Color.Lerp(text.color, new Color(1f, 1f, 1f, 0f), Time.deltaTime * 20f);
		}
		else
		{
			showTimer -= Time.deltaTime;
			text.color = Color.Lerp(text.color, new Color(1f, 1f, 1f, 0.5f), Time.deltaTime * 5f);
		}
		Vector3 position = playerAvatar.playerAvatarVisuals.headLookAtTransform.position;
		position.y = playerAvatar.playerAvatarVisuals.transform.position.y;
		if (playerAvatar == SessionManager.instance.CrownedPlayerGet())
		{
			position.y += 0.02f;
		}
		followTarget = Vector3.Lerp(followTarget, position, Time.deltaTime * 30f);
		float num = playerAvatar.playerAvatarVisuals.headLookAtTransform.position.y - playerAvatar.playerAvatarVisuals.transform.position.y + 0.35f;
		if (Mathf.Abs(followTargetY - num) > 0.2f)
		{
			followTargetY = Mathf.Lerp(followTargetY, num, Time.deltaTime * 20f);
		}
		else
		{
			followTargetY = Mathf.Lerp(followTargetY, num, Time.deltaTime * 3f);
		}
		worldPosition = followTarget + Vector3.up * followTargetY;
		float num2 = Vector3.Distance(worldPosition, Camera.main.transform.position);
		text.fontSize = 20f - num2;
	}

	public void Show()
	{
		showTimeTotal += 0.25f;
		showTimeTotalResetTimer = 0.5f;
		if (showTimeTotal >= 1f)
		{
			showTimer = 0.5f;
		}
	}
}
