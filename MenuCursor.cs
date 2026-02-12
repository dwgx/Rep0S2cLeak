using UnityEngine;

public class MenuCursor : MonoBehaviour
{
	private float showTimer;

	private GameObject mesh;

	public static MenuCursor instance;

	private float overridePosTimer;

	private void Start()
	{
		mesh = base.transform.GetChild(0).gameObject;
		base.transform.localScale = Vector3.zero;
		mesh.SetActive(value: false);
		instance = this;
	}

	public void OverridePosition(Vector3 _position)
	{
		base.transform.localPosition = new Vector3(_position.x, _position.y, 0f);
		overridePosTimer = 0.1f;
	}

	private void Update()
	{
		if (overridePosTimer <= 0f)
		{
			if (Cursor.lockState == CursorLockMode.None)
			{
				Vector2 vector = SemiFunc.UIMousePosToUIPos();
				base.transform.localPosition = new Vector3(vector.x, vector.y, 0f);
			}
		}
		else
		{
			overridePosTimer -= Time.deltaTime;
		}
		if (showTimer > 0f)
		{
			if (!mesh.activeSelf)
			{
				mesh.SetActive(value: true);
			}
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, Vector3.one, Time.deltaTime * 30f);
			showTimer -= Time.deltaTime;
		}
		else if (mesh.activeSelf)
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, Vector3.zero, Time.deltaTime * 30f);
			if (base.transform.localScale.magnitude < 0.1f && mesh.activeSelf)
			{
				mesh.SetActive(value: false);
			}
		}
	}

	public void Show()
	{
		showTimer = 0.01f;
	}
}
