using Mirror;
using StarterAssets;
using UnityEngine;

public class PlayerSpawnCube : NetworkBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private float spawnDistance = 2f;
    [SerializeField] private float ttlSeconds = 15f; // время жизни (опц.)
    private StarterAssetsInputs _input;
    void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
    }
    void Update()
    {
        if (!isLocalPlayer) return;
        if (_input.spawnCube){
            CmdSpawnCube();
            _input.spawnCube = false;
            
        }
    }

    [Command] // клиент -> сервер
    private void CmdSpawnCube()
    {
        Debug.Log("CmdSpawnCube");
        Vector3 pos = transform.position + transform.forward * spawnDistance + Vector3.up * 0.5f;
        Quaternion rot = Quaternion.LookRotation(transform.forward, Vector3.up);

        var go = Instantiate(cubePrefab, pos, rot);    // cubePrefab = CubePhysics
        NetworkServer.Spawn(go);                       // у всех появится одновременно;
        if (ttlSeconds > 0f) StartCoroutine(DestroyLater(go, ttlSeconds));
    }

    private System.Collections.IEnumerator DestroyLater(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        if (go) NetworkServer.Destroy(go);
    }
}
