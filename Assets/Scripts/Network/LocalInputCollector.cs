using Mirror;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

/// <summary>
/// Собирает локальный ввод, сглаживает его и отправляет на сервер.
/// Работает только у локального игрока.
/// </summary>
[RequireComponent(typeof(StarterAssetsInputs))]
[RequireComponent(typeof(PlayerInput))]
public sealed class LocalInputCollector : NetworkBehaviour
{
	// Configs
	private NetworkConfig networkConfig;
	private InputConfig inputConfig;

	// Net Tuning
	private float sendRateHz = 60f;
	private float inputSmooth = 50f;
	// Move Space
	private Transform moveSpaceOverride;
	// Input
	private StarterAssetsInputs _input;
	private PlayerInput _playerInput;
	// Cached
	private float _sendAccumulator;
	private Vector2 _smoothedMove;
	private bool _pendingJump;

	[Inject]
	public void Construct(NetworkConfig networkConfig, InputConfig inputConfig)
	{
		this.networkConfig = networkConfig;
		this.inputConfig = inputConfig;
	}


	public override void OnStartLocalPlayer()
	{
		_input = GetComponent<StarterAssetsInputs>();
		_playerInput = GetComponent<PlayerInput>();

		// применяем конфиги
		if (networkConfig != null)
		{
			sendRateHz = Mathf.Max(1f, networkConfig.sendRateHz);
		}
		if (inputConfig != null)
		{
			inputSmooth = Mathf.Max(0f, inputConfig.inputSmooth);
		}

		if (_input != null) _input.enabled = true;
		if (_playerInput != null) _playerInput.enabled = true;
		enabled = true;
	}

	public override void OnStopLocalPlayer()
	{
		if (_playerInput != null) _playerInput.enabled = false;
		if (_input != null) _input.enabled = false;
		_sendAccumulator = 0f;
		_smoothedMove = Vector2.zero;
		_pendingJump = false;
		enabled = false;
	}

	[ClientCallback]
	private void Update()
	{
		if (!isLocalPlayer || _input == null) return;

		_smoothedMove = Vector2.Lerp(
			_smoothedMove,
			_input.move,
			1f - Mathf.Exp(-Mathf.Max(0f, inputSmooth) * Time.deltaTime)
		);

		bool sprint = _input.sprint;
		if (_input.jump)
		{
			_input.jump = false;
			_pendingJump = true; // латчим до отправки
		}

		Vector3 forward;
		Vector3 right;
		{
			Transform basis = Camera.main ? Camera.main.transform : transform;
			forward = Vector3.ProjectOnPlane(basis.forward, Vector3.up).normalized;
			right   = Vector3.ProjectOnPlane(basis.right,   Vector3.up).normalized;
		}

		Vector3 wishDir = forward * _smoothedMove.y + right * _smoothedMove.x;
		if (wishDir.sqrMagnitude > 1e-6f) wishDir.Normalize();
		float moveMag = Mathf.Clamp01(_smoothedMove.magnitude);

		_sendAccumulator += Time.deltaTime;
		if (_sendAccumulator >= 1f / Mathf.Max(1f, sendRateHz))
		{
			_sendAccumulator = 0f;
			bool sendJump = _pendingJump; // отправляем и сбрасываем латч
			_pendingJump = false;
			CmdSendInput(wishDir, sprint, sendJump, moveMag);
		}
	}

	[Command]
	private void CmdSendInput(Vector3 wishDir, bool sprint, bool jump, float moveMag)
	{
		var motor = GetComponent<ServerCharacterMotor>();
		if (motor != null)
		{
			motor.ApplyInput(wishDir, sprint, jump, moveMag);
		}
	}
}


