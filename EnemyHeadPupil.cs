using UnityEngine;

public class EnemyHeadPupil : MonoBehaviour
{
	public EnemyHeadEyeTarget EyeTarget;

	public bool Active = true;

	private void Update()
	{
		if (Active)
		{
			base.transform.localScale = new Vector3(EyeTarget.PupilCurrentSize, base.transform.localScale.y, EyeTarget.PupilCurrentSize);
		}
	}
}
