using Mirror;
using StarterAssets;
using UnityEngine;

public class PlayerSpawnCube : NetworkBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    private StarterAssetsInputs _input;
    [SerializeField] private float spawnDistance = 2f;
    [SerializeField] private float ttlSeconds = 15f; // время жизни (опц.)

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
    }

    private System.Collections.IEnumerator DestroyLater(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        if (go) NetworkServer.Destroy(go);
    }
}
