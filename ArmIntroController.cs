using System.Collections;
using UnityEngine;

public class ArmIntroController : MonoBehaviour
{
	public bool DebugDisable;

	[Space]
	public Animator Animator;

	public Transform CameraTransform;

	public GameObject Hide;

	[Space]
	public float WaitTimer = 0.25f;

	[Space]
	public Sound MoveShort;

	public Sound MoveLong;

	public Sound GlovePull;

	public Sound GloveSnap;

	public void Start()
	{
		Animator.enabled = false;
		base.transform.parent = CameraTransform;
		Hide.SetActive(value: false);
		StartCoroutine(StartIntro());
	}

	public void Update()
	{
		PlayerController.instance.CrouchDisable(0.1f);
	}

	private IEnumerator StartIntro()
	{
		while (GameDirector.instance.currentState != GameDirector.gameState.Main)
		{
			yield return null;
		}
		if (DebugDisable)
		{
			Object.Destroy(base.gameObject);
			yield break;
		}
		yield return new WaitForSeconds(WaitTimer);
		Animator.enabled = true;
		Hide.SetActive(value: true);
	}

	public void AnimationDone()
	{
		Object.Destroy(base.gameObject);
	}

	public void PlayGlovePull()
	{
		GlovePull.Play(base.transform.position);
		GameDirector.instance.CameraImpact.Shake(0.25f, 1f);
	}

	public void PlayGloveSnap()
	{
		GloveSnap.Play(base.transform.position);
		GameDirector.instance.CameraImpact.Shake(1f, 0.1f);
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
