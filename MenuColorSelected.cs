using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuColorSelected : MonoBehaviour
{
	public SpringVector3 positionSpring;

	internal Vector3 selectedPosition;

	public RawImage rawImage;

	private MenuPage parentPage;

	private bool goTime;

	private void Start()
	{
		parentPage = GetComponentInParent<MenuPage>();
		positionSpring = new SpringVector3();
		positionSpring.speed = 50f;
		positionSpring.damping = 0.55f;
		positionSpring.lastPosition = base.transform.position;
	}

	public void SetColor(Color color, Vector3 position)
	{
		StartCoroutine(SetColorRoutine(color, position));
	}

	private IEnumerator SetColorRoutine(Color color, Vector3 position)
	{
		yield return new WaitForSeconds(0.01f);
		rawImage.color = color;
		selectedPosition = position;
		goTime = true;
	}

	private void Update()
	{
		if (goTime && parentPage.currentPageState != MenuPage.PageState.Closing)
		{
			base.transform.position = SemiFunc.SpringVector3Get(positionSpring, selectedPosition + Vector3.up * 0.038f + Vector3.right * 0.046f);
			base.transform.position = new Vector3(base.transform.position.x + 18f, base.transform.position.y + 16f, 1f);
		}
	}
}
