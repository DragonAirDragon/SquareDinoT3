using Mirror;
using UnityEngine;

namespace SquareDinoT3.Network
{
    /// <summary>
    /// Pushes rigidbodies on collision.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class ServerRigidbodyPusher : NetworkBehaviour
    {
        [SerializeField] private LayerMask _pushLayers = ~0;
        [SerializeField] private bool _canPush = true;
        [SerializeField, Range(0.5f, 5f)] private float _strength = 1.1f;
        [SerializeField] private bool _massAgnostic = false;

        [ServerCallback]
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!_canPush) return;

            Rigidbody body = hit.collider.attachedRigidbody;
            if (body == null || body.isKinematic) return;

            // layer should be allowed
            if ((_pushLayers.value & (1 << body.gameObject.layer)) == 0) return;

            // don't push below us
            if (hit.moveDirection.y < -0.3f) return;

            // horizontal push direction from controller movement
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
            if (pushDir.sqrMagnitude < 1e-6f) return;

            // light normalization + strength
            pushDir.Normalize();
            var mode = _massAgnostic ? ForceMode.VelocityChange : ForceMode.Impulse;

            // impulse is proportional to our "speed" of contact
            // (hit.moveLength / fixedDeltaTime approximately gives speed of this frame)
            float approxSpeed = hit.moveLength / Time.fixedDeltaTime;
            float impulse = _strength * Mathf.Clamp(approxSpeed, 0.5f, 10f);

            body.AddForce(pushDir * impulse, mode);
        }
    }
}