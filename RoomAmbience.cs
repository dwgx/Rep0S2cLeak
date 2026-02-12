using UnityEngine;

[CreateAssetMenu(fileName = "Room Ambience - _____", menuName = "Audio/Room Ambience", order = 0)]
public class RoomAmbience : ScriptableObject
{
	[Range(0f, 2f)]
	public float volume = 1f;
}
