using SquareDinoT3.Network;

namespace SquareDinoT3.Services
{
	/// <summary>
	/// Mirror-specific session service.
	/// </summary>
	public sealed class MirrorSessionService : ISessionService
	{
		private readonly DiNetworkManager _manager;

		public MirrorSessionService(DiNetworkManager manager)
		{
			_manager = manager;
		}

		public void StartHost()   => _manager.StartHost();
		public void StartClient() => _manager.StartClient();
		public void StartServer() => _manager.StartServer();
		public void StopHost()    => _manager.StopHost();
		public void StopClient()  => _manager.StopClient();
		public void StopServer()  => _manager.StopServer();
	}
}

