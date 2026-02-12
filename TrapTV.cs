using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class TrapTV : Trap
{
	public GameObject TVScreen;

	public UnityEvent TVTimer;

	public float runTime = 10f;

	private float Timer;

	private float speedMulti = 1f;

	public MeshRenderer TVBackground;

	public GameObject CatObject;

	public GameObject MouseObject;

	public MeshRenderer TVStatic;

	public Light TVLight;

	public AnimationCurve TVStaticCurve;

	public float TVStaticTime = 0.5f;

	public float TVStaticTimer;

	[Space]
	[Header("___________________ Cartoon Cat Talk ___________________")]
	public float catTalkTime = 10f;

	public float catTalkPauseTime = 2f;

	public int catTalkCount = 2;

	private int catTalkCounter;

	public float catTalkStartScale = 0.2f;

	public float catTalkEndScale = 0.277f;

	public Sound catTalkSound1;

	public Sound catTalkSound2;

	[Space]
	[Header("___________________ Cartoon Mouse Talk ___________________")]
	public float mouseTalkTime = 10f;

	public float mouseTalkPauseTime = 2f;

	public int mouseTalkCount = 2;

	private int mouseTalkCounter;

	public float mouseTalkStartScale = 0.12f;

	public float mouseTalkEndScale = 0.155f;

	public Sound mouseTalkSound1;

	public Sound mouseTalkSound2;

	private int state;

	private int stateRunning;

	private int stateCatTalk = 1;

	private int stateCatTalkPause = 2;

	private int stateMouseTalk = 3;

	private int stateMouseTalkPause = 4;

	private float updateInterval = 1f / 12f;

	private bool TVStaticIntro = true;

	public bool TVStaticOutro;

	[Space]
	[Header("___________________ TV Sounds ___________________")]
	public Sound LoopSound;

	public Sound StartSound;

	public Sound StopSound;

	[HideInInspector]
	public bool TrapDone;

	private bool TVStart = true;

	private Material CatMaterial;

	private Material MouseMaterial;

	protected override void Start()
	{
		base.Start();
		TVStaticOutro = false;
		TVBackground.enabled = false;
		TVStatic.enabled = false;
		TVStaticIntro = true;
		TVLight.enabled = false;
		CatObject.SetActive(value: false);
		MouseObject.SetActive(value: false);
		CatMaterial = CatObject.GetComponent<Renderer>().material;
		MouseMaterial = MouseObject.GetComponent<Renderer>().material;
		photonView = GetComponent<PhotonView>();
		if (GameManager.instance.gameMode == 0)
		{
			isLocal = true;
		}
	}

	private IEnumerator AnimationCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(updateInterval);
			float num = updateInterval;
			if (!trapActive)
			{
				break;
			}
			float y = 0.5f;
			if (state == stateCatTalk)
			{
				y = 0f;
			}
			if (CatMaterial.mainTextureOffset.x < 1f)
			{
				CatMaterial.mainTextureOffset = new Vector2(CatMaterial.mainTextureOffset.x + 0.33f, y);
			}
			else
			{
				CatMaterial.mainTextureOffset = new Vector2(0f, y);
			}
			float y2 = 0.5f;
			if (state == stateMouseTalk)
			{
				y2 = 0f;
			}
			if (CatMaterial.mainTextureOffset.x < 1f)
			{
				MouseMaterial.mainTextureOffset = new Vector2(MouseMaterial.mainTextureOffset.x + 0.33f, y2);
			}
			else
			{
				MouseMaterial.mainTextureOffset = new Vector2(0f, y2);
			}
			if (state == stateRunning)
			{
				Timer += num * speedMulti;
				if (Timer > runTime)
				{
					Timer = 0f;
					state = stateCatTalk;
					catTalkSound1.Play(physGrabObject.centerPoint);
				}
			}
			if (state == stateCatTalk)
			{
				Timer += num * speedMulti;
				if (Timer > catTalkTime)
				{
					Timer = 0f;
					state = stateCatTalkPause;
					catTalkCounter++;
				}
			}
			if (state == stateCatTalkPause)
			{
				Timer += num * speedMulti;
				if (Timer > catTalkPauseTime)
				{
					Timer = 0f;
					if (catTalkCounter < catTalkCount)
					{
						state = stateCatTalk;
						catTalkSound2.Play(physGrabObject.centerPoint);
					}
					else
					{
						catTalkCounter = 0;
						state = stateMouseTalk;
						mouseTalkSound1.Play(physGrabObject.centerPoint);
					}
				}
			}
			if (state == stateMouseTalk)
			{
				Timer += num * speedMulti;
				if (Timer > mouseTalkTime)
				{
					Timer = 0f;
					state = stateMouseTalkPause;
					mouseTalkCounter++;
				}
			}
			if (state != stateMouseTalkPause)
			{
				continue;
			}
			Timer += num * speedMulti;
			if (Timer > mouseTalkPauseTime)
			{
				Timer = 0f;
				if (mouseTalkCounter < mouseTalkCount)
				{
					state = stateMouseTalk;
					mouseTalkSound2.Play(physGrabObject.centerPoint);
				}
				else
				{
					mouseTalkCounter = 0;
					state = stateRunning;
				}
			}
		}
	}

	public void TrapActivate()
	{
		if (!trapTriggered)
		{
			trapActive = true;
			trapTriggered = true;
			TVStart = true;
			TVTimer.Invoke();
		}
	}

	public void TrapStop()
	{
		TVStaticOutro = true;
		TVStaticTimer = 0f;
		StopSound.Play(physGrabObject.centerPoint);
	}

	protected override void Update()
	{
		base.Update();
		LoopSound.PlayLoop(trapActive, 0.9f, 0.9f);
		if (trapStart)
		{
			TrapActivate();
		}
		if (!trapActive)
		{
			return;
		}
		enemyInvestigate = true;
		if (TVStart)
		{
			TVBackground.enabled = true;
			TVLight.enabled = true;
			TVStart = false;
			StartSound.Play(physGrabObject.centerPoint);
			CatObject.SetActive(value: true);
			MouseObject.SetActive(value: true);
			StartCoroutine(AnimationCoroutine());
		}
		if (!TVStaticIntro && !TVStaticOutro)
		{
			return;
		}
		float num = TVStaticCurve.Evaluate(TVStaticTimer / TVStaticTime);
		TVStaticTimer += 1f * Time.deltaTime * speedMulti;
		if (num > 0.5f)
		{
			TVStatic.enabled = true;
		}
		else
		{
			TVStatic.enabled = false;
		}
		if (TVStaticTimer > TVStaticTime)
		{
			TVStaticIntro = false;
			TVStaticTimer = 0f;
			TVStatic.enabled = false;
			if (TVStaticOutro)
			{
				trapActive = false;
				TVScreen.SetActive(value: false);
			}
		}
	}
}
