using Mirror;

public sealed class MirrorSessionService : ISessionService
{
	readonly DiNetworkManager _manager;

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


