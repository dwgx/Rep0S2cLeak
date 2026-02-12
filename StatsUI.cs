using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatsUI : SemiUI
{
	private TextMeshProUGUI Text;

	private TextMeshProUGUI textNumbers;

	private TextMeshProUGUI upgradesHeader;

	public GameObject scanlineObject;

	public static StatsUI instance;

	private Dictionary<string, int> playerUpgrades = new Dictionary<string, int>();

	private float showStatsTimer;

	private bool fetched;

	protected override void Start()
	{
		base.Start();
		Text = GetComponent<TextMeshProUGUI>();
		instance = this;
		textNumbers = base.transform.Find("StatsNumbers").GetComponent<TextMeshProUGUI>();
		Text.text = "";
		upgradesHeader = base.transform.Find("Upgrades Header").GetComponent<TextMeshProUGUI>();
		textNumbers.text = "";
		upgradesHeader.enabled = false;
	}

	public void Fetch()
	{
		playerUpgrades = StatsManager.instance.FetchPlayerUpgrades(PlayerController.instance.playerSteamID);
		Text.text = "";
		textNumbers.text = "";
		upgradesHeader.enabled = false;
		scanlineObject.SetActive(value: false);
		foreach (KeyValuePair<string, int> playerUpgrade in playerUpgrades)
		{
			string text = playerUpgrade.Key.ToUpper();
			if (text == "LAUNCH")
			{
				text = "TUMBLE LAUNCH";
			}
			if (playerUpgrade.Value > 0)
			{
				TextMeshProUGUI text2 = Text;
				text2.text = text2.text + text + "\n";
				TextMeshProUGUI textMeshProUGUI = textNumbers;
				textMeshProUGUI.text = textMeshProUGUI.text + "<b>" + playerUpgrade.Value + "\n</b>";
			}
		}
		if (Text.text != "")
		{
			upgradesHeader.enabled = true;
			scanlineObject.SetActive(value: true);
		}
	}

	public void ShowStats()
	{
		SemiUISpringShakeY(20f, 10f, 0.3f);
		SemiUISpringScale(0.4f, 5f, 0.2f);
		showStatsTimer = 5f;
	}

	protected override void Update()
	{
		base.Update();
		Hide();
		if (showStatsTimer > 0f)
		{
			showStatsTimer -= Time.deltaTime;
			Show();
		}
		if (showTimer > 0f)
		{
			if (!fetched)
			{
				Fetch();
			}
		}
		else
		{
			fetched = false;
		}
	}
}
