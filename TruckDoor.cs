using System.Collections;
using UnityEngine;

public class TruckDoor : MonoBehaviour
{
	public Sound doorLoopStart;

	public Sound doorLoopEnd;

	public Sound doorSound;

	private float startYPosition;

	private bool fullyOpen;

	private float doorEval;

	public AnimationCurve doorCurve;

	public Transform doorMesh;

	private float doorDelay = 2f;

	private bool doorOpen;

	private ExtractionPoint extractionPointNearest;

	private float playerInTruckCheckTimer;

	private bool timeToCheck;

	private bool introActivationDone;

	private void Start()
	{
		playerInTruckCheckTimer = 2f;
		startYPosition = base.transform.position.y;
		StartCoroutine(DelayedStart());
	}

	private IEnumerator DelayedStart()
	{
		while (!SemiFunc.LevelGenDone())
		{
			yield return new WaitForSeconds(0.3f);
		}
		while (!extractionPointNearest)
		{
			yield return new WaitForSeconds(0.1f);
			extractionPointNearest = SemiFunc.ExtractionPointGetNearest(base.transform.position);
		}
		timeToCheck = true;
	}

	private void Update()
	{
		if (timeToCheck)
		{
			if (playerInTruckCheckTimer > 0f)
			{
				playerInTruckCheckTimer -= Time.deltaTime;
			}
			else
			{
				playerInTruckCheckTimer = 0.5f;
				if (!introActivationDone && !SemiFunc.PlayersAllInTruck())
				{
					introActivationDone = true;
					if (!TutorialDirector.instance.tutorialActive)
					{
						extractionPointNearest.ActivateTheFirstExtractionPointAutomaticallyWhenAPlayerLeaveTruck();
					}
				}
			}
		}
		if (doorDelay > 0f && SemiFunc.LevelGenDone())
		{
			doorDelay -= Time.deltaTime;
		}
		if (doorDelay <= 0f && doorEval < 1f)
		{
			if (!doorOpen)
			{
				doorOpen = true;
				if (SemiFunc.RunIsShop())
				{
					SemiFunc.UIFocusText("Buy stuff in the shop", Color.white, AssetManager.instance.colorYellow);
				}
				GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, base.transform.position, 0.1f);
				doorLoopStart.Play(base.transform.position);
			}
			float num = doorCurve.Evaluate(doorEval);
			doorEval += 1.5f * Time.deltaTime;
			base.transform.position = new Vector3(base.transform.position.x, startYPosition + 2.5f * num, base.transform.position.z);
		}
		if (doorEval >= 1f && !fullyOpen)
		{
			fullyOpen = true;
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 3f, 8f, base.transform.position, 0.1f);
			doorLoopEnd.Play(base.transform.position);
			doorSound.Play(base.transform.position);
		}
	}
}
