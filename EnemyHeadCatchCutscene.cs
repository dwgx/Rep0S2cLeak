using UnityEngine;

public class EnemyHeadCatchCutscene : MonoBehaviour
{
	public Sound BiteBegin;

	public Sound BiteFirst;

	public Sound BiteLast;

	[Space]
	public Sound Music01;

	public Sound Music02;

	[Space]
	public Sound CameraGlitchLong01;

	public Sound CameraGlitchLong02;

	[Space]
	public Sound CameraGlitchShort01;

	public Sound CameraGlitchShort02;

	[Space]
	public Sound GlassBreak01;

	public Sound GlassBreak02;

	public Sound GlassTension;

	public void PlayBiteBegin()
	{
		BiteBegin.Play(base.transform.position);
	}

	public void PlayBiteFirst()
	{
		BiteFirst.Play(base.transform.position);
		CameraGlitchShort01.Play(base.transform.position);
		GlassBreak01.Play(base.transform.position);
	}

	public void PlayBiteLast()
	{
		BiteLast.Play(base.transform.position);
		CameraGlitchShort02.Play(base.transform.position);
		GlassBreak02.Play(base.transform.position);
	}

	public void PlayMusic01()
	{
		Music01.Play(base.transform.position);
	}

	public void PlayMusic02()
	{
		Music02.Play(base.transform.position);
	}

	public void PlayCameraGlitchLong01()
	{
		CameraGlitchLong01.Play(base.transform.position);
	}

	public void PlayCameraGlitchLong02()
	{
		CameraGlitchLong02.Play(base.transform.position);
	}

	public void PlayGlassTension()
	{
		GlassTension.Play(base.transform.position);
	}
}
