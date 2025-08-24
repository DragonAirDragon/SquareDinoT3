using UnityEngine;
using TMPro;
using VContainer;

using SquareDinoT3.Network;

namespace SquareDinoT3.Views
{
    /// <summary>
    /// View for main menu.
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nickInput;
        private MainMenuPresenter _presenter;
        private INetworkClientEvents _networkEvents;

        [Inject]
        public void Construct(MainMenuPresenter presenter, INetworkClientEvents networkEvents)
        {
            _presenter = presenter;
            _networkEvents = networkEvents;
            _networkEvents.ClientConnected += OnClientConnected;
            _networkEvents.ClientDisconnected += OnClientDisconnected;
        }

        private void Start()
        {
            var def = _presenter.GetDefaultNickname();
            nickInput.text = def;
        }

        private void OnDestroy()
        {
            if (_networkEvents != null)
            {
                _networkEvents.ClientConnected -= OnClientConnected;
                _networkEvents.ClientDisconnected -= OnClientDisconnected;
            }
        }

        public void OnHost()
        {
            _presenter.Host(nickInput.text);
        }

        public void OnClient()
        {
            _presenter.Client(nickInput.text);
        }

        public void OnServer()
        {
            _presenter.Server(nickInput.text);
        }

        private void OnClientConnected()
        {
            gameObject.SetActive(false);
        }

        private void OnClientDisconnected()
        {
            gameObject.SetActive(true);
        }
    }
}