using Mirror;
using StarterAssets;
using UnityEngine;


namespace SquareDinoT3.Network
{
    /// <summary>
    /// Sends a message to the server (console)
    /// </summary>
    public class NetworkMessage : NetworkBehaviour
    {
        private StarterAssetsInputs _input;
        private PlayerNicknameSync _nick;
        private void Awake()
        {
            _nick = GetComponent<PlayerNicknameSync>();
            _input = GetComponent<StarterAssetsInputs>();
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            if (_input.sendMessage)
            {
                string who = _nick ? _nick.PlayerName : "Unknown";
                _input.sendMessage = false;
                CmdHello(who);
            }
        }

        [Command]
        private void CmdHello(string who)
        {
            RpcHello(who);
        }

        [ClientRpc]
        private void RpcHello(string who)
        {
            Debug.Log($"Привет от {who}");
        }
    }
}