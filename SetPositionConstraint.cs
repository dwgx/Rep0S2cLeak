using UnityEngine;
using UnityEngine.Animations;

public class SetPositionConstraint : MonoBehaviour
{
	private void Start()
	{
		GetComponent<PositionConstraint>().constraintActive = true;
	}
}
