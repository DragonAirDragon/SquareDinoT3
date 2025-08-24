using UnityEngine;
namespace SquareDinoT3.Configs
{
	/// <summary>
	/// Config for player movement
	/// </summary>
	[CreateAssetMenu(fileName = "MovementConfig", menuName = "Configs/Movement Config")]
	public class MovementConfig : ScriptableObject
	{
		[Header("Movement")]
		public float MoveSpeed = 2.0f;
		public float SprintSpeed = 5.335f;
		[Range(0.0f, 0.3f)] public float RotationSmoothTime = 0.12f;
		public float SpeedChangeRate = 10.0f;
		public float JumpHeight = 1.2f;
		public float Gravity = -15.0f;
		public float JumpTimeout = 0.50f;
		public float FallTimeout = 0.15f;

		[Header("Grounded")]
		public float GroundedOffset = -0.14f;
		public float GroundedRadius = 0.28f;
		public LayerMask GroundedLayers;
	}
}

