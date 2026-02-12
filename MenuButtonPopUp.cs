using UnityEngine;
using UnityEngine.Events;

public class MenuButtonPopUp : MonoBehaviour
{
	public bool richText = true;

	[Space]
	public UnityEvent option1Event;

	public UnityEvent option2Event;

	public string headerText = "Oh really?";

	public Color headerColor = new Color(1f, 0.55f, 0f);

	[TextArea(3, 10)]
	public string bodyText = "Is that really so?";

	public string option1Text = "Yes!";

	public string option2Text = "No";
}
