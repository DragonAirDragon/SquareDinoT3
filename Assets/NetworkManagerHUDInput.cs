using UnityEngine;
using UnityEngine.InputSystem;

namespace Mirror
{
    /// <summary>
    /// NetworkManager HUD, управляемый через New Input System.
    /// H — Host (Server + Client), J — Join (Client).
    /// Подсказки берутся из привязок Input System, если они заданы через InputActionReference.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/Network Manager HUD (Input System)")]
    [RequireComponent(typeof(NetworkManager))]
    public class NetworkManagerHUDInput : MonoBehaviour
    {
        NetworkManager manager;

        [Header("Layout")]
        public int offsetX;
        public int offsetY;

        [Header("Input")] 
        public InputActionReference hostAction;
        public InputActionReference joinAction;

        InputAction host;
        InputAction join;
        bool createdHostAction;
        bool createdJoinAction;

        void Awake()
        {
            manager = GetComponent<NetworkManager>();
        }

        void OnEnable()
        {
            // Host action
            if (hostAction != null && hostAction.action != null)
            {
                host = hostAction.action;
            }
            else
            {
                host = new InputAction(name: "Host", type: InputActionType.Button, binding: "<Keyboard>/h");
                createdHostAction = true;
            }

            // Join action
            if (joinAction != null && joinAction.action != null)
            {
                join = joinAction.action;
            }
            else
            {
                join = new InputAction(name: "Join", type: InputActionType.Button, binding: "<Keyboard>/j");
                createdJoinAction = true;
            }

            host.Enable();
            join.Enable();
        }

        void OnDisable()
        {
            if (host != null)
            {
                host.Disable();
                if (createdHostAction)
                {
                    host.Dispose();
                    createdHostAction = false;
                }
            }
            if (join != null)
            {
                join.Disable();
                if (createdJoinAction)
                {
                    join.Dispose();
                    createdJoinAction = false;
                }
            }
        }

        void Update()
        {
            // Запускаем по клавишам, только пока не подключены и не активен сервер
            if (!NetworkClient.isConnected && !NetworkServer.active)
            {
                if (!NetworkClient.active)
                {
                    if (host != null && host.WasPressedThisFrame())
                    {
#if UNITY_WEBGL
                        // WebGL: нельзя быть сервером, делаем как в оригинальном HUD — "Single Player"
                        NetworkServer.listen = false;
                        manager.StartHost();
#else
                        manager.StartHost();
#endif
                    }

                    if (join != null && join.WasPressedThisFrame())
                    {
                        manager.StartClient();
                    }
                }
            }
        }

        void OnGUI()
        {
            int width = 300;
            GUILayout.BeginArea(new Rect(10 + offsetX, 40 + offsetY, width, 9999));

            if (!NetworkClient.isConnected && !NetworkServer.active)
                StartSection();
            else
                StatusLabels();

            if (NetworkClient.isConnected && !NetworkClient.ready)
            {
                if (GUILayout.Button("Client Ready"))
                {
                    NetworkClient.Ready();
                    if (NetworkClient.localPlayer == null)
                        NetworkClient.AddPlayer();
                }
            }

            StopButtons();
            GUILayout.EndArea();
        }

        void StartSection()
        {
            // Инструкции (клавиши берем из привязок)
            string hostKey = host != null ? host.GetBindingDisplayString() : "H";
            string joinKey = join != null ? join.GetBindingDisplayString() : "J";

#if UNITY_WEBGL
            GUILayout.Label($"Нажмите [{hostKey}] — Single Player (WebGL)");
#else
            GUILayout.Label($"Нажмите [{hostKey}] — Host (Server + Client)");
#endif
            GUILayout.Label($"Нажмите [{joinKey}] — Join (Client)");
            GUILayout.Space(10);

            // Поля адреса/порта для Join
            GUILayout.BeginHorizontal();
            GUILayout.Label("Address:", GUILayout.Width(60));
            manager.networkAddress = GUILayout.TextField(manager.networkAddress);

            if (Transport.active is PortTransport portTransport)
            {
                if (ushort.TryParse(GUILayout.TextField(portTransport.Port.ToString()), out ushort port))
                    portTransport.Port = port;
            }

            GUILayout.EndHorizontal();

            // Если идет попытка подключения — показать состояние и дать отменить кнопкой
            if (NetworkClient.active)
            {
                GUILayout.Label($"Connecting to {manager.networkAddress}..");
                if (GUILayout.Button("Cancel Connection Attempt"))
                    manager.StopClient();
            }
        }

        void StatusLabels()
        {
            if (NetworkServer.active && NetworkClient.active)
            {
                GUILayout.Label($"<b>Host</b>: running via {Transport.active}");
            }
            else if (NetworkServer.active)
            {
                GUILayout.Label($"<b>Server</b>: running via {Transport.active}");
            }
            else if (NetworkClient.isConnected)
            {
                GUILayout.Label($"<b>Client</b>: connected to {manager.networkAddress} via {Transport.active}");
            }
        }

        void StopButtons()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                GUILayout.BeginHorizontal();
#if UNITY_WEBGL
                if (GUILayout.Button("Stop Single Player"))
                    manager.StopHost();
#else
                if (GUILayout.Button("Stop Host"))
                    manager.StopHost();
                if (GUILayout.Button("Stop Client"))
                    manager.StopClient();
#endif
                GUILayout.EndHorizontal();
            }
            else if (NetworkClient.isConnected)
            {
                if (GUILayout.Button("Stop Client"))
                    manager.StopClient();
            }
            else if (NetworkServer.active)
            {
                if (GUILayout.Button("Stop Server"))
                    manager.StopServer();
            }
        }
    }
}


