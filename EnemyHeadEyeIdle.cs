using UnityEngine;

public class EnemyHeadEyeIdle : MonoBehaviour
{
	public EnemyHeadEyeTarget EyeTarget;

	public float Speed;

	[Space]
	public float TimeMin;

	public float TimeMax;

	private float Timer;

	[Space]
	public float MinX;

	public float MaxX;

	private float CurrentX;

	[Space]
	public float MinY;

	public float MaxY;

	private float CurrentY;

	private void Update()
	{
		if (EyeTarget.Idle)
		{
			if (Timer <= 0f)
			{
				Timer = Random.Range(TimeMin, TimeMax);
				CurrentX = Random.Range(MinX, MaxX);
				CurrentY = Random.Range(MinY, MaxY);
			}
			else
			{
				Timer -= Time.deltaTime;
			}
		}
		else
		{
			CurrentX = 0f;
			CurrentY = 0f;
		}
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, new Vector3(CurrentX, CurrentY, base.transform.localPosition.z), Speed * Time.deltaTime);
	}
}
