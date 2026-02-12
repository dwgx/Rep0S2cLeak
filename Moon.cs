using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMoon", menuName = "Other/Moon")]
public class Moon : ScriptableObject
{
	[Space]
	public string moonName = "N/A";

	public Texture moonIcon;

	[Space]
	public List<string> moonAttributes;
}
