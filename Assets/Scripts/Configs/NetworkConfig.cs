using UnityEngine;

[CreateAssetMenu(fileName = "NetworkConfig", menuName = "Configs/Network Config")]
public class NetworkConfig : ScriptableObject
{
	[Header("Rates")]
	[Min(1f)] public float sendRateHz = 60f;


    /*
	[Header("Transform Sync")]
	public bool useFixedUpdate = false;
	public bool interpolatePosition = true;
	public bool interpolateRotation = true;
	public float positionPrecision = 0.02f;
	public float rotationSensitivity = 0.1f;
    */
}


