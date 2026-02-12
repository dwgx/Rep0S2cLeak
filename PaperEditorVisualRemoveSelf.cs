using UnityEngine;

public class PaperEditorVisualRemoveSelf : MonoBehaviour
{
	private void Start()
	{
		Object.Destroy(base.gameObject);
	}
}
