using UnityEngine;
using TMPro;
using VContainer;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private TMP_InputField nickInput;
    private MainMenuPresenter _presenter;

    [Inject]
    public void Construct(MainMenuPresenter presenter)
    {
        _presenter = presenter;
    }

    void Start()
    {
        var def = _presenter.GetDefaultNickname();
        nickInput.text = def;
    }

    public void OnHost()
    {
        _presenter.Host(nickInput.text);
        gameObject.SetActive(false);
    }

    public void OnClient()
    {
        _presenter.Client(nickInput.text);
        gameObject.SetActive(false);
    }

    public void OnServer()
    {
        _presenter.Server(nickInput.text);
        gameObject.SetActive(false);
    }
}
