using Mirror;
using UnityEngine;
using TMPro;

public class PlayerNickname : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;     // присвой в инспекторе (WorldSpace TMP над головой)
    [SerializeField] private Transform billboardTo; // можно оставить пустым: возьмём главную камеру

    [SyncVar(hook = nameof(OnNameChanged))]
    private string playerName;

    public string CurrentName => playerName;

    public override void OnStartLocalPlayer()
    {
        // 1) Ник из локального источника (например, из меню) или автогенерация
        var chosen = PlayerPrefs.GetString("nickname", string.Empty);
        if (string.IsNullOrWhiteSpace(chosen))
        {
            chosen = $"Player_{Random.Range(1000, 9999)}"; // ТЗ допускает автоник, если не введён
        }
        CmdSetName(chosen.Trim());
    }

    [Command]
    private void CmdSetName(string newName)
    {
        // простая валидация
        if (string.IsNullOrWhiteSpace(newName)) newName = $"Player_{Random.Range(1000, 9999)}";
        newName = newName.Length > 24 ? newName.Substring(0, 24) : newName;
        playerName = newName; // триггерит hook на всех
    }

    void OnNameChanged(string _, string newValue)
    {
        if (nameText != null) nameText.text = newValue;
    }

    void LateUpdate()
    {
        // биллбординг: разворачиваем табличку к локальной камере
        if (nameText == null) return;
        var cam = (billboardTo != null) ? billboardTo : (Camera.main ? Camera.main.transform : null);
        if (cam != null) nameText.transform.rotation = Quaternion.LookRotation(nameText.transform.position - cam.position);
    }
}
