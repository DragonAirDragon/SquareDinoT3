using UnityEngine;
using TMPro;
using Mirror;

public class MainMenu : MonoBehaviour
{
    [SerializeField] TMP_InputField nickInput;
    [SerializeField] NetworkManager manager;

    void Start()
    {
        var def = PlayerPrefs.GetString("nickname", "");
        if (string.IsNullOrWhiteSpace(def))
            def = $"Player_{Random.Range(1000, 9999)}";
        nickInput.text = def;
    }

    public void OnHost()
    {
        SaveNick();
        manager.StartHost();
        gameObject.SetActive(false);
    }

    public void OnClient()
    {
        SaveNick();
        manager.StartClient();
        gameObject.SetActive(false);
    }

    public void OnServer()
    {
        SaveNick();
        manager.StartServer();
        gameObject.SetActive(false);
    }

    void SaveNick()
    {
        var s = nickInput.text.Trim();
        if (string.IsNullOrWhiteSpace(s))
            s = $"Player_{Random.Range(1000, 9999)}";
        if (s.Length > 24) s = s.Substring(0, 24);

        PlayerPrefs.SetString("nickname", s);
        PlayerPrefs.Save();
    }
}
