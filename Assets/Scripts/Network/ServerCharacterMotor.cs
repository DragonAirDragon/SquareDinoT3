using Mirror;
using UnityEngine;

/// <summary>
/// Серверная симуляция перемещения персонажа. Принимает ввод, выполняет шаг симуляции.
/// Обновляет анимационные параметры (или делегирует это Driver-у).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public sealed class ServerCharacterMotor : NetworkBehaviour
{
	[Header("Config")]
	[SerializeField] private MovementConfig movement;

	[Header("Runtime State")]
	public bool grounded = true;

	private CharacterController _cc;
	private Animator _animator;
	private PlayerAnimatorDriver _animDriver;

	// input (последний принятый от клиента)
	private Vector3 _wishDir;
	private bool _sprint;
	private bool _jumpQueued;
	private float _moveMagnitude;

	// sim state
	private float _speed;
	private float _targetYaw, _rotVelocity;
	private float _verticalVelocity;
	private float _terminalVelocity = 53f;
	private float _jumpCooldown;
	private float _fallTimer;

	public override void OnStartServer()
	{
		_cc = GetComponent<CharacterController>();
		_animator = GetComponent<Animator>();
		_animDriver = GetComponent<PlayerAnimatorDriver>();
		enabled = true;
	}

	public void ApplyInput(Vector3 wishDir, bool sprint, bool jump, float moveMag)
	{
		if (!isServer) return;
		_wishDir = wishDir;
		_sprint = sprint;
		_moveMagnitude = moveMag;
		if (jump) _jumpQueued = true;
	}

	[ServerCallback]
	private void FixedUpdate()
	{
		float dt = Time.fixedDeltaTime;
		bool doJump = _jumpQueued;
		_jumpQueued = false;
		SimulateStep(_wishDir, _sprint, doJump, dt);

		// Обновление анимации
		if (_animDriver != null)
		{
			_animDriver.Apply(_speed, grounded, _moveMagnitude);
		}
		else if (_animator != null)
		{
			_animator.SetFloat("Speed", _speed);
			_animator.SetBool("Grounded", grounded);
			_animator.SetFloat("MotionSpeed", _moveMagnitude);
		}
	}

	private void SimulateStep(Vector3 wishDir, bool sprint, bool jump, float dt)
	{
		if (movement == null) return;

		// grounded
		Vector3 p = new Vector3(transform.position.x, transform.position.y - movement.groundedOffset, transform.position.z);
		grounded = Physics.CheckSphere(p, movement.groundedRadius, movement.groundedLayers, QueryTriggerInteraction.Ignore);
		if (grounded && _verticalVelocity < 0f) _verticalVelocity = -2f;

		// скорость
		float baseSpeed = sprint ? movement.sprintSpeed : movement.moveSpeed;
		float targetSpeed = wishDir.sqrMagnitude > 1e-6f ? baseSpeed : 0f;
		_speed = Mathf.Lerp(_speed, targetSpeed, dt * Mathf.Max(0.001f, movement.speedChangeRate));

		// поворот
		if (wishDir.sqrMagnitude > 1e-6f)
		{
			float desiredYaw = Mathf.Atan2(wishDir.x, wishDir.z) * Mathf.Rad2Deg;
			_targetYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, desiredYaw, ref _rotVelocity, movement.rotationSmoothTime);
			transform.rotation = Quaternion.Euler(0f, _targetYaw, 0f);
		}

		// прыжок / гравитация
		if (grounded)
		{
			_fallTimer = movement.fallTimeout;
			if (_jumpCooldown > 0f) _jumpCooldown -= dt;
			// по умолчанию, на земле флаги: Jump=false, FreeFall=false
			if (_animDriver != null) { _animDriver.SetFreeFall(false); _animDriver.SetJump(false); }
			else if (_animator != null) { _animator.SetBool("FreeFall", false); _animator.SetBool("Jump", false); }
			if (jump && _jumpCooldown <= 0f)
			{
				_verticalVelocity = Mathf.Sqrt(movement.jumpHeight * -2f * movement.gravity);
				_jumpCooldown = movement.jumpTimeout;
				if (_animDriver != null) _animDriver.SetJumpTrigger();
				else if (_animator != null) _animator.SetBool("Jump", true);
			}
		}
		else
		{
			if (_fallTimer > 0f) _fallTimer -= dt;
			else { if (_animDriver != null) _animDriver.SetFreeFall(true); else if (_animator != null) _animator.SetBool("FreeFall", true); }
			_jumpCooldown = movement.jumpTimeout;
		}
		if (_verticalVelocity < _terminalVelocity) _verticalVelocity += movement.gravity * dt;

		// перемещение
		Vector3 motion = wishDir * (_speed * dt) + Vector3.up * (_verticalVelocity * dt);
		_cc.Move(motion);
	}
}


