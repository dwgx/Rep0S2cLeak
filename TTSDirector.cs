using LeastSquares.Overtone;
using UnityEngine;

public class TTSDirector : MonoBehaviour
{
	public static TTSDirector instance;

	internal TTSEngine engine;

	private void Start()
	{
		instance = this;
		engine = GetComponent<TTSEngine>();
	}
}
