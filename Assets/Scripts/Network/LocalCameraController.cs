using Mirror;
using Cinemachine;
using UnityEngine;
using StarterAssets;
using VContainer;

/// <summary>
/// Управляет локальной камерой (Cinemachine) и курсором у локального игрока.
/// </summary>
public sealed class LocalCameraController : NetworkBehaviour
{
	[SerializeField] private GameObject cinemachineTarget;
	[SerializeField] private string playerFollowCameraTag = "PlayerFollowCamera";
	[SerializeField] private bool lockCursor = true;

	// Config
	private InputConfig inputConfig;

	// Cached
	private float topClamp = 70f;
	private float bottomClamp = -30f;
	private float cameraAngleOverride = 0f;
	private float _yaw;
	private float _pitch;
	private StarterAssetsInputs _input;
    [Inject]
	public void Construct(InputConfig inputConfig)
	{
		this.inputConfig = inputConfig;
	}

	public override void OnStartLocalPlayer()
	{
		_input = GetComponent<StarterAssetsInputs>();

		// применить конфиг
		if (inputConfig != null)
		{
			lockCursor = inputConfig.lockCursor;
			topClamp = inputConfig.topClamp;
			bottomClamp = inputConfig.bottomClamp;
			cameraAngleOverride = inputConfig.cameraAngleOverride;
		}

		AttachVcam();
		if (lockCursor)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		_yaw = (cinemachineTarget != null ? cinemachineTarget.transform.eulerAngles.y : transform.eulerAngles.y);
	}

	public override void OnStopLocalPlayer()
	{
		if (lockCursor)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		var brain = Camera.main ? Camera.main.GetComponent<CinemachineBrain>() : null;
		if (brain != null) brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;
	}

	private void AttachVcam()
	{
		var vcamGo = GameObject.FindGameObjectWithTag(playerFollowCameraTag);
		var vcam = vcamGo ? vcamGo.GetComponent<CinemachineVirtualCamera>() : null;
		if (vcam != null)
		{
			vcam.Follow = (cinemachineTarget != null) ? cinemachineTarget.transform : transform;
			var brain = Camera.main ? Camera.main.GetComponent<CinemachineBrain>() : null;
			if (brain != null) brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
		}
	}

	private void LateUpdate()
	{
		if (!isLocalPlayer || cinemachineTarget == null) return;
		Vector2 look = _input != null ? _input.look : Vector2.zero;
		if (look.sqrMagnitude > 0f)
		{
			bool mouse = true; // StarterAssets нормализует по схеме
			float mul = mouse ? 1f : Time.deltaTime;
			_yaw += look.x * mul;
			_pitch += look.y * mul;
		}
		_pitch = Mathf.Clamp(_pitch, bottomClamp, topClamp);
		cinemachineTarget.transform.rotation = Quaternion.Euler(_pitch + cameraAngleOverride, _yaw, 0f);
	}
}


