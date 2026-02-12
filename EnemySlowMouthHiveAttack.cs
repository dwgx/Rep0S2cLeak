using UnityEngine;

public class EnemySlowMouthHiveAttack : MonoBehaviour
{
	public Transform hitPositionTransform;

	public Transform blobTransform;

	public Transform blobMeshTransform;

	public AnimationCurve flyUpCurve;

	private float curveProgress;

	private Vector3 prevCheckPosition;

	public EnemyParent enemyParent;

	private void Start()
	{
	}

	private void Update()
	{
		Vector3 position = base.transform.position;
		Vector3 position2 = hitPositionTransform.position;
		if (curveProgress < 1f)
		{
			curveProgress += Time.deltaTime * 2f;
			Vector3 position3 = Vector3.Lerp(position, position2, curveProgress);
			blobTransform.position = position3;
			float num = flyUpCurve.Evaluate(curveProgress);
			blobTransform.position += Vector3.up * 2f * num;
			if (!(Vector3.Distance(prevCheckPosition, blobTransform.position) > 0.5f))
			{
				return;
			}
			Collider[] array = Physics.OverlapSphere(blobTransform.position, blobMeshTransform.localScale.x / 2f, SemiFunc.LayerMaskGetShouldHits());
			for (int i = 0; i < array.Length; i++)
			{
				EnemyParent componentInParent = array[i].GetComponentInParent<EnemyParent>();
				if (!componentInParent || !(componentInParent == enemyParent))
				{
					Splat();
					break;
				}
			}
			prevCheckPosition = blobTransform.position;
		}
		else
		{
			curveProgress = 0f;
		}
	}

	private void Splat()
	{
		curveProgress = 0f;
	}
}
