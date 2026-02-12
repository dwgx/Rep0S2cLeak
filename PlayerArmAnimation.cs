using UnityEngine;

public class PlayerArmAnimation : MonoBehaviour
{
	private PlayerController Player;

	private Animator Animator;

	private int Crouching;

	private int Crawling;

	private PlayerVoice Voice;

	public Sound MoveShort;

	public Sound MoveLong;

	private void Start()
	{
		Player = PlayerController.instance;
		Voice = PlayerVoice.Instance;
		Animator = GetComponent<Animator>();
		Crouching = Animator.StringToHash("Crouching");
		Crawling = Animator.StringToHash("Crawling");
	}

	private void Update()
	{
		if (Player.Crouching)
		{
			Animator.SetBool(Crouching, value: true);
		}
		else
		{
			Animator.SetBool(Crouching, value: false);
			Animator.SetBool(Crawling, value: false);
		}
		if (Player.Crawling)
		{
			Animator.SetBool(Crawling, value: true);
		}
		else
		{
			Animator.SetBool(Crawling, value: false);
		}
	}

	public void PlayCrouchHush()
	{
	}

	public void PlayMoveShort()
	{
		MoveShort.Play(base.transform.position);
	}

	public void PlayMoveLong()
	{
		MoveLong.Play(base.transform.position);
	}
}
