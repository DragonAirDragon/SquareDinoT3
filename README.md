<p align="center">
  <video src="docs/demo.mp4" width="800" controls>
    Your browser does not support the video tag.
  </video>
</p>

<h1 align="center">SquareDinoT3 — Mirror (Unity 2022.3)</h1>
<p align="center">
  <a href="https://unity.com/">Unity 2022.3 LTS</a> · 
  <a href="https://mirror-networking.gitbook.io/docs/">Mirror</a> · 
  <a href="https://vcontainer.hadashikick.jp/">VContainer</a> · 
  Input System · TextMeshPro
</p>
<p align="center">
  <a href="#"><img src="https://img.shields.io/badge/Unity-2022.3_LTS-informational" /></a>
  <a href="#"><img src="https://img.shields.io/badge/Mirror-server_authority-success" /></a>
  <a href="#"><img src="https://img.shields.io/badge/DI-VContainer-blue" /></a>
</p>
## Содержание
- [Итог](#итог)
- [Быстрый старт](#быстрый-старт)
- [Управление](#управление)
- [ТЗ → Реализация](#тз--реализация)
- [Архитектура](#архитектура)
- [Структура проекта](#структура-проекта)
- [Сцены и префабы](#сцены-и-префабы)
- [Конфиги](#конфиги)
- [Зависимости](#зависимости)

## Итог
Мультиплеерная сцена на **Mirror** с **server-authority** симуляцией: клиент шлёт только ввод; сервер считает движение, спавнит объекты и рассылает состояние через **Commands / ClientRpc / SyncVar**. Реализованы: никнеймы, сообщение всем клиентам, спавн сетевого куба, Humanoid-анимации. ✨

## Быстрый старт
1. **Клонируй репозиторий** и открой в **Unity 2022.3 LTS**. 🧩  
2. Открой сцену **`Assets/Scenes/MainScene.unity`**. 🗺️  
3. В ParrelSync запусти один инстанс как **Host**, клон — как **Client** (кнопки в главном меню). 🧪  
4. Введи ник (или сгенерируй) → играй. 🕹️  

## Управление
| Действие 🎮                                          | Клавиша ⌨️   |
|-----------------------------------------------------|--------------|
| Ходьба / Бег                                        | WASD / Shift |
| Прыжок                                              | Space        |
| Отправить сетевое сообщение "Привет от &lt;nick&gt;"| **T**        |
| Заспавнить сетевой куб перед персонажем             | **F**        |

## ТЗ → Реализация
**Все обязательные задачи и дополнительные выполнены.**

| Требование ТЗ                                                | Статус | Где в коде                                                                                                                                                          |
| ------------------------------------------------------------ | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Ник игрока виден всем                                        | ✅      | `Assets/Scripts/Network/PlayerNicknameSync.cs` (SyncVar `playerName`, hook `OnNameChanged`, `[Command]` `CmdSetName`)                                               |
| Управляю только своим персонажем                             | ✅      | `Assets/Scripts/Network/LocalInputSender.cs`, `Assets/Scripts/Network/NetworkMessage.cs`, `Assets/Scripts/Network/NetworkCubeSpawner.cs` — проверки `isLocalPlayer` |
| Сообщение всем клиентам (по T)                               | ✅      | `Assets/Scripts/Network/NetworkMessage.cs` (`CmdHello` → `RpcHello`)                                                                                                |
| Замена капсулы на Humanoid c анимациями Idle/Run/Sprint/Jump | ✅      | `Assets/Scripts/Network/ServerAnimatorDriver.cs` + вызовы из `Assets/Scripts/Network/ServerCharacterMotor.cs`                                                       |
| Сетевой спавн физического куба (по F)                        | ✅      | `Assets/Scripts/Network/NetworkCubeSpawner.cs` (`CmdSpawnCube` + `NetworkServer.Spawn`)                                                                             |

## Архитектура
**Принципы**
- **Server-authority**: клиент передаёт ввод (`CmdSendInput`), **сервер** обновляет `CharacterController`, гравитацию, прыжок; рассылка состояния — через `ClientRpc`/`SyncVar`. 🚦  
- **DI (VContainer)**: конфиги/сервисы регистрируются в `AppLifetimeScope`; сетевой спавн и доступ к зависимостям — через `DiNetworkManager` + `IObjectResolver`. 🧰  

## Структура проекта
```
Assets/
  Scenes/
    MainScene.unity
  Prefabs/
    DI/AppScope.prefab
    GamePlay/PlayerPrefab.prefab
    GamePlay/CubePhysics.prefab
  Scripts/
    ConfigAssets/       # MovementConfig, NetworkConfig, InputConfig
    DI/                 # AppLifetimeScope, DiNetworkManager
    Services/           # ISessionService, IPlayerNameService
    Network/            # LocalInputSender, ServerCharacterMotor, ServerAnimatorDriver
                        # PlayerNicknameSync, NetworkMessage, NetworkCubeSpawner
    Views/              # MainMenuView, MainMenuPresenter
```

## Сцены и префабы
- **`MainScene.unity`** — основная сцена.
- **`PlayerPrefab.prefab`** — сетевой игрок (Starter Assets + сетевые скрипты).
- **`CubePhysics.prefab`** —  `Rigidbody`-куб.
- **`AppScope.prefab`** — корневой `LifetimeScope` (DI).

## Конфиги
- `Scripts/ConfigAssets/MovementConfig.asset` — скорости, прыжок, гравитация
- `Scripts/ConfigAssets/NetworkConfig.asset` — частота отправки ввода
- `Scripts/ConfigAssets/InputConfig.asset` — сглаживание ввода

## Зависимости
- **Mirror** — сети и синхронизация (`Commands`, `ClientRpc`, `SyncVar`).
- **VContainer** — DI-контейнер для MonoBehaviour/префабов.
- **ParrelSync (опционально)** — локальное тестирование нескольких клиентов.

