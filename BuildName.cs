using TMPro;
using UnityEngine;

public class BuildName : MonoBehaviour
{
	private void Start()
	{
		GetComponent<TextMeshProUGUI>().text = BuildManager.instance.version.title;
	}
}
