using System.Collections.Generic;
using UnityEngine;

public class ArenaPlatform : MonoBehaviour
{
	public enum States
	{
		Idle,
		Warning,
		GoDown,
		End
	}

	private List<ArenaLight> lights;

	internal States currentState;

	private bool stateStart;

	private float stateTimer;

	public MeshRenderer meshRenderer;

	[Space]
	public DirtFinderMapFloor[] map;

	private void Start()
	{
		lights = new List<ArenaLight>();
		lights.AddRange(GetComponentsInChildren<ArenaLight>());
		meshRenderer.material.SetColor("_EmissionColor", Color.black);
	}

	private void StateIdle()
	{
		if (stateStart)
		{
			stateStart = false;
		}
	}

	private void StateWarning()
	{
		if (stateStart)
		{
			stateStart = false;
			lights.ForEach(delegate(ArenaLight light)
			{
				light.TurnOnArenaWarningLight();
			});
			meshRenderer.material.SetColor("_EmissionColor", Color.red);
		}
		Color color = new Color(0.3f, 0f, 0f);
		meshRenderer.material.SetColor("_EmissionColor", Color.Lerp(meshRenderer.material.GetColor("_EmissionColor"), color, Time.deltaTime * 2f));
	}

	private void StateGoDown()
	{
		if (stateStart)
		{
			DirtFinderMapFloor[] array = map;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].MapObject.Hide();
			}
			stateStart = false;
		}
		if (base.transform.position.y > -60f)
		{
			base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y - 30f * Time.deltaTime, base.transform.position.z);
		}
		else
		{
			StateSet(States.End);
		}
	}

	private void StateEnd()
	{
		if (stateStart)
		{
			stateStart = false;
		}
		Object.Destroy(base.gameObject);
	}

	private void StateMachine()
	{
		switch (currentState)
		{
		case States.Idle:
			StateIdle();
			break;
		case States.Warning:
			StateWarning();
			break;
		case States.GoDown:
			StateGoDown();
			break;
		case States.End:
			StateEnd();
			break;
		}
	}

	private void Update()
	{
		StateMachine();
		if (stateTimer > 0f)
		{
			stateTimer -= Time.deltaTime;
		}
	}

	public void PulsateLights()
	{
		lights.ForEach(delegate(ArenaLight light)
		{
			light.PulsateLight();
		});
		meshRenderer.material.SetColor("_EmissionColor", Color.red);
	}

	public void StateSet(States state)
	{
		currentState = state;
		stateStart = true;
	}
}
