using UnityEngine;

public class ToolHide : MonoBehaviour
{
	public ToolController ToolController;

	public AnimationCurve ShowCurve;

	public AnimationCurve ShowScaleCurve;

	public AnimationCurve HideCurve;

	public AnimationCurve HideScaleCurve;

	[HideInInspector]
	public bool Active;

	[HideInInspector]
	public float ActiveLerp;

	private float ShowTimer;

	public void Show()
	{
		ShowTimer = 0.02f;
		base.transform.localPosition = ToolController.CurrentHidePosition;
		base.transform.localRotation = Quaternion.Euler(ToolController.CurrentHideRotation.x, ToolController.CurrentHideRotation.y, ToolController.CurrentHideRotation.z);
		ActiveLerp = 0f;
		Active = true;
	}

	public void Hide()
	{
		ActiveLerp = 0f;
		Active = false;
	}

	private void Update()
	{
		if (ActiveLerp < 1f)
		{
			ActiveLerp += ToolController.CurrentHideSpeed * Time.deltaTime;
			ActiveLerp = Mathf.Clamp01(ActiveLerp);
			if (ActiveLerp >= 1f && !Active)
			{
				ToolController.HideTool();
			}
		}
		if (Active)
		{
			base.transform.localPosition = Vector3.LerpUnclamped(ToolController.CurrentHidePosition, new Vector3(0f, 0f, 0f), ShowCurve.Evaluate(ActiveLerp));
			base.transform.localRotation = Quaternion.LerpUnclamped(Quaternion.Euler(ToolController.CurrentHideRotation.x, ToolController.CurrentHideRotation.y, ToolController.CurrentHideRotation.z), Quaternion.Euler(0f, 0f, 0f), ShowCurve.Evaluate(ActiveLerp));
			base.transform.localScale = Vector3.LerpUnclamped(new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f), ShowScaleCurve.Evaluate(ActiveLerp));
		}
		else if (ActiveLerp < 1f)
		{
			base.transform.localPosition = Vector3.LerpUnclamped(new Vector3(0f, 0f, 0f), ToolController.CurrentHidePosition, HideCurve.Evaluate(ActiveLerp));
			base.transform.localRotation = Quaternion.LerpUnclamped(Quaternion.Euler(0f, 0f, 0f), Quaternion.Euler(ToolController.CurrentHideRotation.x, ToolController.CurrentHideRotation.y, ToolController.CurrentHideRotation.z), HideCurve.Evaluate(ActiveLerp));
			base.transform.localScale = Vector3.LerpUnclamped(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f), HideScaleCurve.Evaluate(ActiveLerp));
		}
		else
		{
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
		}
		if (ShowTimer > 0f)
		{
			ShowTimer -= 1f * Time.deltaTime;
			if (ShowTimer <= 0f)
			{
				ToolController.ShowTool();
			}
		}
	}
}
