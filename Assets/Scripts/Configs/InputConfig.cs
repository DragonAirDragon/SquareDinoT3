using UnityEngine;

[CreateAssetMenu(fileName = "InputConfig", menuName = "Configs/Input Config")]
public class InputConfig : ScriptableObject
{
	[Header("Client Input")]
	[Min(0f)] public float inputSmooth = 50f;

	[Header("Camera")]
	public float topClamp = 70f;
	public float bottomClamp = -30f;
	public float cameraAngleOverride = 0f;
	public bool lockCursor = true;
}


