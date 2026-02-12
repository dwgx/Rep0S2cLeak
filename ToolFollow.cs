using UnityEngine;

public class ToolFollow : MonoBehaviour
{
	public CameraBob CameraBob;

	private Vector3 StartPosition;

	private Vector3 StartRotation;

	private bool Active;

	public void Activate()
	{
		Active = true;
	}

	public void Deactivate()
	{
		Active = false;
	}

	private void Start()
	{
		StartPosition = base.transform.localPosition;
		StartRotation = base.transform.localEulerAngles;
	}

	private void Update()
	{
		if (Active)
		{
			base.transform.localPosition = new Vector3(StartPosition.x, StartPosition.y + CameraBob.transform.localPosition.y * 0.1f, StartPosition.z);
			base.transform.localRotation = Quaternion.Euler(StartRotation.x + CameraBob.transform.localPosition.y * 25f, StartRotation.y + CameraBob.transform.localEulerAngles.y * 2f, StartRotation.z + CameraBob.transform.localEulerAngles.z * 15f);
		}
		else
		{
			base.transform.localPosition = StartPosition;
			base.transform.localRotation = Quaternion.Euler(StartRotation);
		}
	}
}
