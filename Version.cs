using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Version - ", menuName = "Other/Version", order = 0)]
public class Version : ScriptableObject
{
	public string title = "v0.0.0";

	public string discordRole = "@";

	[TextArea(0, 10)]
	public List<string> newList = new List<string>();

	[TextArea(0, 10)]
	public List<string> changesList = new List<string>();

	[TextArea(0, 10)]
	public List<string> balancingList = new List<string>();

	[TextArea(0, 10)]
	public List<string> fixList = new List<string>();

	private void Discord()
	{
		string text = discordRole;
		text = text + "\n# ''R.E.P.O. " + title + "'' is now live! :taxman_laugh:";
		if (newList.Count > 0)
		{
			text += "\n\n";
			text += "## NEW";
			foreach (string @new in newList)
			{
				text = text + "\n> - " + @new;
			}
		}
		if (changesList.Count > 0)
		{
			text += "\n\n";
			text += "## CHANGES";
			foreach (string changes in changesList)
			{
				text = text + "\n> - " + changes;
			}
		}
		if (balancingList.Count > 0)
		{
			text += "\n\n";
			text += "## BALANCING";
			foreach (string balancing in balancingList)
			{
				text = text + "\n> - " + balancing;
			}
		}
		if (fixList.Count > 0)
		{
			text += "\n\n";
			text += "## FIXES";
			foreach (string fix in fixList)
			{
				text = text + "\n> - " + fix;
			}
		}
		text += "\n\n";
		text += "# __Thanks for helping us test! :smile~1:__";
		GUIUtility.systemCopyBuffer = text;
	}

	private void Steam()
	{
		string text = "[hr][/hr]";
		text += "[b][list]";
		if (newList.Count > 0)
		{
			text += "\n[*][url=#NEW]NEW[/url]";
		}
		if (changesList.Count > 0)
		{
			text += "\n[*][url=#CHANGES]CHANGES[/url]";
		}
		if (balancingList.Count > 0)
		{
			text += "\n[*][url=#BALANCING]BALANCING[/url]";
		}
		if (fixList.Count > 0)
		{
			text += "\n[*][url=#FIXES]FIXES[/url]";
		}
		text += "\n[/list][/b]";
		if (newList.Count > 0)
		{
			text += "[hr][/hr][h2=NEW]NEW[/h2][list]";
			foreach (string @new in newList)
			{
				text = text + "\n[*]" + @new;
			}
			text += "[/list]";
		}
		if (changesList.Count > 0)
		{
			text += "\n[hr][/hr][h2=CHANGES]CHANGES[/h2][list]";
			foreach (string changes in changesList)
			{
				text = text + "\n[*]" + changes;
			}
			text += "[/list]";
		}
		if (balancingList.Count > 0)
		{
			text += "\n[hr][/hr][h2=BALANCING]BALANCING[/h2][list]";
			foreach (string balancing in balancingList)
			{
				text = text + "\n[*]" + balancing;
			}
			text += "[/list]";
		}
		if (fixList.Count > 0)
		{
			text += "\n[hr][/hr][h2=FIXES]FIXES[/h2][list]";
			foreach (string fix in fixList)
			{
				text = text + "\n[*]" + fix;
			}
			text += "[/list]";
		}
		text += "\n[hr][/hr]";
		GUIUtility.systemCopyBuffer = text;
	}

	private void Notepad()
	{
		string text = "";
		if (newList.Count > 0)
		{
			text += "NEW";
			foreach (string @new in newList)
			{
				text = text + "\n- " + @new;
			}
		}
		if (changesList.Count > 0)
		{
			text += "\n\n";
			text += "CHANGES";
			foreach (string changes in changesList)
			{
				text = text + "\n- " + changes;
			}
		}
		if (balancingList.Count > 0)
		{
			text += "\n\n";
			text += "BALANCING";
			foreach (string balancing in balancingList)
			{
				text = text + "\n- " + balancing;
			}
		}
		if (fixList.Count > 0)
		{
			text += "\n\n";
			text += "FIXES";
			foreach (string fix in fixList)
			{
				text = text + "\n- " + fix;
			}
		}
		GUIUtility.systemCopyBuffer = text;
	}
}
