using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Глобальный DI-скоуп приложения. Регистрирует конфиги и базовые сервисы.
/// </summary>
public sealed class AppLifetimeScope : LifetimeScope
{

	[Header("Player Configs")]
	[SerializeField] private MovementConfig movementConfig;
	[SerializeField] private NetworkConfig networkConfig;
	[SerializeField] private InputConfig inputConfig;
	[SerializeField] private DiNetworkManager networkManager;

    [Header("Prefabs")]
    [SerializeField] private MainMenuView mainMenuView;
	
 
	protected override void Configure(IContainerBuilder builder)
	{
		// Конфиги как инстансы
		if (movementConfig != null) builder.RegisterInstance(movementConfig);
		if (networkConfig != null) builder.RegisterInstance(networkConfig);
		if (inputConfig != null) builder.RegisterInstance(inputConfig);
		
		// NetworkManager из сцены (с его уже настроенными полями и spawnPrefabs)
		if (networkManager != null)
		{
			builder.RegisterComponent(networkManager);
			builder.Register<ISessionService, MirrorSessionService>(Lifetime.Singleton);
		}
		
		builder.Register<IPlayerNameService, PlayerPrefsNameService>(Lifetime.Singleton);
		// Presenters
		builder.Register<MainMenuPresenter>(Lifetime.Singleton);
		// Views
		if (mainMenuView != null)
			builder.RegisterComponentInNewPrefab(mainMenuView, Lifetime.Singleton).DontDestroyOnLoad();
	}
 

    #region NetworkResolver
	public static IObjectResolver Resolver { get; private set; }
	protected override void Awake()
    {
        base.Awake();
        Resolver = Container;
        DontDestroyOnLoad(gameObject);
    }
	#endregion
}




