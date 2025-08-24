using Mirror;
using SquareDinoT3.Configs;
using UnityEngine;
using VContainer;

namespace SquareDinoT3.Network
{
	/// <summary>
	/// Server character motor: simulates movement. Accepts input, performs simulation step.
	/// Updates animation parameters (or delegates to Driver).
	/// </summary>
	[RequireComponent(typeof(CharacterController))]
	public sealed class ServerCharacterMotor : NetworkBehaviour
	{
		// Configs
		private MovementConfig _movementConfig;
		// Cached
		private bool _grounded = true;
		private CharacterController _cc;
		private Animator _animator;
		private ServerAnimatorDriver _animDriver;

		// Input (last received from client)
		private Vector3 _wishDir;
		private bool _sprint;
		private bool _jumpQueued;
		private float _moveMagnitude;

		// Sim state
		private float _speed;
		private float _targetYaw, _rotVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53f;
		private float _jumpCooldown;
		private float _fallTimer;

		[Inject]
		public void Construct(MovementConfig movementConfig)
		{
			_movementConfig = movementConfig;
		}

		public override void OnStartServer()
		{
			_cc = GetComponent<CharacterController>();
			_animator = GetComponent<Animator>();
			_animDriver = GetComponent<ServerAnimatorDriver>();
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
				_animDriver.Apply(_speed, _grounded, _moveMagnitude);
			}
			else if (_animator != null)
			{
				_animator.SetFloat("Speed", _speed);
				_animator.SetBool("Grounded", _grounded);
				_animator.SetFloat("MotionSpeed", _moveMagnitude);
			}
		}

		private void SimulateStep(Vector3 wishDir, bool sprint, bool jump, float dt)
		{
			if (_movementConfig == null) return;

			// grounded
			Vector3 p = new Vector3(transform.position.x, transform.position.y - _movementConfig.GroundedOffset, transform.position.z);
			_grounded = Physics.CheckSphere(p, _movementConfig.GroundedRadius, _movementConfig.GroundedLayers, QueryTriggerInteraction.Ignore);
			if (_grounded && _verticalVelocity < 0f) _verticalVelocity = -2f;

			// speed
			float baseSpeed = sprint ? _movementConfig.SprintSpeed : _movementConfig.MoveSpeed;
			float targetSpeed = wishDir.sqrMagnitude > 1e-6f ? baseSpeed : 0f;
			_speed = Mathf.Lerp(_speed, targetSpeed, dt * Mathf.Max(0.001f, _movementConfig.SpeedChangeRate));

			// rotation
			if (wishDir.sqrMagnitude > 1e-6f)
			{
				float desiredYaw = Mathf.Atan2(wishDir.x, wishDir.z) * Mathf.Rad2Deg;
				_targetYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, desiredYaw, ref _rotVelocity, _movementConfig.RotationSmoothTime);
				transform.rotation = Quaternion.Euler(0f, _targetYaw, 0f);
			}

			// jump / gravity
			if (_grounded)
			{
				_fallTimer = _movementConfig.FallTimeout;
				if (_jumpCooldown > 0f) _jumpCooldown -= dt;
				// by default, on ground flags: Jump=false, FreeFall=false
				if (_animDriver != null) { _animDriver.SetFreeFall(false); _animDriver.SetJump(false); }
				else if (_animator != null) { _animator.SetBool("FreeFall", false); _animator.SetBool("Jump", false); }
				if (jump && _jumpCooldown <= 0f)
				{
					_verticalVelocity = Mathf.Sqrt(_movementConfig.JumpHeight * -2f * _movementConfig.Gravity);
					_jumpCooldown = _movementConfig.JumpTimeout;
					if (_animDriver != null) _animDriver.SetJumpTrigger();
					else if (_animator != null) _animator.SetBool("Jump", true);
				}
			}
			else
			{
				if (_fallTimer > 0f) _fallTimer -= dt;
				else { if (_animDriver != null) _animDriver.SetFreeFall(true); else if (_animator != null) _animator.SetBool("FreeFall", true); }
				_jumpCooldown = _movementConfig.JumpTimeout;
			}
			if (_verticalVelocity < _terminalVelocity) _verticalVelocity += _movementConfig.Gravity * dt;

			// movement
			Vector3 motion = wishDir * (_speed * dt) + Vector3.up * (_verticalVelocity * dt);
			_cc.Move(motion);
		}
	}

}
