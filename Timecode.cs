using System;
using TMPro;
using UnityEngine;

public class Timecode : MonoBehaviour
{
	[Serializable]
	public class TimeSnapshot
	{
		public int TimecodeHour;

		public int TimecodeMinute;

		public int TimecodeSecond;

		public int TimeHour;

		public int TimeMinute;
	}

	public RewindEffect RewindEffect;

	public TextMeshProUGUI textMesh;

	public CurrentTime time;

	public Date date;

	private float timeSec;

	private float timeMin;

	private float timeHour;

	private bool SetStartSnapshot = true;

	private TimeSnapshot StartSnapshot;

	public TimeSnapshot GetSnapshot()
	{
		return new TimeSnapshot
		{
			TimecodeSecond = Mathf.FloorToInt(timeSec),
			TimecodeMinute = Mathf.RoundToInt(timeMin),
			TimecodeHour = Mathf.RoundToInt(timeHour),
			TimeMinute = time.minute,
			TimeHour = time.hour
		};
	}

	public void SetTime(TimeSnapshot snapshot)
	{
		timeSec = snapshot.TimecodeSecond;
		timeMin = snapshot.TimecodeMinute;
		timeHour = snapshot.TimecodeHour;
		time.minute = snapshot.TimeMinute;
		time.hour = snapshot.TimeHour;
	}

	public void SetToStartSnapshot()
	{
		SetTime(StartSnapshot);
	}

	private void Update()
	{
		if (!RewindEffect.PlayRewind && GameDirector.instance.currentState < GameDirector.gameState.Outro)
		{
			if (SetStartSnapshot)
			{
				StartSnapshot = GetSnapshot();
				SetStartSnapshot = false;
			}
			timeSec += Time.deltaTime;
			if (Mathf.Round(timeSec) >= 60f)
			{
				timeSec = 0f;
				timeMin += 1f;
				time.minute++;
				if (time.minute >= 60)
				{
					time.minute = 0;
					time.hour++;
					if (time.hour >= 24)
					{
						time.hour = 0;
						date.UpdateDay();
					}
				}
				if (timeMin >= 60f)
				{
					timeMin = 0f;
					timeHour += 1f;
				}
			}
		}
		string text = timeHour.ToString();
		if (timeHour < 10f)
		{
			text = "0" + text;
		}
		string text2 = timeMin.ToString();
		if (timeMin < 10f)
		{
			text2 = "0" + text2;
		}
		float num = Mathf.Round(timeSec);
		string text3 = num.ToString();
		if (num < 10f)
		{
			text3 = "0" + text3;
		}
		textMesh.text = text + ":" + text2 + ":" + text3;
	}
}
