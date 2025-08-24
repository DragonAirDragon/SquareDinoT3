using System.Collections;
using Mirror;
using StarterAssets;
using UnityEngine;

namespace SquareDinoT3.Network
{
    /// <summary>
    /// Spawns a cube on server
    /// </summary>
    public class NetworkCubeSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject _cubePrefab;
        [SerializeField] private float _spawnDistance = 2f;
        [SerializeField] private float _ttlSeconds = 15f; // время жизни (опц.)
        private StarterAssetsInputs _input;
        private void Awake()
        {
            _input = GetComponent<StarterAssetsInputs>();
        }
        private void Update()
        {
            if (!isLocalPlayer) return;
            if (_input.spawnCube){
                CmdSpawnCube();
                _input.spawnCube = false;
                
            }
        }

        [Command] 
        private void CmdSpawnCube()
        {
            Vector3 pos = transform.position + transform.forward * _spawnDistance + Vector3.up * 0.5f;
            Quaternion rot = Quaternion.LookRotation(transform.forward, Vector3.up);
            var go = Instantiate(_cubePrefab, pos, rot);
            NetworkServer.Spawn(go);
            if (_ttlSeconds > 0f) StartCoroutine(DestroyLater(go, _ttlSeconds));
        }

        private IEnumerator DestroyLater(GameObject go, float t)
        {
            yield return new WaitForSeconds(t);
            if (go) NetworkServer.Destroy(go);
        }
    }
}
