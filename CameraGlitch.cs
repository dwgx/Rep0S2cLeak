using UnityEngine;

public class CameraGlitch : MonoBehaviour
{
	public static CameraGlitch Instance;

	public Camera targetCamera;

	private float targetCameraFOV;

	public Animator Animator;

	public GameObject ActiveParent;

	[Space]
	public int GlitchLongCount;

	public int GlitchShortCount;

	public int GlitchTinyCount;

	[Space]
	public Sound GlitchLong;

	public Sound GlitchShort;

	public Sound GlitchTiny;

	[Space]
	public Sound HurtShort;

	public Sound HurtLong;

	[Space]
	public Sound HealShort;

	public Sound HealLong;

	[Space]
	public Sound Upgrade;

	[Space]
	public Sound doNotLookEffectSound;

	private float doNotLookEffectTimer;

	private float doNotLookEffectImpulseTimer;

	private void Awake()
	{
		Instance = this;
		targetCameraFOV = targetCamera.fieldOfView;
	}

	private void Start()
	{
		ActiveParent.SetActive(value: false);
	}

	private void Update()
	{
		float num = targetCamera.fieldOfView / targetCameraFOV;
		if (num > 1.5f)
		{
			num *= 1.2f;
		}
		if (num < 0.5f)
		{
			num *= 0.8f;
		}
		base.transform.localScale = new Vector3(num, num, num);
		if (doNotLookEffectTimer > 0f)
		{
			doNotLookEffectSound.PlayLoop(playing: true, 2f, 1f);
			doNotLookEffectTimer -= Time.deltaTime;
			if (doNotLookEffectImpulseTimer <= 0f)
			{
				PlayShort();
				doNotLookEffectImpulseTimer = Random.Range(0.3f, 1f);
			}
			else
			{
				doNotLookEffectImpulseTimer -= Time.deltaTime;
			}
		}
		else
		{
			doNotLookEffectSound.PlayLoop(playing: false, 2f, 1f);
		}
	}

	public void DoNotLookEffectSet()
	{
		doNotLookEffectTimer = 0.1f;
	}

	public void PlayLong()
	{
		Animator.SetTrigger("Long");
		Animator.SetInteger("Index", Random.Range(0, GlitchLongCount));
		GlitchLong.Play(base.transform.position);
		GameDirector.instance.CameraImpact.Shake(2f, 0.3f);
	}

	public void PlayShort()
	{
		Animator.SetTrigger("Short");
		Animator.SetInteger("Index", Random.Range(0, GlitchShortCount));
		GlitchShort.Play(base.transform.position);
		GameDirector.instance.CameraImpact.Shake(2f, 0.1f);
	}

	public void PlayTiny()
	{
		Animator.SetTrigger("Tiny");
		Animator.SetInteger("Index", Random.Range(0, GlitchTinyCount));
		GlitchTiny.Play(base.transform.position);
	}

	public void PlayLongHurt()
	{
		Animator.SetTrigger("HurtLong");
		Animator.SetInteger("Index", Random.Range(0, GlitchLongCount));
		HurtLong.Play(base.transform.position);
		GlitchLong.Play(base.transform.position);
		GameDirector.instance.CameraShake.Shake(3f, 0.5f);
		GameDirector.instance.CameraImpact.Shake(5f, 0.2f);
	}

	public void PlayShortHurt()
	{
		Animator.SetTrigger("HurtShort");
		Animator.SetInteger("Index", Random.Range(0, GlitchShortCount));
		HurtShort.Play(base.transform.position);
		GlitchShort.Play(base.transform.position);
		GameDirector.instance.CameraShake.Shake(3f, 0.5f);
		GameDirector.instance.CameraImpact.Shake(3f, 0.2f);
	}

	public void PlayLongHeal()
	{
		Animator.SetTrigger("HealLong");
		Animator.SetInteger("Index", Random.Range(0, GlitchLongCount));
		HealLong.Play(base.transform.position);
		GlitchLong.Play(base.transform.position);
		GameDirector.instance.CameraShake.Shake(1.5f, 0.2f);
		GameDirector.instance.CameraImpact.Shake(2.5f, 0.2f);
	}

	public void PlayShortHeal()
	{
		Animator.SetTrigger("HealShort");
		Animator.SetInteger("Index", Random.Range(0, GlitchShortCount));
		HealShort.Play(base.transform.position);
		GlitchShort.Play(base.transform.position);
		GameDirector.instance.CameraShake.Shake(1.5f, 0.2f);
		GameDirector.instance.CameraImpact.Shake(1.5f, 0.2f);
	}

	public void PlayUpgrade()
	{
		Animator.SetTrigger("Upgrade");
		Animator.SetInteger("Index", Random.Range(0, GlitchShortCount));
		Upgrade.Play(base.transform.position);
		GameDirector.instance.CameraShake.Shake(2f, 0.5f);
		GameDirector.instance.CameraImpact.Shake(2f, 0.5f);
	}
}
