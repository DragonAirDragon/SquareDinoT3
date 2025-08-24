using VContainer;
using SquareDinoT3.Services;

namespace SquareDinoT3.Views
{
	/// <summary>
	/// Presenter for main menu.
	/// </summary>
	public sealed class MainMenuPresenter
	{
		private readonly ISessionService _session;
		private readonly IPlayerNameService _names;

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
}

