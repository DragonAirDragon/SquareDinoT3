using Mirror;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class HelloBroadcaster : NetworkBehaviour
{
    private StarterAssetsInputs _input;
    private PlayerNickname _nick;
    void Awake()
    {
        _nick = GetComponent<PlayerNickname>();
        _input = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (_input.sendMessage)
        {
            string who = _nick ? _nick.CurrentName : "Unknown";
            _input.sendMessage = false;
            CmdHello(who);
        }
    }

    [Command]
    private void CmdHello(string who)
    {
        RpcHello(who); // рассылаем всем, включая отправителя
    }

    [ClientRpc]
    private void RpcHello(string who)
    {
        Debug.Log($"Hello from {who}");
    }
}
