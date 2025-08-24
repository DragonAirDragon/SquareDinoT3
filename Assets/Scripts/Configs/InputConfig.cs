using UnityEngine;
namespace SquareDinoT3.Configs
{
	/// <summary>
	/// Config for input sender
	/// </summary>
	[CreateAssetMenu(fileName = "InputConfig", menuName = "Configs/Input Config")]
	public class InputConfig : ScriptableObject
	{
		[Header("Client Input")]
		[Min(0f)] public float InputSmooth = 50f;

		[Header("Camera")]
		public float TopClamp = 70f;
		public float BottomClamp = -30f;
		public float CameraAngleOverride = 0f;
		public bool LockCursor = true;
	}
}


