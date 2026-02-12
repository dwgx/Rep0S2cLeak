using UnityEngine;

public class ArrowUI : MonoBehaviour
{
	public static ArrowUI instance;

	public AnimationCurve arrowCurveMove;

	private float arrowCurveMoveEval;

	public AnimationCurve arrowCurveBop;

	private float showArrowTimer;

	private Vector3 startPosition;

	private Vector3 endPosition;

	private float endRotation;

	private bool endShow;

	public MeshRenderer arrowMesh;

	private float bopEval;

	private bool targetWorldPos;

	private Camera mainCamera;

	private void Awake()
	{
		instance = this;
		arrowMesh.enabled = false;
		mainCamera = Camera.main;
	}

	public void ArrowShow(Vector3 startPos, Vector3 endPos, float rotation)
	{
		if (endPosition != endPos)
		{
			arrowCurveMoveEval = 0f;
			base.transform.localPosition = startPos;
		}
		startPos.z = 0f;
		endPos.z = 0f;
		targetWorldPos = false;
		startPosition = startPos;
		endPosition = endPos;
		endRotation = rotation;
		showArrowTimer = 0.2f;
	}

	public void ArrowShowWorldPos(Vector3 startPos, Vector3 endPos, float rotation)
	{
		if (endPosition != endPos)
		{
			arrowCurveMoveEval = 0f;
			base.transform.position = startPos;
		}
		targetWorldPos = true;
		startPosition = startPos;
		endPosition = endPos;
		endRotation = rotation;
		showArrowTimer = 0.2f;
	}

	private void Update()
	{
		if (targetWorldPos)
		{
			endPosition = mainCamera.WorldToScreenPoint(endPosition).normalized;
			endPosition.z = 0f;
		}
		bopEval += Time.deltaTime;
		bopEval = Mathf.Clamp01(bopEval);
		float num = arrowCurveBop.Evaluate(bopEval);
		arrowMesh.transform.localPosition = new Vector3(-51f + -30f * num, 0f, 0f);
		if (bopEval >= 1f)
		{
			bopEval = 0f;
		}
		if (showArrowTimer > 0f)
		{
			arrowMesh.enabled = true;
			endShow = false;
			showArrowTimer -= Time.deltaTime;
			arrowCurveMoveEval += Time.deltaTime;
			arrowCurveMoveEval = Mathf.Clamp01(arrowCurveMoveEval);
			float t = arrowCurveMove.Evaluate(arrowCurveMoveEval);
			base.transform.localPosition = Vector3.LerpUnclamped(startPosition, endPosition, t);
			base.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(90f, endRotation, t));
			float num2 = arrowCurveMove.Evaluate(arrowCurveMoveEval * 2f);
			base.transform.localScale = new Vector3(num2, num2, num2);
			return;
		}
		if (!endShow)
		{
			arrowCurveMoveEval = 0f;
			endShow = true;
		}
		if (arrowCurveMoveEval >= 1f)
		{
			arrowMesh.enabled = false;
			arrowCurveMoveEval = 1f;
			return;
		}
		arrowCurveMoveEval += Time.deltaTime * 4f;
		float num3 = arrowCurveMove.Evaluate(arrowCurveMoveEval);
		base.transform.localScale = new Vector3(1f - num3, 1f - num3, 1f - num3);
		startPosition = Vector3.zero;
		endPosition = Vector3.one;
	}
}
