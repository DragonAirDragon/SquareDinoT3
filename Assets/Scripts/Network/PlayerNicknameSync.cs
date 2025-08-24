using Mirror;
using UnityEngine;
using TMPro;
using VContainer;

using SquareDinoT3.Services;

namespace SquareDinoT3.Network
{
    /// <summary>
    /// Syncs player nickname above head with server and rotate to camera.
    /// </summary>
    public class PlayerNicknameSync : NetworkBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text _nameText;     
        
        [SyncVar(hook = nameof(OnNameChanged))]
        private string playerName;
        

        // Services
        private IPlayerNameService _nameService;
        // Cached
        private Transform _cachedCam;
        // Properties
        public string PlayerName => playerName;

        [Inject]
        public void Construct(IPlayerNameService nameService)
        {
            _nameService = nameService;
        }

        public override void OnStartLocalPlayer()
        {
            var chosen = _nameService?.GetLocalNick() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(chosen))
                chosen = $"Player_{Random.Range(1000, 9999)}";
            CmdSetName(chosen.Trim());
        }

        [Command]
        private void CmdSetName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                newName = $"Player_{Random.Range(1000, 9999)}";
            if (newName.Length > 24) newName = newName.Substring(0, 24);
            playerName = newName;
        }

        private void OnNameChanged(string _, string newValue)
        {
            if (_nameText != null) _nameText.text = newValue;
        }

        private void LateUpdate()
        {
            if (_nameText == null) return;
            // cache camera transform to avoid Camera.main jitter
            if (_cachedCam == null)
            {
                _cachedCam = Camera.main ? Camera.main.transform : null;
            }
            if (_cachedCam != null)
                _nameText.transform.rotation = Quaternion.LookRotation(_nameText.transform.position - _cachedCam.position);
        }
    }
}