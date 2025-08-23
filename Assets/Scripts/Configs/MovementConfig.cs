using UnityEngine;

[CreateAssetMenu(fileName = "MovementConfig", menuName = "Configs/Movement Config")]
public class MovementConfig : ScriptableObject
{
	[Header("Movement")]
	public float moveSpeed = 2.0f;
	public float sprintSpeed = 5.335f;
	[Range(0.0f, 0.3f)] public float rotationSmoothTime = 0.12f;
	public float speedChangeRate = 10.0f;
	public float jumpHeight = 1.2f;
	public float gravity = -15.0f;
	public float jumpTimeout = 0.50f;
	public float fallTimeout = 0.15f;

	[Header("Grounded")]
	public float groundedOffset = -0.14f;
	public float groundedRadius = 0.28f;
	public LayerMask groundedLayers;
}


