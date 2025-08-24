using UnityEngine;
using VContainer;
using VContainer.Unity;

using SquareDinoT3.Configs;
using SquareDinoT3.Network;
using SquareDinoT3.Services;
using SquareDinoT3.Views;

namespace SquareDinoT3.DI
{
	/// <summary>
	/// Global DI scope for the application. Registers configs and base services.
	/// </summary>
	public sealed class AppLifetimeScope : LifetimeScope
	{

		[Header("Player Configs")]
		[SerializeField] private MovementConfig movementConfig;
		[SerializeField] private NetworkConfig networkConfig;
		[SerializeField] private InputConfig inputConfig;

		[Header("Prefabs")]
		[SerializeField] private MainMenuView mainMenuView;
		
	
		protected override void Configure(IContainerBuilder builder)
		{
			// Configs as instances
			if (movementConfig != null) builder.RegisterInstance(movementConfig);
			if (networkConfig != null) builder.RegisterInstance(networkConfig);
			if (inputConfig != null) builder.RegisterInstance(inputConfig);
			
			// NetworkManager from the scene (with its already configured fields and spawnPrefabs)
			builder.RegisterComponentInHierarchy<DiNetworkManager>();
			builder.Register<INetworkClientEvents>(r => r.Resolve<DiNetworkManager>(), Lifetime.Singleton);
			builder.Register<ISessionService, MirrorSessionService>(Lifetime.Singleton);
			
			// Player name service
			builder.Register<IPlayerNameService, PlayerPrefsNameService>(Lifetime.Singleton);
			
			// Main menu presenter
			builder.Register<MainMenuPresenter>(Lifetime.Singleton);

			// Main menu view
			if (mainMenuView != null)
				builder.RegisterComponentInNewPrefab(mainMenuView, Lifetime.Singleton).DontDestroyOnLoad();

			// Force spawn and resolve MainMenuView
			builder.RegisterBuildCallback(resolver => resolver.Resolve<MainMenuView>());
		}
	


	}
}




