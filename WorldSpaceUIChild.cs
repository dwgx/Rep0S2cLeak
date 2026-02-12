using UnityEngine;

public class WorldSpaceUIChild : MonoBehaviour
{
	internal Vector3 worldPosition;

	private RectTransform myRect;

	internal Vector3 positionOffset;

	protected virtual void Start()
	{
		myRect = GetComponent<RectTransform>();
		SetPosition();
	}

	protected virtual void Update()
	{
		SetPosition();
	}

	private void SetPosition()
	{
		Vector3 vector = SemiFunc.UIWorldToCanvasPosition(worldPosition);
		myRect.anchoredPosition = vector + positionOffset;
	}
}
