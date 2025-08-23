using Mirror;
using UnityEngine;

/// <summary>
/// Серверный драйвер анимации: централизует установку параметров Animator.
/// Если компонента нет, можно обновлять аниматор напрямую в моторе.
/// </summary>
public sealed class PlayerAnimatorDriver : NetworkBehaviour
{
	[SerializeField] private Animator animator;

	private static readonly int SpeedHash = Animator.StringToHash("Speed");
	private static readonly int GroundedHash = Animator.StringToHash("Grounded");
	private static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");
	private static readonly int FreeFallHash = Animator.StringToHash("FreeFall");
	private static readonly int JumpHash = Animator.StringToHash("Jump");

	public override void OnStartServer()
	{
		if (animator == null) animator = GetComponent<Animator>();
		enabled = true;
	}

	[Server]
	public void Apply(float speed, bool grounded, float motionSpeed)
	{
		if (animator == null) return;
		animator.SetFloat(SpeedHash, speed);
		animator.SetBool(GroundedHash, grounded);
		animator.SetFloat(MotionSpeedHash, motionSpeed);
	}

	[Server]
	public void SetFreeFall(bool value)
	{
		if (animator == null) return;
		animator.SetBool(FreeFallHash, value);
	}

	[Server]
	public void SetJump(bool value)
	{
		if (animator == null) return;
		animator.SetBool(JumpHash, value);
	}

	[Server]
	public void SetJumpTrigger()
	{
		if (animator == null) return;
		animator.SetBool(JumpHash, true);
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


