
namespace SquareDinoT3.Services
{
	/// <summary>
	/// Service for managing session.
	/// </summary>
	public interface ISessionService
	{
		void StartHost();
		void StartClient();
		void StartServer();
		void StopHost();
		void StopClient();
		void StopServer();
	}

}
