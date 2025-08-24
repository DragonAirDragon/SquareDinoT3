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
        
        [Header("Target")]
        [SerializeField] private Transform _headAnchor;
        [SerializeField] [Min(0f)] private float _worldYOffset = 0.25f;
        
        [SyncVar(hook = nameof(OnNameChanged))]
        private string playerName;
        

        // Services
        private IPlayerNameService _nameService;
        // Cached
        private Transform _cachedCam;
        private Transform _cachedNameTransform;



        // Properties
        public string PlayerName => playerName;

        [Inject]
        public void Construct(IPlayerNameService nameService)
        {
            _nameService = nameService;
        }

        private void Awake()
        {
            if (_nameText != null)
            {
                _cachedNameTransform = _nameText.transform;
            }
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
            if (_nameText == null && _cachedNameTransform == null) return;
            if (_cachedNameTransform == null && _nameText != null)
                _cachedNameTransform = _nameText.transform;
            // follow head anchor by world position with Y offset (ignore head rotation)
            if (_headAnchor != null && _cachedNameTransform != null)
            {
                Vector3 pos = _headAnchor.position;
                pos.y += _worldYOffset;
                _cachedNameTransform.position = pos;
            }
            // cache camera transform to avoid Camera.main jitter
            if (_cachedCam == null)
            {
                _cachedCam = Camera.main ? Camera.main.transform : null;
            }
            if (_cachedCam != null && _cachedNameTransform != null)
                _cachedNameTransform.rotation = Quaternion.LookRotation(_cachedNameTransform.position - _cachedCam.position);
        }
    }
}