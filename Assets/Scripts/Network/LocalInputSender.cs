using Mirror;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

using SquareDinoT3.Configs;


namespace SquareDinoT3.Network
{
	/// <summary>
	/// Collects local input, smooths it and sends it to the server.
	/// Works only for local player.
	/// </summary>
	[RequireComponent(typeof(StarterAssetsInputs))]
	[RequireComponent(typeof(PlayerInput))]
	public sealed class LocalInputSender : NetworkBehaviour
	{
		// Configs
		private NetworkConfig _networkConfig;
		private InputConfig _inputConfig;

		// Network tuning
		private float _sendRateHz = 60f;
		private float _inputSmooth = 50f;
		// Input
		private StarterAssetsInputs _input;
		private PlayerInput _playerInput;
		// Cached
		private float _sendAccumulator;
		private Vector2 _smoothedMove;
		private bool _pendingJump;
		private Transform _cachedCameraTr;

		[Inject]
		public void Construct(NetworkConfig networkConfig, InputConfig inputConfig)
		{
			_networkConfig = networkConfig;
			_inputConfig = inputConfig;
		}


		public override void OnStartLocalPlayer()
		{
			_input = GetComponent<StarterAssetsInputs>();
			_playerInput = GetComponent<PlayerInput>();

			// apply configs
			if (_networkConfig != null)
			{
				_sendRateHz = Mathf.Max(1f, _networkConfig.SendRateHz);
			}
			if (_inputConfig != null)
			{
				_inputSmooth = Mathf.Max(0f, _inputConfig.InputSmooth);
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
				1f - Mathf.Exp(-Mathf.Max(0f, _inputSmooth) * Time.deltaTime)
			);

			bool sprint = _input.sprint;
			if (_input.jump)
			{
				_input.jump = false;
				_pendingJump = true; // latch until sent
			}

			Vector3 forward;
			Vector3 right;
			{
				// Cache Main Camera transform to avoid Camera.main jitter
				if (_cachedCameraTr == null)
				{
					_cachedCameraTr = Camera.main ? Camera.main.transform : null;
				}
				Transform basis = _cachedCameraTr != null ? _cachedCameraTr : transform;
				forward = Vector3.ProjectOnPlane(basis.forward, Vector3.up).normalized;
				right   = Vector3.ProjectOnPlane(basis.right,   Vector3.up).normalized;
			}

			Vector3 wishDir = forward * _smoothedMove.y + right * _smoothedMove.x;
			if (wishDir.sqrMagnitude > 1e-6f) wishDir.Normalize();
			float moveMag = Mathf.Clamp01(_smoothedMove.magnitude);

			_sendAccumulator += Time.deltaTime;
			if (_sendAccumulator >= 1f / Mathf.Max(1f, _sendRateHz))
			{
				_sendAccumulator = 0f;
				bool sendJump = _pendingJump; // send and reset latch
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
}

