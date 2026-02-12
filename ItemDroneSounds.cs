using UnityEngine;

[CreateAssetMenu(fileName = "Item Drone Sounds", menuName = "Item Sounds/Drone")]
public class ItemDroneSounds : ScriptableObject
{
	public Sound DroneStart;

	public Sound DroneEnd;

	public Sound DroneDeploy;

	public Sound DroneRetract;

	public Sound DroneLoop;

	public Sound DroneBeamLoop;
}
