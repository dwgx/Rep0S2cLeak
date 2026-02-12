using TMPro;
using UnityEngine;

public class Date : MonoBehaviour
{
	public TextMeshProUGUI textMesh;

	public int yearMin;

	public int yearMax;

	private int year;

	public string[] Months;

	public int[] Days;

	private int currentMonth;

	private int currentDay;

	private void Start()
	{
		year = Random.Range(yearMin, yearMax);
		currentMonth = Random.Range(0, Months.Length);
		currentDay = Random.Range(1, Days[currentMonth]);
		UpdateText();
	}

	public void UpdateDay()
	{
		currentDay++;
		if (currentDay > Days[currentMonth])
		{
			currentDay = 1;
			currentMonth++;
			if (currentMonth >= Months.Length)
			{
				currentMonth = 0;
				year++;
			}
		}
		UpdateText();
	}

	private void UpdateText()
	{
		textMesh.text = Months[currentMonth] + currentDay + " " + year;
	}
}
