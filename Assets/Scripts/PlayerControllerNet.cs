using Mirror;
using UnityEngine;
using StarterAssets;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Cinemachine;

// В инспекторе на корне игрока: NetworkTransform (ServerToClient, World, Interpolate=ON, UseFixedUpdate=OFF)

[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif
public class PlayerControllerNet : NetworkBehaviour
{
	#region Inspector_Fields
	[Header("Player")]
	public float MoveSpeed = 2.0f;
	public float SprintSpeed = 5.335f;
	[Range(0.0f, 0.3f)] public float RotationSmoothTime = 0.12f;
	public float SpeedChangeRate = 10.0f;
	public float JumpHeight = 1.2f;
	public float Gravity = -15.0f;
	public float JumpTimeout = 0.50f;
	public float FallTimeout = 0.15f;

	[Header("Grounded")]
	public bool Grounded = true;
	public float GroundedOffset = -0.14f;
	public float GroundedRadius = 0.28f;
	public LayerMask GroundLayers;

	[Header("Cinemachine")]
	public GameObject CinemachineCameraTarget; // локальный таргет из Starter Assets
	public float TopClamp = 70.0f;
	public float BottomClamp = -30.0f;
	public float CameraAngleOverride = 0.0f;
	public bool LockCameraPosition = false;

	[Header("Net Tuning")]
	[Min(5f)] public float SendRateHz = 20f;   // частота отправки ввода
	[Min(0f)] public float InputSmooth = 10f;  // сглаживание WASD/стика
	#endregion

	#region Cached_Components
	private StarterAssetsInputs _input;
	private PlayerInput _playerInput;
	private CharacterController _cc;
	private GameObject _mainCamera;
	private Animator _animator;
	private NetworkAnimator _netAnimator;
	#endregion

	#region Camera_State
	private float _cinYaw, _cinPitch;
	private const float _threshold = 0.01f;
	#endregion

	#region Movement_State
	private float _speed;
	private float _targetYaw, _rotVel;
	private float _verticalVelocity;
	private float _terminalVelocity = 53f;
	private float _jumpCd, _fallTimeout;
	#endregion

	#region Client_Input_Buffers
	private float _sendAccum;
	private Vector2 _smoothedMove;
	private bool _pendingJump; // латч прыжка до ближайшей отправки
	#endregion

	#region Server_Input_State
	private Vector3 _srvWishDir;
	private bool _srvSprint;
	private bool _srvJumpQueued;
	private float _srvMoveMag; // 0..1 — для MotionSpeed
	#endregion

	#region Unity_Lifecycle
	private void Awake()
	{
		_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
	}

	public override void OnStartLocalPlayer()
	{
		// привязка Cinemachine vcam к таргету персонажа
		var vcam = GameObject.FindGameObjectWithTag("PlayerFollowCamera")?.GetComponent<CinemachineVirtualCamera>();
		if (vcam != null)
			vcam.Follow = CinemachineCameraTarget ? CinemachineCameraTarget.transform : transform;

		// гарантируем LateUpdate для меньшего джиттера вида
		var mainCam = Camera.main;
		var brain = mainCam ? mainCam.GetComponent<CinemachineBrain>() : null;
		if (brain != null)
			brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;

		_cinYaw = CinemachineCameraTarget ? CinemachineCameraTarget.transform.eulerAngles.y : transform.eulerAngles.y;
	}

	private void Start()
	{
		_cc = GetComponent<CharacterController>();
		_input = GetComponent<StarterAssetsInputs>();
		_playerInput = GetComponent<PlayerInput>();
		_animator = GetComponent<Animator>();
		_netAnimator = GetComponent<NetworkAnimator>();
		if (_animator != null)
		{
			_animator.updateMode = AnimatorUpdateMode.Normal;
			_animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		}
		_fallTimeout = FallTimeout;
		EnsureInputEnabled(isLocalPlayer);
	}
	#endregion

	#region Helpers
	private void EnsureInputEnabled(bool enable)
	{
		if (_input) _input.enabled = enable;
		if (_playerInput) _playerInput.enabled = enable;
	}
	#endregion

	#region Client_Update
	[ClientCallback]
	private void Update()
	{
		if (!isLocalPlayer) return;

		// сгладим ввод (экспонента: мягкий старт/стоп)
		_smoothedMove = Vector2.Lerp(_smoothedMove, _input.move, 1f - Mathf.Exp(-InputSmooth * Time.deltaTime));
		bool sprint = _input.sprint;
		if (_input.jump) { _input.jump = false; _pendingJump = true; } // латчим до отправки

		// желаемое направление в мировой системе из камеры
		Vector3 camF = Vector3.ProjectOnPlane((_mainCamera ? _mainCamera.transform.forward : transform.forward), Vector3.up).normalized;
		Vector3 camR = Vector3.ProjectOnPlane((_mainCamera ? _mainCamera.transform.right   : transform.right),   Vector3.up).normalized;
		Vector3 wishDir = (camF * _smoothedMove.y + camR * _smoothedMove.x);
		if (wishDir.sqrMagnitude > 1e-6f) wishDir.Normalize();
		float moveMag = Mathf.Clamp01(_smoothedMove.magnitude);

		// отправляем ввод с троттлингом
		_sendAccum += Time.deltaTime;
		if (_sendAccum >= 1f / SendRateHz)
		{
			_sendAccum = 0f;
			bool sendJump = _pendingJump; // отправляем и сбрасываем латч
			_pendingJump = false;
			CmdSendInput(wishDir, sprint, sendJump, moveMag);
		}
	}
	#endregion

	#region Camera
	// локальная орбита камеры (как в Starter Assets)
	private void CameraOrbitLocal()
	{
		if (!isLocalPlayer || _input == null || LockCameraPosition || CinemachineCameraTarget == null) return;

		if (_input.look.sqrMagnitude >= _threshold)
		{
			bool mouse = (_playerInput?.currentControlScheme == "KeyboardMouse");
			float mul = mouse ? 1f : Time.deltaTime;
			_cinYaw   += _input.look.x * mul;
			_cinPitch += _input.look.y * mul;
		}
		_cinYaw   = ClampAngle(_cinYaw, float.MinValue, float.MaxValue);
		_cinPitch = ClampAngle(_cinPitch, BottomClamp, TopClamp);

		CinemachineCameraTarget.transform.rotation =
			Quaternion.Euler(_cinPitch + CameraAngleOverride, _cinYaw, 0.0f);
	}

	private void LateUpdate()
	{
		if (isLocalPlayer) CameraOrbitLocal(); // камера в LateUpdate — меньше джиттера
	}
	#endregion

	#region Server_Network
	[Command]
	void CmdSendInput(Vector3 wishDir, bool sprint, bool jump, float moveMag)
	{
		_srvWishDir = wishDir;
		_srvSprint = sprint;
		_srvMoveMag = moveMag;
		if (jump) _srvJumpQueued = true; // прыжок потребляется один раз в FixedUpdate
	}

	[ServerCallback]
	private void FixedUpdate()
	{
		float dt = Time.fixedDeltaTime;
		bool doJump = _srvJumpQueued;
		_srvJumpQueued = false;
		SimulateStep(_srvWishDir, _srvSprint, doJump, dt);

		// анимация — сервер авторитетен: обновляем параметры здесь
		if (_animator != null)
		{
			_animator.SetFloat("Speed", _speed);
			_animator.SetBool("Grounded", Grounded);
			_animator.SetFloat("MotionSpeed", _srvMoveMag);
		}
	}
	#endregion

	#region Simulation
	// единый шаг симуляции на сервере
	private void SimulateStep(Vector3 wishDir, bool sprint, bool jump, float dt)
	{
		// Grounded
		Vector3 p = new(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
		Grounded = Physics.CheckSphere(p, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		if (Grounded && _verticalVelocity < 0f) _verticalVelocity = -2f;

		// скорость
		float targetSpeed = (wishDir.sqrMagnitude > 1e-6f ? (sprint ? SprintSpeed : MoveSpeed) : 0f);
		_speed = Mathf.Lerp(_speed, targetSpeed, dt * Mathf.Max(0.001f, SpeedChangeRate));

		// поворот
		if (wishDir.sqrMagnitude > 1e-6f)
		{
			float desiredYaw = Mathf.Atan2(wishDir.x, wishDir.z) * Mathf.Rad2Deg;
			_targetYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, desiredYaw, ref _rotVel, RotationSmoothTime);
			transform.rotation = Quaternion.Euler(0f, _targetYaw, 0f);
		}

		// прыжок / гравитация
		if (Grounded)
		{
			_fallTimeout = FallTimeout;
			// таймер окна прыжка убывает только на земле, как в StarterAssets
			if (_jumpCd > 0f) _jumpCd -= dt;
			// по умолчанию, на земле флаги: Jump=false, FreeFall=false
			if (_animator != null)
			{
				_animator.SetBool("FreeFall", false);
				_animator.SetBool("Jump", false);
			}
			if (jump && _jumpCd <= 0f)
			{
				_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				_jumpCd = JumpTimeout;
				// установить Jump=true только в кадре начала прыжка
				if (_animator != null) _animator.SetBool("Jump", true);
			}
		}
		else
		{
			if (_fallTimeout > 0f) _fallTimeout -= dt;
			else
			{
				if (_animator != null) _animator.SetBool("FreeFall", true);
			}
			// в воздухе — как в StarterAssets — сбрасываем окно прыжка
			_jumpCd = JumpTimeout;
		}
		if (_verticalVelocity < _terminalVelocity)
			_verticalVelocity += Gravity * dt;

		// перемещение
		Vector3 motion = wishDir * (_speed * dt) + Vector3.up * (_verticalVelocity * dt);
		_cc.Move(motion);
	}
	#endregion

	#region Utils
	private static float ClampAngle(float a, float min, float max)
	{
		if (a < -360f) a += 360f;
		if (a >  360f) a -= 360f;
		return Mathf.Clamp(a, min, max);
	}

	// заглушки для AnimationEvent из Starter Assets (чтобы не было ошибок в логе)
	private void OnFootstep(AnimationEvent animationEvent) { }
	private void OnLand(AnimationEvent animationEvent) { }
	#endregion
}
