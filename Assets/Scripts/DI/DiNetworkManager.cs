using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SquareDinoT3.Network
{
	/// <summary>
	/// NetworkManager, which spawns player and registered prefabs through VContainer (IObjectResolver.Instantiate).
	/// </summary>
	public sealed class DiNetworkManager : NetworkManager, INetworkClientEvents
	{
		// DI
		private IObjectResolver _resolver;
		
		// Cached
		private readonly HashSet<GameObject> _registeredPrefabs = new();
		private readonly List<GameObject> _diSpawnPrefabs = new();
		

		// Client events
		public event Action ClientConnected;
		public event Action ClientDisconnected;
		

		[Inject]
		public void Construct(IObjectResolver resolver)
		{
			_resolver = resolver;
		}

		public override void Awake()
		{
			base.Awake();
			if (spawnPrefabs != null)
			{
				_diSpawnPrefabs.Clear();
				for (int i = 0; i < spawnPrefabs.Count; i++)
				{
					var prefab = spawnPrefabs[i];
					if (prefab == null) continue;
					// Exclude playerPrefab from DI-spawn to avoid conflicts with Mirror
					if (playerPrefab != null && prefab == playerPrefab) continue;
					if (!_diSpawnPrefabs.Contains(prefab))
						_diSpawnPrefabs.Add(prefab);
				}
				// Cleaning to avoid Mirror registering these prefabs again
				spawnPrefabs.Clear();
			}

		}

		public override void OnStartClient()
		{
			base.OnStartClient();
			RegisterSpawnHandlers();
			if (playerPrefab != null)
			{
				NetworkClient.UnregisterPrefab(playerPrefab);
				NetworkClient.RegisterPrefab(
					playerPrefab,
					msg => _resolver != null
						? _resolver.Instantiate(playerPrefab, msg.position, msg.rotation)
						: Instantiate(playerPrefab, msg.position, msg.rotation),
					go => { if (go) Destroy(go); }
				);
				_registeredPrefabs.Add(playerPrefab);
			}
		}

		public override void OnClientConnect()
		{
			base.OnClientConnect();
			ClientConnected?.Invoke();
		}

		public override void OnClientDisconnect()
		{
			base.OnClientDisconnect();
			ClientDisconnected?.Invoke();
		}
		public override void OnStopClient()
		{
			base.OnStopClient();
			ClientDisconnected?.Invoke();
			// Correctly unregister to avoid leaks/duplicates on reconnect
			foreach (var prefab in _registeredPrefabs)
			{
				if (prefab != null)
					NetworkClient.UnregisterPrefab(prefab);
			}
			_registeredPrefabs.Clear();
		}

		public override void OnStopHost()
		{
			base.OnStopHost();
			ClientDisconnected?.Invoke();
		}

		private void RegisterSpawnHandlers()
		{
			if (_diSpawnPrefabs == null) return;
			foreach (var prefab in _diSpawnPrefabs)
			{
				if (prefab == null || _registeredPrefabs.Contains(prefab)) continue;
				NetworkClient.RegisterPrefab(
					prefab,
					// spawn
					(spawnMsg) =>
					{
						var pos = spawnMsg.position;
						var rot = spawnMsg.rotation;
						if (_resolver == null)
						{
							Debug.LogError("Resolver is null. Ensure AppLifetimeScope is initialized before connecting.");
							return Instantiate(prefab, pos, rot);
						}
						var go = _resolver.Instantiate(prefab, pos, rot);
						return go;
					},
					// unspawn
					(go) =>
					{
						if (go) Destroy(go);
					}
				);
				_registeredPrefabs.Add(prefab);
			}
		}




		public override void OnServerAddPlayer(NetworkConnectionToClient conn)
		{
			Transform start = GetStartPosition();
			Vector3 pos = start ? start.position : Vector3.zero;
			Quaternion rot = start ? start.rotation : Quaternion.identity;
			if (_resolver == null)
			{
				Debug.LogError("Resolver is null. Ensure AppLifetimeScope is initialized before StartHost/StartServer.");
				var fallback = Instantiate(playerPrefab, pos, rot);
				NetworkServer.AddPlayerForConnection(conn, fallback);
				return;
			}
			var player = _resolver.Instantiate(playerPrefab, pos, rot);
			NetworkServer.AddPlayerForConnection(conn, player);
		}
	}
}

