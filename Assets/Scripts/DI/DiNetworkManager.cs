using System.Collections.Generic;
using Mirror;
using UnityEngine;
using VContainer.Unity;

/// <summary>
/// NetworkManager, который спавнит игрока и зарегистрированные префабы через VContainer (IObjectResolver.Instantiate).
/// </summary>
public sealed class DiNetworkManager : NetworkManager
{
	readonly Dictionary<GameObject, bool> _registered = new Dictionary<GameObject, bool>();

	public override void Awake()
	{
		base.Awake();
		RegisterSpawnHandlers();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		// Дублирующий вызов безопасен из-за _registered
		RegisterSpawnHandlers();
	}

	void RegisterSpawnHandlers()
	{
		if (spawnPrefabs == null) return;
		foreach (var prefab in spawnPrefabs)
		{
			if (prefab == null || _registered.ContainsKey(prefab)) continue;
			NetworkClient.RegisterPrefab(
				prefab,
				// spawn
				(spawnMsg) =>
				{
					var pos = spawnMsg.position;
					var rot = spawnMsg.rotation;
					var resolver = AppLifetimeScope.Resolver;
					if (resolver == null)
					{
						Debug.LogError("Resolver is null. Ensure AppLifetimeScope is initialized before connecting.");
						return Instantiate(prefab, pos, rot);
					}
					var go = resolver.Instantiate(prefab, pos, rot);
					return go;
				},
				// unspawn
				(go) =>
				{
					if (go) Destroy(go);
				}
			);
			_registered[prefab] = true;
		}
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		// Mirror не требует обязательного UnregisterPrefab, оставим как есть до рестарта
	}

	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		Transform start = GetStartPosition();
		Vector3 pos = start ? start.position : Vector3.zero;
		Quaternion rot = start ? start.rotation : Quaternion.identity;
		var resolver = AppLifetimeScope.Resolver;
		if (resolver == null)
		{
			Debug.LogError("Resolver is null. Ensure AppLifetimeScope is initialized before StartHost/StartServer.");
			var fallback = Instantiate(playerPrefab, pos, rot);
			NetworkServer.AddPlayerForConnection(conn, fallback);
			return;
		}
		var player = resolver.Instantiate(playerPrefab, pos, rot);
		NetworkServer.AddPlayerForConnection(conn, player);
	}
}


