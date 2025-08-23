using VContainer;

public sealed class MainMenuPresenter
{
	readonly ISessionService _session;
	readonly IPlayerNameService _names;

	[Inject]
	public MainMenuPresenter(ISessionService session, IPlayerNameService names)
	{
		_session = session;
		_names = names;
	}

	public string GetDefaultNickname() => _names.GetLocalNick();

	public void Host(string nickname)
	{
		_names.SaveLocalNick(nickname);
		_session.StartHost();
	}

	public void Client(string nickname)
	{
		_names.SaveLocalNick(nickname);
		_session.StartClient();
	}

	public void Server(string nickname)
	{
		_names.SaveLocalNick(nickname);
		_session.StartServer();
	}
}


