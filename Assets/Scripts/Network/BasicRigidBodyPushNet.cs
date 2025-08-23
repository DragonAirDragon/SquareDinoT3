using Mirror;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BasicRigidBodyPushNet : NetworkBehaviour
{
    public LayerMask pushLayers = ~0;
    public bool canPush = true;
    [Range(0.5f, 5f)] public float strength = 1.1f;
    public bool massAgnostic = false; // true → VelocityChange (не зависит от массы)

    // вызывать логику только на сервере
    [ServerCallback]
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!canPush) return;

        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;

        // слой должен быть разрешён
        if ((pushLayers.value & (1 << body.gameObject.layer)) == 0) return;

        // не толкать то, что ниже нас
        if (hit.moveDirection.y < -0.3f) return;

        // горизонтальное направление толчка из движения контроллера
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
        if (pushDir.sqrMagnitude < 1e-6f) return;

        // лёгкая нормализация + сила
        pushDir.Normalize();
        var mode = massAgnostic ? ForceMode.VelocityChange : ForceMode.Impulse;

        // импульс пропорционален нашей «скорости» соприкосновения
        // (hit.moveLength / fixedDeltaTime приближённо даёт скорость с этого кадра)
        float approxSpeed = hit.moveLength / Time.fixedDeltaTime;
        float impulse = strength * Mathf.Clamp(approxSpeed, 0.5f, 10f);

        body.AddForce(pushDir * impulse, mode);
    }
}
