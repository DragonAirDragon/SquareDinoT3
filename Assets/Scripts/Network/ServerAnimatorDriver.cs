using Mirror;
using UnityEngine;

namespace SquareDinoT3.Network
{
	/// <summary>
	/// Server animator driver: centralizes setting of Animator parameters.
	/// </summary>
	public sealed class ServerAnimatorDriver : NetworkBehaviour
	{
		[SerializeField] private Animator _animator;

		// Hashes for animator parameters
		private static readonly int SpeedHash = Animator.StringToHash("Speed");
		private static readonly int GroundedHash = Animator.StringToHash("Grounded");
		private static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");
		private static readonly int FreeFallHash = Animator.StringToHash("FreeFall");
		private static readonly int JumpHash = Animator.StringToHash("Jump");

		public override void OnStartServer()
		{
			if (_animator == null) _animator = GetComponent<Animator>();
			enabled = true;
		}

		[Server]
		public void Apply(float speed, bool grounded, float motionSpeed)
		{
			if (_animator == null) return;
			_animator.SetFloat(SpeedHash, speed);
			_animator.SetBool(GroundedHash, grounded);
			_animator.SetFloat(MotionSpeedHash, motionSpeed);
		}

		[Server]
		public void SetFreeFall(bool value)
		{
			if (_animator == null) return;
			_animator.SetBool(FreeFallHash, value);
		}

		[Server]
		public void SetJump(bool value)
		{
			if (_animator == null) return;
			_animator.SetBool(JumpHash, value);
		}

		[Server]
		public void SetJumpTrigger()
		{
			if (_animator == null) return;
			_animator.SetBool(JumpHash, true);
		}

		private void OnLand()
		{
		}

		private void OnJumpLand()
		{
		}
		private void OnFootstep()
		{
		}
	}
}

