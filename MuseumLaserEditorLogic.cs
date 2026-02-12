using UnityEngine;

[ExecuteAlways]
public class MuseumLaserEditorLogic : MonoBehaviour
{
	public Transform laserBall1Transform;

	public Transform laserBall2Transform;

	public Transform laserBeam1Transform;

	private void Update()
	{
		if (Application.isEditor)
		{
			laserBall1Transform.LookAt(laserBall2Transform);
			laserBall2Transform.LookAt(laserBall1Transform);
			Vector3 vector = new Vector3(0.1f, 0.1f, 0.1f);
			vector.z = Vector3.Distance(laserBall1Transform.position, laserBall2Transform.position);
			Vector3 localScale = new Vector3(1f, 1f, 0f);
			localScale.z = Vector3.Distance(laserBall1Transform.position, laserBall2Transform.position) / 2f;
			laserBeam1Transform.localScale = localScale;
		}
	}
}
