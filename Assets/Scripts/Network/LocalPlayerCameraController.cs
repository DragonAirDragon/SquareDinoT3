using Mirror;
using Cinemachine;
using UnityEngine;
using StarterAssets;
using VContainer;

using SquareDinoT3.Configs;

namespace SquareDinoT3.Network
{
	/// <summary>
	/// Controls local camera (Cinemachine) and cursor for local player.
	/// </summary>
	public sealed class LocalPlayerCameraController : NetworkBehaviour
	{
		[SerializeField] private GameObject _cinemachineTarget;
		[SerializeField] private string _playerFollowCameraTag = "PlayerFollowCamera";
		[SerializeField] private bool _lockCursor = true;
		// Configs
		private InputConfig _inputConfig;
		// Input
		private StarterAssetsInputs _input;
		// Cached
		private float _topClamp = 70f;
		private float _bottomClamp = -30f;
		private float _cameraAngleOverride = 0f;
		private float _yaw;
		private float _pitch;
		
		[Inject]
		public void Construct(InputConfig inputConfig)
		{
			_inputConfig = inputConfig;
		}
		public override void OnStartLocalPlayer()
		{
			_input = GetComponent<StarterAssetsInputs>();

			// применить конфиг
			if (_inputConfig != null)
			{
				_lockCursor = _inputConfig.LockCursor;
				_topClamp = _inputConfig.TopClamp;
				_bottomClamp = _inputConfig.BottomClamp;
				_cameraAngleOverride = _inputConfig.CameraAngleOverride;
			}

			AttachVcam();
			if (_lockCursor)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			_yaw = _cinemachineTarget != null ? _cinemachineTarget.transform.eulerAngles.y : transform.eulerAngles.y;
		}

		public override void OnStopLocalPlayer()
		{
			if (_lockCursor)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			var brain = Camera.main ? Camera.main.GetComponent<CinemachineBrain>() : null;
			if (brain != null) brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;
		}

		private void AttachVcam()
		{
			var vcamGo = GameObject.FindGameObjectWithTag(_playerFollowCameraTag);
			var vcam = vcamGo ? vcamGo.GetComponent<CinemachineVirtualCamera>() : null;
			if (vcam != null)
			{
				vcam.Follow = (_cinemachineTarget != null) ? _cinemachineTarget.transform : transform;
				var brain = Camera.main ? Camera.main.GetComponent<CinemachineBrain>() : null;
				if (brain != null) brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
			}
		}

		private void LateUpdate()
		{
			if (!isLocalPlayer || _cinemachineTarget == null) return;
			Vector2 look = _input != null ? _input.look : Vector2.zero;
			if (look.sqrMagnitude > 0f)
			{
				bool mouse = true; // StarterAssets normalizes by scheme
				float mul = mouse ? 1f : Time.deltaTime;
				_yaw += look.x * mul;
				_pitch += look.y * mul;
			}
			_pitch = Mathf.Clamp(_pitch, _bottomClamp, _topClamp);
			_cinemachineTarget.transform.rotation = Quaternion.Euler(_pitch + _cameraAngleOverride, _yaw, 0f);
		}
	}
}

