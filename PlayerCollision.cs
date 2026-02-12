using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
	public static PlayerCollision instance;

	public PlayerController Player;

	public Transform StandCollision;

	public Transform CrouchCollision;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		if (Player.Crouching && CameraCrouchPosition.instance.Active && CameraCrouchPosition.instance.Lerp > 0.5f)
		{
			base.transform.localScale = CrouchCollision.localScale;
		}
		else
		{
			base.transform.localScale = StandCollision.localScale;
		}
	}

	public void SetCrouchCollision()
	{
		base.transform.localScale = CrouchCollision.localScale;
	}
}
